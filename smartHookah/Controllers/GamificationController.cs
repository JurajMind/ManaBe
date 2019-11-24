using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    public class GamificationController : Controller
    {
        private static GamificationEngine _engine;
        public static GamificationEngine Engine
        {
            get
            {
                if (_engine == null)
                {

                    _engine = new GamificationEngine();

                }
                return _engine;
            }
        }

        // GET: Gamification
        private SmartHookahContext _db { get; set; }

        public GamificationController(SmartHookahContext db)
        {
            _db = db;
            if (Engine == null)
            {
                _engine = new GamificationEngine();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public new ActionResult Profile(int? id)
        {
            if (id == null)
            {
                var user = UserHelper.GetCurentPerson(_db);
                if (user != null)
                {
                    id = user.Id;
                }
                else
                {
                    return RedirectToAction("Index");

                }
            }

            var person = _db.Persons.Where(a => a.Id == id).Include(a => a.GameProfile).FirstOrDefault();

            if (person == null)
                RedirectToAction("Index");

            return View(person);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateGameProfile()
        {
            var persons = _db.Persons.Where(a => a.GameProfile == null);

            foreach (var person in persons)
            {
                if (person.GameProfile != null)
                    continue;
                person.GameProfile = GameProfile.Default();
                person.GameProfile.Person = person;
                _db.Persons.AddOrUpdate(person);
            }

            _db.SaveChanges();
            return null;
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateEventRecepie(int id, int? recepieId = null)
        {
            var e = await _db.Events.FindAsync(id);
            if (e == null)
                return RedirectToAction("Index");

            var recepie = new EventRecepie();
            recepie.EventId = e.Id;
            if (recepieId != null)
                recepie = await _db.EventRecepies.FindAsync(recepieId);

            if (recepie == null)
                return RedirectToAction("Index");

            return View(recepie);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEventRecepie(EventRecepie model)
        {
            if (ModelState.IsValid)
            {
                _db.EventRecepies.AddOrUpdate(model);
                await _db.SaveChangesAsync();
            }
            ReloadEngine();
            return RedirectToAction("EventDetail", new { id = model.Id });
        }

        public async Task<ActionResult> EventDetail(int? id)
        {
            var e = _db.Events.Where(a => a.Id == id)
                .Include(a => a.Recepies)
                .Include(a => a.Rewards).FirstOrDefault();

            if (e == null)
                return RedirectToAction("Index");

            return View(e);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CreateEvent(int? id)
        {
            var ev = new Event();

            if (id != null)
                ev = _db.Events.Find(id);

            if (ev == null)
                return RedirectToAction("Index");

            return View(ev);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEvent(Event e)
        {
            if (ModelState.IsValid)
            {
                if (e.Id == 0)
                {
                    _db.Events.Add(e);
                }
                else
                {
                    _db.Events.AddOrUpdate(e);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("EventDetail", new { id = e.Id });
            }
            ReloadEngine();
            return View(e);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> CreateReward(int? id, int? rewardId)
        {
            var e = await _db.Events.FindAsync(id);

            var reward = new Reward();
            if (e != null)
                reward.EventId = e.Id;

            if (rewardId != null)
                reward = await _db.Rewards.FindAsync(rewardId);

            if (reward == null)
                return RedirectToAction("Index");

            return View(reward);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateReward(Reward reward, string type, int rewardId)
        {
            if (ModelState.IsValid)
            {
                var transform = Reward.Transform(reward, type);
                transform.Id = rewardId;
                _db.Rewards.AddOrUpdate(transform);
                await _db.SaveChangesAsync();
                return RedirectToAction("EventDetail", new { id = reward.EventId });
            }
            return RedirectToAction("Index");
        }

        private void ReloadEngine()
        {
            _engine = new GamificationEngine();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ValidateSession(int id)
        {
            Engine.ManualValidateSession(id);
            return null;
        }

        public ActionResult RewardDetails(int id)
        {
            var reward = _db.Rewards.Find(id);

            if (reward == null)
                return RedirectToAction("Index");

            return View(reward);
        }

        [Authorize(Roles = "Admin")]
        public void ManualValidatePerson(int personId)
        {
            var person = _db.Persons.Find(personId);

            if (person == null)
                return;

            foreach (var session in person.SmokeSessions)
            {
                Engine.ManualValidateSession(session.Id);
            }
        }


        [Authorize(Roles = "Admin")]
        public void ValidateAll()
        {
            var sessions = _db.SmokeSessions.Select(a => a.Id).ToList();

            foreach (var p in sessions)
            {
                ValidateSession(p);
            }
        }
    }


    public class GamificationEngine
    {
        private SmartHookahContext _db;
        private List<EventTrigger> PersonEventsLong { get; set; }

        private List<EventTrigger> PersonEventsTimeFrame { get; set; }

        private List<EventTrigger> PersonEventShort { get; set; }

        private List<EventTrigger> PlaceEvents { get; set; }

        public GamificationEngine()
        {
            _db = new SmartHookahContext();
            var activeEvents = _db.Events.Where(a =>
                (a.From == null || a.From > DateTime.Now) &&
                (a.To == null || a.To < DateTime.Now)).ToList();

            PersonEventShort = activeEvents.Where(a => a.Type == EventType.Person && a.TimeFrameTicks == null &&
            (a.TriggerTime == new TimeSpan() && a.TriggerCount < 2)
            ).Select(b => new EventTrigger(b))
                .ToList();

            PersonEventsLong = activeEvents.Where(a => a.Type == EventType.Person && a.TimeFrameTicks == null &&
             (a.TriggerTime > new TimeSpan() || a.TriggerCount > 2)
                ).Select(b => new EventTrigger(b))
                .ToList();


            PersonEventsTimeFrame = activeEvents.Where(a => a.Type == EventType.Person && a.TimeFrameTicks != null &&
                                    (a.TriggerTime != null || a.TriggerCount > 2))
                .Select(b => new EventTrigger(b)).ToList();

            PlaceEvents = activeEvents.Where(a => a.Type == EventType.Place).Select(b => new EventTrigger(b))
                .ToList();
        }







        public void ManualValidateSession(int smokeSessionId)
        {
            var sessions = _db.SmokeSessions.Where(a => a.Id == smokeSessionId).Include(a => a.MetaData)
                .Include(b => b.Statistics).FirstOrDefault();

            if (sessions == null)
                return;

            ManualValidateSession(sessions);
        }
        public void ManualValidateSession(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return;

            foreach (var eventTrigger in PersonEventShort)
            {
                var processTriger = ProcessTrigger(smokeSession, eventTrigger);

                if (!processTriger)
                    continue;

                var e = eventTrigger.Execute(smokeSession);

                AddEventToPersons(smokeSession, e);
            }

            foreach (var eventTrigger in PersonEventsLong)
            {
                var processTriger = ProcessTrigger(smokeSession, eventTrigger);
                if (!processTriger)
                    continue;
                foreach (var person in smokeSession.Persons)
                {
                    if (processPerson(person, eventTrigger))
                        continue;

                    var progress =
                        _db.EventProgresses.FirstOrDefault(a => a.GameProfileId == person.Id &&
                                                       a.EventId == eventTrigger.Event.Id);
                    if (progress == null)
                        progress = new EventProgress()
                        {
                            GameProfileId = person.Id,
                            EventId = eventTrigger.Event.Id,
                        };

                    var e = eventTrigger.Execute(smokeSession, ref progress);

                    if (e == null)
                    {
                        _db.EventProgresses.AddOrUpdate(progress);
                    }
                    else
                    {
                        AddEvent(person, e);
                        _db.EventProgresses.Remove(progress);
                    }

                }

            }


            foreach (var eventTrigger in PersonEventsTimeFrame)
            {
                var processTriger = ProcessTrigger(smokeSession, eventTrigger);
                if (!processTriger)
                    continue;

                foreach (var person in smokeSession.Persons)
                {


                    if (processPerson(person, eventTrigger))
                        continue;

                    var session = person.SmokeSessions
                        .Where(a => a.Statistics != null &&
                                    (a.Statistics.Start > smokeSession.Statistics.Start - eventTrigger.TimeSpan))
                        .ToList();


                    var progress =
                        _db.EventProgresses.FirstOrDefault(a => a.GameProfileId == person.Id &&
                                                       a.EventId == eventTrigger.Event.Id);

                    if (progress == null)
                    {
                        progress = new EventProgress()
                        {
                            GameProfileId = person.Id,
                            EventId = eventTrigger.Event.Id,
                        };
                    }

                    var e = eventTrigger.Execute(session, ref progress);

                    if (e == null)
                    {
                        _db.EventProgresses.AddOrUpdate(progress);
                    }
                    else
                    {
                        try
                        {
                            _db.EventProgresses.Remove(progress);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);

                        }

                        AddEventToPersons(smokeSession, e);
                    }



                }

            }

            _db.SaveChanges();
        }

        private static bool processPerson(Person person, EventTrigger eventTrigger)
        {
            if (person.GameProfile.DoneEvent.Count(a => a.EventId == eventTrigger.Event.Id) > 0)
                return true;
            return false;
        }

        private void AddEventToPersons(SmokeSession smokeSession, Event e)
        {
            if (e != null)
            {
                foreach (var person in smokeSession.Persons)

                {
                    if (person.GameProfile.DoneEvent.Count(a => a.EventId == e.Id) > 0)
                        continue;
                    AddEvent(person, e);
                }
            }
        }

        private void AddEvent(Person person, Event e)
        {
            if (person.GameProfile.DoneEvent.All(a => a.EventId != e.Id))
            {
                person.GameProfile.DoneEvent.Add(new DoneEvent()
                {
                    GameProfileId = person.Id,
                    EventId = e.Id,
                    Obtain = DateTime.Now
                });
                person.GameProfile.Clouds = person.GameProfile.Clouds + e.Clouds;
                person.GameProfile.Experience = person.GameProfile.Experience + e.Experience;

                //Event is done, remove all progress
                var eventPrograss =
                    _db.EventProgresses.Where(a => a.GameProfileId == person.Id && a.EventRecepie.Id == e.Id);

                if (eventPrograss.Any())
                    _db.EventProgresses.RemoveRange(eventPrograss);
            }
            _db.GameProfiles.AddOrUpdate(person.GameProfile);
        }

        private static bool ProcessTrigger(SmokeSession smokeSession, EventTrigger eventTrigger)
        {
            var processTriger = false;
            foreach (var person in smokeSession.Persons)
            {
                if (person.GameProfile.DoneEvent.Count(a => a.EventId == eventTrigger.Event.Id) < 1)
                {
                    processTriger = true;
                }
            }
            return processTriger;
        }
    }

    public class EventTrigger
    {
        private List<EventPartTrigger> Triggers;
        private Event _event;

        public TimeSpan TimeSpan
        {
            get { return _event.TimeFrame.Value; }
        }

        public Event Event
        {
            get { return _event; }
        }

        public EventTrigger(Event e)
        {
            Triggers = new List<EventPartTrigger>();
            foreach (var recepy in e.Recepies)
            {
                Triggers.Add(EventPartTrigger.Factory(recepy));
            }
            _event = e;
        }


        public virtual Event Execute(SmokeSession smokeSession)
        {
            foreach (var trigger in Triggers)
            {
                if (!trigger.Execute(smokeSession))
                    return null;
            }
            return _event;
        }

        public virtual EventProgress Execute(SmokeSession smokeSession, EventProgress eventProgress)
        {
            var result = new EventProgress();
            foreach (var trigger in Triggers)
            {
                var partialResult = trigger.ExecuteMulti(smokeSession);
                if (partialResult.Success)
                {
                    result = result + partialResult;
                }
                else
                {
                    return null;
                }
            }
            eventProgress.IntProgress = eventProgress.IntProgress + result.IntProgress;
            eventProgress.TimeProgress = eventProgress.TimeProgress + result.TimeProgress;

            return eventProgress;
        }

        public virtual Event Execute(List<SmokeSession> smokeSessions, ref EventProgress progress)
        {
            if (progress == null)
                progress = new EventProgress();


            foreach (var smoke in smokeSessions)
            {
                foreach (var trigger in Triggers)
                {
                    var partResult = trigger.ExecuteMulti(smoke);
                    if (partResult.Success)
                    {
                        progress = progress + partResult;
                    }
                }

                if (_event.TriggerCount > 0)
                {
                    if (progress.IntProgress > _event.TriggerCount)
                        return _event;
                }

                if (_event.TriggerTime > new TimeSpan())
                {
                    if (progress.TimeProgress > _event.TriggerTime)
                        return _event;
                }
            }

            return null;

        }

        public virtual Event Execute(SmokeSession smokeSession, ref EventProgress progress)
        {
            foreach (var trigger in Triggers)
            {
                var partResult = trigger.ExecuteMulti(smokeSession);
                if (partResult.Success)
                {
                    progress = progress + partResult;
                }
            }

            if (_event.TriggerCount > 0)
            {
                if (progress.IntProgress > _event.TriggerCount)
                    return _event;
            }

            if (_event.TriggerTime > new TimeSpan())
            {
                if (progress.TimeProgress > _event.TriggerTime)
                    return _event;
            }

            return null;
        }
    }

    internal class EventPartTrigger
    {
        public static EventPartTrigger Factory(EventRecepie recepie)
        {
            switch (recepie.Type)
            {
                case EventRecepieType.PufCount:
                    return new PufCountTrigger(recepie);

                case EventRecepieType.SessionTime:
                    return new SessionTimeTrigger(recepie);

                case EventRecepieType.SmokeTime:
                    return new SmokeTimeTrigger(recepie);

                case EventRecepieType.LongestPuf:
                    return new LongestPufTrigger(recepie);

                case EventRecepieType.AccesoryBrand:
                    return new AccesoryBrand(recepie);

                case EventRecepieType.PufCountSum:
                    return new PufCountSumTrigger(recepie);

                case EventRecepieType.PlaceVisit:
                    return new PlaceVisitTrigger(recepie);

                default:
                    return new EventPartTrigger(recepie);
            }
        }
        protected EventRecepie _recepie;
        public EventPartTrigger(EventRecepie recepie)
        {
            this._recepie = recepie;
        }

        public virtual bool Execute(SmokeSession smokeSession)
        {
            return false;
        }

        public virtual EventPartResult ExecuteMulti(SmokeSession smokeSession)
        {
            if (Execute(smokeSession))
            {
                return new EventPartResult()
                {
                    IntProgress = 1,
                    Success = true
                };
            }

            return new EventPartResult();

        }
    }

    internal class PufCountSumTrigger : EventPartTrigger
    {
        public PufCountSumTrigger(EventRecepie recepie) : base(recepie)
        {
        }

        public override EventPartResult ExecuteMulti(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return new EventPartResult();

            else
            {
                return new EventPartResult()
                {
                    Success = true,
                    IntProgress = smokeSession.Statistics.PufCount,
                    TimeProgress = new TimeSpan()
                };
            }
        }
    }

    public class EventPartResult
    {
        public bool Success { get; set; }

        public int IntProgress { get; set; }

        public TimeSpan TimeProgress { get; set; }
    }

    internal class AccesoryBrand : EventPartTrigger
    {
        public AccesoryBrand(EventRecepie recepie) : base(recepie)
        {

        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.MetaData == null)
                return false;

            if (_recepie.TriggerCountCompare == "to")
            {
                if (smokeSession.MetaData.Tobacco == null)
                    return false;
                return smokeSession.MetaData.Tobacco.BrandName == _recepie.EventString;
            }

            if (_recepie.TriggerCountCompare == "pi")
            {
                if (smokeSession.MetaData.Pipe == null)
                    return false;
                return smokeSession.MetaData.Pipe.BrandName == _recepie.EventString;
            }

            if (_recepie.TriggerCountCompare == "bo")
            {
                if (smokeSession.MetaData.Bowl == null)
                    return false;
                return smokeSession.MetaData.Bowl.BrandName == _recepie.EventString;

            }
            return false;
        }
    }

    #region SessionTimes

    internal class PufCountTrigger : EventPartTrigger
    {

        public PufCountTrigger(EventRecepie e) : base(e)
        {
        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return false;
            if (_recepie.TriggerCountCompare == "eq")
            {
                if (smokeSession.Statistics.PufCount == _recepie.EventNumber)
                    return true;
            }

            if (_recepie.TriggerCountCompare == "gr")
            {
                if (smokeSession.Statistics.PufCount >= _recepie.EventNumber)
                    return true;
            }
            else
            {

                if (smokeSession.Statistics.PufCount <= _recepie.EventNumber)
                    return true;
            }


            return false;
        }

    }

    internal class SessionTimeTrigger : EventPartTrigger
    {
        public SessionTimeTrigger(EventRecepie e) : base(e)
        {
        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return false;

            if (_recepie.EventBool)
            {
                if (smokeSession.Statistics.SessionDuration > _recepie.EventTimeSpan)
                    return true;
            }
            else
            {

                if (smokeSession.Statistics.SessionDuration < _recepie.EventTimeSpan)
                    return true;
            }


            return false;
        }
    }

    internal class SmokeTimeTrigger : EventPartTrigger
    {
        public SmokeTimeTrigger(EventRecepie e) : base(e)
        {
        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return false;

            if (_recepie.EventBool)
            {
                if (smokeSession.Statistics.SmokeDuration > _recepie.EventTimeSpan)
                    return true;
            }
            else
            {

                if (smokeSession.Statistics.SmokeDuration < _recepie.EventTimeSpan)
                    return true;
            }


            return false;
        }
    }

    internal class LongestPufTrigger : EventPartTrigger
    {
        public LongestPufTrigger(EventRecepie e) : base(e)
        {
        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.Statistics == null)
                return false;

            if (_recepie.EventBool)
            {
                if (smokeSession.Statistics.LongestPuf > _recepie.EventTimeSpan)
                    return true;
            }
            else
            {

                if (smokeSession.Statistics.LongestPuf < _recepie.EventTimeSpan)
                    return true;
            }


            return false;
        }
    }

    #endregion

    internal class PlaceVisitTrigger : EventPartTrigger
    {
        public PlaceVisitTrigger(EventRecepie e) : base(e)
        {
        }

        public override bool Execute(SmokeSession smokeSession)
        {
            if (smokeSession.Hookah == null)
                return false;

            if (smokeSession.PlaceId == _recepie.EventForeinId)
                return true;

            return false;
        }

    }


}
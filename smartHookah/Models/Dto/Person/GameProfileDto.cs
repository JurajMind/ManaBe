using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    public class GameProfileSimpleDto
    {
        public int PersonId { get; set; }

        public int Level { get; set; }

        public int TestLevel { get; set; }

        public double LvlProgress { get; set; }

        public Int64 Experience { get; set; }

        public int Clouds { get; set; }

        public GamePictureDto Picture { get; set; }

        public static GameProfileSimpleDto FromModel(GameProfile model) => model == null ? null : new GameProfileSimpleDto()
        {
            PersonId = model.PersonId,
            Level = model.Level,
            Experience = model.Experience,
            Clouds = model.Clouds,
            Picture = GamePictureDto.FromModel(model.Picture)
        };

        public GameProfile ToModel()
        {
            return new GameProfile()
            {
                PersonId = PersonId,
                Level = Level,
                Experience = Experience,
                Clouds = Clouds,
                Picture = Picture.ToModel()
            };
        }
    }

    public class GameProfileDto
    {
        public int PersonId { get; set; }
        
        public int Level { get; set; }

        public int TestLevel { get; private set; }

        public double LvlProgress { get; private set; }

        public Int64 Experience { get; set; }

        public int Clouds { get; set; }

        public IEnumerable<Reward> GainRewards { get; private set; }

        public ICollection<DoneEvent> DoneEvent { get; set; }

        public ICollection<EventProgress> InProgresses { get; set; }

        public int? TitleId { get; set; }

        public TitleDto Title { get; set; }

        public GamePictureDto Picture { get; set; }

        public int? Badge1Id { get; set; }

        public BadgeDto Badge1 { get; set; }

        public int? Badge2Id { get; set; }

        public BadgeDto Badge2 { get; set; }

        public int? Badge3Id { get; set; }

        public BadgeDto Badge3 { get; set; }

        public int? Badge4Id { get; set; }

        public BadgeDto Badge4 { get; set; }

        public List<BadgeDto> Badges { get; private set; }

        public static GameProfileDto FromModel(GameProfile model)
        {
            return new GameProfileDto()
            {
                PersonId = model.PersonId, 
                Level = model.Level, 
                TestLevel = model.TestLevel, 
                LvlProgress = model.LvlProgress, 
                Experience = model.Experience, 
                Clouds = model.Clouds, 
                GainRewards = model.GainRewards, 
                DoneEvent = model.DoneEvent, 
                InProgresses = model.InProgresses, 
                TitleId = model.TitleId, 
                Title = TitleDto.FromModel(model.Title), 
                Picture = GamePictureDto.FromModel(model.Picture), 
                Badge1Id = model.Badge1Id, 
                Badge1 = BadgeDto.FromModel(model.Badge1), 
                Badge2Id = model.Badge2Id, 
                Badge2 = BadgeDto.FromModel(model.Badge2), 
                Badge3Id = model.Badge3Id, 
                Badge3 = BadgeDto.FromModel(model.Badge3), 
                Badge4Id = model.Badge4Id, 
                Badge4 = BadgeDto.FromModel(model.Badge4), 
                Badges = model.Badges.Select(BadgeDto.FromModel).ToList(), 
            }; 
        }

        public GameProfile ToModel()
        {
            return new GameProfile()
            {
                PersonId = PersonId, 
                Level = Level, 
                Experience = Experience, 
                Clouds = Clouds, 
                DoneEvent = DoneEvent, 
                InProgresses = InProgresses, 
                TitleId = TitleId, 
                Title = Title.ToModel(), 
                Picture = Picture.ToModel(), 
                Badge1Id = Badge1Id, 
                Badge1 = Badge1.ToModel(), 
                Badge2Id = Badge2Id, 
                Badge2 = Badge2.ToModel(), 
                Badge3Id = Badge3Id, 
                Badge3 = Badge3.ToModel(), 
                Badge4Id = Badge4Id, 
                Badge4 = Badge4.ToModel(), 
            }; 
        }
    }

    public class DoneEventDto
    {
        public int Id { get; set; }

        public int GameProfileId { get; set; }

        public GameProfileDto GameProfile { get; set; }

        public int EventId { get; set; }

        public EventDto Event { get; set; }

        public DateTime Obtain { get; set; }

        public static DoneEventDto FromModel(DoneEvent model)
        {
            return new DoneEventDto()
            {
                Id = model.Id, 
                GameProfileId = model.GameProfileId, 
                GameProfile = GameProfileDto.FromModel(model.GameProfile), 
                EventId = model.EventId, 
                Event = EventDto.FromModel(model.Event), 
                Obtain = model.Obtain, 
            }; 
        }

        public DoneEvent ToModel()
        {
            return new DoneEvent()
            {
                Id = Id, 
                GameProfileId = GameProfileId, 
                GameProfile = GameProfile.ToModel(), 
                EventId = EventId, 
                Event = Event.ToModel(), 
                Obtain = Obtain, 
            }; 
        }
    }

    public class EventProgressDto
    {
        public int Id { get; set; }

        public int GameProfileId { get; set; }

        public GameProfileDto GameProfile { get; set; }

        public int EventId { get; set; }

        public EventDto EventRecepie { get; set; }

        public int IntProgress { get; set; }

        public TimeSpan TimeProgress { get; set; }

        public string StringProgress { get; set; }

        public static EventProgressDto FromModel(EventProgress model)
        {
            return new EventProgressDto()
            {
                Id = model.Id, 
                GameProfileId = model.GameProfileId, 
                GameProfile = GameProfileDto.FromModel(model.GameProfile), 
                EventId = model.EventId, 
                EventRecepie = EventDto.FromModel(model.EventRecepie), 
                IntProgress = model.IntProgress, 
                TimeProgress = model.TimeProgress, 
                StringProgress = model.StringProgress, 
            }; 
        }

        public EventProgress ToModel()
        {
            return new EventProgress()
            {
                Id = Id, 
                GameProfileId = GameProfileId, 
                GameProfile = GameProfile.ToModel(), 
                EventId = EventId, 
                EventRecepie = EventRecepie.ToModel(), 
                IntProgress = IntProgress, 
                TimeProgress = TimeProgress, 
                StringProgress = StringProgress, 
            }; 
        }
    }

    public class EventDto
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public string Description { get; set; }

        public ICollection<Reward> Rewards { get; set; }

        public ICollection<EventRecepie> Recepies { get; set; }

        public ICollection<GameProfile> Done { get; set; }

        public ICollection<EventProgress> InProgresses { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public Int64? TimeFrameTicks { get; set; }

        public TimeSpan? TimeFrame { get; set; }

        public int TriggerCount { get; set; }

        public TimeSpan TriggerTime { get; set; }

        public EventType Type { get; set; }

        public int Experience { get; set; }

        public int Clouds { get; set; }

        public static EventDto FromModel(Event model)
        {
            return new EventDto()
            {
                Id = model.Id, 
                Name = model.Name, 
                Description = model.Description, 
                Rewards = model.Rewards, 
                Recepies = model.Recepies, 
                Done = model.Done, 
                InProgresses = model.InProgresses, 
                From = model.From, 
                To = model.To, 
                TimeFrameTicks = model.TimeFrameTicks, 
                TimeFrame = model.TimeFrame, 
                TriggerCount = model.TriggerCount, 
                TriggerTime = model.TriggerTime, 
                Type = model.Type, 
                Experience = model.Experience, 
                Clouds = model.Clouds, 
            }; 
        }

        public Event ToModel()
        {
            return new Event()
            {
                Id = Id, 
                Name = Name, 
                Description = Description, 
                Rewards = Rewards, 
                Recepies = Recepies, 
                Done = Done, 
                InProgresses = InProgresses, 
                From = From, 
                To = To, 
                TimeFrameTicks = TimeFrameTicks, 
                TimeFrame = TimeFrame, 
                TriggerCount = TriggerCount, 
                TriggerTime = TriggerTime, 
                Type = Type, 
                Experience = Experience, 
                Clouds = Clouds, 
            }; 
        }
    }

    public class EventRecepieDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public EventDto Event { get; set; }

        public string EventString { get; set; }

        public decimal EventNumber { get; set; }

        public bool EventBool { get; set; }

        public int EventForeinId { get; set; }

        public TimeSpan EventTimeSpan { get; set; }

        public bool Active { get; set; }

        public EventRecepieType Type { get; set; }

        public string TriggerCountCompare { get; set; }

        public static EventRecepieDto FromModel(EventRecepie model)
        {
            return new EventRecepieDto()
            {
                Id = model.Id, 
                EventId = model.EventId, 
                Event = EventDto.FromModel(model.Event), 
                EventString = model.EventString, 
                EventNumber = model.EventNumber, 
                EventBool = model.EventBool, 
                EventForeinId = model.EventForeinId, 
                EventTimeSpan = model.EventTimeSpan, 
                Active = model.Active, 
                Type = model.Type, 
                TriggerCountCompare = model.TriggerCountCompare, 
            }; 
        }

        public EventRecepie ToModel()
        {
            return new EventRecepie()
            {
                Id = Id, 
                EventId = EventId, 
                Event = Event.ToModel(), 
                EventString = EventString, 
                EventNumber = EventNumber, 
                EventBool = EventBool, 
                EventForeinId = EventForeinId, 
                EventTimeSpan = EventTimeSpan, 
                Active = Active, 
                Type = Type, 
                TriggerCountCompare = TriggerCountCompare, 
            }; 
        }
    }

    public class RewardDto
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public EventDto Event { get; set; }

        public int? EventId { get; set; }

        public string Param { get; set; }

        public static RewardDto FromModel(Reward model)
        {
            return new RewardDto()
            {
                Id = model.Id, 
                Name = model.Name, 
                Event = EventDto.FromModel(model.Event), 
                EventId = model.EventId, 
                Param = model.Param, 
            }; 
        }

        public Reward ToModel()
        {
            return new Reward()
            {
                Id = Id, 
                Name = Name, 
                Event = Event.ToModel(), 
                EventId = EventId, 
                Param = Param, 
            }; 
        }
    }

    [DataContract]
    public class TitleDto
    {
        [DataMember]
        [JsonProperty("titleName")]
        public string TitleName { get; set; }

        public static TitleDto FromModel(Title model)
        {
            return new TitleDto()
            {
                TitleName = model.TitleName, 
            }; 
        }

        public Title ToModel()
        {
            return new Title()
            {
                TitleName = TitleName, 
            }; 
        }
    }

    public class BadgeDto
    {
        public string BadgePictureUrl { get; set; }

        public static BadgeDto FromModel(Badge model)
        {
            return new BadgeDto()
            {
                BadgePictureUrl = model.BadgePictureUrl, 
            }; 
        }

        public Badge ToModel()
        {
            return new Badge()
            {
                BadgePictureUrl = BadgePictureUrl, 
            }; 
        }
    }

    public class AchievmentDto
    {
        public string AchievmentName { get; set; }

        public static AchievmentDto FromModel(Achievment model)
        {
            return new AchievmentDto()
            {
                AchievmentName = model.AchievmentName, 
            }; 
        }

        public Achievment ToModel()
        {
            return new Achievment()
            {
                AchievmentName = AchievmentName, 
            }; 
        }
    }

    public class GamePictureDto
    {
        public string PictureUrl { get; set; }

        public static GamePictureDto FromModel(GamePicture model) => model == null ? null : new GamePictureDto()
        {
            PictureUrl = model.PictureUrl,
        };

        public GamePicture ToModel()
        {
            return new GamePicture()
            {
                PictureUrl = PictureUrl, 
            }; 
        }
    }
}
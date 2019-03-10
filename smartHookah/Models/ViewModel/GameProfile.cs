using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using smartHookah.Controllers;
using smartHookah.Models.Db;

namespace smartHookah.Models
{
    public class GameProfile
    {
        private const double lvlConst = 0.08;
        public static GameProfile Default()
        {
            var result = new GameProfile();
            result.Level = 1;
            return result;
        }

        [Key]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        public int Level { get; set; }

        [NotMapped]
        public int TestLevel => (int) ((lvlConst * Math.Sqrt(Experience)) + 1);

        [NotMapped]
        public double LvlProgress {
            get { return ((double) Experience / (double) ExpToLevel(TestLevel+1)) * 100; } }
        public int ExpToLevel(int level)
        {
            return (int)(Math.Pow(level / lvlConst, 2));
        }

        public Int64 Experience { get; set; }

        public int Clouds { get; set; }

        public  IEnumerable<Reward> GainRewards
        {
            get { return DoneEvent.SelectMany(a => a.Event.Rewards); }
        }

        public virtual ICollection<DoneEvent> DoneEvent { get; set; }

        public virtual ICollection<EventProgress> InProgresses { get; set; }

        //Title 
        public int? TitleId { get; set; }

        public virtual Title Title { get; set; }

        public virtual GamePicture Picture { get; set; }

        //Badges 
        public int? Badge1Id { get; set; }

        public virtual Badge Badge1 { get; set; }

        public int? Badge2Id { get; set; }
        public virtual Badge Badge2 { get; set; }

        public int? Badge3Id { get; set; }
        public virtual Badge Badge3 { get; set; }
        public int? Badge4Id { get; set; }
        public virtual Badge Badge4 { get; set; }

        [NotMapped]
        public List<Badge> Badges
        {
            get
            {
                var badges = new List<Badge>();

                AddBadge(badges, Badge1);
                AddBadge(badges, Badge2);
                AddBadge(badges, Badge3);
                AddBadge(badges, Badge4);

                return badges;
            }
        }





        private void AddBadge(List<Badge> badges, Badge badge)
        {
            if (badge != null)
            {
                badges.Add(badge);
            }
        }
    }

    public class DoneEvent
    {
        public int Id { get; set; }
        public int GameProfileId { get; set; }
        public virtual GameProfile GameProfile { get; set; }
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public DateTime Obtain { get; set; }
        
    }

    public class EventProgress
    {
       public int Id { get; set; }

        [ForeignKey("GameProfile")]
        public int GameProfileId { get; set; }
        public virtual GameProfile GameProfile { get; set; }
     
        public int EventId { get; set; }
        public virtual Event EventRecepie { get; set; }

        public int IntProgress { get; set; }

        public TimeSpan TimeProgress { get; set; }

        public string StringProgress { get; set; }

        public static EventProgress operator +(EventProgress x,EventProgress y)
        {
            if (x == null || y == null)
                return null;


            if(x.GameProfileId != y.GameProfileId || x.EventId != y.EventId)
            return null;

            x.TimeProgress = x.TimeProgress + y.TimeProgress;
            x.IntProgress = x.IntProgress + y.IntProgress;

            return x;

        }

        public static EventProgress operator +(EventProgress x, EventPartResult y)
        {
            if (x == null || y == null)
                return null;



            x.TimeProgress = x.TimeProgress + y.TimeProgress;
            x.IntProgress = x.IntProgress + y.IntProgress;

            return x;

        }
    }


    public class Event
    {
        public int Id { get; set; }

        [MaxLength(128)]
        public String Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Reward> Rewards { get; set; }

        public virtual ICollection<EventRecepie> Recepies { get; set; }

        public virtual ICollection<GameProfile> Done { get; set; }

        public virtual ICollection<EventProgress> InProgresses { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public Int64? TimeFrameTicks { get; set; }

        [NotMapped]
        public TimeSpan? TimeFrame
        {
            get
            {
                if (TimeFrameTicks == null)
                    return null;
                return TimeSpan.FromTicks(TimeFrameTicks.Value);
            }
            set
            {
                if (value == null)
                    TimeFrameTicks = null;
                else
                {
                    TimeFrameTicks = value.Value.Ticks;
                }
                
            }
        }

        public int TriggerCount { get; set; }

        public TimeSpan TriggerTime { get; set; }

        public EventType Type { get; set; }

        public int Experience { get; set; }

        public int Clouds { get; set; }
    }

    public enum EventType
    {
        Person = 1,
        Place =  2,
    }

    public class EventRecepie
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        public virtual Event Event  {get; set; }

        public string EventString { get; set; }

        public decimal EventNumber { get; set; }

        public bool EventBool { get; set; }

        public int EventForeinId { get; set; }

        public TimeSpan EventTimeSpan { get; set; }

        public bool Active { get; set; }


        public EventRecepieType Type { get; set; }
       

       [MaxLength(2)]
       public string TriggerCountCompare { get; set; }

    }

    public enum EventRecepieType
    {
        PufCount = 0,
        SessionTime = 1,
        SmokeTime = 2,
        LongestPuf = 3,
        PlaceVisit = 4,
        AccesoryBrand = 5,
        Accesory = 6,
        Date = 7,
        Time = 8,
        TimeSpan = 9,
        WeekDay = 10,
        AllAccesoryBrand = 11,
        SmokeWith = 12,
        TobaccoInMix = 13,
        SessionCount = 14,
        PufCountSum = 15,
      
    }


    public class Reward
    {
        public static Reward Transform(Reward reward,string type)
        {
            switch (type)
            {
                case "Title":
                    return new Title(reward);

                case "Badge":
                    return new Badge(reward);

                case "Achievment":
                    return new Achievment(reward);

                case "GamePicture":
                    return new GamePicture(reward);

                default:
                    return reward;
            }
           
        }
        public virtual string GetWebName()
        {
            return "Reward";
        }
        public Reward()
        { }

        public Reward(Reward reward)
        {
            this.Id = reward.Id;
            this.Name = reward.Name;
            this.Event = reward.Event;
            this.EventId = reward.EventId;
            this.Param = reward.Param;
        }

        public int Id { get; set; }

        public String Name { get; set; }

        public virtual Event Event { get; set; }

        public int? EventId { get; set; }

        public string Param { get; set; }

    }

    public class Title : Reward
    {
        public Title()
        {
            
        }
        public Title(Reward reward):base(reward)
        { }
        [NotMapped]
        public string TitleName
        {
            get { return Param; }
            set { Param = value; }
        }
        public override string GetWebName()
        {
            return "Title";
        }
    }

    public class Badge : Reward
    {
        public Badge()
        {

        }
        public Badge(Reward reward):base(reward)
        { }
        [NotMapped]
        public string BadgePictureUrl
        {
            get { return Param; }
            set { Param = value; }
        }
        public override string GetWebName()
        {
            return "Badge";
        }
    }

    public class Achievment : Reward
    {
        public Achievment()
        {

        }
        public Achievment(Reward reward):base(reward)
        { }
        [NotMapped]
        public string AchievmentName
        {
            get { return Param; }
            set { Param = value; }
        }

        public override string GetWebName()
        {
            return "Achievment";
        }
    }

    public class GamePicture : Reward
    {
        public GamePicture()
        {

        }
        public GamePicture(Reward reward):base(reward)
        { }
        [NotMapped]
        public string PictureUrl
        {
            get { return Param; }
            set { Param = value; }
        }

        public override string GetWebName()
        {
            return "GamePicture";
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;
using smartHookah.Helpers.ModelExtensions;

namespace smartHookah.Models
{
    using smartHookah.Models.Redis;

    public  class 
        SmokeSession
    {

        public SmokeSession()
        {
            Token = Support.Support.RandomString(10);
        }
        [Key]
        public int Id { get; set; }

        public byte[] Version { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
        public string SessionId { get; set; }
        public virtual Hookah Hookah { get; set; }

        public virtual ICollection<Person> Persons { get; set; }
        
        public bool IsPersonAssign(int id)
        {
            return Persons.Any(a => a.Id == id);
        }

        public int HookahId { get; set; }

        [Column("Pufs")]
        public virtual ICollection<DbPuf> DbPufs { get; set; }


        public string StorePath { get; set; }

        [NotMapped]
        public virtual ICollection<DbPuf> Pufs {
            get
            {
                if (this.StorePath != null)
                {
                    return this.StoredPufs();
                }

                return this.DbPufs;
            }

        }

        public int? MetaDataId { get; set; }
        public virtual SmokeSessionMetaData MetaData { get; set; }
        
        public virtual ICollection<SmokeEvent> SmokeEvents{get;set;
        }

        public int? StatisticsId { get; set; }
        
        public virtual SmokeSessionStatistics Statistics { get; set; }
        
        public virtual TobaccoReview Review { get; set; }

        [Index("Token")]
        [MaxLength(10)]
        public string Token { get; set; }

        public SessionReport Report { get; set; } 

        [NotMapped]
        public bool PlaceSession { get; set; } = false;

        public virtual Place Place { get; set; }

        public int? PlaceId { get; set; }
        
        public virtual ICollection<HookahOrder> HookahOrder { get; set; }

        [NotMapped]
        public virtual DynamicSmokeStatistic DynamicSmokeStatistic { get; set; }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;
using smartHookah.Models.Db;
using smartHookah.Support;

namespace smartHookah.Models
{
    using smartHookah.Models.Dto;

    public class PipeAccesory
    {
        [Key]
        public int Id { get; set; }

        public byte[] Version { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool Deleted { get; set; }

        public string AccName { get; set; }
        [Required]
        public string BrandName { get; set; }
        [ForeignKey("BrandName")]
        public virtual Brand Brand { get; set; }
        public virtual PipeAccesoryStatistics Statistics { get; set; }

        public virtual ICollection<SimilarAccesories> SimilarAccesories { get; set; }

        public virtual ICollection<Media> Mediae { get; set; }

        public virtual ICollection<PipeAccesoryLike> Likes { get; set; }

        public int LikeCount { get; set; }

        public int DisLikeCount { get; set; }


        [MaxLength(128)]
        public string Picture { get; set; }

        public PipeAccesory()
        {
            
        }

        public string GetTypeName()
        {
            if (this is Bowl)
                return "Bowl";

            if (this is Pipe)
                return "Hookah";

            if (this is Coal)
                return "Coal";
            
            if (this is HeatManagment)
                return "HeatManagement";

            if (this is Tobacco)
                return "Tobacco";

            return "";
        }

        public AccesoryType GetTypeEnum()
        {
            Enum.TryParse<AccesoryType>(this.GetTypeName(), true, out var result);

            return result;
        }


        public PipeAccesory(PipeAccesory accesory)
        {
            this.Id = accesory.Id;
            this.Version = accesory.Version;
            this.CreatedAt = accesory.CreatedAt;
            this.UpdatedAt = accesory.UpdatedAt;
            this.Deleted = accesory.Deleted;
            this.AccName = accesory.AccName;
            this.BrandName = accesory.BrandName;
        }

    }

    public class PipeAccesoryLike
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public virtual Person Person { get; set; }
        
        public int PipeAccesoryId { get; set; }

        public virtual PipeAccesory PipeAccesory { get; set; }
        public int Value { get; set; }
    }

    public class SimilarAccesories
    {
        public int Id { get; set; }

        public int OriginalId { get; set; }
        public virtual PipeAccesory Original { get; set; }

        public int SimilarId { get; set; }
        public virtual PipeAccesory Similar { get; set; }

        public int? PersonId { get; set; }
        public virtual  Person Person { get; set; }
      
    }


    public class TobaccoTaste
    {
        public int Id { get; set; }

        public string OriginalName { get; set; }

        public string EngName { get; set; }

        public string CzName { get; set; }

        public virtual ICollection<Tobacco> Tobaccos { get; set; }


        public string ToLocalize()
        {
            var culture = Extensions.ResolveCulture();
            if (culture.TwoLetterISOLanguageName == "cs")
            {
                return CzName;
            }

            return OriginalName;

        }
    }

    public class Tobacco : PipeAccesory
    {
        public string SubCategory { get; set; }

        public ICollection<TobaccoReview> Reviews { get; set; }

        public Tobacco()
        {
            
        }

        public Tobacco(PipeAccesory accesory) : base(accesory)
        {
            
        }

        public virtual ICollection<TobaccoTaste> Tastes { get; set; }

        public virtual double GetTobacoEstimated(SmartHookahContext db)
        {
            if (this.Statistics == null)
            {
               var stats =  db.Tobaccos.Where(a => a.Id == this.Id).Include(a => a.Statistics);
               var tobaco = stats.FirstOrDefault();
                this.Statistics = tobaco.Statistics;
            }
          
            return this.Statistics?.PufCount ?? 300;
        }
      
    }

    public class TobaccoMix : Tobacco
    {
        public TobaccoMix()
        {
            Tobaccos = new List<TobacoMixPart>();
        }
        public virtual ICollection<TobacoMixPart> Tobaccos { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public MixLayerMethod? LayerMethod { get; set; } = 0;

        public override double GetTobacoEstimated(SmartHookahContext db)
        {
            var storedPufCount = base.GetTobacoEstimated(db);

            if(storedPufCount != 300)
                return storedPufCount;

            else
            {
                var calculatedPufCount = 0d;
                var allTobaccos = Tobaccos.Sum(a => a.Fraction);
                foreach (var tobacoPart in Tobaccos)
                {
                    var fraction = tobacoPart.Fraction / allTobaccos;
                    calculatedPufCount = calculatedPufCount + (fraction * tobacoPart.Tobacco.GetTobacoEstimated(db));
                }

                return calculatedPufCount;
            }

        }
    }

    public enum MixLayerMethod
    {
        Unknown = 0,
        Layering = 1,
        PreMixed = 2,
        FlavorFocusPack = 3,
        DoubleFlavor = 4,
    }

    public class TobacoMixPart { 
        public int Id { get; set; }
        public int TobaccoId { get; set; }
        public int Fraction { get; set; }

        public virtual Tobacco Tobacco { get; set; }

    }
    
    public class Pipe : PipeAccesory
    {
        public Pipe()
        { }

        public Pipe(PipeAccesory accesory) : base(accesory)
        {
            
        }
    }

    public class Bowl : PipeAccesory
    {
        public Bowl()
        {
            
        }

        public Bowl(PipeAccesory bowl) : base(bowl)
        {
            
        }
    }

    public class Coal : PipeAccesory
    {
        public Coal()
        {

        }

        public Coal(PipeAccesory bowl) : base(bowl)
        {

        }
    }

    public class HeatManagment : PipeAccesory
    {
        public HeatManagment()
        {

        }

        public HeatManagment(PipeAccesory bowl) : base(bowl)
        {

        }
    }

    public enum VoteValue { Like = 1, Dislike = -1, Unlike = 0 }

}
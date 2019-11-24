using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class PlaceTranslation
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }

        [ForeignKey("Id")]
        public virtual Place.Place Place { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(2)]
        public string Lng { get; set; }

        public string Descriptions { get; set; }

        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        public DateTime Created { get; set; }

        public bool Deleted { get; set; }

        public DateTime Updated { get; set; }

    }

    public class FeatureMixCreatorTranslation
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }

        [ForeignKey("Id")]
        public virtual FeatureMixCreator FeatureMixCreator { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(2)]
        public string Lng { get; set; }

        public string Descriptions { get; set; }

        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        public DateTime Created { get; set; }

        public bool Deleted { get; set; }

        public DateTime Updated { get; set; }

    }

    public class BrandTranslation
    {
        [Key]
        [Column(Order = 0)]
        public string Id { get; set; }

        [ForeignKey("Id")]
        public virtual Brand Brand { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(2)]
        public string Lng { get; set; }

        public string Descriptions { get; set; }

        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        public DateTime Created { get; set; }

        public bool Deleted { get; set; }

        public DateTime Updated { get; set; }

    }
}
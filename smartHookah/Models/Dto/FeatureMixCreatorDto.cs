using System.Collections.Generic;
using System.Linq;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    public class FeatureMixCreatorDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public IList<SocialMedia> SocialMedias { get; set; }

        public IList<MediaDto> Medias { get; set; }

        public string LogoPicture { get; set; }

        public int PersonId { get; set; }

        public int Id { get; set; }



        public static FeatureMixCreatorDto FromModel(FeatureMixCreator model)
        {
            return new FeatureMixCreatorDto()
            {
                Name = model.Name, 
                Description = model.Description, 
                Location = model.Location, 
                SocialMedias = model.SocialMedias.ToList(), 
                Medias = MediaDto.FromModelList(model.Medias).ToList(), 
                LogoPicture = model.LogoPicture, 
                PersonId = model.PersonId,
                Id = model.Id, 

            }; 
        }

        public static IEnumerable<FeatureMixCreatorDto> FromModelList(IEnumerable<FeatureMixCreator> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }
    }
}
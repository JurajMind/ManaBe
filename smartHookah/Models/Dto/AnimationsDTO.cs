using System.Collections.Generic;

using smartHookah.Helpers;

namespace smartHookah.Models.Dto
{
    public class AnimationsDTO : DTO
    {
        public ICollection<Animation> Animations { get; set; }

        public AnimationsDTO(List<Animation> animations = null, int hookahOSVersion = -1)
        {
            Animations = new List<Animation>();
            if (animations != null)
                foreach (var animation in animations)
                {
                    if (hookahOSVersion >= animation.VersionFrom && hookahOSVersion <= animation.VersionTo)
                        Animations.Add(animation);
                }
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Entities.unsplash
{
    public class UnsplashPhotographer: BaseMetaExtendedEntity
    {
        [Required, MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
        public string UserName { get; set; }

        [Required, MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
        public string Name { get; set; }

        [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
        public string Location { get; set; }

        [MaxLength(BaseMetaEntityUtil.ExtraLongStringMaxLength)]
        public string Bio { get; set; }

        [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
        public string Portfolio { get; set; }

    }
}

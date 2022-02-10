using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Entities
{
    public class BaseMetaExtendedEntity: BaseMetaEntity
    {
        [Required] 
        public bool Archived { get; set; }

        [Required, MaxLength(BaseMetaEntityUtil.EmailMaxLength)]
        public string ModifiedBy { get; set; }

    }
}

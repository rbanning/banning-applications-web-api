using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

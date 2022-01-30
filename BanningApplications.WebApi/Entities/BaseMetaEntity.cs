using System;

namespace BanningApplications.WebApi.Entities
{
    public abstract class BaseMetaEntity: BaseMetaEntityGeneric<string>
    {
		public BaseMetaEntity()
			:base(Guid.NewGuid().ToString("n"))
		{}
	}
}

using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace BanningApplications.WebApi.Entities
{
	public static class BaseMetaEntityUtil
	{
		public const int IdStringMaxLength = 50;	
		public const int IdGuidMaxLength = 36;      //Allows GUID with dashes
		public const int EmailMaxLength = 250;
		public const int ShortStringMaxLength = 20;
		public const int LongStringMaxLength = 100;
		public const int ExtraLongStringMaxLength = 500;
	}
	public abstract class BaseMetaEntityGeneric<T>
    {

		[Required, MaxLength(BaseMetaEntityUtil.IdGuidMaxLength)]
		public T Id { get; set; }
		[Required]
		public DateTime CreateDate { get; set; }
		[Required]
		public DateTime ModifyDate { get; set; }

		public BaseMetaEntityGeneric()
			:this(default(T))
		{}

		public BaseMetaEntityGeneric(T id)
		{
			Id = id;
			CreateDate = DateTime.UtcNow;
			ModifyDate = DateTime.UtcNow;
		}
	}
}

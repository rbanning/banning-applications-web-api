using System;

namespace BanningApplications.WebApi.Dtos
{
    public class BaseAbstractDto
    {
	    public string Id { get; set; }
	    public DateTime CreateDate { get; set; }
	    public DateTime ModifyDate { get; set; }
    }
}

namespace BanningApplications.WebApi.Dtos
{
    public class BaseAbstractExtendedDto: BaseAbstractDto
    {
	    public bool Archived { get; set; }

	    public string ModifiedBy { get; set; }

    }
}

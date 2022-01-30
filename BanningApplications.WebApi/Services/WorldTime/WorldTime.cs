namespace BanningApplications.WebApi.Services.WorldTime
{
    public class WorldTime
    {
	    public int Year { get; set; }
	    public int Month { get; set; }
	    public int Day { get; set; }
	    public int Hour { get; set; }
	    public int Minute { get; set; }
	    public int Seconds { get; set; }
	    public string DateTime { get; set; }
	    public string TimeZone { get; set; }
	    public string DayOfWeek { get; set; }
	    public bool DstActive { get; set; }
    }
}

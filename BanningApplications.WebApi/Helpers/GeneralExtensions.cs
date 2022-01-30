using System;

namespace BanningApplications.WebApi.Helpers
{
	public static class GeneralExtensions
	{

		public static int CalculateAge(this DateTime dob)
		{
			int age = 0;
			age = DateTime.Now.Year - dob.Year;
			if (DateTime.Now.DayOfYear < dob.DayOfYear)
				age = age - 1;

			return age;
		}
	}
}

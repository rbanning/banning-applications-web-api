using System;
using System.Linq;

namespace BanningApplications.WebApi.Helpers
{
    public static class DateExtensions
    {
	    public static bool IsSameDay(this DateTime me, DateTime date)
	    {
		    return _IsSameDay(me.ToUniversalTime(), date.ToUniversalTime());
	    }

	    private static bool _IsSameDay(DateTime uDate1, DateTime uDate2)
	    {
		    return uDate1.Year == uDate2.Year
		           && uDate1.Month == uDate2.Month
		           && uDate1.Day == uDate2.Day;
	    }

	    public static DateTime? ParseToDateTimeLocal(this string me)
	    {
		    if (DateTime.TryParse(me, out DateTime date))
		    {
			    return date.ToLocalTime();
		    }
			//else
			return null;
	    }

	    public static DateTime? ParseToDateTimeUtc(this string me)
	    {
		    if (DateTime.TryParse(me, out DateTime date))
		    {
			    return date.ToUniversalTime();
		    }
			//else
			return null;
	    }

	    public static DateTime SetTime(this DateTime date, int hours, int minutes, int seconds = 0,
		    int milliseconds = 0)
	    {
		    return date.AddHours(hours - date.Hour)
			    .AddMinutes(minutes - date.Minute)
			    .AddSeconds(seconds - date.Second)
			    .AddMilliseconds(milliseconds - date.Millisecond);
	    }
	    public static DateTime? SetTimeNullable(this DateTime? date, int hours, int minutes, int seconds = 0,
		    int milliseconds = 0)
	    {
		    if (date.HasValue)
		    {
			    return SetTime(date.Value, hours, minutes, seconds, milliseconds);
		    }

			//else
			return null;
	    }

	    public static DateTime MoveToBusinessDay(this DateTime date, int factor = 1)
	    {
		    var next = date.AddSeconds(0); //clone

			while (next.DayOfWeek == DayOfWeek.Saturday || next.DayOfWeek == DayOfWeek.Sunday)
			{
				next = next.AddDays(factor);
			}

			return next;
	    }

	    public static DateTime? Min(params DateTime?[] dates)
	    {
		    DateTime? result = null;
		    foreach (var d in dates)
		    {
			    if (d.HasValue && (!result.HasValue || result.Value > d.Value))
			    {
				    result = d;
			    }
		    }

		    return result;
	    }
	    public static DateTime? Max(params DateTime?[] dates)
	    {
		    DateTime? result = null;
		    foreach (var d in dates)
		    {
			    if (d.HasValue && (!result.HasValue || result.Value < d.Value))
			    {
				    result = d;
			    }
		    }

		    return result;
	    }

	    public static int? DateOffset(this DateTime? date)
	    {
			return date.HasValue ? (int?) (date.Value - date.Value.ToUniversalTime()).Hours : null;
	    }

		public static DateTime? QueryDateTime(this string query, bool useUtc = true, bool basedOnStartOfDay = false)
	    {
		    if (string.IsNullOrEmpty(query))
		    {
			    return null;
		    }

		    var result = DateTime.UtcNow;
		    if (basedOnStartOfDay)
		    {
			    result = result.SetTime(0, 0, 0, 0);
		    }

		    var factor = 1;
		    var parts = query.Split(" ");
		    if (parts.Length > 1 && int.TryParse(parts[0], out int xFactor))
		    {
			    query = parts.Last();
			    factor = xFactor;
		    } 
		    else if (query.StartsWith("-"))
		    {
			    query = query.Substring(1);
			    factor = -1;
		    }
		    else if (query.StartsWith("+"))
		    {
			    query = query.Substring(1);
		    }

		    return result.Add(query.ParseDateTimeQuery(factor, useUtc));
	    }

	    public enum DateTimeQueryEnum
	    {
			Day,
			Days,
			Week,
			Weeks,
			Month,
			Months,
			Year,
			Years,
			Hour,
			Hours,
			Minute,
			Minutes,
			Second,
			Seconds,
			BusinessDay,
			BusinessDays,
			BusinessWeek,
			BusinessWeeks,
			BusinessMonth,
			BusinessMonths,
			BusinessYear,
			BusinessYears,
	    }

	    public static DateTimeQueryEnum? TryParseDateTimeQueryEnum(this string query)
	    {
		    // ReSharper disable once RedundantTypeArgumentsOfMethod
		    if (Enum.TryParse<DateTimeQueryEnum>(query, true, out DateTimeQueryEnum result))
		    {
			    return result;
		    }
			//else
			return null;
	    }

	    public static TimeSpan ParseDateTimeQuery(this string query, int factor = 1, bool useUtc = true)
	    {
		    int? amount = null;
		    DateTimeQueryEnum? what = null;

		    if (!string.IsNullOrEmpty(query))
		    {
			    var parts = query.Split(" ").Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();

			    foreach (var part in parts)
			    {
				    if (!amount.HasValue && int.TryParse(part, out int result))
				    {
					    amount = result;
				    }
				    else
				    {
					    what ??= part.TryParseDateTimeQueryEnum();
				    }

				}
		    }


			if (what.HasValue)
		    {
			    amount ??= 1;   //default value if none was provided
			    amount *= factor; //multiply by factor

			    var now = useUtc ? DateTime.UtcNow : DateTime.Now;	//used on some of the cases

			    switch (what)
			    {
				    case DateTimeQueryEnum.Day:
				    case DateTimeQueryEnum.Days:
					    return new TimeSpan(amount.Value, 0, 0, 0);
				    case DateTimeQueryEnum.Week:
				    case DateTimeQueryEnum.Weeks:
					    return new TimeSpan(amount.Value * 7, 0, 0, 0);
				    case DateTimeQueryEnum.Month:
				    case DateTimeQueryEnum.Months:
					    return now.AddMonths(amount.Value).Subtract(now);
				    case DateTimeQueryEnum.Year:
				    case DateTimeQueryEnum.Years:
					    return now.AddYears(amount.Value).Subtract(now);
				    case DateTimeQueryEnum.Hour:
				    case DateTimeQueryEnum.Hours:
					    return new TimeSpan(0, amount.Value, 0, 0);
				    case DateTimeQueryEnum.Minute:
				    case DateTimeQueryEnum.Minutes:
					    return new TimeSpan(0, 0, amount.Value, 0);
				    case DateTimeQueryEnum.Second:
				    case DateTimeQueryEnum.Seconds:
						return new TimeSpan(0, 0, 0, amount.Value);
				    case DateTimeQueryEnum.BusinessDay:
				    case DateTimeQueryEnum.BusinessDays:
					    return now.AddDays(amount.Value).MoveToBusinessDay(factor).Subtract(now);
					case DateTimeQueryEnum.BusinessWeek:
					case DateTimeQueryEnum.BusinessWeeks:
						return now.AddDays(amount.Value * 7).MoveToBusinessDay(factor).Subtract(now);
					case DateTimeQueryEnum.BusinessMonth:
					case DateTimeQueryEnum.BusinessMonths:
						return now.AddMonths(amount.Value).MoveToBusinessDay(factor).Subtract(now);
					case DateTimeQueryEnum.BusinessYear:
					case DateTimeQueryEnum.BusinessYears:
						return now.AddYears(amount.Value).MoveToBusinessDay(factor).Subtract(now);

					default:
					    throw new ArgumentException("Unsupported DateTimeQuery");
			    }
		    }

			//else
		    return new TimeSpan(0);

		}

	}
}

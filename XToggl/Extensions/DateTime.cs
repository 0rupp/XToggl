using System;
using System.Globalization;
using Toggl.Extensions;

namespace XToggl
{
	public static class DateTimeExtensions
	{
		const string WEIRD_TOGGL_FORMAT = "MM/dd/yyyy HH:mm:ss";

		public static long ToTogglStartDuration(this DateTime dt) {
			return (long)new DateTime (1970, 1, 1, 1, 0, 0).Subtract(DateTime.Now).TotalSeconds;
		}


		public static DateTime ToTogglDateTime(this long ms) {
			return new DateTime (1970, 1, 1, 0, 0, 0,
				DateTimeKind.Utc).AddMilliseconds (ms).ToLocalTime ();
		}

		public static long DifferenceInSeconds(this DateTime begin, DateTime end) {
			return (long)end.Subtract (begin).TotalSeconds;
		}

		public static long MsSinceUpTime(this DateTime dt, long upTimeMs) {
			var upDate = DateTime.Now.AddMilliseconds (-upTimeMs);
			var difference = dt.Subtract(upDate);
			return (long)difference.TotalMilliseconds;
		}


		public static DateTime TogglDateTimeWorkAround(this string datetime) {
			if (datetime.Contains ("/"))
				return DateTime.ParseExact (datetime, WEIRD_TOGGL_FORMAT, CultureInfo.InvariantCulture);
			else
				return DateTime.Parse(datetime);
		}

		public static string TogglDateTimeWorkAroundStr(this string datetime) {
			return datetime.TogglDateTimeWorkAround().ToIsoDateStr ();
		}



	}
}


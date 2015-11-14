using System;

namespace XToggl
{
	public static class DateTimeExtensions
	{
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



	}
}


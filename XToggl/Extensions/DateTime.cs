using System;

namespace XToggl
{
	public static class DateTimeExtensions
	{
		public static long ToTogglStartDuration(this DateTime dt) {
			return (long)new DateTime (1970, 1, 1, 1, 0, 0).Subtract(DateTime.Now).TotalSeconds;
		}

		public static long DifferenceInSeconds(this DateTime begin, DateTime end) {
			return (long)end.Subtract (begin).TotalSeconds;
		}



	}
}


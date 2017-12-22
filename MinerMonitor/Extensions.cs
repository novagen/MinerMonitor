using System;

namespace Monitor
{
	public static class Extensions
	{
		public static DateTime ToDate(this long value)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(value);
		}

		public static DateTime ToDate(this int value)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(value);
		}
	}
}
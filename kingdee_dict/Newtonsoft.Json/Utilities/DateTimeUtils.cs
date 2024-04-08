using System;
using System.Globalization;
using System.Xml;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B9 RID: 185
	internal static class DateTimeUtils
	{
		// Token: 0x06000810 RID: 2064 RVA: 0x0001D520 File Offset: 0x0001B720
		public static string GetLocalOffset(this DateTime d)
		{
			TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(d);
			return utcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + utcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x0001D578 File Offset: 0x0001B778
		public static XmlDateTimeSerializationMode ToSerializationMode(DateTimeKind kind)
		{
			switch (kind)
			{
			case DateTimeKind.Unspecified:
				return XmlDateTimeSerializationMode.Unspecified;
			case DateTimeKind.Utc:
				return XmlDateTimeSerializationMode.Utc;
			case DateTimeKind.Local:
				return XmlDateTimeSerializationMode.Local;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("kind", kind, "Unexpected DateTimeKind value.");
			}
		}
	}
}

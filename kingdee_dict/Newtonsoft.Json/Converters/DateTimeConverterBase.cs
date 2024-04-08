using System;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001B RID: 27
	public abstract class DateTimeConverterBase : JsonConverter
	{
		// Token: 0x060000FB RID: 251 RVA: 0x0000541C File Offset: 0x0000361C
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime) || objectType == typeof(DateTime?) || (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?));
		}
	}
}

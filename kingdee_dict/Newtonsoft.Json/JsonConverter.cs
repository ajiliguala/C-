using System;
using Newtonsoft.Json.Schema;

namespace Newtonsoft.Json
{
	// Token: 0x02000016 RID: 22
	public abstract class JsonConverter
	{
		// Token: 0x060000E0 RID: 224
		public abstract void WriteJson(JsonWriter writer, object value, JsonSerializer serializer);

		// Token: 0x060000E1 RID: 225
		public abstract object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);

		// Token: 0x060000E2 RID: 226
		public abstract bool CanConvert(Type objectType);

		// Token: 0x060000E3 RID: 227 RVA: 0x00004E7C File Offset: 0x0000307C
		public virtual JsonSchema GetSchema()
		{
			return null;
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00004E7F File Offset: 0x0000307F
		public virtual bool CanRead
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00004E82 File Offset: 0x00003082
		public virtual bool CanWrite
		{
			get
			{
				return true;
			}
		}
	}
}

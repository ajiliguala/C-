using System;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001A RID: 26
	public abstract class CustomCreationConverter<T> : JsonConverter
	{
		// Token: 0x060000F5 RID: 245 RVA: 0x000053A7 File Offset: 0x000035A7
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotSupportedException("CustomCreationConverter should only be used while deserializing.");
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x000053B4 File Offset: 0x000035B4
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			T t = this.Create(objectType);
			if (t == null)
			{
				throw new JsonSerializationException("No object created.");
			}
			serializer.Populate(reader, t);
			return t;
		}

		// Token: 0x060000F7 RID: 247
		public abstract T Create(Type objectType);

		// Token: 0x060000F8 RID: 248 RVA: 0x000053FC File Offset: 0x000035FC
		public override bool CanConvert(Type objectType)
		{
			return typeof(T).IsAssignableFrom(objectType);
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x0000540E File Offset: 0x0000360E
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}
	}
}

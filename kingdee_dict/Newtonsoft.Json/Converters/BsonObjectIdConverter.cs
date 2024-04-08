using System;
using System.Globalization;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001F RID: 31
	public class BsonObjectIdConverter : JsonConverter
	{
		// Token: 0x0600010B RID: 267 RVA: 0x00005750 File Offset: 0x00003950
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			BsonObjectId bsonObjectId = (BsonObjectId)value;
			BsonWriter bsonWriter = writer as BsonWriter;
			if (bsonWriter != null)
			{
				bsonWriter.WriteObjectId(bsonObjectId.Value);
				return;
			}
			writer.WriteValue(bsonObjectId.Value);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00005788 File Offset: 0x00003988
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Bytes)
			{
				throw new JsonSerializationException("Expected Bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
			byte[] value = (byte[])reader.Value;
			return new BsonObjectId(value);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x000057DC File Offset: 0x000039DC
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(BsonObjectId);
		}
	}
}

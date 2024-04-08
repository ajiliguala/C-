using System;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001D RID: 29
	public class EntityKeyMemberConverter : JsonConverter
	{
		// Token: 0x06000101 RID: 257 RVA: 0x00005480 File Offset: 0x00003680
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(value);
			Type type = (entityKeyMember.Value != null) ? entityKeyMember.Value.GetType() : null;
			writer.WriteStartObject();
			writer.WritePropertyName("Key");
			writer.WriteValue(entityKeyMember.Key);
			writer.WritePropertyName("Type");
			writer.WriteValue((type != null) ? type.FullName : null);
			writer.WritePropertyName("Value");
			if (type != null)
			{
				string value2;
				if (JsonSerializerInternalWriter.TryConvertToString(entityKeyMember.Value, type, out value2))
				{
					writer.WriteValue(value2);
				}
				else
				{
					writer.WriteValue(entityKeyMember.Value);
				}
			}
			else
			{
				writer.WriteNull();
			}
			writer.WriteEndObject();
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00005534 File Offset: 0x00003734
		private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
		{
			EntityKeyMemberConverter.ReadAndAssert(reader);
			if (reader.TokenType != JsonToken.PropertyName || reader.Value.ToString() != propertyName)
			{
				throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					propertyName
				}));
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00005584 File Offset: 0x00003784
		private static void ReadAndAssert(JsonReader reader)
		{
			if (!reader.Read())
			{
				throw new JsonSerializationException("Unexpected end.");
			}
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0000559C File Offset: 0x0000379C
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(Activator.CreateInstance(objectType));
			EntityKeyMemberConverter.ReadAndAssertProperty(reader, "Key");
			EntityKeyMemberConverter.ReadAndAssert(reader);
			entityKeyMember.Key = reader.Value.ToString();
			EntityKeyMemberConverter.ReadAndAssertProperty(reader, "Type");
			EntityKeyMemberConverter.ReadAndAssert(reader);
			string typeName = reader.Value.ToString();
			Type type = Type.GetType(typeName);
			EntityKeyMemberConverter.ReadAndAssertProperty(reader, "Value");
			EntityKeyMemberConverter.ReadAndAssert(reader);
			entityKeyMember.Value = serializer.Deserialize(reader, type);
			EntityKeyMemberConverter.ReadAndAssert(reader);
			return DynamicWrapper.GetUnderlyingObject(entityKeyMember);
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00005627 File Offset: 0x00003827
		public override bool CanConvert(Type objectType)
		{
			return objectType.AssignableToTypeName("System.Data.EntityKeyMember");
		}

		// Token: 0x04000077 RID: 119
		private const string EntityKeyMemberFullTypeName = "System.Data.EntityKeyMember";
	}
}

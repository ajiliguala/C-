using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001E RID: 30
	public class KeyValuePairConverter : JsonConverter
	{
		// Token: 0x06000107 RID: 263 RVA: 0x0000563C File Offset: 0x0000383C
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Type type = value.GetType();
			PropertyInfo property = type.GetProperty("Key");
			PropertyInfo property2 = type.GetProperty("Value");
			writer.WriteStartObject();
			writer.WritePropertyName("Key");
			serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property, value));
			writer.WritePropertyName("Value");
			serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property2, value));
			writer.WriteEndObject();
		}

		// Token: 0x06000108 RID: 264 RVA: 0x000056A8 File Offset: 0x000038A8
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			IList<Type> genericArguments = objectType.GetGenericArguments();
			Type objectType2 = genericArguments[0];
			Type objectType3 = genericArguments[1];
			reader.Read();
			reader.Read();
			object obj = serializer.Deserialize(reader, objectType2);
			reader.Read();
			reader.Read();
			object obj2 = serializer.Deserialize(reader, objectType3);
			reader.Read();
			return ReflectionUtils.CreateInstance(objectType, new object[]
			{
				obj,
				obj2
			});
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000571F File Offset: 0x0000391F
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsValueType && objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<, >);
		}
	}
}

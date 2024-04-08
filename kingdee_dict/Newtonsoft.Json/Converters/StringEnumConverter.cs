using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000021 RID: 33
	public class StringEnumConverter : JsonConverter
	{
		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00005A5A File Offset: 0x00003C5A
		// (set) Token: 0x06000119 RID: 281 RVA: 0x00005A62 File Offset: 0x00003C62
		public bool CamelCaseText { get; set; }

		// Token: 0x0600011A RID: 282 RVA: 0x00005A6C File Offset: 0x00003C6C
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			Enum @enum = (Enum)value;
			string text = @enum.ToString("G");
			if (char.IsNumber(text[0]) || text[0] == '-')
			{
				writer.WriteValue(value);
				return;
			}
			if (this.CamelCaseText)
			{
				text = StringUtils.ToCamelCase(text);
			}
			writer.WriteValue(text);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005AD0 File Offset: 0x00003CD0
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type type = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
			if (reader.TokenType == JsonToken.Null)
			{
				if (!ReflectionUtils.IsNullableType(objectType))
				{
					throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						objectType
					}));
				}
				return null;
			}
			else
			{
				if (reader.TokenType == JsonToken.String)
				{
					return Enum.Parse(type, reader.Value.ToString(), true);
				}
				if (reader.TokenType == JsonToken.Integer)
				{
					return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, type);
				}
				throw new Exception("Unexpected token when parsing enum. Expected String or Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00005B88 File Offset: 0x00003D88
		public override bool CanConvert(Type objectType)
		{
			Type type = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
			return type.IsEnum;
		}
	}
}

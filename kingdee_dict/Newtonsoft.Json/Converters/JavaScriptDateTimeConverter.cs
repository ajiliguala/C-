﻿using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000047 RID: 71
	public class JavaScriptDateTimeConverter : DateTimeConverterBase
	{
		// Token: 0x060002A9 RID: 681 RVA: 0x0000A1FC File Offset: 0x000083FC
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			long value2;
			if (value is DateTime)
			{
				DateTime dateTime = ((DateTime)value).ToUniversalTime();
				value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime);
			}
			else
			{
				if (!(value is DateTimeOffset))
				{
					throw new Exception("Expected date object value.");
				}
				value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(((DateTimeOffset)value).ToUniversalTime().UtcDateTime);
			}
			writer.WriteStartConstructor("Date");
			writer.WriteValue(value2);
			writer.WriteEndConstructor();
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000A274 File Offset: 0x00008474
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type left = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
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
				if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
				{
					throw new Exception("Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType,
						reader.Value
					}));
				}
				reader.Read();
				if (reader.TokenType != JsonToken.Integer)
				{
					throw new Exception("Unexpected token parsing date. Expected Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType
					}));
				}
				long javaScriptTicks = (long)reader.Value;
				DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
				reader.Read();
				if (reader.TokenType != JsonToken.EndConstructor)
				{
					throw new Exception("Unexpected token parsing date. Expected EndConstructor, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType
					}));
				}
				if (left == typeof(DateTimeOffset))
				{
					return new DateTimeOffset(dateTime);
				}
				return dateTime;
			}
		}
	}
}

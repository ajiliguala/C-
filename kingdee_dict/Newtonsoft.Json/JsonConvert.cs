using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x02000063 RID: 99
	public static class JsonConvert
	{
		// Token: 0x060003C6 RID: 966 RVA: 0x0000DE30 File Offset: 0x0000C030
		public static string ToString(DateTime value)
		{
			string result;
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				JsonConvert.WriteDateTimeString(stringWriter, value, JsonConvert.GetUtcOffset(value), value.Kind);
				result = stringWriter.ToString();
			}
			return result;
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0000DE80 File Offset: 0x0000C080
		public static string ToString(DateTimeOffset value)
		{
			string result;
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				JsonConvert.WriteDateTimeString(stringWriter, value.UtcDateTime, value.Offset, DateTimeKind.Local);
				result = stringWriter.ToString();
			}
			return result;
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0000DED0 File Offset: 0x0000C0D0
		private static TimeSpan GetUtcOffset(DateTime dateTime)
		{
			return TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x0000DEDD File Offset: 0x0000C0DD
		internal static void WriteDateTimeString(TextWriter writer, DateTime value)
		{
			JsonConvert.WriteDateTimeString(writer, value, JsonConvert.GetUtcOffset(value), value.Kind);
		}

		// Token: 0x060003CA RID: 970 RVA: 0x0000DEF4 File Offset: 0x0000C0F4
		internal static void WriteDateTimeString(TextWriter writer, DateTime value, TimeSpan offset, DateTimeKind kind)
		{
			long value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(value, offset);
			writer.Write("\"\\/Date(");
			writer.Write(value2);
			switch (kind)
			{
			case DateTimeKind.Unspecified:
			case DateTimeKind.Local:
			{
				writer.Write((offset.Ticks >= 0L) ? "+" : "-");
				int num = Math.Abs(offset.Hours);
				if (num < 10)
				{
					writer.Write(0);
				}
				writer.Write(num);
				int num2 = Math.Abs(offset.Minutes);
				if (num2 < 10)
				{
					writer.Write(0);
				}
				writer.Write(num2);
				break;
			}
			}
			writer.Write(")\\/\"");
		}

		// Token: 0x060003CB RID: 971 RVA: 0x0000DF99 File Offset: 0x0000C199
		private static long ToUniversalTicks(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				return dateTime.Ticks;
			}
			return JsonConvert.ToUniversalTicks(dateTime, JsonConvert.GetUtcOffset(dateTime));
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0000DFBC File Offset: 0x0000C1BC
		private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				return dateTime.Ticks;
			}
			long num = dateTime.Ticks - offset.Ticks;
			if (num > 3155378975999999999L)
			{
				return 3155378975999999999L;
			}
			if (num < 0L)
			{
				return 0L;
			}
			return num;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0000E00C File Offset: 0x0000C20C
		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
		{
			long universialTicks = JsonConvert.ToUniversalTicks(dateTime, offset);
			return JsonConvert.UniversialTicksToJavaScriptTicks(universialTicks);
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0000E027 File Offset: 0x0000C227
		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
		{
			return JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime, true);
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0000E030 File Offset: 0x0000C230
		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
		{
			long universialTicks = convertToUtc ? JsonConvert.ToUniversalTicks(dateTime) : dateTime.Ticks;
			return JsonConvert.UniversialTicksToJavaScriptTicks(universialTicks);
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x0000E058 File Offset: 0x0000C258
		private static long UniversialTicksToJavaScriptTicks(long universialTicks)
		{
			return (universialTicks - JsonConvert.InitialJavaScriptDateTicks) / 10000L;
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x0000E078 File Offset: 0x0000C278
		internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
		{
			DateTime result = new DateTime(javaScriptTicks * 10000L + JsonConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
			return result;
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0000E09C File Offset: 0x0000C29C
		public static string ToString(bool value)
		{
			if (!value)
			{
				return JsonConvert.False;
			}
			return JsonConvert.True;
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x0000E0AC File Offset: 0x0000C2AC
		public static string ToString(char value)
		{
			return JsonConvert.ToString(char.ToString(value));
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x0000E0B9 File Offset: 0x0000C2B9
		public static string ToString(Enum value)
		{
			return value.ToString("D");
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0000E0C6 File Offset: 0x0000C2C6
		public static string ToString(int value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x0000E0D5 File Offset: 0x0000C2D5
		public static string ToString(short value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0000E0E4 File Offset: 0x0000C2E4
		[CLSCompliant(false)]
		public static string ToString(ushort value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0000E0F3 File Offset: 0x0000C2F3
		[CLSCompliant(false)]
		public static string ToString(uint value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0000E102 File Offset: 0x0000C302
		public static string ToString(long value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0000E111 File Offset: 0x0000C311
		[CLSCompliant(false)]
		public static string ToString(ulong value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0000E120 File Offset: 0x0000C320
		public static string ToString(float value)
		{
			return JsonConvert.EnsureDecimalPlace((double)value, value.ToString("R", CultureInfo.InvariantCulture));
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0000E13A File Offset: 0x0000C33A
		public static string ToString(double value)
		{
			return JsonConvert.EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0000E153 File Offset: 0x0000C353
		private static string EnsureDecimalPlace(double value, string text)
		{
			if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0000E188 File Offset: 0x0000C388
		private static string EnsureDecimalPlace(string text)
		{
			if (text.IndexOf('.') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		// Token: 0x060003DF RID: 991 RVA: 0x0000E1A2 File Offset: 0x0000C3A2
		public static string ToString(byte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0000E1B1 File Offset: 0x0000C3B1
		[CLSCompliant(false)]
		public static string ToString(sbyte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0000E1C0 File Offset: 0x0000C3C0
		public static string ToString(decimal value)
		{
			return JsonConvert.EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x0000E1D4 File Offset: 0x0000C3D4
		public static string ToString(Guid value)
		{
			return '"' + value.ToString("D", CultureInfo.InvariantCulture) + '"';
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0000E1FA File Offset: 0x0000C3FA
		public static string ToString(string value)
		{
			return JsonConvert.ToString(value, '"');
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0000E204 File Offset: 0x0000C404
		public static string ToString(string value, char delimter)
		{
			return JavaScriptUtils.ToEscapedJavaScriptString(value, delimter, true);
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x0000E210 File Offset: 0x0000C410
		public static string ToString(object value)
		{
			if (value == null)
			{
				return JsonConvert.Null;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				switch (convertible.GetTypeCode())
				{
				case TypeCode.DBNull:
					return JsonConvert.Null;
				case TypeCode.Boolean:
					return JsonConvert.ToString(convertible.ToBoolean(CultureInfo.InvariantCulture));
				case TypeCode.Char:
					return JsonConvert.ToString(convertible.ToChar(CultureInfo.InvariantCulture));
				case TypeCode.SByte:
					return JsonConvert.ToString(convertible.ToSByte(CultureInfo.InvariantCulture));
				case TypeCode.Byte:
					return JsonConvert.ToString(convertible.ToByte(CultureInfo.InvariantCulture));
				case TypeCode.Int16:
					return JsonConvert.ToString(convertible.ToInt16(CultureInfo.InvariantCulture));
				case TypeCode.UInt16:
					return JsonConvert.ToString(convertible.ToUInt16(CultureInfo.InvariantCulture));
				case TypeCode.Int32:
					return JsonConvert.ToString(convertible.ToInt32(CultureInfo.InvariantCulture));
				case TypeCode.UInt32:
					return JsonConvert.ToString(convertible.ToUInt32(CultureInfo.InvariantCulture));
				case TypeCode.Int64:
					return JsonConvert.ToString(convertible.ToInt64(CultureInfo.InvariantCulture));
				case TypeCode.UInt64:
					return JsonConvert.ToString(convertible.ToUInt64(CultureInfo.InvariantCulture));
				case TypeCode.Single:
					return JsonConvert.ToString(convertible.ToSingle(CultureInfo.InvariantCulture));
				case TypeCode.Double:
					return JsonConvert.ToString(convertible.ToDouble(CultureInfo.InvariantCulture));
				case TypeCode.Decimal:
					return JsonConvert.ToString(convertible.ToDecimal(CultureInfo.InvariantCulture));
				case TypeCode.DateTime:
					return JsonConvert.ToString(convertible.ToDateTime(CultureInfo.InvariantCulture));
				case TypeCode.String:
					return JsonConvert.ToString(convertible.ToString(CultureInfo.InvariantCulture));
				}
			}
			else if (value is DateTimeOffset)
			{
				return JsonConvert.ToString((DateTimeOffset)value);
			}
			throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				value.GetType()
			}));
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0000E3CC File Offset: 0x0000C5CC
		private static bool IsJsonPrimitiveTypeCode(TypeCode typeCode)
		{
			switch (typeCode)
			{
			case TypeCode.DBNull:
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
			case TypeCode.DateTime:
			case TypeCode.String:
				return true;
			}
			return false;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0000E42C File Offset: 0x0000C62C
		internal static bool IsJsonPrimitiveType(Type type)
		{
			if (ReflectionUtils.IsNullableType(type))
			{
				type = Nullable.GetUnderlyingType(type);
			}
			return type == typeof(DateTimeOffset) || type == typeof(byte[]) || JsonConvert.IsJsonPrimitiveTypeCode(Type.GetTypeCode(type));
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x0000E47C File Offset: 0x0000C67C
		internal static bool IsJsonPrimitive(object value)
		{
			if (value == null)
			{
				return true;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				return JsonConvert.IsJsonPrimitiveTypeCode(convertible.GetTypeCode());
			}
			return value is DateTimeOffset || value is byte[];
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x0000E4B9 File Offset: 0x0000C6B9
		public static string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value, Formatting.None, null);
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x0000E4C3 File Offset: 0x0000C6C3
		public static string SerializeObject(object value, Formatting formatting)
		{
			return JsonConvert.SerializeObject(value, formatting, null);
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0000E4CD File Offset: 0x0000C6CD
		public static string SerializeObject(object value, params JsonConverter[] converters)
		{
			return JsonConvert.SerializeObject(value, Formatting.None, converters);
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0000E4D8 File Offset: 0x0000C6D8
		public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
		{
			JsonSerializerSettings settings = (converters != null && converters.Length > 0) ? new JsonSerializerSettings
			{
				Converters = converters
			} : null;
			return JsonConvert.SerializeObject(value, formatting, settings);
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0000E508 File Offset: 0x0000C708
		public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			StringBuilder sb = new StringBuilder(128);
			StringWriter stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture);
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
			{
				jsonTextWriter.Formatting = formatting;
				jsonSerializer.Serialize(jsonTextWriter, value);
			}
			return stringWriter.ToString();
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x0000E56C File Offset: 0x0000C76C
		public static object DeserializeObject(string value)
		{
			return JsonConvert.DeserializeObject(value, null, null);
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0000E576 File Offset: 0x0000C776
		public static object DeserializeObject(string value, JsonSerializerSettings settings)
		{
			return JsonConvert.DeserializeObject(value, null, settings);
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x0000E580 File Offset: 0x0000C780
		public static object DeserializeObject(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type, null);
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0000E58A File Offset: 0x0000C78A
		public static T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value, null);
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x0000E593 File Offset: 0x0000C793
		public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0000E59B File Offset: 0x0000C79B
		public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
		{
			return (T)((object)JsonConvert.DeserializeObject(value, typeof(T), converters));
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x0000E5B3 File Offset: 0x0000C7B3
		public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
		{
			return (T)((object)JsonConvert.DeserializeObject(value, typeof(T), settings));
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x0000E5CC File Offset: 0x0000C7CC
		public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
		{
			JsonSerializerSettings settings = (converters != null && converters.Length > 0) ? new JsonSerializerSettings
			{
				Converters = converters
			} : null;
			return JsonConvert.DeserializeObject(value, type, settings);
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0000E5FC File Offset: 0x0000C7FC
		public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
		{
			StringReader reader = new StringReader(value);
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			object result;
			using (JsonReader jsonReader = new JsonTextReader(reader))
			{
				result = jsonSerializer.Deserialize(jsonReader, type);
				if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
				{
					throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
				}
			}
			return result;
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0000E660 File Offset: 0x0000C860
		public static void PopulateObject(string value, object target)
		{
			JsonConvert.PopulateObject(value, target, null);
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0000E66C File Offset: 0x0000C86C
		public static void PopulateObject(string value, object target, JsonSerializerSettings settings)
		{
			StringReader reader = new StringReader(value);
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			using (JsonReader jsonReader = new JsonTextReader(reader))
			{
				jsonSerializer.Populate(jsonReader, target);
				if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
				{
					throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
				}
			}
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x0000E6D0 File Offset: 0x0000C8D0
		public static string SerializeXmlNode(XmlNode node)
		{
			return JsonConvert.SerializeXmlNode(node, Formatting.None);
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x0000E6DC File Offset: 0x0000C8DC
		public static string SerializeXmlNode(XmlNode node, Formatting formatting)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			return JsonConvert.SerializeObject(node, formatting, new JsonConverter[]
			{
				xmlNodeConverter
			});
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x0000E704 File Offset: 0x0000C904
		public static string SerializeXmlNode(XmlNode node, Formatting formatting, bool omitRootObject)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
			{
				OmitRootObject = omitRootObject
			};
			return JsonConvert.SerializeObject(node, formatting, new JsonConverter[]
			{
				xmlNodeConverter
			});
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x0000E733 File Offset: 0x0000C933
		public static XmlDocument DeserializeXmlNode(string value)
		{
			return JsonConvert.DeserializeXmlNode(value, null);
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x0000E73C File Offset: 0x0000C93C
		public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
		{
			return JsonConvert.DeserializeXmlNode(value, deserializeRootElementName, false);
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x0000E748 File Offset: 0x0000C948
		public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
			xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
			return (XmlDocument)JsonConvert.DeserializeObject(value, typeof(XmlDocument), new JsonConverter[]
			{
				xmlNodeConverter
			});
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x0000E78A File Offset: 0x0000C98A
		public static string SerializeXNode(XObject node)
		{
			return JsonConvert.SerializeXNode(node, Formatting.None);
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x0000E793 File Offset: 0x0000C993
		public static string SerializeXNode(XObject node, Formatting formatting)
		{
			return JsonConvert.SerializeXNode(node, formatting, false);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0000E7A0 File Offset: 0x0000C9A0
		public static string SerializeXNode(XObject node, Formatting formatting, bool omitRootObject)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
			{
				OmitRootObject = omitRootObject
			};
			return JsonConvert.SerializeObject(node, formatting, new JsonConverter[]
			{
				xmlNodeConverter
			});
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x0000E7CF File Offset: 0x0000C9CF
		public static XDocument DeserializeXNode(string value)
		{
			return JsonConvert.DeserializeXNode(value, null);
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x0000E7D8 File Offset: 0x0000C9D8
		public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
		{
			return JsonConvert.DeserializeXNode(value, deserializeRootElementName, false);
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x0000E7E4 File Offset: 0x0000C9E4
		public static XDocument DeserializeXNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
			xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
			return (XDocument)JsonConvert.DeserializeObject(value, typeof(XDocument), new JsonConverter[]
			{
				xmlNodeConverter
			});
		}

		// Token: 0x0400011C RID: 284
		public static readonly string True = "true";

		// Token: 0x0400011D RID: 285
		public static readonly string False = "false";

		// Token: 0x0400011E RID: 286
		public static readonly string Null = "null";

		// Token: 0x0400011F RID: 287
		public static readonly string Undefined = "undefined";

		// Token: 0x04000120 RID: 288
		public static readonly string PositiveInfinity = "Infinity";

		// Token: 0x04000121 RID: 289
		public static readonly string NegativeInfinity = "-Infinity";

		// Token: 0x04000122 RID: 290
		public static readonly string NaN = "NaN";

		// Token: 0x04000123 RID: 291
		internal static readonly long InitialJavaScriptDateTicks = 621355968000000000L;
	}
}

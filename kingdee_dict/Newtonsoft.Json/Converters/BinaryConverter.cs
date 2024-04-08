using System;
using System.Data.SqlTypes;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000017 RID: 23
	public class BinaryConverter : JsonConverter
	{
		// Token: 0x060000E7 RID: 231 RVA: 0x00004E90 File Offset: 0x00003090
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			byte[] byteArray = this.GetByteArray(value);
			writer.WriteValue(byteArray);
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00004EB8 File Offset: 0x000030B8
		private byte[] GetByteArray(object value)
		{
			if (value.GetType().AssignableToTypeName("System.Data.Linq.Binary"))
			{
				IBinary binary = DynamicWrapper.CreateWrapper<IBinary>(value);
				return binary.ToArray();
			}
			if (value is SqlBinary)
			{
				return ((SqlBinary)value).Value;
			}
			throw new Exception("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				value.GetType()
			}));
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00004F24 File Offset: 0x00003124
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type type = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
			if (reader.TokenType == JsonToken.Null)
			{
				if (!ReflectionUtils.IsNullable(objectType))
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
				if (reader.TokenType != JsonToken.String)
				{
					throw new Exception("Unexpected token parsing binary. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType
					}));
				}
				string s = reader.Value.ToString();
				byte[] array = Convert.FromBase64String(s);
				if (type.AssignableToTypeName("System.Data.Linq.Binary"))
				{
					return Activator.CreateInstance(type, new object[]
					{
						array
					});
				}
				if (type == typeof(SqlBinary))
				{
					return new SqlBinary(array);
				}
				throw new Exception("Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					objectType
				}));
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00005027 File Offset: 0x00003227
		public override bool CanConvert(Type objectType)
		{
			return objectType.AssignableToTypeName("System.Data.Linq.Binary") || (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?));
		}

		// Token: 0x04000076 RID: 118
		private const string BinaryTypeName = "System.Data.Linq.Binary";
	}
}

using System;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200002F RID: 47
	internal class JsonFormatterConverter : IFormatterConverter
	{
		// Token: 0x060001F3 RID: 499 RVA: 0x00008209 File Offset: 0x00006409
		public JsonFormatterConverter(JsonSerializer serializer)
		{
			ValidationUtils.ArgumentNotNull(serializer, "serializer");
			this._serializer = serializer;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00008224 File Offset: 0x00006424
		private T GetTokenValue<T>(object value)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			JValue jvalue = (JValue)value;
			return (T)((object)System.Convert.ChangeType(jvalue.Value, typeof(T), CultureInfo.InvariantCulture));
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00008264 File Offset: 0x00006464
		public object Convert(object value, Type type)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			JToken jtoken = value as JToken;
			if (jtoken == null)
			{
				throw new ArgumentException("Value is not a JToken.", "value");
			}
			return this._serializer.Deserialize(jtoken.CreateReader(), type);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x000082A8 File Offset: 0x000064A8
		public object Convert(object value, TypeCode typeCode)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is JValue)
			{
				value = ((JValue)value).Value;
			}
			return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x000082D6 File Offset: 0x000064D6
		public bool ToBoolean(object value)
		{
			return this.GetTokenValue<bool>(value);
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x000082DF File Offset: 0x000064DF
		public byte ToByte(object value)
		{
			return this.GetTokenValue<byte>(value);
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x000082E8 File Offset: 0x000064E8
		public char ToChar(object value)
		{
			return this.GetTokenValue<char>(value);
		}

		// Token: 0x060001FA RID: 506 RVA: 0x000082F1 File Offset: 0x000064F1
		public DateTime ToDateTime(object value)
		{
			return this.GetTokenValue<DateTime>(value);
		}

		// Token: 0x060001FB RID: 507 RVA: 0x000082FA File Offset: 0x000064FA
		public decimal ToDecimal(object value)
		{
			return this.GetTokenValue<decimal>(value);
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00008303 File Offset: 0x00006503
		public double ToDouble(object value)
		{
			return this.GetTokenValue<double>(value);
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000830C File Offset: 0x0000650C
		public short ToInt16(object value)
		{
			return this.GetTokenValue<short>(value);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00008315 File Offset: 0x00006515
		public int ToInt32(object value)
		{
			return this.GetTokenValue<int>(value);
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000831E File Offset: 0x0000651E
		public long ToInt64(object value)
		{
			return this.GetTokenValue<long>(value);
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00008327 File Offset: 0x00006527
		public sbyte ToSByte(object value)
		{
			return this.GetTokenValue<sbyte>(value);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00008330 File Offset: 0x00006530
		public float ToSingle(object value)
		{
			return this.GetTokenValue<float>(value);
		}

		// Token: 0x06000202 RID: 514 RVA: 0x00008339 File Offset: 0x00006539
		public string ToString(object value)
		{
			return this.GetTokenValue<string>(value);
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00008342 File Offset: 0x00006542
		public ushort ToUInt16(object value)
		{
			return this.GetTokenValue<ushort>(value);
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000834B File Offset: 0x0000654B
		public uint ToUInt32(object value)
		{
			return this.GetTokenValue<uint>(value);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00008354 File Offset: 0x00006554
		public ulong ToUInt64(object value)
		{
			return this.GetTokenValue<ulong>(value);
		}

		// Token: 0x04000097 RID: 151
		private readonly JsonSerializer _serializer;
	}
}

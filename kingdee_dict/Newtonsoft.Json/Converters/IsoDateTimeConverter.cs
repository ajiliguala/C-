using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000046 RID: 70
	public class IsoDateTimeConverter : DateTimeConverterBase
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x060002A0 RID: 672 RVA: 0x00009F82 File Offset: 0x00008182
		// (set) Token: 0x060002A1 RID: 673 RVA: 0x00009F8A File Offset: 0x0000818A
		public DateTimeStyles DateTimeStyles
		{
			get
			{
				return this._dateTimeStyles;
			}
			set
			{
				this._dateTimeStyles = value;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x060002A2 RID: 674 RVA: 0x00009F93 File Offset: 0x00008193
		// (set) Token: 0x060002A3 RID: 675 RVA: 0x00009FA4 File Offset: 0x000081A4
		public string DateTimeFormat
		{
			get
			{
				return this._dateTimeFormat ?? string.Empty;
			}
			set
			{
				this._dateTimeFormat = StringUtils.NullEmptyString(value);
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x00009FB2 File Offset: 0x000081B2
		// (set) Token: 0x060002A5 RID: 677 RVA: 0x00009FC3 File Offset: 0x000081C3
		public CultureInfo Culture
		{
			get
			{
				return this._culture ?? CultureInfo.CurrentCulture;
			}
			set
			{
				this._culture = value;
			}
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x00009FCC File Offset: 0x000081CC
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string value2;
			if (value is DateTime)
			{
				DateTime dateTime = (DateTime)value;
				if ((this._dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (this._dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
				{
					dateTime = dateTime.ToUniversalTime();
				}
				value2 = dateTime.ToString(this._dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", this.Culture);
			}
			else
			{
				if (!(value is DateTimeOffset))
				{
					throw new Exception("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						ReflectionUtils.GetObjectType(value)
					}));
				}
				DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
				if ((this._dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (this._dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
				{
					dateTimeOffset = dateTimeOffset.ToUniversalTime();
				}
				value2 = dateTimeOffset.ToString(this._dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", this.Culture);
			}
			writer.WriteValue(value2);
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000A0A8 File Offset: 0x000082A8
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			bool flag = ReflectionUtils.IsNullableType(objectType);
			Type left = flag ? Nullable.GetUnderlyingType(objectType) : objectType;
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
				if (reader.TokenType != JsonToken.String)
				{
					throw new Exception("Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType
					}));
				}
				string text = reader.Value.ToString();
				if (string.IsNullOrEmpty(text) && flag)
				{
					return null;
				}
				if (left == typeof(DateTimeOffset))
				{
					if (!string.IsNullOrEmpty(this._dateTimeFormat))
					{
						return DateTimeOffset.ParseExact(text, this._dateTimeFormat, this.Culture, this._dateTimeStyles);
					}
					return DateTimeOffset.Parse(text, this.Culture, this._dateTimeStyles);
				}
				else
				{
					if (!string.IsNullOrEmpty(this._dateTimeFormat))
					{
						return DateTime.ParseExact(text, this._dateTimeFormat, this.Culture, this._dateTimeStyles);
					}
					return DateTime.Parse(text, this.Culture, this._dateTimeStyles);
				}
			}
		}

		// Token: 0x040000E1 RID: 225
		private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

		// Token: 0x040000E2 RID: 226
		private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

		// Token: 0x040000E3 RID: 227
		private string _dateTimeFormat;

		// Token: 0x040000E4 RID: 228
		private CultureInfo _culture;
	}
}

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000003 RID: 3
	internal class BsonBinaryWriter
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public DateTimeKind DateTimeKindHandling { get; set; }

		// Token: 0x06000003 RID: 3 RVA: 0x00002061 File Offset: 0x00000261
		public BsonBinaryWriter(Stream stream)
		{
			this.DateTimeKindHandling = DateTimeKind.Utc;
			this._writer = new BinaryWriter(stream);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000207C File Offset: 0x0000027C
		public void Flush()
		{
			this._writer.Flush();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002089 File Offset: 0x00000289
		public void WriteToken(BsonToken t)
		{
			this.CalculateSize(t);
			this.WriteTokenInternal(t);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000209C File Offset: 0x0000029C
		private void WriteTokenInternal(BsonToken t)
		{
			switch (t.Type)
			{
			case BsonType.Number:
			{
				BsonValue bsonValue = (BsonValue)t;
				this._writer.Write(Convert.ToDouble(bsonValue.Value, CultureInfo.InvariantCulture));
				return;
			}
			case BsonType.String:
			{
				BsonString bsonString = (BsonString)t;
				this.WriteString((string)bsonString.Value, bsonString.ByteCount, new int?(bsonString.CalculatedSize - 4));
				return;
			}
			case BsonType.Object:
			{
				BsonObject bsonObject = (BsonObject)t;
				this._writer.Write(bsonObject.CalculatedSize);
				foreach (BsonProperty bsonProperty in bsonObject)
				{
					this._writer.Write((sbyte)bsonProperty.Value.Type);
					this.WriteString((string)bsonProperty.Name.Value, bsonProperty.Name.ByteCount, null);
					this.WriteTokenInternal(bsonProperty.Value);
				}
				this._writer.Write(0);
				return;
			}
			case BsonType.Array:
			{
				BsonArray bsonArray = (BsonArray)t;
				this._writer.Write(bsonArray.CalculatedSize);
				int num = 0;
				foreach (BsonToken bsonToken in bsonArray)
				{
					this._writer.Write((sbyte)bsonToken.Type);
					this.WriteString(num.ToString(CultureInfo.InvariantCulture), MathUtils.IntLength(num), null);
					this.WriteTokenInternal(bsonToken);
					num++;
				}
				this._writer.Write(0);
				return;
			}
			case BsonType.Binary:
			{
				BsonValue bsonValue2 = (BsonValue)t;
				byte[] array = (byte[])bsonValue2.Value;
				this._writer.Write(array.Length);
				this._writer.Write(0);
				this._writer.Write(array);
				return;
			}
			case BsonType.Undefined:
			case BsonType.Null:
				return;
			case BsonType.Oid:
			{
				BsonValue bsonValue3 = (BsonValue)t;
				byte[] buffer = (byte[])bsonValue3.Value;
				this._writer.Write(buffer);
				return;
			}
			case BsonType.Boolean:
			{
				BsonValue bsonValue4 = (BsonValue)t;
				this._writer.Write((bool)bsonValue4.Value);
				return;
			}
			case BsonType.Date:
			{
				BsonValue bsonValue5 = (BsonValue)t;
				long value;
				if (bsonValue5.Value is DateTime)
				{
					DateTime dateTime = (DateTime)bsonValue5.Value;
					if (this.DateTimeKindHandling == DateTimeKind.Utc)
					{
						dateTime = dateTime.ToUniversalTime();
					}
					else if (this.DateTimeKindHandling == DateTimeKind.Local)
					{
						dateTime = dateTime.ToLocalTime();
					}
					value = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime, false);
				}
				else
				{
					DateTimeOffset dateTimeOffset = (DateTimeOffset)bsonValue5.Value;
					value = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.UtcDateTime, dateTimeOffset.Offset);
				}
				this._writer.Write(value);
				return;
			}
			case BsonType.Regex:
			{
				BsonRegex bsonRegex = (BsonRegex)t;
				this.WriteString((string)bsonRegex.Pattern.Value, bsonRegex.Pattern.ByteCount, null);
				this.WriteString((string)bsonRegex.Options.Value, bsonRegex.Options.ByteCount, null);
				return;
			}
			case BsonType.Integer:
			{
				BsonValue bsonValue6 = (BsonValue)t;
				this._writer.Write(Convert.ToInt32(bsonValue6.Value, CultureInfo.InvariantCulture));
				return;
			}
			case BsonType.Long:
			{
				BsonValue bsonValue7 = (BsonValue)t;
				this._writer.Write(Convert.ToInt64(bsonValue7.Value, CultureInfo.InvariantCulture));
				return;
			}
			}
			throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				t.Type
			}));
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002494 File Offset: 0x00000694
		private void WriteString(string s, int byteCount, int? calculatedlengthPrefix)
		{
			if (calculatedlengthPrefix != null)
			{
				this._writer.Write(calculatedlengthPrefix.Value);
			}
			if (s != null)
			{
				if (this._largeByteBuffer == null)
				{
					this._largeByteBuffer = new byte[256];
					this._maxChars = 256 / BsonBinaryWriter.Encoding.GetMaxByteCount(1);
				}
				if (byteCount <= 256)
				{
					BsonBinaryWriter.Encoding.GetBytes(s, 0, s.Length, this._largeByteBuffer, 0);
					this._writer.Write(this._largeByteBuffer, 0, byteCount);
				}
				else
				{
					int num = 0;
					int num2;
					for (int i = s.Length; i > 0; i -= num2)
					{
						num2 = ((i > this._maxChars) ? this._maxChars : i);
						int bytes = BsonBinaryWriter.Encoding.GetBytes(s, num, num2, this._largeByteBuffer, 0);
						this._writer.Write(this._largeByteBuffer, 0, bytes);
						num += num2;
					}
				}
			}
			this._writer.Write(0);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002586 File Offset: 0x00000786
		private int CalculateSize(int stringByteCount)
		{
			return stringByteCount + 1;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000258C File Offset: 0x0000078C
		private int CalculateSizeWithLength(int stringByteCount, bool includeSize)
		{
			int num = includeSize ? 5 : 1;
			return num + stringByteCount;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000025A4 File Offset: 0x000007A4
		private int CalculateSize(BsonToken t)
		{
			switch (t.Type)
			{
			case BsonType.Number:
				return 8;
			case BsonType.String:
			{
				BsonString bsonString = (BsonString)t;
				string text = (string)bsonString.Value;
				bsonString.ByteCount = ((text != null) ? BsonBinaryWriter.Encoding.GetByteCount(text) : 0);
				bsonString.CalculatedSize = this.CalculateSizeWithLength(bsonString.ByteCount, bsonString.IncludeLength);
				return bsonString.CalculatedSize;
			}
			case BsonType.Object:
			{
				BsonObject bsonObject = (BsonObject)t;
				int num = 4;
				foreach (BsonProperty bsonProperty in bsonObject)
				{
					int num2 = 1;
					num2 += this.CalculateSize(bsonProperty.Name);
					num2 += this.CalculateSize(bsonProperty.Value);
					num += num2;
				}
				num++;
				bsonObject.CalculatedSize = num;
				return num;
			}
			case BsonType.Array:
			{
				BsonArray bsonArray = (BsonArray)t;
				int num3 = 4;
				int num4 = 0;
				foreach (BsonToken t2 in bsonArray)
				{
					num3++;
					num3 += this.CalculateSize(MathUtils.IntLength(num4));
					num3 += this.CalculateSize(t2);
					num4++;
				}
				num3++;
				bsonArray.CalculatedSize = num3;
				return bsonArray.CalculatedSize;
			}
			case BsonType.Binary:
			{
				BsonValue bsonValue = (BsonValue)t;
				byte[] array = (byte[])bsonValue.Value;
				bsonValue.CalculatedSize = 5 + array.Length;
				return bsonValue.CalculatedSize;
			}
			case BsonType.Undefined:
			case BsonType.Null:
				return 0;
			case BsonType.Oid:
				return 12;
			case BsonType.Boolean:
				return 1;
			case BsonType.Date:
				return 8;
			case BsonType.Regex:
			{
				BsonRegex bsonRegex = (BsonRegex)t;
				int num5 = 0;
				num5 += this.CalculateSize(bsonRegex.Pattern);
				num5 += this.CalculateSize(bsonRegex.Options);
				bsonRegex.CalculatedSize = num5;
				return bsonRegex.CalculatedSize;
			}
			case BsonType.Integer:
				return 4;
			case BsonType.Long:
				return 8;
			}
			throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				t.Type
			}));
		}

		// Token: 0x04000008 RID: 8
		private static readonly Encoding Encoding = Encoding.UTF8;

		// Token: 0x04000009 RID: 9
		private readonly BinaryWriter _writer;

		// Token: 0x0400000A RID: 10
		private byte[] _largeByteBuffer;

		// Token: 0x0400000B RID: 11
		private int _maxChars;
	}
}

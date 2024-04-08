using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000006 RID: 6
	public class BsonReader : JsonReader
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000026 RID: 38 RVA: 0x00002C80 File Offset: 0x00000E80
		// (set) Token: 0x06000027 RID: 39 RVA: 0x00002C88 File Offset: 0x00000E88
		public bool JsonNet35BinaryCompatibility
		{
			get
			{
				return this._jsonNet35BinaryCompatibility;
			}
			set
			{
				this._jsonNet35BinaryCompatibility = value;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00002C91 File Offset: 0x00000E91
		// (set) Token: 0x06000029 RID: 41 RVA: 0x00002C99 File Offset: 0x00000E99
		public bool ReadRootValueAsArray
		{
			get
			{
				return this._readRootValueAsArray;
			}
			set
			{
				this._readRootValueAsArray = value;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002A RID: 42 RVA: 0x00002CA2 File Offset: 0x00000EA2
		// (set) Token: 0x0600002B RID: 43 RVA: 0x00002CAA File Offset: 0x00000EAA
		public DateTimeKind DateTimeKindHandling
		{
			get
			{
				return this._dateTimeKindHandling;
			}
			set
			{
				this._dateTimeKindHandling = value;
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002CB3 File Offset: 0x00000EB3
		public BsonReader(Stream stream) : this(stream, false, DateTimeKind.Local)
		{
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002CBE File Offset: 0x00000EBE
		public BsonReader(Stream stream, bool readRootValueAsArray, DateTimeKind dateTimeKindHandling)
		{
			ValidationUtils.ArgumentNotNull(stream, "stream");
			this._reader = new BinaryReader(stream);
			this._stack = new List<BsonReader.ContainerContext>();
			this._readRootValueAsArray = readRootValueAsArray;
			this._dateTimeKindHandling = dateTimeKindHandling;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002CF8 File Offset: 0x00000EF8
		private string ReadElement()
		{
			this._currentElementType = this.ReadType();
			return this.ReadString();
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002D1C File Offset: 0x00000F1C
		public override byte[] ReadAsBytes()
		{
			this.Read();
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Bytes)
			{
				return (byte[])this.Value;
			}
			throw new JsonReaderException("Error reading bytes. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				this.TokenType
			}));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002D7C File Offset: 0x00000F7C
		public override decimal? ReadAsDecimal()
		{
			this.Read();
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Integer || this.TokenType == JsonToken.Float)
			{
				this.SetToken(JsonToken.Float, Convert.ToDecimal(this.Value, CultureInfo.InvariantCulture));
				return new decimal?((decimal)this.Value);
			}
			throw new JsonReaderException("Error reading decimal. Expected a number but got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				this.TokenType
			}));
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002E10 File Offset: 0x00001010
		public override DateTimeOffset? ReadAsDateTimeOffset()
		{
			this.Read();
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Date)
			{
				this.SetToken(JsonToken.Date, new DateTimeOffset((DateTime)this.Value));
				return new DateTimeOffset?((DateTimeOffset)this.Value);
			}
			throw new JsonReaderException("Error reading date. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				this.TokenType
			}));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002E9C File Offset: 0x0000109C
		public override bool Read()
		{
			bool result;
			try
			{
				switch (this._bsonReaderState)
				{
				case BsonReader.BsonReaderState.Normal:
					result = this.ReadNormal();
					break;
				case BsonReader.BsonReaderState.ReferenceStart:
				case BsonReader.BsonReaderState.ReferenceRef:
				case BsonReader.BsonReaderState.ReferenceId:
					result = this.ReadReference();
					break;
				case BsonReader.BsonReaderState.CodeWScopeStart:
				case BsonReader.BsonReaderState.CodeWScopeCode:
				case BsonReader.BsonReaderState.CodeWScopeScope:
				case BsonReader.BsonReaderState.CodeWScopeScopeObject:
				case BsonReader.BsonReaderState.CodeWScopeScopeEnd:
					result = this.ReadCodeWScope();
					break;
				default:
					throw new JsonReaderException("Unexpected state: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						this._bsonReaderState
					}));
				}
			}
			catch (EndOfStreamException)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002F38 File Offset: 0x00001138
		private bool ReadCodeWScope()
		{
			switch (this._bsonReaderState)
			{
			case BsonReader.BsonReaderState.CodeWScopeStart:
				this.SetToken(JsonToken.PropertyName, "$code");
				this._bsonReaderState = BsonReader.BsonReaderState.CodeWScopeCode;
				return true;
			case BsonReader.BsonReaderState.CodeWScopeCode:
				this.ReadInt32();
				this.SetToken(JsonToken.String, this.ReadLengthString());
				this._bsonReaderState = BsonReader.BsonReaderState.CodeWScopeScope;
				return true;
			case BsonReader.BsonReaderState.CodeWScopeScope:
			{
				if (base.CurrentState == JsonReader.State.PostValue)
				{
					this.SetToken(JsonToken.PropertyName, "$scope");
					return true;
				}
				base.SetToken(JsonToken.StartObject);
				this._bsonReaderState = BsonReader.BsonReaderState.CodeWScopeScopeObject;
				BsonReader.ContainerContext containerContext = new BsonReader.ContainerContext(BsonType.Object);
				this.PushContext(containerContext);
				containerContext.Length = this.ReadInt32();
				return true;
			}
			case BsonReader.BsonReaderState.CodeWScopeScopeObject:
			{
				bool flag = this.ReadNormal();
				if (flag && this.TokenType == JsonToken.EndObject)
				{
					this._bsonReaderState = BsonReader.BsonReaderState.CodeWScopeScopeEnd;
				}
				return flag;
			}
			case BsonReader.BsonReaderState.CodeWScopeScopeEnd:
				base.SetToken(JsonToken.EndObject);
				this._bsonReaderState = BsonReader.BsonReaderState.Normal;
				return true;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003014 File Offset: 0x00001214
		private bool ReadReference()
		{
			JsonReader.State currentState = base.CurrentState;
			switch (currentState)
			{
			case JsonReader.State.Property:
				if (this._bsonReaderState == BsonReader.BsonReaderState.ReferenceRef)
				{
					this.SetToken(JsonToken.String, this.ReadLengthString());
					return true;
				}
				if (this._bsonReaderState == BsonReader.BsonReaderState.ReferenceId)
				{
					this.SetToken(JsonToken.Bytes, this.ReadBytes(12));
					return true;
				}
				throw new JsonReaderException("Unexpected state when reading BSON reference: " + this._bsonReaderState);
			case JsonReader.State.ObjectStart:
				this.SetToken(JsonToken.PropertyName, "$ref");
				this._bsonReaderState = BsonReader.BsonReaderState.ReferenceRef;
				return true;
			default:
				if (currentState != JsonReader.State.PostValue)
				{
					throw new JsonReaderException("Unexpected state when reading BSON reference: " + base.CurrentState);
				}
				if (this._bsonReaderState == BsonReader.BsonReaderState.ReferenceRef)
				{
					this.SetToken(JsonToken.PropertyName, "$id");
					this._bsonReaderState = BsonReader.BsonReaderState.ReferenceId;
					return true;
				}
				if (this._bsonReaderState == BsonReader.BsonReaderState.ReferenceId)
				{
					base.SetToken(JsonToken.EndObject);
					this._bsonReaderState = BsonReader.BsonReaderState.Normal;
					return true;
				}
				throw new JsonReaderException("Unexpected state when reading BSON reference: " + this._bsonReaderState);
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003114 File Offset: 0x00001314
		private bool ReadNormal()
		{
			switch (base.CurrentState)
			{
			case JsonReader.State.Start:
			{
				JsonToken token = (!this._readRootValueAsArray) ? JsonToken.StartObject : JsonToken.StartArray;
				BsonType type = (!this._readRootValueAsArray) ? BsonType.Object : BsonType.Array;
				base.SetToken(token);
				BsonReader.ContainerContext containerContext = new BsonReader.ContainerContext(type);
				this.PushContext(containerContext);
				containerContext.Length = this.ReadInt32();
				return true;
			}
			case JsonReader.State.Complete:
			case JsonReader.State.Closed:
				return false;
			case JsonReader.State.Property:
				this.ReadType(this._currentElementType);
				return true;
			case JsonReader.State.ObjectStart:
			case JsonReader.State.ArrayStart:
			case JsonReader.State.PostValue:
			{
				BsonReader.ContainerContext currentContext = this._currentContext;
				if (currentContext == null)
				{
					return false;
				}
				int num = currentContext.Length - 1;
				if (currentContext.Position < num)
				{
					if (currentContext.Type == BsonType.Array)
					{
						this.ReadElement();
						this.ReadType(this._currentElementType);
						return true;
					}
					this.SetToken(JsonToken.PropertyName, this.ReadElement());
					return true;
				}
				else
				{
					if (currentContext.Position != num)
					{
						throw new JsonReaderException("Read past end of current container context.");
					}
					if (this.ReadByte() != 0)
					{
						throw new JsonReaderException("Unexpected end of object byte value.");
					}
					this.PopContext();
					if (this._currentContext != null)
					{
						this.MovePosition(currentContext.Length);
					}
					JsonToken token2 = (currentContext.Type == BsonType.Object) ? JsonToken.EndObject : JsonToken.EndArray;
					base.SetToken(token2);
					return true;
				}
				break;
			}
			case JsonReader.State.ConstructorStart:
			case JsonReader.State.Constructor:
			case JsonReader.State.Error:
			case JsonReader.State.Finished:
				return false;
			}
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003268 File Offset: 0x00001468
		private void PopContext()
		{
			this._stack.RemoveAt(this._stack.Count - 1);
			if (this._stack.Count == 0)
			{
				this._currentContext = null;
				return;
			}
			this._currentContext = this._stack[this._stack.Count - 1];
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000032C0 File Offset: 0x000014C0
		private void PushContext(BsonReader.ContainerContext newContext)
		{
			this._stack.Add(newContext);
			this._currentContext = newContext;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000032D5 File Offset: 0x000014D5
		private byte ReadByte()
		{
			this.MovePosition(1);
			return this._reader.ReadByte();
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000032EC File Offset: 0x000014EC
		private void ReadType(BsonType type)
		{
			switch (type)
			{
			case BsonType.Number:
				this.SetToken(JsonToken.Float, this.ReadDouble());
				return;
			case BsonType.String:
			case BsonType.Symbol:
				this.SetToken(JsonToken.String, this.ReadLengthString());
				return;
			case BsonType.Object:
			{
				base.SetToken(JsonToken.StartObject);
				BsonReader.ContainerContext containerContext = new BsonReader.ContainerContext(BsonType.Object);
				this.PushContext(containerContext);
				containerContext.Length = this.ReadInt32();
				return;
			}
			case BsonType.Array:
			{
				base.SetToken(JsonToken.StartArray);
				BsonReader.ContainerContext containerContext2 = new BsonReader.ContainerContext(BsonType.Array);
				this.PushContext(containerContext2);
				containerContext2.Length = this.ReadInt32();
				return;
			}
			case BsonType.Binary:
				this.SetToken(JsonToken.Bytes, this.ReadBinary());
				return;
			case BsonType.Undefined:
				base.SetToken(JsonToken.Undefined);
				return;
			case BsonType.Oid:
			{
				byte[] value = this.ReadBytes(12);
				this.SetToken(JsonToken.Bytes, value);
				return;
			}
			case BsonType.Boolean:
			{
				bool flag = Convert.ToBoolean(this.ReadByte());
				this.SetToken(JsonToken.Boolean, flag);
				return;
			}
			case BsonType.Date:
			{
				long javaScriptTicks = this.ReadInt64();
				DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
				DateTime dateTime2;
				switch (this.DateTimeKindHandling)
				{
				case DateTimeKind.Unspecified:
					dateTime2 = DateTime.SpecifyKind(dateTime.ToLocalTime(), DateTimeKind.Unspecified);
					goto IL_153;
				case DateTimeKind.Local:
					dateTime2 = dateTime.ToLocalTime();
					goto IL_153;
				}
				dateTime2 = dateTime;
				IL_153:
				this.SetToken(JsonToken.Date, dateTime2);
				return;
			}
			case BsonType.Null:
				base.SetToken(JsonToken.Null);
				return;
			case BsonType.Regex:
			{
				string str = this.ReadString();
				string str2 = this.ReadString();
				string value2 = "/" + str + "/" + str2;
				this.SetToken(JsonToken.String, value2);
				return;
			}
			case BsonType.Reference:
				base.SetToken(JsonToken.StartObject);
				this._bsonReaderState = BsonReader.BsonReaderState.ReferenceStart;
				return;
			case BsonType.Code:
				this.SetToken(JsonToken.String, this.ReadLengthString());
				return;
			case BsonType.CodeWScope:
				base.SetToken(JsonToken.StartObject);
				this._bsonReaderState = BsonReader.BsonReaderState.CodeWScopeStart;
				return;
			case BsonType.Integer:
				this.SetToken(JsonToken.Integer, (long)this.ReadInt32());
				return;
			case BsonType.TimeStamp:
			case BsonType.Long:
				this.SetToken(JsonToken.Integer, this.ReadInt64());
				return;
			default:
				throw new ArgumentOutOfRangeException("type", "Unexpected BsonType value: " + type);
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003504 File Offset: 0x00001704
		private byte[] ReadBinary()
		{
			int count = this.ReadInt32();
			BsonBinaryType bsonBinaryType = (BsonBinaryType)this.ReadByte();
			if (bsonBinaryType == BsonBinaryType.Data && !this._jsonNet35BinaryCompatibility)
			{
				count = this.ReadInt32();
			}
			return this.ReadBytes(count);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000353C File Offset: 0x0000173C
		private string ReadString()
		{
			this.EnsureBuffers();
			StringBuilder stringBuilder = null;
			int num = 0;
			int num2 = 0;
			int num4;
			for (;;)
			{
				int num3 = num2;
				byte b;
				while (num3 < 128 && (b = this._reader.ReadByte()) > 0)
				{
					this._byteBuffer[num3++] = b;
				}
				num4 = num3 - num2;
				num += num4;
				if (num3 < 128 && stringBuilder == null)
				{
					break;
				}
				int lastFullCharStop = this.GetLastFullCharStop(num3 - 1);
				int chars = Encoding.UTF8.GetChars(this._byteBuffer, 0, lastFullCharStop + 1, this._charBuffer, 0);
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(256);
				}
				stringBuilder.Append(this._charBuffer, 0, chars);
				if (lastFullCharStop < num4 - 1)
				{
					num2 = num4 - lastFullCharStop - 1;
					Array.Copy(this._byteBuffer, lastFullCharStop + 1, this._byteBuffer, 0, num2);
				}
				else
				{
					if (num3 < 128)
					{
						goto Block_6;
					}
					num2 = 0;
				}
			}
			int chars2 = Encoding.UTF8.GetChars(this._byteBuffer, 0, num4, this._charBuffer, 0);
			this.MovePosition(num + 1);
			return new string(this._charBuffer, 0, chars2);
			Block_6:
			this.MovePosition(num + 1);
			return stringBuilder.ToString();
		}

		// Token: 0x0600003C RID: 60 RVA: 0x0000365C File Offset: 0x0000185C
		private string ReadLengthString()
		{
			int num = this.ReadInt32();
			this.MovePosition(num);
			string @string = this.GetString(num - 1);
			this._reader.ReadByte();
			return @string;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003690 File Offset: 0x00001890
		private string GetString(int length)
		{
			if (length == 0)
			{
				return string.Empty;
			}
			this.EnsureBuffers();
			StringBuilder stringBuilder = null;
			int num = 0;
			int num2 = 0;
			int num3;
			for (;;)
			{
				int count = (length - num > 128 - num2) ? (128 - num2) : (length - num);
				num3 = this._reader.BaseStream.Read(this._byteBuffer, num2, count);
				if (num3 == 0)
				{
					break;
				}
				num += num3;
				num3 += num2;
				if (num3 == length)
				{
					goto Block_4;
				}
				int lastFullCharStop = this.GetLastFullCharStop(num3 - 1);
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(length);
				}
				int chars = Encoding.UTF8.GetChars(this._byteBuffer, 0, lastFullCharStop + 1, this._charBuffer, 0);
				stringBuilder.Append(this._charBuffer, 0, chars);
				if (lastFullCharStop < num3 - 1)
				{
					num2 = num3 - lastFullCharStop - 1;
					Array.Copy(this._byteBuffer, lastFullCharStop + 1, this._byteBuffer, 0, num2);
				}
				else
				{
					num2 = 0;
				}
				if (num >= length)
				{
					goto Block_7;
				}
			}
			throw new EndOfStreamException("Unable to read beyond the end of the stream.");
			Block_4:
			int chars2 = Encoding.UTF8.GetChars(this._byteBuffer, 0, num3, this._charBuffer, 0);
			return new string(this._charBuffer, 0, chars2);
			Block_7:
			return stringBuilder.ToString();
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000037AC File Offset: 0x000019AC
		private int GetLastFullCharStop(int start)
		{
			int i = start;
			int num = 0;
			while (i >= 0)
			{
				num = this.BytesInSequence(this._byteBuffer[i]);
				if (num == 0)
				{
					i--;
				}
				else
				{
					if (num != 1)
					{
						i--;
						break;
					}
					break;
				}
			}
			if (num == start - i)
			{
				return start;
			}
			return i;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000037F0 File Offset: 0x000019F0
		private int BytesInSequence(byte b)
		{
			if (b <= BsonReader._seqRange1[1])
			{
				return 1;
			}
			if (b >= BsonReader._seqRange2[0] && b <= BsonReader._seqRange2[1])
			{
				return 2;
			}
			if (b >= BsonReader._seqRange3[0] && b <= BsonReader._seqRange3[1])
			{
				return 3;
			}
			if (b >= BsonReader._seqRange4[0] && b <= BsonReader._seqRange4[1])
			{
				return 4;
			}
			return 0;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x0000384C File Offset: 0x00001A4C
		private void EnsureBuffers()
		{
			if (this._byteBuffer == null)
			{
				this._byteBuffer = new byte[128];
			}
			if (this._charBuffer == null)
			{
				int maxCharCount = Encoding.UTF8.GetMaxCharCount(128);
				this._charBuffer = new char[maxCharCount];
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003895 File Offset: 0x00001A95
		private double ReadDouble()
		{
			this.MovePosition(8);
			return this._reader.ReadDouble();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000038A9 File Offset: 0x00001AA9
		private int ReadInt32()
		{
			this.MovePosition(4);
			return this._reader.ReadInt32();
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000038BD File Offset: 0x00001ABD
		private long ReadInt64()
		{
			this.MovePosition(8);
			return this._reader.ReadInt64();
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000038D1 File Offset: 0x00001AD1
		private BsonType ReadType()
		{
			this.MovePosition(1);
			return (BsonType)this._reader.ReadSByte();
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000038E5 File Offset: 0x00001AE5
		private void MovePosition(int count)
		{
			this._currentContext.Position += count;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000038FA File Offset: 0x00001AFA
		private byte[] ReadBytes(int count)
		{
			this.MovePosition(count);
			return this._reader.ReadBytes(count);
		}

		// Token: 0x04000023 RID: 35
		private const int MaxCharBytesSize = 128;

		// Token: 0x04000024 RID: 36
		private static readonly byte[] _seqRange1 = new byte[]
		{
			0,
			127
		};

		// Token: 0x04000025 RID: 37
		private static readonly byte[] _seqRange2 = new byte[]
		{
			194,
			223
		};

		// Token: 0x04000026 RID: 38
		private static readonly byte[] _seqRange3 = new byte[]
		{
			224,
			239
		};

		// Token: 0x04000027 RID: 39
		private static readonly byte[] _seqRange4 = new byte[]
		{
			240,
			244
		};

		// Token: 0x04000028 RID: 40
		private readonly BinaryReader _reader;

		// Token: 0x04000029 RID: 41
		private readonly List<BsonReader.ContainerContext> _stack;

		// Token: 0x0400002A RID: 42
		private byte[] _byteBuffer;

		// Token: 0x0400002B RID: 43
		private char[] _charBuffer;

		// Token: 0x0400002C RID: 44
		private BsonType _currentElementType;

		// Token: 0x0400002D RID: 45
		private BsonReader.BsonReaderState _bsonReaderState;

		// Token: 0x0400002E RID: 46
		private BsonReader.ContainerContext _currentContext;

		// Token: 0x0400002F RID: 47
		private bool _readRootValueAsArray;

		// Token: 0x04000030 RID: 48
		private bool _jsonNet35BinaryCompatibility;

		// Token: 0x04000031 RID: 49
		private DateTimeKind _dateTimeKindHandling;

		// Token: 0x02000007 RID: 7
		private enum BsonReaderState
		{
			// Token: 0x04000033 RID: 51
			Normal,
			// Token: 0x04000034 RID: 52
			ReferenceStart,
			// Token: 0x04000035 RID: 53
			ReferenceRef,
			// Token: 0x04000036 RID: 54
			ReferenceId,
			// Token: 0x04000037 RID: 55
			CodeWScopeStart,
			// Token: 0x04000038 RID: 56
			CodeWScopeCode,
			// Token: 0x04000039 RID: 57
			CodeWScopeScope,
			// Token: 0x0400003A RID: 58
			CodeWScopeScopeObject,
			// Token: 0x0400003B RID: 59
			CodeWScopeScopeEnd
		}

		// Token: 0x02000008 RID: 8
		private class ContainerContext
		{
			// Token: 0x06000048 RID: 72 RVA: 0x00003986 File Offset: 0x00001B86
			public ContainerContext(BsonType type)
			{
				this.Type = type;
			}

			// Token: 0x0400003C RID: 60
			public readonly BsonType Type;

			// Token: 0x0400003D RID: 61
			public int Length;

			// Token: 0x0400003E RID: 62
			public int Position;
		}
	}
}

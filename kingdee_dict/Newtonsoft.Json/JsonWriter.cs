using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x02000011 RID: 17
	public abstract class JsonWriter : IDisposable
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600006C RID: 108 RVA: 0x00003B3A File Offset: 0x00001D3A
		protected internal int Top
		{
			get
			{
				return this._top;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00003B44 File Offset: 0x00001D44
		public WriteState WriteState
		{
			get
			{
				switch (this._currentState)
				{
				case JsonWriter.State.Start:
					return WriteState.Start;
				case JsonWriter.State.Property:
					return WriteState.Property;
				case JsonWriter.State.ObjectStart:
				case JsonWriter.State.Object:
					return WriteState.Object;
				case JsonWriter.State.ArrayStart:
				case JsonWriter.State.Array:
					return WriteState.Array;
				case JsonWriter.State.ConstructorStart:
				case JsonWriter.State.Constructor:
					return WriteState.Constructor;
				case JsonWriter.State.Closed:
					return WriteState.Closed;
				case JsonWriter.State.Error:
					return WriteState.Error;
				}
				throw new JsonWriterException("Invalid state: " + this._currentState);
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600006E RID: 110 RVA: 0x00003BB4 File Offset: 0x00001DB4
		// (set) Token: 0x0600006F RID: 111 RVA: 0x00003BBC File Offset: 0x00001DBC
		public Formatting Formatting
		{
			get
			{
				return this._formatting;
			}
			set
			{
				this._formatting = value;
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003BC5 File Offset: 0x00001DC5
		public JsonWriter()
		{
			this._stack = new List<JTokenType>(8);
			this._stack.Add(JTokenType.None);
			this._currentState = JsonWriter.State.Start;
			this._formatting = Formatting.None;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00003BF4 File Offset: 0x00001DF4
		private void Push(JTokenType value)
		{
			this._top++;
			if (this._stack.Count <= this._top)
			{
				this._stack.Add(value);
				return;
			}
			this._stack[this._top] = value;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003C44 File Offset: 0x00001E44
		private JTokenType Pop()
		{
			JTokenType result = this.Peek();
			this._top--;
			return result;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00003C67 File Offset: 0x00001E67
		private JTokenType Peek()
		{
			return this._stack[this._top];
		}

		// Token: 0x06000074 RID: 116
		public abstract void Flush();

		// Token: 0x06000075 RID: 117 RVA: 0x00003C7A File Offset: 0x00001E7A
		public virtual void Close()
		{
			this.AutoCompleteAll();
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00003C82 File Offset: 0x00001E82
		public virtual void WriteStartObject()
		{
			this.AutoComplete(JsonToken.StartObject);
			this.Push(JTokenType.Object);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003C92 File Offset: 0x00001E92
		public void WriteEndObject()
		{
			this.AutoCompleteClose(JsonToken.EndObject);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00003C9C File Offset: 0x00001E9C
		public virtual void WriteStartArray()
		{
			this.AutoComplete(JsonToken.StartArray);
			this.Push(JTokenType.Array);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00003CAC File Offset: 0x00001EAC
		public void WriteEndArray()
		{
			this.AutoCompleteClose(JsonToken.EndArray);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00003CB6 File Offset: 0x00001EB6
		public virtual void WriteStartConstructor(string name)
		{
			this.AutoComplete(JsonToken.StartConstructor);
			this.Push(JTokenType.Constructor);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003CC6 File Offset: 0x00001EC6
		public void WriteEndConstructor()
		{
			this.AutoCompleteClose(JsonToken.EndConstructor);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00003CD0 File Offset: 0x00001ED0
		public virtual void WritePropertyName(string name)
		{
			this.AutoComplete(JsonToken.PropertyName);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00003CD9 File Offset: 0x00001ED9
		public void WriteEnd()
		{
			this.WriteEnd(this.Peek());
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003CE8 File Offset: 0x00001EE8
		public void WriteToken(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			int initialDepth;
			if (reader.TokenType == JsonToken.None)
			{
				initialDepth = -1;
			}
			else if (!this.IsStartToken(reader.TokenType))
			{
				initialDepth = reader.Depth + 1;
			}
			else
			{
				initialDepth = reader.Depth;
			}
			this.WriteToken(reader, initialDepth);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003D34 File Offset: 0x00001F34
		internal void WriteToken(JsonReader reader, int initialDepth)
		{
			for (;;)
			{
				switch (reader.TokenType)
				{
				case JsonToken.None:
					goto IL_1AB;
				case JsonToken.StartObject:
					this.WriteStartObject();
					goto IL_1AB;
				case JsonToken.StartArray:
					this.WriteStartArray();
					goto IL_1AB;
				case JsonToken.StartConstructor:
				{
					string strA = reader.Value.ToString();
					if (string.Compare(strA, "Date", StringComparison.Ordinal) == 0)
					{
						this.WriteConstructorDate(reader);
						goto IL_1AB;
					}
					this.WriteStartConstructor(reader.Value.ToString());
					goto IL_1AB;
				}
				case JsonToken.PropertyName:
					this.WritePropertyName(reader.Value.ToString());
					goto IL_1AB;
				case JsonToken.Comment:
					this.WriteComment(reader.Value.ToString());
					goto IL_1AB;
				case JsonToken.Raw:
					this.WriteRawValue((string)reader.Value);
					goto IL_1AB;
				case JsonToken.Integer:
					this.WriteValue(MyConvert.ToLong(reader.Value, null));
					goto IL_1AB;
				case JsonToken.Float:
					this.WriteValue(MyConvert.ToDouble(reader.Value, null));
					goto IL_1AB;
				case JsonToken.String:
					this.WriteValue(reader.Value.ToString());
					goto IL_1AB;
				case JsonToken.Boolean:
					this.WriteValue((bool)reader.Value);
					goto IL_1AB;
				case JsonToken.Null:
					this.WriteNull();
					goto IL_1AB;
				case JsonToken.Undefined:
					this.WriteUndefined();
					goto IL_1AB;
				case JsonToken.EndObject:
					this.WriteEndObject();
					goto IL_1AB;
				case JsonToken.EndArray:
					this.WriteEndArray();
					goto IL_1AB;
				case JsonToken.EndConstructor:
					this.WriteEndConstructor();
					goto IL_1AB;
				case JsonToken.Date:
					this.WriteValue((DateTime)reader.Value);
					goto IL_1AB;
				case JsonToken.Bytes:
					this.WriteValue((byte[])reader.Value);
					goto IL_1AB;
				}
				break;
				IL_1AB:
				if (initialDepth - 1 >= reader.Depth - (this.IsEndToken(reader.TokenType) ? 1 : 0) || !reader.Read())
				{
					return;
				}
			}
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", reader.TokenType, "Unexpected token type.");
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00003F18 File Offset: 0x00002118
		private void WriteConstructorDate(JsonReader reader)
		{
			if (!reader.Read())
			{
				throw new Exception("Unexpected end while reading date constructor.");
			}
			if (reader.TokenType != JsonToken.Integer)
			{
				throw new Exception("Unexpected token while reading date constructor. Expected Integer, got " + reader.TokenType);
			}
			long javaScriptTicks = (long)reader.Value;
			DateTime value = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
			if (!reader.Read())
			{
				throw new Exception("Unexpected end while reading date constructor.");
			}
			if (reader.TokenType != JsonToken.EndConstructor)
			{
				throw new Exception("Unexpected token while reading date constructor. Expected EndConstructor, got " + reader.TokenType);
			}
			this.WriteValue(value);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003FB0 File Offset: 0x000021B0
		private bool IsEndToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
			case JsonToken.EndArray:
			case JsonToken.EndConstructor:
				return true;
			default:
				return false;
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00003FDC File Offset: 0x000021DC
		private bool IsStartToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.StartObject:
			case JsonToken.StartArray:
			case JsonToken.StartConstructor:
				return true;
			default:
				return false;
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00004004 File Offset: 0x00002204
		private void WriteEnd(JTokenType type)
		{
			switch (type)
			{
			case JTokenType.Object:
				this.WriteEndObject();
				return;
			case JTokenType.Array:
				this.WriteEndArray();
				return;
			case JTokenType.Constructor:
				this.WriteEndConstructor();
				return;
			default:
				throw new JsonWriterException("Unexpected type when writing end: " + type);
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00004053 File Offset: 0x00002253
		private void AutoCompleteAll()
		{
			while (this._top > 0)
			{
				this.WriteEnd();
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00004068 File Offset: 0x00002268
		private JTokenType GetTypeForCloseToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
				return JTokenType.Object;
			case JsonToken.EndArray:
				return JTokenType.Array;
			case JsonToken.EndConstructor:
				return JTokenType.Constructor;
			default:
				throw new JsonWriterException("No type for token: " + token);
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000040AC File Offset: 0x000022AC
		private JsonToken GetCloseTokenForType(JTokenType type)
		{
			switch (type)
			{
			case JTokenType.Object:
				return JsonToken.EndObject;
			case JTokenType.Array:
				return JsonToken.EndArray;
			case JTokenType.Constructor:
				return JsonToken.EndConstructor;
			default:
				throw new JsonWriterException("No close token for type: " + type);
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000040F0 File Offset: 0x000022F0
		private void AutoCompleteClose(JsonToken tokenBeingClosed)
		{
			int num = 0;
			for (int i = 0; i < this._top; i++)
			{
				int index = this._top - i;
				if (this._stack[index] == this.GetTypeForCloseToken(tokenBeingClosed))
				{
					num = i + 1;
					break;
				}
			}
			if (num == 0)
			{
				throw new JsonWriterException("No token to close.");
			}
			for (int j = 0; j < num; j++)
			{
				JsonToken closeTokenForType = this.GetCloseTokenForType(this.Pop());
				if (this._currentState != JsonWriter.State.ObjectStart && this._currentState != JsonWriter.State.ArrayStart)
				{
					this.WriteIndent();
				}
				this.WriteEnd(closeTokenForType);
			}
			JTokenType jtokenType = this.Peek();
			switch (jtokenType)
			{
			case JTokenType.None:
				this._currentState = JsonWriter.State.Start;
				return;
			case JTokenType.Object:
				this._currentState = JsonWriter.State.Object;
				return;
			case JTokenType.Array:
				this._currentState = JsonWriter.State.Array;
				return;
			case JTokenType.Constructor:
				this._currentState = JsonWriter.State.Array;
				return;
			default:
				throw new JsonWriterException("Unknown JsonType: " + jtokenType);
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000041D7 File Offset: 0x000023D7
		protected virtual void WriteEnd(JsonToken token)
		{
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000041D9 File Offset: 0x000023D9
		protected virtual void WriteIndent()
		{
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000041DB File Offset: 0x000023DB
		protected virtual void WriteValueDelimiter()
		{
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000041DD File Offset: 0x000023DD
		protected virtual void WriteIndentSpace()
		{
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000041E0 File Offset: 0x000023E0
		internal void AutoComplete(JsonToken tokenBeingWritten)
		{
			int num;
			switch (tokenBeingWritten)
			{
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				num = 7;
				break;
			default:
				num = (int)tokenBeingWritten;
				break;
			}
			JsonWriter.State state = JsonWriter.stateArray[num][(int)this._currentState];
			if (state == JsonWriter.State.Error)
			{
				throw new JsonWriterException("Token {0} in state {1} would result in an invalid JavaScript object.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					tokenBeingWritten.ToString(),
					this._currentState.ToString()
				}));
			}
			if ((this._currentState == JsonWriter.State.Object || this._currentState == JsonWriter.State.Array || this._currentState == JsonWriter.State.Constructor) && tokenBeingWritten != JsonToken.Comment)
			{
				this.WriteValueDelimiter();
			}
			else if (this._currentState == JsonWriter.State.Property && this._formatting == Formatting.Indented)
			{
				this.WriteIndentSpace();
			}
			WriteState writeState = this.WriteState;
			if ((tokenBeingWritten == JsonToken.PropertyName && writeState != WriteState.Start) || writeState == WriteState.Array || writeState == WriteState.Constructor)
			{
				this.WriteIndent();
			}
			this._currentState = state;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000042E2 File Offset: 0x000024E2
		public virtual void WriteNull()
		{
			this.AutoComplete(JsonToken.Null);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000042EC File Offset: 0x000024EC
		public virtual void WriteUndefined()
		{
			this.AutoComplete(JsonToken.Undefined);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000042F6 File Offset: 0x000024F6
		public virtual void WriteRaw(string json)
		{
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000042F8 File Offset: 0x000024F8
		public virtual void WriteRawValue(string json)
		{
			this.AutoComplete(JsonToken.Undefined);
			this.WriteRaw(json);
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00004309 File Offset: 0x00002509
		public virtual void WriteValue(string value)
		{
			this.AutoComplete(JsonToken.String);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00004313 File Offset: 0x00002513
		public virtual void WriteValue(int value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x06000093 RID: 147 RVA: 0x0000431C File Offset: 0x0000251C
		[CLSCompliant(false)]
		public virtual void WriteValue(uint value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00004325 File Offset: 0x00002525
		public virtual void WriteValue(long value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x0000432E File Offset: 0x0000252E
		[CLSCompliant(false)]
		public virtual void WriteValue(ulong value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00004337 File Offset: 0x00002537
		public virtual void WriteValue(float value)
		{
			this.AutoComplete(JsonToken.Float);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00004340 File Offset: 0x00002540
		public virtual void WriteValue(double value)
		{
			this.AutoComplete(JsonToken.Float);
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004349 File Offset: 0x00002549
		public virtual void WriteValue(bool value)
		{
			this.AutoComplete(JsonToken.Boolean);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004353 File Offset: 0x00002553
		public virtual void WriteValue(short value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x0000435C File Offset: 0x0000255C
		[CLSCompliant(false)]
		public virtual void WriteValue(ushort value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00004365 File Offset: 0x00002565
		public virtual void WriteValue(char value)
		{
			this.AutoComplete(JsonToken.String);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000436F File Offset: 0x0000256F
		public virtual void WriteValue(byte value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00004378 File Offset: 0x00002578
		[CLSCompliant(false)]
		public virtual void WriteValue(sbyte value)
		{
			this.AutoComplete(JsonToken.Integer);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00004381 File Offset: 0x00002581
		public virtual void WriteValue(decimal value)
		{
			this.AutoComplete(JsonToken.Float);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000438A File Offset: 0x0000258A
		public virtual void WriteValue(DateTime value)
		{
			this.AutoComplete(JsonToken.Date);
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004394 File Offset: 0x00002594
		public virtual void WriteValue(DateTimeOffset value)
		{
			this.AutoComplete(JsonToken.Date);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000439E File Offset: 0x0000259E
		public virtual void WriteValue(int? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x000043BD File Offset: 0x000025BD
		[CLSCompliant(false)]
		public virtual void WriteValue(uint? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000043DC File Offset: 0x000025DC
		public virtual void WriteValue(long? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000043FB File Offset: 0x000025FB
		[CLSCompliant(false)]
		public virtual void WriteValue(ulong? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000441A File Offset: 0x0000261A
		public virtual void WriteValue(float? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004439 File Offset: 0x00002639
		public virtual void WriteValue(double? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00004458 File Offset: 0x00002658
		public virtual void WriteValue(bool? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00004478 File Offset: 0x00002678
		public virtual void WriteValue(short? value)
		{
			short? num = value;
			int? num2 = (num != null) ? new int?((int)num.GetValueOrDefault()) : null;
			if (num2 == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x000044C8 File Offset: 0x000026C8
		[CLSCompliant(false)]
		public virtual void WriteValue(ushort? value)
		{
			ushort? num = value;
			int? num2 = (num != null) ? new int?((int)num.GetValueOrDefault()) : null;
			if (num2 == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004518 File Offset: 0x00002718
		public virtual void WriteValue(char? value)
		{
			char? c = value;
			int? num = (c != null) ? new int?((int)c.GetValueOrDefault()) : null;
			if (num == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004568 File Offset: 0x00002768
		public virtual void WriteValue(byte? value)
		{
			byte? b = value;
			int? num = (b != null) ? new int?((int)b.GetValueOrDefault()) : null;
			if (num == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AC RID: 172 RVA: 0x000045B8 File Offset: 0x000027B8
		[CLSCompliant(false)]
		public virtual void WriteValue(sbyte? value)
		{
			sbyte? b = value;
			int? num = (b != null) ? new int?((int)b.GetValueOrDefault()) : null;
			if (num == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004605 File Offset: 0x00002805
		public virtual void WriteValue(decimal? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004624 File Offset: 0x00002824
		public virtual void WriteValue(DateTime? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004643 File Offset: 0x00002843
		public virtual void WriteValue(DateTimeOffset? value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.WriteValue(value.Value);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004662 File Offset: 0x00002862
		public virtual void WriteValue(byte[] value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			this.AutoComplete(JsonToken.Bytes);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004678 File Offset: 0x00002878
		public virtual void WriteValue(object value)
		{
			if (value == null)
			{
				this.WriteNull();
				return;
			}
			if (value is IConvertible)
			{
				IConvertible convertible = value as IConvertible;
				switch (convertible.GetTypeCode())
				{
				case TypeCode.DBNull:
					this.WriteNull();
					return;
				case TypeCode.Boolean:
					this.WriteValue(convertible.ToBoolean(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Char:
					this.WriteValue(convertible.ToChar(CultureInfo.InvariantCulture));
					return;
				case TypeCode.SByte:
					this.WriteValue(convertible.ToSByte(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Byte:
					this.WriteValue(convertible.ToByte(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Int16:
					this.WriteValue(convertible.ToInt16(CultureInfo.InvariantCulture));
					return;
				case TypeCode.UInt16:
					this.WriteValue(convertible.ToUInt16(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Int32:
					this.WriteValue(convertible.ToInt32(CultureInfo.InvariantCulture));
					return;
				case TypeCode.UInt32:
					this.WriteValue(convertible.ToUInt32(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Int64:
					this.WriteValue(convertible.ToInt64(CultureInfo.InvariantCulture));
					return;
				case TypeCode.UInt64:
					this.WriteValue(convertible.ToUInt64(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Single:
					this.WriteValue(convertible.ToSingle(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Double:
					this.WriteValue(convertible.ToDouble(CultureInfo.InvariantCulture));
					return;
				case TypeCode.Decimal:
					this.WriteValue(convertible.ToDecimal(CultureInfo.InvariantCulture));
					return;
				case TypeCode.DateTime:
					this.WriteValue(convertible.ToDateTime(CultureInfo.InvariantCulture));
					return;
				case TypeCode.String:
					this.WriteValue(convertible.ToString(CultureInfo.InvariantCulture));
					return;
				}
			}
			else
			{
				if (value is DateTimeOffset)
				{
					this.WriteValue((DateTimeOffset)value);
					return;
				}
				if (value is byte[])
				{
					this.WriteValue((byte[])value);
					return;
				}
			}
			throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				value.GetType()
			}));
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x0000485D File Offset: 0x00002A5D
		public virtual void WriteComment(string text)
		{
			this.AutoComplete(JsonToken.Comment);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00004866 File Offset: 0x00002A66
		public virtual void WriteWhitespace(string ws)
		{
			if (ws != null && !StringUtils.IsWhiteSpace(ws))
			{
				throw new JsonWriterException("Only white space characters should be used.");
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x0000487E File Offset: 0x00002A7E
		void IDisposable.Dispose()
		{
			this.Dispose(true);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004887 File Offset: 0x00002A87
		private void Dispose(bool disposing)
		{
			if (this.WriteState != WriteState.Closed)
			{
				this.Close();
			}
		}

		// Token: 0x04000060 RID: 96
		private static readonly JsonWriter.State[][] stateArray = new JsonWriter.State[][]
		{
			new JsonWriter.State[]
			{
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.ObjectStart,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.ArrayStart,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.ConstructorStart,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.Property,
				JsonWriter.State.Error,
				JsonWriter.State.Property,
				JsonWriter.State.Property,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.Start,
				JsonWriter.State.Property,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.Object,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.Array,
				JsonWriter.State.Constructor,
				JsonWriter.State.Constructor,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.Start,
				JsonWriter.State.Property,
				JsonWriter.State.ObjectStart,
				JsonWriter.State.Object,
				JsonWriter.State.ArrayStart,
				JsonWriter.State.Array,
				JsonWriter.State.Constructor,
				JsonWriter.State.Constructor,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			},
			new JsonWriter.State[]
			{
				JsonWriter.State.Start,
				JsonWriter.State.Object,
				JsonWriter.State.Error,
				JsonWriter.State.Error,
				JsonWriter.State.Array,
				JsonWriter.State.Array,
				JsonWriter.State.Constructor,
				JsonWriter.State.Constructor,
				JsonWriter.State.Error,
				JsonWriter.State.Error
			}
		};

		// Token: 0x04000061 RID: 97
		private int _top;

		// Token: 0x04000062 RID: 98
		private readonly List<JTokenType> _stack;

		// Token: 0x04000063 RID: 99
		private JsonWriter.State _currentState;

		// Token: 0x04000064 RID: 100
		private Formatting _formatting;

		// Token: 0x02000012 RID: 18
		private enum State
		{
			// Token: 0x04000066 RID: 102
			Start,
			// Token: 0x04000067 RID: 103
			Property,
			// Token: 0x04000068 RID: 104
			ObjectStart,
			// Token: 0x04000069 RID: 105
			Object,
			// Token: 0x0400006A RID: 106
			ArrayStart,
			// Token: 0x0400006B RID: 107
			Array,
			// Token: 0x0400006C RID: 108
			ConstructorStart,
			// Token: 0x0400006D RID: 109
			Constructor,
			// Token: 0x0400006E RID: 110
			Bytes,
			// Token: 0x0400006F RID: 111
			Closed,
			// Token: 0x04000070 RID: 112
			Error
		}
	}
}

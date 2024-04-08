using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x02000004 RID: 4
	public abstract class JsonReader : IDisposable
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002814 File Offset: 0x00000A14
		protected JsonReader.State CurrentState
		{
			get
			{
				return this._currentState;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000D RID: 13 RVA: 0x0000281C File Offset: 0x00000A1C
		// (set) Token: 0x0600000E RID: 14 RVA: 0x00002824 File Offset: 0x00000A24
		public virtual char QuoteChar
		{
			get
			{
				return this._quoteChar;
			}
			protected internal set
			{
				this._quoteChar = value;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000F RID: 15 RVA: 0x0000282D File Offset: 0x00000A2D
		public virtual JsonToken TokenType
		{
			get
			{
				return this._token;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002835 File Offset: 0x00000A35
		public virtual object Value
		{
			get
			{
				return this._value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000011 RID: 17 RVA: 0x0000283D File Offset: 0x00000A3D
		public virtual Type ValueType
		{
			get
			{
				return this._valueType;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002848 File Offset: 0x00000A48
		public virtual int Depth
		{
			get
			{
				int num = this._top - 1;
				if (JsonReader.IsStartToken(this.TokenType))
				{
					return num - 1;
				}
				return num;
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002870 File Offset: 0x00000A70
		protected JsonReader()
		{
			this._currentState = JsonReader.State.Start;
			this._stack = new List<JTokenType>();
			this.Push(JTokenType.None);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002891 File Offset: 0x00000A91
		private void Push(JTokenType value)
		{
			this._stack.Add(value);
			this._top++;
			this._currentTypeContext = value;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000028B4 File Offset: 0x00000AB4
		private JTokenType Pop()
		{
			JTokenType result = this.Peek();
			this._stack.RemoveAt(this._stack.Count - 1);
			this._top--;
			this._currentTypeContext = this._stack[this._top - 1];
			return result;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002908 File Offset: 0x00000B08
		private JTokenType Peek()
		{
			return this._currentTypeContext;
		}

		// Token: 0x06000017 RID: 23
		public abstract bool Read();

		// Token: 0x06000018 RID: 24
		public abstract byte[] ReadAsBytes();

		// Token: 0x06000019 RID: 25
		public abstract decimal? ReadAsDecimal();

		// Token: 0x0600001A RID: 26
		public abstract DateTimeOffset? ReadAsDateTimeOffset();

		// Token: 0x0600001B RID: 27 RVA: 0x00002910 File Offset: 0x00000B10
		public void Skip()
		{
			if (JsonReader.IsStartToken(this.TokenType))
			{
				int depth = this.Depth;
				while (this.Read() && depth < this.Depth)
				{
				}
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002942 File Offset: 0x00000B42
		protected void SetToken(JsonToken newToken)
		{
			this.SetToken(newToken, null);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000294C File Offset: 0x00000B4C
		protected virtual void SetToken(JsonToken newToken, object value)
		{
			this._token = newToken;
			switch (newToken)
			{
			case JsonToken.StartObject:
				this._currentState = JsonReader.State.ObjectStart;
				this.Push(JTokenType.Object);
				break;
			case JsonToken.StartArray:
				this._currentState = JsonReader.State.ArrayStart;
				this.Push(JTokenType.Array);
				break;
			case JsonToken.StartConstructor:
				this._currentState = JsonReader.State.ConstructorStart;
				this.Push(JTokenType.Constructor);
				break;
			case JsonToken.PropertyName:
				this._currentState = JsonReader.State.Property;
				this.Push(JTokenType.Property);
				break;
			case JsonToken.Raw:
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				this._currentState = JsonReader.State.PostValue;
				break;
			case JsonToken.EndObject:
				this.ValidateEnd(JsonToken.EndObject);
				this._currentState = JsonReader.State.PostValue;
				break;
			case JsonToken.EndArray:
				this.ValidateEnd(JsonToken.EndArray);
				this._currentState = JsonReader.State.PostValue;
				break;
			case JsonToken.EndConstructor:
				this.ValidateEnd(JsonToken.EndConstructor);
				this._currentState = JsonReader.State.PostValue;
				break;
			}
			JTokenType jtokenType = this.Peek();
			if (jtokenType == JTokenType.Property && this._currentState == JsonReader.State.PostValue)
			{
				this.Pop();
			}
			if (value != null)
			{
				this._value = value;
				this._valueType = value.GetType();
				return;
			}
			this._value = null;
			this._valueType = null;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002A6C File Offset: 0x00000C6C
		private void ValidateEnd(JsonToken endToken)
		{
			JTokenType jtokenType = this.Pop();
			if (this.GetTypeForCloseToken(endToken) != jtokenType)
			{
				throw new JsonReaderException("JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					endToken,
					jtokenType
				}));
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002ABC File Offset: 0x00000CBC
		protected void SetStateBasedOnCurrent()
		{
			JTokenType jtokenType = this.Peek();
			switch (jtokenType)
			{
			case JTokenType.None:
				this._currentState = JsonReader.State.Finished;
				return;
			case JTokenType.Object:
				this._currentState = JsonReader.State.Object;
				return;
			case JTokenType.Array:
				this._currentState = JsonReader.State.Array;
				return;
			case JTokenType.Constructor:
				this._currentState = JsonReader.State.Constructor;
				return;
			default:
				throw new JsonReaderException("While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					jtokenType
				}));
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002B34 File Offset: 0x00000D34
		internal static bool IsPrimitiveToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return true;
			}
			return false;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002B7C File Offset: 0x00000D7C
		internal static bool IsStartToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.None:
			case JsonToken.Comment:
			case JsonToken.Raw:
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.EndObject:
			case JsonToken.EndArray:
			case JsonToken.EndConstructor:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return false;
			case JsonToken.StartObject:
			case JsonToken.StartArray:
			case JsonToken.StartConstructor:
			case JsonToken.PropertyName:
				return true;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected JsonToken value.");
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002BF4 File Offset: 0x00000DF4
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
				throw new JsonReaderException("Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					token
				}));
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002C45 File Offset: 0x00000E45
		void IDisposable.Dispose()
		{
			this.Dispose(true);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002C4E File Offset: 0x00000E4E
		protected virtual void Dispose(bool disposing)
		{
			if (this._currentState != JsonReader.State.Closed && disposing)
			{
				this.Close();
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002C62 File Offset: 0x00000E62
		public virtual void Close()
		{
			this._currentState = JsonReader.State.Closed;
			this._token = JsonToken.None;
			this._value = null;
			this._valueType = null;
		}

		// Token: 0x0400000D RID: 13
		private JsonToken _token;

		// Token: 0x0400000E RID: 14
		private object _value;

		// Token: 0x0400000F RID: 15
		private Type _valueType;

		// Token: 0x04000010 RID: 16
		private char _quoteChar;

		// Token: 0x04000011 RID: 17
		private JsonReader.State _currentState;

		// Token: 0x04000012 RID: 18
		private JTokenType _currentTypeContext;

		// Token: 0x04000013 RID: 19
		private int _top;

		// Token: 0x04000014 RID: 20
		private readonly List<JTokenType> _stack;

		// Token: 0x02000005 RID: 5
		protected enum State
		{
			// Token: 0x04000016 RID: 22
			Start,
			// Token: 0x04000017 RID: 23
			Complete,
			// Token: 0x04000018 RID: 24
			Property,
			// Token: 0x04000019 RID: 25
			ObjectStart,
			// Token: 0x0400001A RID: 26
			Object,
			// Token: 0x0400001B RID: 27
			ArrayStart,
			// Token: 0x0400001C RID: 28
			Array,
			// Token: 0x0400001D RID: 29
			Closed,
			// Token: 0x0400001E RID: 30
			PostValue,
			// Token: 0x0400001F RID: 31
			ConstructorStart,
			// Token: 0x04000020 RID: 32
			Constructor,
			// Token: 0x04000021 RID: 33
			Error,
			// Token: 0x04000022 RID: 34
			Finished
		}
	}
}

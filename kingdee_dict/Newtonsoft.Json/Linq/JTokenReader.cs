using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200006D RID: 109
	public class JTokenReader : JsonReader, IJsonLineInfo
	{
		// Token: 0x06000518 RID: 1304 RVA: 0x00011802 File Offset: 0x0000FA02
		public JTokenReader(JToken token)
		{
			ValidationUtils.ArgumentNotNull(token, "token");
			this._root = token;
			this._current = token;
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x00011824 File Offset: 0x0000FA24
		public override byte[] ReadAsBytes()
		{
			this.Read();
			if (this.TokenType == JsonToken.String)
			{
				string text = (string)this.Value;
				byte[] value = (text.Length == 0) ? new byte[0] : Convert.FromBase64String(text);
				this.SetToken(JsonToken.Bytes, value);
			}
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

		// Token: 0x0600051A RID: 1306 RVA: 0x000118BC File Offset: 0x0000FABC
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

		// Token: 0x0600051B RID: 1307 RVA: 0x00011950 File Offset: 0x0000FB50
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

		// Token: 0x0600051C RID: 1308 RVA: 0x000119DC File Offset: 0x0000FBDC
		public override bool Read()
		{
			if (base.CurrentState == JsonReader.State.Start)
			{
				this.SetToken(this._current);
				return true;
			}
			JContainer jcontainer = this._current as JContainer;
			if (jcontainer != null && this._parent != jcontainer)
			{
				return this.ReadInto(jcontainer);
			}
			return this.ReadOver(this._current);
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x00011A2C File Offset: 0x0000FC2C
		private bool ReadOver(JToken t)
		{
			if (t == this._root)
			{
				return this.ReadToEnd();
			}
			JToken next = t.Next;
			if (next != null && next != t && t != t.Parent.Last)
			{
				this._current = next;
				this.SetToken(this._current);
				return true;
			}
			if (t.Parent == null)
			{
				return this.ReadToEnd();
			}
			return this.SetEnd(t.Parent);
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x00011A95 File Offset: 0x0000FC95
		private bool ReadToEnd()
		{
			return false;
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x0600051F RID: 1311 RVA: 0x00011A98 File Offset: 0x0000FC98
		private bool IsEndElement
		{
			get
			{
				return this._current == this._parent;
			}
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x00011AA8 File Offset: 0x0000FCA8
		private JsonToken? GetEndToken(JContainer c)
		{
			switch (c.Type)
			{
			case JTokenType.Object:
				return new JsonToken?(JsonToken.EndObject);
			case JTokenType.Array:
				return new JsonToken?(JsonToken.EndArray);
			case JTokenType.Constructor:
				return new JsonToken?(JsonToken.EndConstructor);
			case JTokenType.Property:
				return null;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type.");
			}
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00011B14 File Offset: 0x0000FD14
		private bool ReadInto(JContainer c)
		{
			JToken first = c.First;
			if (first == null)
			{
				return this.SetEnd(c);
			}
			this.SetToken(first);
			this._current = first;
			this._parent = c;
			return true;
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00011B4C File Offset: 0x0000FD4C
		private bool SetEnd(JContainer c)
		{
			JsonToken? endToken = this.GetEndToken(c);
			if (endToken != null)
			{
				base.SetToken(endToken.Value);
				this._current = c;
				this._parent = c;
				return true;
			}
			return this.ReadOver(c);
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00011B90 File Offset: 0x0000FD90
		private void SetToken(JToken token)
		{
			switch (token.Type)
			{
			case JTokenType.Object:
				base.SetToken(JsonToken.StartObject);
				return;
			case JTokenType.Array:
				base.SetToken(JsonToken.StartArray);
				return;
			case JTokenType.Constructor:
				base.SetToken(JsonToken.StartConstructor);
				return;
			case JTokenType.Property:
				this.SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
				return;
			case JTokenType.Comment:
				this.SetToken(JsonToken.Comment, ((JValue)token).Value);
				return;
			case JTokenType.Integer:
				this.SetToken(JsonToken.Integer, ((JValue)token).Value);
				return;
			case JTokenType.Float:
				this.SetToken(JsonToken.Float, ((JValue)token).Value);
				return;
			case JTokenType.String:
				this.SetToken(JsonToken.String, ((JValue)token).Value);
				return;
			case JTokenType.Boolean:
				this.SetToken(JsonToken.Boolean, ((JValue)token).Value);
				return;
			case JTokenType.Null:
				this.SetToken(JsonToken.Null, ((JValue)token).Value);
				return;
			case JTokenType.Undefined:
				this.SetToken(JsonToken.Undefined, ((JValue)token).Value);
				return;
			case JTokenType.Date:
				this.SetToken(JsonToken.Date, ((JValue)token).Value);
				return;
			case JTokenType.Raw:
				this.SetToken(JsonToken.Raw, ((JValue)token).Value);
				return;
			case JTokenType.Bytes:
				this.SetToken(JsonToken.Bytes, ((JValue)token).Value);
				return;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00011CF4 File Offset: 0x0000FEF4
		bool IJsonLineInfo.HasLineInfo()
		{
			if (base.CurrentState == JsonReader.State.Start)
			{
				return false;
			}
			IJsonLineInfo jsonLineInfo = this.IsEndElement ? null : this._current;
			return jsonLineInfo != null && jsonLineInfo.HasLineInfo();
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x00011D28 File Offset: 0x0000FF28
		int IJsonLineInfo.LineNumber
		{
			get
			{
				if (base.CurrentState == JsonReader.State.Start)
				{
					return 0;
				}
				IJsonLineInfo jsonLineInfo = this.IsEndElement ? null : this._current;
				if (jsonLineInfo != null)
				{
					return jsonLineInfo.LineNumber;
				}
				return 0;
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000526 RID: 1318 RVA: 0x00011D5C File Offset: 0x0000FF5C
		int IJsonLineInfo.LinePosition
		{
			get
			{
				if (base.CurrentState == JsonReader.State.Start)
				{
					return 0;
				}
				IJsonLineInfo jsonLineInfo = this.IsEndElement ? null : this._current;
				if (jsonLineInfo != null)
				{
					return jsonLineInfo.LinePosition;
				}
				return 0;
			}
		}

		// Token: 0x04000141 RID: 321
		private readonly JToken _root;

		// Token: 0x04000142 RID: 322
		private JToken _parent;

		// Token: 0x04000143 RID: 323
		private JToken _current;
	}
}

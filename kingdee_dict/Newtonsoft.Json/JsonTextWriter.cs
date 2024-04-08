using System;
using System.IO;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x0200005F RID: 95
	public class JsonTextWriter : JsonWriter
	{
		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600038F RID: 911 RVA: 0x0000D8F9 File Offset: 0x0000BAF9
		private Base64Encoder Base64Encoder
		{
			get
			{
				if (this._base64Encoder == null)
				{
					this._base64Encoder = new Base64Encoder(this._writer);
				}
				return this._base64Encoder;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000390 RID: 912 RVA: 0x0000D91A File Offset: 0x0000BB1A
		// (set) Token: 0x06000391 RID: 913 RVA: 0x0000D922 File Offset: 0x0000BB22
		public int Indentation
		{
			get
			{
				return this._indentation;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Indentation value must be greater than 0.");
				}
				this._indentation = value;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0000D93A File Offset: 0x0000BB3A
		// (set) Token: 0x06000393 RID: 915 RVA: 0x0000D942 File Offset: 0x0000BB42
		public char QuoteChar
		{
			get
			{
				return this._quoteChar;
			}
			set
			{
				if (value != '"' && value != '\'')
				{
					throw new ArgumentException("Invalid JavaScript string quote character. Valid quote characters are ' and \".");
				}
				this._quoteChar = value;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000394 RID: 916 RVA: 0x0000D960 File Offset: 0x0000BB60
		// (set) Token: 0x06000395 RID: 917 RVA: 0x0000D968 File Offset: 0x0000BB68
		public char IndentChar
		{
			get
			{
				return this._indentChar;
			}
			set
			{
				this._indentChar = value;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000396 RID: 918 RVA: 0x0000D971 File Offset: 0x0000BB71
		// (set) Token: 0x06000397 RID: 919 RVA: 0x0000D979 File Offset: 0x0000BB79
		public bool QuoteName
		{
			get
			{
				return this._quoteName;
			}
			set
			{
				this._quoteName = value;
			}
		}

		// Token: 0x06000398 RID: 920 RVA: 0x0000D982 File Offset: 0x0000BB82
		public JsonTextWriter(TextWriter textWriter)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException("textWriter");
			}
			this._writer = textWriter;
			this._quoteChar = '"';
			this._quoteName = true;
			this._indentChar = ' ';
			this._indentation = 2;
		}

		// Token: 0x06000399 RID: 921 RVA: 0x0000D9BD File Offset: 0x0000BBBD
		public override void Flush()
		{
			this._writer.Flush();
		}

		// Token: 0x0600039A RID: 922 RVA: 0x0000D9CA File Offset: 0x0000BBCA
		public override void Close()
		{
			base.Close();
			this._writer.Close();
		}

		// Token: 0x0600039B RID: 923 RVA: 0x0000D9DD File Offset: 0x0000BBDD
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			this._writer.Write("{");
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0000D9F5 File Offset: 0x0000BBF5
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			this._writer.Write("[");
		}

		// Token: 0x0600039D RID: 925 RVA: 0x0000DA0D File Offset: 0x0000BC0D
		public override void WriteStartConstructor(string name)
		{
			base.WriteStartConstructor(name);
			this._writer.Write("new ");
			this._writer.Write(name);
			this._writer.Write("(");
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0000DA44 File Offset: 0x0000BC44
		protected override void WriteEnd(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
				this._writer.Write("}");
				return;
			case JsonToken.EndArray:
				this._writer.Write("]");
				return;
			case JsonToken.EndConstructor:
				this._writer.Write(")");
				return;
			default:
				throw new JsonWriterException("Invalid JsonToken: " + token);
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0000DAB2 File Offset: 0x0000BCB2
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			JavaScriptUtils.WriteEscapedJavaScriptString(this._writer, name, this._quoteChar, this._quoteName);
			this._writer.Write(':');
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x0000DAE0 File Offset: 0x0000BCE0
		protected override void WriteIndent()
		{
			if (base.Formatting == Formatting.Indented)
			{
				this._writer.Write(Environment.NewLine);
				int num = base.Top * this._indentation;
				for (int i = 0; i < num; i++)
				{
					this._writer.Write(this._indentChar);
				}
			}
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x0000DB31 File Offset: 0x0000BD31
		protected override void WriteValueDelimiter()
		{
			this._writer.Write(',');
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0000DB40 File Offset: 0x0000BD40
		protected override void WriteIndentSpace()
		{
			this._writer.Write(' ');
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x0000DB4F File Offset: 0x0000BD4F
		private void WriteValueInternal(string value, JsonToken token)
		{
			this._writer.Write(value);
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0000DB5D File Offset: 0x0000BD5D
		public override void WriteNull()
		{
			base.WriteNull();
			this.WriteValueInternal(JsonConvert.Null, JsonToken.Null);
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0000DB72 File Offset: 0x0000BD72
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			this.WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0000DB87 File Offset: 0x0000BD87
		public override void WriteRaw(string json)
		{
			base.WriteRaw(json);
			this._writer.Write(json);
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0000DB9C File Offset: 0x0000BD9C
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			if (value == null)
			{
				this.WriteValueInternal(JsonConvert.Null, JsonToken.Null);
				return;
			}
			JavaScriptUtils.WriteEscapedJavaScriptString(this._writer, value, this._quoteChar, true);
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0000DBC9 File Offset: 0x0000BDC9
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0000DBDF File Offset: 0x0000BDDF
		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0000DBF5 File Offset: 0x0000BDF5
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0000DC0B File Offset: 0x0000BE0B
		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0000DC21 File Offset: 0x0000BE21
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0000DC37 File Offset: 0x0000BE37
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0000DC4D File Offset: 0x0000BE4D
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0000DC64 File Offset: 0x0000BE64
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0000DC7A File Offset: 0x0000BE7A
		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0000DC90 File Offset: 0x0000BE90
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0000DCA6 File Offset: 0x0000BEA6
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x0000DCBC File Offset: 0x0000BEBC
		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x0000DCD2 File Offset: 0x0000BED2
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x0000DCE8 File Offset: 0x0000BEE8
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			JsonConvert.WriteDateTimeString(this._writer, value);
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0000DD00 File Offset: 0x0000BF00
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			if (value != null)
			{
				this._writer.Write(this._quoteChar);
				this.Base64Encoder.Encode(value, 0, value.Length);
				this.Base64Encoder.Flush();
				this._writer.Write(this._quoteChar);
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0000DD54 File Offset: 0x0000BF54
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Date);
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0000DD6B File Offset: 0x0000BF6B
		public override void WriteComment(string text)
		{
			base.WriteComment(text);
			this._writer.Write("/*");
			this._writer.Write(text);
			this._writer.Write("*/");
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0000DDA0 File Offset: 0x0000BFA0
		public override void WriteWhitespace(string ws)
		{
			base.WriteWhitespace(ws);
			this._writer.Write(ws);
		}

		// Token: 0x04000114 RID: 276
		private readonly TextWriter _writer;

		// Token: 0x04000115 RID: 277
		private Base64Encoder _base64Encoder;

		// Token: 0x04000116 RID: 278
		private char _indentChar;

		// Token: 0x04000117 RID: 279
		private int _indentation;

		// Token: 0x04000118 RID: 280
		private char _quoteChar;

		// Token: 0x04000119 RID: 281
		private bool _quoteName;
	}
}

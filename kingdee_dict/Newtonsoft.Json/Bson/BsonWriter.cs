using System;
using System.IO;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000013 RID: 19
	public class BsonWriter : JsonWriter
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00004AAC File Offset: 0x00002CAC
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00004AB9 File Offset: 0x00002CB9
		public DateTimeKind DateTimeKindHandling
		{
			get
			{
				return this._writer.DateTimeKindHandling;
			}
			set
			{
				this._writer.DateTimeKindHandling = value;
			}
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00004AC7 File Offset: 0x00002CC7
		public BsonWriter(Stream stream)
		{
			ValidationUtils.ArgumentNotNull(stream, "stream");
			this._writer = new BsonBinaryWriter(stream);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004AE6 File Offset: 0x00002CE6
		public override void Flush()
		{
			this._writer.Flush();
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004AF3 File Offset: 0x00002CF3
		protected override void WriteEnd(JsonToken token)
		{
			base.WriteEnd(token);
			this.RemoveParent();
			if (base.Top == 0)
			{
				this._writer.WriteToken(this._root);
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004B1B File Offset: 0x00002D1B
		public override void WriteComment(string text)
		{
			throw new JsonWriterException("Cannot write JSON comment as BSON.");
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004B27 File Offset: 0x00002D27
		public override void WriteStartConstructor(string name)
		{
			throw new JsonWriterException("Cannot write JSON constructor as BSON.");
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004B33 File Offset: 0x00002D33
		public override void WriteRaw(string json)
		{
			throw new JsonWriterException("Cannot write raw JSON as BSON.");
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004B3F File Offset: 0x00002D3F
		public override void WriteRawValue(string json)
		{
			throw new JsonWriterException("Cannot write raw JSON as BSON.");
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004B4B File Offset: 0x00002D4B
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			this.AddParent(new BsonArray());
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004B5E File Offset: 0x00002D5E
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			this.AddParent(new BsonObject());
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004B71 File Offset: 0x00002D71
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			this._propertyName = name;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00004B81 File Offset: 0x00002D81
		private void AddParent(BsonToken container)
		{
			this.AddToken(container);
			this._parent = container;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004B91 File Offset: 0x00002D91
		private void RemoveParent()
		{
			this._parent = this._parent.Parent;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004BA4 File Offset: 0x00002DA4
		private void AddValue(object value, BsonType type)
		{
			this.AddToken(new BsonValue(value, type));
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004BB4 File Offset: 0x00002DB4
		internal void AddToken(BsonToken token)
		{
			if (this._parent == null)
			{
				this._parent = token;
				this._root = token;
				return;
			}
			if (this._parent is BsonObject)
			{
				((BsonObject)this._parent).Add(this._propertyName, token);
				this._propertyName = null;
				return;
			}
			((BsonArray)this._parent).Add(token);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004C15 File Offset: 0x00002E15
		public override void WriteNull()
		{
			base.WriteNull();
			this.AddValue(null, BsonType.Null);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004C26 File Offset: 0x00002E26
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			this.AddValue(null, BsonType.Undefined);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00004C36 File Offset: 0x00002E36
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			if (value == null)
			{
				this.AddValue(null, BsonType.Null);
				return;
			}
			this.AddToken(new BsonString(value, true));
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00004C59 File Offset: 0x00002E59
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00004C70 File Offset: 0x00002E70
		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			if (value > 2147483647U)
			{
				throw new JsonWriterException("Value is too large to fit in a signed 32 bit integer. BSON does not support unsigned values.");
			}
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00004C9A File Offset: 0x00002E9A
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Long);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00004CB1 File Offset: 0x00002EB1
		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			if (value > 9223372036854775807UL)
			{
				throw new JsonWriterException("Value is too large to fit in a signed 64 bit integer. BSON does not support unsigned values.");
			}
			base.WriteValue(value);
			this.AddValue(value, BsonType.Long);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00004CDF File Offset: 0x00002EDF
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Number);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00004CF5 File Offset: 0x00002EF5
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Number);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00004D0B File Offset: 0x00002F0B
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Boolean);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00004D21 File Offset: 0x00002F21
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00004D38 File Offset: 0x00002F38
		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00004D4F File Offset: 0x00002F4F
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			this.AddToken(new BsonString(value.ToString(), true));
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00004D6B File Offset: 0x00002F6B
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00004D82 File Offset: 0x00002F82
		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Integer);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00004D99 File Offset: 0x00002F99
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Number);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00004DAF File Offset: 0x00002FAF
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Date);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00004DC6 File Offset: 0x00002FC6
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Date);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00004DDD File Offset: 0x00002FDD
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			this.AddValue(value, BsonType.Binary);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00004DEE File Offset: 0x00002FEE
		public void WriteObjectId(byte[] value)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value.Length != 12)
			{
				throw new Exception("An object id must be 12 bytes");
			}
			base.AutoComplete(JsonToken.Undefined);
			this.AddValue(value, BsonType.Oid);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00004E1D File Offset: 0x0000301D
		public void WriteRegex(string pattern, string options)
		{
			ValidationUtils.ArgumentNotNull(pattern, "pattern");
			base.AutoComplete(JsonToken.Undefined);
			this.AddToken(new BsonRegex(pattern, options));
		}

		// Token: 0x04000071 RID: 113
		private readonly BsonBinaryWriter _writer;

		// Token: 0x04000072 RID: 114
		private BsonToken _root;

		// Token: 0x04000073 RID: 115
		private BsonToken _parent;

		// Token: 0x04000074 RID: 116
		private string _propertyName;
	}
}

using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200006E RID: 110
	public class JTokenWriter : JsonWriter
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x00011D90 File Offset: 0x0000FF90
		public JToken Token
		{
			get
			{
				if (this._token != null)
				{
					return this._token;
				}
				return this._value;
			}
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x00011DA7 File Offset: 0x0000FFA7
		public JTokenWriter(JContainer container)
		{
			ValidationUtils.ArgumentNotNull(container, "container");
			this._token = container;
			this._parent = container;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x00011DC8 File Offset: 0x0000FFC8
		public JTokenWriter()
		{
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x00011DD0 File Offset: 0x0000FFD0
		public override void Flush()
		{
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x00011DD2 File Offset: 0x0000FFD2
		public override void Close()
		{
			base.Close();
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x00011DDA File Offset: 0x0000FFDA
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			this.AddParent(new JObject());
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00011DED File Offset: 0x0000FFED
		private void AddParent(JContainer container)
		{
			if (this._parent == null)
			{
				this._token = container;
			}
			else
			{
				this._parent.Add(container);
			}
			this._parent = container;
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00011E13 File Offset: 0x00010013
		private void RemoveParent()
		{
			this._parent = this._parent.Parent;
			if (this._parent != null && this._parent.Type == JTokenType.Property)
			{
				this._parent = this._parent.Parent;
			}
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00011E4D File Offset: 0x0001004D
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			this.AddParent(new JArray());
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00011E60 File Offset: 0x00010060
		public override void WriteStartConstructor(string name)
		{
			base.WriteStartConstructor(name);
			this.AddParent(new JConstructor(name));
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x00011E75 File Offset: 0x00010075
		protected override void WriteEnd(JsonToken token)
		{
			this.RemoveParent();
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x00011E7D File Offset: 0x0001007D
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			this.AddParent(new JProperty(name));
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x00011E92 File Offset: 0x00010092
		private void AddValue(object value, JsonToken token)
		{
			this.AddValue(new JValue(value), token);
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x00011EA1 File Offset: 0x000100A1
		internal void AddValue(JValue value, JsonToken token)
		{
			if (this._parent != null)
			{
				this._parent.Add(value);
				if (this._parent.Type == JTokenType.Property)
				{
					this._parent = this._parent.Parent;
					return;
				}
			}
			else
			{
				this._value = value;
			}
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x00011EDE File Offset: 0x000100DE
		public override void WriteNull()
		{
			base.WriteNull();
			this.AddValue(null, JsonToken.Null);
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x00011EEF File Offset: 0x000100EF
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			this.AddValue(null, JsonToken.Undefined);
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x00011F00 File Offset: 0x00010100
		public override void WriteRaw(string json)
		{
			base.WriteRaw(json);
			this.AddValue(new JRaw(json), JsonToken.Raw);
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00011F16 File Offset: 0x00010116
		public override void WriteComment(string text)
		{
			base.WriteComment(text);
			this.AddValue(JValue.CreateComment(text), JsonToken.Comment);
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00011F2C File Offset: 0x0001012C
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			this.AddValue(value ?? string.Empty, JsonToken.String);
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00011F47 File Offset: 0x00010147
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x00011F5D File Offset: 0x0001015D
		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x00011F73 File Offset: 0x00010173
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x00011F89 File Offset: 0x00010189
		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x00011F9F File Offset: 0x0001019F
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Float);
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x00011FB5 File Offset: 0x000101B5
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Float);
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x00011FCB File Offset: 0x000101CB
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Boolean);
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00011FE2 File Offset: 0x000101E2
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00011FF8 File Offset: 0x000101F8
		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0001200E File Offset: 0x0001020E
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			this.AddValue(value.ToString(), JsonToken.String);
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00012026 File Offset: 0x00010226
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x0001203C File Offset: 0x0001023C
		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Integer);
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x00012052 File Offset: 0x00010252
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Float);
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x00012068 File Offset: 0x00010268
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Date);
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0001207F File Offset: 0x0001027F
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Date);
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x00012096 File Offset: 0x00010296
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			this.AddValue(value, JsonToken.Bytes);
		}

		// Token: 0x04000144 RID: 324
		private JContainer _token;

		// Token: 0x04000145 RID: 325
		private JContainer _parent;

		// Token: 0x04000146 RID: 326
		private JValue _value;
	}
}

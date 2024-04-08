using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200006C RID: 108
	public class JArray : JContainer, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable
	{
		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060004FD RID: 1277 RVA: 0x00011542 File Offset: 0x0000F742
		public override JTokenType Type
		{
			get
			{
				return JTokenType.Array;
			}
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x00011545 File Offset: 0x0000F745
		public JArray()
		{
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x0001154D File Offset: 0x0000F74D
		public JArray(JArray other) : base(other)
		{
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x00011556 File Offset: 0x0000F756
		public JArray(params object[] content) : this(content)
		{
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0001155F File Offset: 0x0000F75F
		public JArray(object content)
		{
			base.Add(content);
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x00011570 File Offset: 0x0000F770
		internal override bool DeepEquals(JToken node)
		{
			JArray jarray = node as JArray;
			return jarray != null && base.ContentsEqual(jarray);
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00011590 File Offset: 0x0000F790
		internal override JToken CloneToken()
		{
			return new JArray(this);
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00011598 File Offset: 0x0000F798
		public new static JArray Load(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JArray from JsonReader.");
			}
			if (reader.TokenType != JsonToken.StartArray)
			{
				throw new Exception("Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
			JArray jarray = new JArray();
			jarray.SetLineInfo(reader as IJsonLineInfo);
			jarray.ReadTokenFrom(reader);
			return jarray;
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00011610 File Offset: 0x0000F810
		public new static JArray Parse(string json)
		{
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return JArray.Load(reader);
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0001162F File Offset: 0x0000F82F
		public new static JArray FromObject(object o)
		{
			return JArray.FromObject(o, new JsonSerializer());
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0001163C File Offset: 0x0000F83C
		public new static JArray FromObject(object o, JsonSerializer jsonSerializer)
		{
			JToken jtoken = JToken.FromObjectInternal(o, jsonSerializer);
			if (jtoken.Type != JTokenType.Array)
			{
				throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					jtoken.Type
				}));
			}
			return (JArray)jtoken;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0001168C File Offset: 0x0000F88C
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartArray();
			foreach (JToken jtoken in this.Children())
			{
				jtoken.WriteTo(writer, converters);
			}
			writer.WriteEndArray();
		}

		// Token: 0x170000F9 RID: 249
		public override JToken this[object key]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				if (!(key is int))
				{
					throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Array position index expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				return this.GetItem((int)key);
			}
			set
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				if (!(key is int))
				{
					throw new ArgumentException("Set JArray values with invalid key value: {0}. Array position index expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				this.SetItem((int)key, value);
			}
		}

		// Token: 0x170000FA RID: 250
		public JToken this[int index]
		{
			get
			{
				return this.GetItem(index);
			}
			set
			{
				this.SetItem(index, value);
			}
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x000117A6 File Offset: 0x0000F9A6
		public int IndexOf(JToken item)
		{
			return base.IndexOfItem(item);
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x000117AF File Offset: 0x0000F9AF
		public void Insert(int index, JToken item)
		{
			this.InsertItem(index, item);
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x000117B9 File Offset: 0x0000F9B9
		public void RemoveAt(int index)
		{
			this.RemoveItemAt(index);
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x000117C2 File Offset: 0x0000F9C2
		public void Add(JToken item)
		{
			base.Add(item);
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x000117CB File Offset: 0x0000F9CB
		public void Clear()
		{
			this.ClearItems();
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x000117D3 File Offset: 0x0000F9D3
		public bool Contains(JToken item)
		{
			return this.ContainsItem(item);
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x000117DC File Offset: 0x0000F9DC
		void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
		{
			this.CopyItemsTo(array, arrayIndex);
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x000117E6 File Offset: 0x0000F9E6
		public int Count
		{
			get
			{
				return this.CountItems();
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000515 RID: 1301 RVA: 0x000117EE File Offset: 0x0000F9EE
		bool ICollection<JToken>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x000117F1 File Offset: 0x0000F9F1
		public bool Remove(JToken item)
		{
			return this.RemoveItem(item);
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x000117FA File Offset: 0x0000F9FA
		internal override int GetDeepHashCode()
		{
			return base.ContentsHashCode();
		}
	}
}

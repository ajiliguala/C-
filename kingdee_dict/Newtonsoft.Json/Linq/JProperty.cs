using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000070 RID: 112
	public class JProperty : JContainer
	{
		// Token: 0x17000101 RID: 257
		// (get) Token: 0x0600055D RID: 1373 RVA: 0x000123CE File Offset: 0x000105CE
		public string Name
		{
			[DebuggerStepThrough]
			get
			{
				return this._name;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x0600055E RID: 1374 RVA: 0x000123D6 File Offset: 0x000105D6
		// (set) Token: 0x0600055F RID: 1375 RVA: 0x000123E0 File Offset: 0x000105E0
		public new JToken Value
		{
			[DebuggerStepThrough]
			get
			{
				return base.Content;
			}
			set
			{
				base.CheckReentrancy();
				JToken jtoken = value ?? new JValue(null);
				if (base.Content == null)
				{
					jtoken = base.EnsureParentToken(jtoken);
					base.Content = jtoken;
					base.Content.Parent = this;
					base.Content.Next = base.Content;
					return;
				}
				base.Content.Replace(jtoken);
			}
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00012440 File Offset: 0x00010640
		internal override void ReplaceItem(JToken existing, JToken replacement)
		{
			if (JContainer.IsTokenUnchanged(existing, replacement))
			{
				return;
			}
			if (base.Parent != null)
			{
				((JObject)base.Parent).InternalPropertyChanging(this);
			}
			base.ReplaceItem(existing, replacement);
			if (base.Parent != null)
			{
				((JObject)base.Parent).InternalPropertyChanged(this);
			}
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00012491 File Offset: 0x00010691
		public JProperty(JProperty other) : base(other)
		{
			this._name = other.Name;
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x000124A8 File Offset: 0x000106A8
		internal override void AddItem(bool isLast, JToken previous, JToken item)
		{
			if (this.Value != null)
			{
				throw new Exception("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					typeof(JProperty)
				}));
			}
			this.Value = item;
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x000124EE File Offset: 0x000106EE
		internal override JToken GetItem(int index)
		{
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return this.Value;
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x000124FF File Offset: 0x000106FF
		internal override void SetItem(int index, JToken item)
		{
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.Value = item;
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x00012514 File Offset: 0x00010714
		internal override bool RemoveItem(JToken item)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeof(JProperty)
			}));
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0001254C File Offset: 0x0001074C
		internal override void RemoveItemAt(int index)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeof(JProperty)
			}));
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x00012584 File Offset: 0x00010784
		internal override void InsertItem(int index, JToken item)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeof(JProperty)
			}));
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x000125BA File Offset: 0x000107BA
		internal override bool ContainsItem(JToken item)
		{
			return this.Value == item;
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x000125C8 File Offset: 0x000107C8
		internal override void ClearItems()
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeof(JProperty)
			}));
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x000125FE File Offset: 0x000107FE
		public override JEnumerable<JToken> Children()
		{
			return new JEnumerable<JToken>(this.GetValueEnumerable());
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x000126E4 File Offset: 0x000108E4
		private IEnumerable<JToken> GetValueEnumerable()
		{
			yield return this.Value;
			yield break;
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x00012704 File Offset: 0x00010904
		internal override bool DeepEquals(JToken node)
		{
			JProperty jproperty = node as JProperty;
			return jproperty != null && this._name == jproperty.Name && base.ContentsEqual(jproperty);
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x00012737 File Offset: 0x00010937
		internal override JToken CloneToken()
		{
			return new JProperty(this);
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0001273F File Offset: 0x0001093F
		public override JTokenType Type
		{
			[DebuggerStepThrough]
			get
			{
				return JTokenType.Property;
			}
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x00012742 File Offset: 0x00010942
		internal JProperty(string name)
		{
			ValidationUtils.ArgumentNotNull(name, "name");
			this._name = name;
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0001275C File Offset: 0x0001095C
		public JProperty(string name, params object[] content) : this(name, content)
		{
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x00012766 File Offset: 0x00010966
		public JProperty(string name, object content)
		{
			ValidationUtils.ArgumentNotNull(name, "name");
			this._name = name;
			this.Value = (base.IsMultiContent(content) ? new JArray(content) : base.CreateFromContent(content));
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0001279E File Offset: 0x0001099E
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WritePropertyName(this._name);
			this.Value.WriteTo(writer, converters);
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x000127B9 File Offset: 0x000109B9
		internal override int GetDeepHashCode()
		{
			return this._name.GetHashCode() ^ ((this.Value != null) ? this.Value.GetDeepHashCode() : 0);
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x000127E0 File Offset: 0x000109E0
		public new static JProperty Load(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JProperty from JsonReader.");
			}
			if (reader.TokenType != JsonToken.PropertyName)
			{
				throw new Exception("Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
			JProperty jproperty = new JProperty((string)reader.Value);
			jproperty.SetLineInfo(reader as IJsonLineInfo);
			jproperty.ReadTokenFrom(reader);
			return jproperty;
		}

		// Token: 0x0400014E RID: 334
		private readonly string _name;
	}
}

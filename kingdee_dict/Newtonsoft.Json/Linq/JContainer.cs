using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000067 RID: 103
	public abstract class JContainer : JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, ITypedList, IBindingList, IList, ICollection, IEnumerable, INotifyCollectionChanged
	{
		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600044B RID: 1099 RVA: 0x0000F558 File Offset: 0x0000D758
		// (remove) Token: 0x0600044C RID: 1100 RVA: 0x0000F590 File Offset: 0x0000D790
		public event ListChangedEventHandler ListChanged;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x0600044D RID: 1101 RVA: 0x0000F5C8 File Offset: 0x0000D7C8
		// (remove) Token: 0x0600044E RID: 1102 RVA: 0x0000F600 File Offset: 0x0000D800
		public event AddingNewEventHandler AddingNew;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x0600044F RID: 1103 RVA: 0x0000F638 File Offset: 0x0000D838
		// (remove) Token: 0x06000450 RID: 1104 RVA: 0x0000F670 File Offset: 0x0000D870
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x0000F6A5 File Offset: 0x0000D8A5
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x0000F6AD File Offset: 0x0000D8AD
		internal JToken Content
		{
			get
			{
				return this._content;
			}
			set
			{
				this._content = value;
			}
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x0000F6B6 File Offset: 0x0000D8B6
		internal JContainer()
		{
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x0000F6C0 File Offset: 0x0000D8C0
		internal JContainer(JContainer other)
		{
			ValidationUtils.ArgumentNotNull(other, "c");
			JToken jtoken = other.Last;
			if (jtoken != null)
			{
				do
				{
					jtoken = jtoken._next;
					this.Add(jtoken.CloneToken());
				}
				while (jtoken != other.Last);
			}
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x0000F704 File Offset: 0x0000D904
		internal void CheckReentrancy()
		{
			if (this._busy)
			{
				throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x0000F740 File Offset: 0x0000D940
		protected virtual void OnAddingNew(AddingNewEventArgs e)
		{
			AddingNewEventHandler addingNew = this.AddingNew;
			if (addingNew != null)
			{
				addingNew(this, e);
			}
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x0000F760 File Offset: 0x0000D960
		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			ListChangedEventHandler listChanged = this.ListChanged;
			if (listChanged != null)
			{
				this._busy = true;
				try
				{
					listChanged(this, e);
				}
				finally
				{
					this._busy = false;
				}
			}
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x0000F7A0 File Offset: 0x0000D9A0
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				this._busy = true;
				try
				{
					collectionChanged(this, e);
				}
				finally
				{
					this._busy = false;
				}
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x0000F7E0 File Offset: 0x0000D9E0
		public override bool HasValues
		{
			get
			{
				return this._content != null;
			}
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x0000F7F0 File Offset: 0x0000D9F0
		internal bool ContentsEqual(JContainer container)
		{
			JToken jtoken = this.First;
			JToken jtoken2 = container.First;
			if (jtoken == jtoken2)
			{
				return true;
			}
			while (jtoken != null || jtoken2 != null)
			{
				if (jtoken == null || jtoken2 == null || !jtoken.DeepEquals(jtoken2))
				{
					return false;
				}
				jtoken = ((jtoken != this.Last) ? jtoken.Next : null);
				jtoken2 = ((jtoken2 != container.Last) ? jtoken2.Next : null);
			}
			return true;
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600045B RID: 1115 RVA: 0x0000F851 File Offset: 0x0000DA51
		public override JToken First
		{
			get
			{
				if (this.Last == null)
				{
					return null;
				}
				return this.Last._next;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600045C RID: 1116 RVA: 0x0000F868 File Offset: 0x0000DA68
		public override JToken Last
		{
			[DebuggerStepThrough]
			get
			{
				return this._content;
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0000F870 File Offset: 0x0000DA70
		public override JEnumerable<JToken> Children()
		{
			return new JEnumerable<JToken>(this.ChildrenInternal());
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000F98C File Offset: 0x0000DB8C
		internal IEnumerable<JToken> ChildrenInternal()
		{
			JToken first = this.First;
			JToken current = first;
			if (current != null)
			{
				do
				{
					yield return current;
				}
				while ((current = current.Next) != null);
			}
			yield break;
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x0000F9A9 File Offset: 0x0000DBA9
		public override IEnumerable<T> Values<T>()
		{
			return this.Children().Convert<JToken, T>();
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x0000FC3C File Offset: 0x0000DE3C
		public IEnumerable<JToken> Descendants()
		{
			foreach (JToken o in this.Children())
			{
				yield return o;
				JContainer c = o as JContainer;
				if (c != null)
				{
					foreach (JToken d in c.Descendants())
					{
						yield return d;
					}
				}
			}
			yield break;
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0000FC59 File Offset: 0x0000DE59
		internal bool IsMultiContent(object content)
		{
			return content is IEnumerable && !(content is string) && !(content is JToken) && !(content is byte[]);
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x0000FC84 File Offset: 0x0000DE84
		internal virtual void AddItem(bool isLast, JToken previous, JToken item)
		{
			this.CheckReentrancy();
			this.ValidateToken(item, null);
			item = this.EnsureParentToken(item);
			JToken next = (previous != null) ? previous._next : item;
			item.Parent = this;
			item.Next = next;
			if (previous != null)
			{
				previous.Next = item;
			}
			if (isLast || previous == null)
			{
				this._content = item;
			}
			if (this.ListChanged != null)
			{
				this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, this.IndexOfItem(item)));
			}
			if (this.CollectionChanged != null)
			{
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this.IndexOfItem(item)));
			}
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x0000FD14 File Offset: 0x0000DF14
		internal JToken EnsureParentToken(JToken item)
		{
			if (item.Parent != null)
			{
				item = item.CloneToken();
			}
			else
			{
				JContainer jcontainer = this;
				while (jcontainer.Parent != null)
				{
					jcontainer = jcontainer.Parent;
				}
				if (item == jcontainer)
				{
					item = item.CloneToken();
				}
			}
			return item;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x0000FD54 File Offset: 0x0000DF54
		internal void AddInternal(bool isLast, JToken previous, object content)
		{
			if (this.IsMultiContent(content))
			{
				IEnumerable enumerable = (IEnumerable)content;
				JToken jtoken = previous;
				using (IEnumerator enumerator = enumerable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object content2 = enumerator.Current;
						this.AddInternal(isLast, jtoken, content2);
						jtoken = ((jtoken != null) ? jtoken._next : this.Last);
					}
					return;
				}
			}
			JToken item = this.CreateFromContent(content);
			this.AddItem(isLast, previous, item);
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x0000FDE4 File Offset: 0x0000DFE4
		internal int IndexOfItem(JToken item)
		{
			int num = 0;
			foreach (JToken jtoken in this.Children())
			{
				if (jtoken == item)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0000FE44 File Offset: 0x0000E044
		internal virtual void InsertItem(int index, JToken item)
		{
			if (index == 0)
			{
				this.AddFirst(item);
				return;
			}
			JToken item2 = this.GetItem(index);
			this.AddInternal(false, item2.Previous, item);
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0000FE74 File Offset: 0x0000E074
		internal virtual void RemoveItemAt(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "index is less than 0.");
			}
			this.CheckReentrancy();
			int num = 0;
			foreach (JToken jtoken in this.Children())
			{
				if (index == num)
				{
					jtoken.Remove();
					this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
					this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, jtoken, index));
					return;
				}
				num++;
			}
			throw new ArgumentOutOfRangeException("index", "index is equal to or greater than Count.");
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x0000FF14 File Offset: 0x0000E114
		internal virtual bool RemoveItem(JToken item)
		{
			if (item == null || item.Parent != this)
			{
				return false;
			}
			this.CheckReentrancy();
			JToken jtoken = this._content;
			int num = 0;
			while (jtoken._next != item)
			{
				num++;
				jtoken = jtoken._next;
			}
			if (jtoken == item)
			{
				this._content = null;
			}
			else
			{
				if (this._content == item)
				{
					this._content = jtoken;
				}
				jtoken._next = item._next;
			}
			item.Parent = null;
			item.Next = null;
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, num));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, num));
			return true;
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0000FFA7 File Offset: 0x0000E1A7
		internal virtual JToken GetItem(int index)
		{
			return this.Children().ElementAt(index);
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x0000FFBC File Offset: 0x0000E1BC
		internal virtual void SetItem(int index, JToken item)
		{
			this.CheckReentrancy();
			JToken item2 = this.GetItem(index);
			item2.Replace(item);
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item2, index));
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x0000FFFC File Offset: 0x0000E1FC
		internal virtual void ClearItems()
		{
			this.CheckReentrancy();
			while (this._content != null)
			{
				JToken content = this._content;
				JToken next = content._next;
				if (content != this._content || next != content._next)
				{
					throw new InvalidOperationException("This operation was corrupted by external code.");
				}
				if (next != content)
				{
					content._next = next._next;
				}
				else
				{
					this._content = null;
				}
				next.Parent = null;
				next._next = null;
			}
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00010084 File Offset: 0x0000E284
		internal virtual void ReplaceItem(JToken existing, JToken replacement)
		{
			if (existing == null || existing.Parent != this)
			{
				return;
			}
			if (JContainer.IsTokenUnchanged(existing, replacement))
			{
				return;
			}
			this.CheckReentrancy();
			replacement = this.EnsureParentToken(replacement);
			this.ValidateToken(replacement, existing);
			JToken jtoken = this._content;
			int num = 0;
			while (jtoken._next != existing)
			{
				num++;
				jtoken = jtoken._next;
			}
			if (jtoken == existing)
			{
				this._content = replacement;
				replacement._next = replacement;
			}
			else
			{
				if (this._content == existing)
				{
					this._content = replacement;
				}
				jtoken._next = replacement;
				replacement._next = existing._next;
			}
			replacement.Parent = this;
			existing.Parent = null;
			existing.Next = null;
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, num));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacement, existing, num));
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x00010146 File Offset: 0x0000E346
		internal virtual bool ContainsItem(JToken item)
		{
			return this.IndexOfItem(item) != -1;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00010158 File Offset: 0x0000E358
		internal virtual void CopyItemsTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
			}
			if (arrayIndex >= array.Length)
			{
				throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
			}
			if (this.CountItems() > array.Length - arrayIndex)
			{
				throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
			}
			int num = 0;
			foreach (JToken value in this.Children())
			{
				array.SetValue(value, arrayIndex + num);
				num++;
			}
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00010204 File Offset: 0x0000E404
		internal virtual int CountItems()
		{
			return this.Children().Count<JToken>();
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00010218 File Offset: 0x0000E418
		internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
		{
			JValue jvalue = currentValue as JValue;
			return jvalue != null && ((jvalue.Type == JTokenType.Null && newValue == null) || jvalue.Equals(newValue));
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00010248 File Offset: 0x0000E448
		internal virtual void ValidateToken(JToken o, JToken existing)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			if (o.Type == JTokenType.Property)
			{
				throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					o.GetType(),
					base.GetType()
				}));
			}
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00010298 File Offset: 0x0000E498
		public void Add(object content)
		{
			this.AddInternal(true, this.Last, content);
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x000102A8 File Offset: 0x0000E4A8
		public void AddFirst(object content)
		{
			this.AddInternal(false, this.Last, content);
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x000102B8 File Offset: 0x0000E4B8
		internal JToken CreateFromContent(object content)
		{
			if (content is JToken)
			{
				return (JToken)content;
			}
			return new JValue(content);
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x000102CF File Offset: 0x0000E4CF
		public JsonWriter CreateWriter()
		{
			return new JTokenWriter(this);
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x000102D7 File Offset: 0x0000E4D7
		public void ReplaceAll(object content)
		{
			this.ClearItems();
			this.Add(content);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x000102E6 File Offset: 0x0000E4E6
		public void RemoveAll()
		{
			this.ClearItems();
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x000102F0 File Offset: 0x0000E4F0
		internal void ReadTokenFrom(JsonReader r)
		{
			int depth = r.Depth;
			if (!r.Read())
			{
				throw new Exception("Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType().Name
				}));
			}
			this.ReadContentFrom(r);
			int depth2 = r.Depth;
			if (depth2 > depth)
			{
				throw new Exception("Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType().Name
				}));
			}
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00010374 File Offset: 0x0000E574
		internal void ReadContentFrom(JsonReader r)
		{
			ValidationUtils.ArgumentNotNull(r, "r");
			IJsonLineInfo lineInfo = r as IJsonLineInfo;
			JContainer jcontainer = this;
			for (;;)
			{
				if (jcontainer is JProperty && ((JProperty)jcontainer).Value != null)
				{
					if (jcontainer == this)
					{
						break;
					}
					jcontainer = jcontainer.Parent;
				}
				switch (r.TokenType)
				{
				case JsonToken.None:
					goto IL_224;
				case JsonToken.StartObject:
				{
					JObject jobject = new JObject();
					jobject.SetLineInfo(lineInfo);
					jcontainer.Add(jobject);
					jcontainer = jobject;
					goto IL_224;
				}
				case JsonToken.StartArray:
				{
					JArray jarray = new JArray();
					jarray.SetLineInfo(lineInfo);
					jcontainer.Add(jarray);
					jcontainer = jarray;
					goto IL_224;
				}
				case JsonToken.StartConstructor:
				{
					JConstructor jconstructor = new JConstructor(r.Value.ToString());
					jconstructor.SetLineInfo(jconstructor);
					jcontainer.Add(jconstructor);
					jcontainer = jconstructor;
					goto IL_224;
				}
				case JsonToken.PropertyName:
				{
					string name = r.Value.ToString();
					JProperty jproperty = new JProperty(name);
					jproperty.SetLineInfo(lineInfo);
					JObject jobject2 = (JObject)jcontainer;
					JProperty jproperty2 = jobject2.Property(name);
					if (jproperty2 == null)
					{
						jcontainer.Add(jproperty);
					}
					else
					{
						jproperty2.Replace(jproperty);
					}
					jcontainer = jproperty;
					goto IL_224;
				}
				case JsonToken.Comment:
				{
					JValue jvalue = JValue.CreateComment(r.Value.ToString());
					jvalue.SetLineInfo(lineInfo);
					jcontainer.Add(jvalue);
					goto IL_224;
				}
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
				{
					JValue jvalue = new JValue(r.Value);
					jvalue.SetLineInfo(lineInfo);
					jcontainer.Add(jvalue);
					goto IL_224;
				}
				case JsonToken.Null:
				{
					JValue jvalue = new JValue(null, JTokenType.Null);
					jvalue.SetLineInfo(lineInfo);
					jcontainer.Add(jvalue);
					goto IL_224;
				}
				case JsonToken.Undefined:
				{
					JValue jvalue = new JValue(null, JTokenType.Undefined);
					jvalue.SetLineInfo(lineInfo);
					jcontainer.Add(jvalue);
					goto IL_224;
				}
				case JsonToken.EndObject:
					if (jcontainer == this)
					{
						return;
					}
					jcontainer = jcontainer.Parent;
					goto IL_224;
				case JsonToken.EndArray:
					if (jcontainer == this)
					{
						return;
					}
					jcontainer = jcontainer.Parent;
					goto IL_224;
				case JsonToken.EndConstructor:
					if (jcontainer == this)
					{
						return;
					}
					jcontainer = jcontainer.Parent;
					goto IL_224;
				}
				goto Block_4;
				IL_224:
				if (!r.Read())
				{
					return;
				}
			}
			return;
			Block_4:
			throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				r.TokenType
			}));
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x000105B0 File Offset: 0x0000E7B0
		internal int ContentsHashCode()
		{
			int num = 0;
			foreach (JToken jtoken in this.Children())
			{
				num ^= jtoken.GetDeepHashCode();
			}
			return num;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00010608 File Offset: 0x0000E808
		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return string.Empty;
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00010610 File Offset: 0x0000E810
		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			ICustomTypeDescriptor customTypeDescriptor = this.First as ICustomTypeDescriptor;
			if (customTypeDescriptor != null)
			{
				return customTypeDescriptor.GetProperties();
			}
			return null;
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x00010634 File Offset: 0x0000E834
		int IList<JToken>.IndexOf(JToken item)
		{
			return this.IndexOfItem(item);
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x0001063D File Offset: 0x0000E83D
		void IList<JToken>.Insert(int index, JToken item)
		{
			this.InsertItem(index, item);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x00010647 File Offset: 0x0000E847
		void IList<JToken>.RemoveAt(int index)
		{
			this.RemoveItemAt(index);
		}

		// Token: 0x170000DB RID: 219
		JToken IList<JToken>.this[int index]
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

		// Token: 0x06000482 RID: 1154 RVA: 0x00010663 File Offset: 0x0000E863
		void ICollection<JToken>.Add(JToken item)
		{
			this.Add(item);
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x0001066C File Offset: 0x0000E86C
		void ICollection<JToken>.Clear()
		{
			this.ClearItems();
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00010674 File Offset: 0x0000E874
		bool ICollection<JToken>.Contains(JToken item)
		{
			return this.ContainsItem(item);
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x0001067D File Offset: 0x0000E87D
		void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
		{
			this.CopyItemsTo(array, arrayIndex);
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x00010687 File Offset: 0x0000E887
		int ICollection<JToken>.Count
		{
			get
			{
				return this.CountItems();
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000487 RID: 1159 RVA: 0x0001068F File Offset: 0x0000E88F
		bool ICollection<JToken>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00010692 File Offset: 0x0000E892
		bool ICollection<JToken>.Remove(JToken item)
		{
			return this.RemoveItem(item);
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x0001069B File Offset: 0x0000E89B
		private JToken EnsureValue(object value)
		{
			if (value == null)
			{
				return null;
			}
			if (value is JToken)
			{
				return (JToken)value;
			}
			throw new ArgumentException("Argument is not a JToken.");
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x000106BB File Offset: 0x0000E8BB
		int IList.Add(object value)
		{
			this.Add(this.EnsureValue(value));
			return this.CountItems() - 1;
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x000106D2 File Offset: 0x0000E8D2
		void IList.Clear()
		{
			this.ClearItems();
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x000106DA File Offset: 0x0000E8DA
		bool IList.Contains(object value)
		{
			return this.ContainsItem(this.EnsureValue(value));
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x000106E9 File Offset: 0x0000E8E9
		int IList.IndexOf(object value)
		{
			return this.IndexOfItem(this.EnsureValue(value));
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x000106F8 File Offset: 0x0000E8F8
		void IList.Insert(int index, object value)
		{
			this.InsertItem(index, this.EnsureValue(value));
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x00010708 File Offset: 0x0000E908
		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x0001070B File Offset: 0x0000E90B
		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0001070E File Offset: 0x0000E90E
		void IList.Remove(object value)
		{
			this.RemoveItem(this.EnsureValue(value));
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x0001071E File Offset: 0x0000E91E
		void IList.RemoveAt(int index)
		{
			this.RemoveItemAt(index);
		}

		// Token: 0x170000E0 RID: 224
		object IList.this[int index]
		{
			get
			{
				return this.GetItem(index);
			}
			set
			{
				this.SetItem(index, this.EnsureValue(value));
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00010740 File Offset: 0x0000E940
		void ICollection.CopyTo(Array array, int index)
		{
			this.CopyItemsTo(array, index);
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000496 RID: 1174 RVA: 0x0001074A File Offset: 0x0000E94A
		int ICollection.Count
		{
			get
			{
				return this.CountItems();
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x00010752 File Offset: 0x0000E952
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000498 RID: 1176 RVA: 0x00010755 File Offset: 0x0000E955
		object ICollection.SyncRoot
		{
			get
			{
				if (this._syncRoot == null)
				{
					Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
				}
				return this._syncRoot;
			}
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00010777 File Offset: 0x0000E977
		void IBindingList.AddIndex(PropertyDescriptor property)
		{
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001077C File Offset: 0x0000E97C
		object IBindingList.AddNew()
		{
			AddingNewEventArgs addingNewEventArgs = new AddingNewEventArgs();
			this.OnAddingNew(addingNewEventArgs);
			if (addingNewEventArgs.NewObject == null)
			{
				throw new Exception("Could not determine new value to add to '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
			if (!(addingNewEventArgs.NewObject is JToken))
			{
				throw new Exception("New item to be added to collection must be compatible with {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					typeof(JToken)
				}));
			}
			JToken jtoken = (JToken)addingNewEventArgs.NewObject;
			this.Add(jtoken);
			return jtoken;
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600049B RID: 1179 RVA: 0x0001080F File Offset: 0x0000EA0F
		bool IBindingList.AllowEdit
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x0600049C RID: 1180 RVA: 0x00010812 File Offset: 0x0000EA12
		bool IBindingList.AllowNew
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x0600049D RID: 1181 RVA: 0x00010815 File Offset: 0x0000EA15
		bool IBindingList.AllowRemove
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00010818 File Offset: 0x0000EA18
		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0001081F File Offset: 0x0000EA1F
		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060004A0 RID: 1184 RVA: 0x00010826 File Offset: 0x0000EA26
		bool IBindingList.IsSorted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00010829 File Offset: 0x0000EA29
		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x0001082B File Offset: 0x0000EA2B
		void IBindingList.RemoveSort()
		{
			throw new NotSupportedException();
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x00010832 File Offset: 0x0000EA32
		ListSortDirection IBindingList.SortDirection
		{
			get
			{
				return ListSortDirection.Ascending;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x00010835 File Offset: 0x0000EA35
		PropertyDescriptor IBindingList.SortProperty
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060004A5 RID: 1189 RVA: 0x00010838 File Offset: 0x0000EA38
		bool IBindingList.SupportsChangeNotification
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x0001083B File Offset: 0x0000EA3B
		bool IBindingList.SupportsSearching
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060004A7 RID: 1191 RVA: 0x0001083E File Offset: 0x0000EA3E
		bool IBindingList.SupportsSorting
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000137 RID: 311
		private JToken _content;

		// Token: 0x04000138 RID: 312
		private object _syncRoot;

		// Token: 0x04000139 RID: 313
		private bool _busy;
	}
}

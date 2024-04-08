using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000BB RID: 187
	internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IWrappedDictionary, IDictionary, ICollection, IEnumerable
	{
		// Token: 0x06000813 RID: 2067 RVA: 0x0001D5B6 File Offset: 0x0001B7B6
		public DictionaryWrapper(IDictionary dictionary)
		{
			ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
			this._dictionary = dictionary;
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x0001D5D0 File Offset: 0x0001B7D0
		public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
		{
			ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
			this._genericDictionary = dictionary;
		}

		// Token: 0x06000815 RID: 2069 RVA: 0x0001D5EA File Offset: 0x0001B7EA
		public void Add(TKey key, TValue value)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.Add(key, value);
				return;
			}
			this._dictionary.Add(key, value);
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x0001D619 File Offset: 0x0001B819
		public bool ContainsKey(TKey key)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.ContainsKey(key);
			}
			return this._dictionary.Contains(key);
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000817 RID: 2071 RVA: 0x0001D641 File Offset: 0x0001B841
		public ICollection<TKey> Keys
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.Keys;
				}
				return this._dictionary.Keys.Cast<TKey>().ToList<TKey>();
			}
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x0001D66C File Offset: 0x0001B86C
		public bool Remove(TKey key)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.Remove(key);
			}
			if (this._dictionary.Contains(key))
			{
				this._dictionary.Remove(key);
				return true;
			}
			return false;
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x0001D6AC File Offset: 0x0001B8AC
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.TryGetValue(key, out value);
			}
			if (!this._dictionary.Contains(key))
			{
				value = default(TValue);
				return false;
			}
			value = (TValue)((object)this._dictionary[key]);
			return true;
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x0600081A RID: 2074 RVA: 0x0001D708 File Offset: 0x0001B908
		public ICollection<TValue> Values
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.Values;
				}
				return this._dictionary.Values.Cast<TValue>().ToList<TValue>();
			}
		}

		// Token: 0x1700018C RID: 396
		public TValue this[TKey key]
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary[key];
				}
				return (TValue)((object)this._dictionary[key]);
			}
			set
			{
				if (this._genericDictionary != null)
				{
					this._genericDictionary[key] = value;
					return;
				}
				this._dictionary[key] = value;
			}
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x0001D78F File Offset: 0x0001B98F
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.Add(item);
				return;
			}
			((IList)this._dictionary).Add(item);
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x0001D7BD File Offset: 0x0001B9BD
		public void Clear()
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.Clear();
				return;
			}
			this._dictionary.Clear();
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0001D7DE File Offset: 0x0001B9DE
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.Contains(item);
			}
			return ((IList)this._dictionary).Contains(item);
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x0001D80C File Offset: 0x0001BA0C
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.CopyTo(array, arrayIndex);
				return;
			}
			foreach (object obj in this._dictionary)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)((object)dictionaryEntry.Key), (TValue)((object)dictionaryEntry.Value));
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000821 RID: 2081 RVA: 0x0001D8A4 File Offset: 0x0001BAA4
		public int Count
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.Count;
				}
				return this._dictionary.Count;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000822 RID: 2082 RVA: 0x0001D8C5 File Offset: 0x0001BAC5
		public bool IsReadOnly
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.IsReadOnly;
				}
				return this._dictionary.IsReadOnly;
			}
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x0001D8E8 File Offset: 0x0001BAE8
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.Remove(item);
			}
			if (!this._dictionary.Contains(item.Key))
			{
				return true;
			}
			object objA = this._dictionary[item.Key];
			if (object.Equals(objA, item.Value))
			{
				this._dictionary.Remove(item.Key);
				return true;
			}
			return false;
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x0001D98C File Offset: 0x0001BB8C
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.GetEnumerator();
			}
			return (from DictionaryEntry de in this._dictionary
			select new KeyValuePair<TKey, TValue>((TKey)((object)de.Key), (TValue)((object)de.Value))).GetEnumerator();
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x0001D9DF File Offset: 0x0001BBDF
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x0001D9E7 File Offset: 0x0001BBE7
		void IDictionary.Add(object key, object value)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.Add((TKey)((object)key), (TValue)((object)value));
				return;
			}
			this._dictionary.Add(key, value);
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x0001DA16 File Offset: 0x0001BC16
		bool IDictionary.Contains(object key)
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.ContainsKey((TKey)((object)key));
			}
			return this._dictionary.Contains(key);
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x0001DA3E File Offset: 0x0001BC3E
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			if (this._genericDictionary != null)
			{
				return new DictionaryWrapper<TKey, TValue>.DictionaryEnumerator<TKey, TValue>(this._genericDictionary.GetEnumerator());
			}
			return this._dictionary.GetEnumerator();
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000829 RID: 2089 RVA: 0x0001DA69 File Offset: 0x0001BC69
		bool IDictionary.IsFixedSize
		{
			get
			{
				return this._genericDictionary == null && this._dictionary.IsFixedSize;
			}
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x0600082A RID: 2090 RVA: 0x0001DA80 File Offset: 0x0001BC80
		ICollection IDictionary.Keys
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.Keys.ToList<TKey>();
				}
				return this._dictionary.Keys;
			}
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x0001DAA6 File Offset: 0x0001BCA6
		public void Remove(object key)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.Remove((TKey)((object)key));
				return;
			}
			this._dictionary.Remove(key);
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x0600082C RID: 2092 RVA: 0x0001DACF File Offset: 0x0001BCCF
		ICollection IDictionary.Values
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary.Values.ToList<TValue>();
				}
				return this._dictionary.Values;
			}
		}

		// Token: 0x17000192 RID: 402
		object IDictionary.this[object key]
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary[(TKey)((object)key)];
				}
				return this._dictionary[key];
			}
			set
			{
				if (this._genericDictionary != null)
				{
					this._genericDictionary[(TKey)((object)key)] = (TValue)((object)value);
					return;
				}
				this._dictionary[key] = value;
			}
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x0001DB51 File Offset: 0x0001BD51
		void ICollection.CopyTo(Array array, int index)
		{
			if (this._genericDictionary != null)
			{
				this._genericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
				return;
			}
			this._dictionary.CopyTo(array, index);
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000830 RID: 2096 RVA: 0x0001DB7B File Offset: 0x0001BD7B
		bool ICollection.IsSynchronized
		{
			get
			{
				return this._genericDictionary == null && this._dictionary.IsSynchronized;
			}
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000831 RID: 2097 RVA: 0x0001DB92 File Offset: 0x0001BD92
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

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000832 RID: 2098 RVA: 0x0001DBB4 File Offset: 0x0001BDB4
		public object UnderlyingDictionary
		{
			get
			{
				if (this._genericDictionary != null)
				{
					return this._genericDictionary;
				}
				return this._dictionary;
			}
		}

		// Token: 0x0400027D RID: 637
		private readonly IDictionary _dictionary;

		// Token: 0x0400027E RID: 638
		private readonly IDictionary<TKey, TValue> _genericDictionary;

		// Token: 0x0400027F RID: 639
		private object _syncRoot;

		// Token: 0x020000BC RID: 188
		private struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator, IEnumerator
		{
			// Token: 0x06000834 RID: 2100 RVA: 0x0001DBCB File Offset: 0x0001BDCB
			public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
			{
				ValidationUtils.ArgumentNotNull(e, "e");
				this._e = e;
			}

			// Token: 0x17000196 RID: 406
			// (get) Token: 0x06000835 RID: 2101 RVA: 0x0001DBDF File Offset: 0x0001BDDF
			public DictionaryEntry Entry
			{
				get
				{
					return (DictionaryEntry)this.Current;
				}
			}

			// Token: 0x17000197 RID: 407
			// (get) Token: 0x06000836 RID: 2102 RVA: 0x0001DBEC File Offset: 0x0001BDEC
			public object Key
			{
				get
				{
					return this.Entry.Key;
				}
			}

			// Token: 0x17000198 RID: 408
			// (get) Token: 0x06000837 RID: 2103 RVA: 0x0001DC08 File Offset: 0x0001BE08
			public object Value
			{
				get
				{
					return this.Entry.Value;
				}
			}

			// Token: 0x17000199 RID: 409
			// (get) Token: 0x06000838 RID: 2104 RVA: 0x0001DC24 File Offset: 0x0001BE24
			public object Current
			{
				get
				{
					KeyValuePair<TEnumeratorKey, TEnumeratorValue> keyValuePair = this._e.Current;
					object key = keyValuePair.Key;
					KeyValuePair<TEnumeratorKey, TEnumeratorValue> keyValuePair2 = this._e.Current;
					return new DictionaryEntry(key, keyValuePair2.Value);
				}
			}

			// Token: 0x06000839 RID: 2105 RVA: 0x0001DC6B File Offset: 0x0001BE6B
			public bool MoveNext()
			{
				return this._e.MoveNext();
			}

			// Token: 0x0600083A RID: 2106 RVA: 0x0001DC78 File Offset: 0x0001BE78
			public void Reset()
			{
				this._e.Reset();
			}

			// Token: 0x04000281 RID: 641
			private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;
		}
	}
}

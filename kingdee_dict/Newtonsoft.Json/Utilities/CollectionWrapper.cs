using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B8 RID: 184
	internal class CollectionWrapper<T> : ICollection<T>, IEnumerable<T>, IWrappedCollection, IList, ICollection, IEnumerable
	{
		// Token: 0x060007F6 RID: 2038 RVA: 0x0001D178 File Offset: 0x0001B378
		public CollectionWrapper(IList list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			if (list is ICollection<T>)
			{
				this._genericCollection = (ICollection<T>)list;
				return;
			}
			this._list = list;
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x0001D1A7 File Offset: 0x0001B3A7
		public CollectionWrapper(ICollection<T> list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			this._genericCollection = list;
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x0001D1C1 File Offset: 0x0001B3C1
		public virtual void Add(T item)
		{
			if (this._genericCollection != null)
			{
				this._genericCollection.Add(item);
				return;
			}
			this._list.Add(item);
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x0001D1EA File Offset: 0x0001B3EA
		public virtual void Clear()
		{
			if (this._genericCollection != null)
			{
				this._genericCollection.Clear();
				return;
			}
			this._list.Clear();
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x0001D20B File Offset: 0x0001B40B
		public virtual bool Contains(T item)
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.Contains(item);
			}
			return this._list.Contains(item);
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x0001D233 File Offset: 0x0001B433
		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			if (this._genericCollection != null)
			{
				this._genericCollection.CopyTo(array, arrayIndex);
				return;
			}
			this._list.CopyTo(array, arrayIndex);
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x060007FC RID: 2044 RVA: 0x0001D258 File Offset: 0x0001B458
		public virtual int Count
		{
			get
			{
				if (this._genericCollection != null)
				{
					return this._genericCollection.Count;
				}
				return this._list.Count;
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060007FD RID: 2045 RVA: 0x0001D279 File Offset: 0x0001B479
		public virtual bool IsReadOnly
		{
			get
			{
				if (this._genericCollection != null)
				{
					return this._genericCollection.IsReadOnly;
				}
				return this._list.IsReadOnly;
			}
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x0001D29C File Offset: 0x0001B49C
		public virtual bool Remove(T item)
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.Remove(item);
			}
			bool flag = this._list.Contains(item);
			if (flag)
			{
				this._list.Remove(item);
			}
			return flag;
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x0001D2E5 File Offset: 0x0001B4E5
		public virtual IEnumerator<T> GetEnumerator()
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.GetEnumerator();
			}
			return this._list.Cast<T>().GetEnumerator();
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x0001D30B File Offset: 0x0001B50B
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.GetEnumerator();
			}
			return this._list.GetEnumerator();
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x0001D32C File Offset: 0x0001B52C
		int IList.Add(object value)
		{
			CollectionWrapper<T>.VerifyValueType(value);
			this.Add((T)((object)value));
			return this.Count - 1;
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x0001D348 File Offset: 0x0001B548
		bool IList.Contains(object value)
		{
			return CollectionWrapper<T>.IsCompatibleObject(value) && this.Contains((T)((object)value));
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x0001D360 File Offset: 0x0001B560
		int IList.IndexOf(object value)
		{
			if (this._genericCollection != null)
			{
				throw new Exception("Wrapped ICollection<T> does not support IndexOf.");
			}
			if (CollectionWrapper<T>.IsCompatibleObject(value))
			{
				return this._list.IndexOf((T)((object)value));
			}
			return -1;
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x0001D395 File Offset: 0x0001B595
		void IList.RemoveAt(int index)
		{
			if (this._genericCollection != null)
			{
				throw new Exception("Wrapped ICollection<T> does not support RemoveAt.");
			}
			this._list.RemoveAt(index);
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x0001D3B6 File Offset: 0x0001B5B6
		void IList.Insert(int index, object value)
		{
			if (this._genericCollection != null)
			{
				throw new Exception("Wrapped ICollection<T> does not support Insert.");
			}
			CollectionWrapper<T>.VerifyValueType(value);
			this._list.Insert(index, (T)((object)value));
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000806 RID: 2054 RVA: 0x0001D3E8 File Offset: 0x0001B5E8
		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x0001D3EB File Offset: 0x0001B5EB
		void IList.Remove(object value)
		{
			if (CollectionWrapper<T>.IsCompatibleObject(value))
			{
				this.Remove((T)((object)value));
			}
		}

		// Token: 0x17000185 RID: 389
		object IList.this[int index]
		{
			get
			{
				if (this._genericCollection != null)
				{
					throw new Exception("Wrapped ICollection<T> does not support indexer.");
				}
				return this._list[index];
			}
			set
			{
				if (this._genericCollection != null)
				{
					throw new Exception("Wrapped ICollection<T> does not support indexer.");
				}
				CollectionWrapper<T>.VerifyValueType(value);
				this._list[index] = (T)((object)value);
			}
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x0001D455 File Offset: 0x0001B655
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			this.CopyTo((T[])array, arrayIndex);
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x0600080B RID: 2059 RVA: 0x0001D464 File Offset: 0x0001B664
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x0600080C RID: 2060 RVA: 0x0001D467 File Offset: 0x0001B667
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

		// Token: 0x0600080D RID: 2061 RVA: 0x0001D48C File Offset: 0x0001B68C
		private static void VerifyValueType(object value)
		{
			if (!CollectionWrapper<T>.IsCompatibleObject(value))
			{
				throw new ArgumentException("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					value,
					typeof(T)
				}), "value");
			}
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x0001D4D4 File Offset: 0x0001B6D4
		private static bool IsCompatibleObject(object value)
		{
			return value is T || (value == null && (!typeof(T).IsValueType || ReflectionUtils.IsNullableType(typeof(T))));
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x0600080F RID: 2063 RVA: 0x0001D506 File Offset: 0x0001B706
		public object UnderlyingCollection
		{
			get
			{
				if (this._genericCollection != null)
				{
					return this._genericCollection;
				}
				return this._list;
			}
		}

		// Token: 0x0400027A RID: 634
		private readonly IList _list;

		// Token: 0x0400027B RID: 635
		private readonly ICollection<T> _genericCollection;

		// Token: 0x0400027C RID: 636
		private object _syncRoot;
	}
}

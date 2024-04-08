using System;
using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000C7 RID: 199
	internal class ListWrapper<T> : CollectionWrapper<T>, IList<T>, ICollection<T>, IEnumerable<T>, IWrappedList, IList, ICollection, IEnumerable
	{
		// Token: 0x06000883 RID: 2179 RVA: 0x0001F338 File Offset: 0x0001D538
		public ListWrapper(IList list) : base(list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			if (list is IList<T>)
			{
				this._genericList = (IList<T>)list;
			}
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x0001F360 File Offset: 0x0001D560
		public ListWrapper(IList<T> list) : base(list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			this._genericList = list;
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x0001F37B File Offset: 0x0001D57B
		public int IndexOf(T item)
		{
			if (this._genericList != null)
			{
				return this._genericList.IndexOf(item);
			}
			return ((IList)this).IndexOf(item);
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x0001F39E File Offset: 0x0001D59E
		public void Insert(int index, T item)
		{
			if (this._genericList != null)
			{
				this._genericList.Insert(index, item);
				return;
			}
			((IList)this).Insert(index, item);
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0001F3C3 File Offset: 0x0001D5C3
		public void RemoveAt(int index)
		{
			if (this._genericList != null)
			{
				this._genericList.RemoveAt(index);
				return;
			}
			((IList)this).RemoveAt(index);
		}

		// Token: 0x1700019E RID: 414
		public T this[int index]
		{
			get
			{
				if (this._genericList != null)
				{
					return this._genericList[index];
				}
				return (T)((object)((IList)this)[index]);
			}
			set
			{
				if (this._genericList != null)
				{
					this._genericList[index] = value;
					return;
				}
				((IList)this)[index] = value;
			}
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x0001F429 File Offset: 0x0001D629
		public override void Add(T item)
		{
			if (this._genericList != null)
			{
				this._genericList.Add(item);
				return;
			}
			base.Add(item);
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x0001F447 File Offset: 0x0001D647
		public override void Clear()
		{
			if (this._genericList != null)
			{
				this._genericList.Clear();
				return;
			}
			base.Clear();
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x0001F463 File Offset: 0x0001D663
		public override bool Contains(T item)
		{
			if (this._genericList != null)
			{
				return this._genericList.Contains(item);
			}
			return base.Contains(item);
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x0001F481 File Offset: 0x0001D681
		public override void CopyTo(T[] array, int arrayIndex)
		{
			if (this._genericList != null)
			{
				this._genericList.CopyTo(array, arrayIndex);
				return;
			}
			base.CopyTo(array, arrayIndex);
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x0600088E RID: 2190 RVA: 0x0001F4A1 File Offset: 0x0001D6A1
		public override int Count
		{
			get
			{
				if (this._genericList != null)
				{
					return this._genericList.Count;
				}
				return base.Count;
			}
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x0001F4BD File Offset: 0x0001D6BD
		public override bool IsReadOnly
		{
			get
			{
				if (this._genericList != null)
				{
					return this._genericList.IsReadOnly;
				}
				return base.IsReadOnly;
			}
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x0001F4DC File Offset: 0x0001D6DC
		public override bool Remove(T item)
		{
			if (this._genericList != null)
			{
				return this._genericList.Remove(item);
			}
			bool flag = base.Contains(item);
			if (flag)
			{
				base.Remove(item);
			}
			return flag;
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x0001F512 File Offset: 0x0001D712
		public override IEnumerator<T> GetEnumerator()
		{
			if (this._genericList != null)
			{
				return this._genericList.GetEnumerator();
			}
			return base.GetEnumerator();
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000892 RID: 2194 RVA: 0x0001F52E File Offset: 0x0001D72E
		public object UnderlyingList
		{
			get
			{
				if (this._genericList != null)
				{
					return this._genericList;
				}
				return base.UnderlyingCollection;
			}
		}

		// Token: 0x040002A7 RID: 679
		private readonly IList<T> _genericList;
	}
}

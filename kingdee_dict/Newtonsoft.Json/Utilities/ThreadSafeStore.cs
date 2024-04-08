using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B2 RID: 178
	internal class ThreadSafeStore<TKey, TValue>
	{
		// Token: 0x060007D3 RID: 2003 RVA: 0x0001C6FE File Offset: 0x0001A8FE
		public ThreadSafeStore(Func<TKey, TValue> creator)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}
			this._creator = creator;
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x0001C728 File Offset: 0x0001A928
		public TValue Get(TKey key)
		{
			if (this._store == null)
			{
				return this.AddValue(key);
			}
			TValue result;
			if (!this._store.TryGetValue(key, out result))
			{
				return this.AddValue(key);
			}
			return result;
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x0001C760 File Offset: 0x0001A960
		private TValue AddValue(TKey key)
		{
			TValue tvalue = this._creator(key);
			TValue result2;
			lock (this._lock)
			{
				if (this._store == null)
				{
					this._store = new Dictionary<TKey, TValue>();
					this._store[key] = tvalue;
				}
				else
				{
					TValue result;
					if (this._store.TryGetValue(key, out result))
					{
						return result;
					}
					Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this._store);
					dictionary[key] = tvalue;
					this._store = dictionary;
				}
				result2 = tvalue;
			}
			return result2;
		}

		// Token: 0x0400026C RID: 620
		private readonly object _lock = new object();

		// Token: 0x0400026D RID: 621
		private Dictionary<TKey, TValue> _store;

		// Token: 0x0400026E RID: 622
		private readonly Func<TKey, TValue> _creator;
	}
}

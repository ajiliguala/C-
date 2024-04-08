using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B4 RID: 180
	internal class BidirectionalDictionary<TFirst, TSecond>
	{
		// Token: 0x060007D6 RID: 2006 RVA: 0x0001C800 File Offset: 0x0001AA00
		public BidirectionalDictionary() : this(EqualityComparer<TFirst>.Default, EqualityComparer<TSecond>.Default)
		{
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x0001C812 File Offset: 0x0001AA12
		public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
		{
			this._firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
			this._secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x0001C834 File Offset: 0x0001AA34
		public void Add(TFirst first, TSecond second)
		{
			if (this._firstToSecond.ContainsKey(first) || this._secondToFirst.ContainsKey(second))
			{
				throw new ArgumentException("Duplicate first or second");
			}
			this._firstToSecond.Add(first, second);
			this._secondToFirst.Add(second, first);
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x0001C882 File Offset: 0x0001AA82
		public bool TryGetByFirst(TFirst first, out TSecond second)
		{
			return this._firstToSecond.TryGetValue(first, out second);
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x0001C891 File Offset: 0x0001AA91
		public bool TryGetBySecond(TSecond second, out TFirst first)
		{
			return this._secondToFirst.TryGetValue(second, out first);
		}

		// Token: 0x04000275 RID: 629
		private readonly IDictionary<TFirst, TSecond> _firstToSecond;

		// Token: 0x04000276 RID: 630
		private readonly IDictionary<TSecond, TFirst> _secondToFirst;
	}
}

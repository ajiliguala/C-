using System;
using System.Collections;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B7 RID: 183
	internal interface IWrappedCollection : IList, ICollection, IEnumerable
	{
		// Token: 0x17000181 RID: 385
		// (get) Token: 0x060007F5 RID: 2037
		object UnderlyingCollection { get; }
	}
}

using System;
using System.Collections;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000BA RID: 186
	internal interface IWrappedDictionary : IDictionary, ICollection, IEnumerable
	{
		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000812 RID: 2066
		object UnderlyingDictionary { get; }
	}
}

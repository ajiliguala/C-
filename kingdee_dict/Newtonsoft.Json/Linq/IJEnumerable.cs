using System;
using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000026 RID: 38
	public interface IJEnumerable<out T> : IEnumerable<T>, IEnumerable where T : JToken
	{
		// Token: 0x17000025 RID: 37
		IJEnumerable<JToken> this[object key]
		{
			get;
		}
	}
}

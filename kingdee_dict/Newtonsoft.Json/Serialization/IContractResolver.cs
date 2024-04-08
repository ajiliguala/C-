using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200007D RID: 125
	public interface IContractResolver
	{
		// Token: 0x060005D8 RID: 1496
		JsonContract ResolveContract(Type type);
	}
}

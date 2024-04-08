using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200002E RID: 46
	public class JsonDynamicContract : JsonContract
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060001F0 RID: 496 RVA: 0x000081DE File Offset: 0x000063DE
		// (set) Token: 0x060001F1 RID: 497 RVA: 0x000081E6 File Offset: 0x000063E6
		public JsonPropertyCollection Properties { get; private set; }

		// Token: 0x060001F2 RID: 498 RVA: 0x000081EF File Offset: 0x000063EF
		public JsonDynamicContract(Type underlyingType) : base(underlyingType)
		{
			this.Properties = new JsonPropertyCollection(base.UnderlyingType);
		}
	}
}

using System;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000091 RID: 145
	public class JsonObjectContract : JsonContract
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x00016FC2 File Offset: 0x000151C2
		// (set) Token: 0x060006C8 RID: 1736 RVA: 0x00016FCA File Offset: 0x000151CA
		public MemberSerialization MemberSerialization { get; set; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x00016FD3 File Offset: 0x000151D3
		// (set) Token: 0x060006CA RID: 1738 RVA: 0x00016FDB File Offset: 0x000151DB
		public JsonPropertyCollection Properties { get; private set; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060006CB RID: 1739 RVA: 0x00016FE4 File Offset: 0x000151E4
		// (set) Token: 0x060006CC RID: 1740 RVA: 0x00016FEC File Offset: 0x000151EC
		public ConstructorInfo OverrideConstructor { get; set; }

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060006CD RID: 1741 RVA: 0x00016FF5 File Offset: 0x000151F5
		// (set) Token: 0x060006CE RID: 1742 RVA: 0x00016FFD File Offset: 0x000151FD
		public ConstructorInfo ParametrizedConstructor { get; set; }

		// Token: 0x060006CF RID: 1743 RVA: 0x00017006 File Offset: 0x00015206
		public JsonObjectContract(Type underlyingType) : base(underlyingType)
		{
			this.Properties = new JsonPropertyCollection(base.UnderlyingType);
		}
	}
}

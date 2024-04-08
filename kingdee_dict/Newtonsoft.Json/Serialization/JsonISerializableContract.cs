using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000030 RID: 48
	public class JsonISerializableContract : JsonContract
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000206 RID: 518 RVA: 0x0000835D File Offset: 0x0000655D
		// (set) Token: 0x06000207 RID: 519 RVA: 0x00008365 File Offset: 0x00006565
		public ObjectConstructor<object> ISerializableCreator { get; set; }

		// Token: 0x06000208 RID: 520 RVA: 0x0000836E File Offset: 0x0000656E
		public JsonISerializableContract(Type underlyingType) : base(underlyingType)
		{
		}
	}
}

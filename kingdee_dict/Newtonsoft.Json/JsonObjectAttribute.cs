using System;

namespace Newtonsoft.Json
{
	// Token: 0x0200003F RID: 63
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class JsonObjectAttribute : JsonContainerAttribute
	{
		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600023F RID: 575 RVA: 0x000087A8 File Offset: 0x000069A8
		// (set) Token: 0x06000240 RID: 576 RVA: 0x000087B0 File Offset: 0x000069B0
		public MemberSerialization MemberSerialization
		{
			get
			{
				return this._memberSerialization;
			}
			set
			{
				this._memberSerialization = value;
			}
		}

		// Token: 0x06000241 RID: 577 RVA: 0x000087B9 File Offset: 0x000069B9
		public JsonObjectAttribute()
		{
		}

		// Token: 0x06000242 RID: 578 RVA: 0x000087C1 File Offset: 0x000069C1
		public JsonObjectAttribute(MemberSerialization memberSerialization)
		{
			this.MemberSerialization = memberSerialization;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x000087D0 File Offset: 0x000069D0
		public JsonObjectAttribute(string id) : base(id)
		{
		}

		// Token: 0x040000AF RID: 175
		private MemberSerialization _memberSerialization;
	}
}

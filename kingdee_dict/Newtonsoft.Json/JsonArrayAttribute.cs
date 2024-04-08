using System;

namespace Newtonsoft.Json
{
	// Token: 0x0200003C RID: 60
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class JsonArrayAttribute : JsonContainerAttribute
	{
		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000237 RID: 567 RVA: 0x000086FB File Offset: 0x000068FB
		// (set) Token: 0x06000238 RID: 568 RVA: 0x00008703 File Offset: 0x00006903
		public bool AllowNullItems
		{
			get
			{
				return this._allowNullItems;
			}
			set
			{
				this._allowNullItems = value;
			}
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000870C File Offset: 0x0000690C
		public JsonArrayAttribute()
		{
		}

		// Token: 0x0600023A RID: 570 RVA: 0x00008714 File Offset: 0x00006914
		public JsonArrayAttribute(bool allowNullItems)
		{
			this._allowNullItems = allowNullItems;
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00008723 File Offset: 0x00006923
		public JsonArrayAttribute(string id) : base(id)
		{
		}

		// Token: 0x040000AA RID: 170
		private bool _allowNullItems;
	}
}

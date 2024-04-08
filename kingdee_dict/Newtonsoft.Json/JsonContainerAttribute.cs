using System;

namespace Newtonsoft.Json
{
	// Token: 0x0200003B RID: 59
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public abstract class JsonContainerAttribute : Attribute
	{
		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600022D RID: 557 RVA: 0x0000867C File Offset: 0x0000687C
		// (set) Token: 0x0600022E RID: 558 RVA: 0x00008684 File Offset: 0x00006884
		public string Id { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600022F RID: 559 RVA: 0x0000868D File Offset: 0x0000688D
		// (set) Token: 0x06000230 RID: 560 RVA: 0x00008695 File Offset: 0x00006895
		public string Title { get; set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000231 RID: 561 RVA: 0x0000869E File Offset: 0x0000689E
		// (set) Token: 0x06000232 RID: 562 RVA: 0x000086A6 File Offset: 0x000068A6
		public string Description { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000233 RID: 563 RVA: 0x000086B0 File Offset: 0x000068B0
		// (set) Token: 0x06000234 RID: 564 RVA: 0x000086D6 File Offset: 0x000068D6
		public bool IsReference
		{
			get
			{
				return this._isReference ?? false;
			}
			set
			{
				this._isReference = new bool?(value);
			}
		}

		// Token: 0x06000235 RID: 565 RVA: 0x000086E4 File Offset: 0x000068E4
		protected JsonContainerAttribute()
		{
		}

		// Token: 0x06000236 RID: 566 RVA: 0x000086EC File Offset: 0x000068EC
		protected JsonContainerAttribute(string id)
		{
			this.Id = id;
		}

		// Token: 0x040000A6 RID: 166
		internal bool? _isReference;
	}
}

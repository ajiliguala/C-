using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000035 RID: 53
	public class ErrorEventArgs : EventArgs
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000210 RID: 528 RVA: 0x000084A8 File Offset: 0x000066A8
		// (set) Token: 0x06000211 RID: 529 RVA: 0x000084B0 File Offset: 0x000066B0
		public object CurrentObject { get; private set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000212 RID: 530 RVA: 0x000084B9 File Offset: 0x000066B9
		// (set) Token: 0x06000213 RID: 531 RVA: 0x000084C1 File Offset: 0x000066C1
		public ErrorContext ErrorContext { get; private set; }

		// Token: 0x06000214 RID: 532 RVA: 0x000084CA File Offset: 0x000066CA
		public ErrorEventArgs(object currentObject, ErrorContext errorContext)
		{
			this.CurrentObject = currentObject;
			this.ErrorContext = errorContext;
		}
	}
}

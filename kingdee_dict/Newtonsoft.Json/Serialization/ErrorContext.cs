using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000083 RID: 131
	public class ErrorContext
	{
		// Token: 0x0600060F RID: 1551 RVA: 0x00014C37 File Offset: 0x00012E37
		internal ErrorContext(object originalObject, object member, Exception error)
		{
			this.OriginalObject = originalObject;
			this.Member = member;
			this.Error = error;
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000610 RID: 1552 RVA: 0x00014C54 File Offset: 0x00012E54
		// (set) Token: 0x06000611 RID: 1553 RVA: 0x00014C5C File Offset: 0x00012E5C
		public Exception Error { get; private set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x00014C65 File Offset: 0x00012E65
		// (set) Token: 0x06000613 RID: 1555 RVA: 0x00014C6D File Offset: 0x00012E6D
		public object OriginalObject { get; private set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000614 RID: 1556 RVA: 0x00014C76 File Offset: 0x00012E76
		// (set) Token: 0x06000615 RID: 1557 RVA: 0x00014C7E File Offset: 0x00012E7E
		public object Member { get; private set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000616 RID: 1558 RVA: 0x00014C87 File Offset: 0x00012E87
		// (set) Token: 0x06000617 RID: 1559 RVA: 0x00014C8F File Offset: 0x00012E8F
		public bool Handled { get; set; }
	}
}

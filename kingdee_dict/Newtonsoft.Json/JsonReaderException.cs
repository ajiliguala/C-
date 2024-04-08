using System;

namespace Newtonsoft.Json
{
	// Token: 0x02000061 RID: 97
	public class JsonReaderException : Exception
	{
		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060003BD RID: 957 RVA: 0x0000DDD0 File Offset: 0x0000BFD0
		// (set) Token: 0x060003BE RID: 958 RVA: 0x0000DDD8 File Offset: 0x0000BFD8
		public int LineNumber { get; private set; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060003BF RID: 959 RVA: 0x0000DDE1 File Offset: 0x0000BFE1
		// (set) Token: 0x060003C0 RID: 960 RVA: 0x0000DDE9 File Offset: 0x0000BFE9
		public int LinePosition { get; private set; }

		// Token: 0x060003C1 RID: 961 RVA: 0x0000DDF2 File Offset: 0x0000BFF2
		public JsonReaderException()
		{
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0000DDFA File Offset: 0x0000BFFA
		public JsonReaderException(string message) : base(message)
		{
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0000DE03 File Offset: 0x0000C003
		public JsonReaderException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0000DE0D File Offset: 0x0000C00D
		internal JsonReaderException(string message, Exception innerException, int lineNumber, int linePosition) : base(message, innerException)
		{
			this.LineNumber = lineNumber;
			this.LinePosition = linePosition;
		}
	}
}

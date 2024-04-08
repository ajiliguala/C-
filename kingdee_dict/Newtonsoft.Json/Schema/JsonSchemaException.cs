using System;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x02000073 RID: 115
	public class JsonSchemaException : Exception
	{
		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000578 RID: 1400 RVA: 0x00012918 File Offset: 0x00010B18
		// (set) Token: 0x06000579 RID: 1401 RVA: 0x00012920 File Offset: 0x00010B20
		public int LineNumber { get; private set; }

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x00012929 File Offset: 0x00010B29
		// (set) Token: 0x0600057B RID: 1403 RVA: 0x00012931 File Offset: 0x00010B31
		public int LinePosition { get; private set; }

		// Token: 0x0600057C RID: 1404 RVA: 0x0001293A File Offset: 0x00010B3A
		public JsonSchemaException()
		{
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x00012942 File Offset: 0x00010B42
		public JsonSchemaException(string message) : base(message)
		{
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0001294B File Offset: 0x00010B4B
		public JsonSchemaException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x00012955 File Offset: 0x00010B55
		internal JsonSchemaException(string message, Exception innerException, int lineNumber, int linePosition) : base(message, innerException)
		{
			this.LineNumber = lineNumber;
			this.LinePosition = linePosition;
		}
	}
}

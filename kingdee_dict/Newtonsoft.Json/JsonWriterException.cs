using System;

namespace Newtonsoft.Json
{
	// Token: 0x02000060 RID: 96
	public class JsonWriterException : Exception
	{
		// Token: 0x060003BA RID: 954 RVA: 0x0000DDB5 File Offset: 0x0000BFB5
		public JsonWriterException()
		{
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0000DDBD File Offset: 0x0000BFBD
		public JsonWriterException(string message) : base(message)
		{
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0000DDC6 File Offset: 0x0000BFC6
		public JsonWriterException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

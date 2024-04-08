using System;

namespace Newtonsoft.Json
{
	// Token: 0x02000064 RID: 100
	public class JsonSerializationException : Exception
	{
		// Token: 0x06000406 RID: 1030 RVA: 0x0000E889 File Offset: 0x0000CA89
		public JsonSerializationException()
		{
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0000E891 File Offset: 0x0000CA91
		public JsonSerializationException(string message) : base(message)
		{
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0000E89A File Offset: 0x0000CA9A
		public JsonSerializationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

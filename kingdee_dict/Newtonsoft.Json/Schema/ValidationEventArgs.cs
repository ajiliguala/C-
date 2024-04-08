using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x0200007B RID: 123
	public class ValidationEventArgs : EventArgs
	{
		// Token: 0x060005D1 RID: 1489 RVA: 0x00013A9F File Offset: 0x00011C9F
		internal ValidationEventArgs(JsonSchemaException ex)
		{
			ValidationUtils.ArgumentNotNull(ex, "ex");
			this._ex = ex;
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060005D2 RID: 1490 RVA: 0x00013AB9 File Offset: 0x00011CB9
		public JsonSchemaException Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060005D3 RID: 1491 RVA: 0x00013AC1 File Offset: 0x00011CC1
		public string Message
		{
			get
			{
				return this._ex.Message;
			}
		}

		// Token: 0x04000187 RID: 391
		private readonly JsonSchemaException _ex;
	}
}

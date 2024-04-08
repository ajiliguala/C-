using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000092 RID: 146
	internal abstract class JsonSerializerInternalBase
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060006D0 RID: 1744 RVA: 0x00017020 File Offset: 0x00015220
		// (set) Token: 0x060006D1 RID: 1745 RVA: 0x00017028 File Offset: 0x00015228
		internal JsonSerializer Serializer { get; private set; }

		// Token: 0x060006D2 RID: 1746 RVA: 0x00017031 File Offset: 0x00015231
		protected JsonSerializerInternalBase(JsonSerializer serializer)
		{
			ValidationUtils.ArgumentNotNull(serializer, "serializer");
			this.Serializer = serializer;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0001704B File Offset: 0x0001524B
		protected ErrorContext GetErrorContext(object currentObject, object member, Exception error)
		{
			if (this._currentErrorContext == null)
			{
				this._currentErrorContext = new ErrorContext(currentObject, member, error);
			}
			if (this._currentErrorContext.Error != error)
			{
				throw new InvalidOperationException("Current error context error is different to requested error.");
			}
			return this._currentErrorContext;
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x00017082 File Offset: 0x00015282
		protected void ClearErrorContext()
		{
			if (this._currentErrorContext == null)
			{
				throw new InvalidOperationException("Could not clear error context. Error context is already null.");
			}
			this._currentErrorContext = null;
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x000170A0 File Offset: 0x000152A0
		protected bool IsErrorHandled(object currentObject, JsonContract contract, object keyValue, Exception ex)
		{
			ErrorContext errorContext = this.GetErrorContext(currentObject, keyValue, ex);
			contract.InvokeOnError(currentObject, this.Serializer.Context, errorContext);
			if (!errorContext.Handled)
			{
				this.Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));
			}
			return errorContext.Handled;
		}

		// Token: 0x04000221 RID: 545
		private ErrorContext _currentErrorContext;
	}
}

using System;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B1 RID: 177
	// (Invoke) Token: 0x060007D0 RID: 2000
	internal delegate TResult MethodCall<T, TResult>(T target, params object[] args);
}

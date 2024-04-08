using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000033 RID: 51
	public interface IValueProvider
	{
		// Token: 0x0600020B RID: 523
		void SetValue(object target, object value);

		// Token: 0x0600020C RID: 524
		object GetValue(object target);
	}
}

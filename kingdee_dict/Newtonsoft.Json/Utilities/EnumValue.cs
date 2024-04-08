using System;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000BE RID: 190
	internal class EnumValue<T> where T : struct
	{
		// Token: 0x1700019A RID: 410
		// (get) Token: 0x0600084A RID: 2122 RVA: 0x0001E248 File Offset: 0x0001C448
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x0600084B RID: 2123 RVA: 0x0001E250 File Offset: 0x0001C450
		public T Value
		{
			get
			{
				return this._value;
			}
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0001E258 File Offset: 0x0001C458
		public EnumValue(string name, T value)
		{
			this._name = name;
			this._value = value;
		}

		// Token: 0x04000284 RID: 644
		private string _name;

		// Token: 0x04000285 RID: 645
		private T _value;
	}
}

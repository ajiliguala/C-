using System;
using System.Collections.ObjectModel;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000BF RID: 191
	internal class EnumValues<T> : KeyedCollection<string, EnumValue<T>> where T : struct
	{
		// Token: 0x0600084D RID: 2125 RVA: 0x0001E26E File Offset: 0x0001C46E
		protected override string GetKeyForItem(EnumValue<T> item)
		{
			return item.Name;
		}
	}
}

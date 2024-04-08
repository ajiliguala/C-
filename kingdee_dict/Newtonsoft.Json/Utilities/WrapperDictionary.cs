using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000AA RID: 170
	internal class WrapperDictionary
	{
		// Token: 0x060007A1 RID: 1953 RVA: 0x0001B8ED File Offset: 0x00019AED
		private static string GenerateKey(Type interfaceType, Type realObjectType)
		{
			return interfaceType.Name + "_" + realObjectType.Name;
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x0001B908 File Offset: 0x00019B08
		public Type GetType(Type interfaceType, Type realObjectType)
		{
			string key = WrapperDictionary.GenerateKey(interfaceType, realObjectType);
			if (this._wrapperTypes.ContainsKey(key))
			{
				return this._wrapperTypes[key];
			}
			return null;
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x0001B93C File Offset: 0x00019B3C
		public void SetType(Type interfaceType, Type realObjectType, Type wrapperType)
		{
			string key = WrapperDictionary.GenerateKey(interfaceType, realObjectType);
			if (this._wrapperTypes.ContainsKey(key))
			{
				this._wrapperTypes[key] = wrapperType;
				return;
			}
			this._wrapperTypes.Add(key, wrapperType);
		}

		// Token: 0x04000266 RID: 614
		private readonly Dictionary<string, Type> _wrapperTypes = new Dictionary<string, Type>();
	}
}

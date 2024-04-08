using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000043 RID: 67
	public class JTokenEqualityComparer : IEqualityComparer<JToken>
	{
		// Token: 0x0600029D RID: 669 RVA: 0x00009F64 File Offset: 0x00008164
		public bool Equals(JToken x, JToken y)
		{
			return JToken.DeepEquals(x, y);
		}

		// Token: 0x0600029E RID: 670 RVA: 0x00009F6D File Offset: 0x0000816D
		public int GetHashCode(JToken obj)
		{
			if (obj == null)
			{
				return 0;
			}
			return obj.GetDeepHashCode();
		}
	}
}

using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200007F RID: 127
	public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
	{
		// Token: 0x06000601 RID: 1537 RVA: 0x00014A5F File Offset: 0x00012C5F
		public CamelCasePropertyNamesContractResolver() : base(true)
		{
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00014A68 File Offset: 0x00012C68
		protected override string ResolvePropertyName(string propertyName)
		{
			return StringUtils.ToCamelCase(propertyName);
		}
	}
}

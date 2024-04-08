using System;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200009B RID: 155
	internal class LateBoundMetadataTypeAttribute : IMetadataTypeAttribute
	{
		// Token: 0x06000754 RID: 1876 RVA: 0x0001A0BF File Offset: 0x000182BF
		public LateBoundMetadataTypeAttribute(object attribute)
		{
			this._attribute = attribute;
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000755 RID: 1877 RVA: 0x0001A0CE File Offset: 0x000182CE
		public Type MetadataClassType
		{
			get
			{
				if (LateBoundMetadataTypeAttribute._metadataClassTypeProperty == null)
				{
					LateBoundMetadataTypeAttribute._metadataClassTypeProperty = this._attribute.GetType().GetProperty("MetadataClassType");
				}
				return (Type)ReflectionUtils.GetMemberValue(LateBoundMetadataTypeAttribute._metadataClassTypeProperty, this._attribute);
			}
		}

		// Token: 0x04000243 RID: 579
		private static PropertyInfo _metadataClassTypeProperty;

		// Token: 0x04000244 RID: 580
		private readonly object _attribute;
	}
}

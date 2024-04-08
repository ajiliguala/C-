using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x0200003E RID: 62
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class JsonConverterAttribute : Attribute
	{
		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600023C RID: 572 RVA: 0x0000872C File Offset: 0x0000692C
		public Type ConverterType
		{
			get
			{
				return this._converterType;
			}
		}

		// Token: 0x0600023D RID: 573 RVA: 0x00008734 File Offset: 0x00006934
		public JsonConverterAttribute(Type converterType)
		{
			if (converterType == null)
			{
				throw new ArgumentNullException("converterType");
			}
			this._converterType = converterType;
		}

		// Token: 0x0600023E RID: 574 RVA: 0x00008758 File Offset: 0x00006958
		internal static JsonConverter CreateJsonConverterInstance(Type converterType)
		{
			JsonConverter result;
			try
			{
				result = (JsonConverter)Activator.CreateInstance(converterType);
			}
			catch (Exception innerException)
			{
				throw new Exception("Error creating {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					converterType
				}), innerException);
			}
			return result;
		}

		// Token: 0x040000AE RID: 174
		private readonly Type _converterType;
	}
}

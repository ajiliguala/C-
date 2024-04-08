using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000087 RID: 135
	public class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
	{
		// Token: 0x0600064D RID: 1613 RVA: 0x00015215 File Offset: 0x00013415
		public JsonPropertyCollection(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			this._type = type;
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0001522F File Offset: 0x0001342F
		protected override string GetKeyForItem(JsonProperty item)
		{
			return item.PropertyName;
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x00015238 File Offset: 0x00013438
		public void AddProperty(JsonProperty property)
		{
			if (base.Contains(property.PropertyName))
			{
				if (property.Ignored)
				{
					return;
				}
				JsonProperty jsonProperty = base[property.PropertyName];
				if (!jsonProperty.Ignored)
				{
					throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						property.PropertyName,
						this._type
					}));
				}
				base.Remove(jsonProperty);
			}
			base.Add(property);
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x000152B0 File Offset: 0x000134B0
		public JsonProperty GetClosestMatchProperty(string propertyName)
		{
			JsonProperty property = this.GetProperty(propertyName, StringComparison.Ordinal);
			if (property == null)
			{
				property = this.GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);
			}
			return property;
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x000152D4 File Offset: 0x000134D4
		public JsonProperty GetProperty(string propertyName, StringComparison comparisonType)
		{
			foreach (JsonProperty jsonProperty in this)
			{
				if (string.Equals(propertyName, jsonProperty.PropertyName, comparisonType))
				{
					return jsonProperty;
				}
			}
			return null;
		}

		// Token: 0x040001BB RID: 443
		private readonly Type _type;
	}
}

using System;
using System.ComponentModel;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.ComponentModel
{
	// Token: 0x02000036 RID: 54
	public class JPropertyDescriptor : PropertyDescriptor
	{
		// Token: 0x06000215 RID: 533 RVA: 0x000084E0 File Offset: 0x000066E0
		public JPropertyDescriptor(string name, Type propertyType) : base(name, null)
		{
			ValidationUtils.ArgumentNotNull(name, "name");
			ValidationUtils.ArgumentNotNull(propertyType, "propertyType");
			this._propertyType = propertyType;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x00008507 File Offset: 0x00006707
		private static JObject CastInstance(object instance)
		{
			return (JObject)instance;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000850F File Offset: 0x0000670F
		public override bool CanResetValue(object component)
		{
			return false;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00008514 File Offset: 0x00006714
		public override object GetValue(object component)
		{
			return JPropertyDescriptor.CastInstance(component)[this.Name];
		}

		// Token: 0x06000219 RID: 537 RVA: 0x00008534 File Offset: 0x00006734
		public override void ResetValue(object component)
		{
		}

		// Token: 0x0600021A RID: 538 RVA: 0x00008538 File Offset: 0x00006738
		public override void SetValue(object component, object value)
		{
			JToken value2 = (value is JToken) ? ((JToken)value) : new JValue(value);
			JPropertyDescriptor.CastInstance(component)[this.Name] = value2;
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0000856E File Offset: 0x0000676E
		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600021C RID: 540 RVA: 0x00008571 File Offset: 0x00006771
		public override Type ComponentType
		{
			get
			{
				return typeof(JObject);
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600021D RID: 541 RVA: 0x0000857D File Offset: 0x0000677D
		public override bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600021E RID: 542 RVA: 0x00008580 File Offset: 0x00006780
		public override Type PropertyType
		{
			get
			{
				return this._propertyType;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600021F RID: 543 RVA: 0x00008588 File Offset: 0x00006788
		protected override int NameHashCode
		{
			get
			{
				return base.NameHashCode;
			}
		}

		// Token: 0x0400009E RID: 158
		private readonly Type _propertyType;
	}
}

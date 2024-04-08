using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200009C RID: 156
	public class ReflectionValueProvider : IValueProvider
	{
		// Token: 0x06000756 RID: 1878 RVA: 0x0001A10C File Offset: 0x0001830C
		public ReflectionValueProvider(MemberInfo memberInfo)
		{
			ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
			this._memberInfo = memberInfo;
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x0001A128 File Offset: 0x00018328
		public void SetValue(object target, object value)
		{
			try
			{
				ReflectionUtils.SetMemberValue(this._memberInfo, target, value);
			}
			catch (Exception innerException)
			{
				throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._memberInfo.Name,
					target.GetType()
				}), innerException);
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x0001A18C File Offset: 0x0001838C
		public object GetValue(object target)
		{
			object memberValue;
			try
			{
				memberValue = ReflectionUtils.GetMemberValue(this._memberInfo, target);
			}
			catch (Exception innerException)
			{
				throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._memberInfo.Name,
					target.GetType()
				}), innerException);
			}
			return memberValue;
		}

		// Token: 0x04000245 RID: 581
		private readonly MemberInfo _memberInfo;
	}
}

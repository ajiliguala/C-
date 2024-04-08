using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000034 RID: 52
	public class DynamicValueProvider : IValueProvider
	{
		// Token: 0x0600020D RID: 525 RVA: 0x00008389 File Offset: 0x00006589
		public DynamicValueProvider(MemberInfo memberInfo)
		{
			ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
			this._memberInfo = memberInfo;
		}

		// Token: 0x0600020E RID: 526 RVA: 0x000083A4 File Offset: 0x000065A4
		public void SetValue(object target, object value)
		{
			try
			{
				if (this._setter == null)
				{
					this._setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(this._memberInfo);
				}
				this._setter(target, value);
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

		// Token: 0x0600020F RID: 527 RVA: 0x00008424 File Offset: 0x00006624
		public object GetValue(object target)
		{
			object result;
			try
			{
				if (this._getter == null)
				{
					this._getter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(this._memberInfo);
				}
				result = this._getter(target);
			}
			catch (Exception innerException)
			{
				throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._memberInfo.Name,
					target.GetType()
				}), innerException);
			}
			return result;
		}

		// Token: 0x04000099 RID: 153
		private readonly MemberInfo _memberInfo;

		// Token: 0x0400009A RID: 154
		private Func<object, object> _getter;

		// Token: 0x0400009B RID: 155
		private Action<object, object> _setter;
	}
}

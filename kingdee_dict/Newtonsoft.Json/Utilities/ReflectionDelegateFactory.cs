using System;
using System.Globalization;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000AC RID: 172
	internal abstract class ReflectionDelegateFactory
	{
		// Token: 0x060007AC RID: 1964 RVA: 0x0001BD7C File Offset: 0x00019F7C
		public Func<T, object> CreateGet<T>(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				return this.CreateGet<T>(propertyInfo);
			}
			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
			{
				return this.CreateGet<T>(fieldInfo);
			}
			throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				memberInfo
			}));
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x0001BDDC File Offset: 0x00019FDC
		public Action<T, object> CreateSet<T>(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				return this.CreateSet<T>(propertyInfo);
			}
			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
			{
				return this.CreateSet<T>(fieldInfo);
			}
			throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				memberInfo
			}));
		}

		// Token: 0x060007AE RID: 1966
		public abstract MethodCall<T, object> CreateMethodCall<T>(MethodBase method);

		// Token: 0x060007AF RID: 1967
		public abstract Func<T> CreateDefaultConstructor<T>(Type type);

		// Token: 0x060007B0 RID: 1968
		public abstract Func<T, object> CreateGet<T>(PropertyInfo propertyInfo);

		// Token: 0x060007B1 RID: 1969
		public abstract Func<T, object> CreateGet<T>(FieldInfo fieldInfo);

		// Token: 0x060007B2 RID: 1970
		public abstract Action<T, object> CreateSet<T>(FieldInfo fieldInfo);

		// Token: 0x060007B3 RID: 1971
		public abstract Action<T, object> CreateSet<T>(PropertyInfo propertyInfo);
	}
}

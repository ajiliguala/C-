using System;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B0 RID: 176
	internal class LateBoundReflectionDelegateFactory : ReflectionDelegateFactory
	{
		// Token: 0x060007C7 RID: 1991 RVA: 0x0001C4A0 File Offset: 0x0001A6A0
		public override MethodCall<T, object> CreateMethodCall<T>(MethodBase method)
		{
			ValidationUtils.ArgumentNotNull(method, "method");
			ConstructorInfo c = method as ConstructorInfo;
			if (c != null)
			{
				return (T o, object[] a) => c.Invoke(a);
			}
			return (T o, object[] a) => method.Invoke(o, a);
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x0001C538 File Offset: 0x0001A738
		public override Func<T> CreateDefaultConstructor<T>(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			if (type.IsValueType)
			{
				return () => (T)((object)ReflectionUtils.CreateInstance(type, new object[0]));
			}
			ConstructorInfo constructorInfo = ReflectionUtils.GetDefaultConstructor(type, true);
			return () => (T)((object)constructorInfo.Invoke(null));
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x0001C5B8 File Offset: 0x0001A7B8
		public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
		{
			ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
			return (T o) => propertyInfo.GetValue(o, null);
		}

		// Token: 0x060007CA RID: 1994 RVA: 0x0001C60C File Offset: 0x0001A80C
		public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
		{
			ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
			return (T o) => fieldInfo.GetValue(o);
		}

		// Token: 0x060007CB RID: 1995 RVA: 0x0001C660 File Offset: 0x0001A860
		public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
		{
			ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
			return delegate(T o, object v)
			{
				fieldInfo.SetValue(o, v);
			};
		}

		// Token: 0x060007CC RID: 1996 RVA: 0x0001C6B4 File Offset: 0x0001A8B4
		public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
		{
			ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
			return delegate(T o, object v)
			{
				propertyInfo.SetValue(o, v, null);
			};
		}

		// Token: 0x0400026B RID: 619
		public static readonly LateBoundReflectionDelegateFactory Instance = new LateBoundReflectionDelegateFactory();
	}
}

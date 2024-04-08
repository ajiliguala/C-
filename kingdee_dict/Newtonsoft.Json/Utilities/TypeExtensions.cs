using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000AB RID: 171
	internal static class TypeExtensions
	{
		// Token: 0x060007A5 RID: 1957 RVA: 0x0001B9A8 File Offset: 0x00019BA8
		public static MethodInfo GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
		{
			IEnumerable<MethodInfo> enumerable = from method in type.GetMethods()
			where method.Name == name
			select method;
			foreach (MethodInfo methodInfo in enumerable)
			{
				if (methodInfo.HasParameters(parameterTypes))
				{
					return methodInfo;
				}
			}
			return null;
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x0001BA2C File Offset: 0x00019C2C
		public static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
		{
			Type[] array = (from parameter in method.GetParameters()
			select parameter.ParameterType).ToArray<Type>();
			if (array.Length != parameterTypes.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToString() != parameterTypes[i].ToString())
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x0001BCF4 File Offset: 0x00019EF4
		public static IEnumerable<Type> AllInterfaces(this Type target)
		{
			foreach (Type IF in target.GetInterfaces())
			{
				yield return IF;
				foreach (Type childIF in IF.AllInterfaces())
				{
					yield return childIF;
				}
			}
			yield break;
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x0001BD1C File Offset: 0x00019F1C
		public static IEnumerable<MethodInfo> AllMethods(this Type target)
		{
			List<Type> list = target.AllInterfaces().ToList<Type>();
			list.Add(target);
			return from type in list
			from method in type.GetMethods()
			select method;
		}
	}
}

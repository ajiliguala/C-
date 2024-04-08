using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000A9 RID: 169
	internal class WrapperMethodBuilder
	{
		// Token: 0x06000798 RID: 1944 RVA: 0x0001B70B File Offset: 0x0001990B
		public WrapperMethodBuilder(Type realObjectType, TypeBuilder proxyBuilder)
		{
			this._realObjectType = realObjectType;
			this._wrapperBuilder = proxyBuilder;
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x0001B734 File Offset: 0x00019934
		public void Generate(MethodInfo newMethod)
		{
			if (newMethod.IsGenericMethod)
			{
				newMethod = newMethod.GetGenericMethodDefinition();
			}
			FieldInfo field = typeof(DynamicWrapperBase).GetField("UnderlyingObject", BindingFlags.Instance | BindingFlags.NonPublic);
			ParameterInfo[] parameters = newMethod.GetParameters();
			Type[] parameterTypes = (from parameter in parameters
			select parameter.ParameterType).ToArray<Type>();
			MethodBuilder methodBuilder = this._wrapperBuilder.DefineMethod(newMethod.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual, newMethod.ReturnType, parameterTypes);
			if (newMethod.IsGenericMethod)
			{
				methodBuilder.DefineGenericParameters((from arg in newMethod.GetGenericArguments()
				select arg.Name).ToArray<string>());
			}
			ILGenerator ilgenerator = methodBuilder.GetILGenerator();
			WrapperMethodBuilder.LoadUnderlyingObject(ilgenerator, field);
			WrapperMethodBuilder.PushParameters(parameters, ilgenerator);
			this.ExecuteMethod(newMethod, parameterTypes, ilgenerator);
			WrapperMethodBuilder.Return(ilgenerator);
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x0001B818 File Offset: 0x00019A18
		private static void Return(ILGenerator ilGenerator)
		{
			ilGenerator.Emit(OpCodes.Ret);
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x0001B828 File Offset: 0x00019A28
		private void ExecuteMethod(MethodBase newMethod, Type[] parameterTypes, ILGenerator ilGenerator)
		{
			MethodInfo method = this.GetMethod(newMethod, parameterTypes);
			if (method == null)
			{
				throw new MissingMethodException("Unable to find method " + newMethod.Name + " on " + this._realObjectType.FullName);
			}
			ilGenerator.Emit(OpCodes.Call, method);
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x0001B879 File Offset: 0x00019A79
		private MethodInfo GetMethod(MethodBase realMethod, Type[] parameterTypes)
		{
			if (realMethod.IsGenericMethod)
			{
				return this._realObjectType.GetGenericMethod(realMethod.Name, parameterTypes);
			}
			return this._realObjectType.GetMethod(realMethod.Name, parameterTypes);
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x0001B8A8 File Offset: 0x00019AA8
		private static void PushParameters(ICollection<ParameterInfo> parameters, ILGenerator ilGenerator)
		{
			for (int i = 1; i < parameters.Count + 1; i++)
			{
				ilGenerator.Emit(OpCodes.Ldarg, i);
			}
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x0001B8D4 File Offset: 0x00019AD4
		private static void LoadUnderlyingObject(ILGenerator ilGenerator, FieldInfo srcField)
		{
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, srcField);
		}

		// Token: 0x04000262 RID: 610
		private readonly Type _realObjectType;

		// Token: 0x04000263 RID: 611
		private readonly TypeBuilder _wrapperBuilder;
	}
}

using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000AD RID: 173
	internal class DynamicReflectionDelegateFactory : ReflectionDelegateFactory
	{
		// Token: 0x060007B5 RID: 1973 RVA: 0x0001BE44 File Offset: 0x0001A044
		private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
		{
			return (!owner.IsInterface) ? new DynamicMethod(name, returnType, parameterTypes, owner, true) : new DynamicMethod(name, returnType, parameterTypes, owner.Module, true);
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x0001BE78 File Offset: 0x0001A078
		public override MethodCall<T, object> CreateMethodCall<T>(MethodBase method)
		{
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod(method.ToString(), typeof(object), new Type[]
			{
				typeof(object),
				typeof(object[])
			}, method.DeclaringType);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			ParameterInfo[] parameters = method.GetParameters();
			Label label = ilgenerator.DefineLabel();
			ilgenerator.Emit(OpCodes.Ldarg_1);
			ilgenerator.Emit(OpCodes.Ldlen);
			ilgenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
			ilgenerator.Emit(OpCodes.Beq, label);
			ilgenerator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes));
			ilgenerator.Emit(OpCodes.Throw);
			ilgenerator.MarkLabel(label);
			if (!method.IsConstructor && !method.IsStatic)
			{
				ilgenerator.PushInstance(method.DeclaringType);
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				ilgenerator.Emit(OpCodes.Ldarg_1);
				ilgenerator.Emit(OpCodes.Ldc_I4, i);
				ilgenerator.Emit(OpCodes.Ldelem_Ref);
				ilgenerator.UnboxIfNeeded(parameters[i].ParameterType);
			}
			if (method.IsConstructor)
			{
				ilgenerator.Emit(OpCodes.Newobj, (ConstructorInfo)method);
			}
			else if (method.IsFinal || !method.IsVirtual)
			{
				ilgenerator.CallMethod((MethodInfo)method);
			}
			Type type = method.IsConstructor ? method.DeclaringType : ((MethodInfo)method).ReturnType;
			if (type != typeof(void))
			{
				ilgenerator.BoxIfNeeded(type);
			}
			else
			{
				ilgenerator.Emit(OpCodes.Ldnull);
			}
			ilgenerator.Return();
			return (MethodCall<T, object>)dynamicMethod.CreateDelegate(typeof(MethodCall<T, object>));
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x0001C034 File Offset: 0x0001A234
		public override Func<T> CreateDefaultConstructor<T>(Type type)
		{
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod("Create" + type.FullName, typeof(object), Type.EmptyTypes, type);
			dynamicMethod.InitLocals = true;
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			if (type.IsValueType)
			{
				ilgenerator.DeclareLocal(type);
				ilgenerator.Emit(OpCodes.Ldloc_0);
				ilgenerator.Emit(OpCodes.Box, type);
			}
			else
			{
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				if (constructor == null)
				{
					throw new Exception("Could not get constructor for {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						type
					}));
				}
				ilgenerator.Emit(OpCodes.Newobj, constructor);
			}
			ilgenerator.Return();
			return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x0001C100 File Offset: 0x0001A300
		public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod(true);
			if (getMethod == null)
			{
				throw new Exception("Property '{0}' does not have a getter.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					propertyInfo.Name
				}));
			}
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod("Get" + propertyInfo.Name, typeof(T), new Type[]
			{
				typeof(object)
			}, propertyInfo.DeclaringType);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			if (!getMethod.IsStatic)
			{
				ilgenerator.PushInstance(propertyInfo.DeclaringType);
			}
			ilgenerator.CallMethod(getMethod);
			ilgenerator.BoxIfNeeded(propertyInfo.PropertyType);
			ilgenerator.Return();
			return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x0001C1CC File Offset: 0x0001A3CC
		public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
		{
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod("Get" + fieldInfo.Name, typeof(T), new Type[]
			{
				typeof(object)
			}, fieldInfo.DeclaringType);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			if (!fieldInfo.IsStatic)
			{
				ilgenerator.PushInstance(fieldInfo.DeclaringType);
			}
			ilgenerator.Emit(OpCodes.Ldfld, fieldInfo);
			ilgenerator.BoxIfNeeded(fieldInfo.FieldType);
			ilgenerator.Return();
			return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x0001C264 File Offset: 0x0001A464
		public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
		{
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod("Set" + fieldInfo.Name, null, new Type[]
			{
				typeof(object),
				typeof(object)
			}, fieldInfo.DeclaringType);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			if (!fieldInfo.IsStatic)
			{
				ilgenerator.PushInstance(fieldInfo.DeclaringType);
			}
			ilgenerator.Emit(OpCodes.Ldarg_1);
			ilgenerator.UnboxIfNeeded(fieldInfo.FieldType);
			ilgenerator.Emit(OpCodes.Stfld, fieldInfo);
			ilgenerator.Return();
			return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x0001C30C File Offset: 0x0001A50C
		public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
		{
			MethodInfo setMethod = propertyInfo.GetSetMethod(true);
			DynamicMethod dynamicMethod = DynamicReflectionDelegateFactory.CreateDynamicMethod("Set" + propertyInfo.Name, null, new Type[]
			{
				typeof(object),
				typeof(object)
			}, propertyInfo.DeclaringType);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			if (!setMethod.IsStatic)
			{
				ilgenerator.PushInstance(propertyInfo.DeclaringType);
			}
			ilgenerator.Emit(OpCodes.Ldarg_1);
			ilgenerator.UnboxIfNeeded(propertyInfo.PropertyType);
			ilgenerator.CallMethod(setMethod);
			ilgenerator.Return();
			return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
		}

		// Token: 0x0400026A RID: 618
		public static DynamicReflectionDelegateFactory Instance = new DynamicReflectionDelegateFactory();
	}
}

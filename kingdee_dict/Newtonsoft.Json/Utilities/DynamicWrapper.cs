using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000A8 RID: 168
	internal static class DynamicWrapper
	{
		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000790 RID: 1936 RVA: 0x0001B452 File Offset: 0x00019652
		private static ModuleBuilder ModuleBuilder
		{
			get
			{
				DynamicWrapper.Init();
				return DynamicWrapper._moduleBuilder;
			}
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x0001B460 File Offset: 0x00019660
		private static void Init()
		{
			if (DynamicWrapper._moduleBuilder == null)
			{
				lock (DynamicWrapper._lock)
				{
					if (DynamicWrapper._moduleBuilder == null)
					{
						AssemblyName assemblyName = new AssemblyName("Newtonsoft.Json.Dynamic");
						assemblyName.KeyPair = new StrongNameKeyPair(DynamicWrapper.GetStrongKey());
						AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
						DynamicWrapper._moduleBuilder = assemblyBuilder.DefineDynamicModule("Newtonsoft.Json.DynamicModule", false);
					}
				}
			}
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x0001B4EC File Offset: 0x000196EC
		private static byte[] GetStrongKey()
		{
			string name = "Newtonsoft.Json.Dynamic.snk";
			byte[] result;
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				if (manifestResourceStream == null)
				{
					throw new MissingManifestResourceException("Should have a Newtonsoft.Json.Dynamic.snk as an embedded resource.");
				}
				int num = (int)manifestResourceStream.Length;
				byte[] array = new byte[num];
				manifestResourceStream.Read(array, 0, num);
				result = array;
			}
			return result;
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x0001B554 File Offset: 0x00019754
		public static Type GetWrapper(Type interfaceType, Type realObjectType)
		{
			Type type = DynamicWrapper._wrapperDictionary.GetType(interfaceType, realObjectType);
			if (type == null)
			{
				lock (DynamicWrapper._lock)
				{
					type = DynamicWrapper._wrapperDictionary.GetType(interfaceType, realObjectType);
					if (type == null)
					{
						type = DynamicWrapper.GenerateWrapperType(interfaceType, realObjectType);
						DynamicWrapper._wrapperDictionary.SetType(interfaceType, realObjectType, type);
					}
				}
			}
			return type;
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x0001B5D0 File Offset: 0x000197D0
		public static object GetUnderlyingObject(object wrapper)
		{
			DynamicWrapperBase dynamicWrapperBase = wrapper as DynamicWrapperBase;
			if (dynamicWrapperBase == null)
			{
				throw new ArgumentException("Object is not a wrapper.", "wrapper");
			}
			return dynamicWrapperBase.UnderlyingObject;
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x0001B600 File Offset: 0x00019800
		private static Type GenerateWrapperType(Type interfaceType, Type underlyingType)
		{
			TypeBuilder typeBuilder = DynamicWrapper.ModuleBuilder.DefineType("{0}_{1}_Wrapper".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				interfaceType.Name,
				underlyingType.Name
			}), TypeAttributes.Sealed, typeof(DynamicWrapperBase), new Type[]
			{
				interfaceType
			});
			WrapperMethodBuilder wrapperMethodBuilder = new WrapperMethodBuilder(underlyingType, typeBuilder);
			foreach (MethodInfo newMethod in interfaceType.AllMethods())
			{
				wrapperMethodBuilder.Generate(newMethod);
			}
			return typeBuilder.CreateType();
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x0001B6B4 File Offset: 0x000198B4
		public static T CreateWrapper<T>(object realObject) where T : class
		{
			Type wrapper = DynamicWrapper.GetWrapper(typeof(T), realObject.GetType());
			DynamicWrapperBase dynamicWrapperBase = (DynamicWrapperBase)Activator.CreateInstance(wrapper);
			dynamicWrapperBase.UnderlyingObject = realObject;
			return dynamicWrapperBase as T;
		}

		// Token: 0x0400025F RID: 607
		private static readonly object _lock = new object();

		// Token: 0x04000260 RID: 608
		private static readonly WrapperDictionary _wrapperDictionary = new WrapperDictionary();

		// Token: 0x04000261 RID: 609
		private static ModuleBuilder _moduleBuilder;
	}
}

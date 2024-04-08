using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000AF RID: 175
	internal static class ILGeneratorExtensions
	{
		// Token: 0x060007C2 RID: 1986 RVA: 0x0001C3C8 File Offset: 0x0001A5C8
		public static void PushInstance(this ILGenerator generator, Type type)
		{
			generator.Emit(OpCodes.Ldarg_0);
			if (type.IsValueType)
			{
				generator.Emit(OpCodes.Unbox, type);
				return;
			}
			generator.Emit(OpCodes.Castclass, type);
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x0001C3F6 File Offset: 0x0001A5F6
		public static void BoxIfNeeded(this ILGenerator generator, Type type)
		{
			if (type.IsValueType)
			{
				generator.Emit(OpCodes.Box, type);
				return;
			}
			generator.Emit(OpCodes.Castclass, type);
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x0001C419 File Offset: 0x0001A619
		public static void UnboxIfNeeded(this ILGenerator generator, Type type)
		{
			if (type.IsValueType)
			{
				generator.Emit(OpCodes.Unbox_Any, type);
				return;
			}
			generator.Emit(OpCodes.Castclass, type);
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x0001C43C File Offset: 0x0001A63C
		public static void CallMethod(this ILGenerator generator, MethodInfo methodInfo)
		{
			if (methodInfo.IsFinal || !methodInfo.IsVirtual)
			{
				generator.Emit(OpCodes.Call, methodInfo);
				return;
			}
			generator.Emit(OpCodes.Callvirt, methodInfo);
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x0001C467 File Offset: 0x0001A667
		public static void Return(this ILGenerator generator)
		{
			generator.Emit(OpCodes.Ret);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000A2 RID: 162
	internal static class DynamicUtils
	{
		// Token: 0x06000780 RID: 1920 RVA: 0x0001AFD4 File Offset: 0x000191D4
		public static bool TryGetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, out object value)
		{
			ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
			GetMemberBinder innerBinder = (GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils));
			CallSite<Func<CallSite, object, object>> callSite = CallSite<Func<CallSite, object, object>>.Create(new DynamicUtils.NoThrowGetBinderMember(innerBinder));
			object obj = callSite.Target(callSite, dynamicProvider);
			if (!object.ReferenceEquals(obj, DynamicUtils.NoThrowExpressionVisitor.ErrorResult))
			{
				value = obj;
				return true;
			}
			value = null;
			return false;
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x0001B034 File Offset: 0x00019234
		public static bool TrySetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, object value)
		{
			ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
			SetMemberBinder innerBinder = (SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils));
			CallSite<Func<CallSite, object, object, object>> callSite = CallSite<Func<CallSite, object, object, object>>.Create(new DynamicUtils.NoThrowSetBinderMember(innerBinder));
			object objA = callSite.Target(callSite, dynamicProvider, value);
			return !object.ReferenceEquals(objA, DynamicUtils.NoThrowExpressionVisitor.ErrorResult);
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x0001B08C File Offset: 0x0001928C
		public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
		{
			DynamicMetaObject metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
			return metaObject.GetDynamicMemberNames();
		}

		// Token: 0x020000A3 RID: 163
		internal static class BinderWrapper
		{
			// Token: 0x06000783 RID: 1923 RVA: 0x0001B0AC File Offset: 0x000192AC
			private static void Init()
			{
				if (!DynamicUtils.BinderWrapper._init)
				{
					Type type = Type.GetType("Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);
					if (type == null)
					{
						throw new Exception("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							"Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
						}));
					}
					int[] values = new int[1];
					DynamicUtils.BinderWrapper._getCSharpArgumentInfoArray = DynamicUtils.BinderWrapper.CreateSharpArgumentInfoArray(values);
					DynamicUtils.BinderWrapper._setCSharpArgumentInfoArray = DynamicUtils.BinderWrapper.CreateSharpArgumentInfoArray(new int[]
					{
						0,
						3
					});
					DynamicUtils.BinderWrapper.CreateMemberCalls();
					DynamicUtils.BinderWrapper._init = true;
				}
			}

			// Token: 0x06000784 RID: 1924 RVA: 0x0001B130 File Offset: 0x00019330
			private static object CreateSharpArgumentInfoArray(params int[] values)
			{
				Type type = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				Type type2 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				Array array = Array.CreateInstance(type, values.Length);
				for (int i = 0; i < values.Length; i++)
				{
					MethodInfo method = type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public, null, new Type[]
					{
						type2,
						typeof(string)
					}, null);
					MethodBase methodBase = method;
					object obj = null;
					object[] array2 = new object[2];
					array2[0] = 0;
					object value = methodBase.Invoke(obj, array2);
					array.SetValue(value, i);
				}
				return array;
			}

			// Token: 0x06000785 RID: 1925 RVA: 0x0001B1C4 File Offset: 0x000193C4
			private static void CreateMemberCalls()
			{
				Type type = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				Type type2 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				Type type3 = Type.GetType("Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				Type type4 = typeof(IEnumerable<>).MakeGenericType(new Type[]
				{
					type
				});
				MethodInfo method = type3.GetMethod("GetMember", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					type2,
					typeof(string),
					typeof(Type),
					type4
				}, null);
				DynamicUtils.BinderWrapper._getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
				MethodInfo method2 = type3.GetMethod("SetMember", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					type2,
					typeof(string),
					typeof(Type),
					type4
				}, null);
				DynamicUtils.BinderWrapper._setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method2);
			}

			// Token: 0x06000786 RID: 1926 RVA: 0x0001B2B8 File Offset: 0x000194B8
			public static CallSiteBinder GetMember(string name, Type context)
			{
				DynamicUtils.BinderWrapper.Init();
				return (CallSiteBinder)DynamicUtils.BinderWrapper._getMemberCall(null, new object[]
				{
					0,
					name,
					context,
					DynamicUtils.BinderWrapper._getCSharpArgumentInfoArray
				});
			}

			// Token: 0x06000787 RID: 1927 RVA: 0x0001B2FC File Offset: 0x000194FC
			public static CallSiteBinder SetMember(string name, Type context)
			{
				DynamicUtils.BinderWrapper.Init();
				return (CallSiteBinder)DynamicUtils.BinderWrapper._setMemberCall(null, new object[]
				{
					0,
					name,
					context,
					DynamicUtils.BinderWrapper._setCSharpArgumentInfoArray
				});
			}

			// Token: 0x04000251 RID: 593
			public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Token: 0x04000252 RID: 594
			private const string BinderTypeName = "Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Token: 0x04000253 RID: 595
			private const string CSharpArgumentInfoTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Token: 0x04000254 RID: 596
			private const string CSharpArgumentInfoFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Token: 0x04000255 RID: 597
			private const string CSharpBinderFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Token: 0x04000256 RID: 598
			private static object _getCSharpArgumentInfoArray;

			// Token: 0x04000257 RID: 599
			private static object _setCSharpArgumentInfoArray;

			// Token: 0x04000258 RID: 600
			private static MethodCall<object, object> _getMemberCall;

			// Token: 0x04000259 RID: 601
			private static MethodCall<object, object> _setMemberCall;

			// Token: 0x0400025A RID: 602
			private static bool _init;
		}

		// Token: 0x020000A4 RID: 164
		internal class NoThrowGetBinderMember : GetMemberBinder
		{
			// Token: 0x06000788 RID: 1928 RVA: 0x0001B33F File Offset: 0x0001953F
			public NoThrowGetBinderMember(GetMemberBinder innerBinder) : base(innerBinder.Name, innerBinder.IgnoreCase)
			{
				this._innerBinder = innerBinder;
			}

			// Token: 0x06000789 RID: 1929 RVA: 0x0001B35C File Offset: 0x0001955C
			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject dynamicMetaObject = this._innerBinder.Bind(target, new DynamicMetaObject[0]);
				DynamicUtils.NoThrowExpressionVisitor noThrowExpressionVisitor = new DynamicUtils.NoThrowExpressionVisitor();
				Expression expression = noThrowExpressionVisitor.Visit(dynamicMetaObject.Expression);
				return new DynamicMetaObject(expression, dynamicMetaObject.Restrictions);
			}

			// Token: 0x0400025B RID: 603
			private readonly GetMemberBinder _innerBinder;
		}

		// Token: 0x020000A5 RID: 165
		internal class NoThrowSetBinderMember : SetMemberBinder
		{
			// Token: 0x0600078A RID: 1930 RVA: 0x0001B39D File Offset: 0x0001959D
			public NoThrowSetBinderMember(SetMemberBinder innerBinder) : base(innerBinder.Name, innerBinder.IgnoreCase)
			{
				this._innerBinder = innerBinder;
			}

			// Token: 0x0600078B RID: 1931 RVA: 0x0001B3B8 File Offset: 0x000195B8
			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject dynamicMetaObject = this._innerBinder.Bind(target, new DynamicMetaObject[]
				{
					value
				});
				DynamicUtils.NoThrowExpressionVisitor noThrowExpressionVisitor = new DynamicUtils.NoThrowExpressionVisitor();
				Expression expression = noThrowExpressionVisitor.Visit(dynamicMetaObject.Expression);
				return new DynamicMetaObject(expression, dynamicMetaObject.Restrictions);
			}

			// Token: 0x0400025C RID: 604
			private readonly SetMemberBinder _innerBinder;
		}

		// Token: 0x020000A6 RID: 166
		internal class NoThrowExpressionVisitor : ExpressionVisitor
		{
			// Token: 0x0600078C RID: 1932 RVA: 0x0001B402 File Offset: 0x00019602
			protected override Expression VisitConditional(ConditionalExpression node)
			{
				if (node.IfFalse.NodeType == ExpressionType.Throw)
				{
					return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(DynamicUtils.NoThrowExpressionVisitor.ErrorResult));
				}
				return base.VisitConditional(node);
			}

			// Token: 0x0400025D RID: 605
			internal static readonly object ErrorResult = new object();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x0200009F RID: 159
	internal sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
	{
		// Token: 0x0600075E RID: 1886 RVA: 0x0001A3B9 File Offset: 0x000185B9
		internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy, bool dontFallbackFirst) : base(expression, BindingRestrictions.Empty, value)
		{
			this._proxy = proxy;
			this._dontFallbackFirst = dontFallbackFirst;
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x0600075F RID: 1887 RVA: 0x0001A3DC File Offset: 0x000185DC
		private new T Value
		{
			get
			{
				return (T)((object)base.Value);
			}
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x0001A41E File Offset: 0x0001861E
		private bool IsOverridden(string method)
		{
			return this._proxy.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public).Cast<MethodInfo>().Any((MethodInfo info) => info.DeclaringType != typeof(DynamicProxy<T>) && info.GetBaseDefinition().DeclaringType == typeof(DynamicProxy<T>));
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x0001A478 File Offset: 0x00018678
		public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		{
			if (!this.IsOverridden("TryGetMember"))
			{
				return base.BindGetMember(binder);
			}
			return this.CallMethodWithResult("TryGetMember", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject e) => binder.FallbackGetMember(this, e), null);
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x0001A4F8 File Offset: 0x000186F8
		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
		{
			if (!this.IsOverridden("TrySetMember"))
			{
				return base.BindSetMember(binder, value);
			}
			return this.CallMethodReturnLast("TrySetMember", binder, DynamicProxyMetaObject<T>.GetArgs(new DynamicMetaObject[]
			{
				value
			}), (DynamicMetaObject e) => binder.FallbackSetMember(this, value, e));
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x0001A590 File Offset: 0x00018790
		public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
		{
			if (!this.IsOverridden("TryDeleteMember"))
			{
				return base.BindDeleteMember(binder);
			}
			return this.CallMethodNoResult("TryDeleteMember", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject e) => binder.FallbackDeleteMember(this, e));
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x0001A60C File Offset: 0x0001880C
		public override DynamicMetaObject BindConvert(ConvertBinder binder)
		{
			if (!this.IsOverridden("TryConvert"))
			{
				return base.BindConvert(binder);
			}
			return this.CallMethodWithResult("TryConvert", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject e) => binder.FallbackConvert(this, e), null);
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x0001A6A4 File Offset: 0x000188A4
		public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
		{
			if (!this.IsOverridden("TryInvokeMember"))
			{
				return base.BindInvokeMember(binder, args);
			}
			DynamicProxyMetaObject<T>.Fallback fallback = (DynamicMetaObject e) => binder.FallbackInvokeMember(this, args, e);
			DynamicMetaObject dynamicMetaObject = this.BuildCallMethodWithResult("TryInvokeMember", binder, DynamicProxyMetaObject<T>.GetArgArray(args), this.BuildCallMethodWithResult("TryGetMember", new DynamicProxyMetaObject<T>.GetBinderAdapter(binder), DynamicProxyMetaObject<T>.NoArgs, fallback(null), (DynamicMetaObject e) => binder.FallbackInvoke(e, args, null)), null);
			if (!this._dontFallbackFirst)
			{
				return fallback(dynamicMetaObject);
			}
			return dynamicMetaObject;
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x0001A778 File Offset: 0x00018978
		public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
		{
			if (!this.IsOverridden("TryCreateInstance"))
			{
				return base.BindCreateInstance(binder, args);
			}
			return this.CallMethodWithResult("TryCreateInstance", binder, DynamicProxyMetaObject<T>.GetArgArray(args), (DynamicMetaObject e) => binder.FallbackCreateInstance(this, args, e), null);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x0001A80C File Offset: 0x00018A0C
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
		{
			if (!this.IsOverridden("TryInvoke"))
			{
				return base.BindInvoke(binder, args);
			}
			return this.CallMethodWithResult("TryInvoke", binder, DynamicProxyMetaObject<T>.GetArgArray(args), (DynamicMetaObject e) => binder.FallbackInvoke(this, args, e), null);
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x0001A8A0 File Offset: 0x00018AA0
		public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
		{
			if (!this.IsOverridden("TryBinaryOperation"))
			{
				return base.BindBinaryOperation(binder, arg);
			}
			return this.CallMethodWithResult("TryBinaryOperation", binder, DynamicProxyMetaObject<T>.GetArgs(new DynamicMetaObject[]
			{
				arg
			}), (DynamicMetaObject e) => binder.FallbackBinaryOperation(this, arg, e), null);
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x0001A938 File Offset: 0x00018B38
		public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
		{
			if (!this.IsOverridden("TryUnaryOperation"))
			{
				return base.BindUnaryOperation(binder);
			}
			return this.CallMethodWithResult("TryUnaryOperation", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject e) => binder.FallbackUnaryOperation(this, e), null);
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x0001A9B8 File Offset: 0x00018BB8
		public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
		{
			if (!this.IsOverridden("TryGetIndex"))
			{
				return base.BindGetIndex(binder, indexes);
			}
			return this.CallMethodWithResult("TryGetIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes), (DynamicMetaObject e) => binder.FallbackGetIndex(this, indexes, e), null);
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x0001AA54 File Offset: 0x00018C54
		public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
		{
			if (!this.IsOverridden("TrySetIndex"))
			{
				return base.BindSetIndex(binder, indexes, value);
			}
			return this.CallMethodReturnLast("TrySetIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes, value), (DynamicMetaObject e) => binder.FallbackSetIndex(this, indexes, value, e));
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x0001AAFC File Offset: 0x00018CFC
		public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
		{
			if (!this.IsOverridden("TryDeleteIndex"))
			{
				return base.BindDeleteIndex(binder, indexes);
			}
			return this.CallMethodNoResult("TryDeleteIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes), (DynamicMetaObject e) => binder.FallbackDeleteIndex(this, indexes, e));
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x0001AB83 File Offset: 0x00018D83
		private static Expression[] GetArgs(params DynamicMetaObject[] args)
		{
			return (from arg in args
			select Expression.Convert(arg.Expression, typeof(object))).ToArray<UnaryExpression>();
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x0001ABB0 File Offset: 0x00018DB0
		private static Expression[] GetArgArray(DynamicMetaObject[] args)
		{
			return new NewArrayExpression[]
			{
				Expression.NewArrayInit(typeof(object), DynamicProxyMetaObject<T>.GetArgs(args))
			};
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x0001ABE0 File Offset: 0x00018DE0
		private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
		{
			return new Expression[]
			{
				Expression.NewArrayInit(typeof(object), DynamicProxyMetaObject<T>.GetArgs(args)),
				Expression.Convert(value.Expression, typeof(object))
			};
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x0001AC28 File Offset: 0x00018E28
		private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
		{
			Type type = binder.GetType();
			while (!type.IsVisible)
			{
				type = type.BaseType;
			}
			return Expression.Constant(binder, type);
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x0001AC54 File Offset: 0x00018E54
		private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicProxyMetaObject<T>.Fallback fallback, DynamicProxyMetaObject<T>.Fallback fallbackInvoke = null)
		{
			DynamicMetaObject fallbackResult = fallback(null);
			DynamicMetaObject dynamicMetaObject = this.BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
			if (!this._dontFallbackFirst)
			{
				return fallback(dynamicMetaObject);
			}
			return dynamicMetaObject;
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x0001AC8C File Offset: 0x00018E8C
		private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicMetaObject fallbackResult, DynamicProxyMetaObject<T>.Fallback fallbackInvoke)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
			IList<Expression> list = new List<Expression>();
			list.Add(Expression.Convert(base.Expression, typeof(T)));
			list.Add(DynamicProxyMetaObject<T>.Constant(binder));
			list.AddRange(args);
			list.Add(parameterExpression);
			DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty);
			if (binder.ReturnType != typeof(object))
			{
				UnaryExpression expression = Expression.Convert(dynamicMetaObject.Expression, binder.ReturnType);
				dynamicMetaObject = new DynamicMetaObject(expression, dynamicMetaObject.Restrictions);
			}
			if (fallbackInvoke != null)
			{
				dynamicMetaObject = fallbackInvoke(dynamicMetaObject);
			}
			return new DynamicMetaObject(Expression.Block(new ParameterExpression[]
			{
				parameterExpression
			}, new Expression[]
			{
				Expression.Condition(Expression.Call(Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), dynamicMetaObject.Expression, fallbackResult.Expression, binder.ReturnType)
			}), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions).Merge(fallbackResult.Restrictions));
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x0001ADB8 File Offset: 0x00018FB8
		private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicProxyMetaObject<T>.Fallback fallback)
		{
			DynamicMetaObject dynamicMetaObject = fallback(null);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
			IList<Expression> list = new List<Expression>();
			list.Add(Expression.Convert(base.Expression, typeof(T)));
			list.Add(DynamicProxyMetaObject<T>.Constant(binder));
			list.AddRange(args);
			list[args.Length + 1] = Expression.Assign(parameterExpression, list[args.Length + 1]);
			DynamicMetaObject dynamicMetaObject2 = new DynamicMetaObject(Expression.Block(new ParameterExpression[]
			{
				parameterExpression
			}, new Expression[]
			{
				Expression.Condition(Expression.Call(Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), parameterExpression, dynamicMetaObject.Expression, typeof(object))
			}), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
			if (!this._dontFallbackFirst)
			{
				return fallback(dynamicMetaObject2);
			}
			return dynamicMetaObject2;
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x0001AEB4 File Offset: 0x000190B4
		private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicProxyMetaObject<T>.Fallback fallback)
		{
			DynamicMetaObject dynamicMetaObject = fallback(null);
			IList<Expression> list = new List<Expression>();
			list.Add(Expression.Convert(base.Expression, typeof(T)));
			list.Add(DynamicProxyMetaObject<T>.Constant(binder));
			list.AddRange(args);
			DynamicMetaObject dynamicMetaObject2 = new DynamicMetaObject(Expression.Condition(Expression.Call(Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), Expression.Empty(), dynamicMetaObject.Expression, typeof(void)), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
			if (!this._dontFallbackFirst)
			{
				return fallback(dynamicMetaObject2);
			}
			return dynamicMetaObject2;
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x0001AF62 File Offset: 0x00019162
		private BindingRestrictions GetRestrictions()
		{
			if (this.Value != null || !base.HasValue)
			{
				return BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType);
			}
			return BindingRestrictions.GetInstanceRestriction(base.Expression, null);
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x0001AF97 File Offset: 0x00019197
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return this._proxy.GetDynamicMemberNames(this.Value);
		}

		// Token: 0x0400024C RID: 588
		private readonly DynamicProxy<T> _proxy;

		// Token: 0x0400024D RID: 589
		private readonly bool _dontFallbackFirst;

		// Token: 0x0400024E RID: 590
		private static readonly Expression[] NoArgs = new Expression[0];

		// Token: 0x020000A0 RID: 160
		// (Invoke) Token: 0x0600077B RID: 1915
		private delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

		// Token: 0x020000A1 RID: 161
		private sealed class GetBinderAdapter : GetMemberBinder
		{
			// Token: 0x0600077E RID: 1918 RVA: 0x0001AFB7 File Offset: 0x000191B7
			internal GetBinderAdapter(InvokeMemberBinder binder) : base(binder.Name, binder.IgnoreCase)
			{
			}

			// Token: 0x0600077F RID: 1919 RVA: 0x0001AFCB File Offset: 0x000191CB
			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				throw new NotSupportedException();
			}
		}
	}
}

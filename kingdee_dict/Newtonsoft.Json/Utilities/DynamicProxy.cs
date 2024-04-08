using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x02000024 RID: 36
	internal class DynamicProxy<T>
	{
		// Token: 0x0600011F RID: 287 RVA: 0x00005BBD File Offset: 0x00003DBD
		public virtual IEnumerable<string> GetDynamicMemberNames(T instance)
		{
			return new string[0];
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00005BC5 File Offset: 0x00003DC5
		public virtual bool TryBinaryOperation(T instance, BinaryOperationBinder binder, object arg, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00005BCC File Offset: 0x00003DCC
		public virtual bool TryConvert(T instance, ConvertBinder binder, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00005BD2 File Offset: 0x00003DD2
		public virtual bool TryCreateInstance(T instance, CreateInstanceBinder binder, object[] args, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00005BD9 File Offset: 0x00003DD9
		public virtual bool TryDeleteIndex(T instance, DeleteIndexBinder binder, object[] indexes)
		{
			return false;
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00005BDC File Offset: 0x00003DDC
		public virtual bool TryDeleteMember(T instance, DeleteMemberBinder binder)
		{
			return false;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00005BDF File Offset: 0x00003DDF
		public virtual bool TryGetIndex(T instance, GetIndexBinder binder, object[] indexes, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00005BE6 File Offset: 0x00003DE6
		public virtual bool TryGetMember(T instance, GetMemberBinder binder, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00005BEC File Offset: 0x00003DEC
		public virtual bool TryInvoke(T instance, InvokeBinder binder, object[] args, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00005BF3 File Offset: 0x00003DF3
		public virtual bool TryInvokeMember(T instance, InvokeMemberBinder binder, object[] args, out object result)
		{
			result = null;
			return false;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005BFA File Offset: 0x00003DFA
		public virtual bool TrySetIndex(T instance, SetIndexBinder binder, object[] indexes, object value)
		{
			return false;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00005BFD File Offset: 0x00003DFD
		public virtual bool TrySetMember(T instance, SetMemberBinder binder, object value)
		{
			return false;
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00005C00 File Offset: 0x00003E00
		public virtual bool TryUnaryOperation(T instance, UnaryOperationBinder binder, out object result)
		{
			result = null;
			return false;
		}
	}
}

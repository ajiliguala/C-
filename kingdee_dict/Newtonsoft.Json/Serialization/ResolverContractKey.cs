using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000080 RID: 128
	internal struct ResolverContractKey : IEquatable<ResolverContractKey>
	{
		// Token: 0x06000603 RID: 1539 RVA: 0x00014A70 File Offset: 0x00012C70
		public ResolverContractKey(Type resolverType, Type contractType)
		{
			this._resolverType = resolverType;
			this._contractType = contractType;
		}

		// Token: 0x06000604 RID: 1540 RVA: 0x00014A80 File Offset: 0x00012C80
		public override int GetHashCode()
		{
			return this._resolverType.GetHashCode() ^ this._contractType.GetHashCode();
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00014A99 File Offset: 0x00012C99
		public override bool Equals(object obj)
		{
			return obj is ResolverContractKey && this.Equals((ResolverContractKey)obj);
		}

		// Token: 0x06000606 RID: 1542 RVA: 0x00014AB1 File Offset: 0x00012CB1
		public bool Equals(ResolverContractKey other)
		{
			return this._resolverType == other._resolverType && this._contractType == other._contractType;
		}

		// Token: 0x04000194 RID: 404
		private readonly Type _resolverType;

		// Token: 0x04000195 RID: 405
		private readonly Type _contractType;
	}
}

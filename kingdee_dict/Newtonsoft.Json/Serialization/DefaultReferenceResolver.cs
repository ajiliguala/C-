using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000038 RID: 56
	internal class DefaultReferenceResolver : IReferenceResolver
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000224 RID: 548 RVA: 0x0000859D File Offset: 0x0000679D
		private BidirectionalDictionary<string, object> Mappings
		{
			get
			{
				if (this._mappings == null)
				{
					this._mappings = new BidirectionalDictionary<string, object>(EqualityComparer<string>.Default, new DefaultReferenceResolver.ReferenceEqualsEqualityComparer());
				}
				return this._mappings;
			}
		}

		// Token: 0x06000225 RID: 549 RVA: 0x000085C4 File Offset: 0x000067C4
		public object ResolveReference(string reference)
		{
			object result;
			this.Mappings.TryGetByFirst(reference, out result);
			return result;
		}

		// Token: 0x06000226 RID: 550 RVA: 0x000085E4 File Offset: 0x000067E4
		public string GetReference(object value)
		{
			string text;
			if (!this.Mappings.TryGetBySecond(value, out text))
			{
				this._referenceCount++;
				text = this._referenceCount.ToString(CultureInfo.InvariantCulture);
				this.Mappings.Add(text, value);
			}
			return text;
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000862E File Offset: 0x0000682E
		public void AddReference(string reference, object value)
		{
			this.Mappings.Add(reference, value);
		}

		// Token: 0x06000228 RID: 552 RVA: 0x00008640 File Offset: 0x00006840
		public bool IsReferenced(object value)
		{
			string text;
			return this.Mappings.TryGetBySecond(value, out text);
		}

		// Token: 0x0400009F RID: 159
		private int _referenceCount;

		// Token: 0x040000A0 RID: 160
		private BidirectionalDictionary<string, object> _mappings;

		// Token: 0x02000039 RID: 57
		private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
		{
			// Token: 0x0600022A RID: 554 RVA: 0x00008663 File Offset: 0x00006863
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return object.ReferenceEquals(x, y);
			}

			// Token: 0x0600022B RID: 555 RVA: 0x0000866C File Offset: 0x0000686C
			int IEqualityComparer<object>.GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000069 RID: 105
	public struct JEnumerable<T> : IJEnumerable<T>, IEnumerable<T>, IEnumerable where T : JToken
	{
		// Token: 0x060004B7 RID: 1207 RVA: 0x00010A85 File Offset: 0x0000EC85
		public JEnumerable(IEnumerable<T> enumerable)
		{
			ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
			this._enumerable = enumerable;
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x00010A99 File Offset: 0x0000EC99
		public IEnumerator<T> GetEnumerator()
		{
			return this._enumerable.GetEnumerator();
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00010AA6 File Offset: 0x0000ECA6
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x170000F0 RID: 240
		public IJEnumerable<JToken> this[object key]
		{
			get
			{
				return new JEnumerable<JToken>(this._enumerable.Values(key));
			}
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00010AC6 File Offset: 0x0000ECC6
		public override bool Equals(object obj)
		{
			return obj is JEnumerable<T> && this._enumerable.Equals(((JEnumerable<T>)obj)._enumerable);
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00010AE8 File Offset: 0x0000ECE8
		public override int GetHashCode()
		{
			return this._enumerable.GetHashCode();
		}

		// Token: 0x0400013B RID: 315
		public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

		// Token: 0x0400013C RID: 316
		private IEnumerable<T> _enumerable;
	}
}

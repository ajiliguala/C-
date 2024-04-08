using System;
using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000B RID: 11
	internal class BsonArray : BsonToken, IEnumerable<BsonToken>, IEnumerable
	{
		// Token: 0x06000054 RID: 84 RVA: 0x00003A2A File Offset: 0x00001C2A
		public void Add(BsonToken token)
		{
			this._children.Add(token);
			token.Parent = this;
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00003A3F File Offset: 0x00001C3F
		public override BsonType Type
		{
			get
			{
				return BsonType.Array;
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003A42 File Offset: 0x00001C42
		public IEnumerator<BsonToken> GetEnumerator()
		{
			return this._children.GetEnumerator();
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003A54 File Offset: 0x00001C54
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04000042 RID: 66
		private readonly List<BsonToken> _children = new List<BsonToken>();
	}
}

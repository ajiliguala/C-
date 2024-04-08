using System;
using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000A RID: 10
	internal class BsonObject : BsonToken, IEnumerable<BsonProperty>, IEnumerable
	{
		// Token: 0x0600004F RID: 79 RVA: 0x000039C0 File Offset: 0x00001BC0
		public void Add(string name, BsonToken token)
		{
			this._children.Add(new BsonProperty
			{
				Name = new BsonString(name, false),
				Value = token
			});
			token.Parent = this;
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000050 RID: 80 RVA: 0x000039FA File Offset: 0x00001BFA
		public override BsonType Type
		{
			get
			{
				return BsonType.Object;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000039FD File Offset: 0x00001BFD
		public IEnumerator<BsonProperty> GetEnumerator()
		{
			return this._children.GetEnumerator();
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003A0F File Offset: 0x00001C0F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04000041 RID: 65
		private readonly List<BsonProperty> _children = new List<BsonProperty>();
	}
}

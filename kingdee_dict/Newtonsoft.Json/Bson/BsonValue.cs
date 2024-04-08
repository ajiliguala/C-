using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000C RID: 12
	internal class BsonValue : BsonToken
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00003A6F File Offset: 0x00001C6F
		public BsonValue(object value, BsonType type)
		{
			this._value = value;
			this._type = type;
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00003A85 File Offset: 0x00001C85
		public object Value
		{
			get
			{
				return this._value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003A8D File Offset: 0x00001C8D
		public override BsonType Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x04000043 RID: 67
		private object _value;

		// Token: 0x04000044 RID: 68
		private BsonType _type;
	}
}

using System;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000056 RID: 86
	internal class XCommentWrapper : XObjectWrapper
	{
		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x0600031D RID: 797 RVA: 0x0000AACB File Offset: 0x00008CCB
		private XComment Text
		{
			get
			{
				return (XComment)base.WrappedNode;
			}
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0000AAD8 File Offset: 0x00008CD8
		public XCommentWrapper(XComment text) : base(text)
		{
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x0600031F RID: 799 RVA: 0x0000AAE1 File Offset: 0x00008CE1
		// (set) Token: 0x06000320 RID: 800 RVA: 0x0000AAEE File Offset: 0x00008CEE
		public override string Value
		{
			get
			{
				return this.Text.Value;
			}
			set
			{
				this.Text.Value = value;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0000AAFC File Offset: 0x00008CFC
		public override IXmlNode ParentNode
		{
			get
			{
				if (this.Text.Parent == null)
				{
					return null;
				}
				return XContainerWrapper.WrapNode(this.Text.Parent);
			}
		}
	}
}

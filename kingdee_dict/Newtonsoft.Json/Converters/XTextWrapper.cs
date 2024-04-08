using System;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000055 RID: 85
	internal class XTextWrapper : XObjectWrapper
	{
		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000318 RID: 792 RVA: 0x0000AA79 File Offset: 0x00008C79
		private XText Text
		{
			get
			{
				return (XText)base.WrappedNode;
			}
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000AA86 File Offset: 0x00008C86
		public XTextWrapper(XText text) : base(text)
		{
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x0600031A RID: 794 RVA: 0x0000AA8F File Offset: 0x00008C8F
		// (set) Token: 0x0600031B RID: 795 RVA: 0x0000AA9C File Offset: 0x00008C9C
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

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x0600031C RID: 796 RVA: 0x0000AAAA File Offset: 0x00008CAA
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

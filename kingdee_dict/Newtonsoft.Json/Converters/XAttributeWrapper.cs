using System;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000058 RID: 88
	internal class XAttributeWrapper : XObjectWrapper
	{
		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000327 RID: 807 RVA: 0x0000AB5B File Offset: 0x00008D5B
		private XAttribute Attribute
		{
			get
			{
				return (XAttribute)base.WrappedNode;
			}
		}

		// Token: 0x06000328 RID: 808 RVA: 0x0000AB68 File Offset: 0x00008D68
		public XAttributeWrapper(XAttribute attribute) : base(attribute)
		{
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000329 RID: 809 RVA: 0x0000AB71 File Offset: 0x00008D71
		// (set) Token: 0x0600032A RID: 810 RVA: 0x0000AB7E File Offset: 0x00008D7E
		public override string Value
		{
			get
			{
				return this.Attribute.Value;
			}
			set
			{
				this.Attribute.Value = value;
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000AB8C File Offset: 0x00008D8C
		public override string LocalName
		{
			get
			{
				return this.Attribute.Name.LocalName;
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600032C RID: 812 RVA: 0x0000AB9E File Offset: 0x00008D9E
		public override string NamespaceURI
		{
			get
			{
				return this.Attribute.Name.NamespaceName;
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600032D RID: 813 RVA: 0x0000ABB0 File Offset: 0x00008DB0
		public override IXmlNode ParentNode
		{
			get
			{
				if (this.Attribute.Parent == null)
				{
					return null;
				}
				return XContainerWrapper.WrapNode(this.Attribute.Parent);
			}
		}
	}
}

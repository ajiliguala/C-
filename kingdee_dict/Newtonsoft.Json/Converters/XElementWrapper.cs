using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000059 RID: 89
	internal class XElementWrapper : XContainerWrapper, IXmlElement, IXmlNode
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x0600032E RID: 814 RVA: 0x0000ABD1 File Offset: 0x00008DD1
		private XElement Element
		{
			get
			{
				return (XElement)base.WrappedNode;
			}
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0000ABDE File Offset: 0x00008DDE
		public XElementWrapper(XElement element) : base(element)
		{
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000ABE8 File Offset: 0x00008DE8
		public void SetAttributeNode(IXmlNode attribute)
		{
			XObjectWrapper xobjectWrapper = (XObjectWrapper)attribute;
			this.Element.Add(xobjectWrapper.WrappedNode);
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0000AC15 File Offset: 0x00008E15
		public override IList<IXmlNode> Attributes
		{
			get
			{
				return (from a in this.Element.Attributes()
				select new XAttributeWrapper(a)).Cast<IXmlNode>().ToList<IXmlNode>();
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000332 RID: 818 RVA: 0x0000AC4E File Offset: 0x00008E4E
		// (set) Token: 0x06000333 RID: 819 RVA: 0x0000AC5B File Offset: 0x00008E5B
		public override string Value
		{
			get
			{
				return this.Element.Value;
			}
			set
			{
				this.Element.Value = value;
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0000AC69 File Offset: 0x00008E69
		public override string LocalName
		{
			get
			{
				return this.Element.Name.LocalName;
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0000AC7B File Offset: 0x00008E7B
		public override string NamespaceURI
		{
			get
			{
				return this.Element.Name.NamespaceName;
			}
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0000AC8D File Offset: 0x00008E8D
		public string GetPrefixOfNamespace(string namespaceURI)
		{
			return this.Element.GetPrefixOfNamespace(namespaceURI);
		}
	}
}

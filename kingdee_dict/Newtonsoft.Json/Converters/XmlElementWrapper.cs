using System;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200004E RID: 78
	internal class XmlElementWrapper : XmlNodeWrapper, IXmlElement, IXmlNode
	{
		// Token: 0x060002E1 RID: 737 RVA: 0x0000A69D File Offset: 0x0000889D
		public XmlElementWrapper(XmlElement element) : base(element)
		{
			this._element = element;
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000A6B0 File Offset: 0x000088B0
		public void SetAttributeNode(IXmlNode attribute)
		{
			XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)attribute;
			this._element.SetAttributeNode((XmlAttribute)xmlNodeWrapper.WrappedNode);
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000A6DB File Offset: 0x000088DB
		public string GetPrefixOfNamespace(string namespaceURI)
		{
			return this._element.GetPrefixOfNamespace(namespaceURI);
		}

		// Token: 0x040000EC RID: 236
		private XmlElement _element;
	}
}

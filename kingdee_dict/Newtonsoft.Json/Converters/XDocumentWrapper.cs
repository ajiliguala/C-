using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000054 RID: 84
	internal class XDocumentWrapper : XContainerWrapper, IXmlDocument, IXmlNode
	{
		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000308 RID: 776 RVA: 0x0000A903 File Offset: 0x00008B03
		private XDocument Document
		{
			get
			{
				return (XDocument)base.WrappedNode;
			}
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0000A910 File Offset: 0x00008B10
		public XDocumentWrapper(XDocument document) : base(document)
		{
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x0600030A RID: 778 RVA: 0x0000A91C File Offset: 0x00008B1C
		public override IList<IXmlNode> ChildNodes
		{
			get
			{
				IList<IXmlNode> childNodes = base.ChildNodes;
				if (this.Document.Declaration != null)
				{
					childNodes.Insert(0, new XDeclarationWrapper(this.Document.Declaration));
				}
				return childNodes;
			}
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000A955 File Offset: 0x00008B55
		public IXmlNode CreateComment(string text)
		{
			return new XObjectWrapper(new XComment(text));
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000A962 File Offset: 0x00008B62
		public IXmlNode CreateTextNode(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000A96F File Offset: 0x00008B6F
		public IXmlNode CreateCDataSection(string data)
		{
			return new XObjectWrapper(new XCData(data));
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0000A97C File Offset: 0x00008B7C
		public IXmlNode CreateWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0000A989 File Offset: 0x00008B89
		public IXmlNode CreateSignificantWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000A996 File Offset: 0x00008B96
		public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000A9A5 File Offset: 0x00008BA5
		public IXmlNode CreateProcessingInstruction(string target, string data)
		{
			return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000A9B3 File Offset: 0x00008BB3
		public IXmlElement CreateElement(string elementName)
		{
			return new XElementWrapper(new XElement(elementName));
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0000A9C8 File Offset: 0x00008BC8
		public IXmlElement CreateElement(string qualifiedName, string namespaceURI)
		{
			string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
			return new XElementWrapper(new XElement(XName.Get(localName, namespaceURI)));
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0000A9ED File Offset: 0x00008BED
		public IXmlNode CreateAttribute(string name, string value)
		{
			return new XAttributeWrapper(new XAttribute(name, value));
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000AA00 File Offset: 0x00008C00
		public IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value)
		{
			string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
			return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceURI), value));
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000316 RID: 790 RVA: 0x0000AA26 File Offset: 0x00008C26
		public IXmlElement DocumentElement
		{
			get
			{
				if (this.Document.Root == null)
				{
					return null;
				}
				return new XElementWrapper(this.Document.Root);
			}
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0000AA48 File Offset: 0x00008C48
		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			XDeclarationWrapper xdeclarationWrapper = newChild as XDeclarationWrapper;
			if (xdeclarationWrapper != null)
			{
				this.Document.Declaration = xdeclarationWrapper._declaration;
				return xdeclarationWrapper;
			}
			return base.AppendChild(newChild);
		}
	}
}

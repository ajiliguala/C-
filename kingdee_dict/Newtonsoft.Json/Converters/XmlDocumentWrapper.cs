using System;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200004C RID: 76
	internal class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument, IXmlNode
	{
		// Token: 0x060002D2 RID: 722 RVA: 0x0000A56D File Offset: 0x0000876D
		public XmlDocumentWrapper(XmlDocument document) : base(document)
		{
			this._document = document;
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000A57D File Offset: 0x0000877D
		public IXmlNode CreateComment(string data)
		{
			return new XmlNodeWrapper(this._document.CreateComment(data));
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000A590 File Offset: 0x00008790
		public IXmlNode CreateTextNode(string text)
		{
			return new XmlNodeWrapper(this._document.CreateTextNode(text));
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000A5A3 File Offset: 0x000087A3
		public IXmlNode CreateCDataSection(string data)
		{
			return new XmlNodeWrapper(this._document.CreateCDataSection(data));
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000A5B6 File Offset: 0x000087B6
		public IXmlNode CreateWhitespace(string text)
		{
			return new XmlNodeWrapper(this._document.CreateWhitespace(text));
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000A5C9 File Offset: 0x000087C9
		public IXmlNode CreateSignificantWhitespace(string text)
		{
			return new XmlNodeWrapper(this._document.CreateSignificantWhitespace(text));
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000A5DC File Offset: 0x000087DC
		public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new XmlNodeWrapper(this._document.CreateXmlDeclaration(version, encoding, standalone));
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000A5F1 File Offset: 0x000087F1
		public IXmlNode CreateProcessingInstruction(string target, string data)
		{
			return new XmlNodeWrapper(this._document.CreateProcessingInstruction(target, data));
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000A605 File Offset: 0x00008805
		public IXmlElement CreateElement(string elementName)
		{
			return new XmlElementWrapper(this._document.CreateElement(elementName));
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000A618 File Offset: 0x00008818
		public IXmlElement CreateElement(string qualifiedName, string namespaceURI)
		{
			return new XmlElementWrapper(this._document.CreateElement(qualifiedName, namespaceURI));
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000A62C File Offset: 0x0000882C
		public IXmlNode CreateAttribute(string name, string value)
		{
			return new XmlNodeWrapper(this._document.CreateAttribute(name))
			{
				Value = value
			};
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000A654 File Offset: 0x00008854
		public IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value)
		{
			return new XmlNodeWrapper(this._document.CreateAttribute(qualifiedName, namespaceURI))
			{
				Value = value
			};
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0000A67C File Offset: 0x0000887C
		public IXmlElement DocumentElement
		{
			get
			{
				if (this._document.DocumentElement == null)
				{
					return null;
				}
				return new XmlElementWrapper(this._document.DocumentElement);
			}
		}

		// Token: 0x040000EB RID: 235
		private XmlDocument _document;
	}
}

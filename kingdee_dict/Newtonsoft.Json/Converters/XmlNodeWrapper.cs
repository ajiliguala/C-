using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200004A RID: 74
	internal class XmlNodeWrapper : IXmlNode
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x0000A3DD File Offset: 0x000085DD
		public XmlNodeWrapper(XmlNode node)
		{
			this._node = node;
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x0000A3EC File Offset: 0x000085EC
		public object WrappedNode
		{
			get
			{
				return this._node;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x0000A3F4 File Offset: 0x000085F4
		public XmlNodeType NodeType
		{
			get
			{
				return this._node.NodeType;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x0000A401 File Offset: 0x00008601
		public string Name
		{
			get
			{
				return this._node.Name;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060002BA RID: 698 RVA: 0x0000A40E File Offset: 0x0000860E
		public string LocalName
		{
			get
			{
				return this._node.LocalName;
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060002BB RID: 699 RVA: 0x0000A424 File Offset: 0x00008624
		public IList<IXmlNode> ChildNodes
		{
			get
			{
				return (from XmlNode n in this._node.ChildNodes
				select this.WrapNode(n)).ToList<IXmlNode>();
			}
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000A44C File Offset: 0x0000864C
		private IXmlNode WrapNode(XmlNode node)
		{
			XmlNodeType nodeType = node.NodeType;
			if (nodeType == XmlNodeType.Element)
			{
				return new XmlElementWrapper((XmlElement)node);
			}
			if (nodeType != XmlNodeType.XmlDeclaration)
			{
				return new XmlNodeWrapper(node);
			}
			return new XmlDeclarationWrapper((XmlDeclaration)node);
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x060002BD RID: 701 RVA: 0x0000A492 File Offset: 0x00008692
		public IList<IXmlNode> Attributes
		{
			get
			{
				if (this._node.Attributes == null)
				{
					return null;
				}
				return (from XmlAttribute a in this._node.Attributes
				select this.WrapNode(a)).ToList<IXmlNode>();
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060002BE RID: 702 RVA: 0x0000A4CC File Offset: 0x000086CC
		public IXmlNode ParentNode
		{
			get
			{
				XmlNode xmlNode = (this._node is XmlAttribute) ? ((XmlAttribute)this._node).OwnerElement : this._node.ParentNode;
				if (xmlNode == null)
				{
					return null;
				}
				return this.WrapNode(xmlNode);
			}
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060002BF RID: 703 RVA: 0x0000A510 File Offset: 0x00008710
		// (set) Token: 0x060002C0 RID: 704 RVA: 0x0000A51D File Offset: 0x0000871D
		public string Value
		{
			get
			{
				return this._node.Value;
			}
			set
			{
				this._node.Value = value;
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000A52C File Offset: 0x0000872C
		public IXmlNode AppendChild(IXmlNode newChild)
		{
			XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
			this._node.AppendChild(xmlNodeWrapper._node);
			return newChild;
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x0000A553 File Offset: 0x00008753
		public string Prefix
		{
			get
			{
				return this._node.Prefix;
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060002C3 RID: 707 RVA: 0x0000A560 File Offset: 0x00008760
		public string NamespaceURI
		{
			get
			{
				return this._node.NamespaceURI;
			}
		}

		// Token: 0x040000EA RID: 234
		private readonly XmlNode _node;
	}
}

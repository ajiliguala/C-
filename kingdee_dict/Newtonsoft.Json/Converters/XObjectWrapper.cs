using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000051 RID: 81
	internal class XObjectWrapper : IXmlNode
	{
		// Token: 0x060002EF RID: 751 RVA: 0x0000A73C File Offset: 0x0000893C
		public XObjectWrapper(XObject xmlObject)
		{
			this._xmlObject = xmlObject;
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x0000A74B File Offset: 0x0000894B
		public object WrappedNode
		{
			get
			{
				return this._xmlObject;
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x0000A753 File Offset: 0x00008953
		public virtual XmlNodeType NodeType
		{
			get
			{
				return this._xmlObject.NodeType;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000A760 File Offset: 0x00008960
		public virtual string LocalName
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x0000A763 File Offset: 0x00008963
		public virtual IList<IXmlNode> ChildNodes
		{
			get
			{
				return new List<IXmlNode>();
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0000A76A File Offset: 0x0000896A
		public virtual IList<IXmlNode> Attributes
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x0000A76D File Offset: 0x0000896D
		public virtual IXmlNode ParentNode
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x0000A770 File Offset: 0x00008970
		// (set) Token: 0x060002F7 RID: 759 RVA: 0x0000A773 File Offset: 0x00008973
		public virtual string Value
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000A77A File Offset: 0x0000897A
		public virtual IXmlNode AppendChild(IXmlNode newChild)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x0000A781 File Offset: 0x00008981
		public virtual string NamespaceURI
		{
			get
			{
				return null;
			}
		}

		// Token: 0x040000EE RID: 238
		private readonly XObject _xmlObject;
	}
}

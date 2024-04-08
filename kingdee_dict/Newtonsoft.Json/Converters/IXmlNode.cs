using System;
using System.Collections.Generic;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000049 RID: 73
	internal interface IXmlNode
	{
		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060002AC RID: 684
		XmlNodeType NodeType { get; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060002AD RID: 685
		string LocalName { get; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060002AE RID: 686
		IList<IXmlNode> ChildNodes { get; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060002AF RID: 687
		IList<IXmlNode> Attributes { get; }

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x060002B0 RID: 688
		IXmlNode ParentNode { get; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060002B1 RID: 689
		// (set) Token: 0x060002B2 RID: 690
		string Value { get; set; }

		// Token: 0x060002B3 RID: 691
		IXmlNode AppendChild(IXmlNode newChild);

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060002B4 RID: 692
		string NamespaceURI { get; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x060002B5 RID: 693
		object WrappedNode { get; }
	}
}

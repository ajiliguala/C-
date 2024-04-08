using System;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200004D RID: 77
	internal interface IXmlElement : IXmlNode
	{
		// Token: 0x060002DF RID: 735
		void SetAttributeNode(IXmlNode attribute);

		// Token: 0x060002E0 RID: 736
		string GetPrefixOfNamespace(string namespaceURI);
	}
}

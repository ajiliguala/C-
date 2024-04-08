using System;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200004B RID: 75
	internal interface IXmlDocument : IXmlNode
	{
		// Token: 0x060002C6 RID: 710
		IXmlNode CreateComment(string text);

		// Token: 0x060002C7 RID: 711
		IXmlNode CreateTextNode(string text);

		// Token: 0x060002C8 RID: 712
		IXmlNode CreateCDataSection(string data);

		// Token: 0x060002C9 RID: 713
		IXmlNode CreateWhitespace(string text);

		// Token: 0x060002CA RID: 714
		IXmlNode CreateSignificantWhitespace(string text);

		// Token: 0x060002CB RID: 715
		IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone);

		// Token: 0x060002CC RID: 716
		IXmlNode CreateProcessingInstruction(string target, string data);

		// Token: 0x060002CD RID: 717
		IXmlElement CreateElement(string elementName);

		// Token: 0x060002CE RID: 718
		IXmlElement CreateElement(string qualifiedName, string namespaceURI);

		// Token: 0x060002CF RID: 719
		IXmlNode CreateAttribute(string name, string value);

		// Token: 0x060002D0 RID: 720
		IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value);

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060002D1 RID: 721
		IXmlElement DocumentElement { get; }
	}
}

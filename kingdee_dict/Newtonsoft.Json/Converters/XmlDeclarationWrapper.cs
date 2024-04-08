using System;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000050 RID: 80
	internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration, IXmlNode
	{
		// Token: 0x060002E9 RID: 745 RVA: 0x0000A6E9 File Offset: 0x000088E9
		public XmlDeclarationWrapper(XmlDeclaration declaration) : base(declaration)
		{
			this._declaration = declaration;
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060002EA RID: 746 RVA: 0x0000A6F9 File Offset: 0x000088F9
		public string Version
		{
			get
			{
				return this._declaration.Version;
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060002EB RID: 747 RVA: 0x0000A706 File Offset: 0x00008906
		// (set) Token: 0x060002EC RID: 748 RVA: 0x0000A713 File Offset: 0x00008913
		public string Encoding
		{
			get
			{
				return this._declaration.Encoding;
			}
			set
			{
				this._declaration.Encoding = value;
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060002ED RID: 749 RVA: 0x0000A721 File Offset: 0x00008921
		// (set) Token: 0x060002EE RID: 750 RVA: 0x0000A72E File Offset: 0x0000892E
		public string Standalone
		{
			get
			{
				return this._declaration.Standalone;
			}
			set
			{
				this._declaration.Standalone = value;
			}
		}

		// Token: 0x040000ED RID: 237
		private XmlDeclaration _declaration;
	}
}

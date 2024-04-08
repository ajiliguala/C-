using System;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000052 RID: 82
	internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration, IXmlNode
	{
		// Token: 0x060002FA RID: 762 RVA: 0x0000A784 File Offset: 0x00008984
		public XDeclarationWrapper(XDeclaration declaration) : base(null)
		{
			this._declaration = declaration;
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060002FB RID: 763 RVA: 0x0000A794 File Offset: 0x00008994
		public override XmlNodeType NodeType
		{
			get
			{
				return XmlNodeType.XmlDeclaration;
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060002FC RID: 764 RVA: 0x0000A798 File Offset: 0x00008998
		public string Version
		{
			get
			{
				return this._declaration.Version;
			}
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060002FD RID: 765 RVA: 0x0000A7A5 File Offset: 0x000089A5
		// (set) Token: 0x060002FE RID: 766 RVA: 0x0000A7B2 File Offset: 0x000089B2
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

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060002FF RID: 767 RVA: 0x0000A7C0 File Offset: 0x000089C0
		// (set) Token: 0x06000300 RID: 768 RVA: 0x0000A7CD File Offset: 0x000089CD
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

		// Token: 0x040000EF RID: 239
		internal readonly XDeclaration _declaration;
	}
}

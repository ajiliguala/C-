using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000053 RID: 83
	internal class XContainerWrapper : XObjectWrapper
	{
		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000301 RID: 769 RVA: 0x0000A7DB File Offset: 0x000089DB
		private XContainer Container
		{
			get
			{
				return (XContainer)base.WrappedNode;
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000A7E8 File Offset: 0x000089E8
		public XContainerWrapper(XContainer container) : base(container)
		{
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000303 RID: 771 RVA: 0x0000A7F9 File Offset: 0x000089F9
		public override IList<IXmlNode> ChildNodes
		{
			get
			{
				return (from n in this.Container.Nodes()
				select XContainerWrapper.WrapNode(n)).ToList<IXmlNode>();
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000304 RID: 772 RVA: 0x0000A82D File Offset: 0x00008A2D
		public override IXmlNode ParentNode
		{
			get
			{
				if (this.Container.Parent == null)
				{
					return null;
				}
				return XContainerWrapper.WrapNode(this.Container.Parent);
			}
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0000A850 File Offset: 0x00008A50
		internal static IXmlNode WrapNode(XObject node)
		{
			if (node is XDocument)
			{
				return new XDocumentWrapper((XDocument)node);
			}
			if (node is XElement)
			{
				return new XElementWrapper((XElement)node);
			}
			if (node is XContainer)
			{
				return new XContainerWrapper((XContainer)node);
			}
			if (node is XProcessingInstruction)
			{
				return new XProcessingInstructionWrapper((XProcessingInstruction)node);
			}
			if (node is XText)
			{
				return new XTextWrapper((XText)node);
			}
			if (node is XComment)
			{
				return new XCommentWrapper((XComment)node);
			}
			if (node is XAttribute)
			{
				return new XAttributeWrapper((XAttribute)node);
			}
			return new XObjectWrapper(node);
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0000A8EF File Offset: 0x00008AEF
		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			this.Container.Add(newChild.WrappedNode);
			return newChild;
		}
	}
}

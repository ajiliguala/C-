using System;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000057 RID: 87
	internal class XProcessingInstructionWrapper : XObjectWrapper
	{
		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000322 RID: 802 RVA: 0x0000AB1D File Offset: 0x00008D1D
		private XProcessingInstruction ProcessingInstruction
		{
			get
			{
				return (XProcessingInstruction)base.WrappedNode;
			}
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000AB2A File Offset: 0x00008D2A
		public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction) : base(processingInstruction)
		{
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000324 RID: 804 RVA: 0x0000AB33 File Offset: 0x00008D33
		public override string LocalName
		{
			get
			{
				return this.ProcessingInstruction.Target;
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000325 RID: 805 RVA: 0x0000AB40 File Offset: 0x00008D40
		// (set) Token: 0x06000326 RID: 806 RVA: 0x0000AB4D File Offset: 0x00008D4D
		public override string Value
		{
			get
			{
				return this.ProcessingInstruction.Data;
			}
			set
			{
				this.ProcessingInstruction.Data = value;
			}
		}
	}
}

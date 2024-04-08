using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockTranser
{
	// Token: 0x0200005C RID: 92
	[Description("直接调拨单单据类型参数插件")]
	public class StockTransDrtBillTypeParmSetEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000407 RID: 1031 RVA: 0x000309CF File Offset: 0x0002EBCF
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetComValue();
			this.SetControlsVisibility();
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x000309E4 File Offset: 0x0002EBE4
		private void SetControlsVisibility()
		{
			string a = this.View.Model.GetValue("FBusinessType") as string;
			if (a != "CONSIGNMENT")
			{
				this.View.GetControl("FConsignReturnUnrelatedNormal").Visible = false;
				this.View.GetControl("FConsignReturnUnrelatedNormalTips").Visible = false;
			}
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00030A58 File Offset: 0x0002EC58
		private void SetComValue()
		{
			ComboFieldEditor comboFieldEditor = this.View.GetFieldEditor("FBusinessType", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			if (this.View.Context.IsStandardEdition())
			{
				ComboField comboField = this.Model.BusinessInfo.GetElement("FBusinessType") as ComboField;
				List<EnumItem> list = new List<EnumItem>();
				list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
				EnumItem item = (from p in list
				where p.Value == "DRPTRANS"
				select p).FirstOrDefault<EnumItem>();
				list.Remove(item);
				comboFieldEditor.SetComboItems(list);
			}
		}
	}
}

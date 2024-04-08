using System;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Resource;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000079 RID: 121
	public class OutInBillGroupManage : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600058A RID: 1418 RVA: 0x000440D8 File Offset: 0x000422D8
		public override void OnInitialize(InitializeEventArgs e)
		{
			this._formID = (e.Paramter.GetCustomParameter("_FormID") as string);
			this._name = (e.Paramter.GetCustomParameter("_Name") as string);
			if (string.IsNullOrWhiteSpace(this._formID))
			{
				this._status = "ADDNODE";
				return;
			}
			this._formID = this._formID.TrimStart(this._salt.ToArray<char>());
			this._status = "EDITGROUP";
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0004415B File Offset: 0x0004235B
		public override void AfterCreateNewData(EventArgs e)
		{
			this.View.Model.SetValue("FFORMID", this._formID);
			this.View.Model.SetValue("FNAME", this._name);
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x00044194 File Offset: 0x00042394
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBSAVE"))
				{
					if (!(a == "TBCANCE"))
					{
						return;
					}
				}
				else
				{
					this.SaveNewGroup();
				}
			}
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x000441D4 File Offset: 0x000423D4
		private void SaveNewGroup()
		{
			object value = this.View.Model.GetValue("FFORMID");
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("编码必须录入!", "004023030002215", 5, new object[0]), "", 0);
				return;
			}
			LocaleValue localeValue = this.View.Model.GetValue("FNAME") as LocaleValue;
			if (localeValue == null || string.IsNullOrEmpty(localeValue.ToString()))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("名称必须录入!", "004023030002218", 5, new object[0]), "", 0);
				return;
			}
			this.node.id = value.ToString();
			this.node.text = localeValue.ToString();
			this.node.parentid = "";
			this.node.xtype = "system";
			int num = StockServiceHelper.UpdateStockBillInManage(base.Context, this.node, this._status, localeValue, this._formID);
			if (num == 1)
			{
				this.ReturnData();
				return;
			}
			if (num == 100)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("编码重复，请修改!", "004023030002221", 5, new object[0]), "", 0);
			}
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0004431C File Offset: 0x0004251C
		private void ReturnData()
		{
			if (this.node != null && !string.IsNullOrWhiteSpace(this.node.id))
			{
				this.node.id = this._salt + this.node.id;
				this.View.ReturnToParentWindow(this.node);
			}
			this.View.Close();
		}

		// Token: 0x0400021B RID: 539
		private TreeNode node = new TreeNode();

		// Token: 0x0400021C RID: 540
		private string _formID = string.Empty;

		// Token: 0x0400021D RID: 541
		private string _name = string.Empty;

		// Token: 0x0400021E RID: 542
		private string _salt = "";

		// Token: 0x0400021F RID: 543
		private string _status;
	}
}

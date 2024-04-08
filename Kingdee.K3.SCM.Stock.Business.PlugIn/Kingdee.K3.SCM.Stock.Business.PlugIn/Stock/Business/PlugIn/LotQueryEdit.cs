using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000074 RID: 116
	public class LotQueryEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600053E RID: 1342 RVA: 0x00040474 File Offset: 0x0003E674
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (e.Paramter.GetCustomParameter("QueryFilter") != null)
			{
				this._queryFilter = e.Paramter.GetCustomParameter("QueryFilter").ToString();
			}
			if (e.Paramter.GetCustomParameter("StockOrgId") != null)
			{
				this._stockOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("StockOrgId"));
			}
			if (e.Paramter.GetCustomParameter("MaterialId") != null)
			{
				this._materialId = Convert.ToInt64(e.Paramter.GetCustomParameter("MaterialId"));
			}
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x00040510 File Offset: 0x0003E710
		public override void OnLoad(EventArgs e)
		{
			this.View.Model.SetValue("FStockOrgId", this._stockOrgId);
			this.View.Model.SetValue("FMaterialId", this._materialId);
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x0004055D File Offset: 0x0003E75D
		public override void AfterBindData(EventArgs e)
		{
			this.ShowQueryList();
			base.AfterBindData(e);
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x0004056C File Offset: 0x0003E76C
		private void ShowQueryList()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new ListShowParameter();
			dynamicFormShowParameter.FormId = "STK_InvLotQuery";
			dynamicFormShowParameter.OpenStyle.TagetKey = "FInvPanel";
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			this._pageId = SequentialGuid.NewGuid().ToString();
			dynamicFormShowParameter.PageId = this._pageId;
			dynamicFormShowParameter.CustomParams.Add("QueryFilter", this._queryFilter);
			dynamicFormShowParameter.CustomParams.Add("QueryPage", this.View.PageId);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00040608 File Offset: 0x0003E808
		public override void CustomEvents(CustomEventsArgs e)
		{
			if (!e.Key.Equals(this.View.PageId))
			{
				return;
			}
			string a;
			if ((a = e.EventName.ToUpper()) != null)
			{
				if (a == "RETURNDETAILDATA")
				{
					string empty = string.Empty;
					if (this.GetReturnData(out empty))
					{
						this.View.ReturnToParentWindow(empty);
						this.View.Close();
					}
					((IListView)this.View.GetView(this._pageId)).SendDynamicFormAction(this.View);
					return;
				}
				if (!(a == "CLOSEWINDOWBYDETAIL"))
				{
					return;
				}
				this.View.Close();
				((IListView)this.View.GetView(this._pageId)).SendDynamicFormAction(this.View);
			}
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x000406D0 File Offset: 0x0003E8D0
		private bool GetReturnData(out string retData)
		{
			retData = string.Empty;
			List<string> list = new List<string>();
			foreach (ListSelectedRow listSelectedRow in ((IListView)this.View.GetView(this._pageId)).SelectedRowsInfo)
			{
				if (listSelectedRow.Selected && listSelectedRow.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
				{
					list.Add(string.Format("{0}", listSelectedRow.PrimaryKeyValue));
				}
			}
			string text = string.Join("','", list);
			if (text.Length <= 0)
			{
				return true;
			}
			if (StockServiceHelper.IsSameInvData(base.Context, text, " T1.FOWNERTYPEID, T1.FOWNERID"))
			{
				retData = text;
				return true;
			}
			this.View.ShowMessage(ResManager.LoadKDString("只能返回相同货主", "004023030000268", 5, new object[0]), 0);
			return false;
		}

		// Token: 0x040001FA RID: 506
		private string _queryFilter;

		// Token: 0x040001FB RID: 507
		private long _stockOrgId;

		// Token: 0x040001FC RID: 508
		private long _materialId;

		// Token: 0x040001FD RID: 509
		private string _pageId;
	}
}

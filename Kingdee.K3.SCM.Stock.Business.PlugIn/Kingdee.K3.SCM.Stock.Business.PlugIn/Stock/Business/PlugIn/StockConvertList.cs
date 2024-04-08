using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200003E RID: 62
	public class StockConvertList : AbstractListPlugIn
	{
		// Token: 0x06000255 RID: 597 RVA: 0x0001C5D8 File Offset: 0x0001A7D8
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FSTOCKORGID",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "STK_StockParameter",
				BusinessGroupKey = "FSTOCKERGROUPID",
				OperatorType = "WHY"
			});
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.AppendQueryFilter(text);
			}
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0001C660 File Offset: 0x0001A860
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Operation.FormOperation.Operation, "UnAudit"))
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				List<object> list = (from p in selectedRowsInfo
				select p.PrimaryKeyValue).ToList<object>();
				DynamicObject[] dynamicObjectCollection = this.GetDynamicObjectCollection(list.ToArray());
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (!StringUtils.EqualsIgnoreCase(dynamicObject["DocumentStatus"].ToString(), "C") && !StringUtils.EqualsIgnoreCase(dynamicObject["DocumentStatus"].ToString(), "B"))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("单据在提交后才可以执行反审核操作!", "004046030002275", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					object obj = dynamicObject["ConvertReason"];
					if (obj != null && obj.ToString() == "2")
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("库存状态转换单{0}由库存请检单请检冻结生成，不允许反审核。", "004023000019802", 5, new object[0]), dynamicObject["FBillNo"]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
						e.Cancel = true;
						break;
					}
				}
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0001C7E0 File Offset: 0x0001A9E0
		private DynamicObject[] GetDynamicObjectCollection(object[] fid)
		{
			if (this._Metadata == null)
			{
				this._Metadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, true);
			}
			return BusinessDataServiceHelper.Load(base.Context, fid, this._Metadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x040000CD RID: 205
		private FormMetadata _Metadata;
	}
}

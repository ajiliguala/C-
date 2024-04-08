using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200009A RID: 154
	public class InvInitOnOff : AbstractDynamicFormPlugIn
	{
		// Token: 0x060008A4 RID: 2212 RVA: 0x00070AA8 File Offset: 0x0006ECA8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("Direct");
			if (customParameter != null)
			{
				string text = customParameter.ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					string text2 = ResManager.LoadKDString("结束初始化", "004023030000274", 5, new object[0]);
					string text3 = ResManager.LoadKDString("反初始化", "004023030000271", 5, new object[0]);
					this.isOpen = text.Equals("O", StringComparison.OrdinalIgnoreCase);
					this.View.SetFormTitle(new LocaleValue(this.isOpen ? text3 : text2, base.Context.UserLocale.LCID));
					this.View.SetInnerTitle(new LocaleValue(this.isOpen ? text3 : text2, base.Context.UserLocale.LCID));
					this.View.GetMainBarItem("tbAction").Text = (this.isOpen ? text3 : text2);
				}
			}
			this.bShowErr = false;
			this.ShowErrGrid(this.bShowErr);
			this.View.GetControl<EntryGrid>("FEntityAction").SetFireDoubleClickEvent(true);
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x00070BCC File Offset: 0x0006EDCC
		public override void CreateNewData(BizDataEventArgs e)
		{
			DynamicObjectType dynamicObjectType = this.Model.BillBusinessInfo.GetDynamicObjectType();
			Entity entity = this.View.BusinessInfo.Entrys[1];
			DynamicObjectType dynamicObjectType2 = entity.DynamicObjectType;
			DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(dynamicObject);
			BusinessObject businessObject = new BusinessObject
			{
				Id = "STK_Init",
				PermissionControl = 1,
				SubSystemId = "STK"
			};
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, this.isOpen ? "3d609f6910784ac88276b7b432346f15" : "6e010cfad06e4371bbcf87ae7d2f9c44");
			if (permissionOrg == null || permissionOrg.Count < 1)
			{
				e.BizDataObject = dynamicObject;
				return;
			}
			Dictionary<string, object> batchStockDate = StockServiceHelper.GetBatchStockDate(base.Context, permissionOrg);
			if (batchStockDate == null || batchStockDate.Keys.Count < 1)
			{
				e.BizDataObject = dynamicObject;
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FName"));
			list.Add(new SelectorItemInfo("FNumber"));
			list.Add(new SelectorItemInfo("FDescription"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = string.Format(" FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' \r\nAND FORGFUNCTIONS LIKE '%103%' AND {0} EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\nWHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\nAND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ", this.isOpen ? "" : "NOT"),
				OrderByClauseWihtKey = "FNumber",
				IsolationOrgList = null,
				RequiresDataPermission = true
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			int num = 0;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				object obj = null;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					if (permissionOrg.Contains(Convert.ToInt64(dynamicObject2["FORGID"])))
					{
						batchStockDate.TryGetValue(dynamicObject2["FORGID"].ToString(), out obj);
						if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
						{
							DynamicObject dynamicObject3 = new DynamicObject(dynamicObjectType2);
							dynamicObject3["Check"] = true;
							dynamicObject3["StockOrgNo"] = dynamicObject2["FNumber"].ToString();
							dynamicObject3["StockOrgName"] = ((dynamicObject2["FName"] == null || string.IsNullOrEmpty(dynamicObject2["FName"].ToString())) ? "" : dynamicObject2["FName"].ToString());
							dynamicObject3["StockOrgDesc"] = ((dynamicObject2["FDescription"] == null || string.IsNullOrEmpty(dynamicObject2["FDescription"].ToString())) ? "" : dynamicObject2["FDescription"].ToString());
							dynamicObject3["StockOrgID"] = dynamicObject2["FORGID"].ToString();
							dynamicObject3["Result"] = "";
							dynamicObject3["RetFlag"] = false;
							dynamicObject3["Seq"] = ++num;
							value.Add(dynamicObject3);
						}
					}
				}
			}
			e.BizDataObject = dynamicObject;
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x00070F88 File Offset: 0x0006F188
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (!e.Entity.Key.Equals("FEntityErrInfo"))
			{
				return;
			}
			if (this.ret == null || e.Row != 0)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.Entrys[2];
			DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
			DynamicObjectCollection dynamicObjectCollection = entity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			if (dynamicObjectCollection == null)
			{
				dynamicObjectCollection = new DynamicObjectCollection(dynamicObjectType, null);
			}
			dynamicObjectCollection.Clear();
			int index = this.Model.GetEntryCurrentRowIndex("FEntityAction");
			StockOrgOperateResult stockOrgOperateResult = (from p in this.ret
			where p.StockOrgID == Convert.ToInt64(this.Model.GetValue("FStockOrgID", index))
			select p).SingleOrDefault<StockOrgOperateResult>();
			if (stockOrgOperateResult != null)
			{
				foreach (OperateErrorInfo operateErrorInfo in stockOrgOperateResult.ErrInfo)
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["ErrType"] = operateErrorInfo.ErrType;
					dynamicObject["ErrObjType"] = operateErrorInfo.ErrObjType;
					dynamicObject["ErrObjKeyField"] = operateErrorInfo.ErrObjKeyField;
					dynamicObject["ErrObjKeyID"] = operateErrorInfo.ErrObjKeyID;
					dynamicObject["ErrMsg"] = operateErrorInfo.ErrMsg;
					dynamicObjectCollection.Add(dynamicObject);
				}
			}
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00071108 File Offset: 0x0006F308
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbAction"))
				{
					if (barItemKey == "tbErrDetail")
					{
						this.ShowErrGrid(!this.bShowErr);
						return;
					}
					if (!(barItemKey == "tbExit"))
					{
						return;
					}
					this.View.Close();
				}
				else
				{
					string operateName = this.isOpen ? ResManager.LoadKDString("反初始化", "004023030000271", 5, new object[0]) : ResManager.LoadKDString("结束初始化", "004023030000274", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						e.Cancel = true;
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					this.DoAction();
					this.isClicked = true;
					return;
				}
			}
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x000711DF File Offset: 0x0006F3DF
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			if (this.isClicked && e.Key.Equals("FEntityAction", StringComparison.OrdinalIgnoreCase))
			{
				this.RefreshErrEntity();
				this.ShowErrGrid(true);
			}
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x00071248 File Offset: 0x0006F448
		private void DoAction()
		{
			if (this.bBusiness)
			{
				return;
			}
			this.bBusiness = true;
			List<long> list = new List<long>();
			List<string> list2 = new List<string>();
			this.ret = null;
			DynamicObjectCollection entryDataObject = this.Model.DataObject["EntityAction"] as DynamicObjectCollection;
			for (int j = 0; j < this.Model.GetEntryRowCount("FEntityAction"); j++)
			{
				if (Convert.ToBoolean(entryDataObject[j]["Check"]) && !Convert.ToBoolean(entryDataObject[j]["RetFlag"]))
				{
					list.Add(Convert.ToInt64(entryDataObject[j]["StockOrgID"]));
					list2.Add(entryDataObject[j]["StockOrgNo"].ToString());
					this.Model.SetValue("FResult", "", j);
				}
			}
			if (list.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选择未成功处理过的库存组织！", "004023030000277", 5, new object[0]), 0);
			}
			else
			{
				List<NetworkCtrlResult> list3 = this.BatchStartNetCtl(list2);
				if (list3 != null && list3.Count == list2.Count)
				{
					try
					{
						this.ret = StockServiceHelper.InvInitOpenClose(base.Context, list, this.isOpen);
					}
					catch (Exception ex)
					{
						this.View.ShowErrMessage(ex.Message, string.Format(ResManager.LoadKDString("执行{0}失败", "004023030002137", 5, new object[0]), this.isOpen ? ResManager.LoadKDString("反初始化", "004023030000271", 5, new object[0]) : ResManager.LoadKDString("结束初始化", "004023030000274", 5, new object[0])), 0);
					}
				}
				NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list3);
				if (this.ret != null && this.ret.Count > 0)
				{
					int i;
					for (i = 0; i < this.Model.GetEntryRowCount("FEntityAction"); i++)
					{
						if (Convert.ToBoolean(entryDataObject[i]["Check"]))
						{
							StockOrgOperateResult stockOrgOperateResult = (from p in this.ret
							where p.StockOrgID == Convert.ToInt64(entryDataObject[i]["StockOrgID"])
							select p).SingleOrDefault<StockOrgOperateResult>();
							if (stockOrgOperateResult != null)
							{
								this.Model.SetValue("FRetFlag", stockOrgOperateResult.OperateSuccess, i);
								this.Model.SetValue("FResult", stockOrgOperateResult.OperateSuccess ? ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]) : ResManager.LoadKDString("失败", "004023030000253", 5, new object[0]), i);
								this.Model.WriteLog(new LogObject
								{
									ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
									Description = string.Format(ResManager.LoadKDString("库存组织{0}{1}{2}{3}", "004023030000256", 5, new object[0]), new object[]
									{
										stockOrgOperateResult.StockOrgNumber,
										stockOrgOperateResult.StockOrgName,
										this.isOpen ? ResManager.LoadKDString("反初始化", "004023030000271", 5, new object[0]) : ResManager.LoadKDString("结束初始化", "004023030000274", 5, new object[0]),
										stockOrgOperateResult.OperateSuccess ? ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]) : ResManager.LoadKDString("失败", "004023030000253", 5, new object[0])
									}),
									Environment = 3,
									OperateName = (this.isOpen ? ResManager.LoadKDString("反初始化", "004023030000271", 5, new object[0]) : ResManager.LoadKDString("结束初始化", "004023030000274", 5, new object[0])),
									SubSystemId = "21"
								});
							}
						}
					}
				}
				this.RefreshErrEntity();
				this.ShowErrGrid(true);
			}
			this.bBusiness = false;
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x000716C0 File Offset: 0x0006F8C0
		private List<NetworkCtrlResult> BatchStartNetCtl(List<string> orgNum)
		{
			if (orgNum != null)
			{
				List<NetworkCtrlResult> list = null;
				for (int i = 0; i < orgNum.Count; i++)
				{
					NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.AddNetCtrlObj(base.Context, new LocaleValue(base.GetType().Name, 2052), base.GetType().Name, base.GetType().Name + orgNum[i], 6, null, " ", true, true);
					NetworkCtrlServiceHelper.AddMutexNetCtrlObj(base.Context, networkCtrlObject.Id, networkCtrlObject.Id);
					NetWorkRunTimeParam netWorkRunTimeParam = new NetWorkRunTimeParam();
					NetworkCtrlResult networkCtrlResult = NetworkCtrlServiceHelper.BeginNetCtrl(base.Context, networkCtrlObject, netWorkRunTimeParam);
					if (!networkCtrlResult.StartSuccess)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：库存组织编码【%1】正处于结束初始化或反初始化，不允许操作！", "004023030000280", 5, new object[0]), orgNum[i]), "", 0);
						break;
					}
					if (list == null)
					{
						list = new List<NetworkCtrlResult>();
					}
					list.Add(networkCtrlResult);
				}
				return list;
			}
			return null;
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x000717F0 File Offset: 0x0006F9F0
		private void RefreshErrEntity()
		{
			Entity entity = this.View.BusinessInfo.Entrys[2];
			int index = this.Model.GetEntryCurrentRowIndex("FEntityAction");
			if (this.ret == null)
			{
				return;
			}
			this.Model.DeleteEntryData("FEntityErrInfo");
			StockOrgOperateResult stockOrgOperateResult = (from p in this.ret
			where p.StockOrgID == Convert.ToInt64(this.Model.GetValue("FStockOrgID", index))
			select p).SingleOrDefault<StockOrgOperateResult>();
			if (stockOrgOperateResult != null && stockOrgOperateResult.ErrInfo != null && stockOrgOperateResult.ErrInfo.Count > 0)
			{
				this.Model.CreateNewEntryRow(entity, 0);
			}
			this.View.UpdateView("FEntityErrInfo");
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x000718A3 File Offset: 0x0006FAA3
		private void ShowErrGrid(bool isVisible)
		{
			this.View.GetControl<SplitContainer>("FSplitContainer").HideSecondPanel(!isVisible);
			this.bShowErr = isVisible;
		}

		// Token: 0x04000361 RID: 865
		private bool isOpen;

		// Token: 0x04000362 RID: 866
		private bool bBusiness;

		// Token: 0x04000363 RID: 867
		private List<StockOrgOperateResult> ret;

		// Token: 0x04000364 RID: 868
		private bool bShowErr;

		// Token: 0x04000365 RID: 869
		private bool isClicked;
	}
}

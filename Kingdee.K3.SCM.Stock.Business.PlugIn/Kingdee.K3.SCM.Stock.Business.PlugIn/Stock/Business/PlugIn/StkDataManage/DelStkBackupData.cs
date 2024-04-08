using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
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

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StkDataManage
{
	// Token: 0x02000030 RID: 48
	[Description("删除库存备份数据插件")]
	public class DelStkBackupData : AbstractDynamicFormPlugIn
	{
		// Token: 0x060001D7 RID: 471 RVA: 0x00016D40 File Offset: 0x00014F40
		public bool IsPageFilled(string entitykey)
		{
			bool result = false;
			this._pageFilledInfo.TryGetValue(entitykey, out result);
			return result;
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x00016D5F File Offset: 0x00014F5F
		public void SetPageFilled(string entitykey, bool isFilled)
		{
			this._pageFilledInfo[entitykey] = isFilled;
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060001D9 RID: 473 RVA: 0x00016D6E File Offset: 0x00014F6E
		public string CurEntityKey
		{
			get
			{
				return this._curEntityKey;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060001DA RID: 474 RVA: 0x00016D76 File Offset: 0x00014F76
		public string CurEntityPropertyName
		{
			get
			{
				return this._curEntityPtyName;
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00016D80 File Offset: 0x00014F80
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (this.View.BillBusinessInfo != null)
			{
				TabControl control = this.View.GetControl<TabControl>("FTabFunc");
				if (control != null)
				{
					control.SetFireSelChanged(true);
				}
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x00016DBC File Offset: 0x00014FBC
		public override void OnLoad(EventArgs e)
		{
			TabControl control = this.View.GetControl<TabControl>("FTabFunc");
			if (control != null)
			{
				control.SelectedIndex = 0;
				this.SwitchCurrentEntity("FTab_PInvLog");
			}
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00016DF0 File Offset: 0x00014FF0
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBACTION"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("删除", "004023000018997", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.DoAction();
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00016E70 File Offset: 0x00015070
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			base.TabItemSelectedChange(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FTABFUNC"))
				{
					return;
				}
				this.SwitchCurrentEntity(e.TabKey);
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00016EB0 File Offset: 0x000150B0
		public virtual void SwitchCurrentEntity(string tabKey)
		{
			this._curEntityKey = "";
			string key;
			switch (key = tabKey.ToUpper())
			{
			case "FTAB_PINVBAL":
				this._curEntityKey = "FEntityInvBal";
				this._curEntityPtyName = "EntityInvBal";
				break;
			case "FTAB_PINVLOG":
				this._curEntityKey = "FEntityInvLog";
				this._curEntityPtyName = "EntityInvLog";
				break;
			case "FTAB_PSTKLOCKLOG":
				this._curEntityKey = "FEntityLockLog";
				this._curEntityPtyName = "EntityLockLog";
				break;
			case "FTAB_PLOTTRACE":
				this._curEntityKey = "FEntityLotTrace";
				this._curEntityPtyName = "EntityLotTrace";
				break;
			case "FTAB_PBALOCCURLOG":
				this._curEntityKey = "FEntityBalOccurLog";
				this._curEntityPtyName = "EntityBalOccurLog";
				break;
			case "FTAB_PBALRESULTLOG":
				this._curEntityKey = "FEntityBalResultLog";
				this._curEntityPtyName = "EntityBalResultLog";
				break;
			}
			this.InitCurrentEntity();
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00016FFE File Offset: 0x000151FE
		public virtual void InitCurrentEntity()
		{
			if (string.IsNullOrWhiteSpace(this._curEntityKey))
			{
				return;
			}
			if (this.IsPageFilled(this._curEntityKey))
			{
				return;
			}
			this.InitEntityData();
			this.SetPageFilled(this._curEntityKey, true);
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00017030 File Offset: 0x00015230
		private void InitEntityData()
		{
			string funMark = this.GetFunMark(this._curEntityKey);
			if (string.IsNullOrWhiteSpace(funMark))
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity(this._curEntityKey);
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection value = entryEntity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			value.Clear();
			List<long> permitedOrgs = this.GetPermitedOrgs();
			if (permitedOrgs == null || permitedOrgs.Count < 1)
			{
				return;
			}
			Dictionary<long, DateTime> stockLastBackupDate = StockServiceHelper.GetStockLastBackupDate(base.Context, "STK", funMark, permitedOrgs);
			this.BuildEntityDatas(funMark, entryEntity, dynamicObjectType, value, stockLastBackupDate);
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x000170C8 File Offset: 0x000152C8
		public string GetFunMark(string entitykey)
		{
			string result = "";
			if (string.IsNullOrWhiteSpace(entitykey))
			{
				return result;
			}
			string key;
			switch (key = entitykey.ToUpper())
			{
			case "FENTITYINVBAL":
				result = "InvBal";
				break;
			case "FENTITYINVLOG":
				result = "InvLog";
				break;
			case "FENTITYLOCKLOG":
				result = "LockLog";
				break;
			case "FENTITYLOTTRACE":
				result = "LotTrace";
				break;
			case "FENTITYBALOCCURLOG":
				result = "BalOccurLog";
				break;
			case "FENTITYBALRESULTLOG":
				result = "BalResultLog";
				break;
			}
			return result;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x000171D4 File Offset: 0x000153D4
		private List<long> GetPermitedOrgs()
		{
			if (this.orgInfos != null && this.orgInfos.Count > 0)
			{
				return (from p in this.orgInfos
				select Convert.ToInt64(p["FORGID"])).ToList<long>();
			}
			BusinessObject businessObject = new BusinessObject
			{
				Id = "STK_DelStkBackupData",
				PermissionControl = 1,
				SubSystemId = "STK"
			};
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "24f64c0dbfa945f78a6be123197a63f5");
			if (permissionOrg == null || permissionOrg.Count < 1)
			{
				return null;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FName"));
			list.Add(new SelectorItemInfo("FNumber"));
			list.Add(new SelectorItemInfo("FDescription"));
			string text = this.GetInFilter(" FORGID", permissionOrg);
			text += " AND FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' ";
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = text,
				OrderByClauseWihtKey = "FNUMBER",
				IsolationOrgList = null,
				RequiresDataPermission = true
			};
			this.orgInfos = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null).ToList<DynamicObject>();
			return (from p in this.orgInfos
			select Convert.ToInt64(p["FORGID"])).ToList<long>();
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00017358 File Offset: 0x00015558
		private void BuildEntityDatas(string funcMark, EntryEntity actionEntity, DynamicObjectType actionObjType, DynamicObjectCollection entryDataObject, Dictionary<long, DateTime> lastBackDates)
		{
			if (this.orgInfos == null || this.orgInfos.Count < 1 || lastBackDates == null || lastBackDates.Count < 1)
			{
				return;
			}
			int num = 1;
			foreach (DynamicObject dynamicObject in this.orgInfos)
			{
				long key = Convert.ToInt64(dynamicObject["FORGID"]);
				DateTime minValue = DateTime.MinValue;
				lastBackDates.TryGetValue(key, out minValue);
				if (minValue > DateTime.MinValue)
				{
					DynamicObject dynamicObject2 = new DynamicObject(actionObjType);
					dynamicObject2[funcMark + "Sel"] = false;
					dynamicObject2[funcMark + "OrgNumber"] = dynamicObject["FNumber"].ToString();
					dynamicObject2[funcMark + "OrgName"] = ((dynamicObject["FName"] == null || string.IsNullOrEmpty(dynamicObject["FName"].ToString())) ? "" : dynamicObject["FName"].ToString());
					dynamicObject2[funcMark + "OrgDesc"] = ((dynamicObject["FDescription"] == null || string.IsNullOrEmpty(dynamicObject["FDescription"].ToString())) ? "" : dynamicObject["FDescription"].ToString());
					dynamicObject2[funcMark + "LastBackDate"] = lastBackDates[key];
					dynamicObject2[funcMark + "Message"] = "";
					dynamicObject2[funcMark + "OrgId"] = dynamicObject["FORGID"].ToString();
					dynamicObject2[funcMark + "Result"] = "0";
					dynamicObject2["Seq"] = num++;
					entryDataObject.Add(dynamicObject2);
				}
			}
			this.RefreshDataExInfo(funcMark, true);
			this.View.UpdateView(this._curEntityKey);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x000175E0 File Offset: 0x000157E0
		public virtual void DoAction()
		{
			List<KeyValuePair<long, DateTime>> selOrgs = new List<KeyValuePair<long, DateTime>>();
			List<string> orgNums = new List<string>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[this._curEntityPtyName] as DynamicObjectCollection;
			string funcMark = this.GetFunMark(this._curEntityKey);
			StringBuilder stringBuilder = new StringBuilder();
			string format = ResManager.LoadKDString("组织【{0}】【{1}】", "004023000018995", 5, new object[0]);
			for (int i = 0; i < this.Model.GetEntryRowCount(this._curEntityKey); i++)
			{
				if (Convert.ToBoolean(dynamicObjectCollection[i][funcMark + "Sel"]))
				{
					string text = this.CheckActionData(funcMark, dynamicObjectCollection[i]);
					if (string.IsNullOrWhiteSpace(text))
					{
						selOrgs.Add(new KeyValuePair<long, DateTime>(Convert.ToInt64(dynamicObjectCollection[i][funcMark + "OrgId"]), Convert.ToDateTime(dynamicObjectCollection[i][funcMark + "ActionDate"])));
						orgNums.Add(dynamicObjectCollection[i][funcMark + "OrgNumber"].ToString());
						dynamicObjectCollection[i][funcMark + "Message"] = "";
						dynamicObjectCollection[i][funcMark + "Result"] = "0";
					}
					else
					{
						dynamicObjectCollection[i][funcMark + "Message"] = text;
						dynamicObjectCollection[i][funcMark + "Result"] = "0";
						string arg = string.Format(format, dynamicObjectCollection[i][funcMark + "OrgNumber"].ToString(), dynamicObjectCollection[i][funcMark + "OrgName"].ToString());
						stringBuilder.AppendLine(string.Format("{0}{1}", arg, text));
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				this.View.UpdateView(this._curEntityKey);
				this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				return;
			}
			if (selOrgs.Count < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请先选择要执行操作的组织并为组织设置符合条件的执行日期后再执行操作！", "004023000018996", 5, new object[0]), "", 0);
				return;
			}
			string actionCheckWarnMsg = this.GetActionCheckWarnMsg();
			if (!string.IsNullOrWhiteSpace(actionCheckWarnMsg))
			{
				this.View.ShowWarnningMessage("", actionCheckWarnMsg, 4, delegate(MessageBoxResult reslt)
				{
					if (reslt == 6)
					{
						List<StockOrgOperateResult> ret2 = this.DoOrgBackDataDelete(funcMark, selOrgs, orgNums);
						this.RefreshOrgActionResult(funcMark, ret2);
					}
				}, 1);
				return;
			}
			List<StockOrgOperateResult> ret = this.DoOrgBackDataDelete(funcMark, selOrgs, orgNums);
			this.RefreshOrgActionResult(funcMark, ret);
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00017914 File Offset: 0x00015B14
		private List<StockOrgOperateResult> DoOrgBackDataDelete(string funcMark, List<KeyValuePair<long, DateTime>> selOrgs, List<string> orgNums)
		{
			if (this.isbBusiness)
			{
				this.View.ShowMessage(ResManager.LoadKDString("上次提交未执行完毕，请稍后再试", "004023030002134", 5, new object[0]), 0);
				return null;
			}
			this.isbBusiness = true;
			List<StockOrgOperateResult> result = null;
			List<NetworkCtrlResult> list = this.BatchStartNetCtl(orgNums);
			if (list != null && list.Count == orgNums.Count)
			{
				try
				{
					result = StockServiceHelper.STKDataTableOperate(base.Context, "Delete", funcMark, selOrgs);
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message, string.Format(ResManager.LoadKDString("执行{0}失败", "004023030002137", 5, new object[0]), ResManager.LoadKDString("删除", "004023000018997", 5, new object[0])), 0);
				}
			}
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list);
			this.isbBusiness = false;
			return result;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00017A08 File Offset: 0x00015C08
		private void RefreshOrgActionResult(string funcMark, List<StockOrgOperateResult> ret)
		{
			string text = ResManager.LoadKDString("由于未知原因，操作执行失败!", "004023000018998", 5, new object[0]);
			if (ret == null || ret.Count < 1)
			{
				this.View.ShowMessage(text, 0);
				return;
			}
			string text2 = ResManager.LoadKDString("删除", "004023000018997", 5, new object[0]);
			string text3 = ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]);
			string text4 = ResManager.LoadKDString("失败", "004023030000253", 5, new object[0]);
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[this._curEntityPtyName] as DynamicObjectCollection;
			List<long> list = new List<long>();
			for (int i = 0; i < this.Model.GetEntryRowCount(this._curEntityKey); i++)
			{
				if (Convert.ToBoolean(dynamicObjectCollection[i][funcMark + "Sel"]))
				{
					list.Add(Convert.ToInt64(dynamicObjectCollection[i][funcMark + "OrgId"]));
				}
			}
			Dictionary<long, DateTime> dictionary = new Dictionary<long, DateTime>();
			if (list.Count > 0)
			{
				dictionary = StockServiceHelper.GetStockLastBackupDate(base.Context, "STK", funcMark, list);
			}
			int j = 0;
			while (j < this.Model.GetEntryRowCount(this._curEntityKey))
			{
				if (!Convert.ToBoolean(dynamicObjectCollection[j][funcMark + "Sel"]))
				{
					goto IL_37A;
				}
				long orgId = Convert.ToInt64(dynamicObjectCollection[j][funcMark + "OrgId"]);
				DateTime minValue = DateTime.MinValue;
				StockOrgOperateResult stockOrgOperateResult = ret.FirstOrDefault((StockOrgOperateResult p) => p.StockOrgID == orgId);
				if (stockOrgOperateResult != null)
				{
					if (stockOrgOperateResult.ErrInfo.Count > 0)
					{
						string errMsg = stockOrgOperateResult.ErrInfo[0].ErrMsg;
						if (dictionary != null)
						{
							dictionary.TryGetValue(orgId, out minValue);
						}
						if (stockOrgOperateResult.OperateSuccess)
						{
							DateTime minValue2 = DateTime.MinValue;
							DateTime minValue3 = DateTime.MinValue;
							DateTime minValue4 = DateTime.MinValue;
							if (minValue == DateTime.MinValue)
							{
								dynamicObjectCollection[j][funcMark + "LastBackDate"] = null;
							}
							else
							{
								dynamicObjectCollection[j][funcMark + "LastBackDate"] = minValue;
							}
							dynamicObjectCollection[j][funcMark + "Result"] = "1";
						}
						else
						{
							dynamicObjectCollection[j][funcMark + "Result"] = "0";
						}
						dynamicObjectCollection[j][funcMark + "Message"] = errMsg;
						string text5 = dynamicObjectCollection[j][funcMark + "OrgNumber"].ToString();
						string text6 = dynamicObjectCollection[j][funcMark + "OrgName"].ToString();
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = string.Format(ResManager.LoadKDString("库存组织{0}{1}库存数据表{2}{3}", "004023000018999", 5, new object[0]), new object[]
							{
								text5,
								text6,
								text2,
								stockOrgOperateResult.OperateSuccess ? text3 : text4
							}),
							Environment = 3,
							OperateName = text2,
							SubSystemId = "21"
						});
						goto IL_37A;
					}
					goto IL_37A;
				}
				IL_393:
				j++;
				continue;
				IL_37A:
				this.RefreshDataExInfo(funcMark, false);
				this.View.UpdateView(this._curEntityKey);
				goto IL_393;
			}
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00017DC8 File Offset: 0x00015FC8
		private string GetActionCheckWarnMsg()
		{
			if (string.IsNullOrWhiteSpace(this._curEntityKey))
			{
				return "";
			}
			string result = "";
			string key;
			switch (key = this._curEntityKey.ToUpper())
			{
			}
			return result;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00017E90 File Offset: 0x00016090
		private string CheckActionData(string funcMark, DynamicObject dyRowData)
		{
			if (string.IsNullOrWhiteSpace(this._curEntityKey))
			{
				return "";
			}
			DateTime dateTime = DateTime.MinValue;
			DateTime dateTime2 = DateTime.MinValue;
			object obj = dyRowData[funcMark + "ActionDate"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				dateTime2 = Convert.ToDateTime(obj.ToString()).Date;
			}
			if (dateTime2 == DateTime.MinValue)
			{
				return ResManager.LoadKDString("未录入执行日期！", "004023000019000", 5, new object[0]);
			}
			obj = dyRowData[funcMark + "LastBackDate"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				dateTime = Convert.ToDateTime(obj.ToString()).Date;
			}
			if (dateTime == DateTime.MinValue)
			{
				return ResManager.LoadKDString("未执行过备份操作，不允许执行数据删除！", "004023000019001", 5, new object[0]);
			}
			if (dateTime2 > dateTime)
			{
				return ResManager.LoadKDString("执行日期不能大于最近备份日期！", "004023000019002", 5, new object[0]);
			}
			string result = "";
			string key;
			switch (key = this._curEntityKey.ToUpper())
			{
			}
			return result;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00018070 File Offset: 0x00016270
		private void RefreshDataExInfo(string funcMark, bool refreshAllRows)
		{
			DynamicObjectCollection stkOpTableDataInfos = StockServiceHelper.GetStkOpTableDataInfos(base.Context, true, funcMark, (from p in this.orgInfos
			select Convert.ToInt64(p["FORGID"])).ToList<long>());
			new List<long>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[this._curEntityPtyName] as DynamicObjectCollection;
			for (int i = 0; i < this.Model.GetEntryRowCount(this._curEntityKey); i++)
			{
				if (refreshAllRows || Convert.ToBoolean(dynamicObjectCollection[i][funcMark + "Sel"]))
				{
					long orgId = Convert.ToInt64(dynamicObjectCollection[i][funcMark + "OrgId"]);
					DynamicObject dynamicObject = null;
					if (stkOpTableDataInfos != null)
					{
						dynamicObject = stkOpTableDataInfos.FirstOrDefault((DynamicObject p) => Convert.ToInt64(p["FORGID"]) == orgId);
					}
					if (dynamicObject != null)
					{
						dynamicObjectCollection[i][funcMark + "DataCount"] = Convert.ToInt64(dynamicObject["FDATACOUNT"]);
						dynamicObjectCollection[i][funcMark + "MinDateTime"] = dynamicObject["FMINDATETIME"];
					}
					else
					{
						dynamicObjectCollection[i][funcMark + "DataCount"] = 0;
						dynamicObjectCollection[i][funcMark + "MinDateTime"] = null;
					}
				}
			}
		}

		// Token: 0x060001EB RID: 491 RVA: 0x000181FB File Offset: 0x000163FB
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count < 1)
			{
				return string.Format(" {0} = -1 ", key);
			}
			return string.Format(" {0} in ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0001822C File Offset: 0x0001642C
		public List<NetworkCtrlResult> BatchStartNetCtl(List<string> orgNum)
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
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：组织编码[{0}]正在进行库存数据表的删除，不允许操作！", "004023000019050", 5, new object[0]), orgNum[i]), "", 0);
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

		// Token: 0x040000AA RID: 170
		private Dictionary<string, bool> _pageFilledInfo = new Dictionary<string, bool>();

		// Token: 0x040000AB RID: 171
		private string _curEntityKey = "";

		// Token: 0x040000AC RID: 172
		private string _curEntityPtyName = "";

		// Token: 0x040000AD RID: 173
		private List<DynamicObject> orgInfos;

		// Token: 0x040000AE RID: 174
		private bool isbBusiness;
	}
}

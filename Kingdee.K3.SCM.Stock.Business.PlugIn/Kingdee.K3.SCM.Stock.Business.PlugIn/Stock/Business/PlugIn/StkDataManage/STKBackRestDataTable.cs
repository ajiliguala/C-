using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
	// Token: 0x02000031 RID: 49
	[Description("备份还原库存数据插件")]
	public class STKBackRestDataTable : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060001F1 RID: 497 RVA: 0x00018350 File Offset: 0x00016550
		public bool IsRestore
		{
			get
			{
				return this._isRestore;
			}
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00018358 File Offset: 0x00016558
		public bool IsPageFilled(string entitykey)
		{
			bool result = false;
			this._pageFilledInfo.TryGetValue(entitykey, out result);
			return result;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00018377 File Offset: 0x00016577
		public void SetPageFilled(string entitykey, bool isFilled)
		{
			this._pageFilledInfo[entitykey] = isFilled;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060001F4 RID: 500 RVA: 0x00018386 File Offset: 0x00016586
		public string CurEntityKey
		{
			get
			{
				return this._curEntityKey;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060001F5 RID: 501 RVA: 0x0001838E File Offset: 0x0001658E
		public string CurEntityPropertyName
		{
			get
			{
				return this._curEntityPtyName;
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00018398 File Offset: 0x00016598
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._isRestore = false;
			object customParameter = this.View.OpenParameter.GetCustomParameter("Action");
			if (customParameter != null)
			{
				string text = customParameter.ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					this._isRestore = text.Equals("R", StringComparison.OrdinalIgnoreCase);
				}
			}
			if (this.View.BillBusinessInfo != null)
			{
				TabControl control = this.View.GetControl<TabControl>("FTabFunc");
				if (control != null)
				{
					control.SetFireSelChanged(true);
				}
			}
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00018418 File Offset: 0x00016618
		public override void OnLoad(EventArgs e)
		{
			this.SetBarTitle();
			TabControl control = this.View.GetControl<TabControl>("FTabFunc");
			if (control != null)
			{
				control.SelectedIndex = 0;
				this.SwitchCurrentEntity("FTab_PInvBal");
			}
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00018454 File Offset: 0x00016654
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
				string operateName = this._isRestore ? ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]) : ResManager.LoadKDString("备份", "004023000019003", 5, new object[0]);
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

		// Token: 0x060001F9 RID: 505 RVA: 0x000184F4 File Offset: 0x000166F4
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

		// Token: 0x060001FA RID: 506 RVA: 0x00018534 File Offset: 0x00016734
		private void SetBarTitle()
		{
			string text = ResManager.LoadKDString("备份", "004023000019003", 5, new object[0]);
			string text2 = ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]);
			string text3 = this._isRestore ? text2 : text;
			this.View.GetBarItem("FEntityInvBal", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityInvBal", "tbAction").ToolTip = text3;
			this.View.GetBarItem("FEntityInvLog", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityInvLog", "tbAction").ToolTip = text3;
			this.View.GetBarItem("FEntityLockLog", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityLockLog", "tbAction").ToolTip = text3;
			this.View.GetBarItem("FEntityLotTrace", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityLotTrace", "tbAction").ToolTip = text3;
			this.View.GetBarItem("FEntityBalOccurLog", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityBalOccurLog", "tbAction").ToolTip = text3;
			this.View.GetBarItem("FEntityBalResultLog", "tbAction").Text = text3;
			this.View.GetBarItem("FEntityBalResultLog", "tbAction").ToolTip = text3;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x000186C0 File Offset: 0x000168C0
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

		// Token: 0x060001FC RID: 508 RVA: 0x0001880E File Offset: 0x00016A0E
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

		// Token: 0x060001FD RID: 509 RVA: 0x00018840 File Offset: 0x00016A40
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
			List<long> list = this.GetPermitedOrgs();
			if (list == null || list.Count < 1)
			{
				return;
			}
			if (funMark != "LotTrace")
			{
				list = this.GetStockOrgLastCloseInfo(list);
				if (list == null || list.Count < 1)
				{
					return;
				}
			}
			Dictionary<long, DateTime> stockLastBackupDate = StockServiceHelper.GetStockLastBackupDate(base.Context, "STK", funMark, list);
			this.BuildEntityDatas(funMark, entryEntity, dynamicObjectType, value, stockLastBackupDate);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x000188FC File Offset: 0x00016AFC
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

		// Token: 0x060001FF RID: 511 RVA: 0x00018A08 File Offset: 0x00016C08
		private List<long> GetPermitedOrgs()
		{
			if (this.orgInfos != null && this.orgInfos.Count > 0)
			{
				return (from p in this.orgInfos
				select Convert.ToInt64(p["FORGID"])).ToList<long>();
			}
			BusinessObject businessObject = new BusinessObject
			{
				Id = "STK_BackRestDataTable",
				PermissionControl = 1,
				SubSystemId = "STK"
			};
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, this._isRestore ? "00505694265cb6cf11e3b590d1da8712" : "552e0a3b43553f");
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

		// Token: 0x06000200 RID: 512 RVA: 0x00018B9C File Offset: 0x00016D9C
		private List<long> GetStockOrgLastCloseInfo(List<long> orgList)
		{
			if (this.lastCloseDates != null)
			{
				return this.lastCloseDates.Keys.ToList<long>();
			}
			List<long> list = new List<long>();
			this.lastCloseDates = new Dictionary<long, DateTime>();
			DataTable stockOrgAcctLastCloseDate = CommonServiceHelper.GetStockOrgAcctLastCloseDate(base.Context, string.Join<long>(",", orgList));
			foreach (object obj in stockOrgAcctLastCloseDate.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				if (!(dataRow["FCLOSEDATE"] is DBNull) && !string.IsNullOrWhiteSpace(dataRow["FCLOSEDATE"].ToString()))
				{
					long num = Convert.ToInt64(dataRow["FORGID"]);
					this.lastCloseDates[num] = Convert.ToDateTime(dataRow["FCLOSEDATE"]);
					list.Add(num);
				}
			}
			return list;
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00018C94 File Offset: 0x00016E94
		private void BuildEntityDatas(string funcMark, EntryEntity actionEntity, DynamicObjectType actionObjType, DynamicObjectCollection entryDataObject, Dictionary<long, DateTime> lastBackDates)
		{
			if (this.orgInfos == null || this.orgInfos.Count < 1)
			{
				return;
			}
			if (funcMark != "LotTrace" && (this.lastCloseDates == null || this.lastCloseDates.Count < 1))
			{
				return;
			}
			int num = 1;
			foreach (DynamicObject dynamicObject in this.orgInfos)
			{
				long key = Convert.ToInt64(dynamicObject["FORGID"]);
				if (!(funcMark != "LotTrace") || this.lastCloseDates.ContainsKey(key))
				{
					DynamicObject dynamicObject2 = new DynamicObject(actionObjType);
					dynamicObject2[funcMark + "Sel"] = false;
					dynamicObject2[funcMark + "OrgNumber"] = dynamicObject["FNumber"].ToString();
					dynamicObject2[funcMark + "OrgName"] = ((dynamicObject["FName"] == null || string.IsNullOrEmpty(dynamicObject["FName"].ToString())) ? "" : dynamicObject["FName"].ToString());
					dynamicObject2[funcMark + "OrgDesc"] = ((dynamicObject["FDescription"] == null || string.IsNullOrEmpty(dynamicObject["FDescription"].ToString())) ? "" : dynamicObject["FDescription"].ToString());
					if (this.lastCloseDates != null && this.lastCloseDates.ContainsKey(key))
					{
						dynamicObject2[funcMark + "LastCloseDate"] = this.lastCloseDates[key];
					}
					else
					{
						dynamicObject2[funcMark + "LastCloseDate"] = null;
					}
					if (lastBackDates != null && lastBackDates.ContainsKey(key))
					{
						dynamicObject2[funcMark + "LastBackDate"] = lastBackDates[key];
					}
					else
					{
						dynamicObject2[funcMark + "LastBackDate"] = null;
					}
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

		// Token: 0x06000202 RID: 514 RVA: 0x00018F94 File Offset: 0x00017194
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
						List<StockOrgOperateResult> ret2 = this.DoOrgBackRestore(funcMark, selOrgs, orgNums);
						this.RefreshOrgActionResult(funcMark, ret2);
					}
				}, 1);
				return;
			}
			List<StockOrgOperateResult> ret = this.DoOrgBackRestore(funcMark, selOrgs, orgNums);
			this.RefreshOrgActionResult(funcMark, ret);
		}

		// Token: 0x06000203 RID: 515 RVA: 0x000192C8 File Offset: 0x000174C8
		private List<StockOrgOperateResult> DoOrgBackRestore(string funcMark, List<KeyValuePair<long, DateTime>> selOrgs, List<string> orgNums)
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
					result = StockServiceHelper.STKDataTableOperate(base.Context, this.IsRestore ? "Restore" : "BackUp", funcMark, selOrgs);
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message, string.Format(ResManager.LoadKDString("执行{0}失败", "004023030002137", 5, new object[0]), this.IsRestore ? ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]) : ResManager.LoadKDString("备份", "004023000019003", 5, new object[0])), 0);
				}
			}
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list);
			this.isbBusiness = false;
			return result;
		}

		// Token: 0x06000204 RID: 516 RVA: 0x000193F0 File Offset: 0x000175F0
		private void RefreshOrgActionResult(string funcMark, List<StockOrgOperateResult> ret)
		{
			string text = ResManager.LoadKDString("由于未知原因，操作执行失败!", "004023000018998", 5, new object[0]);
			if (ret == null || ret.Count < 1)
			{
				this.View.ShowMessage(text, 0);
				return;
			}
			string text2 = this.IsRestore ? ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]) : ResManager.LoadKDString("备份", "004023000019003", 5, new object[0]);
			string text3 = ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]);
			string text4 = ResManager.LoadKDString("失败", "004023030000253", 5, new object[0]);
			List<long> list = new List<long>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[this._curEntityPtyName] as DynamicObjectCollection;
			for (int i = 0; i < this.Model.GetEntryRowCount(this._curEntityKey); i++)
			{
				if (Convert.ToBoolean(dynamicObjectCollection[i][funcMark + "Sel"]))
				{
					long orgId = Convert.ToInt64(dynamicObjectCollection[i][funcMark + "OrgId"]);
					string text5 = text;
					StockOrgOperateResult stockOrgOperateResult = ret.FirstOrDefault((StockOrgOperateResult p) => p.StockOrgID == orgId);
					if (stockOrgOperateResult != null)
					{
						if (stockOrgOperateResult.ErrInfo.Count > 0)
						{
							text5 = stockOrgOperateResult.ErrInfo[0].ErrMsg;
						}
						if (stockOrgOperateResult.OperateSuccess)
						{
							if (this.IsRestore)
							{
								list.Add(orgId);
							}
							else
							{
								dynamicObjectCollection[i][funcMark + "LastBackDate"] = dynamicObjectCollection[i][funcMark + "ActionDate"];
							}
							dynamicObjectCollection[i][funcMark + "Result"] = "1";
						}
						else
						{
							dynamicObjectCollection[i][funcMark + "Result"] = "0";
						}
						dynamicObjectCollection[i][funcMark + "Message"] = text5;
						string text6 = dynamicObjectCollection[i][funcMark + "OrgNumber"].ToString();
						string text7 = dynamicObjectCollection[i][funcMark + "OrgName"].ToString();
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = string.Format(ResManager.LoadKDString("库存组织{0}{1}库存数据表{2}{3}", "004023000018999", 5, new object[0]), new object[]
							{
								text6,
								text7,
								text2,
								stockOrgOperateResult.OperateSuccess ? text3 : text4
							}),
							Environment = 3,
							OperateName = text2,
							SubSystemId = "21"
						});
					}
				}
			}
			if (list.Count > 0)
			{
				this.RefreshRowLastBackupDate(funcMark, list);
				this.RefreshDataExInfo(funcMark, false);
			}
			this.View.UpdateView(this._curEntityKey);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00019728 File Offset: 0x00017928
		private void RefreshRowLastBackupDate(string funcMark, List<long> orgIds)
		{
			Dictionary<long, DateTime> stockLastBackupDate = StockServiceHelper.GetStockLastBackupDate(base.Context, "STK", funcMark, orgIds);
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[this._curEntityPtyName] as DynamicObjectCollection;
			for (int i = 0; i < this.Model.GetEntryRowCount(this._curEntityKey); i++)
			{
				long num = Convert.ToInt64(dynamicObjectCollection[i][funcMark + "OrgId"]);
				if (orgIds.Contains(num))
				{
					if (stockLastBackupDate != null && stockLastBackupDate.ContainsKey(num))
					{
						dynamicObjectCollection[i][funcMark + "LastBackDate"] = stockLastBackupDate[num];
					}
					else
					{
						dynamicObjectCollection[i][funcMark + "LastBackDate"] = null;
					}
				}
			}
		}

		// Token: 0x06000206 RID: 518 RVA: 0x000197F8 File Offset: 0x000179F8
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
			case "FENTITYINVBAL":
				if (!this._isRestore)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(ResManager.LoadKDString("库存余额数据备份后库存报表将只能查询起始日期晚于最近备份日期的数据，如果需要查询早于备份日期的数据则需要先执行还原操作。", "004023030009290", 5, new object[0]));
					stringBuilder.AppendLine(ResManager.LoadKDString("是否继续执行备份操作？", "004023000019005", 5, new object[0]));
					result = stringBuilder.ToString();
				}
				break;
			case "FENTITYLOTTRACE":
				if (!this._isRestore)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					stringBuilder2.AppendLine(ResManager.LoadKDString("批号追踪数据备份后在批号主档中将无法查询到单据业务日期早于最近备份日期的追踪数据，如果需要在批号主档中查询早于备份日期的追踪数据则需要先执行还原操作。", "004023000019006", 5, new object[0]));
					stringBuilder2.AppendLine(ResManager.LoadKDString("是否继续执行备份操作？", "004023000019005", 5, new object[0]));
					result = stringBuilder2.ToString();
				}
				break;
			}
			return result;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0001996C File Offset: 0x00017B6C
		private string CheckActionData(string funcMark, DynamicObject dyRowData)
		{
			if (string.IsNullOrWhiteSpace(this._curEntityKey))
			{
				return "";
			}
			DateTime dateTime = DateTime.MinValue;
			DateTime dateTime2 = DateTime.MinValue;
			DateTime dateTime3 = DateTime.MinValue;
			object obj = dyRowData[funcMark + "ActionDate"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				dateTime3 = Convert.ToDateTime(obj.ToString()).Date;
			}
			if (dateTime3 == DateTime.MinValue)
			{
				return ResManager.LoadKDString("未录入执行日期！", "004023000019000", 5, new object[0]);
			}
			obj = dyRowData[funcMark + "LastCloseDate"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				dateTime = Convert.ToDateTime(obj.ToString()).Date;
			}
			obj = dyRowData[funcMark + "LastBackDate"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				dateTime2 = Convert.ToDateTime(obj.ToString()).Date;
			}
			if (this._isRestore)
			{
				if (dateTime2 == DateTime.MinValue)
				{
					return ResManager.LoadKDString("未执行过备份操作，不允许执行数据还原。", "004023000019007", 5, new object[0]);
				}
				if (dateTime3 > dateTime2)
				{
					return ResManager.LoadKDString("执行日期不能大于最近备份日期。", "004023000019008", 5, new object[0]);
				}
			}
			string result = "";
			string key;
			switch (key = this._curEntityKey.ToUpper())
			{
			case "FENTITYINVBAL":
				if (!this._isRestore)
				{
					if (dateTime == DateTime.MinValue)
					{
						result = ResManager.LoadKDString("未做过库存关账，不允许执行库存余额备份。", "004023000019009", 5, new object[0]);
					}
					else if (dateTime3.AddMonths(3) > dateTime)
					{
						result = ResManager.LoadKDString("执行日期不能大于关账日期的前三个月。", "004023000019010", 5, new object[0]);
					}
					else if (dateTime2 != DateTime.MinValue && dateTime3 <= dateTime2)
					{
						result = ResManager.LoadKDString("执行日期必须大于最近备份日期。", "004023000019011", 5, new object[0]);
					}
				}
				break;
			case "FENTITYINVLOG":
			case "FENTITYLOCKLOG":
			case "FENTITYLOTTRACE":
			case "FENTITYBALOCCURLOG":
			case "FENTITYBALRESULTLOG":
				if (!this._isRestore)
				{
					if (dateTime2 != DateTime.MinValue && dateTime3 <= dateTime2)
					{
						result = ResManager.LoadKDString("执行日期必须大于最近备份日期。", "004023000019011", 5, new object[0]);
					}
					else if (dateTime3.AddMonths(3) > TimeServiceHelper.GetSystemDateTime(this.View.Context).Date)
					{
						result = ResManager.LoadKDString("执行日期不能大于当前日期的前三个月。", "004023000019012", 5, new object[0]);
					}
				}
				break;
			}
			return result;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x00019CAC File Offset: 0x00017EAC
		private void RefreshDataExInfo(string funcMark, bool refreshAllRows)
		{
			DynamicObjectCollection stkOpTableDataInfos = StockServiceHelper.GetStkOpTableDataInfos(base.Context, this._isRestore, funcMark, (from p in this.orgInfos
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

		// Token: 0x06000209 RID: 521 RVA: 0x00019E3C File Offset: 0x0001803C
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count < 1)
			{
				return string.Format(" {0} = -1 ", key);
			}
			return string.Format(" {0} in ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00019E6C File Offset: 0x0001806C
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
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：组织编码[{0}]正在进行库存数据表的备份或还原，不允许操作！", "004023000019051", 5, new object[0]), orgNum[i]), "", 0);
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

		// Token: 0x040000B2 RID: 178
		private bool _isRestore;

		// Token: 0x040000B3 RID: 179
		private Dictionary<string, bool> _pageFilledInfo = new Dictionary<string, bool>();

		// Token: 0x040000B4 RID: 180
		private string _curEntityKey = "";

		// Token: 0x040000B5 RID: 181
		private string _curEntityPtyName = "";

		// Token: 0x040000B6 RID: 182
		private Dictionary<long, DateTime> lastCloseDates;

		// Token: 0x040000B7 RID: 183
		private List<DynamicObject> orgInfos;

		// Token: 0x040000B8 RID: 184
		private bool isbBusiness;
	}
}

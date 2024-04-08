using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004A RID: 74
	public class ClearZeroInventory : AbstractDynamicFormPlugIn
	{
		// Token: 0x060002FF RID: 767 RVA: 0x00023920 File Offset: 0x00021B20
		public override void CreateNewData(BizDataEventArgs e)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObject dynamicObject = new DynamicObject(this.Model.BillBusinessInfo.GetDynamicObjectType());
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(dynamicObject);
			string text = " FORGID = -1 ";
			List<Organization> userOrg = PermissionServiceHelper.GetUserOrg(this.View.Context);
			List<long> allStockDateLst = StockServiceHelper.GetAllStockDateLst(this.View.Context);
			List<long> list = new List<long>();
			if (userOrg == null || userOrg.Count < 1)
			{
				text = " FORGID = -1 ";
			}
			else
			{
				foreach (Organization organization in userOrg)
				{
					if (allStockDateLst.Contains(organization.Id))
					{
						list.Add(organization.Id);
					}
				}
				if (list != null && list.Count > 0)
				{
					text = string.Format(" FORGID in ({0}) ", string.Join<long>(",", list.ToArray()));
				}
			}
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			list2.Add(new SelectorItemInfo("FName"));
			list2.Add(new SelectorItemInfo("FNumber"));
			list2.Add(new SelectorItemInfo("FDescription"));
			text += " AND t0.FDOCUMENTSTATUS = 'C' AND t0.FFORBIDSTATUS = 'A' ";
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = text,
				RequiresDataPermission = true,
				OrderByClauseWihtKey = "FNUMBER"
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
				dynamicObject2["Check"] = false;
				dynamicObject2["StockOrgNum"] = dynamicObjectCollection[i]["FNumber"].ToString();
				if (dynamicObjectCollection[i]["FName"] != null)
				{
					dynamicObject2["StockOrgName"] = dynamicObjectCollection[i]["FName"].ToString();
				}
				if (dynamicObjectCollection[i]["FDescription"] != null)
				{
					dynamicObject2["StockOrgDesc"] = dynamicObjectCollection[i]["FDescription"].ToString();
				}
				dynamicObject2["StockOrgId"] = dynamicObjectCollection[i]["FORGID"].ToString();
				dynamicObject2["Result"] = "";
				dynamicObject2["Seq"] = i + 1;
				value.Add(dynamicObject2);
			}
			e.BizDataObject = dynamicObject;
		}

		// Token: 0x06000300 RID: 768 RVA: 0x00023C04 File Offset: 0x00021E04
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.Equals("tbClear", StringComparison.OrdinalIgnoreCase))
			{
				string operateName = ResManager.LoadKDString("清除", "004023030009252", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.CheckPermission(e))
				{
					e.Cancel = true;
					this.View.ShowMessage(ResManager.LoadKDString("您没有该操作权限!", "004023000018974", 5, new object[0]), 0);
					return;
				}
				try
				{
					if (!this.isbBusiness)
					{
						List<long> list = new List<long>();
						List<string> list2 = new List<string>();
						List<string> list3 = new List<string>();
						List<long> list4 = new List<long>();
						List<int> list5 = new List<int>();
						List<DynamicObject> list6 = new List<DynamicObject>();
						this.isbBusiness = true;
						this.ret = null;
						DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["StockOrg"] as DynamicObjectCollection;
						for (int i = 0; i < this.Model.GetEntryRowCount("FEntity"); i++)
						{
							if (Convert.ToBoolean(dynamicObjectCollection[i]["Check"]) && !Convert.ToBoolean(dynamicObjectCollection[i]["RetFlag"]))
							{
								list.Add(Convert.ToInt64(dynamicObjectCollection[i]["StockOrgID"]));
								list2.Add(dynamicObjectCollection[i]["StockOrgNum"].ToString());
								list3.Add(Convert.ToString(dynamicObjectCollection[i]["StockOrgName"]));
								this.Model.SetValue("FResult", "", i);
								list6.Add(dynamicObjectCollection[i]);
								list5.Add(i);
							}
						}
						if (list6.Count < 1)
						{
							this.View.ShowMessage(ResManager.LoadKDString("请先选择未成功处理过的库存组织", "004023030000247", 5, new object[0]), 0);
						}
						else
						{
							bool flag = false;
							object value = this.View.Model.GetValue("FKeepDataInBal");
							if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
							{
								flag = Convert.ToBoolean(value);
							}
							list4 = StockServiceHelper.DeleteZeroInventory(base.Context, list6, "0", flag);
							string format = ResManager.LoadKDString("清除完成，共清除{0}条零库存记录。", "004023000013455", 5, new object[0]);
							for (int j = 0; j < list6.Count; j++)
							{
								this.Model.SetValue("FResult", string.Format(format, list4[j]), list5[j]);
							}
						}
					}
				}
				finally
				{
					this.isbBusiness = false;
				}
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x00023EF4 File Offset: 0x000220F4
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
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：库存组织{0}正在校对，不允许操作！", "004023030002155", 5, new object[0]), orgNum[i]), "", 0);
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

		// Token: 0x06000302 RID: 770 RVA: 0x00023FF0 File Offset: 0x000221F0
		private string GetInFilter(string key, List<Organization> valList)
		{
			string text = "";
			if (valList == null || valList.Count < 1)
			{
				return string.Format(" {0} = -1 ", key);
			}
			foreach (Organization organization in valList)
			{
				text = text + organization.Id.ToString() + ",";
			}
			text = text.Substring(0, text.Length - 1);
			return string.Format(" {0} in ({1})", key, text);
		}

		// Token: 0x06000303 RID: 771 RVA: 0x000240AC File Offset: 0x000222AC
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = this.View.LayoutInfo.GetFormAppearance().Menu.BarItems;
			string text = string.Empty;
			string id = "STK_ClearZeroInventory";
			text = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, text);
			return permissionAuthResult.Passed;
		}

		// Token: 0x04000114 RID: 276
		private bool isbBusiness;

		// Token: 0x04000115 RID: 277
		private List<StockOrgOperateResult> ret;
	}
}

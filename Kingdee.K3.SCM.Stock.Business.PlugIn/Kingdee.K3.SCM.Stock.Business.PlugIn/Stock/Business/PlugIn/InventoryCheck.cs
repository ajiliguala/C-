using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200006C RID: 108
	public class InventoryCheck : AbstractDynamicFormPlugIn
	{
		// Token: 0x060004B6 RID: 1206 RVA: 0x00038590 File Offset: 0x00036790
		public override void CreateNewData(BizDataEventArgs e)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObject dynamicObject = new DynamicObject(this.Model.BillBusinessInfo.GetDynamicObjectType());
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(dynamicObject);
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(this.View.Context, new BusinessObject
			{
				Id = this.Model.BillBusinessInfo.GetForm().Id
			}, "e3056f217e8b4500bbf9164e97f3f3d9");
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FName"));
			list.Add(new SelectorItemInfo("FNumber"));
			list.Add(new SelectorItemInfo("FDescription"));
			string text = this.GetInFilter(" FORGID", permissionOrg);
			text += " AND t0.FDOCUMENTSTATUS = 'C' AND t0.FFORBIDSTATUS = 'A' AND t0.FORGFUNCTIONS like '%103%' AND t0.FORGID in (SELECT ts.FORGID FROM T_BAS_SYSTEMPROFILE ts \r\n                                        WHERE ts.FCATEGORY = 'STK' AND ts.FACCOUNTBOOKID = 0 \r\n                                        AND ts.FKEY = 'IsInvEndInitial' AND ts.FVALUE = '1')";
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
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

		// Token: 0x060004B7 RID: 1207 RVA: 0x000387F4 File Offset: 0x000369F4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FMATERIALID"))
				{
					return;
				}
				string selOrgIds = this.GetSelOrgIds();
				if (selOrgIds.Length == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "004024030000910", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
				listShowParameter.MutilListUseOrgId = selOrgIds;
				listShowParameter.UseOrgId = 0L;
				string text;
				if (this.GetFieldFilter(e.FieldKey, e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				e.IsShowUsed = false;
				e.IsShowApproved = true;
			}
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x000388D8 File Offset: 0x00036AD8
		private string GetSelOrgIds()
		{
			List<long> list = new List<long>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["StockOrg"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (Convert.ToBoolean(dynamicObject["Check"]))
				{
					list.Add(Convert.ToInt64(dynamicObject["StockOrgID"]));
				}
			}
			return string.Join<long>(",", list);
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00038980 File Offset: 0x00036B80
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpper()) != null)
			{
				if (!(a == "FMATERIALID"))
				{
					return;
				}
				string selOrgIds = this.GetSelOrgIds();
				if (selOrgIds.Length == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "004024030000910", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
					}
					else
					{
						e.Filter = e.Filter + " AND " + text;
					}
				}
				e.IsShowUsed = false;
				e.IsShowApproved = true;
			}
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x00038A78 File Offset: 0x00036C78
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "RecInvCheckMidData", false);
			bool flag = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag = Convert.ToBoolean(systemProfile);
			}
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "CleanReleaseLink", false);
			bool flag2 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag2 = Convert.ToBoolean(systemProfile);
			}
			if (e.BarItemKey.Equals("tbCheck", StringComparison.OrdinalIgnoreCase))
			{
				string operateName = ResManager.LoadKDString("校对", "004023030009251", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				try
				{
					if (this.isbBusiness)
					{
						return;
					}
					List<long> list = new List<long>();
					List<string> list2 = new List<string>();
					this.isbBusiness = true;
					this.ret = null;
					DynamicObjectCollection entryDataObject = this.View.Model.DataObject["StockOrg"] as DynamicObjectCollection;
					for (int j = 0; j < this.Model.GetEntryRowCount("FEntity"); j++)
					{
						if (Convert.ToBoolean(entryDataObject[j]["Check"]) && !Convert.ToBoolean(entryDataObject[j]["RetFlag"]))
						{
							list.Add(Convert.ToInt64(entryDataObject[j]["StockOrgID"]));
							list2.Add(entryDataObject[j]["StockOrgNum"].ToString());
							this.Model.SetValue("FResult", "", j);
						}
					}
					if (list.Count < 1)
					{
						this.View.ShowMessage(ResManager.LoadKDString("请先选择未成功处理过的库存组织", "004023030000247", 5, new object[0]), 0);
					}
					else
					{
						List<NetworkCtrlResult> list3 = this.BatchStartNetCtl(list2);
						if (list3 != null && list3.Count == list2.Count)
						{
							try
							{
								Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
								BaseDataField baseDataField = this.View.BusinessInfo.GetField("FMaterialId") as BaseDataField;
								DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["MatEntry"] as DynamicObjectCollection;
								foreach (DynamicObject dynamicObject in dynamicObjectCollection)
								{
									DynamicObject value = baseDataField.DynamicProperty.GetValue<DynamicObject>(dynamicObject);
									if (value != null)
									{
										string key = value["Number"].ToString();
										if (!dictionary.ContainsKey(key))
										{
											dictionary[key] = true;
										}
									}
								}
								List<string> list4 = new List<string>();
								if (dictionary.Keys.Count > 0)
								{
									list4 = dictionary.Keys.ToList<string>();
								}
								this.ret = StockServiceHelper.StockCheck(base.Context, list, list4, flag, flag2);
							}
							catch (Exception ex)
							{
								this.View.ShowErrMessage(ex.Message, "", 0);
							}
							finally
							{
								NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list3);
							}
						}
						if (this.ret != null && this.ret.Count > 0)
						{
							int i;
							for (i = 0; i < this.Model.GetEntryRowCount("FEntity"); i++)
							{
								if (!Convert.ToBoolean(entryDataObject[i]["Check"]))
								{
									this.Model.SetValue("FResult", "", i);
									this.Model.SetValue("FRetFlag", false, i);
								}
								else
								{
									StockOrgOperateResult stockOrgOperateResult = (from p in this.ret
									where p.StockOrgID == Convert.ToInt64(entryDataObject[i]["StockOrgID"])
									select p).FirstOrDefault<StockOrgOperateResult>();
									if (stockOrgOperateResult != null)
									{
										this.Model.SetValue("FResult", stockOrgOperateResult.OperateSuccess ? ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]) : stockOrgOperateResult.ErrInfo[0].ErrMsg, i);
										this.Model.SetValue("FRetFlag", stockOrgOperateResult.OperateSuccess, i);
									}
								}
							}
						}
						this.View.UpdateView("FEntity");
					}
					return;
				}
				finally
				{
					this.isbBusiness = false;
				}
			}
			if (!e.BarItemKey.Equals("tbClearOldInfo", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string operateName2 = ResManager.LoadKDString("清除校对标志", "004023030009279", 5, new object[0]);
			string onlyViewMsg2 = Common.GetOnlyViewMsg(base.Context, operateName2);
			if (string.IsNullOrWhiteSpace(onlyViewMsg2))
			{
				this.ClearOldClosingInfo();
				return;
			}
			e.Cancel = true;
			this.View.ShowErrMessage(onlyViewMsg2, "", 0);
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00039018 File Offset: 0x00037218
		private void ClearOldClosingInfo()
		{
			List<long> list = new List<long>();
			List<string> list2 = new List<string>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["StockOrg"] as DynamicObjectCollection;
			for (int i = 0; i < this.Model.GetEntryRowCount("FEntity"); i++)
			{
				if (Convert.ToBoolean(dynamicObjectCollection[i]["Check"]))
				{
					list.Add(Convert.ToInt64(dynamicObjectCollection[i]["StockOrgID"]));
					list2.Add(dynamicObjectCollection[i]["StockOrgNum"].ToString());
				}
			}
			if (list.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先勾选要执行清理的库存组织！", "004023000022494", 5, new object[0]), 0);
				return;
			}
			List<NetworkCtrlResult> list3 = this.BatchStartNetCtl(list2);
			try
			{
				if (list3 != null && list3.Count == list2.Count)
				{
					StockServiceHelper.DeleteOrgCheckingInfo(base.Context, list);
					this.View.ShowMessage(ResManager.LoadKDString("遗留校对中标志信息清理完成！", "004023000032924", 5, new object[0]), 0);
				}
			}
			catch (KDException ex)
			{
				this.View.ShowErrMessage(ex.Message, "", 0);
			}
			finally
			{
				NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list3);
			}
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00039180 File Offset: 0x00037380
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

		// Token: 0x060004BD RID: 1213 RVA: 0x0003927B File Offset: 0x0003747B
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count < 1)
			{
				return string.Format(" {0} = -1 ", key);
			}
			return string.Format(" {0} in ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x000392AC File Offset: 0x000374AC
		private bool GetFieldFilter(string fieldKey, int row, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpper()) != null && a == "FMATERIALID")
			{
				filter = " FIsInventory = '1'";
			}
			return true;
		}

		// Token: 0x040001BF RID: 447
		private bool isbBusiness;

		// Token: 0x040001C0 RID: 448
		private List<StockOrgOperateResult> ret;
	}
}

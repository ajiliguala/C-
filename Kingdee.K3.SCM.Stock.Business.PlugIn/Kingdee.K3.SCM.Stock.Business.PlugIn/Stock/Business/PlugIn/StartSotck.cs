using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200002B RID: 43
	public class StartSotck : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600019F RID: 415 RVA: 0x00013D54 File Offset: 0x00011F54
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FSTOCKSTARTDATE"))
				{
					return;
				}
				if (this._noCheck)
				{
					this._noCheck = false;
					return;
				}
				DateTime d = DateTime.MinValue;
				if (e.Value != null && e.Value.ToString() != null && !string.IsNullOrWhiteSpace(e.Value.ToString()))
				{
					d = Convert.ToDateTime(e.Value);
				}
				if (d == DateTime.MinValue)
				{
					object value = this.Model.GetValue("FRetFlag", e.Row);
					if (value != null && Convert.ToBoolean(value))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("组织已经启用，不允许清空启用日期", "004023000014231", 5, new object[0]), "", 0);
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00013E4C File Offset: 0x0001204C
		public override void CreateNewData(BizDataEventArgs e)
		{
			List<Organization> allowUseOrgList = SystemParameterServiceHelper.GetAllowUseOrgList(base.Context, "STK_StartStock", false, null);
			DynamicObjectType dynamicObjectType = this.Model.BillBusinessInfo.GetDynamicObjectType();
			DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
			DynamicObjectCollection dynamicObjectCollection = dynamicObject["StockOrg"] as DynamicObjectCollection;
			EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity("FEntity");
			int num = 0;
			if (allowUseOrgList.Count < 1)
			{
				return;
			}
			List<long> list = (from p in allowUseOrgList
			where p.OrgFunctions.Contains("103")
			select p.Id).Distinct<long>().ToList<long>();
			if (list == null || list.Count < 1)
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (list.Count > 0)
			{
				dictionary = StockServiceHelper.GetBatchStockDate(this.View.Context, list);
			}
			Dictionary<long, DateTime> dictionary2 = new Dictionary<long, DateTime>();
			if (list.Count > 0)
			{
				dictionary2 = CommonServiceHelper.GetInvEndInitialDate(this.View.Context, list);
			}
			DataTable stockOrgAcctLastCloseDate = CommonServiceHelper.GetStockOrgAcctLastCloseDate(this.View.Context, string.Join<long>(",", list));
			Dictionary<long, bool> dictionary3 = new Dictionary<long, bool>();
			foreach (object obj in stockOrgAcctLastCloseDate.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				dictionary3[Convert.ToInt64(dataRow[0])] = true;
			}
			List<long> list2 = new List<long>();
			foreach (long num2 in list)
			{
				if (!dictionary3.ContainsKey(num2))
				{
					list2.Add(num2);
				}
			}
			List<long> list3 = list2;
			Dictionary<long, bool> dictionary4 = StockServiceHelper.ExistInvRecordOrg(this.View.Context, list3);
			foreach (Organization organization in (from p in allowUseOrgList
			orderby p.Number
			select p).ToList<Organization>())
			{
				if (list.Contains(organization.Id))
				{
					DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
					dynamicObject2["StockOrgNum"] = organization.Number;
					dynamicObject2["StockOrgName"] = organization.Name.ToString();
					dynamicObject2["StockOrgId"] = organization.Id;
					object obj2 = null;
					dictionary.TryGetValue(organization.Id.ToString(), out obj2);
					DateTime dateTime = DateTime.MinValue;
					if (obj2 != null && !string.IsNullOrWhiteSpace(obj2.ToString()))
					{
						dateTime = Convert.ToDateTime(obj2);
						dynamicObject2["StockStartDate"] = dateTime;
						dynamicObject2["RetFlag"] = true;
					}
					dynamicObject2["Seq"] = num + 1;
					bool flag = false;
					DateTime minValue = DateTime.MinValue;
					dictionary2.TryGetValue(organization.Id, out minValue);
					if (minValue != DateTime.MinValue)
					{
						flag = true;
					}
					bool flag2 = false;
					dictionary4.TryGetValue(organization.Id, out flag2);
					if (flag)
					{
						if (flag2)
						{
							dynamicObject2["Status"] = ResManager.LoadKDString("已启用，存在即时库存", "004023000014232", 5, new object[0]);
							dynamicObject2["CanEdit"] = false;
						}
						else if (dictionary3.ContainsKey(organization.Id))
						{
							dynamicObject2["Status"] = ResManager.LoadKDString("已启用，已关账", "004023000014233", 5, new object[0]);
							dynamicObject2["CanEdit"] = false;
						}
						else
						{
							dynamicObject2["Status"] = ResManager.LoadKDString("已启用，已结束初始化", "004023000014234", 5, new object[0]);
							dynamicObject2["CanEdit"] = false;
						}
					}
					else if (flag2)
					{
						dynamicObject2["Status"] = ResManager.LoadKDString("已启用，存在即时库存", "004023000014232", 5, new object[0]);
						dynamicObject2["CanEdit"] = false;
					}
					else if (dateTime != DateTime.MinValue)
					{
						dynamicObject2["Status"] = ResManager.LoadKDString("已启用", "004023000014235", 5, new object[0]);
						dynamicObject2["CanEdit"] = true;
					}
					else
					{
						dynamicObject2["Status"] = ResManager.LoadKDString("未启用", "004023000014236", 5, new object[0]);
						dynamicObject2["CanEdit"] = true;
					}
					dynamicObjectCollection.Add(dynamicObject2);
					num++;
				}
			}
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				DBServiceHelper.LoadReferenceObject(base.Context, dynamicObjectCollection.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
			e.BizDataObject = dynamicObject;
			base.CreateNewData(e);
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x000143CC File Offset: 0x000125CC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBSAVE"))
				{
					if (!(a == "TBCLOSE"))
					{
						return;
					}
					if (this.View.GetMainBarItem("tbSave").Enabled)
					{
						this.View.ShowMessage(ResManager.LoadKDString("参数设置未保存，是否继续？", "004023030002260", 5, new object[0]), 4, delegate(MessageBoxResult result)
						{
							if (result == 6)
							{
								this.View.Close();
							}
						}, "", 0);
					}
				}
				else
				{
					string operateName = ResManager.LoadKDString("启用库存管理", "004023000020447", 5, new object[0]);
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
					this.DoSaveData();
					return;
				}
			}
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x000144DC File Offset: 0x000126DC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpper()) != null)
			{
				if (!(a == "FSELECT"))
				{
					return;
				}
				if (!Convert.ToBoolean(e.NewValue))
				{
					object value = this.Model.GetValue("FRetFlag", e.Row);
					if (value != null && !Convert.ToBoolean(value))
					{
						this._noCheck = true;
						this.Model.SetValue("FStockStartDate", null, e.Row);
					}
				}
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00014578 File Offset: 0x00012778
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = this.View.LayoutInfo.GetFormAppearance().Menu.BarItems;
			string text = string.Empty;
			string id = "STK_StartStock";
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

		// Token: 0x060001A4 RID: 420 RVA: 0x00014614 File Offset: 0x00012814
		private void DoSaveData()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			List<long> list = new List<long>();
			int num = 0;
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				if (!Convert.ToBoolean(dynamicObject["Select"]))
				{
					this.Model.SetValue("FResult", "", num);
				}
				else
				{
					list.Add(Convert.ToInt64(dynamicObject["StockOrgId"]));
				}
			}
			if (list.Count < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选择需要启用的库存组织！", "004023030002251", 5, new object[0]), "", 4);
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (list.Count > 0)
			{
				dictionary = StockServiceHelper.GetBatchStockDate(this.View.Context, list);
			}
			Dictionary<long, DateTime> dictionary2 = new Dictionary<long, DateTime>();
			if (list.Count > 0)
			{
				dictionary2 = CommonServiceHelper.GetInvEndInitialDate(this.View.Context, list);
			}
			DataTable stockOrgAcctLastCloseDate = CommonServiceHelper.GetStockOrgAcctLastCloseDate(this.View.Context, string.Join<long>(",", list));
			Dictionary<long, object> dictionary3 = new Dictionary<long, object>();
			foreach (object obj in stockOrgAcctLastCloseDate.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				dictionary3[Convert.ToInt64(dataRow[0])] = dataRow[1];
			}
			List<long> list2 = new List<long>();
			foreach (long num2 in list)
			{
				if (!dictionary3.ContainsKey(num2))
				{
					list2.Add(num2);
				}
			}
			list = list2;
			Dictionary<long, bool> dictionary4 = StockServiceHelper.ExistInvRecordOrg(this.View.Context, list);
			list2 = new List<long>();
			foreach (long num3 in list)
			{
				bool flag = true;
				dictionary4.TryGetValue(num3, out flag);
				if (!flag)
				{
					list2.Add(num3);
				}
			}
			list = list2;
			Dictionary<long, bool> dictionary5 = new Dictionary<long, bool>();
			if (list != null && list.Count > 0)
			{
				dictionary5 = StockServiceHelper.ExistStockBillOrg(this.View.Context, list);
			}
			num = 0;
			Dictionary<int, KeyValuePair<long, DateTime>> dictionary6 = new Dictionary<int, KeyValuePair<long, DateTime>>();
			foreach (DynamicObject dynamicObject2 in entityDataObject)
			{
				if (!Convert.ToBoolean(dynamicObject2["Select"]))
				{
					num++;
				}
				else
				{
					DateTime dateTime = (dynamicObject2["StockStartDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(dynamicObject2["StockStartDate"]);
					if (dateTime != DateTime.MinValue)
					{
						long key = Convert.ToInt64(dynamicObject2["StockOrgId"]);
						object obj2 = null;
						dictionary.TryGetValue(key.ToString(), out obj2);
						bool flag2 = false;
						DateTime minValue = DateTime.MinValue;
						dictionary2.TryGetValue(key, out minValue);
						flag2 = (minValue != DateTime.MinValue);
						if (!flag2)
						{
							dictionary4.TryGetValue(key, out flag2);
							if (!flag2)
							{
								dictionary5.TryGetValue(key, out flag2);
								if (!flag2)
								{
									DateTime d = DateTime.MaxValue;
									object obj3 = null;
									dictionary3.TryGetValue(key, out obj3);
									if (obj3 != null && !string.IsNullOrWhiteSpace(obj3.ToString()))
									{
										d = Convert.ToDateTime(obj3);
									}
									if (d == DateTime.MaxValue)
									{
										dictionary6[num] = new KeyValuePair<long, DateTime>(key, dateTime);
									}
									else
									{
										this.Model.SetValue("FCanEdit", false, num);
										this.Model.SetValue("FSelect", false, num);
										this.Model.SetValue("FStockStartDate", obj2, num);
										this.Model.SetValue("FStatus", ResManager.LoadKDString("已启用，已关账", "004023000014233", 5, new object[0]), num);
										this.Model.SetValue("FResult", ResManager.LoadKDString("当前库存组织已关账，不能修改库存启用日期", "004023000014237", 5, new object[0]), num);
									}
								}
								else
								{
									this.Model.SetValue("FStockStartDate", obj2, num);
									this.Model.SetValue("FRetFlag", true, num);
									this.Model.SetValue("FCanEdit", false, num);
									this.Model.SetValue("FSelect", false, num);
									this.Model.SetValue("FResult", ResManager.LoadKDString("当前库存组织存在库存单据，不能修改库存启用日期", "004023000014238", 5, new object[0]), num);
									this.Model.SetValue("FStatus", ResManager.LoadKDString("已启用，存在未更新库存的库存单据", "004023000014239", 5, new object[0]), num);
								}
							}
							else
							{
								this.Model.SetValue("FStockStartDate", obj2, num);
								this.Model.SetValue("FCanEdit", false, num);
								this.Model.SetValue("FSelect", false, num);
								this.Model.SetValue("FStatus", ResManager.LoadKDString("已启用，存在即时库存", "004023000014232", 5, new object[0]), num);
								this.Model.SetValue("FResult", ResManager.LoadKDString("当前库存组织下存在即时库存数据，不能修改库存启用日期", "004023000014240", 5, new object[0]), num);
							}
						}
						else
						{
							this.Model.SetValue("FStockStartDate", obj2, num);
							this.Model.SetValue("FCanEdit", false, num);
							this.Model.SetValue("FSelect", false, num);
							this.Model.SetValue("FStatus", ResManager.LoadKDString("已启用，已结束初始化", "004023000014234", 5, new object[0]), num);
							this.Model.SetValue("FResult", ResManager.LoadKDString("当前库存组织已经库存结束初始化，不能修改库存启用日期", "004023000014241", 5, new object[0]), num);
						}
					}
					else
					{
						this.Model.SetValue("FResult", ResManager.LoadKDString("请录入有效的启用日期！", "004023000014242", 5, new object[0]), num);
					}
					num++;
				}
			}
			if (dictionary6.Count > 0)
			{
				Dictionary<long, DateTime> dictionary7 = new Dictionary<long, DateTime>();
				foreach (KeyValuePair<long, DateTime> keyValuePair in dictionary6.Values)
				{
					dictionary7[keyValuePair.Key] = keyValuePair.Value;
				}
				if (StockServiceHelper.UpdateOrgStartDate(this.View.Context, dictionary7))
				{
					foreach (int num4 in dictionary6.Keys)
					{
						this.Model.SetValue("FResult", ResManager.LoadKDString("保存成功！", "004023030002266", 5, new object[0]), num4);
						this.Model.SetValue("FRetFlag", true, num4);
						this.Model.SetValue("FStatus", ResManager.LoadKDString("已启用", "004023000014235", 5, new object[0]), num4);
						string arg = "";
						string arg2 = "";
						object value = this.Model.GetValue("FStockOrgNum", num4);
						if (value != null)
						{
							arg = value.ToString();
						}
						value = this.Model.GetValue("FStockOrgName", num4);
						if (value != null)
						{
							arg2 = value.ToString();
						}
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = string.Format(ResManager.LoadKDString("库存组织{0}{1}启用库存管理成功", "004023000020446", 5, new object[0]), arg, arg2),
							Environment = 3,
							OperateName = ResManager.LoadKDString("启用库存管理", "004023000020447", 5, new object[0]),
							SubSystemId = "21"
						});
					}
					this.View.ShowMessage(ResManager.LoadKDString("保存完成！", "004023000014243", 5, new object[0]), 0);
				}
			}
		}

		// Token: 0x0400009D RID: 157
		private const string STOCKFUNCTIONID = "103";

		// Token: 0x0400009E RID: 158
		private bool _noCheck;
	}
}

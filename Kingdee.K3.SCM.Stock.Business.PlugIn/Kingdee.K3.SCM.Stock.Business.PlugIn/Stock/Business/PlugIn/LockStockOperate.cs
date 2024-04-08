using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200009E RID: 158
	public class LockStockOperate : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000948 RID: 2376 RVA: 0x0007B79C File Offset: 0x0007999C
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.SplitHideCtrl(false);
			BOSRule item = this.View.RuleContainer.AddPluginRule("FSubEntiry", 41, new Action<DynamicObject, object>(this.ReviseValue), new string[]
			{
				"FLockQty",
				"FLockBaseQty",
				"FLockSecQty"
			});
			this.View.RuleContainer.Rules.Remove(item);
			this.View.RuleContainer.Rules.Insert(0, item);
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0007B824 File Offset: 0x00079A24
		private void ReviseValue(DynamicObject row, dynamic dynamicRow)
		{
			if ((decimal)row["LockQty"] > (decimal)row["CanLockQty"])
			{
				row["LockQty"] = row["CanLockQty"];
			}
			if ((decimal)row["LockBaseQty"] > (decimal)row["CanLockBaseQty"])
			{
				row["LockBaseQty"] = row["CanLockBaseQty"];
			}
			if ((decimal)row["LockSecQty"] > (decimal)row["CanLockSecQty"])
			{
				if ((decimal)row["CanLockSecQty"] >= 0m)
				{
					row["LockSecQty"] = row["CanLockSecQty"];
					return;
				}
				row["LockSecQty"] = 0;
			}
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0007B917 File Offset: 0x00079B17
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "BTN_SAVE"))
			{
				e.Cancel = this.SaveStockLock();
			}
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0007B938 File Offset: 0x00079B38
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "BTN_MATCH"))
				{
					return;
				}
				this.MatchMaterial();
			}
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0007B968 File Offset: 0x00079B68
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			if (e.Key.IndexOf("__") > -1)
			{
				this._isFlex = true;
			}
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0007B9C8 File Offset: 0x00079BC8
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			if (this._isFlex)
			{
				this._isFlex = false;
				return;
			}
			if (!this.IsClose)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("是否保存锁库信息?", "004023030000310", 5, new object[0]), 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						e.Cancel = this.SaveStockLock();
						return;
					}
					this.IsClose = true;
					this.View.Close();
				}, "", 0);
			}
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x0007BA48 File Offset: 0x00079C48
		public override void CreateNewData(BizDataEventArgs e)
		{
			base.CreateNewData(e);
			if (this.orgLockStockList == null)
			{
				BusinessObject businessObject = new BusinessObject
				{
					Id = "STK_LockStock",
					PermissionControl = 1,
					SubSystemId = "STK"
				};
				this.orgLockStockList = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6ff9853429aa4384892b4f5d0f86dd1b");
			}
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0007BAA0 File Offset: 0x00079CA0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string text = "";
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FLOT"))
					{
						return;
					}
					text = this.GetLotCommonFilter(e.BaseDataField as LotField, false, e.Row);
					if (string.IsNullOrWhiteSpace(text))
					{
						this.View.ShowMessage(ResManager.LoadKDString("请先录入批号对应的库存组织和物料！", "002012030003658", 2, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
				}
				else if (this.GetOrgFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0007BBBC File Offset: 0x00079DBC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				string lotCommonFilter;
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FLOT"))
					{
						return;
					}
					lotCommonFilter = this.GetLotCommonFilter(e.BaseDataField as LotField, false, e.Row);
					if (string.IsNullOrWhiteSpace(lotCommonFilter))
					{
						this.View.ShowMessage(ResManager.LoadKDString("请先录入批号对应的库存组织和物料！", "002012030003658", 2, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = lotCommonFilter;
						return;
					}
					e.Filter = e.Filter + " AND " + lotCommonFilter;
				}
				else if (this.GetOrgFieldFilter(e.BaseDataFieldKey, out lotCommonFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = lotCommonFilter;
						return;
					}
					e.Filter = e.Filter + " AND " + lotCommonFilter;
					return;
				}
			}
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0007BCB4 File Offset: 0x00079EB4
		private bool GetOrgFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FSTOCKORGID")
			{
				if (this.orgLockStockList != null && this.orgLockStockList.Count > 0)
				{
					filter = string.Format(" FOrgId IN ({0}) ", string.Join<long>(",", this.orgLockStockList));
				}
				else
				{
					filter = " 1<>1 ";
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x0007BD30 File Offset: 0x00079F30
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FReleaseDate"))
				{
					if (!(key == "FLockSecQty"))
					{
						return;
					}
					decimal d = Convert.ToDecimal(this.Model.GetValue("FCanLockSecQty", e.Row));
					if (d <= 0m && Convert.ToDecimal(e.Value) != 0m)
					{
						e.Cancel = true;
					}
				}
				else
				{
					if (e.Value == null || string.IsNullOrWhiteSpace(e.Value.ToString()))
					{
						return;
					}
					DateTime t = DateTime.Parse(e.Value.ToString());
					if (t < DateTime.Today)
					{
						this.View.ShowMessage(ResManager.LoadKDString("预计解锁日期不能小于当前日期！", "004023000017455", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
				}
			}
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0007BE14 File Offset: 0x0007A014
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FStockOrgId")
				{
					EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntityLock");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					if (entityDataObject != null && entityDataObject.Count > 0)
					{
						entityDataObject.Clear();
					}
					this.View.UpdateView("FEntityLock");
					return;
				}
				if (!(key == "FReleaseDate"))
				{
					return;
				}
				if (e.NewValue == null || string.IsNullOrWhiteSpace(e.NewValue.ToString()))
				{
					this.Model.SetValue("FReserveDays", 0, e.Row);
					return;
				}
				object value = this.Model.GetValue("FReserveDate", e.Row);
				if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				{
					return;
				}
				DateTime value2 = DateTime.Parse(value.ToString());
				TimeSpan timeSpan = DateTime.Parse(e.NewValue.ToString()).Subtract(value2);
				this.Model.SetValue("FReserveDays", timeSpan.Days, e.Row);
			}
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0007BF40 File Offset: 0x0007A140
		private void MatchMaterial()
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(this.LockDemandEntity);
			Entity entity = this.View.BusinessInfo.GetEntity(this.LockDemandEntity);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
			if (this.CheckMatchData(entityDataObject))
			{
				this.FillMatchInfoEntiy(entityDataObject);
			}
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0007BF94 File Offset: 0x0007A194
		private bool CheckMatchData(DynamicObject selectedObject)
		{
			if (selectedObject == null || selectedObject["MaterialId"] == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("选中行无数据", "004023030000313", 5, new object[0]), 0);
				return false;
			}
			if (Convert.ToInt64(selectedObject["MaterialId_Id"]) <= 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("物料必须录入", "004023030000316", 5, new object[0]), 0);
				return false;
			}
			if (Convert.ToDecimal(selectedObject["LeftQty"].ToString()) == 0m)
			{
				this.View.ShowMessage(ResManager.LoadKDString("待锁数量必须录入", "004023030001015", 5, new object[0]), 0);
				return false;
			}
			if (Convert.ToDecimal(selectedObject["LeftQty"].ToString()) < 0m)
			{
				this.View.ShowMessage(ResManager.LoadKDString("待锁数量不能录入负数", "004023030001018", 5, new object[0]), 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x0007C1F0 File Offset: 0x0007A3F0
		private bool SaveStockLock()
		{
			bool flag = false;
			Entity entity = this.View.BillBusinessInfo.GetEntity(this.LockDemandEntity);
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			List<LockStockArgs> list = new List<LockStockArgs>();
			Dictionary<string, decimal> accLeftQty = new Dictionary<string, decimal>();
			Dictionary<string, decimal> accLeftSecQty = new Dictionary<string, decimal>();
			Dictionary<string, List<KeyValuePair<int, string>>> dictionary = new Dictionary<string, List<KeyValuePair<int, string>>>();
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["SubEntiry"];
				decimal num = Convert.ToDecimal(dynamicObject["LeftQty"]);
				decimal d = 0m;
				if (num <= 0m)
				{
					this.View.ShowMessage(ResManager.LoadKDString("待锁数量不能录入负数或为0", "004023030001021", 5, new object[0]), 0);
					break;
				}
				DateTime? reserveDate = null;
				if (dynamicObject["ReserveDate"] != null && !string.IsNullOrWhiteSpace(dynamicObject["ReserveDate"].ToString()))
				{
					reserveDate = new DateTime?(DateTime.Parse(dynamicObject["ReserveDate"].ToString()));
				}
				int reserveDays = Convert.ToInt32(dynamicObject["ReserveDays"]);
				DateTime? reLeaseDate = null;
				if (dynamicObject["ReleaseDate"] != null && !string.IsNullOrWhiteSpace(dynamicObject["ReleaseDate"].ToString()))
				{
					reLeaseDate = new DateTime?(DateTime.Parse(dynamicObject["ReleaseDate"].ToString()));
				}
				string requestNote = Convert.ToString(dynamicObject["RequestNote"]);
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					if (Convert.ToDecimal(dynamicObject2["LockQty"]) > 0m)
					{
						if (Convert.ToDecimal(dynamicObject2["LockQty"]) > Convert.ToDecimal(dynamicObject2["CanLockQty"]))
						{
							this.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料[{1}]的第{2}行锁库信息，当前锁库量不能超过当前可锁量！", "004023030000322", 5, new object[0]), dynamicObject["Seq"], ((DynamicObject)dynamicObject["MaterialID"])["Name"].ToString(), dynamicObject2["Seq"]), 0);
							flag = true;
							break;
						}
						d += Convert.ToDecimal(dynamicObject2["LockQty"]);
						if (d > num)
						{
							this.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料[{1}]的当前锁库量总和，应小于等于待锁库数量", "004023030000325", 5, new object[0]), dynamicObject["Seq"], ((DynamicObject)dynamicObject["MaterialID"])["Name"].ToString()), 0);
							flag = true;
							break;
						}
						if (!accLeftQty.Keys.Contains(Convert.ToString(dynamicObject2["InvDetailID"])))
						{
							accLeftQty.Add(Convert.ToString(dynamicObject2["InvDetailID"]), Convert.ToDecimal(dynamicObject2["CanLockQty"]) - Convert.ToDecimal(dynamicObject2["LockQty"]));
							dictionary.Add(Convert.ToString(dynamicObject2["InvDetailID"]), new List<KeyValuePair<int, string>>
							{
								new KeyValuePair<int, string>(Convert.ToInt32(dynamicObject["Seq"]), ((DynamicObject)dynamicObject["MaterialID"])["Name"].ToString())
							});
						}
						else
						{
							accLeftQty[Convert.ToString(dynamicObject2["InvDetailID"])] = accLeftQty[Convert.ToString(dynamicObject2["InvDetailID"])] - Convert.ToDecimal(dynamicObject2["LockQty"]);
							dictionary[Convert.ToString(dynamicObject2["InvDetailID"])].Add(new KeyValuePair<int, string>(Convert.ToInt32(dynamicObject["Seq"]), ((DynamicObject)dynamicObject["MaterialID"])["Name"].ToString()));
						}
						if (dynamicObject2["SecUnitID_Info"] != null)
						{
							if (Convert.ToDecimal(dynamicObject2["LockSecQty"]) > 0m && Convert.ToDecimal(dynamicObject2["LockSecQty"]) > Convert.ToDecimal(dynamicObject2["CanLockSecQty"]))
							{
								this.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料[{1}]的第{2}行锁库信息，当前锁库量(辅助)不能超过当前可锁量(辅助)！", "004023030000331", 5, new object[0]), dynamicObject["Seq"], ((DynamicObject)dynamicObject["MaterialID"])["Name"].ToString(), dynamicObject2["Seq"]), 0);
								flag = true;
								break;
							}
							if (Convert.ToDecimal(dynamicObject2["LockSecQty"]) > 0m)
							{
								if (!accLeftSecQty.Keys.Contains(Convert.ToString(dynamicObject2["InvDetailID"])))
								{
									accLeftSecQty.Add(Convert.ToString(dynamicObject2["InvDetailID"]), Convert.ToDecimal(dynamicObject2["CanLockSecQty"]) - Convert.ToDecimal(dynamicObject2["LockSecQty"]));
								}
								else
								{
									accLeftSecQty[Convert.ToString(dynamicObject2["InvDetailID"])] = accLeftSecQty[Convert.ToString(dynamicObject2["InvDetailID"])] - Convert.ToDecimal(dynamicObject2["LockSecQty"]);
								}
							}
						}
						LockStockArgs stockArg = new LockStockArgs();
						stockArg.ObjectId = "STK_Inventory";
						stockArg.BillId = Convert.ToString(dynamicObject2["InvDetailID"]);
						stockArg.BillDetailID = "";
						stockArg.FInvDetailID = Convert.ToString(dynamicObject2["InvDetailID"]);
						stockArg.StockOrgID = this.GetDynamicValue(this.View.Model.GetValue("FStockOrgId") as DynamicObject);
						stockArg.DemandOrgId = stockArg.StockOrgID;
						stockArg.MaterialID = this.GetDynamicValue(dynamicObject2["MaterialID_Info"] as DynamicObject);
						stockArg.DemandMaterialId = stockArg.MaterialID;
						stockArg.DemandDateTime = null;
						stockArg.DemandPriority = "";
						object obj = ((DynamicObjectCollection)(dynamicObject2["MaterialID_Info"] as DynamicObject)["MaterialPlan"])[0]["PlanMode"];
						if (obj != null && obj.ToString() == "1")
						{
							stockArg.IsMto = "1";
						}
						stockArg.MtoNo = Convert.ToString(dynamicObject2["MtoNo_Info"]);
						stockArg.ProjectNo = Convert.ToString(dynamicObject2["ProjectNo_Info"]);
						stockArg.BOMID = this.GetDynamicValue(dynamicObject2["BOMID_Info"] as DynamicObject);
						stockArg.AuxPropId = this.GetDynamicValue(dynamicObject2["AuxPropertyId_Info"] as DynamicObject);
						DynamicObject dynamicObject3 = dynamicObject2["Lot_Info"] as DynamicObject;
						if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0L)
						{
							stockArg.Lot = this.GetDynamicValue(dynamicObject3);
							stockArg.LotNo = dynamicObject3["Number"].ToString();
						}
						if (dynamicObject2["ProduceDate_Info"] != null)
						{
							stockArg.ProduceDate = new DateTime?(DateTime.Parse(dynamicObject2["ProduceDate_Info"].ToString()));
						}
						if (dynamicObject2["EXPIRYDATE_Info"] != null)
						{
							stockArg.ExpiryDate = new DateTime?(DateTime.Parse(dynamicObject2["EXPIRYDATE_Info"].ToString()));
						}
						stockArg.STOCKID = this.GetDynamicValue(dynamicObject2["STOCKID_Info"] as DynamicObject);
						stockArg.StockLocID = this.GetDynamicValue(dynamicObject2["StockPlaceID_Info"] as DynamicObject);
						stockArg.StockStatusID = this.GetDynamicValue(dynamicObject2["StockStatusID_Info"] as DynamicObject);
						stockArg.OwnerTypeID = Convert.ToString(dynamicObject2["OwnerTypeID_Info"]);
						stockArg.OwnerID = this.GetDynamicValue(dynamicObject2["OwnerID_Info"] as DynamicObject);
						stockArg.KeeperTypeID = Convert.ToString(dynamicObject2["KeeperTypeID_Info"]);
						stockArg.KeeperID = this.GetDynamicValue(dynamicObject2["KeeperID_Info"] as DynamicObject);
						stockArg.UnitID = this.GetDynamicValue(dynamicObject2["UnitID_Info"] as DynamicObject);
						stockArg.BaseUnitID = this.GetDynamicValue(dynamicObject2["BaseUnitID_Info"] as DynamicObject);
						stockArg.SecUnitID = this.GetDynamicValue(dynamicObject2["SecUnitID_Info"] as DynamicObject);
						stockArg.LockQty = decimal.Parse(dynamicObject2["LockQty"].ToString());
						stockArg.LockBaseQty = decimal.Parse(dynamicObject2["LockBaseQty"].ToString());
						stockArg.LockSecQty = decimal.Parse(dynamicObject2["LockSecQty"].ToString());
						stockArg.ReserveDate = reserveDate;
						stockArg.ReserveDays = reserveDays;
						stockArg.ReLeaseDate = reLeaseDate;
						stockArg.RequestNote = requestNote;
						stockArg.SupplyNote = Convert.ToString(dynamicObject2["SupplyNote"]);
						LockStockArgs lockStockArgs = (from p in list
						where p.FInvDetailID == stockArg.FInvDetailID && p.ReserveDate == stockArg.ReserveDate && p.ReserveDays == stockArg.ReserveDays && p.ReLeaseDate == stockArg.ReLeaseDate
						select p).FirstOrDefault<LockStockArgs>();
						if (lockStockArgs == null)
						{
							list.Add(stockArg);
						}
						else
						{
							lockStockArgs.LockQty += stockArg.LockQty;
							lockStockArgs.LockBaseQty += stockArg.LockBaseQty;
							lockStockArgs.LockSecQty += stockArg.LockSecQty;
							lockStockArgs.RequestNote = stockArg.RequestNote;
							lockStockArgs.SupplyNote = stockArg.SupplyNote;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (!flag && accLeftQty != null && accLeftQty.Count<KeyValuePair<string, decimal>>() > 0)
			{
				string text = (from p in accLeftQty.Keys
				where accLeftQty[p] < 0m
				select p).FirstOrDefault<string>();
				if (!string.IsNullOrEmpty(text))
				{
					string.Join<int>(",", from p in dictionary[text]
					select p.Key);
					(from p in dictionary[text]
					select p.Value).FirstOrDefault<string>();
					this.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料[{1}]，当前累计锁库量不能超过当前可锁量！", "004023000033722", 5, new object[0]), string.Join<int>(",", from p in dictionary[text]
					select p.Key), (from p in dictionary[text]
					select p.Value).FirstOrDefault<string>()), 0);
					flag = true;
				}
			}
			if (!flag && accLeftSecQty != null && accLeftSecQty.Count<KeyValuePair<string, decimal>>() > 0)
			{
				string text2 = (from p in accLeftSecQty.Keys
				where accLeftSecQty[p] < 0m
				select p).FirstOrDefault<string>();
				if (!string.IsNullOrEmpty(text2))
				{
					string.Join<int>(",", from p in dictionary[text2]
					select p.Key);
					(from p in dictionary[text2]
					select p.Value).FirstOrDefault<string>();
					this.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料[{1}]，当前累计锁库量(辅助)不能超过当前可锁量(辅助)！", "004023000033723", 5, new object[0]), string.Join<int>(",", from p in dictionary[text2]
					select p.Key), (from p in dictionary[text2]
					select p.Value).FirstOrDefault<string>()), 0);
					flag = true;
				}
			}
			if (list.Count > 0 && !flag)
			{
				StockServiceHelper.SaveLockInfo(base.Context, list, "Inv", false);
				this.View.ShowNotificationMessage(ResManager.LoadKDString("锁库操作成功!", "004023030000334", 5, new object[0]), "", 0);
				this.View.ReturnToParentWindow(true);
				this.IsClose = true;
				this.View.Close();
			}
			else if (!flag)
			{
				this.View.ShowNotificationMessage(ResManager.LoadKDString("锁库量录入负数或为0!无须保存!", "004023030001024", 5, new object[0]), "", 0);
			}
			return flag;
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x0007D108 File Offset: 0x0007B308
		private void FillMatchInfoEntiy(DynamicObject obj)
		{
			this.Model.DeleteEntryData(this.LockDetailEntity);
			LockStockArgs lockStockArgs = this.CreateLockNeedInfo();
			List<LockStockArgs> materialList = StockServiceHelper.GetMaterialList(base.Context, lockStockArgs, false);
			if (materialList.Count > 0)
			{
				Entity entity = this.View.BusinessInfo.GetEntity(this.LockDetailEntity);
				DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				entityDataObject.Clear();
				int i = 0;
				decimal num = Convert.ToDecimal(obj["BaseLeftQty"]);
				decimal num2 = Convert.ToDecimal(obj["SecLeftQty"]);
				foreach (LockStockArgs material in materialList)
				{
					i = this.AddNewRow(dynamicObjectType, entityDataObject, i, material, ref num, ref num2);
				}
				DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), dynamicObjectType, false);
				this.View.UpdateView(this.LockDetailEntity);
				this.View.ShowNotificationMessage(ResManager.LoadKDString("匹配成功!请查看锁库信息!", "004023030000340", 5, new object[0]), "", 0);
				this.SplitHideCtrl(true);
				return;
			}
			this.View.ShowNotificationMessage(ResManager.LoadKDString("即时库存无数据匹配!", "004023030000343", 5, new object[0]), "", 0);
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x0007D26C File Offset: 0x0007B46C
		private LockStockArgs CreateLockNeedInfo()
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(this.LockDemandEntity);
			Entity entity = this.View.BusinessInfo.GetEntity(this.LockDemandEntity);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
			return new LockStockArgs
			{
				MaterialID = this.GetDynamicValue(entityDataObject["MaterialId"] as DynamicObject),
				StockOrgID = this.GetDynamicValue(this.View.Model.GetValue("FStockOrgId") as DynamicObject),
				BOMID = this.GetDynamicValue(entityDataObject["BomId"] as DynamicObject),
				AuxProp = (entityDataObject["AuxPropId"] as DynamicObject),
				AuxPropId = this.GetDynamicValue(entityDataObject["AuxPropId"] as DynamicObject),
				Lot = this.GetDynamicValue(entityDataObject["Lot"] as DynamicObject),
				ProduceDate = new DateTime?((entityDataObject["ProduceDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(entityDataObject["ProduceDate"])),
				ExpiryDate = new DateTime?((entityDataObject["ExpiryDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(entityDataObject["ExpiryDate"])),
				STOCKID = this.GetDynamicValue(entityDataObject["StockId"] as DynamicObject),
				StockLocID = this.GetDynamicValue(entityDataObject["StockLocId"] as DynamicObject),
				StockStatusID = this.GetDynamicValue(entityDataObject["StockStatusId"] as DynamicObject),
				OwnerTypeID = ((entityDataObject["OwnerTypeId"] == null) ? null : entityDataObject["OwnerTypeId"].ToString()),
				OwnerID = this.GetDynamicValue(entityDataObject["OwnerId"] as DynamicObject),
				KeeperTypeID = ((entityDataObject["KeeperTypeId"] == null) ? null : entityDataObject["KeeperTypeId"].ToString()),
				KeeperID = this.GetDynamicValue(entityDataObject["KeeperId"] as DynamicObject),
				MtoNo = ((entityDataObject["MtoNo"] == null) ? null : entityDataObject["MtoNo"].ToString()),
				ProjectNo = ((entityDataObject["ProjectNo"] == null) ? null : entityDataObject["ProjectNo"].ToString())
			};
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x0007D4FC File Offset: 0x0007B6FC
		private int AddNewRow(DynamicObjectType objType, DynamicObjectCollection infoEntrys, int i, LockStockArgs material, ref decimal leftBaseQty, ref decimal leftSecQty)
		{
			DynamicObject dynamicObject = new DynamicObject(objType);
			dynamicObject["MaterialID_Info_Id"] = material.MaterialID;
			dynamicObject["BaseUnitID_Info_Id"] = material.BaseUnitID;
			dynamicObject["SecUnitID_Info_Id"] = material.SecUnitID;
			dynamicObject["UnitID_Info_Id"] = material.UnitID;
			dynamicObject["CurrentQty"] = material.Qty;
			dynamicObject["CurrentBaseQty"] = material.BaseQty;
			dynamicObject["CurrentSecQty"] = material.SecQty;
			dynamicObject["LockedQty"] = material.LockQty;
			dynamicObject["LockedBaseQty"] = material.LockBaseQty;
			dynamicObject["LockedSecQty"] = material.LockSecQty;
			dynamicObject["CanLockBaseQty"] = material.BaseQty - material.LockBaseQty;
			dynamicObject["CanLockSecQty"] = material.SecQty - material.LockSecQty;
			dynamicObject["CanLockQty"] = material.Qty - material.LockQty;
			if (leftBaseQty > 0m)
			{
				if (material.BaseQty - material.LockBaseQty < leftBaseQty)
				{
					dynamicObject["LockBaseQty"] = material.BaseQty - material.LockBaseQty;
					dynamicObject["LockQty"] = material.Qty - material.LockQty;
					leftBaseQty -= material.BaseQty - material.LockBaseQty;
				}
				else
				{
					dynamicObject["LockBaseQty"] = leftBaseQty;
					dynamicObject["LockQty"] = this.GetConvertRate(material.MaterialID, material.BaseUnitID, material.UnitID, leftBaseQty);
					leftBaseQty = 0m;
				}
			}
			if (leftSecQty > 0m)
			{
				if (material.SecQty - material.LockSecQty < leftSecQty)
				{
					if (material.SecQty - material.LockSecQty < 0m)
					{
						dynamicObject["LockSecQty"] = 0;
					}
					else
					{
						dynamicObject["LockSecQty"] = material.SecQty - material.LockSecQty;
						leftSecQty -= material.SecQty - material.LockSecQty;
					}
				}
				else
				{
					dynamicObject["LockSecQty"] = leftSecQty;
					leftSecQty = 0m;
				}
			}
			if (material.AuxPropId > 0L)
			{
				dynamicObject["AuxPropertyId_Info_Id"] = material.AuxPropId;
			}
			dynamicObject["Lot_Info_Id"] = material.Lot;
			dynamicObject["ProduceDate_Info"] = material.ProduceDate;
			dynamicObject["EXPIRYDATE_Info"] = material.ExpiryDate;
			if (material.StockStatusID > 0L)
			{
				dynamicObject["StockStatusID_Info_Id"] = material.StockStatusID;
			}
			if (!string.IsNullOrWhiteSpace(material.OwnerTypeID))
			{
				dynamicObject["OwnerTypeID_Info"] = material.OwnerTypeID;
				dynamicObject["OwnerID_Info_Id"] = material.OwnerID;
			}
			if (!string.IsNullOrWhiteSpace(material.KeeperTypeID))
			{
				dynamicObject["KeeperTypeID_Info"] = material.KeeperTypeID;
				dynamicObject["KeeperID_Info_Id"] = material.KeeperID;
			}
			dynamicObject["STOCKID_Info_Id"] = material.STOCKID;
			dynamicObject["StockPlaceID_Info_Id"] = material.StockLocID;
			dynamicObject["BOMID_Info_Id"] = material.BOMID;
			dynamicObject["MtoNo_Info"] = material.MtoNo;
			dynamicObject["ProjectNo_Info"] = material.ProjectNo;
			dynamicObject["InvDetailID"] = material.FInvDetailID;
			dynamicObject["Seq"] = ++i;
			infoEntrys.Add(dynamicObject);
			return i;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0007D9C5 File Offset: 0x0007BBC5
		private void SplitHideCtrl(bool isVisible)
		{
			this.View.GetControl<SplitContainer>("FSpliteContainer").HideSecondPanel(!isVisible);
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0007D9E0 File Offset: 0x0007BBE0
		private long GetDynamicValue(DynamicObject obj)
		{
			if (obj == null)
			{
				return 0L;
			}
			if (obj.DynamicObjectType.Properties.ContainsKey(FormConst.MASTER_ID))
			{
				return Convert.ToInt64(obj[FormConst.MASTER_ID]);
			}
			if (obj.DynamicObjectType.Properties.ContainsKey("Id"))
			{
				return Convert.ToInt64(obj["Id"]);
			}
			return 0L;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x0007DA48 File Offset: 0x0007BC48
		private decimal GetConvertRate(long materialId, long sourceUnitId, long desUnitId, decimal sourceQty)
		{
			UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, new GetUnitConvertRateArgs
			{
				MaterialId = materialId,
				SourceUnitId = sourceUnitId,
				DestUnitId = desUnitId
			});
			return unitConvertRate.ConvertQty(sourceQty, "");
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0007DA8C File Offset: 0x0007BC8C
		protected string GetLotCommonFilter(LotField lotField, bool useNStatusLot, int index = -1)
		{
			int num = index;
			if (num == -1)
			{
				num = this.View.Model.GetEntryCurrentRowIndex(lotField.EntityKey);
			}
			long num2 = 0L;
			DynamicObject dynamicObject = this.View.Model.GetValue(lotField.OrgFieldKey, num) as DynamicObject;
			if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
			{
				num2 = Convert.ToInt64(dynamicObject["Id"]);
			}
			long num3 = 0L;
			dynamicObject = (this.View.Model.GetValue(lotField.ControlFieldKey, num) as DynamicObject);
			if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
			{
				num3 = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
			}
			if (num2 < 1L || num3 < 1L)
			{
				return "";
			}
			string str = " ( ";
			str += string.Format(" FUseOrgId = {0} AND FMaterialId = {1} ", num2, num3);
			return str + ") ";
		}

		// Token: 0x040003B0 RID: 944
		private string LockDemandEntity = "FEntityLock";

		// Token: 0x040003B1 RID: 945
		private string LockDetailEntity = "FSubEntiry";

		// Token: 0x040003B2 RID: 946
		private bool IsClose;

		// Token: 0x040003B3 RID: 947
		private List<long> orgLockStockList;

		// Token: 0x040003B4 RID: 948
		private bool _isFlex;
	}
}

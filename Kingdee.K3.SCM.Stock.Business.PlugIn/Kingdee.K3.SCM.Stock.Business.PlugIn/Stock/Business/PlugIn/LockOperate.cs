using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200006E RID: 110
	public class LockOperate : AbstractDynamicFormPlugIn
	{
		// Token: 0x060004D4 RID: 1236 RVA: 0x00039F04 File Offset: 0x00038104
		public override void OnInitialize(InitializeEventArgs e)
		{
			try
			{
				if (e.Paramter.GetCustomParameter("Parameters") != null)
				{
					this.paraStr = e.Paramter.GetCustomParameter("Parameters").ToString();
				}
				if (e.Paramter.GetCustomParameter("OpType") != null)
				{
					this.opType = e.Paramter.GetCustomParameter("OpType").ToString();
				}
				if (e.Paramter.GetCustomParameter("OrgId") != null && this.opType.Equals("Inv"))
				{
					this.orgId = Convert.ToInt64(e.Paramter.GetCustomParameter("OrgId").ToString());
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException(ex.Message);
			}
			BOSRule item = this.View.RuleContainer.AddPluginRule("FEntity", 41, new Action<DynamicObject, object>(this.ReviseValue), new string[]
			{
				"FLockQty",
				"FSecLockQty"
			});
			this.View.RuleContainer.Rules.Remove(item);
			this.View.RuleContainer.Rules.Insert(0, item);
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x0003A038 File Offset: 0x00038238
		private void ReviseValue(DynamicObject row, dynamic dynamicRow)
		{
			if ((decimal)row["LockQty"] > (decimal)row["ValidQty"])
			{
				row["LockQty"] = row["ValidQty"];
			}
			if ((decimal)row["SecLockQty"] > (decimal)row["SecValidQty"])
			{
				if ((decimal)row["SecValidQty"] >= 0m)
				{
					row["SecLockQty"] = row["SecValidQty"];
					return;
				}
				row["SecLockQty"] = 0;
			}
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0003A10C File Offset: 0x0003830C
		public override void AfterCreateNewData(EventArgs e)
		{
			IEnumerable<InvQueryRetRecord> enumerable = null;
			if (!string.IsNullOrWhiteSpace(this.paraStr))
			{
				enumerable = from p in StockServiceHelper.GetLockInvDatas(this.View.Context, this.paraStr, this.orgId, false)
				where p.BaseQty - p.BaseLockQty > 0m
				select p;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			int i = 0;
			foreach (InvQueryRetRecord data in enumerable)
			{
				i = this.CreateNewRow(entity, entityDataObject, i, data);
			}
			DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), entity.DynamicObjectType, false);
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0003A1F8 File Offset: 0x000383F8
		private int CreateNewRow(Entity entity, DynamicObjectCollection objs, int i, InvQueryRetRecord data)
		{
			DynamicObject dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["InvDetailID"] = data.InventoryID;
			dynamicObject["StockOrgID_Id"] = data.StockOrgID;
			dynamicObject["MaterialID_Id"] = data.MaterialID;
			dynamicObject["BaseUnitId_Id"] = data.BaseUnitID;
			dynamicObject["UnitID_Id"] = data.UnitID;
			dynamicObject["SecUnitId_Id"] = data.SecUnitID;
			dynamicObject["SecQty"] = data.SecQty;
			dynamicObject["SecValidQty"] = data.SecQty - data.SecLockQty;
			dynamicObject["SecLockedQty"] = data.SecLockQty;
			dynamicObject["BaseQty"] = data.BaseQty;
			dynamicObject["BaseValidQty"] = data.BaseQty - data.BaseLockQty;
			dynamicObject["BaseLockedQty"] = data.BaseLockQty;
			dynamicObject["Qty"] = data.Qty;
			dynamicObject["ValidQty"] = data.Qty - data.LockQty;
			dynamicObject["LockedQty"] = data.LockQty;
			dynamicObject["BomID_Id"] = data.BOMID;
			dynamicObject["Lot_Id"] = data.Lot;
			dynamicObject["ProjectNo"] = data.ProjectNo;
			dynamicObject["MtoNo"] = data.MtoNo;
			dynamicObject["StockID_Id"] = data.StockID;
			dynamicObject["StockLocId_Id"] = data.StockPlaceID;
			dynamicObject["StockStatusID_Id"] = data.StockStatusID;
			dynamicObject["OwnerTypeId"] = data.OwnerTypeID;
			dynamicObject["OwnerId_Id"] = data.OwnerID;
			dynamicObject["KeeperTypeId"] = data.KeeperTypeID;
			dynamicObject["KeeperID_Id"] = data.KeeperID;
			dynamicObject["ProduceDate"] = data.ProduceDate;
			dynamicObject["ExpiryDate"] = data.ValidateTo;
			dynamicObject["AuxPropId_Id"] = data.AuxpropertyID;
			dynamicObject["ReserveDate"] = DateTime.Today;
			dynamicObject["ReserveUnit"] = "D";
			dynamicObject["Seq"] = ++i;
			objs.Add(dynamicObject);
			return i;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0003A4FC File Offset: 0x000386FC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBSAVE"))
				{
					if (!(a == "TBCANCEL"))
					{
						return;
					}
				}
				else
				{
					this.SaveLockInfo();
				}
			}
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x0003A53C File Offset: 0x0003873C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FReleaseDate"))
				{
					if (!(key == "FSecLockQty"))
					{
						return;
					}
					decimal d = Convert.ToDecimal(this.Model.GetValue("FSecValidQty", e.Row));
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

		// Token: 0x060004DA RID: 1242 RVA: 0x0003A620 File Offset: 0x00038820
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
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

		// Token: 0x060004DB RID: 1243 RVA: 0x0003A728 File Offset: 0x00038928
		private void SaveLockInfo()
		{
			List<LockStockArgs> list = new List<LockStockArgs>();
			string a;
			if ((a = this.opType) != null)
			{
				if (!(a == "Inv"))
				{
					return;
				}
				Entity entity = this.View.BillBusinessInfo.GetEntity("FEntity");
				List<DynamicObject> list2 = (from p in this.View.Model.GetEntityDataObject(entity)
				where Convert.ToDecimal(p["LockQty"]) > 0m && p["MaterialId"] != null
				select p).ToList<DynamicObject>();
				if (list2.Count > 0)
				{
					foreach (DynamicObject dynamicObject in list2)
					{
						if (this.CheckSubData(dynamicObject))
						{
							list.Add(this.GetSubData(dynamicObject));
						}
					}
					if (list.Count > 0)
					{
						StockServiceHelper.SaveLockInfo(base.Context, list, "Inv", false);
						this.View.ShowMessage(ResManager.LoadKDString("锁库成功", "004023030002176", 5, new object[0]), 0);
						this.View.Refresh();
						this.View.ReturnToParentWindow(true);
						return;
					}
				}
				else
				{
					this.View.ShowMessage(ResManager.LoadKDString("锁库数量不能小于等于0", "004023000012233", 5, new object[0]), 0);
				}
			}
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0003A884 File Offset: 0x00038A84
		private bool CheckSubData(DynamicObject obj)
		{
			bool flag = true;
			int num = Convert.ToInt32(obj["Seq"]);
			decimal d = Convert.ToDecimal(obj["LockQty"]);
			decimal num2 = Convert.ToDecimal(obj["ValidQty"]);
			DynamicObject dynamicObject = (DynamicObject)obj["SecUnitId"];
			decimal d2 = Convert.ToDecimal(obj["SecLockQty"]);
			decimal d3 = Convert.ToDecimal(obj["SecValidQty"]);
			DynamicObject dynamicObject2 = (DynamicObject)obj["MaterialId"];
			StringBuilder stringBuilder = new StringBuilder();
			if (num2 <= 0m)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行物料【{1}】的可锁数量≤0", "004023030000289", 5, new object[0]), num, dynamicObject2["Name"]));
				flag = false;
			}
			if (d > num2)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行物料【{1}】的锁库量＞可锁量", "004023030000292", 5, new object[0]), num, dynamicObject2["Name"]));
				flag = false;
			}
			if (dynamicObject != null && d2 > 0m && d2 > d3)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行物料【{1}】的锁库量(辅助)大于可锁量(辅助)", "004023030000298", 5, new object[0]), num, dynamicObject2["Name"]));
				flag = false;
			}
			if (!flag)
			{
				this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
			}
			return flag;
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0003AA0C File Offset: 0x00038C0C
		private LockStockArgs GetSubData(DynamicObject materialObj)
		{
			LockStockArgs lockStockArgs = new LockStockArgs();
			lockStockArgs.ObjectId = "STK_Inventory";
			lockStockArgs.BillId = materialObj["InvDetailID"].ToString();
			lockStockArgs.BillDetailID = "";
			lockStockArgs.FInvDetailID = materialObj["InvDetailID"].ToString();
			lockStockArgs.StockOrgID = Convert.ToInt64(((DynamicObject)materialObj["StockOrgId"])["Id"]);
			lockStockArgs.DemandOrgId = lockStockArgs.StockOrgID;
			lockStockArgs.MaterialID = this.GetDynamicValue(materialObj["MaterialId"] as DynamicObject);
			lockStockArgs.DemandMaterialId = lockStockArgs.MaterialID;
			lockStockArgs.DemandDateTime = null;
			lockStockArgs.DemandPriority = "";
			object obj = ((DynamicObjectCollection)(materialObj["MaterialID"] as DynamicObject)["MaterialPlan"])[0]["PlanMode"];
			if (obj != null && obj.ToString() == "1")
			{
				lockStockArgs.IsMto = "1";
			}
			lockStockArgs.BOMID = this.GetDynamicValue(materialObj["BomId"] as DynamicObject);
			lockStockArgs.AuxPropId = this.GetDynamicValue(materialObj["AuxPropId"] as DynamicObject);
			DynamicObject dynamicObject = materialObj["Lot"] as DynamicObject;
			if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
			{
				lockStockArgs.Lot = this.GetDynamicValue(dynamicObject);
				lockStockArgs.LotNo = dynamicObject["Number"].ToString();
			}
			lockStockArgs.MtoNo = materialObj["MtoNo"].ToString();
			lockStockArgs.ProjectNo = materialObj["ProjectNo"].ToString();
			if (materialObj["ProduceDate"] != null)
			{
				lockStockArgs.ProduceDate = new DateTime?(DateTime.Parse(materialObj["ProduceDate"].ToString()));
			}
			if (materialObj["ExpiryDate"] != null)
			{
				lockStockArgs.ExpiryDate = new DateTime?(DateTime.Parse(materialObj["ExpiryDate"].ToString()));
			}
			lockStockArgs.STOCKID = this.GetDynamicValue(materialObj["StockId"] as DynamicObject);
			lockStockArgs.StockLocID = this.GetDynamicValue(materialObj["StockLocId"] as DynamicObject);
			lockStockArgs.StockStatusID = this.GetDynamicValue(materialObj["StockStatusId"] as DynamicObject);
			lockStockArgs.OwnerTypeID = materialObj["OwnerTypeId"].ToString();
			lockStockArgs.OwnerID = this.GetDynamicValue(materialObj["OwnerId"] as DynamicObject);
			lockStockArgs.KeeperTypeID = materialObj["KeeperTypeId"].ToString();
			lockStockArgs.KeeperID = this.GetDynamicValue(materialObj["KeeperId"] as DynamicObject);
			lockStockArgs.UnitID = this.GetDynamicValue(materialObj["UnitID"] as DynamicObject);
			lockStockArgs.BaseUnitID = this.GetDynamicValue(materialObj["BaseUnitId"] as DynamicObject);
			lockStockArgs.SecUnitID = this.GetDynamicValue(materialObj["SecUnitId"] as DynamicObject);
			lockStockArgs.LockQty = decimal.Parse(materialObj["LockQty"].ToString());
			lockStockArgs.LockBaseQty = decimal.Parse(materialObj["BaseLockQty"].ToString());
			lockStockArgs.LockSecQty = decimal.Parse(materialObj["SecLockQty"].ToString());
			object obj2 = materialObj["ReserveDate"];
			if (obj2 != null && !string.IsNullOrWhiteSpace(obj2.ToString()))
			{
				lockStockArgs.ReserveDate = new DateTime?(DateTime.Parse(obj2.ToString()));
			}
			lockStockArgs.ReserveDays = Convert.ToInt32(materialObj["ReserveDays"]);
			obj2 = materialObj["ReleaseDate"];
			if (obj2 != null && !string.IsNullOrWhiteSpace(obj2.ToString()))
			{
				lockStockArgs.ReLeaseDate = new DateTime?(DateTime.Parse(obj2.ToString()));
			}
			lockStockArgs.SupplyNote = Convert.ToString(materialObj["SupplyNote"]);
			lockStockArgs.RequestNote = lockStockArgs.SupplyNote;
			return lockStockArgs;
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0003AE28 File Offset: 0x00039028
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

		// Token: 0x040001CF RID: 463
		private string opType = "Inv";

		// Token: 0x040001D0 RID: 464
		private string paraStr;

		// Token: 0x040001D1 RID: 465
		private long orgId;
	}
}

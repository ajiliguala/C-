using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.BusinessFlow.ReserveLogic;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A7 RID: 167
	public class UnLockStockOperate : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A5A RID: 2650 RVA: 0x0008D714 File Offset: 0x0008B914
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
				if (e.Paramter.GetCustomParameter("ObjectId") != null)
				{
					this.objectId = e.Paramter.GetCustomParameter("ObjectId").ToString();
				}
				if (e.Paramter.GetCustomParameter("EntiryKey") != null)
				{
					this.entiryKey = e.Paramter.GetCustomParameter("EntiryKey").ToString();
				}
				if (this.opType.Equals("StockLockLog"))
				{
					this.dyStockLockLogs = (e.Paramter.GetCustomParameter("StockLockLogData") as List<DynamicObject>);
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException(ex.Message);
			}
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x0008D824 File Offset: 0x0008BA24
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x0008D844 File Offset: 0x0008BA44
		public override void AfterCreateNewData(EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.paraStr) || string.IsNullOrWhiteSpace(this.opType))
			{
				return;
			}
			List<LockStockArgs> lockAndBillInfoByID = StockServiceHelper.GetLockAndBillInfoByID(base.Context, this.paraStr, this.opType, false);
			if (lockAndBillInfoByID.Count > 0)
			{
				Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
				DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				entityDataObject.Clear();
				int num = 0;
				foreach (LockStockArgs lockStockArgs in lockAndBillInfoByID)
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["FSelect"] = true;
					dynamicObject["ID"] = lockStockArgs.FEntryID;
					dynamicObject["InvDetailID"] = lockStockArgs.FInvDetailID;
					dynamicObject["BILLTypeID_Id"] = lockStockArgs.BillTypeID;
					dynamicObject["ObjectId_Id"] = lockStockArgs.ObjectId;
					dynamicObject["BillDetailID"] = lockStockArgs.BillDetailID;
					dynamicObject["BILLNO"] = lockStockArgs.BillNo;
					dynamicObject["BILLSEQ"] = lockStockArgs.BillSEQ;
					dynamicObject["StockOrgId_Id"] = lockStockArgs.StockOrgID;
					dynamicObject["MaterialId_Id"] = lockStockArgs.MaterialID;
					dynamicObject["BomId_Id"] = lockStockArgs.BOMID;
					dynamicObject["Lot_Id"] = lockStockArgs.Lot;
					dynamicObject["ProduceDate"] = lockStockArgs.ProduceDate;
					dynamicObject["ExpiryDate"] = lockStockArgs.ExpiryDate;
					dynamicObject["StockID_Id"] = lockStockArgs.STOCKID;
					dynamicObject["StockLocID_Id"] = lockStockArgs.StockLocID;
					dynamicObject["OwnerTypeID"] = lockStockArgs.OwnerTypeID;
					dynamicObject["OwnerID_Id"] = lockStockArgs.OwnerID;
					dynamicObject["KeeperTypeID"] = lockStockArgs.KeeperTypeID;
					dynamicObject["KeeperID_Id"] = lockStockArgs.KeeperID;
					dynamicObject["StockStatusID_Id"] = lockStockArgs.StockStatusID;
					dynamicObject["BaseUnitId_Id"] = lockStockArgs.BaseUnitID;
					dynamicObject["UnitID_Id"] = lockStockArgs.UnitID;
					dynamicObject["SecUnitID_Id"] = lockStockArgs.SecUnitID;
					dynamicObject["BaseLcokQty"] = lockStockArgs.LockBaseQty;
					dynamicObject["LockQty"] = lockStockArgs.LockQty;
					dynamicObject["SecLockQty"] = lockStockArgs.LockSecQty;
					this.SetUnLockQty(dynamicObject, lockStockArgs);
					dynamicObject["AuxPropId_Id"] = lockStockArgs.AuxPropId;
					dynamicObject["MtoNo"] = lockStockArgs.MtoNo;
					dynamicObject["ProjectNo"] = lockStockArgs.ProjectNo;
					dynamicObject["ReserveDate"] = lockStockArgs.ReserveDate;
					dynamicObject["ReserveDays"] = lockStockArgs.ReserveDays;
					dynamicObject["ReleaseDate"] = lockStockArgs.ReLeaseDate;
					dynamicObject["RequestNote"] = lockStockArgs.RequestNote;
					dynamicObject["SupplyNote"] = lockStockArgs.SupplyNote;
					dynamicObject["Seq"] = ++num;
					entityDataObject.Add(dynamicObject);
				}
				DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), dynamicObjectType, false);
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("无锁库信息，无法解锁", "004023030000418", 5, new object[0]), 1, delegate(MessageBoxResult result)
			{
				this.IsClose = true;
				this.View.Close();
			}, "", 0);
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x0008DCA4 File Offset: 0x0008BEA4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "BTN_SAVE"))
				{
					if (!(a == "TBRESERVE"))
					{
						return;
					}
					this.DoReserve();
				}
				else
				{
					e.Cancel = this.SaveUnLockStock();
					if (!e.Cancel)
					{
						this.View.Close();
						return;
					}
				}
			}
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x0008DD70 File Offset: 0x0008BF70
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			if (!this.IsClose)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("是否保存解锁信息?", "004023030000421", 5, new object[0]), 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						e.Cancel = this.SaveUnLockStock();
						if (!e.Cancel)
						{
							this.View.Close();
							return;
						}
					}
					else
					{
						this.IsClose = true;
						this.View.Close();
					}
				}, "", 0);
			}
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x0008DE40 File Offset: 0x0008C040
		private void SetUnLockQty(DynamicObject obj, LockStockArgs materialInfo)
		{
			obj["UnLockQty"] = materialInfo.LockQty;
			obj["BaseUnLockQty"] = materialInfo.LockBaseQty;
			obj["SecUnLockQty"] = materialInfo.LockSecQty;
			if (this.opType.Equals("StockLockLog") && this.dyStockLockLogs != null)
			{
				IEnumerable<DynamicObject> enumerable = from p in this.dyStockLockLogs
				where Convert.ToInt64(p["FENTRYID"]) == materialInfo.FEntryID
				select p;
				if (enumerable != null && enumerable.Count<DynamicObject>() > 0)
				{
					decimal num = (from p in enumerable
					select Convert.ToDecimal(p["FBASEQTY"])).Sum();
					if (num < materialInfo.LockBaseQty)
					{
						obj["UnLockQty"] = (from p in enumerable
						select Convert.ToDecimal(p["FQTY"])).Sum();
						obj["BaseUnLockQty"] = num;
					}
					num = (from p in enumerable
					select Convert.ToDecimal(p["FSECQTY"])).Sum();
					if (num < materialInfo.LockBaseQty)
					{
						obj["SecUnLockQty"] = num;
					}
				}
			}
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x0008E0CC File Offset: 0x0008C2CC
		private bool SaveUnLockStock()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			List<LockStockArgs> stockArgList = new List<LockStockArgs>();
			List<LockStockArgs> warnStockArgList = new List<LockStockArgs>();
			bool isCancel = false;
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			int num = 0;
			bool flag = false;
			string format = ResManager.LoadKDString("第{0}行记录", "004023000017456", 5, new object[0]);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				num++;
				if (Convert.ToBoolean(dynamicObject["FSelect"]))
				{
					flag = true;
					isCancel = false;
					bool flag2 = false;
					decimal unLockQty = Convert.ToDecimal(dynamicObject["UnLockQty"]);
					decimal lockQty = Convert.ToDecimal(dynamicObject["LockQty"]);
					long dynamicValue = this.GetDynamicValue(dynamicObject["SecUnitID"] as DynamicObject);
					decimal num2 = Convert.ToDecimal(dynamicObject["SecUnLockQty"]);
					decimal d = Convert.ToDecimal(dynamicObject["SecLockQty"]);
					Convert.ToDecimal(dynamicObject["LeftQty"]);
					decimal d2 = Convert.ToDecimal(dynamicObject["SecLeftQty"]);
					decimal num3 = Convert.ToDecimal(dynamicObject["BaseUnLockQty"]);
					decimal d3 = Convert.ToDecimal(dynamicObject["BaseLcokQty"]);
					decimal d4 = Convert.ToDecimal(dynamicObject["BaseLeftQty"]);
					if (!(num3 == 0m) || !(num2 == 0m))
					{
						if (dynamicObject["ReleaseDate"] != null && !string.IsNullOrWhiteSpace(dynamicObject["ReleaseDate"].ToString()) && DateTime.Parse(dynamicObject["ReleaseDate"].ToString()) != DateTime.Today)
						{
							flag2 = true;
							string format2 = ResManager.LoadKDString("第{0}行记录的预计解锁日期{1}不等于系统当前日期{2}", "004023030009708", 5, new object[0]);
							list2.Add(string.Format(format2, num, ((DateTime)dynamicObject["ReleaseDate"]).ToShortDateString(), DateTime.Today.ToShortDateString()));
						}
						if (num3 < 0m)
						{
							list.Add(string.Format(format, num) + ResManager.LoadKDString("解锁数量（基本）不能小于0", "004023000022233", 5, new object[0]));
							isCancel = true;
						}
						else if (d3 < num3)
						{
							list.Add(string.Format(format, num) + ResManager.LoadKDString("解锁数量（基本）超过可解锁数量（基本）", "004023000022234", 5, new object[0]));
							isCancel = true;
						}
						else if (dynamicValue > 0L)
						{
							if (d < num2)
							{
								list.Add(string.Format(format, num) + ResManager.LoadKDString("解锁数量（辅助）超过可解锁数量（辅助）", "004023030000436", 5, new object[0]));
								isCancel = true;
							}
							else if (d4 == 0m && d2 != 0m)
							{
								list.Add(string.Format(format, num) + ResManager.LoadKDString("剩余解锁量（基本）和剩余解锁量（辅助）一个为0，另一个不为0", "004023000022246", 5, new object[0]));
								isCancel = true;
							}
						}
						if (!isCancel)
						{
							LockStockArgs lockStockArgs = new LockStockArgs();
							lockStockArgs.FEntryID = Convert.ToInt64(dynamicObject["Id"]);
							lockStockArgs.FInvDetailID = dynamicObject["InvDetailID"].ToString();
							lockStockArgs.BillDetailID = dynamicObject["BillDetailID"].ToString();
							lockStockArgs.BillNo = Convert.ToString(dynamicObject["BILLNO"]);
							if (Convert.ToInt32(dynamicObject["BILLSEQ"]) > 0)
							{
								lockStockArgs.BillSEQ = Convert.ToInt32(dynamicObject["BILLSEQ"]);
							}
							DynamicObject dynamicObject2 = dynamicObject["Lot"] as DynamicObject;
							if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L)
							{
								lockStockArgs.Lot = this.GetDynamicValue(dynamicObject2);
							}
							lockStockArgs.LockQty = lockQty;
							lockStockArgs.UnLockQty = unLockQty;
							lockStockArgs.LockBaseQty = Convert.ToDecimal(dynamicObject["BaseLcokQty"]);
							lockStockArgs.UnLockBaseQty = num3;
							lockStockArgs.LockSecQty = Convert.ToDecimal(dynamicObject["SecLockQty"]);
							lockStockArgs.UnLockSecQty = num2;
							if (dynamicObject["ReserveDate"] != null && !string.IsNullOrWhiteSpace(dynamicObject["ReserveDate"].ToString()))
							{
								lockStockArgs.ReserveDate = new DateTime?(DateTime.Parse(dynamicObject["ReserveDate"].ToString()));
							}
							lockStockArgs.ReserveDays = Convert.ToInt32(dynamicObject["ReserveDays"]);
							if (dynamicObject["ReleaseDate"] != null && !string.IsNullOrWhiteSpace(dynamicObject["ReleaseDate"].ToString()))
							{
								lockStockArgs.ReLeaseDate = new DateTime?(DateTime.Parse(dynamicObject["ReleaseDate"].ToString()));
							}
							lockStockArgs.UnLockNote = Convert.ToString(dynamicObject["UnLockNote"]);
							if (flag2)
							{
								warnStockArgList.Add(lockStockArgs);
							}
							else
							{
								stockArgList.Add(lockStockArgs);
							}
						}
					}
				}
			}
			if (!flag)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据解锁，请先选择数据。", "004023000019865", 5, new object[0]), "", 0);
				return true;
			}
			isCancel = false;
			if (list.Count > 0)
			{
				isCancel = true;
				List<FieldAppearance> list3 = new List<FieldAppearance>();
				FieldAppearance fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(this.View.Context, "FErrInfo", ResManager.LoadKDString("异常信息", "004023000017458", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("500", this.View.Context.UserLocale.LCID);
				list3.Add(fieldAppearance);
				K3DisplayerModel k3DisplayerModel = K3DisplayerModel.Create(this.View.Context, list3.ToArray(), null);
				foreach (string text in list)
				{
					k3DisplayerModel.AddMessage(text);
				}
				k3DisplayerModel.CancelButton.Visible = false;
				this.View.ShowK3Displayer(k3DisplayerModel, null, "BOS_K3Displayer");
				return true;
			}
			if (list2.Count > 0)
			{
				List<FieldAppearance> list4 = new List<FieldAppearance>();
				FieldAppearance fieldAppearance2 = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(this.View.Context, "FErrInfo", ResManager.LoadKDString("异常信息", "004023000017458", 5, new object[0]), "", null);
				fieldAppearance2.Width = new LocaleValue("500", this.View.Context.UserLocale.LCID);
				list4.Add(fieldAppearance2);
				K3DisplayerModel k3DisplayerModel2 = K3DisplayerModel.Create(this.View.Context, list4.ToArray(), null);
				foreach (string text2 in list2)
				{
					k3DisplayerModel2.AddMessage(text2);
				}
				k3DisplayerModel2.CancelButton.Visible = true;
				k3DisplayerModel2.CancelButton.Caption = new LocaleValue(ResManager.LoadKDString("否", "004023000013912", 5, new object[0]));
				k3DisplayerModel2.OKButton.Caption = new LocaleValue(ResManager.LoadKDString("是", "004023030005539", 5, new object[0]));
				k3DisplayerModel2.OKButton.Visible = true;
				k3DisplayerModel2.SummaryMessage = ResManager.LoadKDString("以下锁库记录的预计解锁日期与当前日期不同，是否继续？", "004023000017459", 5, new object[0]);
				this.View.ShowK3Displayer(k3DisplayerModel2, delegate(FormResult o)
				{
					if (o != null && o.ReturnData is K3DisplayerModel && (o.ReturnData as K3DisplayerModel).IsOK)
					{
						isCancel = true;
						stockArgList.AddRange(warnStockArgList);
						if (stockArgList.Count > 0)
						{
							StockServiceHelper.SaveUnLockInfo(this.Context, stockArgList, (this.opType.Equals("StockLock") || this.opType.Equals("StockLockLog")) ? "Inv" : this.opType);
							this.IsClose = true;
							this.View.ReturnToParentWindow(true);
							this.View.Close();
							return;
						}
					}
					else
					{
						isCancel = true;
					}
				}, "BOS_K3Displayer");
				return true;
			}
			if (!isCancel && stockArgList.Count > 0)
			{
				StockServiceHelper.SaveUnLockInfo(base.Context, stockArgList, (this.opType.Equals("StockLock") || this.opType.Equals("StockLockLog")) ? "Inv" : this.opType);
				this.View.ShowNotificationMessage(ResManager.LoadKDString("解锁操作成功!", "004023030000442", 5, new object[0]), "", 0);
				this.IsClose = true;
				this.View.ReturnToParentWindow(true);
			}
			else if (!isCancel)
			{
				this.IsClose = true;
				this.View.ReturnToParentWindow(false);
			}
			return isCancel;
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x0008EA34 File Offset: 0x0008CC34
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

		// Token: 0x06000A62 RID: 2658 RVA: 0x0008EA9C File Offset: 0x0008CC9C
		private void DoReserve()
		{
			ReserveViewOpenOption reserveViewOption = this.GetReserveViewOption();
			if (reserveViewOption == null)
			{
				return;
			}
			this.ShowReserveView(reserveViewOption);
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x0008EABC File Offset: 0x0008CCBC
		private ReserveViewOpenOption GetReserveViewOption()
		{
			DynamicObject dynamicObject = this.Model.GetValue("FObjectId", this.Model.GetEntryCurrentRowIndex("FEntity")) as DynamicObject;
			if (dynamicObject == null || !StringUtils.EqualsIgnoreCase(dynamicObject["Id"].ToString(), "SAL_SaleOrder"))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("只有销售订单锁库才能执行高级预留！", "004023030004327", 5, new object[0]), "", 0);
				return null;
			}
			if (this.orderBusiness == null)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "SAL_SaleOrder", true) as FormMetadata;
				if (formMetadata == null)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("读取销售订单配置信息错误", "004023030004330", 5, new object[0]), "", 0);
					return null;
				}
				this.orderBusiness = formMetadata.BusinessInfo;
			}
			FormOperation operation = this.orderBusiness.GetForm().GetOperation("Reserve");
			if (operation == null)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("读取销售订单预留操作配置信息错误", "004023030004333", 5, new object[0]), "", 0);
				return null;
			}
			OperationParameter parmeter = operation.Parmeter;
			ReserveViewOpenOption reserveViewOpenOption = new ReserveViewOpenOption();
			Entity entity = this.orderBusiness.GetEntity("FSaleOrderEntry");
			reserveViewOpenOption.OperationBillBusinessInfo = this.orderBusiness;
			reserveViewOpenOption.OperationEntryKey = entity.Key;
			reserveViewOpenOption.OperationBillInfo = this.GetOpationBillInfo();
			if (reserveViewOpenOption.OperationBillInfo != null && reserveViewOpenOption.OperationBillInfo.Count < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("选中的分录都无法执行高级预留，请检查订单的预留类型！", "004023030005993", 5, new object[0]), "", 0);
				return null;
			}
			reserveViewOpenOption.ReturnType = "1";
			return reserveViewOpenOption;
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x0008EC64 File Offset: 0x0008CE64
		private List<OriBillInfo> GetOpationBillInfo()
		{
			int[] selectedRows = this.View.GetControl<EntryGrid>("FEntity").GetSelectedRows();
			if (selectedRows == null || selectedRows.Length < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请先选择要执行高级预留的需求分录！", "004023030005994", 5, new object[0]), "", 0);
				return null;
			}
			string text = "SAL_SaleOrder";
			List<OriBillInfo> list = new List<OriBillInfo>();
			List<long> list2 = new List<long>();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			foreach (int index in selectedRows)
			{
				long item = Convert.ToInt64(entityDataObject[index]["BillDetailID"]);
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
			}
			List<SelectorItemInfo> list3 = new List<SelectorItemInfo>();
			list3.Add(new SelectorItemInfo("FBillNo"));
			list3.Add(new SelectorItemInfo("FReserveType"));
			OQLFilter oqlfilter = new OQLFilter();
			oqlfilter.Add(new OQLFilterHeadEntityItem
			{
				EntityKey = "FBillHead",
				FilterString = string.Format(" EXISTS (SELECT 1 FROM {0} OE WHERE FID = OE.FID AND OE.FENTRYID IN ({1} ))", "T_SAL_ORDERENTRY", list2.ToString())
			});
			DynamicObject[] array2 = BusinessDataServiceHelper.Load(this.View.Context, text, list3, oqlfilter);
			if (selectedRows == null || selectedRows.Length < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("选择分录对应的销售订单不存在或者已经被删除！", "004023030005995", 5, new object[0]), "", 0);
				return null;
			}
			foreach (DynamicObject dynamicObject in array2)
			{
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["SaleOrderEntry"];
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					long item2 = Convert.ToInt64(dynamicObject2["Id"]);
					string text2 = (dynamicObject2["ReserveType"] == null) ? "" : dynamicObject2["ReserveType"].ToString();
					if (!string.IsNullOrWhiteSpace(text2) && text2 != "2" && list2.Contains(item2))
					{
						OriBillInfo item3 = new OriBillInfo(text, dynamicObject["Id"].ToString(), item2.ToString());
						list.Add(item3);
					}
				}
			}
			return list;
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x0008EF20 File Offset: 0x0008D120
		private void ShowReserveView(object inputParam)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "PLN_REVERSEVIEW";
			dynamicFormShowParameter.Height = 600;
			dynamicFormShowParameter.Width = 800;
			if (inputParam != null)
			{
				this.View.Session["FormInputParam"] = inputParam;
			}
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (inputParam != null)
				{
					this.View.Session.Remove("FormInputParam");
				}
			});
		}

		// Token: 0x04000420 RID: 1056
		private string opType = "Inv";

		// Token: 0x04000421 RID: 1057
		private string paraStr;

		// Token: 0x04000422 RID: 1058
		private bool IsClose;

		// Token: 0x04000423 RID: 1059
		private string objectId;

		// Token: 0x04000424 RID: 1060
		private string entiryKey;

		// Token: 0x04000425 RID: 1061
		private BusinessInfo orderBusiness;

		// Token: 0x04000426 RID: 1062
		private bool usePLNReserve;

		// Token: 0x04000427 RID: 1063
		private List<DynamicObject> dyStockLockLogs;
	}
}

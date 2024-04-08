using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.SUB;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Core.Utils;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200006B RID: 107
	public class InStockList : AbstractListPlugIn
	{
		// Token: 0x060004A6 RID: 1190 RVA: 0x00037502 File Offset: 0x00035702
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			this.callSys = Convert.ToString(e.OpenParameter.SubSystemId);
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00037521 File Offset: 0x00035721
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00037538 File Offset: 0x00035738
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (e.BarItemKey.ToUpperInvariant().Equals("TB_BACKFLUSH"))
			{
				this.ShowBackFlush();
			}
			if (e.BarItemKey.ToUpperInvariant().Equals("TBLOTSNR"))
			{
				this.ProcessLotSNR();
			}
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBBUSINESSPUSH"))
				{
					return;
				}
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				List<long> list = (from p in selectedRowsInfo
				select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
				bool flag = CommonServiceHelper.IsContainsIOSBill(base.Context, list, "T_STK_INSTOCKFIN");
				if (flag)
				{
					e.Cancel = true;
					this.View.ShowErrMessage(ResManager.LoadKDString("选择下推的单据包含有内部交易单据，不允许下推！", "004023000010996", 5, new object[0]), "", 0);
				}
			}
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00037620 File Offset: 0x00035820
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			bool flag = Convert.ToBoolean(e.CustomFilter["FSelectAllOrg"]);
			string text2 = string.Empty;
			if (!flag)
			{
				text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			}
			else
			{
				List<long> isolationOrgList = this.ListView.Model.FilterParameter.IsolationOrgList;
				if (isolationOrgList != null && isolationOrgList.Count<long>() > 0)
				{
					text2 = string.Join<long>(",", isolationOrgList);
				}
			}
			if (this.ListView.OpenParameter.IsIsolationOrg)
			{
				text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
				{
					OrgIdKey = "FStockOrgId",
					PurchaseParameterKey = "GroupDataIsolation",
					PurchaseParameterObject = "PUR_SystemParameter",
					BusinessGroupKey = "FSTOCKERGROUPID",
					OperatorType = "WHY"
				});
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.AppendQueryFilter(text);
			}
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x00037714 File Offset: 0x00035914
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			List<long> list = (from p in selectedRowsInfo
			select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DISASSEMBLY"))
				{
					if (!(a == "MERGE"))
					{
						return;
					}
					if (list.Distinct<long>().Count<long>() > 1)
					{
						this.ListView.ShowMessage(ResManager.LoadKDString("只能选择一张单据进行合并操作！", "004023000021642", 5, new object[0]), 0);
						e.Cancel = true;
					}
					object hslastestCloseDate = CommonServiceHelper.GetHSLastestCloseDate(base.Context, selectedRowsInfo[0].MainOrgId);
					if (hslastestCloseDate != null)
					{
						DateTime t = Convert.ToDateTime(selectedRowsInfo[0].DataRow["FDate"]);
						DateTime t2 = Convert.ToDateTime(hslastestCloseDate);
						if (t <= t2)
						{
							this.ListView.ShowErrMessage(ResManager.LoadKDString("当前合并单据的入库日期小于等于本期核算关账日期，不允许进行合并操作！", "004023030035010", 5, new object[0]), "", 0);
							e.Cancel = true;
						}
					}
				}
				else
				{
					if (this.ListView.SelectedRowsInfo.Count == 0)
					{
						this.ListView.ShowMessage(ResManager.LoadKDString("请至少选择一行分录！", "004023030004276", 5, new object[0]), 0);
						e.Cancel = true;
					}
					if (list.Distinct<long>().Count<long>() > 1)
					{
						this.ListView.ShowMessage(ResManager.LoadKDString("只能选择一张单据进行拆分操作！", "004023000021640", 5, new object[0]), 0);
						e.Cancel = true;
					}
					long num = Convert.ToInt64(list[0]);
					if (num != 0L && PurchaseNewServiceHelper.CheckHasRelateInnerBill(base.Context, Convert.ToInt64(num)))
					{
						this.ListView.ShowErrMessage(ResManager.LoadKDString("当前单据有关联的组织间内部交易单据，不允许进行拆分操作！", "004023000021641", 5, new object[0]), "", 0);
						e.Cancel = true;
					}
					object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, selectedRowsInfo[0].MainOrgId, "PUR_SystemParameter", "PISpliteAfterHSClose", false);
					if (systemProfile == null || !Convert.ToBoolean(systemProfile))
					{
						object hslastestCloseDate2 = CommonServiceHelper.GetHSLastestCloseDate(base.Context, selectedRowsInfo[0].MainOrgId);
						if (hslastestCloseDate2 != null)
						{
							DateTime t3 = Convert.ToDateTime(selectedRowsInfo[0].DataRow["FDate"]);
							DateTime t4 = Convert.ToDateTime(hslastestCloseDate2);
							if (t3 <= t4)
							{
								this.ListView.ShowErrMessage(ResManager.LoadKDString("当前拆单单据的入库日期小于等于本期核算关账日期，不允许进行拆分操作！", "004023030034658", 5, new object[0]), "", 0);
								e.Cancel = true;
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x000379CC File Offset: 0x00035BCC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "TAKEREFERENCEPRICE"))
				{
					return;
				}
				string arg = ResManager.LoadKDString("以下单据取参考价不成功：\r\n", "004023030004273", 5, new object[0]);
				string value = string.Empty;
				DynamicObject[] array = null;
				StringBuilder stringBuilder = new StringBuilder();
				using (TakeReferencePrice takeReferencePrice = new TakeReferencePrice(this.View.Context))
				{
					List<string> list = new List<string>();
					List<long> list2 = new List<long>();
					List<long> list3 = new List<long>();
					List<long> list4 = new List<long>();
					List<long> list5 = new List<long>();
					List<long> list6 = new List<long>();
					List<long> list7 = new List<long>();
					if (this.ListView.SelectedRowsInfo.Count == 0)
					{
						this.ListView.ShowMessage(ResManager.LoadKDString("请至少选择一行分录！", "004023030004276", 5, new object[0]), 0);
						return;
					}
					for (int i = 0; i < this.ListView.SelectedRowsInfo.Count; i++)
					{
						if (!string.IsNullOrWhiteSpace(this.ListView.SelectedRowsInfo[i].EntryPrimaryKeyValue))
						{
							list2.Add(Convert.ToInt64(this.ListView.SelectedRowsInfo[i].EntryPrimaryKeyValue));
						}
						list.Add(this.ListView.SelectedRowsInfo[i].PrimaryKeyValue);
					}
					DynamicObject[] dynamicObjectCollection = takeReferencePrice.GetDynamicObjectCollection(this.View.Context, list.ToArray(), "STK_InStock");
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						string text = Convert.ToString(dynamicObject["BillTypeId_Id"]);
						if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) || StringUtils.EqualsIgnoreCase(text, "5b91410d323043f3b4f3a7079aad3c68"))
						{
							long num = Convert.ToInt64(dynamicObject["StockOrgId_Id"]);
							object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "PUR_SystemParameter", "ReferencePriceSource", "1");
							string text2 = Convert.ToString(systemProfile);
							if (!(text2 == "1"))
							{
								list7.Add(num);
								DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["InStockFin"] as DynamicObjectCollection;
								if (dynamicObjectCollection2 != null && dynamicObjectCollection2.Count > 0)
								{
									list4.Add(Convert.ToInt64(dynamicObjectCollection2[0]["SettleCurrId_ID"]));
								}
								DynamicObjectCollection dynamicObjectCollection3 = dynamicObject["InStockEntry"] as DynamicObjectCollection;
								if (dynamicObject["DocumentStatus"].ToString() == "A" || dynamicObject["DocumentStatus"].ToString() == "D")
								{
									foreach (DynamicObject dynamicObject2 in dynamicObjectCollection3)
									{
										list5.Add(Convert.ToInt64(dynamicObject2["UnitId_Id"]));
										list6.Add(Convert.ToInt64(dynamicObject2["AuxPropId_Id"]));
										list3.Add(Convert.ToInt64(dynamicObject2["MaterialId_Id"]));
									}
									DynamicObjectCollection referencePriceLists = takeReferencePrice.GetReferencePriceLists(this.View.Context, list3, list4, list7, text2);
									value = takeReferencePrice.AddChangeList(dynamicObject, "STK_InStock", "InStockFin", "InStockEntry", referencePriceLists, list2);
									stringBuilder.AppendLine(value);
								}
							}
						}
					}
					if (takeReferencePrice.UpdateBillObj != null && takeReferencePrice.UpdateBillObj.Length > 0)
					{
						array = takeReferencePrice.UpdateBillObj;
						PurchaseServiceHelper.TakeReferencePrice(base.Context, null, array, "STK_InStock");
					}
				}
				if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()))
				{
					this.ListView.ShowMessage(string.Format("{0}{1}", arg, stringBuilder.ToString()), 0);
					return;
				}
				if (array != null && array.Length > 0 && string.IsNullOrWhiteSpace(stringBuilder.ToString()))
				{
					this.ListView.ShowMessage(ResManager.LoadKDString("取参考价成功", "004023030004279", 5, new object[0]), 0);
					this.ListView.Refresh();
				}
			}
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00037E20 File Offset: 0x00036020
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation == 12 && !ListUtils.IsEmpty<ConvertBillElement>(list))
			{
				list = (from w in list
				where !StringUtils.EqualsIgnoreCase(w.FormID, "QT_LotSNRelation")
				select w).ToList<ConvertBillElement>();
			}
			e.Bills = list;
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00037EA4 File Offset: 0x000360A4
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (e.FieldKey.ToUpperInvariant() == "FSALOUTSTOCKBILLNO")
			{
				if (e.Row < 0)
				{
					return;
				}
				ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == e.Row);
				if (listSelectedRow == null || listSelectedRow.EntryEntityKey != "FInStockEntry" || string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
				{
					return;
				}
				long entryIdData = SaleServiceHelper2.GetEntryIdData(base.Context, Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue), "FSALOUTSTOCKENTRYID", "T_STK_INSTOCKENTRY");
				if (entryIdData > 0L)
				{
					PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
					{
						Id = "SAL_OUTSTOCK"
					}, "6e44119a58cb4a8e86f6c385e14a17ad");
					if (!permissionAuthResult.Passed)
					{
						this.View.ShowMessage(ResManager.LoadKDString("你没有当前单据查看权限，请设置！", "004104000013505", 5, new object[0]), 0);
						return;
					}
					ListShowParameter listShowParameter = new ListShowParameter();
					listShowParameter.ListFilterParameter = new ListRegularFilterParameter
					{
						Filter = string.Format(" t2.FENTRYID={0} ", entryIdData)
					};
					listShowParameter.FormId = "SAL_OUTSTOCK";
					listShowParameter.IsLookUp = false;
					listShowParameter.IsIsolationOrg = false;
					this.View.ShowForm(listShowParameter);
				}
				e.Cancel = true;
			}
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00038024 File Offset: 0x00036224
		public override void BeforeSaveImportData(BeforeSaveImportDataArgs e)
		{
			if (e.DataEntities == null || e.DataEntities.Length <= 0)
			{
				return;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "STK_InStock", true);
			IBillView billView = (IBillView)SCMFormUtils.GetBillView(base.Context, formMetadata, null);
			DynamicObject[] array = new DynamicObject[e.DataEntities.Length];
			e.DataEntities.CopyTo(array, 0);
			foreach (DynamicObject dataObject in array)
			{
				if (billView != null)
				{
					billView.Model.DataObject = dataObject;
					billView.UpdateView();
					bool flag = true;
					SCMCommon.SetAllotHolisticDiscountOperation(null, billView, ref flag);
				}
			}
			if (billView != null)
			{
				billView.Close();
			}
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x000380F4 File Offset: 0x000362F4
		private void ProcessLotSNR()
		{
			ListSelectedRow listSelectedRow = this.ListView.SelectedRowsInfo.FirstOrDefault<ListSelectedRow>();
			if (ObjectUtils.IsNullOrEmpty(listSelectedRow))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选择需要查看的数据！", "004023030002128", 5, new object[0]), "", 0);
				return;
			}
			long entryId = Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue);
			string primaryKeyValue = listSelectedRow.PrimaryKeyValue;
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, new List<string>
			{
				primaryKeyValue
			}.ToArray(), this.ListView.BillBusinessInfo.GetDynamicObjectType());
			if (ListUtils.IsEmpty<DynamicObject>(array))
			{
				return;
			}
			DynamicObject dynamicObject = (from w in (DynamicObjectCollection)array.FirstOrDefault<DynamicObject>()["InStockEntry"]
			where Convert.ToInt64(w["Id"]) == entryId
			select w).FirstOrDefault<DynamicObject>();
			int num = Convert.ToInt32(dynamicObject["Seq"]);
			ListSelectedRow listSelectedRow2 = new ListSelectedRow(primaryKeyValue, entryId.ToString(), num, "STK_InStock");
			Tuple<List<DynamicObject>, long> lotSNRBySrcBillInfo = MFGServiceHelperForSCM.GetLotSNRBySrcBillInfo(base.Context, "STK_InStock", entryId, listSelectedRow2);
			BillShowParameter billShowParameter = new BillShowParameter();
			if (lotSNRBySrcBillInfo.Item2 > 0L)
			{
				billShowParameter.PKey = lotSNRBySrcBillInfo.Item2.ToString();
				billShowParameter.Status = 2;
			}
			else if (!ListUtils.IsEmpty<DynamicObject>(lotSNRBySrcBillInfo.Item1))
			{
				billShowParameter.CustomComplexParams.Add("LotSNR", lotSNRBySrcBillInfo.Item1);
				billShowParameter.Status = 0;
			}
			billShowParameter.FormId = "QT_LotSNRelation";
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			billShowParameter.OpenStyle.ShowType = 4;
			billShowParameter.CustomParams.Add("showbeforesave", "1");
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x000382DC File Offset: 0x000364DC
		private void ShowBackFlush()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "SUB_BackFlush"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有委外倒冲领料工作台的查看权限！", "004023030002125", 5, new object[0]), "", 0);
				return;
			}
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
			{
				if (!string.IsNullOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
				{
					list.Add(listSelectedRow.EntryPrimaryKeyValue);
				}
				if (!string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
				{
					list2.Add(listSelectedRow.PrimaryKeyValue);
				}
			}
			if (list.Count <= 0 && list2.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选择需要查看的数据！", "004023030002128", 5, new object[0]), "", 0);
				return;
			}
			string text = (list.Count > 0) ? ("e," + string.Join(",", list)) : ("b," + string.Join(",", list2));
			SUBBackFlushSearchParam subbackFlushSearchParam = new SUBBackFlushSearchParam();
			subbackFlushSearchParam.StartTime = KDTimeZone.MinSystemDateTime;
			subbackFlushSearchParam.EndTime = TimeServiceHelper.GetSystemDateTime(this.View.Context);
			subbackFlushSearchParam.BillType = "STK_InStock";
			subbackFlushSearchParam.Option = OperateOption.Create();
			subbackFlushSearchParam.Option.SetVariableValue("ids", text);
			List<DynamicObject> subBackFlushItems = MFGServiceHelperForSCM.GetSubBackFlushItems(base.Context, subbackFlushSearchParam);
			if (subBackFlushItems.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当前选择行没有对应的倒冲物料！", "004023030002131", 5, new object[0]), "", 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "SUB_BackFlush";
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomParams.Add("ids", text);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00038514 File Offset: 0x00036714
		private string GetSelectedEntryParentId()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("0");
			for (int i = 0; i < this.ListView.SelectedRowsInfo.Count; i++)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(this.ListView.SelectedRowsInfo[i].PrimaryKeyValue);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x040001BB RID: 443
		private string callSys = "";
	}
}

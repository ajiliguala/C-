using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Metadata.FormValidationElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Business.PlugIn.Common;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.SAL;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.Core.SAL;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008F RID: 143
	public class StockTransferDirectEdit : AbstractBillPlugIn
	{
		// Token: 0x06000735 RID: 1845 RVA: 0x0005B240 File Offset: 0x00059440
		public override void OnInitialize(InitializeEventArgs e)
		{
			this._baseDataOrgCtl = CustMatMapping.GetSalBaseDataCtrolType(base.View.Context);
			base.View.RuleContainer.AddPluginRule<STK_TransferDirect>("FBillHead", 1, new Action<STK_TransferDirect>(this.ChangeStockerData), new string[]
			{
				"FStockerId"
			});
			base.View.RuleContainer.AddPluginRule<STK_TransferDirect>("FBillHead", 3, new Action<STK_TransferDirect>(this.LockAndUnLockParentWhenInitAndValueChanged), new string[]
			{
				"FParentRowId"
			});
			base.View.RuleContainer.AddPluginRule<STK_TransferDirect>("FBillHead", 16, new Action<STK_TransferDirect>(this.LockAndUnLockParentWhenItemRemoved), new string[]
			{
				"FParentRowId"
			});
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x0005B328 File Offset: 0x00059528
		public override void DataChanged(DataChangedEventArgs e)
		{
			string text = "";
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FMATERIALID":
			{
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				string text2 = Convert.ToString(base.View.Model.GetValue("FBizType"));
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L)
				{
					if (this.defaultStock != 0L && StringUtils.EqualsIgnoreCase(text2, "VMI"))
					{
						string text3 = Convert.ToString(base.View.Model.GetValue("FTransferDirect"));
						if (text3.Equals("GENERAL"))
						{
							base.View.Model.SetValue("FSrcStockId", this.defaultStock, e.Row);
						}
					}
					text = dynamicObject2["Number"].ToString();
				}
				if (this.para_UseCustMatMapping)
				{
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					string text4 = (dynamicObject3 != null) ? Convert.ToString(dynamicObject3["Id"]) : "";
					this.bQueryStockReturn = (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id));
					if (((this.bQueryStockReturn || this.isPickReturn) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4)) || (!this.bQueryStockReturn && !this.associatedCopyEntryRow && !this.isPickReturn))
					{
						string text5 = Convert.ToString(base.View.Model.GetValue("FKeeperTypeId", e.Row));
						long num3 = 0L;
						if (StringUtils.EqualsIgnoreCase(text5, "BD_Customer"))
						{
							DynamicObject dynamicObject4 = base.View.Model.GetValue("FKeeperId", e.Row) as DynamicObject;
							num3 = ((dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"].ToString()));
						}
						DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOutOrgId", e.Row) as DynamicObject;
						DynamicObject dynamicObject6 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
						long num4 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"].ToString());
						long num5 = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"].ToString());
						CustMatMapping.SetRelativeCodeByMaterialId(this, "FCustMatId", num5, num3, num4, e.Row);
					}
				}
				this.Model.SetItemValueByNumber("FDestMaterialID", text, e.Row);
				base.View.InvokeFieldUpdateService("FDestMaterialID", e.Row);
				base.View.UpdateView("FDestMaterialID", e.Row);
				if (!this.expandSuiteBySonMaterial)
				{
					this.SynOwnerType("FOwnerTypeOutIdHead", "FOwnerTypeOutId", e.Row);
					this.SynOwnerType("FOwnerTypeIdHead", "FOwnerTypeId", e.Row);
					DynamicObject dynamicObject7 = base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject;
					if (dynamicObject7 != null)
					{
						base.View.Model.SetValue("FOwnerOutId", Convert.ToInt64(dynamicObject7["Id"]), e.Row);
					}
					dynamicObject7 = (base.View.Model.GetValue("FOwnerIdHead") as DynamicObject);
					if (dynamicObject7 != null)
					{
						base.View.Model.SetValue("FOwnerId", Convert.ToInt64(dynamicObject7["Id"]), e.Row);
					}
				}
				object value = base.View.Model.GetValue("FRowId", e.Row);
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
				{
					string text6 = SequentialGuid.NewGuid().ToString();
					base.View.Model.SetValue("FRowId", text6, e.Row);
				}
				if (this.para_UseSuiteProduct && !this.expandSuiteBySonMaterial)
				{
					SCMCommon.SetRightProductTypeWhenChangeMat(this, e.Field.Key, e.Row);
					this.SetParentMatForSonEntry(e.Row, true);
				}
				break;
			}
			case "FKEEPERID":
				if (this.para_UseCustMatMapping)
				{
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					string text4 = (dynamicObject3 != null) ? Convert.ToString(dynamicObject3["Id"]) : "";
					this.bQueryStockReturn = (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id));
					if ((this.bQueryStockReturn && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4)) || (!this.bQueryStockReturn && !this.associatedCopyEntryRow))
					{
						string text5 = Convert.ToString(base.View.Model.GetValue("FKeeperTypeId", e.Row));
						long num3 = 0L;
						if (StringUtils.EqualsIgnoreCase(text5, "BD_Customer"))
						{
							DynamicObject dynamicObject4 = base.View.Model.GetValue("FKeeperId", e.Row) as DynamicObject;
							num3 = ((dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]));
						}
						DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOutOrgId", e.Row) as DynamicObject;
						DynamicObject dynamicObject6 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
						long num4 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"].ToString());
						long num5 = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"].ToString());
						List<KeyValuePair<int, long>> list = new List<KeyValuePair<int, long>>
						{
							new KeyValuePair<int, long>(e.Row, num5)
						};
						if (num4 == 0L || list == null || list.Count<KeyValuePair<int, long>>() == 0)
						{
							return;
						}
						Dictionary<int, CustomerMaterialResult> relativeCodeByCust = CommonServiceHelper.GetRelativeCodeByCust(base.Context, list, num3, num4);
						base.View.Model.BeginIniti();
						CustomerMaterialResult customerMaterialResult = relativeCodeByCust[e.Row];
						if (customerMaterialResult == null)
						{
							base.View.Model.SetValue("FCustMatId", null, e.Row);
						}
						else
						{
							base.View.Model.SetValue("FCustMatId", customerMaterialResult.Fid, e.Row);
						}
						base.View.Model.EndIniti();
						base.View.UpdateView("FCustMatId", e.Row);
						base.View.UpdateView("FCustMatName", e.Row);
					}
				}
				break;
			case "FCUSTMATID":
				if (!this.associatedCopyEntryRow)
				{
					CustomerMaterialMappingArgs customerMaterialMappingArgs = new CustomerMaterialMappingArgs();
					DynamicObject dynamicObject8 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					customerMaterialMappingArgs.CustMatId = ((dynamicObject8 == null) ? "" : dynamicObject8["Id"].ToString());
					DynamicObject dynamicObject9 = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
					customerMaterialMappingArgs.MainOrgId = ((dynamicObject9 == null) ? 0L : Convert.ToInt64(dynamicObject9["Id"]));
					customerMaterialMappingArgs.NeedOrgCtrl = this._baseDataOrgCtl["BD_MATERIAL"];
					customerMaterialMappingArgs.MaterialIdKey = "FMaterialId";
					customerMaterialMappingArgs.AuxpropIdKey = "FAuxpropId";
					customerMaterialMappingArgs.Row = e.Row;
					DynamicObject dynamicObject10 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
					if (this.para_UseSuiteProduct && dynamicObject10 == null)
					{
						bool flag = SaleServiceHelper2.CheckMaterialIdIsSuiteByCustMatId(base.Context, customerMaterialMappingArgs);
						if (flag)
						{
							base.View.Model.SetValue("FRowType", "Parent", e.Row);
						}
					}
					Common.SetMaterialIdAndAuxpropIdByCustMatId(this, customerMaterialMappingArgs);
				}
				break;
			case "FSTOCKOUTORGID":
			{
				this.SetDefLocalCurrencyAndExchangeType();
				this.SetExchangeRate();
				base.View.Model.SetValue("FStockerId", 0);
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOutOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
				}
				break;
			}
			case "FSRCSTOCKID":
			{
				DynamicObject dynamicObject11 = base.View.Model.GetValue("FSrcStockID", e.Row) as DynamicObject;
				SCMCommon.TakeDefaultStockStatusOther(this, "FSrcStockStatusId", dynamicObject11, e.Row, "'0','8'");
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT")), "RETURN") && ObjectUtils.IsNullOrEmptyOrWhiteSpace(Convert.ToString(base.View.Model.GetValue("FSrcBillTypeId", e.Row))))
				{
					long num6 = (dynamicObject11 == null) ? 0L : Convert.ToInt64(dynamicObject11["CustomerId_Id"]);
					base.View.Model.SetValue("FKeeperOutId", num6, e.Row);
				}
				break;
			}
			case "FDESTSTOCKID":
			{
				DynamicObject dynamicObject12 = base.View.Model.GetValue("FDestStockID", e.Row) as DynamicObject;
				SCMCommon.TakeDefaultStockStatusOther(this, "FDestStockStatusID", dynamicObject12, e.Row, "'0','8'");
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT") && !StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT")), "RETURN") && ObjectUtils.IsNullOrEmptyOrWhiteSpace(Convert.ToString(base.View.Model.GetValue("FSrcBillTypeId", e.Row))))
				{
					long num7 = (dynamicObject12 == null) ? 0L : Convert.ToInt64(dynamicObject12["CustomerId_Id"]);
					base.View.Model.SetValue("FKeeperId", num7, e.Row);
				}
				break;
			}
			case "FTRANSFERDIRECT":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT"))
				{
					string text7 = Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT"));
					bool flag2;
					if (StringUtils.EqualsIgnoreCase(text7, "RETURN"))
					{
						base.View.StyleManager.SetEnabled("FKeeperTypeOutId", "", false);
						base.View.StyleManager.SetEnabled("FKeeperTypeId", "", true);
						flag2 = StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FKeeperTypeId", 0)), "BD_Customer");
					}
					else
					{
						base.View.StyleManager.SetEnabled("FKeeperTypeOutId", "", true);
						base.View.StyleManager.SetEnabled("FKeeperTypeId", "", false);
						flag2 = StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FKeeperTypeOutId", 0)), "BD_Customer");
					}
					if (flag2)
					{
						int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
						DynamicObjectCollection dyEntity = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
						for (int i = 0; i < entryRowCount; i++)
						{
							this.SwapFields(dyEntity, i);
						}
						base.View.UpdateView("FPDSrcStockLocId");
						base.View.UpdateView("FPDDestStockLocId");
					}
				}
				this.SetCustAndSupplierValue();
				break;
			case "FDESTMATERIALID":
				if (!this.isDoFillMaterial)
				{
					this.UpdateDestBom(e.Row);
				}
				break;
			case "FBOMID":
				this.UpdateDestBom(e.Row);
				break;
			case "FLOT":
				this.SyncEntryLot(e.Row);
				break;
			case "FSTOCKORGID":
			{
				int entryRowCount2 = base.View.Model.GetEntryRowCount("FBillEntry");
				string value2 = BillUtils.GetValue<string>(this.Model, "FOwnerTypeIdHead", -1, "", null);
				long num8 = 0L;
				long num9 = 0L;
				if (e.NewValue != null)
				{
					num8 = Convert.ToInt64(e.NewValue);
				}
				DynamicObject dynamicObject13 = base.View.Model.GetValue("FOwnerIdHead", -1) as DynamicObject;
				if (dynamicObject13 != null)
				{
					num9 = Convert.ToInt64(dynamicObject13["Id"]);
				}
				if (value2.Equals("BD_OwnerOrg") && num8 != num9)
				{
					this.Model.SetValue("FOwnerIdHead", num8, -1);
					base.View.InvokeFieldUpdateService("FOwnerIdHead", -1);
				}
				else if (!value2.Equals("BD_OwnerOrg"))
				{
					this.Model.SetValue("FOwnerIdHead", 0, -1);
					base.View.InvokeFieldUpdateService("FOwnerIdHead", -1);
				}
				for (int j = 0; j < entryRowCount2; j++)
				{
					text = "";
					if (e.NewValue == null)
					{
						this.Model.SetValue("FDestMaterialId", null, j);
						this.Model.SetValue("FDestBomId", null, j);
						this.Model.SetValue("FDestLot", null, j);
					}
					else
					{
						dynamicObject13 = (base.View.Model.GetValue("FMATERIALID", j) as DynamicObject);
						if (dynamicObject13 != null)
						{
							text = dynamicObject13["Number"].ToString();
						}
						this.Model.SetItemValueByNumber("FDestMaterialId", text, j);
						base.View.InvokeFieldUpdateService("FDestMaterialId", j);
						this.SyncEntryLot(j);
						this.UpdateDestBom(j);
					}
				}
				break;
			}
			case "FDATE":
				if (base.Context.ClientType != 32)
				{
					this.CheckAccountDate();
				}
				break;
			case "FOWNERTYPEOUTIDHEAD":
				Common.SynOwnerType(this, "FOwnerTypeOutIdHead", "FOwnerTypeOutId");
				break;
			case "FOWNEROUTIDHEAD":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "VMI") && Convert.ToString(base.View.Model.GetValue("FTransferDirect")).Equals("GENERAL"))
				{
					this.defaultStock = Common.GetDefaultVMIStock(this, "FOWNEROUTIDHEAD", "FSrcStockId", "0,7", true);
				}
				else
				{
					string text8 = base.View.Model.GetValue("FTransferBizType").ToString();
					if (StringUtils.EqualsIgnoreCase(text8, "OverOrgTransfer"))
					{
						DynamicObject dynamicObject14 = base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject;
						if (ObjectUtils.IsNullOrEmpty(dynamicObject14))
						{
							base.View.Model.SetValue("FOwnerIdHead", null);
						}
						else
						{
							string text9 = Convert.ToString(dynamicObject14["Number"]);
							base.View.Model.SetItemValueByNumber("FOwnerIdHead", text9, 0);
						}
					}
				}
				this.SetCustAndSupplierValue();
				this.SynHeadToEntry("FOwnerOutIdHead", "FOwnerOutId");
				this.SetDefLocalCurrencyAndExchangeType();
				this.SetExchangeRate();
				break;
			case "FOWNERTYPEOUTID":
				this.SetParentFieldValueToSon(e.Field.Key, e.NewValue, e.Row);
				break;
			case "FOWNEROUTID":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "VMI") && Convert.ToString(base.View.Model.GetValue("FTransferDirect")).Equals("GENERAL"))
				{
					this.defaultStock = Common.GetDefaultVMIStockByRow(this, "FOWNEROUTID", "FSrcStockId", "0,7", e.Row);
				}
				this.SetParentFieldValueToSon(e.Field.Key, e.NewValue, e.Row);
				break;
			case "FOWNERTYPEIDHEAD":
				Common.SynOwnerType(this, "FOwnerTypeIdHead", "FOwnerTypeId");
				break;
			case "FOWNERIDHEAD":
				this.SetCustAndSupplierValue();
				this.SynHeadToEntry("FOwnerIdHead", "FOwnerId");
				break;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				break;
			}
			case "FBIZTYPE":
				this.SetComValue();
				break;
			case "FBASEQTY":
				if (!this.changeStockBaseQtyByMaterial)
				{
					this.ChangeParentQtyForAllSonQty(e);
				}
				else
				{
					this.changeStockBaseQtyByMaterial = false;
				}
				break;
			case "FROWTYPE":
			{
				DynamicObject dynamicObject15 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
				long num10 = (dynamicObject15 != null) ? Convert.ToInt64(dynamicObject15["Id"]) : 0L;
				string text10 = Convert.ToString(e.OldValue);
				string text11 = Convert.ToString(e.NewValue);
				if (num10 > 0L && StringUtils.EqualsIgnoreCase(text10, "Parent") && !StringUtils.EqualsIgnoreCase(text11, "Parent"))
				{
					base.View.Model.SetValue("FMaterialId", 0, e.Row);
					base.View.InvokeFieldUpdateService("FMaterialId", e.Row);
				}
				if ((StringUtils.EqualsIgnoreCase(text11, "Service") || StringUtils.EqualsIgnoreCase(text11, "Parent")) && dynamicObject15 != null && num10 > 0L)
				{
					bool flag3 = false;
					bool flag4 = false;
					DynamicObjectCollection dynamicObjectCollection = dynamicObject15["MaterialBase"] as DynamicObjectCollection;
					if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
					{
						flag3 = StringUtils.EqualsIgnoreCase(Convert.ToString(dynamicObjectCollection[0]["ErpClsID"]), "6");
						flag4 = StringUtils.EqualsIgnoreCase(Convert.ToString(dynamicObjectCollection[0]["Suite"]), "1");
					}
					if ((StringUtils.EqualsIgnoreCase(text11, "Service") && dynamicObjectCollection != null && !flag3) || (StringUtils.EqualsIgnoreCase(text11, "Parent") && dynamicObjectCollection != null && !flag4))
					{
						base.View.Model.SetValue("FMaterialId", 0, e.Row);
						base.View.InvokeFieldUpdateService("FMaterialId", e.Row);
					}
				}
				if (!StringUtils.EqualsIgnoreCase(text10, "Parent") && StringUtils.EqualsIgnoreCase(text11, "Son"))
				{
					this.SetParentMatForSonEntry(e.Row, false);
				}
				if (!this.expandSuiteBySonMaterial && StringUtils.EqualsIgnoreCase(text11, "Son"))
				{
					List<string> list2 = new List<string>
					{
						"FOwnerTypeOutID",
						"FOwnerOutID",
						"FOwnerTypeID",
						"FOwnerID"
					};
					object parentRowId = base.View.Model.GetValue("FParentRowId", e.Row);
					Entity entity = base.View.Model.BusinessInfo.GetEntity("FBillEntry");
					DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
					if (entityDataObject != null)
					{
						DynamicObject dynamicObject16 = entityDataObject.ToList<DynamicObject>().SingleOrDefault((DynamicObject n) => StringUtils.EqualsIgnoreCase(Convert.ToString(n["RowId"]), Convert.ToString(parentRowId)));
						if (dynamicObject16 != null)
						{
							foreach (string text12 in list2)
							{
								Field field = base.View.Model.BusinessInfo.GetField(text12);
								if (field != null)
								{
									object obj = dynamicObject16[field.PropertyName];
									base.View.Model.SetValue(text12, obj, e.Row);
								}
							}
						}
					}
				}
				if (StringUtils.EqualsIgnoreCase(text10, "Son") && !StringUtils.EqualsIgnoreCase(text11, "Son"))
				{
					base.View.Model.SetValue("FParentMatId", 0, e.Row);
					base.View.Model.SetValue("FParentRowId", "", e.Row);
				}
				break;
			}
			case "FSETTLECURRID":
				this.SetExchangeRate();
				break;
			}
			base.DataChanged(e);
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x0005CA1C File Offset: 0x0005AC1C
		private void SyncEntryLot(int index)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FDestMaterialId", index) as DynamicObject;
			if (dynamicObject != null)
			{
				object obj = dynamicObject["MaterialStock"];
				DynamicObject dynamicObject2 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
				if (dynamicObject2 != null && Convert.ToBoolean(dynamicObject2["IsBatchManage"]))
				{
					object value = this.Model.GetValue("FLot", index);
					this.Model.SetValue("FDestLot", value, index);
				}
				else
				{
					this.Model.SetValue("FDestLot", null, index);
				}
			}
			else
			{
				this.Model.SetValue("FDestLot", null, index);
			}
			base.View.InvokeFieldUpdateService("FDestLot", index);
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x0005CAF4 File Offset: 0x0005ACF4
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation == 13)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				if (list != null && list.Count > 0)
				{
					if (base.Context.IsMultiOrg)
					{
						e.Bills = (from c in list
						where !c.FormID.Equals("DRP_NeedApplication", StringComparison.OrdinalIgnoreCase)
						select c).ToList<ConvertBillElement>();
					}
					long value = BillUtils.GetValue<long>(base.View.Model, "FStockOutOrgId", -1, 0L, null);
					if (value > 0L && Common.HaveBOMViewPermission(base.Context, value))
					{
						Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
					}
				}
				return;
			}
			if (convertOperation != 26)
			{
				return;
			}
			List<ConvertBillElement> list2 = null;
			if (e.Bills != null)
			{
				list2 = (e.Bills as List<ConvertBillElement>);
			}
			if (e.Bills == null)
			{
				list2 = new List<ConvertBillElement>();
			}
			ConvertBillElement convertBillElement = new ConvertBillElement();
			convertBillElement.FormID = "SUB_SUBREQORDER";
			ConvertBillElement convertBillElement2 = new ConvertBillElement();
			convertBillElement2.FormID = "PRD_MO";
			list2.Add(convertBillElement);
			list2.Add(convertBillElement2);
			e.AddReplaceRelation("SUB_PPBOM", convertBillElement.FormID);
			e.AddReplaceRelation("PRD_PPBOM", convertBillElement2.FormID);
			e.Bills = list2;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x0005CC38 File Offset: 0x0005AE38
		public override void OnShowTrackResult(ShowTrackResultEventArgs e)
		{
			base.OnShowTrackResult(e);
			FormOperationEnum trackOperation = e.TrackOperation;
			if (trackOperation != 26)
			{
				return;
			}
			if (e.TrackResult != null)
			{
				BillNode billNode = e.TrackResult as BillNode;
				if (!billNode.FormKey.Equals(e.TargetFormKey, StringComparison.OrdinalIgnoreCase))
				{
					e.TrackResult = this.GetReplaceTrackResult(billNode, e.TargetFormKey);
				}
			}
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x0005CC94 File Offset: 0x0005AE94
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x0005CCAC File Offset: 0x0005AEAC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FSTOCKOUTORGID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
				break;
			}
			case "FOWNERID":
			case "FOWNEROUTID":
			case "FOWNERIDHEAD":
			case "FOWNEROUTIDHEAD":
			{
				string text;
				if (this.GetOwnerFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrEmpty(e.Filter) ? text : (e.Filter + "AND" + text));
					return;
				}
				break;
			}
			case "FMATERIALID":
			case "FSRCSTOCKID":
			case "FDESTSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			case "FSALEUNITID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
				break;
			}
			case "FSRCSTOCKSTATUSID":
			case "FDESTSTOCKSTATUSID":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x0005CEF4 File Offset: 0x0005B0F4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKOUTORGID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FOWNERID":
			case "FOWNEROUTID":
			case "FOWNERIDHEAD":
			case "FSTOCKORGID":
			case "FOWNEROUTIDHEAD":
			{
				string lotF8InvFilter;
				if (this.GetOwnerFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + "AND" + lotF8InvFilter));
					return;
				}
				break;
			}
			case "FMATERIALID":
			case "FSRCSTOCKID":
			case "FDESTSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			case "FSALEUNITID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FSRCSTOCKSTATUSID":
			case "FDESTSTOCKSTATUSID":
			{
				string lotF8InvFilter;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FLOT":
			{
				string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
				{
					MaterialFieldKey = "FMaterialId",
					StockOrgFieldKey = "FStockOutOrgId",
					OwnerTypeFieldKey = "FOwnerTypeOutId",
					OwnerFieldKey = "FOwnerOutId",
					KeeperTypeFieldKey = "FKeeperTypeOutId",
					KeeperFieldKey = "FKeeperOutId",
					AuxpropFieldKey = "FAuxPropId",
					BomFieldKey = "FBomId",
					StockFieldKey = "FSrcStockId",
					StockLocFieldKey = "FSrcStockLocId",
					StockStatusFieldKey = "FSrcStockStatusId",
					MtoFieldKey = "FMtoNo",
					ProjectFieldKey = "FProjectNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
					listFilterParameter4.Filter = listFilterParameter4.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FCUSTMATID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter5 = e.ListFilterParameter;
					listFilterParameter5.Filter = listFilterParameter5.Filter + " AND " + lotF8InvFilter;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x0005D2EC File Offset: 0x0005B4EC
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x0005D350 File Offset: 0x0005B550
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x0005D388 File Offset: 0x0005B588
		private string GetStockFilter(string entryKey, string stockIDKey, string[] msg)
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(entryKey);
			DynamicObject dynamicObject = base.View.Model.GetValue(stockIDKey, entryCurrentRowIndex) as DynamicObject;
			if (dynamicObject == null)
			{
				base.View.ShowMessage(msg[0], 0);
				return "";
			}
			if (!Convert.ToBoolean(dynamicObject["IsOpenLocation"]))
			{
				base.View.ShowMessage(msg[1], 0);
				return "";
			}
			return " FSTOCKID = " + dynamicObject["ID"];
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x0005D410 File Offset: 0x0005B610
		private void GetBindSerials()
		{
			if (this.isFirstOpen)
			{
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
				List<long> list = new List<long>();
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0 && Convert.ToInt64(dynamicObjectCollection[0]["Id"]) > 0L)
				{
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						long value = Convert.ToInt64(dynamicObject["Id"]);
						long num = (dynamicObject["QmEntryID"] == null) ? 0L : Convert.ToInt64(dynamicObject["QmEntryID"]);
						if (num != 0L)
						{
							string key = Convert.ToString(num) + "|" + Convert.ToString(value);
							DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["STK_StkTransInSerial"] as DynamicObjectCollection;
							foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
							{
								long num2 = Convert.ToInt64(dynamicObject2["SerialId_Id"]);
								if (num2 > 0L && !list.Contains(num2))
								{
									list.Add(num2);
								}
							}
							if (!this.bindSerialIds.ContainsKey(key) && list.Count > 0)
							{
								this.bindSerialIds.Add(key, list);
								list = new List<long>();
							}
						}
					}
					this.isFirstOpen = false;
				}
			}
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0005D5DC File Offset: 0x0005B7DC
		public override void AfterBindData(EventArgs e)
		{
			this.GetBindSerials();
			this.SetComValue();
			this.SetCustAndSupplierValue();
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT"))
			{
				string text = Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT"));
				if (StringUtils.EqualsIgnoreCase(text, "RETURN"))
				{
					base.View.StyleManager.SetEnabled("FKeeperTypeOutId", "", false);
				}
				else
				{
					base.View.StyleManager.SetEnabled("FKeeperTypeId", "", false);
				}
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				long num = Convert.ToInt64(dynamicObject["Id"]);
				object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "SAL_SystemParameter", "UseCustMatMapping", false);
				this.para_UseCustMatMapping = (systemProfile != null && Convert.ToBoolean(systemProfile));
			}
			base.View.GetControl("FCustMatId").Visible = this.para_UseCustMatMapping;
			base.View.GetControl("FCustMatName").Visible = this.para_UseCustMatMapping;
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "VMI") && Convert.ToString(base.View.Model.GetValue("FTransferDirect")).Equals("GENERAL"))
			{
				this.defaultStock = Common.GetDefaultVMIStock(this, "FOWNEROUTIDHEAD", "FSrcStockId", "0,7", false);
			}
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				this.SetOwnerTypeHead();
			}
			this.Model.DataChanged = false;
			if (base.Context.ClientType == 16)
			{
				base.View.GetControl("FBillNo").SetFocus();
			}
			this.InitializeSuitParaData();
			this.SetSuiteProductVisible();
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0005D7F8 File Offset: 0x0005B9F8
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
			{
				List<string> list = new List<string>();
				SelBomBillParam bomExpandBillFieldValue = Common.GetBomExpandBillFieldValue(base.View, "FStockOutOrgId", "FOwnerTypeOutIdHead", "FOwnerOutIdHead");
				if (Common.ValidateBomExpandBillFieldValue(base.View, bomExpandBillFieldValue, list))
				{
					base.View.Session["SelInStockBillParam"] = bomExpandBillFieldValue;
					Common.SetBomExpandConvertRuleinfo(base.Context, base.View, e);
					return;
				}
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("【{0}】 字段为选单必录项！", "004023030004312", 5, new object[0]), string.Join(ResManager.LoadKDString("】,【", "004023030004315", 5, new object[0]), list)), "", 0);
			}
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x0005D940 File Offset: 0x0005BB40
		public override void OnChangeConvertRuleEnumList(ChangeConvertRuleEnumListEventArgs e)
		{
			base.OnChangeConvertRuleEnumList(e);
			ConvertRuleElement convertRuleElement = e.Convertrules.FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement != null && convertRuleElement.SourceFormId.Equals("STK_TRANSFERAPPLY") && convertRuleElement.TargetFormId.Equals("STK_TransferDirect"))
			{
				string text = Convert.ToString(base.View.Model.GetValue("FTransferDirect"));
				string a;
				if ((a = text) != null)
				{
					if (a == "GENERAL")
					{
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApp_R-StkTransferDirect_R") || p.EnumId.Equals("StkTransferApply-StkTransferDirect_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApp_R-StkTransferDirect_R") || p.Id.Equals("StkTransferApply-StkTransferDirect_R"));
						return;
					}
					if (!(a == "RETURN"))
					{
						return;
					}
					e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApply-StkTransferDirect"));
					e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApply-StkTransferDirect"));
				}
			}
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x0005DA74 File Offset: 0x0005BC74
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				Common.DoBomExpandDraw(base.View, Common.GetBomExpandBillFieldValue(base.View, "FStockOutOrgId", "", ""));
				base.View.UpdateView("FBillEntry");
				base.View.Model.DataChanged = true;
			}
			if ("PickInvReturnBegin".Equals(e.EventName))
			{
				this.isPickReturn = true;
				return;
			}
			if ("PickInvReturnFinish".Equals(e.EventName))
			{
				this.isPickReturn = false;
			}
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x0005DB44 File Offset: 0x0005BD44
		private void LockFieldThirdBill()
		{
			string value = Convert.ToString(base.View.Model.GetValue("FThirdSrcBillNo")).Trim();
			if (!string.IsNullOrEmpty(value))
			{
				this.LockFieldHead();
				int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
				for (int i = 0; i < entryRowCount; i++)
				{
					this.LockField(i);
				}
			}
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x0005DBA8 File Offset: 0x0005BDA8
		private void LockFieldHead()
		{
			BillUtils.LockField(base.View, "FStockOutOrgId", false, -1);
			BillUtils.LockField(base.View, "FOwnerTypeOutIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerOutIdHead", false, -1);
			BillUtils.LockField(base.View, "FStockOrgId", false, -1);
			BillUtils.LockField(base.View, "FOwnerTypeIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerIdHead", false, -1);
			BillUtils.LockField(base.View, "FTransferDirect", false, -1);
			BillUtils.LockField(base.View, "FTransferBizType", false, -1);
			BillUtils.LockField(base.View, "FDate", false, -1);
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x0005DC58 File Offset: 0x0005BE58
		private void LockField(int row)
		{
			base.View.GetFieldEditor("FMaterialId", row).Enabled = false;
			base.View.GetFieldEditor("FAuxPropId", row).Enabled = false;
			base.View.GetFieldEditor("FSrcStockId", row).Enabled = false;
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FQty", row).Enabled = false;
			base.View.GetFieldEditor("FLot", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitId", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitQty", row).Enabled = false;
			base.View.GetFieldEditor("FSaleUnitId", row).Enabled = false;
			base.View.GetFieldEditor("FSaleQty", row).Enabled = false;
			base.View.GetFieldEditor("FProduceDate", row).Enabled = false;
			base.View.GetFieldEditor("FExpiryDate", row).Enabled = false;
			base.View.GetFieldEditor("FSrcStockStatusId", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperTypeOutId", row).Enabled = false;
			base.View.GetFieldEditor("FBusinessDate", row).Enabled = false;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x0005DDEC File Offset: 0x0005BFEC
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FBillEntry"))
			{
				base.View.Model.SetValue("FOwnerTypeOutId", Convert.ToString(base.View.Model.GetValue("FOwnerTypeOutIdHead")), e.Row);
				base.View.Model.SetValue("FOwnerTypeId", Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead")), e.Row);
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT"))
				{
					string text = Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT"));
					if (StringUtils.EqualsIgnoreCase(text, "RETURN"))
					{
						base.View.Model.SetValue("FKeeperTypeOutId", "BD_Customer", e.Row);
						base.View.Model.SetValue("FKeeperOutId", 0, e.Row);
						base.View.GetFieldEditor("FKeeperOutId", e.Row).Enabled = true;
						return;
					}
					base.View.Model.SetValue("FKeeperTypeId", "BD_Customer", e.Row);
					base.View.Model.SetValue("FKeeperId", 0, e.Row);
					base.View.GetFieldEditor("FKeeperId", e.Row).Enabled = true;
				}
			}
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x0005DF8C File Offset: 0x0005C18C
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				if (base.View.OpenParameter.CreateFrom != 1)
				{
					long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOutOrgId", -1);
					if (baseDataLongValue > 0L)
					{
						SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
					}
					this.SetDefLocalCurrencyAndExchangeType();
					this.SetBusinessTypeByBillType();
				}
				else
				{
					GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOutOrgId", "", "FBaseCurrID", "", "FOwnerTypeOutIdHead", "FOwnerOutIdHead");
					SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
				}
				if (Convert.ToDecimal(base.View.Model.GetValue("FExchangeRate")) == 0m)
				{
					this.SetExchangeRate();
				}
				if (base.View.OpenParameter.CreateFrom == null && base.View.OpenParameter.Status == null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "CONSIGNMENT"))
				{
					string text = Convert.ToString(base.View.Model.GetValue("FTRANSFERDIRECT"));
					string text2;
					string text3;
					if (StringUtils.EqualsIgnoreCase(text, "RETURN"))
					{
						text2 = "FKeeperOutId";
						text3 = "FKeeperTypeOutId";
					}
					else
					{
						text2 = "FKeeperId";
						text3 = "FKeeperTypeId";
					}
					int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
					for (int i = 0; i < entryRowCount; i++)
					{
						base.View.Model.SetValue(text3, "BD_Customer", i);
						base.View.Model.SetValue(text2, 0, i);
					}
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					long num = Convert.ToInt64(dynamicObject["Id"]);
					object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "SAL_SystemParameter", "UseCustMatMapping", false);
					this.para_UseCustMatMapping = (systemProfile != null && Convert.ToBoolean(systemProfile));
				}
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x0005E1B4 File Offset: 0x0005C3B4
		public override void AfterCreateNewData(EventArgs e)
		{
			if (!this.bflag)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgID") as DynamicObject;
				long orgId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				this.CheckStockStartDate(orgId);
				this.bflag = true;
			}
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x0005E20B File Offset: 0x0005C40B
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x0005E214 File Offset: 0x0005C414
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbSDraw" || barItemKey == "tbDraw")
				{
					e.Cancel = !this.ValidatePush();
					return;
				}
				if (!(barItemKey == "tbSyncBaseData"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("填充内码", "004023030009280", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					base.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.DoFillBaseData();
			}
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x0005E2BC File Offset: 0x0005C4BC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
			this.ConsignTransferReturnWriteOffFeature(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (a == "SUBMIT")
				{
					dictionary = this.GetDeleteSerialIds();
					e.Option.SetVariableValue("DeleteSerialIds", dictionary);
					return;
				}
				if (a == "SAVE")
				{
					dictionary = this.GetDeleteSerialIds();
					e.Option.SetVariableValue("DeleteSerialIds", dictionary);
					return;
				}
				if (a == "ASSOCIATEDCOPYENTRYROW")
				{
					this.associatedCopyEntryRow = true;
					return;
				}
				if (a == "DELETEENTRY")
				{
					StockTransferDirectEdit.DeleteEntryParentRowIncludeSonRow(base.View, e, "FBillEntry");
					return;
				}
				if (!(a == "COPYENTRYROW"))
				{
					return;
				}
				if (!this.ValidationsParentRowInfo(e))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("套件父项或套件物料不允许复制！", "004023030009458", 5, new object[0]), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x0005E3B8 File Offset: 0x0005C5B8
		private void ConsignTransferReturnWriteOffFeature(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE") && !(a == "SUBMIT") && !(a == "AUDIT"))
				{
					return;
				}
				string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeId");
				string a2 = Convert.ToString(base.View.Model.GetValue("FBizType"));
				string a3 = Convert.ToString(base.View.Model.GetValue("FTransferDirect"));
				bool flag = Convert.ToBoolean(base.View.Model.GetValue("FWriteOffConsign"));
				bool flag2 = baseDataStringValue == "0bcc8f3ce0a64171b1a901344d1ac239" || a2 == "CONSIGNMENT";
				flag2 = (flag2 && flag && a3 == "RETURN");
				if (flag2)
				{
					string text = ResManager.LoadKDString("寄售调拨退回自动冲销(直接调拨单埋点)", "004023000020136", 5, new object[0]);
					SCMCommon.EventTrackingWithoutView(base.Context, 174, "ConsignTransferReturnAutoWriteOff", text, "ValueChange", e.Operation.FormOperation.Operation, text, base.View.PageId);
				}
			}
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x0005E4F4 File Offset: 0x0005C6F4
		private Dictionary<string, List<long>> GetDeleteSerialIds()
		{
			Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
			if (this.bindSerialIds.Count > 0)
			{
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
				Dictionary<string, List<long>> dictionary2 = new Dictionary<string, List<long>>();
				List<long> list = new List<long>();
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					long value = Convert.ToInt64(dynamicObject["Id"]);
					long num = (dynamicObject["QmEntryID"] == null) ? 0L : Convert.ToInt64(dynamicObject["QmEntryID"]);
					if (num != 0L)
					{
						string key = Convert.ToString(num) + "|" + Convert.ToString(value);
						DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["STK_StkTransInSerial"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
						{
							long num2 = Convert.ToInt64(dynamicObject2["SerialId_Id"]);
							if (num2 > 0L && !list.Contains(num2))
							{
								list.Add(num2);
							}
						}
						if (!dictionary2.ContainsKey(key) && list.Count > 0)
						{
							dictionary2.Add(key, list);
							list = new List<long>();
						}
					}
				}
				foreach (KeyValuePair<string, List<long>> keyValuePair in this.bindSerialIds)
				{
					string key2 = keyValuePair.Key;
					List<long> value2 = keyValuePair.Value;
					if (!dictionary2.ContainsKey(key2))
					{
						dictionary.Add(key2, value2);
					}
					else
					{
						List<long> list2 = dictionary2[key2];
						List<long> list3 = new List<long>();
						foreach (long item in value2)
						{
							if (!list2.Contains(item))
							{
								list3.Add(item);
							}
						}
						if (list3.Count > 0)
						{
							dictionary.Add(key2, list3);
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x0005E784 File Offset: 0x0005C984
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SUBMIT"))
				{
					if (!(a == "SAVE"))
					{
						if (!(a == "ASSOCIATEDCOPYENTRYROW"))
						{
							if (a == "BOMEXPAND")
							{
								SCMCommon.CheckBillStatusAndShowCanNotExpand(this, e, "FDocumentStatus", "FCANCELSTATUS", "FCloseStatus");
								if (!e.OperationResult.IsSuccess)
								{
									return;
								}
								this.ExecuteBOMExpandAction(false);
							}
						}
						else
						{
							this.associatedCopyEntryRow = false;
						}
					}
					else
					{
						this.GetBindSerials();
					}
				}
				else
				{
					this.GetBindSerials();
				}
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x0005E828 File Offset: 0x0005CA28
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKOUTORGID"))
				{
					if (!(a == "FROWTYPE"))
					{
						return;
					}
					string text = Convert.ToString(base.View.Model.GetValue("FRowType", e.Row));
					string text2 = Convert.ToString(e.Value);
					DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
					long num = (dynamicObject != null) ? Convert.ToInt64(dynamicObject["Id"]) : 0L;
					string text3 = Convert.ToString(base.View.Model.GetValue("FRowId", e.Row));
					if (num > 0L && StringUtils.EqualsIgnoreCase(text, "Parent") && !StringUtils.EqualsIgnoreCase(text2, "Parent"))
					{
						bool flag = false;
						DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
						{
							string text4 = Convert.ToString(dynamicObject2["ParentRowId"]);
							if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text3) && StringUtils.EqualsIgnoreCase(text4, text3))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							e.Cancel = true;
							base.View.ShowNotificationMessage(ResManager.LoadKDString("此套件父项行已存在展开的套件子项分录，不允许变更产品类型，如不需要此套件父项行，请使用删除分录行操作！", "004072030008998", 5, new object[0]), "", 0);
						}
					}
				}
				else
				{
					if (e.Value == null)
					{
						return;
					}
					long orgId;
					if (e.Value.GetType().Name == "DynamicObject")
					{
						orgId = Convert.ToInt64(((DynamicObject)e.Value)["Id"].ToString());
					}
					else
					{
						orgId = Convert.ToInt64(e.Value.ToString());
					}
					e.Cancel = (e.Cancel || this.CheckStockStartDate(orgId));
					return;
				}
			}
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x0005EA4C File Offset: 0x0005CC4C
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			this.AfterCopyBillSetRowId(e.DataObject["TransferDirectEntry"] as DynamicObjectCollection);
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x0005EA69 File Offset: 0x0005CC69
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.EntityKey, "FBillEntry"))
			{
				this.AfterCopyRowSetRowId(base.View, e);
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x0005EAB8 File Offset: 0x0005CCB8
		private void UpdateDestBom(int row)
		{
			string text = "";
			DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", row) as DynamicObject;
			if (dynamicObject == null)
			{
				text = "";
			}
			else
			{
				DynamicObjectCollection source = dynamicObject["MaterialInvPty"] as DynamicObjectCollection;
				if (source.SingleOrDefault((DynamicObject p) => Convert.ToBoolean(p["IsEnable"]) && Convert.ToInt64(p["InvPtyId_Id"]) == 10003L) == null)
				{
					text = "";
				}
				else
				{
					dynamicObject = (base.View.Model.GetValue("FBOMID", row) as DynamicObject);
					if (dynamicObject != null)
					{
						text = dynamicObject["Number"].ToString();
					}
				}
			}
			this.Model.SetItemValueByNumber("FDESTBOMID", text, row);
			base.View.InvokeFieldUpdateService("FDESTBOMID", row);
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x0005EB88 File Offset: 0x0005CD88
		private void DoFillBaseData()
		{
			long num = 0L;
			long num2 = 0L;
			DynamicObject dynamicObject = this.Model.GetValue("FStockOutOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			dynamicObject = (this.Model.GetValue("FStockOrgId") as DynamicObject);
			if (dynamicObject != null)
			{
				num2 = Convert.ToInt64(dynamicObject["Id"]);
			}
			if (num == 0L || num2 == 0L || num == num2)
			{
				return;
			}
			int num3 = 0;
			this.isDoFillMaterial = true;
			List<int> list = Common.FillTransBaseMapData(base.View, num, num2, "FMaterialId", "FDestMaterialId", "FNumber", "Number");
			this.isDoFillMaterial = false;
			if (list != null && list.Count > 0)
			{
				num3 = list.Count;
				foreach (int index in list)
				{
					this.SyncEntryLot(index);
				}
			}
			int num4 = 0;
			list = Common.FillTransBaseMapData(base.View, num, num2, "FBomId", "FDestBomId", "FNumber", "Number");
			if (list != null && list.Count > 0)
			{
				num4 = list.Count;
			}
			string text = string.Format(ResManager.LoadKDString("已填充【{0}】条物料内码，【{1}】条BOM内码。", "004023000039701", 5, new object[0]), num3, num4);
			base.View.ShowMessage(text, 0);
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x0005ED04 File Offset: 0x0005CF04
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
				int num = dynamicObjectCollection.Count - 1;
				for (int i = num; i >= 0; i--)
				{
					if (dynamicObjectCollection[i]["MaterialId"] != null && Convert.ToDecimal(dynamicObjectCollection[i]["Qty"]) == 0m && !StringUtils.EqualsIgnoreCase(Convert.ToString(dynamicObjectCollection[i]["RowType"]), "Parent"))
					{
						this.Model.DeleteEntryRow("FBillEntry", i);
					}
				}
				if (this.Model.GetEntryRowCount("FBillEntry") == 0)
				{
					base.View.ShowErrMessage("", ResManager.LoadKDString("分录“明细”是必填项。", "004023000021872", 5, new object[0]), 0);
					return false;
				}
				base.View.UpdateView("FBillEntry");
			}
			return true;
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x0005EE20 File Offset: 0x0005D020
		private bool CheckStockStartDate(long orgId)
		{
			bool result = false;
			if (base.View.OpenParameter.Status != null)
			{
				return result;
			}
			if (orgId == 0L)
			{
				return result;
			}
			if (StockServiceHelper.GetUpdateStockDate(base.Context, orgId) == null)
			{
				base.View.Model.SetValue("FStockOutOrgID", null);
				if (orgId != 0L)
				{
					base.View.ShowMessage(ResManager.LoadKDString("所选库存组织未启用", "004023030002272", 5, new object[0]), 0);
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x0005EE9C File Offset: 0x0005D09C
		private bool GetBOMFilter(int row, out string filter)
		{
			filter = "";
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			if (dynamicObject == null)
			{
				return false;
			}
			long num = Convert.ToInt64(dynamicObject["msterID"]);
			long value = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			List<long> approvedBomIdByOrgId = MFGServiceHelperForSCM.GetApprovedBomIdByOrgId(base.View.Context, num, value, num2);
			if (!ListUtils.IsEmpty<long>(approvedBomIdByOrgId))
			{
				filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", approvedBomIdByOrgId));
			}
			else
			{
				filter = string.Format(" FID={0}", 0);
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x0005EF90 File Offset: 0x0005D190
		private void AuxpropDataChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this.lastAuxpropId)
			{
				return;
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOutOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, num, false, value2, false);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterial, row);
			}
			this.lastAuxpropId = num;
			base.View.UpdateView("FBillEntry", row);
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x0005F07C File Offset: 0x0005D27C
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOutOrgId", -1, 0L, null);
			long bomDefaultValueByMaterialExceptApi = SCMCommon.GetBomDefaultValueByMaterialExceptApi(base.View, dynamicObject, newAuxpropData, false, value2, value, false);
			if (bomDefaultValueByMaterialExceptApi != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterialExceptApi, row);
			}
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x0005F10C File Offset: 0x0005D30C
		public void CheckAccountDate()
		{
			long num = 0L;
			long num2 = 0L;
			string text = string.Empty;
			DateTime dateTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject;
			string text2 = base.View.Model.GetValue("FOwnerTypeOutIdHead").ToString();
			if (base.View.Model.GetValue("FDate") != null)
			{
				dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate").ToString());
			}
			if (dynamicObject != null || dynamicObject2 != null)
			{
				if (dynamicObject != null)
				{
					num = Convert.ToInt64(dynamicObject["Id"]);
				}
				if (dynamicObject2 != null)
				{
					num2 = Convert.ToInt64(dynamicObject2["Id"]);
				}
				if (text2 != null)
				{
					text = text2.ToString();
				}
				string text3 = CommonServiceHelper.CheckDate(base.Context, num, num2, text, "", dateTime);
				if (text3.Length > 0)
				{
					base.View.ShowErrMessage(text3, ResManager.LoadKDString("单据日期校验不通过", "004023030000370", 5, new object[0]), 0);
					return;
				}
			}
			else
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("库存组织未录入，时间不允许修改", "004023030000373", 5, new object[0]), ResManager.LoadKDString("单据日期校验不通过", "004023030000370", 5, new object[0]), 0);
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x0005F2B4 File Offset: 0x0005D4B4
		protected BillNode GetReplaceTrackResult(BillNode trackResult, string targetFormKey)
		{
			if (trackResult.FormKey.Equals("SUB_PPBOM", StringComparison.OrdinalIgnoreCase) && targetFormKey.Equals("SUB_SUBREQORDER", StringComparison.OrdinalIgnoreCase))
			{
				DynamicObjectCollection subEntityIdsByPPBomEntityId = TrackUpDownHelper.GetSubEntityIdsByPPBomEntityId(base.View.Context, (from o in trackResult.LinkIds
				select Convert.ToInt64(o)).ToList<long>());
				trackResult = BillNode.Create("SUB_SUBREQORDER", "", null);
				trackResult.AddLinkCopyData((from o in subEntityIdsByPPBomEntityId
				select o["FSUBREQENTRYID"].ToString()).ToList<string>());
				return trackResult;
			}
			if (trackResult.FormKey.Equals("PRD_PPBOM", StringComparison.OrdinalIgnoreCase) && targetFormKey.Equals("PRD_MO", StringComparison.OrdinalIgnoreCase))
			{
				DynamicObjectCollection moEntityIdsByPPBomEntityId = TrackUpDownHelper.GetMoEntityIdsByPPBomEntityId(base.View.Context, (from o in trackResult.LinkIds
				select Convert.ToInt64(o)).ToList<long>());
				trackResult = BillNode.Create("PRD_MO", "", null);
				trackResult.AddLinkCopyData((from o in moEntityIdsByPPBomEntityId
				select o["FMOENTRYID"].ToString()).ToList<string>());
				return trackResult;
			}
			return trackResult;
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x0005F410 File Offset: 0x0005D610
		private bool ValidatePush()
		{
			if (base.View.Model.GetValue("FStockOutOrgId") == null || base.View.Model.GetValue("FTransferDirect") == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请录入调拨库存组织和调拨方向！", "004023030002332", 5, new object[0]), "", 0);
				return false;
			}
			if (this.IsExistEntryData())
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("分录中已经存在手工录入数据，不能进行选单操作！", "004023030002335", 5, new object[0]), "", 0);
				return false;
			}
			return true;
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x0005F4A8 File Offset: 0x0005D6A8
		private void SetExchangeRate()
		{
			DynamicObject dynamicObject = (DynamicObject)base.View.Model.GetValue("FBaseCurrID");
			DynamicObject dynamicObject2 = (DynamicObject)base.View.Model.GetValue("FExchangeTypeId");
			DynamicObject dynamicObject3 = (DynamicObject)base.View.Model.GetValue("FSETTLECURRID");
			if (dynamicObject == null || dynamicObject2 == null || dynamicObject3 == null)
			{
				return;
			}
			long num = Convert.ToInt64(dynamicObject["Id"]);
			long num2 = Convert.ToInt64(dynamicObject2["Id"]);
			long num3 = Convert.ToInt64(dynamicObject3["Id"]);
			DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate"));
			if (num == num3)
			{
				base.View.Model.SetValue("FExchangeRate", 1);
				base.View.GetFieldEditor<DecimalFieldEditor>("FExchangeRate", 0).Enabled = false;
				return;
			}
			KeyValuePair<decimal, int> exchangeRateAndDecimal = CommonServiceHelper.GetExchangeRateAndDecimal(base.Context, num3, num, num2, dateTime, dateTime);
			base.View.Model.SetValue("FExchangeRate", exchangeRateAndDecimal.Key);
			base.View.GetFieldEditor<DecimalFieldEditor>("FExchangeRate", 0).Scale = Convert.ToInt16(exchangeRateAndDecimal.Value);
			base.View.GetFieldEditor<DecimalFieldEditor>("FExchangeRate", 0).Enabled = true;
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x0005F60C File Offset: 0x0005D80C
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOutOrgId", "FExchangeTypeId", "FBaseCurrID", "FSettleCurrId", "FOwnerTypeOutIdHead", "FOwnerOutIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x0005F649 File Offset: 0x0005D849
		private void ChangeStockerData(STK_TransferDirect transfer)
		{
			Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x0005F660 File Offset: 0x0005D860
		private void ChangeInOwnerHeadData(STK_TransferDirect transfer)
		{
			this.SetDefLocalCurrencyAndExchangeType();
			this.SetExchangeRate();
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x0005F760 File Offset: 0x0005D960
		private void SetComValue()
		{
			DynamicObjectCollection source = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
			(from p in source
			where !string.IsNullOrWhiteSpace(Convert.ToString(p["SrcBillNo"]))
			select p).Count<DynamicObject>();
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FTransferBizType", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			ComboField comboField = this.Model.BusinessInfo.GetElement("FTransferBizType") as ComboField;
			List<EnumItem> list = new List<EnumItem>();
			list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
			(from p in list
			where p.Value == "InnerOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item = (from p in list
			where p.Value == "OverOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item2 = (from p in list
			where p.Value == "OverOrgSale"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item3 = (from p in list
			where p.Value == "OverOrgPurchase"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item4 = (from p in list
			where p.Value == "OverOrgSubPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item5 = (from p in list
			where p.Value == "OverOrgMisDelivery"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item6 = (from p in list
			where p.Value == "OverOrgPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item7 = (from p in list
			where p.Value == "OverOrgPrdIn"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item8 = (from p in list
			where p.Value == "OverOrgPrdOut"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item9 = (from p in list
			where p.Value == "OverOrgPurVMI"
			select p).FirstOrDefault<EnumItem>();
			if (!base.Context.IsMultiOrg)
			{
				string value = this.Model.GetValue("FBizType", 0) as string;
				if (!"VMI".Equals(value))
				{
					list.Remove(item);
				}
				list.Remove(item2);
				list.Remove(item3);
				list.Remove(item5);
				list.Remove(item4);
				list.Remove(item6);
				list.Remove(item7);
				list.Remove(item8);
				list.Remove(item9);
				comboFieldEditor.SetComboItems(list);
				return;
			}
			list.Remove(item2);
			list.Remove(item3);
			list.Remove(item5);
			list.Remove(item4);
			list.Remove(item6);
			list.Remove(item7);
			list.Remove(item8);
			list.Remove(item9);
			comboFieldEditor.SetComboItems(list);
			ComboFieldEditor comboFieldEditor2 = base.View.GetFieldEditor("FBizType", 0) as ComboFieldEditor;
			if (comboFieldEditor2 == null)
			{
				return;
			}
			comboField = (this.Model.BusinessInfo.GetElement("FBizType") as ComboField);
			List<EnumItem> list2 = new List<EnumItem>();
			list2 = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
			(from p in list2
			where p.Value == "CONSIGNMENT"
			select p).FirstOrDefault<EnumItem>();
			(from p in list2
			where p.Value == "NORMAL"
			select p).FirstOrDefault<EnumItem>();
			comboFieldEditor2.SetComboItems(list2);
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x0005FB50 File Offset: 0x0005DD50
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			string text = string.Empty;
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			long custId = 0L;
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FCUSTMATID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				long mainOrgId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				string text2 = Convert.ToString(base.View.Model.GetValue("FKeeperTypeId", row));
				if (StringUtils.EqualsIgnoreCase(text2, "BD_Customer"))
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FKeeperId", row) as DynamicObject;
					custId = ((dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]));
				}
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FMaterialID", row) as DynamicObject;
				long materialId = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				filter = Common.GetMapIdFilter(mainOrgId, custId, materialId, this._baseDataOrgCtl);
				break;
			}
			case "FSRCSTOCKID":
			{
				string arg = string.Empty;
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FSrcStockStatusID", row) as DynamicObject;
				arg = ((dynamicObject4 == null) ? "" : Convert.ToString(dynamicObject4["Number"]));
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FType"));
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "BD_StockStatus",
					FilterClauseWihtKey = string.Format("FNumber='{0}'", arg),
					SelectItems = list
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				if (dynamicObjectCollection.Count > 0)
				{
					DynamicObject dynamicObject5 = dynamicObjectCollection[0];
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject5["FType"]);
				}
				break;
			}
			case "FDESTSTOCKID":
			{
				string arg2 = string.Empty;
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FDestStockStatusID", row) as DynamicObject;
				arg2 = ((dynamicObject6 == null) ? "" : Convert.ToString(dynamicObject6["Number"]));
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FType"));
				QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
				{
					FormId = "BD_StockStatus",
					FilterClauseWihtKey = string.Format("FNumber='{0}'", arg2),
					SelectItems = list2
				};
				DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
				if (dynamicObjectCollection2.Count > 0)
				{
					DynamicObject dynamicObject7 = dynamicObjectCollection2[0];
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject7["FType"]);
				}
				break;
			}
			case "FMATERIALID":
				filter = " 1=1 ";
				text = (base.View.Model.GetValue("FBizType") as string);
				if (StringUtils.EqualsIgnoreCase(text, "VMI"))
				{
					filter += " and FIsVmiBusiness = '1' ";
				}
				break;
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject8 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject8 == null) ? 0L : Convert.ToInt64(dynamicObject8["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject9 = base.View.Model.GetValue("FStockerId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject9 != null && Convert.ToInt64(dynamicObject9["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject9["Id"]));
				}
				break;
			}
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
				break;
			case "FSTOCKOUTORGID":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00060014 File Offset: 0x0005E214
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSRCSTOCKSTATUSID"))
				{
					if (a == "FDESTSTOCKSTATUSID")
					{
						DynamicObject dynamicObject = base.View.Model.GetValue("FDestStockId", row) as DynamicObject;
						if (dynamicObject != null)
						{
							List<SelectorItemInfo> list = new List<SelectorItemInfo>();
							list.Add(new SelectorItemInfo("FStockStatusType"));
							QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
							{
								FormId = "BD_STOCK",
								FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject["Id"]),
								SelectItems = list
							};
							DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
							string text = "";
							if (dynamicObjectCollection != null)
							{
								DynamicObject dynamicObject2 = dynamicObjectCollection[0];
								text = Convert.ToString(dynamicObject2["FStockStatusType"]);
							}
							if (!string.IsNullOrWhiteSpace(text))
							{
								text = "'" + text.Replace(",", "','") + "'";
								filter = string.Format(" FType IN ({0})", text);
							}
						}
					}
				}
				else
				{
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FSrcStockId", row) as DynamicObject;
					if (dynamicObject3 != null)
					{
						List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
						list2.Add(new SelectorItemInfo("FStockStatusType"));
						QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
						{
							FormId = "BD_STOCK",
							FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject3["Id"]),
							SelectItems = list2
						};
						DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
						string text2 = "";
						if (dynamicObjectCollection2 != null)
						{
							DynamicObject dynamicObject4 = dynamicObjectCollection2[0];
							text2 = Convert.ToString(dynamicObject4["FStockStatusType"]);
						}
						if (!string.IsNullOrWhiteSpace(text2))
						{
							text2 = "'" + text2.Replace(",", "','") + "'";
							filter = string.Format(" FType IN ({0})", text2);
						}
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00060240 File Offset: 0x0005E440
		private bool GetOwnerFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FOWNERIDHEAD") && !(a == "FOWNERID"))
				{
					if (a == "FOWNEROUTIDHEAD" || a == "FOWNEROUTID")
					{
						string a2 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeOutIdHead"));
						string a3 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeOutId", row));
						if (((fieldKey.ToUpperInvariant().Equals("FOWNEROUTIDHEAD") && a2 == "BD_Supplier") || (fieldKey.ToUpperInvariant().Equals("FOWNEROUTID") && a3 == "BD_Supplier")) && base.View.Model.GetValue("FBizType").Equals("VMI"))
						{
							filter = Common.getVMIOwnerFilter();
						}
						if ((fieldKey.ToUpperInvariant().Equals("FOWNEROUTIDHEAD") && a2 == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNEROUTID") && a3 == "BD_OwnerOrg"))
						{
							DynamicObject dynamicObject = base.View.Model.GetValue("FStockOutOrgId") as DynamicObject;
							long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
							bool bizRelation = CommonServiceHelper.GetBizRelation(base.Context, 112L, -1L);
							if (bizRelation)
							{
								filter = string.Format("  (EXISTS (SELECT 1 FROM  t_org_bizrelation a inner join t_org_bizrelationEntry b on a.FBIZRELATIONID=b.FBIZRELATIONID\r\n                                    where a.FBRTYPEID={0} AND b.FRELATIONORGID={1} AND b.FORGID=t0.FORGID) OR t0.FORGID={1})", 112, num);
							}
						}
					}
				}
				else
				{
					string a4 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead"));
					string a5 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", row));
					if ((fieldKey.ToUpperInvariant().Equals("FOWNERIDHEAD") && a4 == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNERID") && a5 == "BD_OwnerOrg"))
					{
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
						long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
						bool bizRelation2 = CommonServiceHelper.GetBizRelation(base.Context, 112L, -1L);
						if (bizRelation2)
						{
							filter = string.Format("  (EXISTS (SELECT 1 FROM  t_org_bizrelation a inner join t_org_bizrelationEntry b on a.FBIZRELATIONID=b.FBIZRELATIONID\r\n                                    where a.FBRTYPEID={0} AND b.FRELATIONORGID={1} AND b.FORGID=t0.FORGID)  OR t0.FORGID = {1} )", 112, num2);
						}
					}
					else if (((fieldKey.ToUpperInvariant().Equals("FOWNERIDHEAD") && a4 == "BD_Supplier") || (fieldKey.ToUpperInvariant().Equals("FOWNERID") && a5 == "BD_Supplier")) && base.View.Model.GetValue("FBizType").Equals("VMI"))
					{
						filter = Common.getVMIOwnerFilter();
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00060554 File Offset: 0x0005E754
		private void LockField()
		{
			DynamicObjectCollection source = base.View.Model.DataObject["TransferDirectEntry"] as DynamicObjectCollection;
			string text = source.FirstOrDefault<DynamicObject>()["SrcBillTypeId"].ToString();
			if (!string.IsNullOrWhiteSpace(text))
			{
				bool flag = false;
				string a;
				if ((a = text) != null)
				{
					if (a == "STK_TransferDirect")
					{
						BillUtils.LockField(base.View, "FTransferDirect", flag, -1);
						BillUtils.LockField(base.View, "FStockOutOrgId", flag, -1);
						BillUtils.LockField(base.View, "FOwnerTypeOutIdHead", flag, -1);
						BillUtils.LockField(base.View, "FOwnerOutIdHead", flag, -1);
						BillUtils.LockField(base.View, "FStockOrgId", flag, -1);
						BillUtils.LockField(base.View, "FOwnerTypeIdHead", flag, -1);
						BillUtils.LockField(base.View, "FOwnerIdHead", flag, -1);
						BillUtils.LockField(base.View, "FTransferDirect", flag, -1);
						BillUtils.LockField(base.View, "FBizType", flag, -1);
						return;
					}
					if (a == "SAL_SaleOrder")
					{
						BillUtils.LockField(base.View, "FBizType", flag, -1);
						return;
					}
					if (a == "")
					{
						return;
					}
				}
				BillUtils.LockField(base.View, "FStockOutOrgId", flag, -1);
				BillUtils.LockField(base.View, "FOwnerTypeOutIdHead", flag, -1);
				BillUtils.LockField(base.View, "FOwnerOutIdHead", flag, -1);
				BillUtils.LockField(base.View, "FTransferDirect", flag, -1);
				BillUtils.LockField(base.View, "FBizType", flag, -1);
			}
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x0006075C File Offset: 0x0005E95C
		private bool IsExistEntryData()
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject[this.GetFirstEntityName(this.Model.BusinessInfo)];
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count <= 0)
			{
				return false;
			}
			IEnumerable<DynamicObject> source = from w in dynamicObjectCollection
			where !string.IsNullOrWhiteSpace(BillUtils.GetDynamicObjectItemValue<string>(w, "SrcBillTypeId", null))
			select w;
			if (source.Count<DynamicObject>() > 0)
			{
				return false;
			}
			IEnumerable<DynamicObject> source2 = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "MaterialId_Id", 0) > 0
			select w;
			IEnumerable<DynamicObject> source3 = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "Qty", 0) > 0
			select w;
			IEnumerable<DynamicObject> source4 = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "UnitId_Id", 0) > 0
			select w;
			IEnumerable<DynamicObject> source5 = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "SrcStockId_Id", 0) > 0
			select w;
			IEnumerable<DynamicObject> source6 = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "DestStockId_Id", 0) > 0
			select w;
			int num = source2.Count<DynamicObject>() + source3.Count<DynamicObject>() + source4.Count<DynamicObject>() + source5.Count<DynamicObject>() + source6.Count<DynamicObject>();
			return num > 0 && source.Count<DynamicObject>() <= 0;
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x000608BC File Offset: 0x0005EABC
		private string GetFirstEntityName(BusinessInfo info)
		{
			return this.GetFirstEntry(info, false);
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x000608C8 File Offset: 0x0005EAC8
		private string GetFirstEntry(BusinessInfo info, bool isKey)
		{
			foreach (Entity entity in info.Entrys)
			{
				if (1 == entity.EntityType)
				{
					return isKey ? entity.Key : entity.EntryName;
				}
			}
			return null;
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00060934 File Offset: 0x0005EB34
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_TransDrtBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00060984 File Offset: 0x0005EB84
		private void SwapFields(DynamicObjectCollection dyEntity, int i)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FKeeperTypeId", i));
			DynamicObject dynamicObject = base.View.Model.GetValue("FKeeperId", i) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FDestStockId", i) as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			DynamicObject dynamicObject3 = base.View.Model.GetValue("FDestStockStatusId", i) as DynamicObject;
			long num3 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
			string text2 = Convert.ToString(base.View.Model.GetValue("FKeeperTypeOutId", i));
			DynamicObject dynamicObject4 = base.View.Model.GetValue("FKeeperOutId", i) as DynamicObject;
			long num4 = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
			DynamicObject dynamicObject5 = base.View.Model.GetValue("FSrcStockId", i) as DynamicObject;
			long num5 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
			DynamicObject dynamicObject6 = base.View.Model.GetValue("FSrcStockStatusId", i) as DynamicObject;
			long num6 = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"]);
			base.View.Model.SetValue("FKeeperTypeId", text2, i);
			base.View.Model.SetValue("FKeeperId", num4, i);
			base.View.Model.SetValue("FDestStockId", num5, i);
			base.View.Model.SetValue("FDestStockStatusId", num6, i);
			base.View.Model.SetValue("FKeeperTypeOutId", text, i);
			base.View.Model.SetValue("FKeeperOutId", num, i);
			base.View.Model.SetValue("FSrcStockId", num2, i);
			base.View.Model.SetValue("FSrcStockStatusId", num3, i);
			long num7 = Convert.ToInt64(dyEntity[i]["SrcStockLocId_Id"]);
			dyEntity[i]["SrcStockLocId_Id"] = Convert.ToInt64(dyEntity[i]["DestStockLocId_Id"]);
			dyEntity[i]["DestStockLocId_Id"] = num7;
			DynamicObject value = (DynamicObject)ObjectUtils.CreateCopy(base.View.Model.GetValue("FSrcStockLocId", i) as DynamicObject);
			DynamicObject value2 = (DynamicObject)ObjectUtils.CreateCopy(base.View.Model.GetValue("FDestStockLocId", i) as DynamicObject);
			Field field = base.View.BusinessInfo.GetField("FSrcStockLocId");
			RelatedFlexGroupField flexField = (RelatedFlexGroupField)field;
			this.SetFlexValue(flexField, value2, i);
			Field field2 = base.View.BusinessInfo.GetField("FDestStockLocId");
			RelatedFlexGroupField flexField2 = (RelatedFlexGroupField)field2;
			this.SetFlexValue(flexField2, value, i);
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00060CF0 File Offset: 0x0005EEF0
		private void SetFlexValue(RelatedFlexGroupField flexField, DynamicObject value, int row)
		{
			if (value == null)
			{
				this.Model.SetValue(flexField.Key, null, row);
				return;
			}
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FBillEntry");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, row);
			DynamicObject dynamicObject = (DynamicObject)ObjectUtils.CreateCopy(value);
			flexField.DynamicProperty.SetValue(entityDataObject, dynamicObject);
			base.View.UpdateView(flexField.Key, row);
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00060D68 File Offset: 0x0005EF68
		private void SetCustAndSupplierValue()
		{
			if (base.View.OpenParameter.Status != null)
			{
				return;
			}
			string a = base.View.Model.GetValue("FOwnerTypeOutIdHead") as string;
			string a2 = base.View.Model.GetValue("FOwnerTypeIdHead") as string;
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject;
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			if (a == "BD_OwnerOrg" && a2 == "BD_OwnerOrg" && dynamicObject != null && dynamicObject2 != null)
			{
				string a3 = base.View.Model.GetValue("FTransferDirect") as string;
				if (a3 == "RETURN")
				{
					Dictionary<string, long> custAndSupplierValue = StockServiceHelper.GetCustAndSupplierValue(base.Context, dynamicObject2, dynamicObject);
					if (custAndSupplierValue != null)
					{
						base.View.Model.SetValue("FCUSTID", custAndSupplierValue["FCUSTID"]);
						base.View.Model.SetValue("FSUPPLIERID", custAndSupplierValue["FSUPPLIERID"]);
						return;
					}
				}
				else
				{
					Dictionary<string, long> custAndSupplierValue2 = StockServiceHelper.GetCustAndSupplierValue(base.Context, dynamicObject, dynamicObject2);
					if (custAndSupplierValue2 != null)
					{
						base.View.Model.SetValue("FCUSTID", custAndSupplierValue2["FCUSTID"]);
						base.View.Model.SetValue("FSUPPLIERID", custAndSupplierValue2["FSUPPLIERID"]);
						return;
					}
				}
			}
			else
			{
				base.View.Model.SetValue("FCUSTID", 0);
				base.View.Model.SetValue("FSUPPLIERID", 0);
			}
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00060F48 File Offset: 0x0005F148
		private void SynOwnerType(string sourfield, string destfield, int row)
		{
			string text = base.View.Model.GetValue(sourfield) as string;
			base.View.BusinessInfo.GetField(destfield);
			base.View.Model.SetValue(destfield, text, row);
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00060F94 File Offset: 0x0005F194
		private void SynHeadToEntry(string sourfield, string destfield)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue(sourfield) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			Field field = base.View.BusinessInfo.GetField(destfield);
			int num2 = base.View.Model.GetEntryRowCount(field.EntityKey) - 1;
			if (this.CheckOwnerType(sourfield))
			{
				for (int i = 0; i <= num2; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						this.Model.SetValue(destfield, num, i);
					}
				}
				return;
			}
			for (int j = 0; j <= num2; j++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", j) as DynamicObject;
				DynamicObject dynamicObject3 = base.View.Model.GetValue(destfield, j) as DynamicObject;
				long num3 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				if (num3 == 0L && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
				{
					this.Model.SetValue(destfield, num, j);
				}
			}
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x000610FC File Offset: 0x0005F2FC
		private void SetOwnerTypeHead()
		{
			if (!this.CheckOwnerType("FOWNEROUTIDHEAD"))
			{
				this.Model.BeginIniti();
				base.View.Model.SetValue("FOWNEROUTIDHEAD", 0);
				this.Model.EndIniti();
				base.View.UpdateView("FOWNEROUTIDHEAD");
			}
			if (!this.CheckOwnerType("FOWNERIDHEAD"))
			{
				this.Model.BeginIniti();
				base.View.Model.SetValue("FOWNERIDHEAD", 0);
				this.Model.EndIniti();
				base.View.UpdateView("FOWNERIDHEAD");
			}
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x000611A8 File Offset: 0x0005F3A8
		private bool CheckOwnerType(string sourfield)
		{
			string text = string.Empty;
			string a;
			if ((a = sourfield.ToUpperInvariant()) != null)
			{
				if (!(a == "FOWNEROUTIDHEAD"))
				{
					if (a == "FOWNERIDHEAD")
					{
						text = "FOwnerTypeIdHead";
					}
				}
				else
				{
					text = "FOwnerTypeOutIdHead";
				}
			}
			return !ObjectUtils.IsNullOrEmpty(text) && base.View.Model.GetValue(text).ToString() == "BD_OwnerOrg";
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00061230 File Offset: 0x0005F430
		private void InitializeSuitParaData()
		{
			this.para_UseSuiteProduct = SCMCommon.GetUseSuiteProductParam(this, "FStockOutOrgId");
			FormOperation formOperation = base.View.GetFormOperation("Copy").FormOperation;
			if (formOperation != null && formOperation.Validations != null && formOperation.Validations.Count<AbstractValidation>() > 0)
			{
				AbstractValidation abstractValidation = (from p in formOperation.Validations
				where StringUtils.EqualsIgnoreCase(p.Id, "19b7ba4e-3075-4943-8ac1-abcfd6b637a8")
				select p).FirstOrDefault<AbstractValidation>();
				if (abstractValidation != null && abstractValidation.IsUsed)
				{
					this._allowCopySuiteProduct = false;
				}
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x000612BD File Offset: 0x0005F4BD
		private void LockAndUnLockParentWhenInitAndValueChanged(IdView idView)
		{
			StockTransferDirectEdit.LockAndUnLockParent(this, false, "FStockOutOrgId");
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x000612CB File Offset: 0x0005F4CB
		private void LockAndUnLockParentWhenItemRemoved(IdView idView)
		{
			StockTransferDirectEdit.LockAndUnLockParent(this, true, "FStockOutOrgId");
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00061340 File Offset: 0x0005F540
		public static void LockAndUnLockParent(AbstractBillPlugIn bill, bool delete = false, string mainOrgFieldKey = "FStockOutOrgId")
		{
			bool useSuiteProductParam = SCMCommon.GetUseSuiteProductParam(bill, mainOrgFieldKey);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(bill) || !useSuiteProductParam)
			{
				return;
			}
			Field field = bill.View.Model.BusinessInfo.GetField("FRowType");
			if (field == null)
			{
				return;
			}
			Entity entity = field.Entity;
			DynamicObjectCollection dynamicObjectCollection = bill.View.Model.DataObject[entity.EntryName] as DynamicObjectCollection;
			if (dynamicObjectCollection != null)
			{
				if (dynamicObjectCollection.ToList<DynamicObject>().Exists((DynamicObject n) => StringUtils.EqualsIgnoreCase(Convert.ToString(n["RowType"]), "Parent")))
				{
					List<DynamicObject> list = (from n in dynamicObjectCollection
					where StringUtils.EqualsIgnoreCase(Convert.ToString(n["RowType"]), "Parent")
					select n).ToList<DynamicObject>();
					List<DynamicObject> source = (from n in dynamicObjectCollection
					where StringUtils.EqualsIgnoreCase(Convert.ToString(n["RowType"]), "Son")
					select n).ToList<DynamicObject>();
					List<string> list2 = (from n in source
					select Convert.ToString(n["ParentRowId"])).ToList<string>();
					EntryGrid control = bill.View.GetControl<EntryGrid>(entity.Key);
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(control))
					{
						return;
					}
					foreach (DynamicObject dynamicObject in list)
					{
						string text = Convert.ToString(dynamicObject["RowId"]);
						bool flag = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) || !list2.Contains(text);
						int rowIndex = bill.View.Model.GetRowIndex(entity, dynamicObject);
						if (rowIndex >= 0)
						{
							if (!flag)
							{
								if (!delete)
								{
									BillUtils.LockField(bill.View, "FMaterialId", false, rowIndex);
									BillUtils.LockField(bill.View, "FRowType", false, rowIndex);
								}
							}
							else
							{
								BillUtils.LockField(bill.View, "FMaterialId", true, rowIndex);
								BillUtils.LockField(bill.View, "FRowType", true, rowIndex);
							}
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00061554 File Offset: 0x0005F754
		private void SetSuiteProductVisible()
		{
			if (!this.para_UseSuiteProduct)
			{
				base.View.GetBarItem("FBillEntry", "tbBOMExpand").Visible = this.para_UseSuiteProduct;
				base.View.GetControl("FRowType").Visible = this.para_UseSuiteProduct;
				base.View.GetControl("FParentMatId").Visible = this.para_UseSuiteProduct;
				base.View.GetControl("FRowId").Visible = this.para_UseSuiteProduct;
				base.View.GetControl("FParentRowId").Visible = this.para_UseSuiteProduct;
			}
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x000615F8 File Offset: 0x0005F7F8
		private void ExecuteBOMExpandAction(bool isAuto = false)
		{
			if (this.expandSuiteBySonMaterial)
			{
				return;
			}
			EntryGrid control = base.View.GetControl<EntryGrid>("FBillEntry");
			int focusRowIndex = control.GetFocusRowIndex();
			this.changeStockBaseQtyByMaterial = false;
			DynamicObjectCollection bomexpandResult = this.GetBOMExpandResult("");
			if (bomexpandResult != null && bomexpandResult.Count > 0)
			{
				this.SetBOMExpandResultData(bomexpandResult);
				if (!isAuto)
				{
					base.View.Model.ClearNoDataRow();
					base.View.Model.CreateNewEntryRow("FBillEntry");
				}
				control.SetFocusRowIndex(focusRowIndex);
			}
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x0006167C File Offset: 0x0005F87C
		private DynamicObjectCollection GetBOMExpandResult(string sCurrRowId = "")
		{
			ProductBomExpandArgs productBomExpandArgs = new ProductBomExpandArgs("FStockOutOrgId", "FStockOutOrgId", "FRowType", "FRowId", "FParentRowId", "FParentMatId", "FMaterialId", "FBomId", "FBaseUnitId", "FBaseQty", "FMaterialId", "FUnitId", "FQty");
			ProductBomExpand productBomExpand = new ProductBomExpand(base.Context, base.View.BusinessInfo.GetForm().Id, base.View.BusinessInfo, base.View.Model.DataObject);
			DynamicObjectCollection productBomExpandResult;
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(sCurrRowId))
			{
				productBomExpandResult = productBomExpand.GetProductBomExpandResult(productBomExpandArgs, sCurrRowId);
			}
			else
			{
				productBomExpandResult = productBomExpand.GetProductBomExpandResult(productBomExpandArgs, "");
			}
			return productBomExpandResult;
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00061754 File Offset: 0x0005F954
		private void SetBOMExpandResultData(DynamicObjectCollection dycExpandResult)
		{
			if (dycExpandResult == null || dycExpandResult.Count < 1)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FBillEntry");
			base.View.Model.ClearNoDataRow();
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (entityDataObject == null || entityDataObject.Count < 1)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = ObjectUtils.CreateCopy(entityDataObject) as DynamicObjectCollection;
			string sCurrProductRowId = "";
			int num = 1;
			long num2 = 0L;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				string text = Convert.ToString(dynamicObject["RowType"]);
				sCurrProductRowId = Convert.ToString(dynamicObject["RowId"]);
				if (!StringUtils.EqualsIgnoreCase(text, "Parent"))
				{
					num++;
				}
				else
				{
					DynamicObject dynamicObject2 = dynamicObject["MaterialId"] as DynamicObject;
					long num3 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					if (num3 == 0L)
					{
						num++;
					}
					else
					{
						IEnumerable<DynamicObject> enumerable = from p in dycExpandResult
						where StringUtils.EqualsIgnoreCase(Convert.ToString(p["SrcBillNo"]), sCurrProductRowId)
						select p;
						if (enumerable == null || enumerable.Count<DynamicObject>() < 1)
						{
							num++;
						}
						else
						{
							DynamicObject dynamicObject3 = dynamicObject["FOwnerOutId"] as DynamicObject;
							DynamicObject dynamicObject4 = dynamicObject["OwnerID"] as DynamicObject;
							string text2 = Convert.ToString(dynamicObject["KeeperTypeOutId"]);
							string text3 = Convert.ToString(dynamicObject["KeeperTypeId"]);
							DynamicObject dynamicObject5 = dynamicObject["KeeperOutId"] as DynamicObject;
							DynamicObject dynamicObject6 = dynamicObject["KeeperId"] as DynamicObject;
							if (dynamicObject3 == null)
							{
								dynamicObject3 = (base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject);
							}
							if (dynamicObject4 == null)
							{
								dynamicObject4 = (base.View.Model.GetValue("FOwnerIdHead") as DynamicObject);
							}
							if (text2.Equals("BD_KeeperOrg") && dynamicObject5 == null)
							{
								dynamicObject5 = (base.View.Model.GetValue("FStockOutOrgId") as DynamicObject);
							}
							if (text3.Equals("BD_KeeperOrg") && dynamicObject6 == null)
							{
								dynamicObject6 = (base.View.Model.GetValue("FStockOutOrgId") as DynamicObject);
							}
							for (int i = 0; i < enumerable.Count<DynamicObject>(); i++)
							{
								DynamicObject dynamicObject7 = enumerable.ElementAt(i)["MaterialId"] as DynamicObject;
								long masterId = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["MsterId"]);
								long num4 = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]);
								string text4 = Convert.ToString(enumerable.ElementAt(i)["RowId"]);
								if (num3 == num4)
								{
									DynamicObject dynamicObject8 = enumerable.ElementAt(i);
									long num5 = Convert.ToInt64(dynamicObject8["BOMId_Id"]);
									base.View.Model.SetValue("FBomEntryId", num5, num - 1);
									num++;
								}
								else
								{
									if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(enumerable.ElementAt(i)["BOMEntryId"]))
									{
										num2 = Convert.ToInt64(enumerable.ElementAt(i)["BomEntryId"]);
									}
									DynamicObjectCollection dynamicObjectCollection2 = dynamicObject7["MaterialStock"] as DynamicObjectCollection;
									long num6 = (dynamicObjectCollection2 == null) ? 0L : Convert.ToInt64(dynamicObjectCollection2[0]["StoreUnitID_Id"]);
									DynamicObject dynamicObject9 = enumerable.ElementAt(i)["BaseUnitId"] as DynamicObject;
									long sourceUnitId = (dynamicObject9 == null) ? 0L : Convert.ToInt64(dynamicObject9["Id"]);
									decimal num7 = Convert.ToDecimal(enumerable.ElementAt(i)["BaseQty"]);
									long num8 = Convert.ToInt64(enumerable.ElementAt(i)["AuxpropId_Id"]);
									UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, new GetUnitConvertRateArgs
									{
										MasterId = masterId,
										SourceUnitId = sourceUnitId,
										DestUnitId = num6
									});
									decimal num9 = unitConvertRate.ConvertQty(num7, "");
									base.View.Model.InsertEntryRow(entity.Key, num - 1);
									this.expandSuiteBySonMaterial = true;
									base.View.Model.SetValue("FRowType", "Son", num - 1);
									base.View.InvokeFieldUpdateService("FRowType", num - 1);
									base.View.Model.SetValue("FParentMatId", num3, num - 1);
									base.View.Model.SetValue("FParentRowId", sCurrProductRowId, num - 1);
									base.View.Model.SetValue("FRowId", text4, num - 1);
									base.View.Model.SetValue("FMaterialId", num4, num - 1);
									base.View.InvokeFieldUpdateService("FMaterialId", num - 1);
									base.View.Model.SetValue("FUnitId", num6, num - 1);
									base.View.Model.SetValue("FBomEntryId", num2, num - 1);
									if (num8 > 0L)
									{
										base.View.Model.SetValue("FAuxPropId", num8, num - 1);
									}
									long num10 = Convert.ToInt64(enumerable.ElementAt(i)["BomId_Id"]);
									if (num10 > 0L)
									{
										DynamicObject dynamicObject10 = base.View.Model.GetValue("FMaterialId", num - 1) as DynamicObject;
										if (dynamicObject10 != null && SCMCommon.CheckMaterialIsEnableBom(dynamicObject10))
										{
											base.View.Model.SetValue("FBOMID", num10, num - 1);
											base.View.InvokeFieldUpdateService("FBOMID", num - 1);
										}
									}
									base.View.Model.SetValue("FOwnerOutID", dynamicObject3, num - 1);
									base.View.Model.SetValue("FOwnerID", dynamicObject4, num - 1);
									base.View.Model.SetValue("FKeeperTypeOutId", text2, num - 1);
									base.View.Model.SetValue("FKeeperTypeId", text3, num - 1);
									base.View.Model.SetValue("FKeeperOutId", dynamicObject5, num - 1);
									base.View.Model.SetValue("FKeeperId", dynamicObject6, num - 1);
									base.View.Model.SetValue("FQty", num9, num - 1);
									base.View.InvokeFieldUpdateService("FQty", num - 1);
									num++;
								}
							}
						}
					}
				}
			}
			this.expandSuiteBySonMaterial = false;
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00061ED8 File Offset: 0x000600D8
		private void SetParentMatForSonEntry(int iRow, bool checkParentIsNull = true)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FRowType", iRow));
			string text2 = Convert.ToString(base.View.Model.GetValue("FParentRowId", iRow));
			if (StringUtils.EqualsIgnoreCase(text, "Son") && (!checkParentIsNull || (checkParentIsNull && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))))
			{
				long num = 0L;
				for (int i = iRow - 1; i >= 0; i--)
				{
					text = Convert.ToString(base.View.Model.GetValue("FRowType", i));
					if (StringUtils.EqualsIgnoreCase(text, "Parent"))
					{
						text2 = Convert.ToString(base.View.Model.GetValue("FRowId", i));
						DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
						num = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
						break;
					}
				}
				if (num > 0L)
				{
					base.View.Model.SetValue("FParentMatId", num, iRow);
					base.View.Model.SetValue("FParentRowId", text2, iRow);
				}
			}
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x00062010 File Offset: 0x00060210
		private void SetParentFieldValueToSon(string fieldKey, object fieldNewValue, int iRow)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FRowType", iRow));
			string text2 = Convert.ToString(base.View.Model.GetValue("FRowId", iRow));
			if (StringUtils.EqualsIgnoreCase(text, "Parent") && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
				for (int i = 0; i < entryRowCount; i++)
				{
					text = Convert.ToString(base.View.Model.GetValue("FRowType", i));
					if (StringUtils.EqualsIgnoreCase(text, "Son"))
					{
						string text3 = Convert.ToString(base.View.Model.GetValue("FParentRowId", i));
						if (StringUtils.EqualsIgnoreCase(text3, text2))
						{
							base.View.Model.SetValue(fieldKey, fieldNewValue, i);
						}
					}
				}
			}
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x00062144 File Offset: 0x00060344
		private void ChangeParentQtyForAllSonQty(DataChangedEventArgs e)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FRowType", e.Row));
			if (!StringUtils.EqualsIgnoreCase(text, "Parent"))
			{
				return;
			}
			string sRowId = Convert.ToString(base.View.Model.GetValue("FRowId", e.Row));
			if (!(base.View.Model.GetValue("FBOMId", e.Row) is DynamicObject))
			{
				base.View.ShowMessage(ResManager.LoadKDString("当前套件父项物料行没有BOM版本值（可能物料没有启用BOM管理）,不能自动联动计算套件子项数量！", "004072000032209", 5, new object[0]), 0);
				return;
			}
			if (StringUtils.EqualsIgnoreCase(text, "Parent"))
			{
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(e.Field.Entity);
				IEnumerable<DynamicObject> enumerable = from p in entityDataObject
				where StringUtils.EqualsIgnoreCase(Convert.ToString(p["ParentRowId"]), sRowId)
				select p;
				if (enumerable.Count<DynamicObject>() > 0)
				{
					DynamicObjectCollection bomexpandResult = this.GetBOMExpandResult(sRowId);
					if (bomexpandResult == null || bomexpandResult.Count < 1)
					{
						return;
					}
					IEnumerable<DynamicObject> enuResult = from p in bomexpandResult
					where StringUtils.EqualsIgnoreCase(Convert.ToString(p["SrcBillNo"]), sRowId)
					select p;
					if (bomexpandResult != null && bomexpandResult.Count > 0)
					{
						decimal num = 0m;
						foreach (DynamicObject dynamicObject in enumerable)
						{
							int num2 = Convert.ToInt32(dynamicObject["Seq"]);
							DynamicObject dySonMat = dynamicObject["MaterialId"] as DynamicObject;
							DynamicObject dySonStockUnit = dynamicObject["UnitId"] as DynamicObject;
							long sonbomEntryId = Convert.ToInt64(dynamicObject["BomEntryId"]);
							this.FindSonQtyBySonMat(enuResult, dySonMat, dySonStockUnit, out num, sonbomEntryId);
							base.View.Model.SetValue("FQty", num, num2 - 1);
							base.View.InvokeFieldUpdateService("FQty", num2 - 1);
						}
					}
				}
			}
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x00062384 File Offset: 0x00060584
		private bool FindSonQtyBySonMat(IEnumerable<DynamicObject> enuResult, DynamicObject dySonMat, DynamicObject dySonStockUnit, out decimal sonMatQty, long sonbomEntryId = 0L)
		{
			bool result = false;
			sonMatQty = 0m;
			if (enuResult == null || enuResult.Count<DynamicObject>() < 1 || dySonMat == null || dySonStockUnit == null)
			{
				return result;
			}
			long num = Convert.ToInt64(dySonMat["Id"]);
			long destUnitId = Convert.ToInt64(dySonStockUnit["Id"]);
			foreach (DynamicObject dynamicObject in enuResult)
			{
				DynamicObject dynamicObject2 = dynamicObject["MaterialId"] as DynamicObject;
				long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				long num3 = Convert.ToInt64(dynamicObject["BomEntryId"]);
				if (num == num2 && (sonbomEntryId <= 0L || sonbomEntryId == num3))
				{
					long masterId = Convert.ToInt64(dynamicObject2["MsterId"]);
					DynamicObject dynamicObject3 = dynamicObject["BaseUnitId"] as DynamicObject;
					long sourceUnitId = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
					decimal num4 = Convert.ToDecimal(dynamicObject["BaseQty"]);
					UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, new GetUnitConvertRateArgs
					{
						MasterId = masterId,
						SourceUnitId = sourceUnitId,
						DestUnitId = destUnitId
					});
					sonMatQty = unitConvertRate.ConvertQty(num4, "");
					result = true;
				}
			}
			return result;
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x00062578 File Offset: 0x00060778
		public static void DeleteEntryParentRowIncludeSonRow(IDynamicFormView currentView, BeforeDoOperationEventArgs e, string sEntryEntityKey)
		{
			int[] selectedRows = currentView.GetControl<EntryGrid>(sEntryEntityKey).GetSelectedRows();
			List<int> list = new List<int>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			if (selectedRows.Length > 0)
			{
				for (int i = 0; i < selectedRows.Length; i++)
				{
					string text = Convert.ToString(currentView.Model.GetValue("FRowType", selectedRows[i]));
					string text2 = Convert.ToString(currentView.Model.GetValue("FRowId", selectedRows[i]));
					if (StringUtils.EqualsIgnoreCase(text, "Parent") && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2) && !list2.Contains(text2))
					{
						list2.Add(text2);
					}
					else
					{
						list3.Add(text2);
					}
				}
			}
			if (list2.Count > 0)
			{
				if (StockTransferDirectEdit.deleteParentFlag)
				{
					Entity entity = currentView.BusinessInfo.GetEntity(sEntryEntityKey);
					DynamicObjectCollection entityDataObject = currentView.Model.GetEntityDataObject(entity);
					if (entityDataObject != null && entityDataObject.Count > 0)
					{
						foreach (DynamicObject dynamicObject in entityDataObject)
						{
							string item = Convert.ToString(dynamicObject["ParentRowId"]);
							string item2 = Convert.ToString(dynamicObject["RowId"]);
							int item3 = Convert.ToInt32(dynamicObject["Seq"]) - 1;
							if (!list.Contains(item3) && (list2.Contains(item) || list2.Contains(item2) || list3.Contains(item2)))
							{
								list.Add(item3);
							}
						}
					}
					OperateOptionUtils.SetEntryDeleteRowsOperation(e.Option, list.ToArray());
					StockTransferDirectEdit.deleteParentFlag = false;
					return;
				}
				if (!StockTransferDirectEdit.deleteParentFlag)
				{
					e.Cancel = true;
					string text3 = ResManager.LoadKDString("删除套件父项分录行，将会自动删除当前父项分录包含的所有套件子项分录，请确认", "004072000039976", 5, new object[0]);
					currentView.ShowWarnningMessage(text3, "", 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							StockTransferDirectEdit.deleteParentFlag = true;
							currentView.InvokeFormOperation(e.Operation.FormOperation.Operation);
							return;
						}
						StockTransferDirectEdit.deleteParentFlag = false;
						e.Cancel = true;
					}, 1);
				}
			}
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x000627E8 File Offset: 0x000609E8
		public void AfterCopyRowSetRowId(IDynamicFormView view, AfterCopyRowEventArgs e)
		{
			view.Model.SetValue("FRowId", Guid.NewGuid().ToString(), e.NewRow);
			if (!this._allowCopySuiteProduct && Convert.ToString(view.Model.GetValue("FRowType", e.Row)).Equals("Son"))
			{
				bool flag = false;
				DynamicObject dynamicObject = view.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
					if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0 && Convert.ToString(dynamicObjectCollection[0]["ErpClsID"]).Equals("6"))
					{
						flag = true;
					}
				}
				if (flag)
				{
					view.Model.SetValue("FRowType", "Service", e.NewRow);
				}
				else
				{
					view.Model.SetValue("FRowType", "Standard", e.NewRow);
				}
				view.Model.SetValue("FParentMatId", null, e.NewRow);
				view.Model.SetValue("FParentRowId", null, e.NewRow);
			}
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00062930 File Offset: 0x00060B30
		public void AfterCopyBillSetRowId(DynamicObjectCollection dycEntryData)
		{
			dycEntryData.Sort<int>((DynamicObject n) => Convert.ToInt32(n["Seq"]), null);
			Dictionary<long, string> dictionary = new Dictionary<long, string>();
			foreach (DynamicObject dynamicObject in dycEntryData)
			{
				string text = Convert.ToString(dynamicObject["RowType"]);
				string text2 = Guid.NewGuid().ToString();
				long num = Convert.ToInt64(dynamicObject["ParentMatId_Id"]);
				dynamicObject["RowId"] = text2;
				if (StringUtils.EqualsIgnoreCase(text, "Parent"))
				{
					long key = Convert.ToInt64(dynamicObject["MaterialId_Id"]);
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, text2);
					}
					else
					{
						dictionary[key] = text2;
					}
				}
				if (!this._allowCopySuiteProduct)
				{
					if (StringUtils.EqualsIgnoreCase(text, "Son"))
					{
						dynamicObject["RowType"] = "Standard";
						dynamicObject["ParentMatId"] = null;
						dynamicObject["ParentRowId"] = null;
					}
				}
				else if (StringUtils.EqualsIgnoreCase(text, "Son") && num > 0L && dictionary.ContainsKey(num))
				{
					dynamicObject["ParentRowId"] = dictionary[num];
				}
			}
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00062ABC File Offset: 0x00060CBC
		public bool ValidationsParentRowInfo(BeforeDoOperationEventArgs e)
		{
			bool result = true;
			if (this._allowCopySuiteProduct)
			{
				return result;
			}
			string text = e.Operation.FormOperation.Parmeter.OperationObjectKey.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return result;
			}
			int[] selectedRows = base.View.GetControl<EntryGrid>(text).GetSelectedRows();
			if (selectedRows == null || selectedRows.Count<int>() < 0)
			{
				return result;
			}
			EntryEntity entryEntity = (EntryEntity)base.View.BillBusinessInfo.GetEntity(text);
			foreach (int num in selectedRows)
			{
				DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, num);
				if (entityDataObject != null && (entityDataObject.DynamicObjectType.Properties.Contains("RowType") || entityDataObject.DynamicObjectType.Properties.Contains("MaterialId")))
				{
					if (entityDataObject.DynamicObjectType.Properties.Contains("RowType") && Convert.ToString(entityDataObject["RowType"]).Equals("Parent"))
					{
						result = false;
						break;
					}
					if (entityDataObject.DynamicObjectType.Properties.Contains("MaterialId"))
					{
						DynamicObject dynamicObject = entityDataObject["MaterialId"] as DynamicObject;
						if (dynamicObject != null)
						{
							DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
							if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0 && Convert.ToString(dynamicObjectCollection[0]["Suite"]).Equals("1"))
							{
								result = false;
								break;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x04000298 RID: 664
		private long defaultStock;

		// Token: 0x04000299 RID: 665
		private bool bflag;

		// Token: 0x0400029A RID: 666
		private bool isDoFillMaterial;

		// Token: 0x0400029B RID: 667
		private long lastAuxpropId;

		// Token: 0x0400029C RID: 668
		private bool para_UseCustMatMapping;

		// Token: 0x0400029D RID: 669
		private bool associatedCopyEntryRow;

		// Token: 0x0400029E RID: 670
		private bool bQueryStockReturn;

		// Token: 0x0400029F RID: 671
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();

		// Token: 0x040002A0 RID: 672
		private Dictionary<string, List<long>> bindSerialIds = new Dictionary<string, List<long>>();

		// Token: 0x040002A1 RID: 673
		private bool isFirstOpen = true;

		// Token: 0x040002A2 RID: 674
		private bool para_UseSuiteProduct;

		// Token: 0x040002A3 RID: 675
		private bool expandSuiteBySonMaterial;

		// Token: 0x040002A4 RID: 676
		private bool changeStockBaseQtyByMaterial;

		// Token: 0x040002A5 RID: 677
		private bool _allowCopySuiteProduct = true;

		// Token: 0x040002A6 RID: 678
		public static bool deleteParentFlag;

		// Token: 0x040002A7 RID: 679
		private bool isPickReturn;
	}
}

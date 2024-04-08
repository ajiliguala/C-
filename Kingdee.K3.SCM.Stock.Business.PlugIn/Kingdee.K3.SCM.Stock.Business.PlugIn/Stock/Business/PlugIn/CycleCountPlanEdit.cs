using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000067 RID: 103
	public class CycleCountPlanEdit : AbstractBillPlugIn
	{
		// Token: 0x06000466 RID: 1126 RVA: 0x00034A58 File Offset: 0x00032C58
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.Model.DataObject["AutoPlanLocaleId"] = base.View.Context.UserLocale.LCID;
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x00034A90 File Offset: 0x00032C90
		public override void AfterBindData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				this.SetStockOrg();
			}
			base.AfterBindData(e);
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x00034AB4 File Offset: 0x00032CB4
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FABCGroupId") as DynamicObject;
			Entity entity = base.View.Model.BillBusinessInfo.GetEntity("FCycleCountMat");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (dynamicObject == null && entityDataObject.Count == 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("ABC分配组编码和必盘物料不能同时为空！", "004023030002065", 5, new object[0]), ResManager.LoadKDString("ABC分配组编码和必盘物料不能同时为空！", "004023030002065", 5, new object[0]), 0);
				e.Cancel = true;
			}
			base.BeforeSave(e);
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00034B5A File Offset: 0x00032D5A
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			this.isCopying = false;
			base.AfterCopyData(e);
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00034B6A File Offset: 0x00032D6A
		public override void CopyData(CopyDataEventArgs e)
		{
			this.isCopying = true;
			base.CopyData(e);
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00034B7C File Offset: 0x00032D7C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			string text;
			if ((a = e.FieldKey.ToUpperInvariant()) != null && (a == "FSTOCKORGID" || a == "FMATERIALID") && this.GetFieldFilter(e.FieldKey, out text))
			{
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = text;
				}
				else
				{
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND" + text;
				}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00034C08 File Offset: 0x00032E08
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			string text;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null && (a == "FSTOCKORGID" || a == "FMATERIALID") && this.GetFieldFilter(e.BaseDataFieldKey, out text))
			{
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text;
				}
				else
				{
					e.Filter = e.Filter + " AND" + text;
				}
			}
			base.BeforeSetItemValueByNumber(e);
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x00034C84 File Offset: 0x00032E84
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FPERIODUNIT":
			case "FPERIOD":
				if (!this.isCalCycleDate)
				{
					this.CalCycleDate("FLastCycleDate");
				}
				this.CalculatePercent();
				break;
			case "FLASTCYCLEDATE":
				if (!this.isCalCycleDate)
				{
					this.CalCycleDate(e.Field.Key);
				}
				break;
			case "FTIMESPYEAR":
				this.CalculatePercent(e.Row);
				break;
			case "FABCGROUPID":
				this.SetCycleCountABC();
				this.SetCycleCountMat();
				break;
			case "FCONTROLTOL":
				this.ControlTol(e.Row);
				break;
			case "FMATCONTROLTOL":
				this.MatControlTol(e.Row);
				break;
			case "FSTOCKRANGE":
				this.ClearStock();
				break;
			}
			base.DataChanged(e);
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00034DD8 File Offset: 0x00032FD8
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBDELETEALLROW") && !(a == "TBDELETEALLROWMAT"))
				{
					if (!(a == "TBNEWENTRYMAT"))
					{
					}
				}
				else
				{
					base.View.Model.DeleteEntryData(e.ParentKey);
					base.View.Model.DeleteEntryData(e.ParentKey);
				}
			}
			base.AfterEntryBarItemClick(e);
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00034E50 File Offset: 0x00033050
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FMATERIALID"))
				{
					if (!(a == "FLAPSEDATE"))
					{
						if (a == "FNEXTCYCLEDATE")
						{
							this.CheckNextCycleDate(e);
						}
					}
					else
					{
						this.CheckLapseDate(e);
					}
				}
				else
				{
					this.CheckMaterialRepeat(e);
				}
			}
			base.BeforeUpdateValue(e);
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00034EB8 File Offset: 0x000330B8
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBGENERATECOUNTTABLE"))
				{
					if (!(a == "TBSTARTSERVICE"))
					{
						if (!(a == "TBSTOPSERVICE"))
						{
							if (!(a == "TBQUERYCOUNTTABLE"))
							{
								return;
							}
							this.QueryCountTable();
						}
						else
						{
							if (!Convert.ToBoolean(this.Model.GetValue("FIsAutoPlan")))
							{
								base.View.ShowMessage(ResManager.LoadKDString("当前盘点计划还未启用自动计划！", "004023030002077", 5, new object[0]), 0);
								return;
							}
							long[] array = new long[]
							{
								Convert.ToInt64(base.View.Model.GetPKValue())
							};
							StockServiceHelper.StartStopAutoCycleCountPlan(base.Context, array, false);
							base.View.ShowMessage(ResManager.LoadKDString("停用自动计划成功！", "004023030002080", 5, new object[0]), 0);
							base.View.Model.SetValue("FOperator", 0);
							base.View.Model.SetValue("FIsAutoPlan", false);
							return;
						}
					}
					else
					{
						if (this.Model.GetValue("FDocumentStatus").ToString() != "C")
						{
							base.View.ShowMessage(ResManager.LoadKDString("已审核单据才允许启用自动计划！", "004023030002068", 5, new object[0]), 0);
							return;
						}
						if (Convert.ToBoolean(this.Model.GetValue("FIsAutoPlan")))
						{
							base.View.ShowMessage(ResManager.LoadKDString("当前盘点计划已经处于自动化计划状态！", "004023030002071", 5, new object[0]), 0);
							return;
						}
						long[] array = new long[]
						{
							Convert.ToInt64(base.View.Model.GetPKValue())
						};
						StockServiceHelper.StartStopAutoCycleCountPlan(base.Context, array, true);
						base.View.ShowMessage(ResManager.LoadKDString("启用自动计划成功！", "004023030002074", 5, new object[0]), 0);
						base.View.Model.SetValue("FOperator", base.Context.UserId);
						base.View.Model.SetValue("FIsAutoPlan", true);
						return;
					}
				}
				else
				{
					DynamicObject[] array2 = new DynamicObject[]
					{
						base.View.Model.DataObject
					};
					OperateResultCollection operateResultCollection = new OperateResultCollection();
					StockServiceHelper.GenerateCycleCountTable(base.Context, array2, operateResultCollection);
					if (operateResultCollection.Count > 0)
					{
						if (operateResultCollection[0].SuccessStatus)
						{
							base.View.ShowMessage(operateResultCollection[0].Message, 0);
							return;
						}
						base.View.ShowErrMessage(operateResultCollection[0].Message, "", 0);
						return;
					}
				}
			}
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00035178 File Offset: 0x00033378
		private bool GetFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKORGID"))
				{
					if (a == "FMATERIALID")
					{
						if (!this.isCopying)
						{
							filter = " FISCYCLECOUNTING = '1' ";
						}
					}
				}
				else
				{
					filter = " FORGFUNCTIONS LIKE '%103%' AND FORGID IN (SELECT FORGID FROM T_BAS_SYSTEMPROFILE WHERE FCATEGORY = 'STK' AND FKEY = 'IsInvEndInitial')";
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x000351E0 File Offset: 0x000333E0
		private void QueryCountTable()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = "STK_CycleCountTable"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有物料盘点表的查看权限！", "004023030002083", 5, new object[0]), "", 0);
				return;
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_CycleCountTable";
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.CustomParams.Add("PlanIds", this.Model.DataObject["Id"].ToString());
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x000352D8 File Offset: 0x000334D8
		private void SetStockOrg()
		{
			long id = base.Context.CurrentOrganizationInfo.ID;
			bool flag = CommonServiceHelper.HaveStockInitCloseRecord(base.Context, id);
			if (base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L) && flag)
			{
				base.View.Model.SetValue("FStockOrgId", id);
			}
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x0003533C File Offset: 0x0003353C
		private void CalCycleDate(string sourceDateKey)
		{
			if (!(base.View.BusinessInfo.GetField(sourceDateKey) is DateField))
			{
				return;
			}
			this.isCalCycleDate = true;
			string text = string.Empty;
			object value = base.View.Model.GetValue("FPeriodUnit");
			if (value != null)
			{
				text = Convert.ToString(value);
			}
			int num = 0;
			object value2 = base.View.Model.GetValue("FPeriod");
			if (value2 != null)
			{
				num = Convert.ToInt32(value2);
			}
			string text2 = "FNextCycleDate";
			int num2 = 1;
			if (sourceDateKey.Equals("FNextCycleDate"))
			{
				text2 = "FLastCycleDate";
				num2 = -1;
			}
			num *= num2;
			object value3 = base.View.Model.GetValue(sourceDateKey);
			if (value3 != null)
			{
				DateTime dateTime = Convert.ToDateTime(value3);
				DateTime dateTime2 = TimeServiceHelper.GetSystemDateTime(base.View.Context);
				if (string.IsNullOrWhiteSpace(text))
				{
					base.View.Model.SetValue(text2, "");
					this.isCalCycleDate = false;
					return;
				}
				string a;
				if ((a = text) != null)
				{
					if (!(a == "0"))
					{
						if (!(a == "1"))
						{
							if (!(a == "2"))
							{
								if (!(a == "3"))
								{
									if (a == "4")
									{
										dateTime2 = dateTime.AddMonths(num * 3);
									}
								}
								else
								{
									dateTime2 = dateTime.AddMonths(num);
								}
							}
							else
							{
								dateTime2 = dateTime.AddDays((double)(num * 10));
							}
						}
						else
						{
							dateTime2 = dateTime.AddDays((double)(num * 7));
						}
					}
					else
					{
						dateTime2 = dateTime.AddDays((double)num);
					}
				}
				base.View.Model.SetValue(text2, dateTime2);
				this.isCalCycleDate = false;
			}
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x000354F0 File Offset: 0x000336F0
		private void CalculatePercent()
		{
			string sPeriodUnit = string.Empty;
			object value = base.View.Model.GetValue("FPeriodUnit");
			if (value != null)
			{
				sPeriodUnit = Convert.ToString(value);
			}
			decimal iPeriod = 0m;
			object value2 = base.View.Model.GetValue("FPeriod");
			if (value2 != null)
			{
				iPeriod = Convert.ToDecimal(value2);
			}
			int entryRowCount = base.View.Model.GetEntryRowCount("FCycleCountABC");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.CalculatingPercent(sPeriodUnit, iPeriod, i);
			}
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x0003557C File Offset: 0x0003377C
		private void CalculatePercent(int iRow)
		{
			string sPeriodUnit = string.Empty;
			object value = base.View.Model.GetValue("FPeriodUnit");
			if (value != null)
			{
				sPeriodUnit = Convert.ToString(value);
			}
			decimal iPeriod = 0m;
			object value2 = base.View.Model.GetValue("FPeriod");
			if (value2 != null)
			{
				iPeriod = Convert.ToDecimal(value2);
			}
			this.CalculatingPercent(sPeriodUnit, iPeriod, iRow);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x000355E0 File Offset: 0x000337E0
		private void CalculatingPercent(string sPeriodUnit, decimal iPeriod, int iRow)
		{
			decimal d = Convert.ToDecimal(base.View.Model.GetValue("FTimesPYear", iRow));
			decimal num = 0m;
			if (sPeriodUnit == "0")
			{
				num = d * iPeriod * 100m / 365m;
			}
			else if (sPeriodUnit == "1")
			{
				num = d * iPeriod * 100m / 52m;
			}
			else if (sPeriodUnit == "2")
			{
				num = d * iPeriod * 100m / 36m;
			}
			else if (sPeriodUnit == "3")
			{
				num = d * iPeriod * 100m / 12m;
			}
			else if (sPeriodUnit == "4")
			{
				num = d * iPeriod * 100m / 4m;
			}
			if (num > 100m)
			{
				num = 100m;
			}
			base.View.Model.SetValue("FPercent", num, iRow);
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00035734 File Offset: 0x00033934
		private void SetCycleCountABC()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FABCGroupId") as DynamicObject;
			long num = (dynamicObject != null) ? Convert.ToInt64(dynamicObject["Id"]) : 0L;
			List<STK_CycleCountPlanABC> list = StockServiceHelper.LoadABCGroupData(base.Context, num);
			base.View.Model.DeleteEntryData("FCycleCountABC");
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				STK_CycleCountPlanABC stk_CycleCountPlanABC = list[i];
				this.Model.CreateNewEntryRow("FCycleCountABC");
				base.View.Model.SetValue("FGroupEntryId", stk_CycleCountPlanABC.GroupEntryId, i);
				base.View.Model.SetValue("FGroupNo", stk_CycleCountPlanABC.GroupNo, i);
				base.View.Model.SetValue("FGroupName", stk_CycleCountPlanABC.GroupName, i);
				base.View.Model.SetValue("FTimesPYear", stk_CycleCountPlanABC.TimesPYear, i);
				base.View.Model.SetValue("FControlTol", stk_CycleCountPlanABC.ControlTol, i);
				base.View.Model.SetValue("FGainTol", stk_CycleCountPlanABC.GainTol, i);
				base.View.Model.SetValue("FLossTol", stk_CycleCountPlanABC.LossTol, i);
				base.View.Model.SetValue("FGropuNote", stk_CycleCountPlanABC.GropuNote, i);
			}
			base.View.UpdateView("FCycleCountABC");
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00035940 File Offset: 0x00033B40
		private void SetCycleCountMat()
		{
			CycleCountPlanEdit.<>c__DisplayClass4 CS$<>8__locals1 = new CycleCountPlanEdit.<>c__DisplayClass4();
			DynamicObject dynamicObject = base.View.Model.GetValue("FABCGroupId") as DynamicObject;
			long num = (dynamicObject != null) ? Convert.ToInt64(dynamicObject["Id"]) : 0L;
			ILookUpField lookUpField = base.View.BusinessInfo.GetField("FMaterialId") as BaseDataField;
			CS$<>8__locals1.listCycleCountMat = StockServiceHelper.LoadMatGroupData(base.Context, num);
			base.View.Model.DeleteEntryData("FCycleCountMat");
			int count = CS$<>8__locals1.listCycleCountMat.Count;
			if (count < 1 || lookUpField == null)
			{
				base.View.UpdateView("FCycleCountMat");
				return;
			}
			IEnumerable<string> source = from p in CS$<>8__locals1.listCycleCountMat
			select p.MaterialId.ToString();
			DynamicObject[] source2 = BusinessDataServiceHelper.LoadFromCache(base.Context, source.ToArray<string>(), lookUpField.RefFormDynamicObjectType);
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FCycleCountMat");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			int i;
			for (i = 0; i < count; i++)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
				dynamicObject2["Seq"] = i + 1;
				dynamicObject2["GroupNumber"] = CS$<>8__locals1.listCycleCountMat[i].GroupNo;
				dynamicObject2["MatGroupName"] = CS$<>8__locals1.listCycleCountMat[i].GroupName;
				dynamicObject2["MaterialId_Id"] = CS$<>8__locals1.listCycleCountMat[i].MaterialId;
				dynamicObject2["MaterialId"] = source2.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["Id"]) == CS$<>8__locals1.listCycleCountMat[i].MaterialId);
				dynamicObject2["MatControlTol"] = CS$<>8__locals1.listCycleCountMat[i].ControlTol;
				dynamicObject2["MatGainTol"] = CS$<>8__locals1.listCycleCountMat[i].GainTol;
				dynamicObject2["MatLossTol"] = CS$<>8__locals1.listCycleCountMat[i].LossTol;
				dynamicObject2["MatNote"] = CS$<>8__locals1.listCycleCountMat[i].GropuNote;
				entityDataObject.Add(dynamicObject2);
			}
			base.View.UpdateView("FCycleCountMat");
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x00035C44 File Offset: 0x00033E44
		private void CheckMaterialRepeat(BeforeUpdateValueEventArgs e)
		{
			Entity entity = base.View.Model.BillBusinessInfo.GetEntity("FCycleCountMat");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			long lMustMaterialId = (e.Value is DynamicObject) ? Convert.ToInt64(((DynamicObject)e.Value)["Id"]) : Convert.ToInt64(e.Value);
			if ((from p in entityDataObject
			where p["MaterialId"] != null && p["MaterialId_Id"].Equals(lMustMaterialId)
			select p).Count<DynamicObject>() > 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("物料编码重复，请重新输入!", "004023030002086", 5, new object[0]), ResManager.LoadKDString("物料编码重复，请重新输入!", "004023030002086", 5, new object[0]), 0);
				e.Cancel = true;
			}
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00035D18 File Offset: 0x00033F18
		private void ControlTol(int iRow)
		{
			object value = base.View.Model.GetValue("FControlTol", iRow);
			if (value == null || !Convert.ToBoolean(value))
			{
				base.View.Model.SetValue("FGainTol", 0, iRow);
				base.View.Model.SetValue("FLossTol", 0, iRow);
			}
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00035D84 File Offset: 0x00033F84
		private void MatControlTol(int iRow)
		{
			object value = base.View.Model.GetValue("FMatControlTol", iRow);
			if (value == null || !Convert.ToBoolean(value))
			{
				base.View.Model.SetValue("FMatGainTol", 0, iRow);
				base.View.Model.SetValue("FMatLossTol", 0, iRow);
			}
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x00035DF0 File Offset: 0x00033FF0
		private void SetMaterialLock()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FCycleCountMat");
			for (int i = 0; i < entryRowCount; i++)
			{
				object value = base.View.Model.GetValue("FGroupNumber", i);
				string value2 = Convert.ToString(value);
				if (!string.IsNullOrWhiteSpace(value2))
				{
					base.View.GetFieldEditor("FMaterialId", i).Enabled = false;
				}
				else
				{
					base.View.GetFieldEditor("FMaterialId", i).Enabled = true;
				}
			}
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00035E78 File Offset: 0x00034078
		private void CheckLapseDate(BeforeUpdateValueEventArgs e)
		{
			DateTime t = DateTime.Today;
			DateTime t2 = DateTime.Today;
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject != null) ? Convert.ToInt64(dynamicObject["Id"]) : 0L;
			object updateStockDate = StockServiceHelper.GetUpdateStockDate(base.Context, num);
			if (updateStockDate != null)
			{
				t = Convert.ToDateTime(updateStockDate);
			}
			object value = e.Value;
			if (value != null && !string.IsNullOrWhiteSpace(Convert.ToString(value)))
			{
				t2 = Convert.ToDateTime(value);
				if (t2 < t)
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("失效日期必须大于等于库存启用日期！", "004023030002089", 5, new object[0]), ResManager.LoadKDString("失效日期必须大于等于库存启用日期！", "004023030002089", 5, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x00035F48 File Offset: 0x00034148
		private void CheckNextCycleDate(BeforeUpdateValueEventArgs e)
		{
			DateTime t = DateTime.Today;
			object value = e.Value;
			if (value != null && !string.IsNullOrWhiteSpace(Convert.ToString(value)))
			{
				t = Convert.ToDateTime(value);
				if (t < DateTime.Today)
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("下次计划日期必须大于等于当前日期！", "004023030002092", 5, new object[0]), ResManager.LoadKDString("下次计划日期必须大于等于当前日期！", "004023030002092", 5, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x00035FC5 File Offset: 0x000341C5
		private void ClearStock()
		{
			base.View.Model.DeleteEntryData("FCycleCountStock");
			base.View.Model.CreateNewEntryRow("FCycleCountStock");
			base.View.UpdateView("FCycleCountStock");
		}

		// Token: 0x040001A6 RID: 422
		private bool isCalCycleDate;

		// Token: 0x040001A7 RID: 423
		private bool isCopying;
	}
}

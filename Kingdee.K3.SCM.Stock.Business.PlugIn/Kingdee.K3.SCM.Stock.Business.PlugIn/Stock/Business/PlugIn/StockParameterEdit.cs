using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A4 RID: 164
	public class StockParameterEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060009E3 RID: 2531 RVA: 0x00085DA0 File Offset: 0x00083FA0
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = e.Paramter.GetCustomParameter("OrgId");
			if (customParameter != null)
			{
				this._orgIdParam = Convert.ToInt64(customParameter);
			}
			else
			{
				this._orgIdParam = Convert.ToInt64(base.Context.CurrentOrganizationInfo.ID);
			}
			this.SetAcctParaSimplizationColumns();
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x00085DF8 File Offset: 0x00083FF8
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			object value = this.Model.GetValue("FPlusAndMinus");
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				this.Model.DataObject["PlusAndMinus"] = "3";
			}
			value = this.Model.GetValue("FOneZeroInv");
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				this.Model.DataObject["OneZeroInv"] = "2";
			}
			if (this.IsFromInitGuide())
			{
				object customParameter = this.View.ParentFormView.OpenParameter.GetCustomParameter("OrgId");
				if (customParameter != null && Convert.ToInt64(customParameter) > 0L)
				{
					this._orgIdParam = Convert.ToInt64(customParameter);
					this.View.ParentFormView.Model.SetValue("FOrgId", Convert.ToInt64(customParameter));
					this.View.ParentFormView.UpdateView("FOrgId");
					this.View.SendAynDynamicFormAction(this.View.ParentFormView);
					Dictionary<string, object> customParameters = this.View.ParentFormView.OpenParameter.GetCustomParameters();
					customParameters.Remove("OrgId");
				}
			}
			if (this._orgIdParam == 0L)
			{
				this.SetAcctParaData();
				this.SetSysParaData();
				return;
			}
			this.SetSysParaDefalutValue();
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x00085F54 File Offset: 0x00084154
		public override void AfterBindData(EventArgs e)
		{
			OrganizationServiceHelper.ReadOrgInfoByOrgId(base.Context, this._orgIdParam);
			bool flag = CommonServiceHelper.IsUseStock(base.Context, this._orgIdParam);
			if (flag)
			{
				this.View.StyleManager.SetEnabled("FUpdateStockPoint", "FUpdateStockPoint", false);
			}
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "RecBalMidData", false);
			bool flag2 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag2 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FRecBalMidData", flag2);
			this.View.UpdateView("FRecBalMidData");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "RecInvCheckMidData", false);
			bool flag3 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag3 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FRecInvCheckMidData", flag3);
			this.View.UpdateView("FRecInvCheckMidData");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "NoUseBrokenLot", false);
			bool flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FNoUseBrokenLot", flag4);
			this.View.UpdateView("FNoUseBrokenLot");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "HideMinusWarnMsg", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FHideMinusWarnMsg", flag4);
			this.View.UpdateView("FHideMinusWarnMsg");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ExpCheckNotUseOrg", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FExpCheckNotUseOrg", flag4);
			this.View.UpdateView("FExpCheckNotUseOrg");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "CleanReleaseLink", false);
			bool flag5 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag5 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FCleanReleaseLink", flag5);
			this.View.UpdateView("FCleanReleaseLink");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ForbidLocCheckInv", false);
			bool flag6 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag6 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("ForbidLocCheckInv", flag6);
			this.View.UpdateView("ForbidLocCheckInv");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "IgnoreZeroSecQty", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FIgnoreZeroSecQty", flag4);
			this.View.UpdateView("FIgnoreZeroSecQty");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FRemoveFlexValuesCache", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FRemoveFlexValuesCache", flag4);
			this.View.UpdateView("FRemoveFlexValuesCache");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FFlexValuesDetailMaxPageRows", 10000);
			int num = 10000;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				int.TryParse(systemProfile.ToString(), out num);
				if (num <= 0)
				{
					num = 10000;
				}
				else if (num > 5000000)
				{
					num = 5000000;
				}
			}
			this.View.Model.SetValue("FFlexValuesDetailMaxPageRows", num);
			this.View.UpdateView("FFlexValuesDetailMaxPageRows");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FStkCountinputMaxRows", 10000);
			num = 10000;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				int.TryParse(systemProfile.ToString(), out num);
				if (num <= 0)
				{
					num = 10000;
				}
				else if (num > 500000)
				{
					num = 500000;
				}
			}
			this.View.Model.SetValue("FStkCountinputMaxRows", num);
			this.View.UpdateView("FStkCountinputMaxRows");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FListSortNotBySeq", "");
			this.View.Model.SetValue("FListSortNotBySeq", (systemProfile == null) ? "" : systemProfile.ToString());
			this.View.UpdateView("FListSortNotBySeq");
			if (this._orgIdParam != 0L)
			{
				this.View.StyleManager.SetEnabled("FRecBalMidData", "FRecBalMidData", false);
				this.View.StyleManager.SetEnabled("FRecInvCheckMidData", "FRecInvCheckMidData", false);
				this.View.StyleManager.SetEnabled("FNoUseBrokenLot", "FNoUseBrokenLot", false);
				this.View.StyleManager.SetEnabled("FHideMinusWarnMsg", "FHideMinusWarnMsg", false);
				this.View.StyleManager.SetEnabled("FExpCheckNotUseOrg", "FExpCheckNotUseOrg", false);
				this.View.StyleManager.SetEnabled("FCleanReleaseLink", "FCleanReleaseLink", false);
				this.View.StyleManager.SetEnabled("ForbidLocCheckInv", "ForbidLocCheckInv", false);
				this.View.StyleManager.SetEnabled("FIgnoreZeroSecQty", "FIgnoreZeroSecQty", false);
				this.View.StyleManager.SetEnabled("FRemoveFlexValuesCache", "FRemoveFlexValuesCache", false);
				this.View.StyleManager.SetEnabled("FFlexValuesDetailMaxPageRows", "FFlexValuesDetailMaxPageRows", false);
				this.View.StyleManager.SetEnabled("FStkCountinputMaxRows", "FStkCountinputMaxRows", false);
				this.View.StyleManager.SetEnabled("FListSortNotBySeq", "FListSortNotBySeq", false);
			}
			else
			{
				this.View.StyleManager.SetEnabled("FRecBalMidData", "FRecBalMidData", true);
				this.View.StyleManager.SetEnabled("FRecInvCheckMidData", "FRecInvCheckMidData", true);
				this.View.StyleManager.SetEnabled("FNoUseBrokenLot", "FNoUseBrokenLot", true);
				this.View.StyleManager.SetEnabled("FHideMinusWarnMsg", "FHideMinusWarnMsg", true);
				this.View.StyleManager.SetEnabled("FExpCheckNotUseOrg", "FExpCheckNotUseOrg", true);
				this.View.StyleManager.SetEnabled("FCleanReleaseLink", "FCleanReleaseLink", true);
				this.View.StyleManager.SetEnabled("ForbidLocCheckInv", "ForbidLocCheckInv", true);
				this.View.StyleManager.SetEnabled("FIgnoreZeroSecQty", "FIgnoreZeroSecQty", true);
				this.View.StyleManager.SetEnabled("FRemoveFlexValuesCache", "FRemoveFlexValuesCache", true);
				this.View.StyleManager.SetEnabled("FFlexValuesDetailMaxPageRows", "FFlexValuesDetailMaxPageRows", true);
				this.View.StyleManager.SetEnabled("FStkCountinputMaxRows", "FStkCountinputMaxRows", true);
				this.View.StyleManager.SetEnabled("FListSortNotBySeq", "FListSortNotBySeq", true);
			}
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ControlSerialNo", 1);
			this.View.Model.SetValue("FControlSerialNo", (systemProfile == null) ? 1 : systemProfile);
			this.View.UpdateView("FControlSerialNo");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "SerialManageLevel", "O");
			this.View.Model.SetValue("FSerialManageLevel", (systemProfile == null) ? "O" : systemProfile);
			this.View.UpdateView("FSerialManageLevel");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "NoCheckSerialInput", false);
			bool flag7 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag7 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FNoCheckSerialInput", flag7);
			this.View.UpdateView("FNoCheckSerialInput");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "SerialNoAutoScan", false);
			bool flag8 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag8 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FSerialNoAutoScan", flag8);
			this.View.UpdateView("FSerialNoAutoScan");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "CheckEmptyMto", false);
			bool flag9 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag9 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FCheckEmptyMto", flag9);
			this.View.UpdateView("FCheckEmptyMto");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "MatchEmptyMto", false);
			bool flag10 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag10 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FMatchEmptyMto", flag10);
			this.View.UpdateView("FMatchEmptyMto");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ListPermitSN", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FListPermitSN", flag4);
			this.View.UpdateView("FListPermitSN");
			systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "IsExcdSavedSerials", false);
			flag4 = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag4 = Convert.ToBoolean(systemProfile);
			}
			this.View.Model.SetValue("FIsExcdSavedSerials", flag4);
			this.View.UpdateView("FIsExcdSavedSerials");
			if (Convert.ToString(this.View.Model.GetValue("FControlSerialNo")) == "True")
			{
				this.View.StyleManager.SetEnabled("FSerialManageLevel", "FSerialManageLevel", true);
				this.View.StyleManager.SetEnabled("FNoCheckSerialInput", "FNoCheckSerialInput", true);
				this.View.StyleManager.SetEnabled("FSerialNoAutoScan", "FSerialNoAutoScan", true);
				this.View.StyleManager.SetEnabled("FCheckEmptyMto", "FCheckEmptyMto", true);
				this.View.StyleManager.SetEnabled("FListPermitSN", "FListPermitSN", true);
				this.View.StyleManager.SetEnabled("FIsExcdSavedSerials", "FIsExcdSavedSerials", true);
				if (Convert.ToString(this.View.Model.GetValue("FCheckEmptyMto")) == "True")
				{
					this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", true);
				}
				else
				{
					this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
				}
			}
			else
			{
				this.View.StyleManager.SetEnabled("FSerialManageLevel", "FSerialManageLevel", false);
				this.View.StyleManager.SetEnabled("FNoCheckSerialInput", "FNoCheckSerialInput", false);
				this.View.StyleManager.SetEnabled("FSerialNoAutoScan", "FSerialNoAutoScan", false);
				this.View.StyleManager.SetEnabled("FCheckEmptyMto", "FCheckEmptyMto", false);
				this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
				this.View.StyleManager.SetEnabled("FListPermitSN", "FListPermitSN", false);
				this.View.StyleManager.SetEnabled("FIsExcdSavedSerials", "FIsExcdSavedSerials", false);
			}
			if (this._orgIdParam != 0L)
			{
				this.View.StyleManager.SetEnabled("FSerialManageLevel", "FSerialManageLevel", false);
				this.View.StyleManager.SetEnabled("FControlSerialNo", "FControlSerialNo", false);
				this.View.StyleManager.SetEnabled("FNoCheckSerialInput", "FNoCheckSerialInput", false);
				this.View.StyleManager.SetEnabled("FSerialNoAutoScan", "FSerialNoAutoScan", false);
				this.View.StyleManager.SetEnabled("FCheckEmptyMto", "FCheckEmptyMto", false);
				this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
				this.View.StyleManager.SetEnabled("FListPermitSN", "FListPermitSN", false);
				this.View.StyleManager.SetEnabled("FIsExcdSavedSerials", "FIsExcdSavedSerials", false);
			}
			else
			{
				object value = this.Model.GetValue("FSerialManageLevel", -1);
				this.oldSnLevel = ((value == null) ? "" : value.ToString());
				this._haveSNMaster = CommonServiceHelper.HaveSerialMaster(base.Context);
				this.View.StyleManager.SetEnabled("FControlSerialNo", "FControlSerialNo", !this._haveSNMaster);
			}
			if (Convert.ToString(this.View.Model.GetValue("FEnableDateControl")) == "True")
			{
				this.View.StyleManager.SetEnabled("FControlPoint", "FControlPoint", true);
				this.View.StyleManager.SetEnabled("FAllowBeforeDays", "FAllowBeforeDays", true);
				this.View.StyleManager.SetEnabled("FAllowAfterDays", "FAllowAfterDays", true);
				this.View.StyleManager.SetEnabled("FComSaveDate", "FComSaveDate", true);
				this.View.StyleManager.SetEnabled("FComSubmitDate", "FComSubmitDate", true);
				this.View.StyleManager.SetEnabled("FIgnoreReAppStatus", "FIgnoreReAppStatus", true);
			}
			else
			{
				this.View.StyleManager.SetEnabled("FControlPoint", "FControlPoint", false);
				this.View.StyleManager.SetEnabled("FAllowBeforeDays", "FAllowBeforeDays", false);
				this.View.StyleManager.SetEnabled("FAllowAfterDays", "FAllowAfterDays", false);
				this.View.StyleManager.SetEnabled("FComSaveDate", "FComSaveDate", false);
				this.View.StyleManager.SetEnabled("FComSubmitDate", "FComSubmitDate", false);
				this.View.StyleManager.SetEnabled("FIgnoreReAppStatus", "FIgnoreReAppStatus", false);
				this.View.Model.SetValue("FAllowBeforeDays", 0);
				this.View.Model.SetValue("FAllowAfterDays", 0);
			}
			this.BindFlexSplit("fzsx");
			this.BindFlexSplit("cw");
			string text = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FlexSplit", "").ToString().Trim();
			string text2 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "AuxPropSplit", "").ToString().Trim();
			this.Model.SetValue("FFlexSplit", string.IsNullOrWhiteSpace(text) ? this._flexSplitDefaultValue : text);
			this.Model.SetValue("FAuxPropSplit", string.IsNullOrWhiteSpace(text2) ? this._flexSplitDefaultValue : text2);
			this.View.UpdateView("FFlexSplit");
			this.View.UpdateView("FAuxPropSplit");
			string text3 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FlexFormatter", "").ToString().Trim();
			string text4 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "AuxPropFormatter", "").ToString().Trim();
			this.Model.SetValue("FFlexFormatter", string.IsNullOrWhiteSpace(text3) ? "2,4" : text3);
			this.Model.SetValue("FAuxPropFormatter", string.IsNullOrWhiteSpace(text4) ? "2,4" : text4);
			this.View.UpdateView("FFlexFormatter");
			this.View.UpdateView("FAuxPropFormatter");
			string text5 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FInvTurnOverShowAMField", "True").ToString().Trim();
			this.Model.SetValue("FInvTurnOverShowAMField", text5);
			this.View.UpdateView("FInvTurnOverShowAMField");
			string text6 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FWebApiAuxBomID", "False").ToString().Trim();
			this.Model.SetValue("FWebApiAuxBomID", text6);
			this.View.UpdateView("FWebApiAuxBomID");
			string text7 = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FShelfLiftAlarmSelfMsg", "False").ToString().Trim();
			this.Model.SetValue("FShelfLiftAlarmSelfMsg", text7);
			this.View.UpdateView("FShelfLiftAlarmSelfMsg");
			if (this._orgIdParam != 0L)
			{
				this.View.StyleManager.SetEnabled("FFlexSplit", "FFlexSplit", false);
				this.View.StyleManager.SetEnabled("FFlexFormatter", "FFlexFormatter", false);
				this.View.StyleManager.SetEnabled("FAuxPropFormatter", "FAuxPropFormatter", false);
				this.View.StyleManager.SetEnabled("FAuxPropSplit", "FAuxPropSplit", false);
			}
			this.SetAcctParaControl();
			this.SetSysParaControl();
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x000871F0 File Offset: 0x000853F0
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FSERIALMANAGELEVEL"))
				{
					return;
				}
				if (!this._haveSNMaster)
				{
					return;
				}
				string text = (e.Value == null) ? "" : e.Value.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "A";
				}
				if ((this.oldSnLevel == "M" && (text == "A" || text == "O")) || (this.oldSnLevel == "O" && text == "A"))
				{
					this.View.ShowMessage(ResManager.LoadKDString("系统中已经存在序列号主档，不允许放宽序列号唯一性范围！", "004023030006389", 5, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x000872C4 File Offset: 0x000854C4
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FCONTROLSERIALNO"))
				{
					if (!(a == "FCHECKEMPTYMTO"))
					{
						if (!(a == "FENABLEDATECONTROL"))
						{
							return;
						}
						if (e.NewValue.ToString() == "True")
						{
							this.View.StyleManager.SetEnabled("FControlPoint", "FControlPoint", true);
							this.View.StyleManager.SetEnabled("FAllowBeforeDays", "FAllowBeforeDays", true);
							this.View.StyleManager.SetEnabled("FAllowAfterDays", "FAllowAfterDays", true);
							this.View.StyleManager.SetEnabled("FComSaveDate", "FComSaveDate", true);
							this.View.StyleManager.SetEnabled("FComSubmitDate", "FComSubmitDate", true);
							this.View.StyleManager.SetEnabled("FIgnoreReAppStatus", "FIgnoreReAppStatus", true);
							return;
						}
						this.View.StyleManager.SetEnabled("FControlPoint", "FControlPoint", false);
						this.View.StyleManager.SetEnabled("FAllowBeforeDays", "FAllowBeforeDays", false);
						this.View.StyleManager.SetEnabled("FAllowAfterDays", "FAllowAfterDays", false);
						this.View.StyleManager.SetEnabled("FComSaveDate", "FComSaveDate", false);
						this.View.StyleManager.SetEnabled("FComSubmitDate", "FComSubmitDate", false);
						this.View.StyleManager.SetEnabled("FIgnoreReAppStatus", "FIgnoreReAppStatus", false);
						this.View.Model.SetValue("FAllowBeforeDays", 0);
						this.View.Model.SetValue("FAllowAfterDays", 0);
					}
					else
					{
						if (e.NewValue.ToString() == "True")
						{
							this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", true);
							return;
						}
						this.View.Model.SetValue("FMatchEmptyMto", false);
						this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
						return;
					}
				}
				else
				{
					if (!(e.NewValue.ToString() == "True"))
					{
						this.View.StyleManager.SetEnabled("FSerialManageLevel", "FSerialManageLevel", false);
						this.View.StyleManager.SetEnabled("FNoCheckSerialInput", "FNoCheckSerialInput", false);
						this.View.StyleManager.SetEnabled("FSerialNoAutoScan", "FSerialNoAutoScan", false);
						this.View.StyleManager.SetEnabled("FCheckEmptyMto", "FCheckEmptyMto", false);
						this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
						this.View.StyleManager.SetEnabled("FListPermitSN", "FListPermitSN", false);
						this.View.StyleManager.SetEnabled("FIsExcdSavedSerials", "FIsExcdSavedSerials", false);
						return;
					}
					this.View.StyleManager.SetEnabled("FSerialManageLevel", "FSerialManageLevel", true);
					this.View.StyleManager.SetEnabled("FNoCheckSerialInput", "FNoCheckSerialInput", true);
					this.View.StyleManager.SetEnabled("FSerialNoAutoScan", "FSerialNoAutoScan", true);
					this.View.StyleManager.SetEnabled("FCheckEmptyMto", "FCheckEmptyMto", true);
					this.View.StyleManager.SetEnabled("FListPermitSN", "FListPermitSN", true);
					this.View.StyleManager.SetEnabled("FIsExcdSavedSerials", "FIsExcdSavedSerials", true);
					if (Convert.ToString(this.View.Model.GetValue("FCheckEmptyMto")) == "True")
					{
						this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", true);
						return;
					}
					this.View.StyleManager.SetEnabled("FMatchEmptyMto", "FMatchEmptyMto", false);
					return;
				}
			}
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x000876F0 File Offset: 0x000858F0
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(e.ParentKey);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBACCTPARADELETEROW"))
				{
					if (!(a == "TBMOVEUP"))
					{
						if (!(a == "TBMOVEDOWN"))
						{
							return;
						}
						if (entryCurrentRowIndex < 0 || entryCurrentRowIndex >= this.View.Model.GetEntryRowCount(e.ParentKey))
						{
							return;
						}
						this.SetSequence(entryCurrentRowIndex, entryCurrentRowIndex + 1, e);
					}
					else
					{
						if (entryCurrentRowIndex < 0 || entryCurrentRowIndex >= this.View.Model.GetEntryRowCount(e.ParentKey))
						{
							return;
						}
						this.SetSequence(entryCurrentRowIndex, entryCurrentRowIndex - 1, e);
						return;
					}
				}
				else if (Convert.ToBoolean(this.Model.GetValue("FIsSysSet", entryCurrentRowIndex)))
				{
					this.View.ShowMessage(ResManager.LoadKDString("系统预置的逻辑校验，不允许删除！", "004023030009600", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x000877E8 File Offset: 0x000859E8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (a == "FRECOVERDEFVAL")
				{
					this.SetSysParaDefalutValue();
					return;
				}
				if (!(a == "FOEMFIELDCONVERTSET"))
				{
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "STK_OEMSOURFIELDSET";
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.PageId = SequentialGuid.NewGuid().ToString();
				dynamicFormShowParameter.OpenStyle.ShowType = 5;
				this.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x00087880 File Offset: 0x00085A80
		public void BindFlexSplit(string flexNumber)
		{
			List<EnumItem> list = new List<EnumItem>();
			EnumItem item = new EnumItem(new DynamicObject(EnumItem.EnumItemType))
			{
				EnumId = this._flexSplitDefaultValue,
				Value = this._flexSplitDefaultValue,
				Caption = new LocaleValue(this._flexSplitDefaultValue, base.Context.UserLocale.LCID)
			};
			list.Add(item);
			item = new EnumItem(new DynamicObject(EnumItem.EnumItemType))
			{
				EnumId = this._flexSplitDotValue,
				Value = this._flexSplitDotValue,
				Caption = new LocaleValue(this._flexSplitDotValue, base.Context.UserLocale.LCID)
			};
			list.Add(item);
			if (this._flexSplit == null)
			{
				this._flexSplit = new Dictionary<string, string>();
				DynamicObjectCollection flexSplit = CommonServiceHelper.GetFlexSplit(base.Context, new List<string>
				{
					"cw",
					"fzsx"
				});
				if (flexSplit != null && flexSplit.Count<DynamicObject>() > 0)
				{
					foreach (DynamicObject dynamicObject in flexSplit)
					{
						this._flexSplit.Add(Convert.ToString(dynamicObject["FNUMBER"]), Convert.ToString(dynamicObject["FSEPARATOR"]));
					}
				}
			}
			if (this._flexSplit.ContainsKey(flexNumber) && !this._flexSplitDotValue.Equals(this._flexSplit[flexNumber]) && !this._flexSplitDefaultValue.Equals(this._flexSplit[flexNumber]))
			{
				EnumItem item2 = new EnumItem(new DynamicObject(EnumItem.EnumItemType))
				{
					EnumId = this._flexSplit[flexNumber],
					Value = this._flexSplit[flexNumber],
					Caption = new LocaleValue(this._flexSplit[flexNumber], base.Context.UserLocale.LCID)
				};
				list.Add(item2);
			}
			string text = (flexNumber == "cw") ? "FFlexSplit" : "FAuxPropSplit";
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>(text, 0);
			fieldEditor.SetComboItems(list);
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x00087AD8 File Offset: 0x00085CD8
		private bool IsFromInitGuide()
		{
			return this.View.ParentFormView.ParentFormView != null && this.View.ParentFormView.ParentFormView.OpenParameter.FormId == "STK_OperateGuide" && this.View.ParentFormView.OpenParameter.GetCustomParameter("OrgId") != null;
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x00087B40 File Offset: 0x00085D40
		private void SetAcctParaSimplizationColumns()
		{
			List<string> list = new List<string>();
			EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity("FAccountParaEntity");
			foreach (Field field in entryEntity.Fields)
			{
				list.Add(field.Key);
			}
			EntryGrid control = this.View.GetControl<EntryGrid>("FAccountParaEntity");
			control.SetSimplizationColumns(list);
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x00087BE0 File Offset: 0x00085DE0
		private void SetAcctParaData()
		{
			BusinessInfo businessInfo = (MetaDataServiceHelper.Load(this.View.Context, "STK_AccountParaPlugIns", true) as FormMetadata).BusinessInfo;
			DynamicObject[] source = BusinessDataServiceHelper.Load(this.View.Context, businessInfo, null, null);
			this.SetAcctDefValue((from p in source
			orderby p["Seq"]
			select p).ToList<DynamicObject>());
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x00087C50 File Offset: 0x00085E50
		private void SetAcctParaControl()
		{
			this.SetAcctDefEnabled();
			this.SetAcctDefVisibled();
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x00087C60 File Offset: 0x00085E60
		private void SetAcctDefValue(List<DynamicObject> datas)
		{
			EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity("FAccountParaEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (this._orgIdParam == 0L && datas != null && datas.Count<DynamicObject>() > 0)
			{
				int num = 1;
				foreach (DynamicObject dynamicObject in datas)
				{
					DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
					dynamicObject2["Seq"] = num++;
					dynamicObject2["Id"] = dynamicObject["Id"];
					dynamicObject2["SelfTopClassId_Id"] = dynamicObject["SelfTopClassId_Id"];
					dynamicObject2["SelfTopClassId"] = dynamicObject["SelfTopClassId"];
					dynamicObject2["VerifyDescription"] = dynamicObject["VerifyDescription"];
					dynamicObject2["VerifiPlugIn"] = dynamicObject["VerifiPlugIn"];
					dynamicObject2["IsEnable"] = dynamicObject["IsEnable"];
					dynamicObject2["IsSysSet"] = dynamicObject["IsSysSet"];
					entityDataObject.Add(dynamicObject2);
				}
			}
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x00087DCC File Offset: 0x00085FCC
		private void SetAcctDefEnabled()
		{
			if (this._orgIdParam != 0L)
			{
				this.View.StyleManager.SetEnabled("FSelfTopClassId", "FSelfTopClassId", false);
				this.View.StyleManager.SetEnabled("FVerifyDescription", "FVerifyDescription", false);
				this.View.StyleManager.SetEnabled("FVerifiPlugIn", "FVerifiPlugIn", false);
				this.View.StyleManager.SetEnabled("FIsEnable", "FIsEnable", false);
				this.View.StyleManager.SetEnabled("FIsSysSet", "FIsSysSet", false);
				this.View.GetBarItem("FAccountParaEntity", "tbAcctParaNewRow").Enabled = false;
				this.View.GetBarItem("FAccountParaEntity", "tbAcctParaDeleteRow").Enabled = false;
				this.View.GetBarItem("FAccountParaEntity", "tbMoveUp").Enabled = false;
				this.View.GetBarItem("FAccountParaEntity", "tbMoveDown").Enabled = false;
			}
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x00087ED9 File Offset: 0x000860D9
		private void SetAcctDefVisibled()
		{
			if (this._orgIdParam != 0L)
			{
				this.View.GetControl("FTab_Account").Visible = false;
				return;
			}
			this.View.GetControl("FTab_Account").Visible = true;
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x00087F14 File Offset: 0x00086114
		private void SetAcctSysSetEnabled(int row, bool enabled)
		{
			this.View.GetFieldEditor("FSelfTopClassId", row).Enabled = enabled;
			this.View.GetFieldEditor("FVerifyDescription", row).Enabled = enabled;
			this.View.GetFieldEditor("FVerifiPlugIn", row).Enabled = enabled;
			this.View.GetFieldEditor("FIsSysSet", row).Enabled = enabled;
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x00087F80 File Offset: 0x00086180
		private void SetSequence(int curIndex, int newIndex, BarItemClickEventArgs e)
		{
			EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity(e.ParentKey);
			int entryRowCount = this.View.Model.GetEntryRowCount(e.ParentKey);
			if (newIndex < 0)
			{
				newIndex = 0;
			}
			else if (newIndex >= entryRowCount)
			{
				newIndex = entryRowCount - 1;
			}
			if (curIndex != newIndex)
			{
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
				DynamicObject dynamicObject = entityDataObject[curIndex];
				DynamicObject dynamicObject2 = entityDataObject[newIndex];
				dynamicObject["Seq"] = newIndex + 1;
				dynamicObject2["Seq"] = curIndex + 1;
				this.View.UpdateView(e.ParentKey);
				this.View.SetEntityFocusRow(e.ParentKey, newIndex);
				this.View.GetControl<EntryGrid>(e.ParentKey).SetFocusRowIndex(newIndex);
			}
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x00088058 File Offset: 0x00086258
		private void SetSysParaData()
		{
			List<string> list = new List<string>
			{
				"MAXSERIALCOUNT",
				"MAXROWLIMITSERIALCOUNT",
				"MAXGETINVSTOCKROWS",
				"IgnoreFlexValueCountCheck",
				"StopDelTranInvMInusData"
			};
			Dictionary<string, object> sysTempRofileConfig = CommonServiceHelper.GetSysTempRofileConfig(base.Context, 0L, "STK", list);
			if (sysTempRofileConfig != null && sysTempRofileConfig.Count<KeyValuePair<string, object>>() > 0)
			{
				using (List<string>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						if (sysTempRofileConfig.ContainsKey(text))
						{
							string a;
							if ((a = text) != null && (a == "IgnoreFlexValueCountCheck" || a == "StopDelTranInvMInusData"))
							{
								this.Model.DataObject[text] = sysTempRofileConfig[text].Equals("1");
							}
							else
							{
								this.Model.DataObject[text] = sysTempRofileConfig[text];
							}
						}
						else
						{
							this.SetFieldDefalutValue("F" + text);
						}
					}
					return;
				}
			}
			this.SetSysParaDefalutValue();
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x00088198 File Offset: 0x00086398
		private void SetSysParaControl()
		{
			this.SetSysDefEnabled();
			this.SetSysDefVisibled();
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x000881A8 File Offset: 0x000863A8
		private void SetSysDefEnabled()
		{
			if (this._orgIdParam != 0L)
			{
				this.View.StyleManager.SetEnabled("FMAXSERIALCOUNT", "FMAXSERIALCOUNT", false);
				this.View.StyleManager.SetEnabled("FMAXROWLIMITSERIALCOUNT", "FMAXROWLIMITSERIALCOUNT", false);
				this.View.StyleManager.SetEnabled("FMAXGETINVSTOCKROWS", "FMAXGETINVSTOCKROWS", false);
				this.View.StyleManager.SetEnabled("FIgnoreFlexValueCountCheck", "FIgnoreFlexValueCountCheck", false);
				this.View.StyleManager.SetEnabled("FStopDelTranInvMInusData", "FStopDelTranInvMInusData", false);
				this.View.StyleManager.SetEnabled("FTransOutReportShowAmountField", "FTransOutReportShowAmountField", false);
				this.View.StyleManager.SetEnabled("FInvTurnOverShowAMField", "FInvTurnOverShowAMField", false);
				this.View.StyleManager.SetEnabled("FWebApiAuxBomID", "FWebApiAuxBomID", false);
				this.View.StyleManager.SetEnabled("FShelfLiftAlarmSelfMsg", "FShelfLiftAlarmSelfMsg", false);
			}
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x000882B5 File Offset: 0x000864B5
		private void SetSysDefVisibled()
		{
			if (this._orgIdParam != 0L)
			{
				this.View.GetControl("FTab_SysPara").Visible = false;
				return;
			}
			this.View.GetControl("FTab_SysPara").Visible = true;
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x000882F0 File Offset: 0x000864F0
		private void SetSysParaDefalutValue()
		{
			this.SetFieldDefalutValue("FMAXSERIALCOUNT");
			this.SetFieldDefalutValue("FMAXROWLIMITSERIALCOUNT");
			this.SetFieldDefalutValue("FMAXGETINVSTOCKROWS");
			this.SetFieldDefalutValue("FIgnoreFlexValueCountCheck");
			this.SetFieldDefalutValue("FStopDelTranInvMInusData");
			this.SetFieldDefalutValue("FTransOutReportShowAmountField");
			this.SetFieldDefalutValue("FShelfLiftAlarmSelfMsg");
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0008834C File Offset: 0x0008654C
		private void SetFieldDefalutValue(string key)
		{
			Field field = this.View.BillBusinessInfo.GetField(key);
			if (field != null)
			{
				string text = (field.DefValue == null) ? "" : field.DefValue.Value;
				this.Model.SetValue(key, text);
			}
		}

		// Token: 0x040003F4 RID: 1012
		private const string STOCKFUNCTIONID = "103";

		// Token: 0x040003F5 RID: 1013
		private long _orgIdParam;

		// Token: 0x040003F6 RID: 1014
		private bool _haveSNMaster;

		// Token: 0x040003F7 RID: 1015
		private string oldSnLevel = "";

		// Token: 0x040003F8 RID: 1016
		private Dictionary<string, string> _flexSplit;

		// Token: 0x040003F9 RID: 1017
		private readonly string _flexSplitDefaultValue = ";";

		// Token: 0x040003FA RID: 1018
		private readonly string _flexSplitDotValue = ".";
	}
}

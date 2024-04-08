using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000054 RID: 84
	public class InvJoinQueryEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060003A4 RID: 932 RVA: 0x0002B770 File Offset: 0x00029970
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.usePLNReserve = CommonServiceHelper.IsUsePLNReserve(this.View.Context);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ControlSerialNo", "");
			if (systemProfile != null)
			{
				this._useSN = Convert.ToBoolean(systemProfile);
			}
			this.InitPermitOrgs();
			this.InitButtonFieldMap();
			if (this.View.ParentFormView.Session.ContainsKey("DetailHeaders"))
			{
				this._headers = (Dictionary<string, InvQueryHeaderArgs>)this.View.ParentFormView.Session["DetailHeaders"];
			}
			this._useBtncaption = ResManager.LoadKDString("清空", "004023000012263", 5, new object[0]);
			this._clearBtncaption = ResManager.LoadKDString("重置", "004023000012264", 5, new object[0]);
			object customParameter = this.View.OpenParameter.GetCustomParameter("QueryMode");
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("NeedReturnData");
			object customParameter3 = this.View.OpenParameter.GetCustomParameter("QueryFilter");
			object customParameter4 = this.View.OpenParameter.GetCustomParameter("QueryOrgId");
			object customParameter5 = this.View.OpenParameter.GetCustomParameter("StockOrgIds");
			if (customParameter != null)
			{
				this.queryMode = Convert.ToInt32(customParameter);
			}
			if (customParameter2 != null)
			{
				this.returnDataMode = Convert.ToInt32(customParameter2);
			}
			if (customParameter4 != null && !string.IsNullOrWhiteSpace(customParameter4.ToString()))
			{
				this.qOrgId = Convert.ToInt64(customParameter4);
			}
			if (customParameter3 != null)
			{
				this._queryFilter = customParameter3.ToString();
			}
			if (customParameter5 != null)
			{
				this.stockOrgIds = customParameter5.ToString();
			}
			object customParameter6 = this.View.OpenParameter.GetCustomParameter("QueryBillFormId");
			if (customParameter6 != null)
			{
				this._queryBillFormId = customParameter6.ToString();
			}
			this.InitHeadButton();
			string text = this._queryFilter;
			bool flag = false;
			string appendFilter = this.BuildHeadFilter(ref flag);
			text = this.AppendFilter(text, appendFilter);
			object customParameter7 = this.View.OpenParameter.GetCustomParameter("BillInputMoreFilter");
			if (customParameter7 != null)
			{
				this._billInputMoreFilter = (customParameter7.ToString() == "1");
			}
			if (!flag)
			{
				flag = this._billInputMoreFilter;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["HaveMoreFilter"] = (flag ? "1" : "0");
			dictionary["QueryFilter"] = text;
			dictionary["QueryPage"] = this.View.PageId;
			dictionary["NeedReturnData"] = this.returnDataMode.ToString();
			dictionary["QueryOrgId"] = this.qOrgId.ToString();
			dictionary["StockOrgIds"] = this.stockOrgIds;
			dictionary["QueryMode"] = this.queryMode.ToString();
			dictionary["QueryBillFormId"] = this._queryBillFormId;
			object customParameter8 = this.View.OpenParameter.GetCustomParameter("QuerySortString");
			dictionary["QuerySortString"] = ((customParameter8 == null) ? "" : customParameter8.ToString());
			this.AddOnInvListForm(dictionary);
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0002BA94 File Offset: 0x00029C94
		public override void AfterBindData(EventArgs e)
		{
			this.View.GetMainBarItem("tbReserveLinkQuery").Visible = this.usePLNReserve;
			this.View.GetMainBarItem("tbreservrLinkTraceBack").Visible = false;
			this.View.GetMainBarItem("tbViewSN").Visible = this._useSN;
			this.View.GetMainBarItem("tbReturnData").Visible = (this.returnDataMode == 1);
			this.View.GetMainBarItem("tbSplitButton_AddToDataCollection").Visible = (this.returnDataMode == 1);
			this.View.GetControl<Panel>("FPanelInvHead").Visible = true;
			this.View.GetControl<Panel>("FPanelInvHead1").Visible = true;
			this.ShowHideExtHead();
			this._queryType = InventoryQuery.GetStringInvUserSet<string>(this.View.Context, "STK_Inventory", "QueryType", "1");
			this.SwitchQueryType(this._queryType);
			this.FillAllHeadFieldData();
			this.SetAllBtnUseStatus();
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0002BBB8 File Offset: 0x00029DB8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBRETURNDATA")
				{
					this.ReturnDetailData();
					e.Cancel = true;
					return;
				}
				if (a == "TBREFRESH")
				{
					this.RefreshListData(true);
					e.Cancel = true;
					return;
				}
				if (a == "TBCLOSE")
				{
					this.View.Close();
					e.Cancel = true;
					return;
				}
				if (a == "TBSPLITBUTTON_ADDTODATACOLLECTION")
				{
					((IListViewService)this.View.GetView(this.detailFormPageId)).MainBarItemClick(e.BarItemKey);
					e.Cancel = true;
					return;
				}
			}
			BarDataManager menu = this.View.LayoutInfo.GetFormAppearance().Menu;
			BarItem barItem = menu.BarItems.FirstOrDefault((BarItem p) => p.Name.Equals(e.BarItemKey));
			if (barItem == null)
			{
				e.Cancel = true;
				return;
			}
			if (barItem.ClickActions == null || barItem.ClickActions.Count < 1)
			{
				((IListViewService)this.View.GetView(this.detailFormPageId)).MainBarItemClick(e.BarItemKey);
				((IListViewService)this.View.GetView(this.detailFormPageId)).CustomEvents(this.detailFormPageId, "DoOperate", e.BarItemKey);
				return;
			}
			base.BarItemClick(e);
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0002BD64 File Offset: 0x00029F64
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string key;
			switch (key = e.Key)
			{
			case "FBTNBOM":
			case "FBTNEXPIRYDATE":
			case "FBTNKEEPER":
			case "FBTNLOT":
			case "FBTNMATERIAL":
			case "FBTNMATNAME":
			case "FBTNMTONO":
			case "FBTNORG":
			case "FBTNOWNER":
			case "FBTNPRODUCEDATE":
			case "FBTNSTOCK":
			case "FBTNSTOCKSTATUS":
				this.SwitchHeadFilterByButton(e.Key);
				return;
			case "FBTNAUXPROP":
			case "FBTNSTOCKLOC":
				this._refreshByreflx = true;
				this.SwitchHeadFilterByButton(e.Key);
				return;
			case "FBTNEXTHEAD":
				this.ShowHideExtHead();
				break;

				return;
			}
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0002BED4 File Offset: 0x0002A0D4
		public override void CustomEvents(CustomEventsArgs e)
		{
			string key;
			switch (key = e.EventName.ToUpperInvariant())
			{
			case "RETURNDETAILDATA":
				this.ReturnDetailData();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "CLOSEWINDOWBYDETAIL":
				this.View.Close();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "CLOSEDATACOLLECTIONANDRETURN":
				this.ReturnDataFromSessionData();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "RETURNANDCLEARDATACOLLECTION":
				this.ReturnDataFromSessionCollection(true);
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "RETURNDATAFROMDATACOLLECTION":
				this.ReturnDataFromSessionData();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "SWITCHQUERYTYPE":
				if (!string.IsNullOrWhiteSpace(e.EventArgs) && !this._queryType.Equals(e.EventArgs.Trim()))
				{
					this._queryType = e.EventArgs.Trim();
					this.SwitchQueryType(this._queryType);
					((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
					return;
				}
				break;
			case "UPDATESUMINFO":
			{
				string eventArgs = e.EventArgs;
				Control control = this.View.GetControl("FLblSumInfo");
				control.SetValue(eventArgs);
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				break;
			}

				return;
			}
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0002C10C File Offset: 0x0002A30C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key)
			{
			case "FKeeperTypeIdH":
				this.Model.SetValue("FKeeperIdH", null);
				this.SetBtnMultiClassDataStatus("FBTNKEEPER", "FKeeperTypeIdH");
				return;
			case "FOwnerTypeIdH":
				this.Model.SetValue("FOwnerIdH", null);
				this.SetBtnMultiClassDataStatus("FBTNOWNER", "FOwnerTypeIdH");
				return;
			case "FBOMIdH":
			case "FExpiryDateH":
			case "FKeeperIdH":
			case "FLotH":
			case "FMaterialIdH":
			case "FMaterialNameH":
			case "FMtoNoH":
			case "FOwnerIdH":
			case "FProduceDateH":
			case "FStockIdH":
			case "FStockOrgIdH":
			case "FStockStatusIdH":
				this.SwitchHeaderFilter(e.Field);
				return;
			case "FAuxPropH":
			case "FStockLocH":
				if (this._refreshByreflx)
				{
					this.SwitchHeaderFilter(e.Field);
					this._refreshByreflx = false;
					return;
				}
				return;
			}
			base.DataChanged(e);
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0002C2DC File Offset: 0x0002A4DC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			this._refreshByreflx = false;
			string text = "";
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FLotH"))
				{
					if (fieldKey == "FStockOrgIdH")
					{
						if (this.GetFieldFilter(e.FieldKey, out text))
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
					}
				}
				else if (this.GetFieldFilter(e.FieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
						listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
					}
				}
			}
			e.PermissionFormId = "STK_Inventory";
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0002C3D1 File Offset: 0x0002A5D1
		public override void FieldLabelClick(FieldLabelClickArgs e)
		{
			base.FieldLabelClick(e);
			e.PermissionFormId = "STK_Inventory";
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0002C3E8 File Offset: 0x0002A5E8
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string text = "";
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FLotH"))
				{
					return;
				}
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
				}
			}
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0002C456 File Offset: 0x0002A656
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropH") || StringUtils.EqualsIgnoreCase(e.FieldKey, "FStockLocH"))
			{
				this._refreshByreflx = true;
				return;
			}
			this._refreshByreflx = false;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0002C48C File Offset: 0x0002A68C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result != 1)
			{
				return;
			}
			if (e.FlexField.Key.Equals("FAuxPropH") || e.FlexField.Key.Equals("FStockLocH"))
			{
				this.SwitchHeaderFilter(e.FlexField);
			}
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0002C4E4 File Offset: 0x0002A6E4
		private bool GetFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			if (fieldKey != null)
			{
				if (!(fieldKey == "FLotH"))
				{
					if (fieldKey == "FStockOrgIdH")
					{
						filter = string.Format("exists (select 1 from  T_SEC_USERORG tur where fuserid={0} and tur.FORGID=t0.FORGID)", base.Context.UserId.ToString());
					}
				}
				else
				{
					DynamicObject dynamicObject = this.Model.GetValue("FMaterialIdH") as DynamicObject;
					if (dynamicObject != null)
					{
						filter = this.AppendFilter(filter, string.Format(" FMaterialId = {0} ", this.GetDynamicValue(dynamicObject)));
					}
					DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgIdH") as DynamicObject;
					if (dynamicObject2 != null)
					{
						filter = this.AppendFilter(filter, string.Format(" FUSEORGID = {0} ", Convert.ToInt64(dynamicObject2["Id"])));
					}
					filter = this.AppendFilter(filter, "FBIZTYPE = '1' AND FLotStatus = '1' ");
				}
			}
			return true;
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0002C5DC File Offset: 0x0002A7DC
		private void InitButtonFieldMap()
		{
			this._buttonHeadFieldMap = new Dictionary<string, string>();
			this._buttonHeadFieldMap["FBTNBOM"] = "FBOMIdH";
			this._buttonHeadFieldMap["FBTNEXPIRYDATE"] = "FExpiryDateH";
			this._buttonHeadFieldMap["FBTNKEEPER"] = "FKeeperIdH";
			this._buttonHeadFieldMap["FBTNLOT"] = "FLotH";
			this._buttonHeadFieldMap["FBTNMATERIAL"] = "FMaterialIdH";
			this._buttonHeadFieldMap["FBTNMTONO"] = "FMtoNoH";
			this._buttonHeadFieldMap["FBTNORG"] = "FStockOrgIdH";
			this._buttonHeadFieldMap["FBTNOWNER"] = "FOwnerIdH";
			this._buttonHeadFieldMap["FBTNPRODUCEDATE"] = "FProduceDateH";
			this._buttonHeadFieldMap["FBTNSTOCK"] = "FStockIdH";
			this._buttonHeadFieldMap["FBTNSTOCKSTATUS"] = "FStockStatusIdH";
			this._buttonHeadFieldMap["FBTNSTOCKLOC"] = "FStockLocH";
			this._buttonHeadFieldMap["FBTNAUXPROP"] = "FAuxPropH";
			this._buttonHeadFieldMap["FBTNMATNAME"] = "FMaterialNameH";
			this._headFieldButtonMap = new Dictionary<string, string>();
			this._headFieldButtonMap["FBOMIdH"] = "FBTNBOM";
			this._headFieldButtonMap["FExpiryDateH"] = "FBTNEXPIRYDATE";
			this._headFieldButtonMap["FKeeperIdH"] = "FBTNKEEPER";
			this._headFieldButtonMap["FLotH"] = "FBTNLOT";
			this._headFieldButtonMap["FMaterialIdH"] = "FBTNMATERIAL";
			this._headFieldButtonMap["FMtoNoH"] = "FBTNMTONO";
			this._headFieldButtonMap["FStockOrgIdH"] = "FBTNORG";
			this._headFieldButtonMap["FOwnerIdH"] = "FBTNOWNER";
			this._headFieldButtonMap["FProduceDateH"] = "FBTNPRODUCEDATE";
			this._headFieldButtonMap["FStockIdH"] = "FBTNSTOCK";
			this._headFieldButtonMap["FStockStatusIdH"] = "FBTNSTOCKSTATUS";
			this._headFieldButtonMap["FStockLocH"] = "FBTNSTOCKLOC";
			this._headFieldButtonMap["FAuxPropH"] = "FBTNAUXPROP";
			this._headFieldButtonMap["FMaterialNameH"] = "FBTNMATNAME";
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0002C84C File Offset: 0x0002AA4C
		private void InitHeadButton()
		{
			this.SetButtonStyle("FBTNORG");
			this.SetButtonStyle("FBTNMATERIAL");
			this.SetButtonStyle("FBTNMATNAME");
			this.SetButtonStyle("FBTNSTOCKLOC");
			this.SetButtonStyle("FBTNAUXPROP");
			this.SetButtonStyle("FBTNLOT");
			this.SetButtonStyle("FBTNSTOCK");
			this.SetButtonStyle("FBTNPRODUCEDATE");
			this.SetButtonStyle("FBTNEXPIRYDATE");
			this.SetButtonStyle("FBTNBOM");
			this.SetButtonStyle("FBTNSTOCKSTATUS");
			this.SetButtonStyle("FBTNMTONO");
			this.SetButtonStyle("FBTNOWNER");
			this.SetButtonStyle("FBTNKEEPER");
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0002C8F4 File Offset: 0x0002AAF4
		private void InitPermitOrgs()
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			string filterClauseWihtKey = " FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' AND FORGFUNCTIONS LIKE '%103%' ";
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = filterClauseWihtKey,
				RequiresDataPermission = false
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(this.View.Context, new BusinessObject
			{
				Id = "STK_Inventory"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			this._permitOrgs = new List<long>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				long item = Convert.ToInt64(dynamicObject["FORGID"]);
				if (permissionOrg.Contains(item))
				{
					this._permitOrgs.Add(item);
				}
			}
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x0002C9F8 File Offset: 0x0002ABF8
		private void SetButtonStyle(string buttonKey)
		{
			Button control = this.View.GetControl<Button>(buttonKey);
			control.SetCustomPropertyValue("ExVisible", false);
			control.SetCustomPropertyValue("Style", "2");
			control.SetCustomPropertyValue("ForeColor", "#FF000000");
			control.SetCustomPropertyValue("ExHoveColor", "#FFFFFFFF");
			control.SetCustomPropertyValue("ExPressColor", "#FFFFFFFF");
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x0002CA64 File Offset: 0x0002AC64
		private void FillAllHeadFieldData()
		{
			if (this._headers == null || this._headers.Count < 1)
			{
				return;
			}
			foreach (string text in this._headers.Keys)
			{
				InvQueryHeaderArgs invQueryHeaderArgs = this._headers[text];
				if (invQueryHeaderArgs.FilteEnabled && invQueryHeaderArgs.Value != null)
				{
					this.Model.SetValue(invQueryHeaderArgs.FieldKey, invQueryHeaderArgs.Value);
					this.View.UpdateView(invQueryHeaderArgs.FieldKey);
					FieldEditor control = this.View.GetControl<FieldEditor>(text);
					control.Enabled = !invQueryHeaderArgs.IsNeedLock;
					if (invQueryHeaderArgs.FieldKey.ToUpperInvariant().Equals("FMaterialIdH".ToUpperInvariant()))
					{
						this.View.InvokeFieldUpdateService(invQueryHeaderArgs.FieldKey, -1);
						this.View.UpdateView("FMaterialNameH");
					}
				}
			}
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x0002CB78 File Offset: 0x0002AD78
		private void SetAllBtnUseStatus()
		{
			this.SetBtnUseStatus("FBTNORG");
			this.SetBtnUseStatus("FBTNMATERIAL");
			this.SetBtnUseStatus("FBTNMATNAME");
			this.SetBtnUseStatus("FBTNSTOCKLOC");
			this.SetBtnUseStatus("FBTNAUXPROP");
			this.SetBtnUseStatus("FBTNLOT");
			this.SetBtnUseStatus("FBTNSTOCK");
			this.SetBtnUseStatus("FBTNPRODUCEDATE");
			this.SetBtnUseStatus("FBTNEXPIRYDATE");
			this.SetBtnUseStatus("FBTNBOM");
			this.SetBtnUseStatus("FBTNSTOCKSTATUS");
			this.SetBtnUseStatus("FBTNMTONO");
			this.SetBtnMultiClassDataStatus("FBTNOWNER", "FOwnerTypeIdH");
			this.SetBtnMultiClassDataStatus("FBTNKEEPER", "FKeeperTypeIdH");
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0002CC2C File Offset: 0x0002AE2C
		private void SetBtnMultiClassDataStatus(string btnKey, string itemClassFieldKey)
		{
			string text = this._buttonHeadFieldMap[btnKey];
			FieldEditor control = this.View.GetControl<FieldEditor>(text);
			Button control2 = this.View.GetControl<Button>(btnKey);
			bool flag = false;
			bool enabled = false;
			if (this._headers != null)
			{
				object value = this.View.Model.GetValue(itemClassFieldKey);
				if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
				{
					enabled = true;
					InvQueryHeaderArgs invQueryHeaderArgs;
					this._headers.TryGetValue(text, out invQueryHeaderArgs);
					flag = (invQueryHeaderArgs == null || invQueryHeaderArgs.FilteEnabled);
					if (invQueryHeaderArgs != null && invQueryHeaderArgs.IsNeedLock)
					{
						enabled = false;
						control2.Visible = false;
					}
				}
			}
			else
			{
				flag = true;
			}
			control.Enabled = enabled;
			control2.Text = (flag ? this._useBtncaption : this._clearBtncaption);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0002CCF0 File Offset: 0x0002AEF0
		private void SetBtnUseStatus(string btnKey)
		{
			string text = this._buttonHeadFieldMap[btnKey];
			Button control = this.View.GetControl<Button>(btnKey);
			bool flag;
			if (this._headers != null)
			{
				InvQueryHeaderArgs invQueryHeaderArgs;
				this._headers.TryGetValue(text, out invQueryHeaderArgs);
				flag = (invQueryHeaderArgs == null || invQueryHeaderArgs.FilteEnabled);
				if (invQueryHeaderArgs != null && invQueryHeaderArgs.IsNeedLock)
				{
					FieldEditor fieldEditor = this.View.GetFieldEditor(text, -1);
					fieldEditor.Enabled = false;
					control.Visible = false;
				}
			}
			else
			{
				flag = true;
			}
			control.Text = (flag ? this._useBtncaption : this._clearBtncaption);
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0002CD84 File Offset: 0x0002AF84
		private void ShowHideExtHead()
		{
			bool flag = !this.View.GetControl<Panel>("FPanelInvHead1").Visible;
			this.View.GetControl<Panel>("FPanelInvHead1").Visible = flag;
			string str;
			if (flag)
			{
				str = "icon_ExpandDownNormal.png";
				this.View.GetControl<Panel>("FPanelHeadb").SetHeight(187);
			}
			else
			{
				str = "icon_ExpandRightNormal.png";
				this.View.GetControl<Panel>("FPanelHeadb").SetHeight(108);
			}
			Button control = this.View.GetControl<Button>("FBTNEXTHEAD");
			control.SetImageUrl("images/silverlight/default/toolbar/" + str);
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0002CE2C File Offset: 0x0002B02C
		private void SwitchHeadFilterByButton(string buttonKey)
		{
			string text = this._buttonHeadFieldMap[buttonKey];
			InvQueryHeaderArgs invQueryHeaderArgs = null;
			if (this._headers != null)
			{
				this._headers.TryGetValue(text, out invQueryHeaderArgs);
			}
			if (invQueryHeaderArgs == null)
			{
				this.Model.SetValue(text, null);
			}
			else if (buttonKey.ToUpperInvariant().Equals("FBTNOWNER"))
			{
				this.SwitchMultiItemField(buttonKey, invQueryHeaderArgs, "FOwnerTypeIdH");
			}
			else if (buttonKey.ToUpperInvariant().Equals("FBTNKEEPER"))
			{
				this.SwitchMultiItemField(buttonKey, invQueryHeaderArgs, "FKeeperTypeIdH");
			}
			else
			{
				bool flag = !invQueryHeaderArgs.FilteEnabled;
				invQueryHeaderArgs.FilteEnabled = flag;
				if (flag)
				{
					this.Model.SetValue(text, invQueryHeaderArgs.Value);
				}
				else
				{
					this.Model.SetValue(text, null);
				}
			}
			if (buttonKey.ToUpperInvariant().Equals("FBTNMATERIAL"))
			{
				this.View.InvokeFieldUpdateService("FMaterialIdH", -1);
			}
			this.View.UpdateView(text);
			this.SetBtnUseStatus(buttonKey);
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0002CF20 File Offset: 0x0002B120
		private void SwitchHeaderFilter(Field headField)
		{
			string text = "";
			Button button = null;
			this._headFieldButtonMap.TryGetValue(headField.Key, out text);
			if (!string.IsNullOrWhiteSpace(text))
			{
				button = this.View.GetControl<Button>(text);
			}
			InvQueryHeaderArgs invQueryHeaderArgs = null;
			if (this._headers != null && this._headers.Count > 0)
			{
				this._headers.TryGetValue(headField.Key, out invQueryHeaderArgs);
			}
			object value = this.Model.GetValue(headField);
			if (this.IsBlankValue(headField, value))
			{
				if (button != null)
				{
					if (invQueryHeaderArgs != null)
					{
						button.Text = this._clearBtncaption;
					}
					else
					{
						button.Text = this._useBtncaption;
					}
				}
				if (invQueryHeaderArgs != null)
				{
					invQueryHeaderArgs.FilteEnabled = false;
				}
			}
			else
			{
				if (button != null)
				{
					button.Text = this._useBtncaption;
				}
				if (invQueryHeaderArgs != null)
				{
					invQueryHeaderArgs.FilteEnabled = true;
				}
			}
			this.RefreshListData(false);
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0002CFF0 File Offset: 0x0002B1F0
		private bool IsBlankValue(Field headField, object value)
		{
			if (value == null)
			{
				return true;
			}
			if (headField is RelatedFlexGroupField)
			{
				DynamicObject dynamicObject = value as DynamicObject;
				return dynamicObject == null || this.IsBlankRelateDynamicObject((RelatedFlexGroupField)headField, dynamicObject);
			}
			if (headField is ItemClassField || headField is BaseDataField)
			{
				DynamicObject dynamicObject2 = value as DynamicObject;
				return dynamicObject2 == null || Convert.ToInt64(dynamicObject2["Id"]) == 0L;
			}
			return (headField is ItemClassTypeField || headField is TextField || headField is DateTimeField) && string.IsNullOrWhiteSpace(value.ToString());
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0002D07C File Offset: 0x0002B27C
		private bool IsBlankRelateDynamicObject(RelatedFlexGroupField relatedFlexGroupField, DynamicObject dyObj)
		{
			if (dyObj == null)
			{
				return true;
			}
			bool result = true;
			DynamicObjectType dynamicObjectType = dyObj.DynamicObjectType;
			foreach (DynamicProperty dynamicProperty in dynamicObjectType.Properties)
			{
				if (!(dynamicProperty.Name == "Id") && !(dynamicProperty.Name == "OPCODE"))
				{
					object value = dynamicProperty.GetValue(dyObj);
					if (value != null)
					{
						if (dynamicProperty.PropertyType == typeof(DynamicObject))
						{
							string text = ((DynamicObject)value)["Id"].ToString().Trim();
							if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
							{
								if (!StringUtils.IsNumeric(text))
								{
									result = false;
									break;
								}
								long num = Convert.ToInt64(text);
								if (num > 0L)
								{
									result = false;
									break;
								}
							}
						}
						else
						{
							string value2 = value.ToString();
							if (!string.IsNullOrWhiteSpace(value2))
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

		// Token: 0x060003BD RID: 957 RVA: 0x0002D180 File Offset: 0x0002B380
		private void SwitchMultiItemField(string buttonKey, InvQueryHeaderArgs head, string itemClassFieldKey)
		{
			bool flag = !head.FilteEnabled;
			head.FilteEnabled = flag;
			if (flag)
			{
				this.Model.SetValue(itemClassFieldKey, this._headers[itemClassFieldKey].Value);
				this.Model.SetValue(head.FieldKey, head.Value);
			}
			else
			{
				this.Model.SetValue(head.FieldKey, null);
			}
			this.SetBtnMultiClassDataStatus(buttonKey, itemClassFieldKey);
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0002D20C File Offset: 0x0002B40C
		private void HideFieldFromBill(FilterArgs e)
		{
			if (string.IsNullOrWhiteSpace(this._billCurFieldInvHeadKey))
			{
				return;
			}
			if (this._headers == null)
			{
				return;
			}
			InvQueryHeaderArgs invQueryHeaderArgs = null;
			this._headers.TryGetValue(this._billCurFieldInvHeadKey, out invQueryHeaderArgs);
			if (invQueryHeaderArgs == null)
			{
				return;
			}
			string controlFields = invQueryHeaderArgs.ControlFields;
			if (string.IsNullOrWhiteSpace(controlFields))
			{
				return;
			}
			string[] array = controlFields.Split(new char[]
			{
				','
			});
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string fieldKey = array2[i];
				ColumnField columnField = e.ColumnFields.FirstOrDefault((ColumnField p) => p.Key.Equals(fieldKey));
				if (columnField != null)
				{
					columnField.Visible = false;
				}
			}
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0002D2C8 File Offset: 0x0002B4C8
		private string BuildHeadFilter(ref bool haveMoreFilter)
		{
			haveMoreFilter = false;
			StringBuilder stringBuilder = new StringBuilder();
			string value = this.BuildBaseDataFieldFilter("FStockOrgIdH", "FStockOrgId");
			stringBuilder.Append(value);
			value = this.BuildBaseDataFieldFilter("FMaterialIdH", "FMaterialId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildMaterialHeadFilter("FMaterialNameH", "FMaterialId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildBaseDataFieldFilter("FLotH", "FLot");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildBaseDataFieldFilter("FStockIdH", "FStockId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildFlexFieldFilter("FStockLocH", "FStockLocId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildFlexFieldFilter("FAuxPropH", "FAuxPropId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildBaseDataFieldFilter("FStockStatusIdH", "FStockStatusId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildBaseDataFieldFilter("FBOMIdH", "FBomId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildStringFieldFilter("FOwnerTypeIdH", "FOwnerTypeId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildStringFieldFilter("FKeeperTypeIdH", "FKeeperTypeId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildMultiItemFieldFilter("FOwnerIdH", "FOwnerId", "FOwnerTypeIdH", "FOwnerTypeId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildMultiItemFieldFilter("FKeeperIdH", "FKeeperId", "FKeeperTypeIdH", "FKeeperTypeId");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildLotPrdDateFieldFilter("FProduceDateH", "FProduceDate");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildLotPrdDateFieldFilter("FExpiryDateH", "FExpiryDate");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			value = this.BuildStringFieldFilter("FMtoNoH", "FMtoNo");
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append(value);
				haveMoreFilter = true;
			}
			string result = "";
			if (stringBuilder.Length > 3)
			{
				result = stringBuilder.ToString().Substring(4, stringBuilder.Length - 4);
			}
			return result;
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0002D54C File Offset: 0x0002B74C
		private string AppendFilter(string filter, string appendFilter)
		{
			string text = "";
			if (string.IsNullOrEmpty(filter))
			{
				text = appendFilter;
			}
			else if (!string.IsNullOrWhiteSpace(appendFilter))
			{
				text += string.Format(" {0} AND {1} ", filter, appendFilter);
			}
			else
			{
				text = filter;
			}
			return text;
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0002D58C File Offset: 0x0002B78C
		private string BuildStringFieldFilter(string headFieldkey, string invFieldKey)
		{
			string result = "";
			object value = this.Model.GetValue(headFieldkey);
			if (value != null)
			{
				string text = value.ToString().Replace("'", "''");
				if (!string.IsNullOrWhiteSpace(text))
				{
					result = string.Format(" AND {0} = '{1}' ", invFieldKey, text);
				}
			}
			else
			{
				bool flag = false;
				InvQueryHeaderArgs invQueryHeaderArgs = null;
				if (this._headers != null)
				{
					this._headers.TryGetValue(headFieldkey, out invQueryHeaderArgs);
					flag = (invQueryHeaderArgs != null && invQueryHeaderArgs.FilteEnabled);
				}
				if (flag)
				{
					result = invQueryHeaderArgs.FilterString;
				}
			}
			return result;
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0002D614 File Offset: 0x0002B814
		private string BuildLotPrdDateFieldFilter(string headFieldkey, string invFieldKey)
		{
			string result = "";
			object value = this.Model.GetValue(headFieldkey);
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				result = string.Format(" AND {0} = TO_DATE('{1}') ", invFieldKey, Convert.ToDateTime(value).Date);
			}
			else
			{
				bool flag = false;
				InvQueryHeaderArgs invQueryHeaderArgs = null;
				if (this._headers != null)
				{
					this._headers.TryGetValue(headFieldkey, out invQueryHeaderArgs);
					flag = (invQueryHeaderArgs != null && invQueryHeaderArgs.FilteEnabled);
				}
				if (flag)
				{
					result = invQueryHeaderArgs.FilterString;
				}
			}
			return result;
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0002D69C File Offset: 0x0002B89C
		private string BuildFlexFieldFilter(string headFieldkey, string invFieldKey)
		{
			string result = "";
			DynamicObject dynamicObject = this.Model.GetValue(headFieldkey) as DynamicObject;
			if (dynamicObject != null)
			{
				string text = headFieldkey.Equals("FStockLocH") ? "BD_FLEXVALUES" : "BD_FLEXAUXPROPERTY";
				string flexGetAuxPropFilter = StockServiceHelper.GetFlexGetAuxPropFilter(this.View.Context, text, dynamicObject, invFieldKey);
				if (!string.IsNullOrWhiteSpace(flexGetAuxPropFilter))
				{
					result = flexGetAuxPropFilter;
				}
			}
			else
			{
				bool flag = false;
				InvQueryHeaderArgs invQueryHeaderArgs = null;
				if (this._headers != null)
				{
					this._headers.TryGetValue(headFieldkey, out invQueryHeaderArgs);
					flag = (invQueryHeaderArgs != null && invQueryHeaderArgs.FilteEnabled);
				}
				if (flag)
				{
					result = invQueryHeaderArgs.FilterString;
				}
			}
			return result;
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0002D738 File Offset: 0x0002B938
		private string BuildBaseDataFieldFilter(string headFieldkey, string invFieldKey)
		{
			string result = "";
			DynamicObject dynamicObject = this.Model.GetValue(headFieldkey) as DynamicObject;
			if (dynamicObject != null)
			{
				result = string.Format(" AND {0} = {1} ", invFieldKey, this.GetDynamicValue(dynamicObject));
			}
			else
			{
				bool flag = false;
				InvQueryHeaderArgs invQueryHeaderArgs = null;
				if (this._headers != null)
				{
					this._headers.TryGetValue(headFieldkey, out invQueryHeaderArgs);
					flag = (invQueryHeaderArgs != null && invQueryHeaderArgs.FilteEnabled);
				}
				if (flag)
				{
					result = invQueryHeaderArgs.FilterString;
				}
			}
			return result;
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0002D7B0 File Offset: 0x0002B9B0
		private string BuildMaterialHeadFilter(string headFieldkey, string invFieldKey)
		{
			string result = "";
			string text = this.Model.GetValue(headFieldkey) as string;
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = text.Replace("'", "''");
				result = string.Format(" AND FMaterialName LIKE N'%{0}%' ", text);
			}
			return result;
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0002D7FC File Offset: 0x0002B9FC
		private string BuildMultiItemFieldFilter(string itemHeadKey, string itemKey, string itemTypeHeadkey, string itemTypeKey)
		{
			string text = "";
			bool flag = false;
			InvQueryHeaderArgs invQueryHeaderArgs = null;
			InvQueryHeaderArgs invQueryHeaderArgs2 = null;
			if (this._headers != null)
			{
				this._headers.TryGetValue(itemHeadKey, out invQueryHeaderArgs);
				flag = (invQueryHeaderArgs != null && invQueryHeaderArgs.FilteEnabled);
				this._headers.TryGetValue(itemTypeHeadkey, out invQueryHeaderArgs2);
			}
			DynamicObject dynamicObject = this.Model.GetValue(itemHeadKey) as DynamicObject;
			if (dynamicObject != null)
			{
				object value = this.Model.GetValue(itemTypeHeadkey);
				string text2 = "";
				if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
				{
					text2 = value.ToString();
				}
				text = string.Format(" AND {0} = '{1}' AND {2} = {3} ", new object[]
				{
					itemTypeKey,
					text2,
					itemKey,
					this.GetDynamicValue(dynamicObject)
				});
			}
			else
			{
				if (invQueryHeaderArgs == null)
				{
					return text;
				}
				if (invQueryHeaderArgs2 != null && invQueryHeaderArgs2.FilteEnabled && !string.IsNullOrWhiteSpace(invQueryHeaderArgs2.FilterString))
				{
					text = invQueryHeaderArgs2.FilterString;
				}
				if (flag)
				{
					text += invQueryHeaderArgs.FilterString;
				}
			}
			return text;
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0002D900 File Offset: 0x0002BB00
		private void SwitchQueryType(string queryType)
		{
			Panel control = this.View.GetControl<Panel>("FPanelHeadb");
			if (!(queryType == "1"))
			{
				control.Visible = false;
				this.View.GetControl<Panel>("FPanelHeadb").SetHeight(0);
				return;
			}
			control.Visible = true;
			bool visible = this.View.GetControl<Panel>("FPanelInvHead1").Visible;
			if (visible)
			{
				this.View.GetControl<Panel>("FPanelHeadb").SetHeight(187);
				return;
			}
			this.View.GetControl<Panel>("FPanelHeadb").SetHeight(108);
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0002D99C File Offset: 0x0002BB9C
		private void AddOnInvListForm(Dictionary<string, string> dicParam = null)
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_Inventory";
			listShowParameter.OpenStyle.TagetKey = "FPanelList";
			listShowParameter.OpenStyle.ShowType = 3;
			this.detailFormPageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.PageId = this.detailFormPageId;
			listShowParameter.CustomParams.Add("IsFromQuery", "True");
			listShowParameter.CustomParams.Add("IsFromDetailQuery", "True");
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsLookUp = true;
			if (!string.IsNullOrWhiteSpace(this.stockOrgIds))
			{
				listShowParameter.MutilListUseOrgId = this.stockOrgIds;
			}
			if (dicParam != null)
			{
				foreach (string key in dicParam.Keys)
				{
					listShowParameter.CustomParams.Add(key, dicParam[key]);
				}
			}
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x0002DAD0 File Offset: 0x0002BCD0
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string id = "STK_Inventory";
			string permissionItemIdByMenuBar = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			if (string.IsNullOrWhiteSpace(permissionItemIdByMenuBar))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, permissionItemIdByMenuBar);
			return permissionAuthResult.Passed;
		}

		// Token: 0x060003CA RID: 970 RVA: 0x0002DB6C File Offset: 0x0002BD6C
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

		// Token: 0x060003CB RID: 971 RVA: 0x0002DBD4 File Offset: 0x0002BDD4
		private void ReturnDetailData()
		{
			if (this.View == null)
			{
				return;
			}
			string text = string.Empty;
			if (this.returnDataMode > 0 && this.qOrgId > 0L)
			{
				List<string> list = new List<string>();
				IListView listView = (IListView)this.View.GetView(this.detailFormPageId);
				if (listView != null && listView.SelectedRowsInfo != null)
				{
					foreach (ListSelectedRow listSelectedRow in listView.SelectedRowsInfo)
					{
						if (listSelectedRow != null && listSelectedRow.Selected && listSelectedRow.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
						{
							list.Add(listSelectedRow.PrimaryKeyValue);
						}
					}
					if (list.Count > 1000)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("对不起，您选择的数据过多，最多只能返回{0}条数据，请重新选择！", "004023000013473", 5, new object[0]), 1000), "", 0);
						return;
					}
					if (list.Count > 0)
					{
						string text2 = string.Join("','", list);
						text = text2;
					}
				}
			}
			this.View.ReturnToParentWindow(text);
			this.View.Close();
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0002DD18 File Offset: 0x0002BF18
		private void ReturnDataFromSessionCollection(bool clearSessionData)
		{
			string text = string.Empty;
			if (this.returnDataMode > 0 && this.qOrgId > 0L)
			{
				List<string> list = new List<string>();
				IListView listView = (IListView)this.View.GetView(this.detailFormPageId);
				if (listView.Session != null && listView.Session.ContainsKey("Data_Collection"))
				{
					Dictionary<string, ListSelectedRow> dictionary = listView.Session["Data_Collection"] as Dictionary<string, ListSelectedRow>;
					if (dictionary != null && dictionary.Count > 0)
					{
						if (dictionary.Count > 1000)
						{
							this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("对不起，您选择的数据过多，最多只能返回{0}条数据，请重新选择！", "004023000013473", 5, new object[0]), 1000), "", 0);
							return;
						}
						foreach (string key in dictionary.Keys)
						{
							ListSelectedRow listSelectedRow = dictionary[key];
							if (listSelectedRow.Selected && listSelectedRow.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
							{
								list.Add(listSelectedRow.PrimaryKeyValue);
							}
						}
						string text2 = string.Join("','", list);
						text = text2;
					}
					if (clearSessionData)
					{
						listView.Session["Data_Collection"] = null;
					}
				}
			}
			this.View.ReturnToParentWindow(text);
			this.View.Close();
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0002DEA0 File Offset: 0x0002C0A0
		private void ReturnDataFromSessionData()
		{
			string text = string.Empty;
			if (this.returnDataMode > 0 && this.qOrgId > 0L)
			{
				List<string> list = new List<string>();
				IListView listView = (IListView)this.View.GetView(this.detailFormPageId);
				if (listView.Session != null && listView.Session.ContainsKey("returnData"))
				{
					Dictionary<string, object> dictionary = listView.Session["returnData"] as Dictionary<string, object>;
					if (dictionary != null && dictionary.Count > 0)
					{
						Dictionary<string, ListSelectedRow> dictionary2 = dictionary["ReturnData"] as Dictionary<string, ListSelectedRow>;
						if (dictionary2 != null && dictionary2.Count > 0)
						{
							if (dictionary2.Count > 1000)
							{
								this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("对不起，您选择的数据过多，最多只能返回{0}条数据，请重新选择！", "004023000013473", 5, new object[0]), 1000), "", 0);
								return;
							}
							foreach (string key in dictionary2.Keys)
							{
								ListSelectedRow listSelectedRow = dictionary2[key];
								if (listSelectedRow.Selected && listSelectedRow.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
								{
									list.Add(listSelectedRow.PrimaryKeyValue);
								}
							}
							string text2 = string.Join("','", list);
							text = text2;
						}
					}
				}
			}
			this.View.ReturnToParentWindow(text);
			this.View.Close();
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0002E040 File Offset: 0x0002C240
		private void RefreshListData(bool isForceRefresh = false)
		{
			string text = "";
			if (!string.IsNullOrWhiteSpace(this._queryFilter))
			{
				text = this._queryFilter;
			}
			bool flag = false;
			string text2 = this.BuildHeadFilter(ref flag);
			if (!string.IsNullOrWhiteSpace(text2))
			{
				text = this.AppendFilter(text, text2);
			}
			text += "];^]";
			IListView listView = this.View.GetView(this.detailFormPageId) as IListView;
			if (listView == null)
			{
				Logger.Info("STK", ResManager.LoadKDString("BillQueryInv: InvJoinQueryEdit:  找不到明细库存列表控件", "004023000023764", 5, new object[0]));
				return;
			}
			List<long> orgList = this.GetOrgList();
			listView.Model.FilterParameter.IsolationOrgList = orgList;
			if (orgList.Count > 0)
			{
				text += string.Format("{0}", string.Join<long>(",", orgList));
			}
			if (!flag)
			{
				flag = this._billInputMoreFilter;
			}
			text += string.Format("];^]{0}", flag ? "1" : "0");
			listView.Session["IsForceRefresh"] = isForceRefresh;
			((IListViewService)this.View.GetView(this.detailFormPageId)).CustomEvents(this.detailFormPageId, "RefreshData", text);
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0002E174 File Offset: 0x0002C374
		private List<long> GetOrgList()
		{
			List<long> list = new List<long>();
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgIdH") as DynamicObject;
			if (dynamicObject == null)
			{
				list.AddRange(this._permitOrgs);
			}
			else
			{
				list.Add(Convert.ToInt64(dynamicObject["Id"]));
			}
			return list;
		}

		// Token: 0x0400013F RID: 319
		private const string imgUrlPre = "images/silverlight/default/toolbar/";

		// Token: 0x04000140 RID: 320
		private const string USEIMAGEKEY = "tbtn_startupserve.png";

		// Token: 0x04000141 RID: 321
		private const string CLEARIMAGEKEY = "tbtn_cancelholiday.png";

		// Token: 0x04000142 RID: 322
		private const string EXTHEADSHOWIMAGEKEY = "icon_ExpandDownNormal.png";

		// Token: 0x04000143 RID: 323
		private const string EXTHEADHIDEIMAGEKEY = "icon_ExpandRightNormal.png";

		// Token: 0x04000144 RID: 324
		private const int maxRet = 1000;

		// Token: 0x04000145 RID: 325
		private const string BTNORG = "FBTNORG";

		// Token: 0x04000146 RID: 326
		private const string BTNMATERIAL = "FBTNMATERIAL";

		// Token: 0x04000147 RID: 327
		private const string BTNLOT = "FBTNLOT";

		// Token: 0x04000148 RID: 328
		private const string BTNSTOCK = "FBTNSTOCK";

		// Token: 0x04000149 RID: 329
		private const string BTNPRODUCEDATE = "FBTNPRODUCEDATE";

		// Token: 0x0400014A RID: 330
		private const string BTNEXPIRYDATE = "FBTNEXPIRYDATE";

		// Token: 0x0400014B RID: 331
		private const string BTNBOM = "FBTNBOM";

		// Token: 0x0400014C RID: 332
		private const string BTNSTOCKSTATUS = "FBTNSTOCKSTATUS";

		// Token: 0x0400014D RID: 333
		private const string BTNMTONO = "FBTNMTONO";

		// Token: 0x0400014E RID: 334
		private const string BTNOWNER = "FBTNOWNER";

		// Token: 0x0400014F RID: 335
		private const string BTNKEEPER = "FBTNKEEPER";

		// Token: 0x04000150 RID: 336
		private const string BTNSTOCKLOC = "FBTNSTOCKLOC";

		// Token: 0x04000151 RID: 337
		private const string BTNAUXPROP = "FBTNAUXPROP";

		// Token: 0x04000152 RID: 338
		private const string BTNEXTHEAD = "FBTNEXTHEAD";

		// Token: 0x04000153 RID: 339
		private const string BTNMATNAME = "FBTNMATNAME";

		// Token: 0x04000154 RID: 340
		private const int PANELHEADBHEIGHTT = 187;

		// Token: 0x04000155 RID: 341
		private const int PANELHEADBHEIGHTS = 108;

		// Token: 0x04000156 RID: 342
		private int queryMode;

		// Token: 0x04000157 RID: 343
		private int returnDataMode;

		// Token: 0x04000158 RID: 344
		private long qOrgId;

		// Token: 0x04000159 RID: 345
		private string stockOrgIds = "";

		// Token: 0x0400015A RID: 346
		private string sFixFilter = "";

		// Token: 0x0400015B RID: 347
		private string detailFormPageId = "";

		// Token: 0x0400015C RID: 348
		private bool usePLNReserve;

		// Token: 0x0400015D RID: 349
		private bool _useSN;

		// Token: 0x0400015E RID: 350
		private string _useBtncaption = "";

		// Token: 0x0400015F RID: 351
		private string _clearBtncaption = "";

		// Token: 0x04000160 RID: 352
		private Dictionary<string, InvQueryHeaderArgs> _headers;

		// Token: 0x04000161 RID: 353
		private string _queryFilter = "";

		// Token: 0x04000162 RID: 354
		private string _billCurFieldInvHeadKey = "";

		// Token: 0x04000163 RID: 355
		private string _queryBillFormId = "";

		// Token: 0x04000164 RID: 356
		private Dictionary<string, string> _buttonHeadFieldMap = new Dictionary<string, string>();

		// Token: 0x04000165 RID: 357
		private Dictionary<string, string> _headFieldButtonMap = new Dictionary<string, string>();

		// Token: 0x04000166 RID: 358
		private List<long> _permitOrgs = new List<long>();

		// Token: 0x04000167 RID: 359
		private bool _refreshByreflx;

		// Token: 0x04000168 RID: 360
		private bool _billInputMoreFilter;

		// Token: 0x04000169 RID: 361
		private string _queryType = "";
	}
}

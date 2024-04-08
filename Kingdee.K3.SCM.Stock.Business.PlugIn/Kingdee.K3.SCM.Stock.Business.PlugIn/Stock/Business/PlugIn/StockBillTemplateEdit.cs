using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200001E RID: 30
	public class StockBillTemplateEdit : AbstractBillPlugIn
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000137 RID: 311 RVA: 0x000115FE File Offset: 0x0000F7FE
		public DateTime? OldDate
		{
			get
			{
				return this._oldDate;
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00011606 File Offset: 0x0000F806
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.OnBillInitialize(e);
			this.InitBillEntityMap();
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00011618 File Offset: 0x0000F818
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FOWNERIDHEAD"))
					{
						return;
					}
					if (this.GetOwnerFieldFilter(e.FieldKey, out text, e.Row))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = text;
							return;
						}
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				else if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
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

		// Token: 0x0600013A RID: 314 RVA: 0x000116F8 File Offset: 0x0000F8F8
		private bool GetFieldFilter(string fieldKey, out string filter, int rowIndex)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FSTOCKORGID")
			{
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00011740 File Offset: 0x0000F940
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg && StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
			{
				e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,再进行库存业务处理.", "004023030002269", 5, new object[0]);
				e.Cancel = true;
			}
		}

		// Token: 0x0600013C RID: 316 RVA: 0x0001179C File Offset: 0x0000F99C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FOWNERIDHEAD"))
					{
						return;
					}
					if (this.GetOwnerFieldFilter(e.BaseDataFieldKey, out text, e.Row))
					{
						if (string.IsNullOrEmpty(e.Filter))
						{
							e.Filter = text;
							return;
						}
						e.Filter = e.Filter + " AND " + text;
					}
				}
				else if (this.GetFieldFilter(e.BaseDataFieldKey.ToUpperInvariant(), out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00011860 File Offset: 0x0000FA60
		public override void AfterCreateNewData(EventArgs e)
		{
			if (!this.bflag)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
				long orgId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				this.CheckStockStartDate(orgId);
				this.bflag = true;
			}
			this._oldDate = null;
		}

		// Token: 0x0600013E RID: 318 RVA: 0x000118C4 File Offset: 0x0000FAC4
		public override void AfterLoadData(EventArgs e)
		{
			base.AfterLoadData(e);
			this._oldDate = null;
			DynamicObject dataObject = this.Model.DataObject;
			if (dataObject != null && Convert.ToInt64(dataObject["Id"]) > 0L)
			{
				object value = this.Model.GetValue("FDocumentStatus");
				if (value != null && "C" != value.ToString())
				{
					return;
				}
				value = this.Model.GetValue("FDate");
				if (value != null)
				{
					this._oldDate = new DateTime?(Convert.ToDateTime(value));
				}
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00011954 File Offset: 0x0000FB54
		public override void AfterSave(AfterSaveEventArgs e)
		{
			base.AfterSave(e);
			if (e.OperationResult.IsSuccess)
			{
				object value = this.Model.GetValue("FDate");
				if (value != null)
				{
					this._oldDate = new DateTime?(Convert.ToDateTime(value));
				}
			}
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0001199C File Offset: 0x0000FB9C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Key.ToUpperInvariant() == "FSTOCKORGID")
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
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00011A30 File Offset: 0x0000FC30
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FDATE"))
				{
					return;
				}
				if (base.Context.ClientType != 32)
				{
					string text = "";
					this._multiOwnerEntityKeyMap.TryGetValue(base.View.BusinessInfo.GetForm().Id, out text);
					if (!string.IsNullOrWhiteSpace(text))
					{
						this.CheckDateBatchOwner(text);
						return;
					}
					this.CheckAccountDate();
				}
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00011AB0 File Offset: 0x0000FCB0
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
				base.View.Model.SetValue("FStockOrgID", null);
				if (orgId != 0L)
				{
					base.View.ShowMessage(ResManager.LoadKDString("所选库存组织未启用", "004023030002272", 5, new object[0]), 0);
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00011B2C File Offset: 0x0000FD2C
		private bool GetOwnerFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FOWNERIDHEAD")
			{
				filter = SCMCommon.GetOwnerFilterByRelation(this, new GetOwnerFilterArgs("FOwnerTypeIdHead", row, "FStockOrgId", row, "112", "2"));
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00011B90 File Offset: 0x0000FD90
		private void InitBillEntityMap()
		{
			if (this._multiOwnerEntityKeyMap == null)
			{
				this._multiOwnerEntityKeyMap = new Dictionary<string, string>();
				this._multiOwnerEntityKeyMap["SAL_OUTSTOCK"] = "FEntity";
				this._multiOwnerEntityKeyMap["SAL_RETURNSTOCK"] = "FEntity";
				this._multiOwnerEntityKeyMap["STK_InStock"] = "FInStockEntry";
				this._multiOwnerEntityKeyMap["PUR_MRB"] = "FPURMRBENTRY";
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00011C04 File Offset: 0x0000FE04
		public void CheckAccountDate()
		{
			if (!this.NeedCheckBillDate())
			{
				return;
			}
			long num = 0L;
			long num2 = 0L;
			string text = string.Empty;
			DateTime dateTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			string text2 = base.View.Model.GetValue("FOwnerTypeIdHead").ToString();
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

		// Token: 0x06000146 RID: 326 RVA: 0x00011D80 File Offset: 0x0000FF80
		public void CheckDateBatchOwner(string entryEntityKey)
		{
			if (!this.NeedCheckBillDate())
			{
				return;
			}
			long num = 0L;
			string value = string.Empty;
			DateTime dateTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			else
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("库存组织未录入，时间不允许修改", "004023030000373", 5, new object[0]), ResManager.LoadKDString("单据日期校验不通过", "004023030000370", 5, new object[0]), 0);
			}
			if (base.View.Model.GetValue("FDate") != null)
			{
				dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate").ToString());
			}
			List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>();
			int entryRowCount = base.View.Model.GetEntryRowCount(entryEntityKey);
			for (int i = 0; i < entryRowCount; i++)
			{
				if (this.NeedCheckBillDateEntry(i))
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerId", i) as DynamicObject;
					if (dynamicObject2 != null)
					{
						long num2 = Convert.ToInt64(dynamicObject2["Id"]);
						if (num2 > 0L)
						{
							object value2 = base.View.Model.GetValue("FOwnerTypeId", i);
							if (value2 != null)
							{
								value = value2.ToString();
								list.Add(new KeyValuePair<long, string>(num2, value));
							}
						}
					}
				}
			}
			OperateResultCollection operateResultCollection = new OperateResultCollection();
			operateResultCollection = CommonServiceHelper.CheckDateBatchOwner(base.Context, num, "", dateTime, list);
			if (operateResultCollection.Count > 0)
			{
				List<FieldAppearance> list2 = new List<FieldAppearance>();
				FieldAppearance fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(base.Context, "FMessage", ResManager.LoadKDString("处理结果", "004023030008968", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("700", base.Context.UserLocale.LCID);
				list2.Add(fieldAppearance);
				K3DisplayerModel k3DisplayerModel = K3DisplayerModel.Create(base.Context, list2.ToArray(), null);
				foreach (OperateResult operateResult in operateResultCollection)
				{
					new K3DisplayerMessage();
					k3DisplayerModel.AddMessage(operateResult.Message);
				}
				k3DisplayerModel.CancelButton.Visible = false;
				ViewUtils.ShowK3Displayer(base.View, k3DisplayerModel, 4, null, "BOS_K3Displayer");
			}
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0001200C File Offset: 0x0001020C
		public virtual bool NeedCheckBillDate()
		{
			if (base.View.BusinessInfo.GetForm().Id.Equals("STK_InvInit"))
			{
				return false;
			}
			if (this.OldDate == null)
			{
				return true;
			}
			object value = this.Model.GetValue("FDate");
			return value == null || this.OldDate != Convert.ToDateTime(value);
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0001208C File Offset: 0x0001028C
		public virtual bool NeedCheckBillDateEntry(int entryIndex)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", entryIndex) as DynamicObject;
			return dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L;
		}

		// Token: 0x04000073 RID: 115
		private bool bflag;

		// Token: 0x04000074 RID: 116
		private DateTime? _oldDate;

		// Token: 0x04000075 RID: 117
		protected Dictionary<string, string> _multiOwnerEntityKeyMap;
	}
}

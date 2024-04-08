using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000071 RID: 113
	public class LotCodeItemEdit : AbstractBillPlugIn
	{
		// Token: 0x0600051E RID: 1310 RVA: 0x0003F324 File Offset: 0x0003D524
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.View.RuleContainer.AddPluginRule("FEntity", 2, new Action<DynamicObject, object>(this.SetFieldValue), new string[]
			{
				"FFieldName"
			});
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0003F364 File Offset: 0x0003D564
		private void SetFieldValue(DynamicObject row, dynamic dynamicRow)
		{
			row["Field"] = row["FieldName"].ToString();
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0003F381 File Offset: 0x0003D581
		public override void AfterLoadData(EventArgs e)
		{
			this.IsSetFieldKey = true;
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x0003F38A File Offset: 0x0003D58A
		public override void AfterBindData(EventArgs e)
		{
			this.IsSetFieldKey = false;
			this.SetComValue();
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0003F39C File Offset: 0x0003D59C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (a == "FTYPE")
				{
					string text = this.Model.GetValue("FType").ToString();
					string key;
					switch (key = text)
					{
					}
					this.Model.DeleteEntryData("FEntity");
					return;
				}
				if (a == "FSOURCEID")
				{
					this.Model.DeleteEntryData("FEntity");
					return;
				}
				if (!(a == "FFIELD"))
				{
					return;
				}
				if (!this.IsSetFieldKey)
				{
					this.Model.BeginIniti();
					base.View.Model.SetValue(e.Field.Key, e.OldValue, e.Row);
					this.Model.EndIniti();
				}
			}
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0003F520 File Offset: 0x0003D720
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSOURCEID"))
				{
					if (!(a == "FFIELD"))
					{
						return;
					}
					this.RowIndex = e.Row;
					string value = base.View.Model.GetValue("FBILLFORMID", this.RowIndex).ToString();
					string text = this.Model.GetValue("FType").ToString();
					string value2 = (this.Model.GetValue("FSourceID") == null) ? "" : ((DynamicObject)this.Model.GetValue("FSourceID"))["Id"].ToString();
					if (text.Equals("Const") || text.Equals("FlowNo") || text.Equals("CurrentDate") || (string.IsNullOrWhiteSpace(value2) && (text.Equals("BaseData") || text.Equals("AssistantData"))))
					{
						base.View.ShowMessage("来源不能为空或类型不能为常量、流水号、系统当前日期!", 0);
						return;
					}
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.MultiSelect = false;
					dynamicFormShowParameter.ParentPageId = base.View.PageId;
					dynamicFormShowParameter.FormId = "LotCodeField";
					dynamicFormShowParameter.CustomParams.Add("BillFormID", value);
					dynamicFormShowParameter.CustomParams.Add("TypeValue", text);
					dynamicFormShowParameter.CustomParams.Add("SourceID", value2);
					base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AfterShowLock));
				}
				else
				{
					string text2 = this.Model.GetValue("FType").ToString();
					string key;
					switch (key = text2)
					{
					case "Number":
					case "BillText":
					case "DateTime":
					case "Const":
					case "FlowNo":
					case "RowNo":
					case "CurrentDate":
						e.Cancel = true;
						return;
					case "BaseData":
					case "AssistantData":
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter += string.Format(" FTYPE = '{0}'", text2);
						return;
					}

						return;
					}
				}
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x0003F7C8 File Offset: 0x0003D9C8
		protected void AfterShowLock(FormResult result)
		{
			if (result.ReturnData != null)
			{
				JSONObject jsonobject = (JSONObject)result.ReturnData;
				base.View.Model.SetValue("FBILLFORMID", jsonobject["billFormID"], this.RowIndex);
				this.IsSetFieldKey = true;
				base.View.Model.SetValue("FField", jsonobject["field"], this.RowIndex);
				this.IsSetFieldKey = false;
				base.View.Model.SetValue("FFieldKey", jsonobject["fieldKey"], this.RowIndex);
				base.View.Model.SetValue("FFieldName", jsonobject["fieldName"], this.RowIndex);
			}
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0003F8DC File Offset: 0x0003DADC
		private void SetComValue()
		{
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FBILLFORMID", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			if (base.View.Context.IsStandardEdition())
			{
				ComboField comboField = this.Model.BusinessInfo.GetElement("FBILLFORMID") as ComboField;
				List<EnumItem> list = new List<EnumItem>();
				list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
				EnumItem item = (from p in list
				where p.Value == "REM_MixedFlowPlan"
				select p).FirstOrDefault<EnumItem>();
				EnumItem item2 = (from p in list
				where p.Value == "REM_INSTOCK"
				select p).FirstOrDefault<EnumItem>();
				EnumItem item3 = (from p in list
				where p.Value == "REM_ProdSubDayPlan"
				select p).FirstOrDefault<EnumItem>();
				EnumItem item4 = (from p in list
				where p.Value == "REM_IndepenReqPlan"
				select p).FirstOrDefault<EnumItem>();
				list.Remove(item);
				list.Remove(item2);
				list.Remove(item3);
				list.Remove(item4);
				comboFieldEditor.SetComboItems(list);
			}
		}

		// Token: 0x040001ED RID: 493
		private int RowIndex = -1;

		// Token: 0x040001EE RID: 494
		private bool IsSetFieldKey;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000070 RID: 112
	public class LotCodeField : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000515 RID: 1301 RVA: 0x0003EC30 File Offset: 0x0003CE30
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.billFormID = ((e.Paramter.GetCustomParameter("BillFormID") == null) ? "" : e.Paramter.GetCustomParameter("BillFormID").ToString());
			this.typeValue = ((e.Paramter.GetCustomParameter("TypeValue") == null) ? "" : e.Paramter.GetCustomParameter("TypeValue").ToString());
			this.sourceID = ((e.Paramter.GetCustomParameter("SourceID") == null) ? "" : e.Paramter.GetCustomParameter("SourceID").ToString());
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0003ECD9 File Offset: 0x0003CED9
		public override void OnLoad(EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(this.billFormID))
			{
				this.View.Model.SetValue("FBILLFORMID", this.billFormID);
				this.SetFieldList(this.billFormID);
			}
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0003ED10 File Offset: 0x0003CF10
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FBILLFORMID"))
				{
					return;
				}
				this.billFormID = ((e.NewValue == null) ? "" : e.NewValue.ToString());
				this.SetFieldList(this.billFormID);
			}
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0003ED88 File Offset: 0x0003CF88
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FBTNOK"))
				{
					return;
				}
				DynamicObject dynamicObject = (DynamicObject)this.View.Model.GetValue("FBILLFORMID");
				this.billFormID = ((dynamicObject == null) ? "" : ((dynamicObject["Id"] == null) ? "" : dynamicObject["Id"].ToString()));
				string fieldKey = this.View.Model.GetValue("FField").ToString();
				Field field = this.fieldList.SingleOrDefault((Field p) => p.Key.Equals(fieldKey));
				if (field != null)
				{
					LocaleValue localeValue = field.Entity.Name.Clone() as LocaleValue;
					localeValue.Merger(field.Name, ".", true);
					JSONObject jsonobject = new JSONObject();
					jsonobject.Add("billFormID", this.billFormID);
					jsonobject.Add("field", field.Entity.Name.ToString() + "." + field.Name.ToString());
					jsonobject.Add("fieldKey", fieldKey);
					jsonobject.Add("fieldName", localeValue);
					this.View.ReturnToParentWindow(jsonobject);
					return;
				}
				e.Cancel = true;
				this.View.ShowErrMessage(ResManager.LoadKDString("字段值无效", "004023030000346", 5, new object[0]), ResManager.LoadKDString("无效数据", "004023030000349", 5, new object[0]), 0);
			}
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0003EF28 File Offset: 0x0003D128
		public virtual bool AcceptBillTypeControl(Field field)
		{
			bool result = false;
			string a;
			if ((a = this.typeValue) != null)
			{
				if (!(a == "Number"))
				{
					if (!(a == "BillText"))
					{
						if (!(a == "DateTime"))
						{
							if (!(a == "BaseData"))
							{
								if (a == "AssistantData")
								{
									result = ((field is AssistantField || field is MulAssistantField) && !string.IsNullOrWhiteSpace(((BaseDataField)field).LookUpObjectID) && ((BaseDataField)field).LookUpObjectID.Equals(this.sourceID));
								}
							}
							else if (this.sourceID.Equals("BD_OPERATOR"))
							{
								string text = ",BD_WAREHOUSEWORKERS,BD_BUYER,BD_Saler,BD_PLANNER,";
								result = (field is BaseDataField && ((BaseDataField)field).LookUpObject != null && text.Contains("," + ((BaseDataField)field).LookUpObject.FormId + ","));
							}
							else
							{
								result = (field is BaseDataField && ((BaseDataField)field).LookUpObject != null && ((BaseDataField)field).LookUpObject.FormId.Equals(this.sourceID));
							}
						}
						else
						{
							result = (field is DateTimeField);
						}
					}
					else
					{
						result = (field.GetType().Name == "TextField");
					}
				}
				else
				{
					result = (field is BillNoField);
				}
			}
			return result;
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0003F0C4 File Offset: 0x0003D2C4
		private void SetFieldList(string billFormID)
		{
			ComboFieldEditor comboFieldEditor = this.View.GetFieldEditor("FField", 0) as ComboFieldEditor;
			List<EnumItem> list = new List<EnumItem>();
			list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
			{
				EnumId = "",
				Value = "",
				Caption = new LocaleValue("", this.View.Context.UserLocale.LCID)
			});
			this.fieldList = new List<Field>();
			if (!string.IsNullOrWhiteSpace(billFormID))
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, billFormID);
				this.fieldList = (from p in formMetaData.BusinessInfo.GetFieldList()
				where this.AcceptBillTypeControl(p) && !(p is ProxyField) && !(p.Entity is SNSubEntryEntity)
				select p).ToList<Field>();
				if (this.typeValue.ToUpperInvariant().Equals("ROWNO"))
				{
					LotField lotField = (from p in formMetaData.BusinessInfo.GetFieldList()
					where p is LotField
					select p).FirstOrDefault<Field>() as LotField;
					if (lotField != null && lotField.Entity != null)
					{
						Entity entity = lotField.Entity;
						if (!string.IsNullOrEmpty(entity.SeqFieldKey))
						{
							IntegerField integerField = new IntegerField();
							integerField.Entity = entity;
							integerField.EntityKey = entity.Key;
							integerField.Name = new LocaleValue(ResManager.LoadKDString("行号", "004023000022541", 5, new object[0]));
							integerField.Key = entity.SeqFieldKey;
							this.fieldList.Add(integerField);
						}
					}
				}
				foreach (Field field in this.fieldList)
				{
					LocaleValue localeValue = field.Entity.Name.Clone() as LocaleValue;
					localeValue.Merger(field.Name, ".", true);
					list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
					{
						EnumId = field.Key,
						Value = field.Key,
						Caption = localeValue
					});
				}
			}
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x040001E8 RID: 488
		private string billFormID;

		// Token: 0x040001E9 RID: 489
		private string typeValue;

		// Token: 0x040001EA RID: 490
		private string sourceID;

		// Token: 0x040001EB RID: 491
		private List<Field> fieldList;
	}
}

using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000094 RID: 148
	public class ExpenseApportionEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060007E6 RID: 2022 RVA: 0x00065AE5 File Offset: 0x00063CE5
		public override void AfterCreateModelData(EventArgs e)
		{
			base.AfterCreateModelData(e);
			this.BindDataToObjectEntry();
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x00065AF4 File Offset: 0x00063CF4
		protected virtual void BindDataToObjectEntry()
		{
			this.View.Model.DeleteEntryData("FBizDataEntity");
			int entryRowCount = this.View.ParentFormView.Model.GetEntryRowCount("FInStockEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				int num = this.AddNewRowToEntity("FBizDataEntity");
				this.View.Model.SetValue("FEntryID", this.View.ParentFormView.Model.DataObject["Id"].ToString(), num);
				this.View.Model.SetValue("FMaterialID", ((DynamicObject)this.View.ParentFormView.Model.GetValue("FMaterialID", i))["Id"].ToString(), num);
				this.View.Model.SetValue("FMaterialName", ((DynamicObject)this.View.ParentFormView.Model.GetValue("FMaterialID", i))["Name"].ToString(), num);
				this.View.Model.SetValue("FQty", Convert.ToDecimal(this.View.ParentFormView.Model.GetValue("FPriceQty", i)), num);
			}
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x00065C4E File Offset: 0x00063E4E
		protected int AddNewRowToEntity(string entryKey)
		{
			this.View.Model.CreateNewEntryRow(entryKey);
			return this.View.Model.GetEntryRowCount(entryKey) - 1;
		}
	}
}

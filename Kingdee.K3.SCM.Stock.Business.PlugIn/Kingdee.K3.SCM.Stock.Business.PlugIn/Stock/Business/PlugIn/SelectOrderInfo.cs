using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000057 RID: 87
	public class SelectOrderInfo : AbstractDynamicFormPlugIn
	{
		// Token: 0x060003EA RID: 1002 RVA: 0x0002F6E8 File Offset: 0x0002D8E8
		public override void OnInitialize(InitializeEventArgs e)
		{
			if (e.Paramter.GetCustomParameter("SessionOrderKey") == null)
			{
				return;
			}
			string key = e.Paramter.GetCustomParameter("SessionOrderKey").ToString();
			if (this.View.ParentFormView.Session.ContainsKey(key))
			{
				this.parentData = (this.View.ParentFormView.Session[key] as DynamicObjectCollection);
				this.View.ParentFormView.Session.Remove(key);
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0002F770 File Offset: 0x0002D970
		public override void CreateNewData(BizDataEventArgs e)
		{
			DynamicObjectType dynamicObjectType = this.View.BusinessInfo.GetDynamicObjectType();
			EntryEntity entryEntity = (EntryEntity)this.View.BusinessInfo.GetEntity("FEntity");
			if (this.parentData != null)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				int num = 1;
				foreach (DynamicObject dynamicObject2 in this.parentData)
				{
					DynamicObject dynamicObject3 = new DynamicObject(entryEntity.DynamicObjectType);
					entryEntity.DynamicProperty.GetValue<DynamicObjectCollection>(dynamicObject).Add(dynamicObject3);
					dynamicObject3["OrderNo"] = Convert.ToString(dynamicObject2["SubSRCBILLNO"]);
					dynamicObject3["OrderSeq"] = Convert.ToString(dynamicObject2["SubSrcSeq"]);
					dynamicObject3["SrcEntryId"] = Convert.ToString(dynamicObject2["SubSrcEntryId"]);
					dynamicObject3["SEQ"] = num;
					num++;
				}
				e.BizDataObject = dynamicObject;
			}
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0002F898 File Offset: 0x0002DA98
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (e.Field.Key.ToUpper() == "FSELECT" && e.NewValue.ToString().ToUpper() == "TRUE")
			{
				Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				new DynamicObject(entity.DynamicObjectType);
				foreach (DynamicObject dynamicObject in entityDataObject)
				{
					if (dynamicObject["Select"].ToString().ToUpper() == "TRUE" && Convert.ToInt32(dynamicObject["Seq"]) - 1 != e.Row)
					{
						this.View.Model.SetValue("FSelect", "False", Convert.ToInt32(dynamicObject["Seq"]) - 1);
					}
				}
			}
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0002F9B4 File Offset: 0x0002DBB4
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			DynamicObject dynamicObject = new DynamicObject(entity.DynamicObjectType);
			foreach (DynamicObject dynamicObject2 in entityDataObject)
			{
				if (dynamicObject2["Select"].ToString().ToUpper() == "TRUE")
				{
					dynamicObject = dynamicObject2;
					break;
				}
			}
			this.View.ReturnToParentWindow(dynamicObject);
		}

		// Token: 0x04000170 RID: 368
		private DynamicObjectCollection parentData;
	}
}

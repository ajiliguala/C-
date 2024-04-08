using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000016 RID: 22
	[Description("受托路线字段携带表单插件")]
	public class OEMConvertSet : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600009C RID: 156 RVA: 0x000088BC File Offset: 0x00006ABC
		public override void AfterCreateModelData(EventArgs e)
		{
			DynamicObjectCollection oemconvertSetDatas = StockServiceHelper.GetOEMConvertSetDatas(this.View.Context, "");
			if (oemconvertSetDatas != null)
			{
				this.SetBillData(oemconvertSetDatas.ToList<DynamicObject>());
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00008998 File Offset: 0x00006B98
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant().Equals("TBSAVE"))
			{
				DynamicObject dataObject = this.View.Model.DataObject;
				if (dataObject == null)
				{
					return;
				}
				DynamicObjectCollection dynamicObjectCollection = dataObject["STK_OEMFIELDCONSET"] as DynamicObjectCollection;
				if (dynamicObjectCollection == null)
				{
					return;
				}
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["STK_OEMFIELDCONSETENTRY"] as DynamicObjectCollection;
					if (dynamicObjectCollection2 != null && dynamicObjectCollection2.Count<DynamicObject>() > 0)
					{
						DynamicObject dynamicObject2 = (from p in dynamicObjectCollection2
						where string.IsNullOrEmpty(Convert.ToString(p["TARGETFIELD"])) && !string.IsNullOrEmpty(Convert.ToString(p["SOURCEFIELD"]))
						select p).FirstOrDefault<DynamicObject>();
						if (dynamicObject2 != null)
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("“目标单字段”必录！", "00444711000018494", 5, new object[0]), "", 0);
							return;
						}
						dynamicObject2 = (from p in dynamicObjectCollection2
						where !string.IsNullOrEmpty(Convert.ToString(p["TARGETFIELD"])) && string.IsNullOrEmpty(Convert.ToString(p["SOURCEFIELD"]))
						select p).FirstOrDefault<DynamicObject>();
						if (dynamicObject2 != null)
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("“源单字段”必录！", "00444711000018495", 5, new object[0]), "", 0);
							return;
						}
						List<string> list = (from p in dynamicObjectCollection2
						where !string.IsNullOrEmpty(Convert.ToString(p["TARGETFIELD"])) && !string.IsNullOrEmpty(Convert.ToString(p["SOURCEFIELD"]))
						select Convert.ToString(p["TARGETFIELD"])).ToList<string>();
						if (list != null && list.Count<string>() != list.Distinct<string>().Count<string>())
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("“目标单字段”存在重复设置，不允许保存！", "00444711030038756", 5, new object[0]), "", 0);
							return;
						}
					}
				}
				bool flag = StockServiceHelper.SaveOemConvertSetData(base.Context, this.View.Model.DataObject);
				if (flag)
				{
					this.View.ShowMessage(ResManager.LoadKDString("保存成功！", "004023030002266", 5, new object[0]), 0);
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("保存失败！", "00444711000018036", 5, new object[0]), "", 0);
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00008C10 File Offset: 0x00006E10
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null && a == "FTARGETFIELD")
			{
				this.View.Model.SetValue("FSOURCEFIELD", null, e.Row);
				int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FENTITY");
				DynamicObject dynamicObject = this.View.Model.GetValue("FSOURCEFORMID", entryCurrentRowIndex) as DynamicObject;
				string sourformid = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Id"]);
				DynamicObject dynamicObject2 = this.View.Model.GetValue("FTARGETFORMID", entryCurrentRowIndex) as DynamicObject;
				string tartformid = (dynamicObject2 == null) ? "" : Convert.ToString(dynamicObject2["Id"]);
				this.InitiSourComboFieldItem(sourformid, tartformid, Convert.ToString(e.NewValue), "FSOURCEFIELD", e.Row);
				this.View.UpdateView("FSOURCEFIELD");
			}
			base.DataChanged(e);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00008D20 File Offset: 0x00006F20
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FSUBENTITY"))
			{
				int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FENTITY");
				DynamicObject dynamicObject = this.View.Model.GetValue("FTARGETFORMID", entryCurrentRowIndex) as DynamicObject;
				string text = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Id"]);
				this.InitiTarComboFieldItem(text, "FTARGETFIELD", entryCurrentRowIndex);
				this.View.UpdateView("FTARGETFIELD");
				DynamicObject dynamicObject2 = this.View.Model.GetValue("FSOURCEFORMID", entryCurrentRowIndex) as DynamicObject;
				string sourformid = (dynamicObject2 == null) ? "" : Convert.ToString(dynamicObject2["Id"]);
				string tartFieldKey = Convert.ToString(this.View.Model.GetValue("FTARGETFIELD", e.Row));
				this.InitiSourComboFieldItem(sourformid, text, tartFieldKey, "FSOURCEFIELD", e.Row);
				this.View.UpdateView("FTARGETFIELD");
			}
			base.EntityRowClick(e);
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00008E9C File Offset: 0x0000709C
		private void InitiTarComboFieldItem(string formid, string fieldkey, int index)
		{
			if (string.IsNullOrEmpty(formid) || string.IsNullOrEmpty(fieldkey))
			{
				return;
			}
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>(fieldkey, index);
			List<EnumItem> list = new List<EnumItem>();
			if (!this._billToFiledKeys.ContainsKey(formid))
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, formid, true) as FormMetadata;
				List<Field> value = (from p in formMetadata.BusinessInfo.GetFieldList()
				where !(p is ProxyField) && !(p is BaseDataPropertyField) && !string.IsNullOrEmpty(p.FieldName) && (p.EntityKey.ToUpperInvariant().Equals("FBILLHEAD") || p.EntityKey.ToUpperInvariant().Equals("FBILLENTRY"))
				orderby p.Tabindex
				select p).ToList<Field>();
				this._billToFiledKeys.Add(formid, value);
			}
			int num = 0;
			foreach (Field field in this._billToFiledKeys[formid])
			{
				EnumItem enumItem = new EnumItem();
				enumItem.EnumId = field.Key;
				enumItem.Value = field.Key;
				enumItem.Caption = new LocaleValue(field.Entity.Name.GetString(base.Context.UserLocale.LCID) + "." + field.Name.GetString(base.Context.UserLocale.LCID), base.Context.UserLocale.LCID);
				num = (enumItem.Seq = num + 1);
				if (formid != null)
				{
					if (!(formid == "STK_OEMInStock"))
					{
						if (formid == "STK_OEMReceive")
						{
							enumItem.Invalid = this._reStockFieldsStr.Split(new char[]
							{
								','
							}).ToList<string>().Contains(field.Key);
						}
					}
					else
					{
						enumItem.Invalid = this._inStockFieldsStr.Split(new char[]
						{
							','
						}).ToList<string>().Contains(field.Key);
					}
				}
				list.Add(enumItem);
			}
			fieldEditor.SetComboItems(list);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x000091C4 File Offset: 0x000073C4
		private void InitiSourComboFieldItem(string sourformid, string tartformid, string tartFieldKey, string fieldkey, int index)
		{
			if (string.IsNullOrEmpty(sourformid) || string.IsNullOrEmpty(fieldkey))
			{
				return;
			}
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>(fieldkey, index);
			List<EnumItem> list = new List<EnumItem>();
			if (!this._billToFiledKeys.ContainsKey(sourformid))
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, sourformid, true) as FormMetadata;
				List<Field> value = (from p in formMetadata.BusinessInfo.GetFieldList()
				where !(p is ProxyField) && !(p is BaseDataPropertyField) && !string.IsNullOrEmpty(p.FieldName) && (p.EntityKey.ToUpperInvariant().Equals("FBILLHEAD") || p.EntityKey.ToUpperInvariant().Equals("FBILLENTRY"))
				orderby p.Tabindex
				select p).ToList<Field>();
				this._billToFiledKeys.Add(sourformid, value);
			}
			if (!this._billToFiledKeys.ContainsKey(tartformid))
			{
				FormMetadata formMetadata2 = MetaDataServiceHelper.Load(this.View.Context, tartformid, true) as FormMetadata;
				List<Field> value2 = (from p in formMetadata2.BusinessInfo.GetFieldList()
				where !(p is ProxyField) && !(p is BaseDataPropertyField) && !string.IsNullOrEmpty(p.FieldName) && (p.EntityKey.ToUpperInvariant().Equals("FBILLHEAD") || p.EntityKey.ToUpperInvariant().Equals("FBILLENTRY"))
				orderby p.Tabindex
				select p).ToList<Field>();
				this._billToFiledKeys.Add(tartformid, value2);
			}
			Field field = (from p in this._billToFiledKeys[tartformid]
			where p.Key.Equals(tartFieldKey)
			select p).FirstOrDefault<Field>();
			int num = 0;
			foreach (Field field2 in this._billToFiledKeys[sourformid])
			{
				EnumItem enumItem = new EnumItem();
				enumItem.EnumId = field2.Key;
				enumItem.Value = field2.Key;
				enumItem.Caption = new LocaleValue(field2.Entity.Name.GetString(base.Context.UserLocale.LCID) + "." + field2.Name.GetString(base.Context.UserLocale.LCID), base.Context.UserLocale.LCID);
				enumItem.Invalid = true;
				num = (enumItem.Seq = num + 1);
				if (string.IsNullOrEmpty(tartFieldKey))
				{
					enumItem.Invalid = true;
				}
				else if (field != null && field is BaseDataField && field2 is BaseDataField)
				{
					if (string.IsNullOrEmpty(((BaseDataField)field).LookUpObjectID))
					{
						enumItem.Invalid = true;
					}
					else
					{
						enumItem.Invalid = !((BaseDataField)field).LookUpObjectID.Equals(((BaseDataField)field2).LookUpObjectID);
					}
				}
				else if (field != null)
				{
					enumItem.Invalid = !field2.GetType().Name.Equals(field.GetType().Name);
				}
				list.Add(enumItem);
			}
			fieldEditor.SetComboItems(list);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00009614 File Offset: 0x00007814
		private void SetBillData(List<DynamicObject> oemEntrySetDatas)
		{
			List<string> list = (from p in oemEntrySetDatas
			select Convert.ToString(p["FRULEID"])).Distinct<string>().ToList<string>();
			if (list == null || list.Count<string>() <= 0)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			Entity entity2 = this.View.BusinessInfo.GetEntity("FSubEntity");
			entityDataObject.Clear();
			int num = 0;
			using (List<string>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OEMConvertSet.<>c__DisplayClass25 CS$<>8__locals1 = new OEMConvertSet.<>c__DisplayClass25();
					CS$<>8__locals1.rule = enumerator.Current;
					List<DynamicObject> list2 = (from p in oemEntrySetDatas
					where Convert.ToString(p["FRULEID"]).Equals(CS$<>8__locals1.rule)
					select p).ToList<DynamicObject>();
					DynamicObject dynamicObject = list2.FirstOrDefault<DynamicObject>();
					DynamicObject item = new DynamicObject(entity.DynamicObjectType);
					entityDataObject.Add(item);
					string text = Convert.ToString(dynamicObject["FSOURCEFORMID"]);
					this.View.Model.SetValue("FSOURCEFORMID", text, num);
					if (!this._billToFiledKeys.ContainsKey(text))
					{
						FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, text, true) as FormMetadata;
						List<Field> value = (from p in formMetadata.BusinessInfo.GetFieldList()
						where !(p is ProxyField) && !(p is BaseDataPropertyField) && !string.IsNullOrEmpty(p.FieldName) && (p.EntityKey.ToUpperInvariant().Equals("FBILLHEAD") || p.EntityKey.ToUpperInvariant().Equals("FBILLENTRY"))
						orderby p.Tabindex
						select p).ToList<Field>();
						this._billToFiledKeys.Add(text, value);
					}
					string text2 = Convert.ToString(dynamicObject["FTARGETFORMID"]);
					this.View.Model.SetValue("FTARGETFORMID", text2, num);
					if (!this._billToFiledKeys.ContainsKey(text2))
					{
						FormMetadata formMetadata2 = MetaDataServiceHelper.Load(this.View.Context, text2, true) as FormMetadata;
						List<Field> value2 = (from p in formMetadata2.BusinessInfo.GetFieldList()
						where !(p is ProxyField) && !(p is BaseDataPropertyField) && !string.IsNullOrEmpty(p.FieldName) && (p.EntityKey.ToUpperInvariant().Equals("FBILLHEAD") || p.EntityKey.ToUpperInvariant().Equals("FBILLENTRY"))
						orderby p.Tabindex
						select p).ToList<Field>();
						this._billToFiledKeys.Add(text2, value2);
					}
					this.View.Model.SetValue("FRULEID", Convert.ToString(dynamicObject["FRULEID"]), num);
					DynamicObjectCollection dynamicObjectCollection = entityDataObject[num]["STK_OEMFIELDCONSETENTRY"] as DynamicObjectCollection;
					dynamicObjectCollection.Clear();
					int num2 = 0;
					string strSourKey;
					string strTartKey;
					foreach (DynamicObject dynamicObject2 in list2)
					{
						DynamicObject dynamicObject3 = new DynamicObject(entity2.DynamicObjectType);
						dynamicObject3["Id"] = Convert.ToInt64(dynamicObject2["FID"]);
						strSourKey = Convert.ToString(dynamicObject2["FSOURCEFIELD"]);
						strTartKey = Convert.ToString(dynamicObject2["FTARGETFIELD"]);
						dynamicObject3["SOURCEFIELD"] = (((from p in this._billToFiledKeys[text]
						where p.Key.Equals(strSourKey)
						select p).FirstOrDefault<Field>() != null) ? strSourKey : null);
						dynamicObject3["TARGETFIELD"] = (((from p in this._billToFiledKeys[text2]
						where p.Key.Equals(strTartKey)
						select p).FirstOrDefault<Field>() != null) ? strTartKey : null);
						dynamicObject3["Seq"] = ++num2;
						dynamicObjectCollection.Add(dynamicObject3);
					}
					DBServiceHelper.LoadReferenceObject(base.Context, dynamicObjectCollection.ToArray<DynamicObject>(), entity2.DynamicObjectType, false);
					num++;
				}
			}
			this.View.UpdateView("FEntity");
			this.View.UpdateView("FSubEntity");
		}

		// Token: 0x04000036 RID: 54
		private readonly string _inStockFieldsStr = "FCustMatId,FBillTypeID,FStockOrgId,FMaterialId,FAuxPropId,FDocumentStatus,FOwnerTypeIdHead,FOwnerIdHead,FBomId,FProduceDate,FExpiryDate,FMtoNo,FSrcEntryId,FSrcBillTypeId,FUnitID,FInStockType,FProjectNo,FQty,FStockId,FStockLocId,FSecQty,FLot,FBaseQty,FStockStatusId,FOwnerTypeId,FSrcObjectId,FSecUnitId,FBaseUnitId,FOwnerId,FKeeperTypeId,FExtAuxUnitId,FKeeperId,FExtAuxUnitQty,FSNUnitID,FSrcBillNo,FSrcSeq,FSNQty,FJoinQty,FBaseJoinQty,FSECJOINQTY,FStockFlag,FSrcSeqNo,FReSrcBillNo,FReSrcSeq,FRESRCENTRYID,FCreateDate,FCreatorId,FModifierId,FModifyDate,FApproverId,FApproveDate,FCancellerId,FCancelDate,FCancelStatus";

		// Token: 0x04000037 RID: 55
		private readonly string _reStockFieldsStr = "FCustMatId,FBillTypeID,FDocumentStatus,FMaterialId,FStockOrgId,FLot,FBomId,FProduceDate,FAuxPropId,FOwnerTypeIdHead,FOwnerIdHead,FExpiryDate,FMtoNo,FSrcEntryId,FProjectNo,FUnitID,FActlandQty,FActReceiveQty,FRejectQty,FRejectReason,FExtAuxUnitId,FActlandSecQty,FExtAuxUnitQty,FRejectSecQty,FStockId,FStockLocId,FStockStatusId,FOwnerTypeId,FOwnerId,FKeeperTypeId,FKeeperId,FBaseUnitId,FActlandBaseQty,FBaseQty,FRejectBaseQty,FAuxUnitId,FAuxUnitQty,FActlandAuxQty,FRejectAuxQty,FSrcSeqNo,FNeedCheck,FBusinessEnd,FBusinessEnderId,FEndDate,FSrcFormId,FSrcBillNo,FSrcBillTypeId,FSrcSeq,FCheckJoinQty,FCheckQty,FSampleDamageQty,FReceiveQty,FRefuseQty,FCsnReceiveQty,FSampleDamageBaseQty,FReceiveBaseQty,FCheckJoinBaseQty,FRefuseBaseQty,FCheckBaseQty,FCsnReceiveBaseQty,FStockJoinBaseQty,FRefuseJoinBaseQty,FCsnReceiveJoinBaseQty,FReturnStkJnBaseQty,FStockJoinAuxQty,FRefuseJoinAuxQty,FCsnReceiveJoinAuxQty,FReturnStkJnAuxQty,FBUSINESSCLOSED,FINBASEQTY,FINSECQTY,FRETNBASEQTY,FRETNSECQTY,FReSrcBillNo,FReSrcSeq,FCreatorId,FCreateDate,FModifierId,FModifyDate,FApproverId,FApproveDate,FCancellerId,FCancelDate,FCancelStatus,FCloseStatus,FCloserId,FCloseDate,FCloseFlag";

		// Token: 0x04000038 RID: 56
		private Dictionary<string, List<Field>> _billToFiledKeys = new Dictionary<string, List<Field>>();
	}
}

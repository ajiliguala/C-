using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000046 RID: 70
	[Description("单据界面批号编辑处理插件")]
	public class BillLotEditPlugIn : AbstractBillPlugIn
	{
		// Token: 0x060002BB RID: 699 RVA: 0x00021E27 File Offset: 0x00020027
		public override void FormClosed(FormClosedEventArgs e)
		{
			base.FormClosed(e);
			if (this._haveLotField)
			{
				this.BreakBillLots();
			}
		}

		// Token: 0x060002BC RID: 700 RVA: 0x00021E40 File Offset: 0x00020040
		public override void BeforeCreateModelData(EventArgs e)
		{
			base.BeforeCreateModelData(e);
			bool flag = false;
			if (base.View.ParentFormView != null)
			{
				flag = "BOS_ConvertResultForm".Equals(base.View.ParentFormView.BusinessInfo.GetForm().Id);
			}
			if (this._haveLotField && !flag)
			{
				this.BreakBillLots();
			}
		}

		// Token: 0x060002BD RID: 701 RVA: 0x00021E99 File Offset: 0x00020099
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (base.View.OpenParameter.CreateFrom == 1 && base.View.OpenParameter.Status == null)
			{
				this.isPush = true;
				return;
			}
			this.isPush = false;
		}

		// Token: 0x060002BE RID: 702 RVA: 0x00021ED6 File Offset: 0x000200D6
		public override void AfterSave(AfterSaveEventArgs e)
		{
			base.AfterSave(e);
			if (e.OperationResult.IsSuccess)
			{
				this.isPush = false;
			}
		}

		// Token: 0x060002BF RID: 703 RVA: 0x00021EF4 File Offset: 0x000200F4
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (!this._haveLotField)
			{
				return;
			}
			BillTypeField billTypeField = base.View.BusinessInfo.GetField(e.Key) as BillTypeField;
			if (billTypeField != null)
			{
				this.BreakBillLots();
			}
			this.RecordOldLottext(e);
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00021F3D File Offset: 0x0002013D
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			base.AfterDeleteRow(e);
			this.BreakRowLotText(e);
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00021F4D File Offset: 0x0002014D
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			this.BreakRowLotText(e);
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x00021F60 File Offset: 0x00020160
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "NoUseBrokenLot", false);
			this._noUseLotBroken = Convert.ToBoolean(systemProfile);
			if (this._noUseLotBroken)
			{
				this._lotFields.Clear();
				return;
			}
			this.CollectLotFieldsInfo();
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00021FB8 File Offset: 0x000201B8
		private void BreakBillLots()
		{
			if (this.Model.DataObject == null)
			{
				return;
			}
			bool flag = this.Model.DataChanged || this._lotDirty || this.isPush;
			if (base.View.OpenParameter.CreateFrom == 1 && base.View.OpenParameter.Status == null)
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			List<LotTextInfo> list = new List<LotTextInfo>();
			ExtendedDataEntitySet extendedDataEntitySet = new ExtendedDataEntitySet();
			extendedDataEntitySet.Parse(new DynamicObject[]
			{
				this.Model.DataObject
			}, base.View.BusinessInfo);
			foreach (BillLotEditPlugIn.LotFieldInfo lotFieldInfo in this._lotFields)
			{
				ExtendedDataEntity[] rowDatas = extendedDataEntitySet.FindByEntityKey(lotFieldInfo.Field.EntityKey);
				list.AddRange(this.GetEntityLotInfors(lotFieldInfo, rowDatas));
			}
			if (list.Count > 0)
			{
				CommonServiceHelper.BreakDroppedLots(base.Context, list.ToArray());
			}
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00022100 File Offset: 0x00020300
		private IEnumerable<LotTextInfo> GetEntityLotInfors(BillLotEditPlugIn.LotFieldInfo field, IEnumerable<ExtendedDataEntity> rowDatas)
		{
			List<LotTextInfo> list = new List<LotTextInfo>();
			if (rowDatas == null || rowDatas.Count<ExtendedDataEntity>() < 0)
			{
				return list;
			}
			string lotText = "";
			long materialid = 0L;
			string text = field.Field.ControlFieldKey + "." + FormConst.MASTER_ID;
			string text2 = field.Field.OrgFieldKey + ".Id";
			foreach (ExtendedDataEntity extendedDataEntity in rowDatas)
			{
				object obj = field.Field.TextDynamicProperty.GetValue(extendedDataEntity.DataEntity);
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					lotText = obj.ToString();
					obj = CalcExprParser.GetExpressionValue(extendedDataEntity.DataEntity, base.View.BusinessInfo, field.Field.EntityKey, base.Context, text);
					if (Convert.ToInt64(obj) != 0L)
					{
						materialid = Convert.ToInt64(obj);
						obj = CalcExprParser.GetExpressionValue(extendedDataEntity.DataEntity, base.View.BusinessInfo, field.Field.EntityKey, base.Context, text2);
						if (Convert.ToInt64(obj) != 0L)
						{
							long orgId = Convert.ToInt64(obj);
							if (!list.Exists((LotTextInfo p) => p.LotText == lotText && p.MaterialMasterId == materialid))
							{
								list.Add(new LotTextInfo
								{
									LotText = lotText,
									OrgId = orgId,
									MaterialMasterId = materialid
								});
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x000222F8 File Offset: 0x000204F8
		private void BreakRowLotText(DataChangedEventArgs e)
		{
			BillLotEditPlugIn.LotFieldInfo lotFieldInfo = this._lotFields.SingleOrDefault((BillLotEditPlugIn.LotFieldInfo p) => StringUtils.EqualsIgnoreCase(p.FieldKey, e.Field.Key));
			if (lotFieldInfo == null)
			{
				return;
			}
			if (e.NewValue != null && !string.IsNullOrWhiteSpace(e.NewValue.ToString()))
			{
				this._lotDirty = true;
			}
			if (!StringUtils.EqualsIgnoreCase(lotFieldInfo.FieldKey, this._fieldKey) || this._index != e.Row || string.IsNullOrWhiteSpace(this._oldValue))
			{
				return;
			}
			string oldValue = this._oldValue;
			DynamicObject dynamicObject = this.Model.GetValue(lotFieldInfo.Field.ControlFieldKey, e.Row) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]) == 0L)
			{
				return;
			}
			long materialMasterId = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
			dynamicObject = (this.Model.GetValue(lotFieldInfo.Field.OrgFieldKey, e.Row) as DynamicObject);
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) == 0L)
			{
				return;
			}
			long orgId = Convert.ToInt64(dynamicObject["Id"]);
			LotTextInfo[] array = new LotTextInfo[]
			{
				new LotTextInfo
				{
					LotText = oldValue,
					MaterialMasterId = materialMasterId,
					OrgId = orgId
				}
			};
			CommonServiceHelper.BreakDroppedLots(base.Context, array);
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x000224BC File Offset: 0x000206BC
		private void BreakRowLotText(AfterDeleteRowEventArgs e)
		{
			string value = e.EntityKey.ToUpperInvariant();
			DynamicObject dataEntity = e.DataEntity;
			List<LotTextInfo> list = new List<LotTextInfo>();
			foreach (BillLotEditPlugIn.LotFieldInfo lotFieldInfo in this._lotFields)
			{
				if (lotFieldInfo.FieldEntity.Key.ToUpperInvariant().Equals(value))
				{
					object value2 = lotFieldInfo.Field.TextDynamicProperty.GetValue(dataEntity);
					if (value2 != null)
					{
						string lotText = value2.ToString();
						if (!string.IsNullOrWhiteSpace(lotText))
						{
							long materialid = 0L;
							BaseDataField baseDataField = base.View.BusinessInfo.GetField(lotFieldInfo.Field.ControlFieldKey) as BaseDataField;
							value2 = baseDataField.DynamicProperty.GetValue(dataEntity);
							if (value2 != null)
							{
								DynamicObject dynamicObject = value2 as DynamicObject;
								if (dynamicObject != null && Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]) != 0L)
								{
									materialid = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
									long orgid = 0L;
									OrgField orgField = base.View.BusinessInfo.GetField(lotFieldInfo.Field.OrgFieldKey) as OrgField;
									if (orgField.EntityKey.ToUpperInvariant().Equals(value))
									{
										value2 = orgField.DynamicProperty.GetValue(dataEntity);
									}
									else
									{
										dynamicObject = (this.Model.GetValue(lotFieldInfo.Field.OrgFieldKey, e.Row) as DynamicObject);
									}
									if (value2 != null)
									{
										dynamicObject = (value2 as DynamicObject);
										if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) != 0L)
										{
											orgid = Convert.ToInt64(dynamicObject["Id"]);
											if (list.FirstOrDefault((LotTextInfo p) => StringUtils.EqualsIgnoreCase(p.LotText, lotText) && p.MaterialMasterId == materialid && p.OrgId == orgid) == null)
											{
												list.Add(new LotTextInfo
												{
													LotText = lotText,
													MaterialMasterId = materialid,
													OrgId = orgid
												});
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (list.Count > 0)
			{
				CommonServiceHelper.BreakDroppedLots(base.Context, list.ToArray());
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00022794 File Offset: 0x00020994
		private void CollectLotFieldsInfo()
		{
			from p in base.View.BusinessInfo.GetFieldList()
			where p is LotField
			select p;
			this._lotFields.Clear();
			this._lotFields.AddRange(from p in base.View.BusinessInfo.GetFieldList()
			where p is LotField
			select new BillLotEditPlugIn.LotFieldInfo
			{
				Field = (LotField)p,
				FieldEntity = p.Entity,
				FieldKey = p.Key,
				IsEntryEntity = (p.Entity is EntryEntity)
			});
			if (this._lotFields.Count > 0)
			{
				this._haveLotField = true;
			}
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x00022874 File Offset: 0x00020A74
		private void RecordOldLottext(BeforeUpdateValueEventArgs e)
		{
			this._index = -1;
			this._fieldKey = "";
			this._oldValue = "";
			BillLotEditPlugIn.LotFieldInfo lotFieldInfo = this._lotFields.SingleOrDefault((BillLotEditPlugIn.LotFieldInfo p) => StringUtils.EqualsIgnoreCase(p.FieldKey, e.Key));
			if (lotFieldInfo == null)
			{
				return;
			}
			DynamicObject dynamicObject;
			if (lotFieldInfo.FieldEntity is EntryEntity)
			{
				dynamicObject = this.Model.GetEntityDataObject(lotFieldInfo.FieldEntity, e.Row);
			}
			else if (lotFieldInfo.FieldEntity is SubEntryEntity)
			{
				EntryEntity parentEntity = ((SubEntryEntity)lotFieldInfo.FieldEntity).ParentEntity;
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(parentEntity, base.View.Model.GetEntryCurrentRowIndex(parentEntity.Key));
				DynamicObjectCollection dynamicObjectCollection = entityDataObject[lotFieldInfo.FieldEntity.EntryName] as DynamicObjectCollection;
				if (ObjectUtils.IsNullOrEmpty(dynamicObjectCollection) || dynamicObjectCollection.Count < e.Row)
				{
					return;
				}
				dynamicObject = dynamicObjectCollection[e.Row];
			}
			else if (lotFieldInfo.FieldEntity is SubHeadEntity)
			{
				DynamicObjectCollection dynamicObjectCollection2 = this.Model.DataObject[lotFieldInfo.FieldEntity.EntryName] as DynamicObjectCollection;
				if (ObjectUtils.IsNullOrEmpty(dynamicObjectCollection2) || dynamicObjectCollection2.Count < 1)
				{
					return;
				}
				dynamicObject = dynamicObjectCollection2[0];
			}
			else
			{
				dynamicObject = this.Model.DataObject;
			}
			object value = lotFieldInfo.Field.TextDynamicProperty.GetValue(dynamicObject);
			if (value != null)
			{
				this._oldValue = value.ToString();
			}
			this._index = e.Row;
			this._fieldKey = e.Key;
		}

		// Token: 0x040000FA RID: 250
		private List<BillLotEditPlugIn.LotFieldInfo> _lotFields = new List<BillLotEditPlugIn.LotFieldInfo>();

		// Token: 0x040000FB RID: 251
		private bool _haveLotField;

		// Token: 0x040000FC RID: 252
		private string _fieldKey = "";

		// Token: 0x040000FD RID: 253
		private string _oldValue = "";

		// Token: 0x040000FE RID: 254
		private int _index;

		// Token: 0x040000FF RID: 255
		private bool _lotDirty;

		// Token: 0x04000100 RID: 256
		private bool _noUseLotBroken;

		// Token: 0x04000101 RID: 257
		private bool isPush;

		// Token: 0x02000047 RID: 71
		private class LotFieldInfo
		{
			// Token: 0x1700001C RID: 28
			// (get) Token: 0x060002CD RID: 717 RVA: 0x00022A58 File Offset: 0x00020C58
			// (set) Token: 0x060002CE RID: 718 RVA: 0x00022A60 File Offset: 0x00020C60
			public string FieldKey { get; set; }

			// Token: 0x1700001D RID: 29
			// (get) Token: 0x060002CF RID: 719 RVA: 0x00022A69 File Offset: 0x00020C69
			// (set) Token: 0x060002D0 RID: 720 RVA: 0x00022A71 File Offset: 0x00020C71
			public LotField Field { get; set; }

			// Token: 0x1700001E RID: 30
			// (get) Token: 0x060002D1 RID: 721 RVA: 0x00022A7A File Offset: 0x00020C7A
			// (set) Token: 0x060002D2 RID: 722 RVA: 0x00022A82 File Offset: 0x00020C82
			public Entity FieldEntity { get; set; }

			// Token: 0x1700001F RID: 31
			// (get) Token: 0x060002D3 RID: 723 RVA: 0x00022A8B File Offset: 0x00020C8B
			// (set) Token: 0x060002D4 RID: 724 RVA: 0x00022A93 File Offset: 0x00020C93
			public bool IsEntryEntity { get; set; }
		}
	}
}

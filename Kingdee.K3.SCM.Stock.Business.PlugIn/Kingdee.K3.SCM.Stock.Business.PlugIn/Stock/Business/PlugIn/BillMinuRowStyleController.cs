using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000048 RID: 72
	[Description("单据负库存数据样式控制插件")]
	public class BillMinuRowStyleController : AbstractBillPlugIn
	{
		// Token: 0x060002D6 RID: 726 RVA: 0x00022AA4 File Offset: 0x00020CA4
		public override void AfterCreateNewData(EventArgs e)
		{
			this._operationId = "";
			this._oldRows = new Dictionary<string, List<int>>();
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00022ABC File Offset: 0x00020CBC
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			this.UpdateEntityMinusStyleAfterDo(e.EntityKey);
			this._clearAllEntry.Add(e.EntityKey);
			if (this._oldRows.ContainsKey(e.EntityKey))
			{
				this._oldRows.Remove(e.EntityKey);
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x00022B0B File Offset: 0x00020D0B
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.UpdateEntityMinusStyleAfterDo(e.Entity.Key);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x00022B20 File Offset: 0x00020D20
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (this._oldRows == null || this._oldRows.Values.Count < 1)
			{
				this._beenMinuRowChecked = false;
				this._retData = null;
				return;
			}
			if (!e.Operation.FormOperation.Id.Equals(this._operationId))
			{
				return;
			}
			this._retData = null;
			this.RestoreBackColor();
			this._operationId = "";
		}

		// Token: 0x060002DA RID: 730 RVA: 0x00022B90 File Offset: 0x00020D90
		public override void AfterConfirmOperation(AfterConfirmOperationEventArgs e)
		{
			if (e.FormResult.ReturnData == null)
			{
				base.AfterConfirmOperation(e);
				return;
			}
			this._retData = (e.FormResult.ReturnData as DynamicObject);
			if (this._retData == null)
			{
				base.AfterConfirmOperation(e);
				return;
			}
			if (!this._retData.DynamicObjectType.Name.Equals("MinusCheckResult"))
			{
				base.AfterConfirmOperation(e);
				return;
			}
			this._operationId = e.formOperation.Id;
			this.SetBillMinusRowStyle(this._retData, "");
			this._beenMinuRowChecked = true;
			base.AfterConfirmOperation(e);
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00022C2C File Offset: 0x00020E2C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			string eventName;
			if ((eventName = e.EventName) != null)
			{
				if (!(eventName == "BatchReturnUpGrid"))
				{
					return;
				}
				object obj = null;
				if (base.View.Session.ContainsKey(e.EventName))
				{
					obj = base.View.Session[e.EventName];
				}
				if (this._retData != null && obj != null && "1".Equals(obj.ToString()))
				{
					string eventArgs = e.EventArgs;
					this.SetBillMinusRowStyle(this._retData, eventArgs);
				}
			}
		}

		// Token: 0x060002DC RID: 732 RVA: 0x00022D7C File Offset: 0x00020F7C
		private void SetBillMinusRowStyle(DynamicObject retData, string entityKey = "")
		{
			DynamicObjectCollection dynamicObjectCollection = retData["Entry"] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count < 1)
			{
				return;
			}
			string billFormId = base.View.BusinessInfo.GetForm().Id;
			IEnumerable<string> enumerable = from p in dynamicObjectCollection
			where p["BillFormId"].ToString() == billFormId
			select p["EntityKey"].ToString();
			if (enumerable == null)
			{
				return;
			}
			IEnumerable<string> enumerable2 = enumerable.Distinct<string>();
			long billId = Convert.ToInt64(this.Model.DataObject["Id"]);
			using (IEnumerator<string> enumerator = enumerable2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string curEtyKey = enumerator.Current;
					if (string.IsNullOrWhiteSpace(entityKey) || curEtyKey.Equals(entityKey))
					{
						IEnumerable<long> enumerable3 = from p in dynamicObjectCollection
						where p["BillFormId"].ToString() == billFormId && Convert.ToInt64(p["BillId"]) == billId && p["EntityKey"].ToString() == curEtyKey
						select Convert.ToInt64(p["EntryId"]);
						if (enumerable3 != null)
						{
							this.UpdateEntiryMinusStyle(curEtyKey, enumerable3.ToArray<long>());
						}
					}
				}
			}
		}

		// Token: 0x060002DD RID: 733 RVA: 0x00022EF0 File Offset: 0x000210F0
		private void UpdateEntiryMinusStyle(string entityKey, long[] entryIds)
		{
			if (string.IsNullOrWhiteSpace(entityKey) || entryIds.Length < 1)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity(entityKey);
			if (entity is SubEntryEntity || entity is EntryEntity)
			{
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				EntryGrid entryGrid = base.View.GetControl(entity.Key) as EntryGrid;
				int entryRowCount = this.Model.GetEntryRowCount(entityKey);
				List<int> list2 = new List<int>();
				List<long> list3 = new List<long>();
				if (entity is SubEntryEntity)
				{
					this._subEntryKey = entityKey;
				}
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, i);
					long num = Convert.ToInt64(entityDataObject["Id"]);
					if (entryIds.Contains(num))
					{
						list.Add(new KeyValuePair<int, string>(i, "#FFEC6E"));
						list2.Add(i);
						list3.Add(num);
					}
				}
				this._colorEntry[entityKey] = list3;
				this._oldRows[entityKey] = list2;
				if (list.Count > 0)
				{
					entryGrid.SetRowBackcolor(list);
				}
			}
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0002300C File Offset: 0x0002120C
		private void RestoreBackColor()
		{
			foreach (string text in this._oldRows.Keys)
			{
				List<int> list = this._oldRows[text];
				if (list != null && list.Count > 0)
				{
					Entity entity = base.View.BusinessInfo.GetEntity(text);
					if (entity is SubEntryEntity || entity is EntryEntity)
					{
						List<KeyValuePair<int, string>> list2 = new List<KeyValuePair<int, string>>();
						EntryGrid entryGrid = base.View.GetControl(entity.Key) as EntryGrid;
						foreach (int key in list)
						{
							list2.Add(new KeyValuePair<int, string>(key, ""));
						}
						if (list2.Count > 0)
						{
							entryGrid.SetRowBackcolor(list2);
						}
					}
				}
			}
			this._oldRows.Clear();
			if (this._clearAllEntry != null && this._clearAllEntry.Count > 0)
			{
				foreach (string text2 in this._clearAllEntry)
				{
					Entity entity2 = base.View.BusinessInfo.GetEntity(text2);
					if (entity2 is SubEntryEntity || entity2 is EntryEntity)
					{
						List<KeyValuePair<int, string>> list3 = new List<KeyValuePair<int, string>>();
						int entryRowCount = this.Model.GetEntryRowCount(text2);
						if (entryRowCount > 0)
						{
							EntryGrid entryGrid2 = base.View.GetControl(text2) as EntryGrid;
							for (int i = 0; i < entryRowCount; i++)
							{
								list3.Add(new KeyValuePair<int, string>(i, ""));
							}
							entryGrid2.SetRowBackcolor(list3);
						}
					}
				}
				this._clearAllEntry.Clear();
			}
		}

		// Token: 0x060002DF RID: 735 RVA: 0x00023218 File Offset: 0x00021418
		private void UpdateEntityMinusStyleAfterDo(string entityKey)
		{
			if (!this._colorEntry.ContainsKey(entityKey))
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity(entityKey);
			if (entity is SubEntryEntity || entity is EntryEntity)
			{
				EntryGrid entryGrid = base.View.GetControl(entity.Key) as EntryGrid;
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				int entryRowCount = this.Model.GetEntryRowCount(entityKey);
				if (entity is SubEntryEntity)
				{
					this._subEntryKey = entityKey;
				}
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, i);
					long value = Convert.ToInt64(entityDataObject["Id"]);
					if (this._colorEntry[entityKey].Contains(value))
					{
						list.Add(new KeyValuePair<int, string>(i, "#FFEC6E"));
					}
					else
					{
						list.Add(new KeyValuePair<int, string>(i, ""));
					}
				}
				entryGrid.SetRowBackcolor(list);
			}
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0002331C File Offset: 0x0002151C
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			List<Entity> isNotSubEntity = base.View.BusinessInfo.GetIsNotSubEntity();
			Entity entity = isNotSubEntity.FirstOrDefault((Entity t) => t.EntityType == 1);
			if (entity != null && e.Key.Equals(entity.Key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(this._subEntryKey))
			{
				this.UpdateEntityMinusStyleAfterDo(this._subEntryKey);
			}
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x00023398 File Offset: 0x00021598
		public override void AfterSave(AfterSaveEventArgs e)
		{
			List<string> list = (from c in this._colorEntry
			select c.Key).ToList<string>();
			if (!this._beenMinuRowChecked)
			{
				this._clearAllEntry = list;
				this.RestoreBackColor();
				return;
			}
			foreach (string entityKey in list)
			{
				this.UpdateEntityMinusStyleAfterDo(entityKey);
			}
		}

		// Token: 0x04000109 RID: 265
		private Dictionary<string, List<int>> _oldRows = new Dictionary<string, List<int>>();

		// Token: 0x0400010A RID: 266
		private string _operationId = "";

		// Token: 0x0400010B RID: 267
		private List<string> _clearAllEntry = new List<string>();

		// Token: 0x0400010C RID: 268
		private Dictionary<string, List<long>> _colorEntry = new Dictionary<string, List<long>>();

		// Token: 0x0400010D RID: 269
		private string _subEntryKey = "";

		// Token: 0x0400010E RID: 270
		private bool _beenMinuRowChecked;

		// Token: 0x0400010F RID: 271
		private DynamicObject _retData;
	}
}

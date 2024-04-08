using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200005B RID: 91
	[Description("序列号批量过滤表单插件")]
	public class StockSerialBatchSelectorEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060003FF RID: 1023 RVA: 0x000302F4 File Offset: 0x0002E4F4
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			object obj = null;
			this.View.ParentFormView.Session.TryGetValue("snDatas", out obj);
			List<string> list = obj as List<string>;
			if (list == null || list.Count < 1)
			{
				return;
			}
			this._tranId = "";
			obj = null;
			this.View.ParentFormView.Session.TryGetValue("tranId", out obj);
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				this._tranId = obj.ToString();
			}
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InputEntry"] as DynamicObjectCollection;
			DynamicProperty dynamicProperty = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["GenText"];
			if (dynamicObjectCollection.Count < list.Count)
			{
				this.Model.BatchCreateNewEntryRow("FInputEntry", list.Count - dynamicObjectCollection.Count);
			}
			int num = 0;
			foreach (string text in list)
			{
				dynamicProperty.SetValueFast(dynamicObjectCollection[num], text);
				num++;
			}
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00030448 File Offset: 0x0002E648
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FGenText"))
				{
					return;
				}
				this._isDirty = true;
				this.Model.SetValue("FExist", "", e.Row);
			}
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0003049C File Offset: 0x0002E69C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBOK")
				{
					this.CloseForm("1");
					return;
				}
				if (a == "TBEXIT")
				{
					this.CloseForm("0");
					return;
				}
				if (!(a == "TBCLEAR"))
				{
					if (!(a == "TBCHECK"))
					{
						return;
					}
					this.CheckSnNumbers();
				}
				else
				{
					DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InputEntry"] as DynamicObjectCollection;
					if (dynamicObjectCollection.Count < 1)
					{
						return;
					}
					int num = 0;
					DynamicProperty dynamicProperty = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["GenText"];
					DynamicProperty dynamicProperty2 = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["Exist"];
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						dynamicProperty.SetValueFast(dynamicObject, "");
						dynamicProperty2.SetValueFast(dynamicObject, "");
						num++;
					}
					this.View.UpdateView("FInputEntry");
					this._isDirty = true;
					return;
				}
			}
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00030608 File Offset: 0x0002E808
		public override void FireEntityBlockPasting(EntityBlockPastingEventArgs e)
		{
			base.FireEntityBlockPasting(e);
			this._isDirty = true;
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00030618 File Offset: 0x0002E818
		private void CloseForm(string retValue)
		{
			if (retValue.Equals("1"))
			{
				List<string> snList = this.GetSnList();
				this.View.ParentFormView.Session["snDatas"] = snList;
				if (this._isDirty && snList.Count > 0)
				{
					this.View.ParentFormView.Session["tranId"] = Guid.NewGuid().ToString().ToUpper();
				}
				this.View.ReturnToParentWindow(new FormResult("1"));
				this.View.Close();
				return;
			}
			this.View.ReturnToParentWindow(new FormResult("0"));
			this.View.Close();
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x000306DC File Offset: 0x0002E8DC
		private void CheckSnNumbers()
		{
			List<string> snList = this.GetSnList();
			if (snList.Count > 0)
			{
				this._tranId = Guid.NewGuid().ToString().ToUpper();
				Dictionary<string, int> dictionary = StockServiceHelper.CheckSnNumberExixts(this.View.Context, this._tranId, snList);
				if (dictionary != null)
				{
					DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InputEntry"] as DynamicObjectCollection;
					DynamicProperty dynamicProperty = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["GenText"];
					DynamicProperty dynamicProperty2 = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["Exist"];
					string text = ResManager.LoadKDString("是", "004023030005539", 5, new object[0]);
					string text2 = ResManager.LoadKDString("否", "004023000013912", 5, new object[0]);
					List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
					int num = 0;
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						object valueFast = dynamicProperty.GetValueFast(dynamicObject);
						if (valueFast != null && !string.IsNullOrWhiteSpace(valueFast.ToString()))
						{
							int num2 = -1;
							dictionary.TryGetValue(valueFast.ToString(), out num2);
							if (num2 > 0)
							{
								dynamicProperty2.SetValueFast(dynamicObject, text);
							}
							else
							{
								dynamicProperty2.SetValueFast(dynamicObject, text2);
								list.Add(new KeyValuePair<int, string>(num, "#FFEC6E"));
							}
						}
						num++;
					}
					if (list.Count > 0)
					{
						EntryGrid entryGrid = this.View.GetControl("FInputEntry") as EntryGrid;
						entryGrid.SetRowBackcolor(list);
					}
					this.View.UpdateView("FInputEntry");
				}
				this.View.ParentFormView.Session["tranId"] = this._tranId;
				this._isDirty = false;
			}
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x000308E4 File Offset: 0x0002EAE4
		private List<string> GetSnList()
		{
			Dictionary<string, byte> dictionary = new Dictionary<string, byte>();
			List<string> list = new List<string>();
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InputEntry"] as DynamicObjectCollection;
			DynamicProperty dynamicProperty = this.View.BusinessInfo.GetEntryEntity("FInputEntry").DynamicObjectType.Properties["GenText"];
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject != null)
				{
					object valueFast = dynamicProperty.GetValueFast(dynamicObject);
					if (valueFast != null)
					{
						string text = valueFast.ToString();
						if (!string.IsNullOrWhiteSpace(text) && !dictionary.ContainsKey(text))
						{
							list.Add(text);
							dictionary[text] = 0;
						}
					}
				}
			}
			return list;
		}

		// Token: 0x04000173 RID: 371
		private string _tranId = "";

		// Token: 0x04000174 RID: 372
		private bool _isDirty;
	}
}

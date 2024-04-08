using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004E RID: 78
	public class FlexValuesComSetEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000372 RID: 882 RVA: 0x00029DC4 File Offset: 0x00027FC4
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.stockObj = (DynamicObject)e.Paramter.GetCustomParameter("FlexData");
			this.stockObjCopy = (DynamicObject)ObjectUtils.CreateCopy(this.stockObj);
			object customParameter = e.Paramter.GetCustomParameter("Direct");
			if (customParameter != null && !string.IsNullOrWhiteSpace(customParameter.ToString()))
			{
				this._isForbid = (customParameter.ToString() == "Forbid");
			}
		}

		// Token: 0x06000373 RID: 883 RVA: 0x00029E9C File Offset: 0x0002809C
		public override void AfterBindData(EventArgs e)
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.stockObj["StockFlexItem"];
			if (dynamicObjectCollection.Count > 0)
			{
				RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = (RelatedFlexGroupFieldAppearance)this.View.LayoutInfo.GetFieldAppearance("FStockLocId");
				using (List<FieldAppearance>.Enumerator enumerator = relatedFlexGroupFieldAppearance.RelateFlexLayoutInfo.GetFieldAppearances().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						FieldAppearance subFieldAp = enumerator.Current;
						if (dynamicObjectCollection.Any((DynamicObject p) => Convert.ToInt64(p["FlexId_Id"]) > 0L && StringUtils.EqualsIgnoreCase(((DynamicObject)p["FlexId"])["FlexNumber"].ToString(), subFieldAp.Field.FieldName)))
						{
							this.SetFlexFixedColVisable(relatedFlexGroupFieldAppearance.Key, subFieldAp.Key, true);
						}
						else
						{
							this.SetFlexFixedColVisable(relatedFlexGroupFieldAppearance.Key, subFieldAp.Key, false);
						}
					}
				}
			}
			this.Model.SetValue("FContext", this._isForbid);
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00029FB0 File Offset: 0x000281B0
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.Model.SetValue("FStockId", this.stockObjCopy, e.Row);
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0002A048 File Offset: 0x00028248
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBSAVE"))
				{
					return;
				}
				string text = string.Format(ResManager.LoadKDString("是否{0}仓库上的这些仓位组合值？", "004023000019415", 5, new object[0]), this._isForbid ? ResManager.LoadKDString("禁用", "004023000019416", 5, new object[0]) : ResManager.LoadKDString("反禁用", "004023000019417", 5, new object[0]));
				this.View.ShowWarnningMessage("", text, 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						string arg = "";
						if (this.ChecInvLoc(this._isForbid, ref arg))
						{
							this.View.ShowMessage(string.Format(ResManager.LoadKDString("仓位值组合{0}存在非零库存，不允许禁用", "004023030009699", 5, new object[0]), arg), 4);
							return;
						}
						this.SaveSetData();
						this.View.ShowMessage(ResManager.LoadKDString("保存成功", "004023000019418", 5, new object[0]), 0);
					}
				}, 1);
			}
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0002A0F0 File Offset: 0x000282F0
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FStockLocId"))
				{
					return;
				}
				if (e.NewValue != null)
				{
					this.Model.CreateNewEntryRow("FEntity");
				}
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0002A132 File Offset: 0x00028332
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase("FStockLocId", e.FieldKey))
			{
				e.MultiSelect = true;
			}
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0002A188 File Offset: 0x00028388
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase("FStockLocId", e.FieldKey))
			{
				if (e.SelectRows.Count > 1)
				{
					this.ReturnMoreDatas(e.SelectRows, e.Row);
					e.Cancel = true;
					return;
				}
				if (e.SelectRows.Count == 1)
				{
					EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					long newLocId = Convert.ToInt64(e.SelectRows[0].PrimaryKeyValue);
					DynamicObject dynamicObject = entityDataObject.FirstOrDefault((DynamicObject p) => p["StockLocId"] != null && newLocId == Convert.ToInt64(p["StockLocId_Id"]));
					if (dynamicObject != null)
					{
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x06000379 RID: 889 RVA: 0x0002A274 File Offset: 0x00028474
		private void ReturnMoreDatas(ListSelectedRowCollection newLocs, int curIndex)
		{
			if (curIndex < 0)
			{
				return;
			}
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			bool flag = true;
			int count = entityDataObject.Count;
			int num = 0;
			for (int i = count - 1; i >= 0; i--)
			{
				if (entityDataObject[i] != null && entityDataObject[i]["StockLocId"] != null)
				{
					flag = false;
					num = i + 1;
					break;
				}
			}
			if (num <= curIndex)
			{
				num = curIndex + 1;
			}
			bool flag2 = true;
			DynamicObject dynamicObject = entityDataObject[curIndex]["StockLocId"] as DynamicObject;
			if (dynamicObject != null)
			{
				flag2 = false;
			}
			List<DynamicObject> list = new List<DynamicObject>();
			bool flag3 = true;
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			for (int j = 0; j < newLocs.Count; j++)
			{
				long newLocId = Convert.ToInt64(newLocs[j].PrimaryKeyValue);
				bool flag4 = false;
				if (!flag)
				{
					dynamicObject = entityDataObject.FirstOrDefault((DynamicObject p) => p["StockLocId"] != null && newLocId == Convert.ToInt64(p["StockLocId_Id"]));
					if (dynamicObject != null)
					{
						flag4 = true;
					}
				}
				if (!flag4)
				{
					DynamicObject dynamicObject2;
					if (flag3)
					{
						dynamicObject2 = entityDataObject[curIndex];
					}
					else if (num >= count)
					{
						dynamicObject2 = new DynamicObject(dynamicObjectType);
						entityDataObject.Add(dynamicObject2);
						num++;
					}
					else
					{
						dynamicObject2 = entityDataObject[num];
						num++;
					}
					dynamicObject2["StockId"] = this.stockObjCopy;
					dynamicObject2["StockId_Id"] = this.stockObjCopy["Id"];
					dynamicObject2["StockLocId_Id"] = newLocId;
					list.Add(dynamicObject2);
					flag3 = false;
				}
				else if (flag3 && !flag2)
				{
					flag3 = false;
				}
			}
			if (list.Count > 0)
			{
				DynamicObject dynamicObject3 = new DynamicObject(dynamicObjectType);
				dynamicObject3["StockId"] = this.stockObjCopy;
				dynamicObject3["StockLocId_Id"] = 0;
				list.Add(dynamicObject3);
				entityDataObject.Add(dynamicObject3);
				DBServiceHelper.LoadReferenceObject(base.Context, list.ToArray(), dynamicObjectType, false);
				this.View.UpdateView("FEntity");
			}
		}

		// Token: 0x0600037A RID: 890 RVA: 0x0002A4E4 File Offset: 0x000286E4
		private void SaveSetData()
		{
			List<long> list = new List<long>();
			DynamicObjectCollection source = this.Model.DataObject["BD_FLEXVALUESCOM"] as DynamicObjectCollection;
			list = (from p in source
			where p != null && p["StockLocId"] != null
			select Convert.ToInt64(((DynamicObject)p["StockLocId"])["Id"])).Distinct<long>().ToList<long>();
			if (list != null && list.Count > 0)
			{
				StockServiceHelper.ForbidUnForbidStockComLoc(this.View.Context, Convert.ToInt64(this.stockObjCopy["Id"]), list, this._isForbid);
			}
		}

		// Token: 0x0600037B RID: 891 RVA: 0x0002A64C File Offset: 0x0002884C
		private bool ChecInvLoc(bool isForbid, ref string haveInvLocMsg)
		{
			bool result = false;
			haveInvLocMsg = "";
			if (!isForbid)
			{
				return false;
			}
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ForbidLocCheckInv", false);
			if (!Convert.ToBoolean(systemProfile))
			{
				return false;
			}
			DynamicObjectCollection source = this.Model.DataObject["BD_FLEXVALUESCOM"] as DynamicObjectCollection;
			DynamicObject[] array = (from m in source
			where m != null && m["StockLocId"] != null
			select m into n
			select n["StockLocId"] as DynamicObject).ToArray<DynamicObject>();
			if (array != null && array.Length > 0)
			{
				List<long> values = (from m in array
				select Convert.ToInt64(m["Id"])).Distinct<long>().ToList<long>();
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "STK_INVENTORY",
					SelectItems = SelectorItemInfo.CreateItems("FSTOCKLOCID"),
					FilterClauseWihtKey = string.Format("FSTOCKID={1} AND FSTOCKLOCID>0 AND FSTOCKLOCID IN({0}) AND (FBASEQTY<>0 OR FSECQTY<>0)", string.Join<long>(",", values), Convert.ToInt64(this.stockObjCopy["Id"]))
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
				{
					result = true;
					List<long> locIds = (from m in dynamicObjectCollection
					select Convert.ToInt64(m["FSTOCKLOCID"])).Distinct<long>().ToList<long>();
					DynamicObject[] array2 = (from m in array
					where locIds.Exists((long n) => n == Convert.ToInt64(m["Id"]))
					select m).ToArray<DynamicObject>();
					List<string> flexNumbers = this.GetFlexNumbers();
					string separator = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "FlexSplit", ";").ToString();
					foreach (DynamicObject dynamicObject in array2)
					{
						List<string> list = new List<string>();
						List<string> list2 = new List<string>();
						foreach (string text in flexNumbers)
						{
							string text2 = text.Substring(1);
							if (dynamicObject[text2] != null)
							{
								DynamicObject dynamicObject2 = dynamicObject[text2] as DynamicObject;
								list.Add(dynamicObject2["Number"].ToString());
								list2.Add(dynamicObject2["Name"].ToString());
							}
						}
						haveInvLocMsg += string.Format("{0}({1})、", string.Join(separator, list), string.Join(separator, list2));
					}
					if (!string.IsNullOrEmpty(haveInvLocMsg))
					{
						haveInvLocMsg = haveInvLocMsg.TrimEnd(new char[]
						{
							'、'
						});
					}
				}
			}
			return result;
		}

		// Token: 0x0600037C RID: 892 RVA: 0x0002A960 File Offset: 0x00028B60
		private List<string> GetFlexNumbers()
		{
			string text = "SELECT FFLEXNUMBER FROM T_BAS_FLEXVALUES WHERE FDOCUMENTSTATUS = 'C' AND FFLEXNUMBER <> ' '";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, new SqlParam[0]);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				return (from m in dynamicObjectCollection
				select m["FFLEXNUMBER"].ToString().Trim()).ToList<string>();
			}
			return null;
		}

		// Token: 0x0600037D RID: 893 RVA: 0x0002A9C4 File Offset: 0x00028BC4
		private void SetFlexFixedColVisable(string fieldKey, string subFieldKey, bool val)
		{
			string text = string.Format("$${0}__{1}", fieldKey, subFieldKey);
			this.View.StyleManager.SetVisible(text, "", val);
		}

		// Token: 0x0400012A RID: 298
		private DynamicObject stockObj;

		// Token: 0x0400012B RID: 299
		private DynamicObject stockObjCopy;

		// Token: 0x0400012C RID: 300
		private bool _isForbid = true;
	}
}

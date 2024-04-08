using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000013 RID: 19
	public class AdjustLotNo : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00006B7E File Offset: 0x00004D7E
		public override void AfterCreateModelData(EventArgs e)
		{
			base.AfterCreateModelData(e);
			this.LockField();
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00006B90 File Offset: 0x00004D90
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBQUERY")
				{
					this.FillData(true);
					return;
				}
				if (!(a == "TBADJUST"))
				{
					return;
				}
				this.DoLotAdjust();
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00006BDC File Offset: 0x00004DDC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FDIFFTYPE"))
				{
					return;
				}
				this.FillData(true);
			}
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00006C10 File Offset: 0x00004E10
		private void FillData(bool showMsg)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FLotEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			string text = Convert.ToString(this.Model.GetValue("FDiffType"));
			int num = 0;
			if (!string.IsNullOrWhiteSpace(text))
			{
				int.TryParse(text, out num);
			}
			if (num == 0)
			{
				this.View.UpdateView("FLotEntity");
				if (showMsg)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选择调整类型！", "004023030009591", 5, new object[0]), 0);
				}
				return;
			}
			DynamicObjectCollection diffLotmasters = StockServiceHelper.GetDiffLotmasters(this.View.Context, num);
			if (diffLotmasters != null && diffLotmasters.Count > 0)
			{
				DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
				int num2 = 1;
				foreach (DynamicObject dynamicObject in diffLotmasters)
				{
					DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectType);
					dynamicObject2["Seq"] = num2++;
					dynamicObject2["UseOrgId_Id"] = dynamicObject["FUSEORGID"];
					dynamicObject2["MaterialId_Id"] = dynamicObject["FMATERIALID"];
					dynamicObject2["Lot_Id"] = dynamicObject["FLOTID"];
					dynamicObject2["TaskLotNo"] = this.GetTaskLotNo(Convert.ToString(dynamicObject["FNUMBER"]), text);
					entityDataObject.Add(dynamicObject2);
				}
				DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), dynamicObjectType, false);
			}
			this.View.UpdateView("FLotEntity");
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006DD4 File Offset: 0x00004FD4
		private string GetTaskLotNo(string srcNumber, string diffType)
		{
			if (string.IsNullOrWhiteSpace(diffType))
			{
				return srcNumber;
			}
			if (srcNumber == null)
			{
				return null;
			}
			string text = srcNumber;
			if (diffType != null)
			{
				if (!(diffType == "1"))
				{
					if (!(diffType == "2"))
					{
						if (diffType == "3")
						{
							text = text.Trim();
						}
					}
					else
					{
						text = text.ToLowerInvariant();
					}
				}
				else
				{
					text = text.ToUpperInvariant();
				}
			}
			return text;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00006E3C File Offset: 0x0000503C
		private void DoLotAdjust()
		{
			string text = Convert.ToString(this.Model.GetValue("FDiffType"));
			int num = 0;
			if (!string.IsNullOrWhiteSpace(text))
			{
				int.TryParse(text, out num);
			}
			if (num == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选择调整类型！", "004023030009591", 5, new object[0]), 0);
				return;
			}
			Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
			bool flag = false;
			Entity entity = this.View.BusinessInfo.GetEntity("FLotEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				bool flag2 = Convert.ToBoolean(dynamicObject["Select"]);
				if (flag2)
				{
					string text2 = Convert.ToString(dynamicObject["TaskLotNo"]);
					if (!string.IsNullOrWhiteSpace(text2))
					{
						string strB = Convert.ToString(((DynamicObject)dynamicObject["Lot"])["Number"]);
						if (string.Compare(text2, strB, false) != 0)
						{
							long num2 = Convert.ToInt64(dynamicObject["MaterialId_Id"]);
							string key = string.Format("{0}&_(_@{1}", text2, num2);
							List<long> list = null;
							dictionary.TryGetValue(key, out list);
							if (list == null)
							{
								list = new List<long>();
								dictionary[key] = list;
							}
							list.Add(Convert.ToInt64(dynamicObject["Lot_Id"]));
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				List<string> list2 = new List<string>();
				Dictionary<long, string> dictionary2 = StockServiceHelper.AdjustLotMasterNumber(this.View.Context, dictionary, num);
				if (dictionary2 != null)
				{
					if (dictionary2.ContainsKey(-1L))
					{
						this.View.ShowMessage(string.Format(ResManager.LoadKDString("批号调整失败，原因：{0}", "004023030009593", 5, new object[0]), dictionary2[-1L]), 0);
					}
					else
					{
						string text3 = ResManager.LoadKDString("调整后存在物料+组织+批号文本重复的批号主档，调整失败", "004023030009614", 5, new object[0]);
						string text4 = ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]);
						foreach (DynamicObject dynamicObject2 in entityDataObject)
						{
							bool flag3 = Convert.ToBoolean(dynamicObject2["Select"]);
							if (flag3)
							{
								long key2 = Convert.ToInt64(dynamicObject2["Lot_Id"]);
								string value = "";
								dictionary2.TryGetValue(key2, out value);
								if (string.IsNullOrWhiteSpace(value))
								{
									list2.Add(key2.ToString());
									dynamicObject2["RetMsg"] = text4;
								}
								else
								{
									dynamicObject2["RetMsg"] = text3;
								}
							}
						}
						this.View.UpdateView("FRetMsg", -1);
					}
				}
				else
				{
					this.View.ShowMessage(ResManager.LoadKDString("批号主档的批号，以及追溯单据中的相关批号调整成功。", "004023030009594", 5, new object[0]), 0);
					foreach (List<long> list3 in dictionary.Values)
					{
						if (list3 != null && list3.Count > 0)
						{
							foreach (long num3 in list3)
							{
								list2.Add(num3.ToString());
							}
						}
					}
					this.FillData(false);
				}
				this.ClearLotCache(list2);
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("请先选择需要处理的行！", "004023030009592", 5, new object[0]), 0);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00007214 File Offset: 0x00005414
		private void ClearLotCache(List<string> sucIds)
		{
			if (sucIds == null || sucIds.Count < 1)
			{
				return;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "BD_BatchMainFile", true);
			BusinessDataServiceHelper.ClearCache(this.View.Context, formMetadata.BusinessInfo.GetDynamicObjectType(), sucIds);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00007268 File Offset: 0x00005468
		private void LockField()
		{
			this.View.LockField("FUseOrgId", true);
			this.View.LockField("FMaterialId", true);
			this.View.LockField("FLot", true);
			this.View.LockField("FTaskLotNo", true);
		}
	}
}

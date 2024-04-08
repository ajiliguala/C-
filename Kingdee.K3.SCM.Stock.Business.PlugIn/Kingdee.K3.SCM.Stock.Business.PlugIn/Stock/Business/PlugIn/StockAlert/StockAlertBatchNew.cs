using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockAlert
{
	// Token: 0x02000032 RID: 50
	[Description("仓库最大最小安全库存批量新增插件")]
	public class StockAlertBatchNew : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600020F RID: 527 RVA: 0x00019F90 File Offset: 0x00018190
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				this.Model.SetValue("FStockOrgID", base.Context.CurrentOrganizationInfo.ID);
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x00019FE4 File Offset: 0x000181E4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string text = "";
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FStockOrgID"))
				{
					if (!(fieldKey == "FStockId"))
					{
						return;
					}
					if (this.GetFieldFilter(e.FieldKey, out text))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = text;
							return;
						}
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				else if (this.GetFieldFilter(e.FieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0001A0C0 File Offset: 0x000182C0
		private bool GetFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			if (fieldKey != null)
			{
				if (!(fieldKey == "FStockOrgID"))
				{
					if (fieldKey == "FStockId")
					{
						filter = "FAVAILABLEALERT =1";
					}
				}
				else
				{
					filter = string.Format("exists (select 1 from  T_SEC_USERORG tur where fuserid={0} and tur.FORGID=t0.FORGID) \r\n                                             AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                                             WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n                                             AND BSP.FKEY = 'STARTSTOCKDATE') ", base.Context.UserId.ToString());
				}
			}
			return true;
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0001A128 File Offset: 0x00018328
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSave"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("保存", "004023030009255", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.BatchNew();
			}
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0001A1A0 File Offset: 0x000183A0
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FStockId") && !(key == "FMaterialId"))
				{
					return;
				}
				bool flag = this.FindIfRepeat(e);
				if (flag)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("录入记录中已经存在该仓库+物料", "004023000024471", 5, new object[0]), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0001A214 File Offset: 0x00018414
		private void GetUpdateRowValue(BeforeUpdateValueEventArgs e, out string material, out string stock)
		{
			string text = null;
			string text2 = null;
			long num = 0L;
			if (e.Value == null)
			{
				material = null;
				stock = null;
				return;
			}
			string key2;
			if (!long.TryParse(e.Value.ToString(), out num))
			{
				DynamicObject dynamicObject = (DynamicObject)e.Value;
				string key;
				if (dynamicObject != null && (key = e.Key) != null)
				{
					if (!(key == "FMaterialId"))
					{
						if (key == "FStockId")
						{
							text2 = ((dynamicObject["Id"] == null) ? "" : dynamicObject["Id"].ToString());
							DynamicObject dynamicObject2 = (DynamicObject)this.View.Model.GetValue("FMaterialId", e.Row);
							if (dynamicObject2 != null)
							{
								text = ((dynamicObject2["Id"] == null) ? "" : dynamicObject2["Id"].ToString());
							}
						}
					}
					else
					{
						text = ((dynamicObject["Id"] == null) ? "" : dynamicObject["Id"].ToString());
						DynamicObject dynamicObject3 = (DynamicObject)this.View.Model.GetValue("FStockId", e.Row);
						if (dynamicObject3 != null)
						{
							text2 = ((dynamicObject3["Id"] == null) ? "" : dynamicObject3["Id"].ToString());
						}
					}
				}
			}
			else if ((key2 = e.Key) != null)
			{
				if (!(key2 == "FMaterialId"))
				{
					if (key2 == "FStockId")
					{
						text2 = e.Value.ToString();
						DynamicObject dynamicObject4 = (DynamicObject)this.View.Model.GetValue("FMaterialId", e.Row);
						if (dynamicObject4 != null)
						{
							text = ((dynamicObject4["Id"] == null) ? "" : dynamicObject4["Id"].ToString());
						}
					}
				}
				else
				{
					text = e.Value.ToString();
					DynamicObject dynamicObject5 = (DynamicObject)this.View.Model.GetValue("FStockId", e.Row);
					if (dynamicObject5 != null)
					{
						text2 = ((dynamicObject5["Id"] == null) ? "" : dynamicObject5["Id"].ToString());
					}
				}
			}
			material = text;
			stock = text2;
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0001A47C File Offset: 0x0001867C
		private bool FindIfRepeat(BeforeUpdateValueEventArgs e)
		{
			string text = null;
			string text2 = null;
			this.GetUpdateRowValue(e, out text, out text2);
			bool result = false;
			if (text != null && text2 != null)
			{
				Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				for (int i = 0; i < entityDataObject.Count; i++)
				{
					if (i != e.Row)
					{
						DynamicObject dynamicObject = entityDataObject[i];
						DynamicObject dynamicObject2 = (DynamicObject)dynamicObject["MaterialId"];
						DynamicObject dynamicObject3 = (DynamicObject)dynamicObject["StockId"];
						if (dynamicObject2 != null && dynamicObject3 != null)
						{
							string b = (dynamicObject2["Id"] == null) ? "" : dynamicObject2["Id"].ToString();
							string b2 = (dynamicObject3["Id"] == null) ? "" : dynamicObject3["Id"].ToString();
							if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text2) && string.Equals(text, b) && string.Equals(text2, b2))
							{
								return true;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0001A5B4 File Offset: 0x000187B4
		private void BatchNew()
		{
			List<StockAlertParam> list = new List<StockAlertParam>();
			long num = Convert.ToInt64(this.Model.DataObject["StockOrg_Id"]);
			if (num == 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("库存组织不能为空！", "004023000024472", 5, new object[0]), "", 0);
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			if (entityDataObject.Count == 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("表体不能为空！", "004023000024473", 5, new object[0]), "", 0);
				return;
			}
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				long fid = Convert.ToInt64(dynamicObject["Id"]);
				long num2 = Convert.ToInt64(dynamicObject["StockId_Id"]);
				long num3 = Convert.ToInt64(dynamicObject["MaterialId_Id"]);
				long baseUnitId = Convert.ToInt64(dynamicObject["BaseUnitId_Id"]);
				decimal minStock = Convert.ToDecimal(dynamicObject["MinStock"]);
				decimal safeStock = Convert.ToDecimal(dynamicObject["SafeStock"]);
				decimal reorderGood = Convert.ToDecimal(dynamicObject["ReorderGood"]);
				decimal econReorderQty = Convert.ToDecimal(dynamicObject["EconReorderQty"]);
				string description = Convert.ToString(dynamicObject["Description"]);
				decimal maxStock = Convert.ToDecimal(dynamicObject["MaxStock"]);
				int rowId = Convert.ToInt32(dynamicObject["Seq"]);
				if (num2 == 0L || num3 == 0L)
				{
					this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录的仓库和物料编码不允许为空！", "004023000024474", 5, new object[0]), dynamicObject["Seq"]), "", 0);
					return;
				}
				list.Add(new StockAlertParam
				{
					FID = fid,
					StockOrg = num,
					StockId = num2,
					MaterialId = num3,
					BaseUnitId = baseUnitId,
					MinStock = minStock,
					SafeStock = safeStock,
					ReorderGood = reorderGood,
					EconReorderQty = econReorderQty,
					MaxStock = maxStock,
					Description = description,
					RowId = rowId
				});
			}
			IOperationResult operationResult = StockServiceHelper.BatchNew(base.Context, list);
			OperateResultCollection operateResult = operationResult.OperateResult;
			if (operationResult.SuccessDataEnity != null)
			{
				IEnumerable<DynamicObject> successDataEnity = operationResult.SuccessDataEnity;
				foreach (DynamicObject dynamicObject2 in successDataEnity)
				{
					long num4 = Convert.ToInt64(dynamicObject2["Id"]);
					int num5 = Convert.ToInt32(dynamicObject2["RowId"]);
					this.View.Model.SetValue("FID", num4, num5 - 1);
				}
			}
			DynamicObjectCollection entityDataObject2 = this.View.Model.GetEntityDataObject(entity);
			if (operationResult.ValidationErrors.Count<ValidationErrorInfo>() <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("保存成功", "004023000019418", 5, new object[0]), 0);
				return;
			}
			for (int i = 0; i < operateResult.Count; i++)
			{
				int num6 = 0;
				foreach (DynamicObject dynamicObject3 in entityDataObject2)
				{
					if (dynamicObject3["Id"] != null && operateResult[i].PKValue.ToString() == dynamicObject3["Id"].ToString())
					{
						num6 = Convert.ToInt32(dynamicObject3["Seq"]);
						break;
					}
				}
				operateResult[i].Message = string.Format(ResManager.LoadKDString("第{0}条分录保存成功!", "004023000024475", 5, new object[0]), num6);
			}
			foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
			{
				OperateResult item = new OperateResult
				{
					DataEntityIndex = validationErrorInfo.DataEntityIndex,
					PKValue = validationErrorInfo.BillPKID,
					RowIndex = validationErrorInfo.RowIndex,
					Name = validationErrorInfo.Title,
					SuccessStatus = false,
					Message = validationErrorInfo.Message,
					MessageType = ((validationErrorInfo.Level == 1) ? 1 : 0)
				};
				if (!operateResult.Contains(item))
				{
					operateResult.Add(item);
				}
			}
			if (operateResult.Count > 0)
			{
				this.View.ShowOperateResult(operateResult, "BOS_BatchTips");
			}
		}
	}
}

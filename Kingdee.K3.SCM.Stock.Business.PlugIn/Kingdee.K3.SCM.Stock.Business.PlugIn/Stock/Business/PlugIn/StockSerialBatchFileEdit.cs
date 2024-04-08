using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200002C RID: 44
	[Description("序列号主档批量归档插件")]
	public class StockSerialBatchFileEdit : AbstractListPlugIn
	{
		// Token: 0x060001AA RID: 426 RVA: 0x00014F34 File Offset: 0x00013134
		public override void BeforeBindData(EventArgs e)
		{
			this.SetStockOrg();
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00014F3C File Offset: 0x0001313C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKORGID"))
				{
					return;
				}
				this.SelOrgId = e.NewValue.ToString();
				this.SetDctSelOrgId();
			}
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00014F84 File Offset: 0x00013184
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string empty = string.Empty;
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null && (a == "FMATERIALIDFROM" || a == "FMATERIALIDTO"))
			{
				e.IsShowUsed = false;
				if (this.SelOrgId.Length == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "004024030000910", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
				if (listShowParameter != null)
				{
					listShowParameter.MutilListUseOrgId = this.SelOrgId;
					listShowParameter.UseOrgId = 0L;
				}
			}
			if (!string.IsNullOrEmpty(empty))
			{
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = empty;
					return;
				}
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter = listFilterParameter.Filter + " AND " + empty;
			}
		}

		// Token: 0x060001AD RID: 429 RVA: 0x0001506C File Offset: 0x0001326C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			if (!(e.BaseDataField is BaseDataField))
			{
				return;
			}
			if (e.BaseDataFieldKey.ToUpper() == "FMATERIALIDFROM" || e.BaseDataFieldKey.ToUpper() == "FMATERIALIDTO")
			{
				e.IsShowUsed = false;
			}
			string filter = e.Filter;
			this.SetOrgForBaseData(e.BaseDataFieldKey, ref filter);
			e.Filter = filter;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x000150DC File Offset: 0x000132DC
		private bool SetOrgForBaseData(string fieldKey, ref string eFilter)
		{
			string text = string.Empty;
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && (a == "FMATERIALIDFROM" || a == "FMATERIALIDTO"))
			{
				if (this.SelOrgId.Length == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "004024030000910", 5, new object[0]), 0);
					return true;
				}
				text = this.GetOrgFilter();
			}
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			if (string.IsNullOrEmpty(eFilter))
			{
				eFilter = text;
			}
			else
			{
				eFilter = eFilter + " AND " + text;
			}
			return false;
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0001517C File Offset: 0x0001337C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBFILE"))
				{
					return;
				}
				if (!this.VaildatePermission("BD_SerialMainFile", "00505694265cb6cf11e3b590a99d8e2e"))
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("对不起，您没有序列号主档的归档权限!", "004023000013913", 5, new object[0]), "", 0);
					return;
				}
				if (this.DctSelOrg.Count == 0)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("请选择库存组织!", "004023000022545", 5, new object[0]), "", 0);
					return;
				}
				List<long> list = (from item in this.DctSelOrg
				select item.Key).ToList<long>();
				DynamicObject dynamicObject = this.View.Model.GetValue("FMaterialIdFrom") as DynamicObject;
				DynamicObject dynamicObject2 = this.View.Model.GetValue("FMaterialIdTo") as DynamicObject;
				string text = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Number"]);
				string text2 = (dynamicObject2 == null) ? "" : Convert.ToString(dynamicObject2["Number"]);
				DynamicObject dynamicObject3 = this.View.Model.GetValue("FLotFrom") as DynamicObject;
				DynamicObject dynamicObject4 = this.View.Model.GetValue("FLotTo") as DynamicObject;
				string text3 = (dynamicObject3 == null) ? "" : Convert.ToString(dynamicObject3["Number"]);
				string text4 = (dynamicObject4 == null) ? "" : Convert.ToString(dynamicObject4["Number"]);
				string text5 = Convert.ToString(this.View.Model.GetValue("FDate"));
				if (text == "" && text3 == "" && text5 == "")
				{
					this.View.ShowMessage(ResManager.LoadKDString("物料、批号、已出库时间三者至少指定一个范围！", "004023000022547", 5, new object[0]), 0);
					return;
				}
				string text6 = "";
				if (!string.IsNullOrEmpty(text5))
				{
					DateTime dateTime;
					if (!DateTime.TryParse(text5, out dateTime))
					{
						this.View.ShowMessage(ResManager.LoadKDString("已出库时间格式异常！", "004023000022548", 5, new object[0]), 0);
						return;
					}
					text6 = dateTime.AddDays(1.0).ToString("yyyy-MM-dd HH:mm:ss");
				}
				List<long> list2 = StockServiceHelper.SerialBatchFileQuery(base.Context, list, text, text2, text3, text4, text6);
				if (list2.Count <= 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("未找到要归档的序列号！", "004023000022546", 5, new object[0]), 0);
					return;
				}
				int num = 100000;
				int num2 = 0;
				string format = ResManager.LoadKDString("共{0}个序列号归档成功！", "004023000022544", 5, new object[0]);
				if (list2.Count < num)
				{
					if (StockSerialBatchFileEdit.ConvertSerials(this.View, false, list2, out num2))
					{
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = ResManager.LoadKDString("序列号归档！", "004023000013915", 5, new object[0]),
							Environment = 3,
							OperateName = ResManager.LoadKDString("序列号归档", "004023000013916", 5, new object[0]),
							SubSystemId = "21"
						});
						this.View.Refresh();
					}
					if (num2 > 0)
					{
						this.View.ShowMessage(string.Format(format, num2), 0);
						return;
					}
				}
				else
				{
					int num3 = (list2.Count - 1) / num + 1;
					for (int i = 0; i < num3; i++)
					{
						int num4 = i * num;
						int num5 = (i < num3 - 1) ? num : (list2.Count - i * num);
						List<long> list3 = new List<long>();
						for (int j = num4; j < num4 + num5; j++)
						{
							list3.Add(list2[j]);
						}
						int num6 = 0;
						StockSerialBatchFileEdit.ConvertSerials(this.View, false, list3, out num6);
						num2 += num6;
					}
					if (num2 > 0)
					{
						this.View.ShowMessage(string.Format(format, num2), 0);
					}
					this.Model.WriteLog(new LogObject
					{
						ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
						Description = ResManager.LoadKDString("序列号归档！", "004023000013915", 5, new object[0]),
						Environment = 3,
						OperateName = ResManager.LoadKDString("序列号归档", "004023000013916", 5, new object[0]),
						SubSystemId = "21"
					});
					this.View.Refresh();
				}
			}
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00015658 File Offset: 0x00013858
		internal static bool ConvertSerials(IDynamicFormView view, bool isRestore, List<long> serialIds, out int successCount)
		{
			successCount = 0;
			if (serialIds.Count < 0)
			{
				return false;
			}
			SerialConvertResult serialConvertResult = StockServiceHelper.ConvertSerialMainFile(view.Context, isRestore, serialIds);
			if (serialConvertResult == null || serialConvertResult.ErrInfos == null || serialConvertResult.ErrInfos.Count < 1)
			{
				return false;
			}
			if (!isRestore)
			{
				ResManager.LoadKDString("归档", "004023000013918", 5, new object[0]);
			}
			else
			{
				ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]);
			}
			if (serialConvertResult.ErrInfos.Count == 1)
			{
				if (!serialConvertResult.Success)
				{
					view.ShowErrMessage(serialConvertResult.ErrInfos[0].ErrMsg, "", 0);
					return false;
				}
				view.ShowMessage(serialConvertResult.ErrInfos[0].ErrMsg, 0);
			}
			else if (serialConvertResult.Success)
			{
				successCount = serialConvertResult.ErrInfos.Count;
			}
			else
			{
				List<FieldAppearance> list = new List<FieldAppearance>();
				FieldAppearance fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(view.Context, "FSerialNo", ResManager.LoadKDString("序列号", "004023000013919", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("100", view.Context.UserLocale.LCID);
				list.Add(fieldAppearance);
				fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(view.Context, "FMsg", ResManager.LoadKDString("结果", "004023000013920", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("700", view.Context.UserLocale.LCID);
				list.Add(fieldAppearance);
				K3DisplayerModel k3DisplayerModel = K3DisplayerModel.Create(view.Context, list.ToArray(), null);
				foreach (OperateErrorInfo operateErrorInfo in serialConvertResult.ErrInfos)
				{
					new K3DisplayerMessage();
					k3DisplayerModel.AddMessage(string.Format("{0}~|~{1}", operateErrorInfo.ErrObjKeyField, operateErrorInfo.ErrMsg));
				}
				k3DisplayerModel.CancelButton.Visible = false;
				view.ShowK3Displayer(k3DisplayerModel, null, "BOS_K3Displayer");
			}
			return true;
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0001587C File Offset: 0x00013A7C
		private void SetStockOrg()
		{
			if (!base.Context.IsMultiOrg)
			{
				this.View.StyleManager.SetEnabled("FStockOrgId", null, false);
			}
			this.InitStkOrgId();
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x000158C8 File Offset: 0x00013AC8
		protected void InitStkOrgId()
		{
			if (this.View.ParentFormView != null)
			{
				this.LstStkOrg = this.GetPermissionOrg(this.View.ParentFormView.BillBusinessInfo.GetForm().Id);
			}
			List<EnumItem> organization = this.GetOrganization(this.View.Context);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(organization);
			this.LstStkOrg = new List<long>();
			foreach (EnumItem enumItem in organization)
			{
				this.LstStkOrg.Add(Convert.ToInt64(enumItem.Value));
			}
			object value = this.Model.GetValue("FStockOrgId");
			if (ObjectUtils.IsNullOrEmpty(value) && organization.Count((EnumItem p) => Convert.ToInt64(p.Value) == base.Context.CurrentOrganizationInfo.ID) > 0 && base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				this.View.Model.SetValue("FStockOrgId", base.Context.CurrentOrganizationInfo.ID);
				this.SelOrgId = base.Context.CurrentOrganizationInfo.ID.ToString();
				if (!this.DctSelOrg.ContainsKey(base.Context.CurrentOrganizationInfo.ID))
				{
					this.DctSelOrg.Add(base.Context.CurrentOrganizationInfo.ID, base.Context.CurrentOrganizationInfo.Name);
				}
			}
			if (ObjectUtils.IsNullOrEmpty(value))
			{
				return;
			}
			if (string.Equals(this.SelOrgId, value.ToString()))
			{
				return;
			}
			this.SelOrgId = value.ToString();
			this.SetDctSelOrgId();
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00015AA0 File Offset: 0x00013CA0
		protected List<long> GetPermissionOrg(string formId)
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = formId,
				PermissionControl = this.View.ParentFormView.BillBusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = this.View.ParentFormView.Model.SubSytemId
			};
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00015B0C File Offset: 0x00013D0C
		protected List<EnumItem> GetOrganization(Context ctx)
		{
			List<EnumItem> list = new List<EnumItem>();
			List<SelectorItemInfo> selectItems = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FORGID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FNAME")
			};
			string text = this.GetInFilter("FORGID", this.LstStkOrg);
			text += string.Format(" AND FORGFUNCTIONS LIKE '%{0}%' ", 103L);
			text += this.GetOtherFilter();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = selectItems,
				FilterClauseWihtKey = text
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType))
				{
					EnumId = dynamicObject["FORGID"].ToString(),
					Value = dynamicObject["FORGID"].ToString()
				};
				long key = (long)dynamicObject["FORGID"];
				string text2 = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(text2, base.Context.UserLocale.LCID);
				list.Add(enumItem);
				if (!this.DctAllOrg.ContainsKey(key))
				{
					this.DctAllOrg.Add(key, text2);
				}
			}
			if (list.Count == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("库存组织未结束初始化，请先结束初始化！", "004024030002389", 5, new object[0]), 0, "", 0);
			}
			return list;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00015D00 File Offset: 0x00013F00
		protected string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00015D30 File Offset: 0x00013F30
		protected string GetOrgFilter()
		{
			return string.Format(" FUSEORGID IN({0}) ", this.SelOrgId);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00015D4F File Offset: 0x00013F4F
		protected virtual string GetOtherFilter()
		{
			return " AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\nWHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\nAND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ";
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00015D58 File Offset: 0x00013F58
		protected void SetDctSelOrgId()
		{
			this.DctSelOrg.Clear();
			if (this.SelOrgId.Length == 0)
			{
				return;
			}
			List<string> list = this.SelOrgId.Split(new char[]
			{
				','
			}).ToList<string>();
			foreach (string value in list)
			{
				if (!this.DctSelOrg.ContainsKey(Convert.ToInt64(value)) && this.DctAllOrg.ContainsKey(Convert.ToInt64(value)))
				{
					this.DctSelOrg.Add(Convert.ToInt64(value), this.DctAllOrg[Convert.ToInt64(value)]);
				}
			}
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00015E20 File Offset: 0x00014020
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = this.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x040000A2 RID: 162
		private const string Archivesecid = "00505694265cb6cf11e3b590a99d8e2e";

		// Token: 0x040000A3 RID: 163
		protected List<long> LstStkOrg = new List<long>();

		// Token: 0x040000A4 RID: 164
		protected string SelOrgId = string.Empty;

		// Token: 0x040000A5 RID: 165
		protected Dictionary<long, string> DctSelOrg = new Dictionary<long, string>();

		// Token: 0x040000A6 RID: 166
		protected Dictionary<long, string> DctAllOrg = new Dictionary<long, string>();
	}
}

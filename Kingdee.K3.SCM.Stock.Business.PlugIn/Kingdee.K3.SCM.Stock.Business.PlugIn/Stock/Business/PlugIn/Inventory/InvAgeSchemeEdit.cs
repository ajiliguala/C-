using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Inventory
{
	// Token: 0x02000008 RID: 8
	public class InvAgeSchemeEdit : AbstractBillPlugIn
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00003B2E File Offset: 0x00001D2E
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._flowsModel = DBServiceHelper.ExecuteDynamicObject(base.View.Context, "SELECT * FROM T_STK_INVAGEBILLSEQMODEL ", null, null, CommandType.Text, new SqlParam[0]);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003B5B File Offset: 0x00001D5B
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			if (!this._isCopy)
			{
				this.SetDefaultOrg();
			}
			this.BuildInOutEntity();
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003B78 File Offset: 0x00001D78
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			this._isCopy = false;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003B88 File Offset: 0x00001D88
		public override void CopyData(CopyDataEventArgs e)
		{
			this._isCopy = true;
			base.CopyData(e);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003B98 File Offset: 0x00001D98
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FRGMATERIALID"))
					{
						if (!(a == "FRGMATERIALGROUPID"))
						{
							return;
						}
						e.IsShowUsed = false;
					}
					else
					{
						e.IsShowUsed = false;
						ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
						if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
						{
							if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
							{
								e.ListFilterParameter.Filter = text;
							}
							else
							{
								IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
								listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
							}
							listShowParameter.MutilListUseOrgId = this._matSelOrgIds;
							listShowParameter.UseOrgId = 0L;
							return;
						}
						e.Cancel = true;
						listShowParameter.MutilListUseOrgId = "";
						listShowParameter.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
						return;
					}
				}
				else if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
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

		// Token: 0x0600002B RID: 43 RVA: 0x00003CE8 File Offset: 0x00001EE8
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FSTOCKORGID"))
				{
					if (!(a == "FRGMATERIALID"))
					{
						if (!(a == "FRGMATERIALGROUPID"))
						{
							return;
						}
						e.IsShowUsed = false;
					}
					else
					{
						e.IsShowUsed = false;
						if (!this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
						{
							e.Cancel = true;
							return;
						}
						if (string.IsNullOrEmpty(e.Filter))
						{
							e.Filter = text;
							return;
						}
						e.Filter = e.Filter + " AND " + text;
						return;
					}
				}
				else if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003DD8 File Offset: 0x00001FD8
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FRGMATERIALGROUPID"))
				{
					return;
				}
				DynamicObject dynamicObject = this.Model.GetValue("FRgMaterialGroupId", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					this.Model.SetValue("FRgSMaterialGroupId", "." + dynamicObject["Id"].ToString(), e.Row);
					return;
				}
				this.Model.SetValue("FRgSMaterialGroupId", "0", e.Row);
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003E74 File Offset: 0x00002074
		public override void ToolBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBCLEARMATRANGE")
				{
					Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMatRange");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					entityDataObject.Clear();
					base.View.UpdateView("FEntityMatRange");
					return;
				}
				if (a == "TBCLEARMATGRPRANGE")
				{
					Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMatGroupRange");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					entityDataObject.Clear();
					base.View.UpdateView("FEntityMatGroupRange");
					return;
				}
			}
			base.ToolBarItemClick(e);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003F3C File Offset: 0x0000213C
		private bool GetFieldFilter(string fieldKey, out string filter, int rowIndex)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKORGID"))
				{
					if (a == "FRGMATERIALID")
					{
						this._matSelOrgIds = "";
						List<long> list = (from p in (DynamicObjectCollection)this.Model.DataObject["EntityOrg"]
						select Convert.ToInt64(p["StockOrg_Id"])).ToList<long>();
						if (list == null || list.Count == 0)
						{
							base.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "004024030000910", 5, new object[0]), 0);
							return false;
						}
						this._matSelOrgIds = string.Join<long>(",", list);
						filter = string.Format(" FUSEORGID IN({0}) ", this._matSelOrgIds);
					}
				}
				else
				{
					if (this.lstStkOrg == null || this.lstStkOrg.Count < 1)
					{
						this.lstStkOrg = this.GetPermissionOrg(base.View.BillBusinessInfo.GetForm().Id, "fce8b1aca2144beeb3c6655eaf78bc34");
					}
					filter = this.GetInFilter("FORGID", this.lstStkOrg);
					filter += string.Format(" AND FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
					filter += this.GetOtherFilter();
				}
			}
			return true;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000040AC File Offset: 0x000022AC
		private List<long> GetPermissionOrg(string formId, string permissionItem)
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = formId,
				PermissionControl = base.View.BusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = base.View.Model.SubSytemId
			};
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, permissionItem);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004108 File Offset: 0x00002308
		private string GetOtherFilter()
		{
			return " AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\nWHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\nAND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ";
		}

		// Token: 0x06000031 RID: 49 RVA: 0x0000410F File Offset: 0x0000230F
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00004140 File Offset: 0x00002340
		private void SetDefaultOrg()
		{
			this.lstStkOrg = this.GetPermissionOrg(base.View.BillBusinessInfo.GetForm().Id, "fce8b1aca2144beeb3c6655eaf78bc34");
			if (!this.lstStkOrg.Contains(base.View.Context.CurrentOrganizationInfo.ID) || !base.View.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				return;
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				FilterClauseWihtKey = string.Format(" FORGID = {0} AND FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' \r\nAND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n  AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ", base.View.Context.CurrentOrganizationInfo.ID),
				OrderByClauseWihtKey = " ",
				IsolationOrgList = null,
				RequiresDataPermission = true
			};
			BaseDataField baseDataField = base.View.BusinessInfo.GetField("FStockOrgId") as BaseDataField;
			DynamicObject[] array = BusinessDataServiceHelper.LoadFromCache(base.View.Context, baseDataField.RefFormDynamicObjectType, queryBuilderParemeter);
			if (array == null || array.Length < 1)
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityOrg");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			DynamicObject dynamicObject;
			if (entityDataObject.Count < 1)
			{
				dynamicObject = new DynamicObject(entryEntity.DynamicObjectType);
				dynamicObject["Seq"] = 1;
				entityDataObject.Add(dynamicObject);
			}
			else
			{
				dynamicObject = entityDataObject[0];
			}
			dynamicObject["StockOrg_Id"] = base.View.Context.CurrentOrganizationInfo.ID;
			dynamicObject["StockOrg"] = array[0];
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00004318 File Offset: 0x00002518
		private void BuildInOutEntity()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityIn");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (entityDataObject == null || entityDataObject.Count < 1)
			{
				IOrderedEnumerable<DynamicObject> orderedEnumerable = from p in this._flowsModel
				where Convert.ToString(p["FTYPE"]) == "I"
				orderby Convert.ToInt32(p["FSEQUENCE"])
				select p;
				int num = 1;
				foreach (DynamicObject dynamicObject in orderedEnumerable)
				{
					DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
					dynamicObject2["Seq"] = num++;
					dynamicObject2["InBillFormID_Id"] = dynamicObject["FBILLFORMID"];
					dynamicObject2["InSequence"] = dynamicObject["FSEQUENCE"];
					entityDataObject.Add(dynamicObject2);
				}
				DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00004454 File Offset: 0x00002654
		private List<EnumItem> GetOrganization(Context ctx)
		{
			List<EnumItem> list = new List<EnumItem>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			list2.Add(new SelectorItemInfo("FNUMBER"));
			list2.Add(new SelectorItemInfo("FNAME"));
			string text = this.GetInFilter("FORGID", this.lstStkOrg);
			text += string.Format(" AND FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
			text += this.GetOtherFilter();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = text
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				long key = (long)dynamicObject["FORGID"];
				string text2 = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(text2, base.Context.UserLocale.LCID);
				list.Add(enumItem);
				if (!this.dctAllOrg.ContainsKey(key))
				{
					this.dctAllOrg.Add(key, text2);
				}
			}
			return list;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00004630 File Offset: 0x00002830
		private void InitStkOrgId()
		{
			this.lstStkOrg = this.GetPermissionOrg(base.View.BillBusinessInfo.GetForm().Id, "fce8b1aca2144beeb3c6655eaf78bc34");
			List<EnumItem> organization = this.GetOrganization(base.View.Context);
			ComboFieldEditor fieldEditor = base.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(organization);
			this.lstStkOrg = new List<long>();
			foreach (EnumItem enumItem in organization)
			{
				this.lstStkOrg.Add(Convert.ToInt64(enumItem.Value));
			}
			string text = Convert.ToString(this.Model.GetValue("FStockOrgId"));
			if (string.IsNullOrWhiteSpace(text) && organization.Count((EnumItem p) => Convert.ToInt64(p.Value) == base.Context.CurrentOrganizationInfo.ID) > 0 && base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				base.View.Model.SetValue("FStockOrgId", base.Context.CurrentOrganizationInfo.ID);
				this.selOrgId = base.Context.CurrentOrganizationInfo.ID.ToString();
				if (!this.dctSelOrg.ContainsKey(base.Context.CurrentOrganizationInfo.ID))
				{
					this.dctSelOrg.Add(base.Context.CurrentOrganizationInfo.ID, base.Context.CurrentOrganizationInfo.Name);
				}
			}
			if (!string.IsNullOrWhiteSpace(text) && !this.selOrgId.Trim().Equals(text.Trim()))
			{
				this.selOrgId = text.ToString();
				this.SetDctSelOrgId();
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004804 File Offset: 0x00002A04
		private void SetDctSelOrgId()
		{
			this.dctSelOrg.Clear();
			if (this.selOrgId.Length == 0)
			{
				return;
			}
			List<string> list = this.selOrgId.Split(new char[]
			{
				','
			}).ToList<string>();
			foreach (string value in list)
			{
				if (!this.dctSelOrg.ContainsKey(Convert.ToInt64(value)) && this.dctAllOrg.ContainsKey(Convert.ToInt64(value)))
				{
					this.dctSelOrg.Add(Convert.ToInt64(value), this.dctAllOrg[Convert.ToInt64(value)]);
				}
			}
		}

		// Token: 0x04000012 RID: 18
		private List<long> lstStkOrg = new List<long>();

		// Token: 0x04000013 RID: 19
		private string selOrgId = string.Empty;

		// Token: 0x04000014 RID: 20
		private Dictionary<long, string> dctSelOrg = new Dictionary<long, string>();

		// Token: 0x04000015 RID: 21
		private Dictionary<long, string> dctAllOrg = new Dictionary<long, string>();

		// Token: 0x04000016 RID: 22
		private DynamicObjectCollection _flowsModel;

		// Token: 0x04000017 RID: 23
		private string _matSelOrgIds = "";

		// Token: 0x04000018 RID: 24
		private bool _isCopy;
	}
}

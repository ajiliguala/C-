using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200006D RID: 109
	public class InvLotQueryList : AbstractListPlugIn
	{
		// Token: 0x060004C0 RID: 1216 RVA: 0x000392F4 File Offset: 0x000374F4
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			object customParameter = this.ListView.OpenParameter.GetCustomParameter("QueryFilter");
			if (customParameter != null)
			{
				e.AppendQueryFilter(customParameter.ToString());
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("QueryPage");
			if (customParameter != null)
			{
				this.queryPage = customParameter.ToString();
			}
			this.AddMustSelFields();
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00039358 File Offset: 0x00037558
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.queryPage, "ReturnDetailData", "");
			e.Cancel = true;
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00039388 File Offset: 0x00037588
		public override void FormatCellValue(FormatCellValueArgs args)
		{
			string text = args.Header.Key.ToUpperInvariant();
			if (!StringUtils.EqualsIgnoreCase(text, "FQTY") && !StringUtils.EqualsIgnoreCase(text, "FProduceDate") && !StringUtils.EqualsIgnoreCase(text, "FExpiryDate"))
			{
				base.FormatCellValue(args);
				return;
			}
			string text2 = "FBaseQty";
			if (!args.DataRow.ColumnContains(text2))
			{
				return;
			}
			if (StringUtils.EqualsIgnoreCase(text, "FProduceDate") || StringUtils.EqualsIgnoreCase(text, "FExpiryDate"))
			{
				string text3 = text;
				string text4 = "FLOT" + text.Substring(1, text.Length - 1);
				if (args.DataRow.ColumnContains(text4) && !(args.DataRow[text4] is DBNull) && args.DataRow[text4] != null)
				{
					args.FormateValue = Convert.ToDateTime(args.DataRow[text4]).Date.ToShortDateString();
					return;
				}
				if (args.DataRow.ColumnContains(text3) && !(args.DataRow[text3] is DBNull) && args.DataRow[text3] != null)
				{
					args.FormateValue = Convert.ToDateTime(args.DataRow[text3]).Date.ToShortDateString();
					return;
				}
			}
			else if (args.DataRow.ColumnContains("FStockUnitID_FPrecision"))
			{
				string text5 = "FStockUnitID_FPrecision";
				decimal num = (args.DataRow["FStoreurNum"] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow["FStoreurNum"]);
				decimal num2 = (args.DataRow["FStoreurNom"] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow["FStoreurNom"]);
				int num3 = (args.DataRow[text5] is DBNull) ? 0 : Convert.ToInt32(args.DataRow[text5]);
				int num4 = (args.DataRow["FUnitRoundType"] is DBNull) ? 0 : Convert.ToInt32(args.DataRow["FUnitRoundType"]);
				decimal d = (args.DataRow[text2] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow[text2]);
				if (num != 0m && num2 != 0m)
				{
					decimal num5 = d * num2 / num;
					switch (num4)
					{
					case 2:
						num5 = MathUtil.Round(num5, num3, 2);
						goto IL_2DA;
					case 3:
						num5 = MathUtil.Round(num5, num3, 3);
						goto IL_2DA;
					}
					num5 = MathUtil.Round(num5, num3, 0);
					IL_2DA:
					args.FormateValue = num5.ToString();
					return;
				}
				base.FormatCellValue(args);
			}
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00039770 File Offset: 0x00037970
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			ColumnField columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FLotProduceDate"));
			if (columnField != null)
			{
				columnField.Visible = false;
			}
			columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FLotExpiryDate"));
			if (columnField != null)
			{
				columnField.Visible = false;
			}
			bool flag = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FQty"));
			if (!flag)
			{
				return;
			}
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseQty"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseQty")) == null && field != null && flag)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNum"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNum")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNom"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNom")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStockUnitId"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStockUnitId")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitRoundType"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitRoundType")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00039D68 File Offset: 0x00037F68
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (!this.CheckPermission(e))
			{
				e.Cancel = true;
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有该操作权限!", "004023030002158", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0, null, 1);
				return;
			}
			string text = e.BarItemKey.ToUpper();
			if (StringUtils.EqualsIgnoreCase(text, "TBCLOSE"))
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.queryPage, "CloseWindowByDetail", "");
				e.Cancel = true;
				return;
			}
			if (StringUtils.EqualsIgnoreCase(text, "TBRETURNDATA"))
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.queryPage, "ReturnDetailData", "");
				e.Cancel = true;
			}
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00039E60 File Offset: 0x00038060
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string id = "STK_Inventory";
			string permissionItemIdByMenuBar = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			if (string.IsNullOrWhiteSpace(permissionItemIdByMenuBar))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, permissionItemIdByMenuBar);
			return permissionAuthResult.Passed;
		}

		// Token: 0x040001C1 RID: 449
		private string queryPage;
	}
}

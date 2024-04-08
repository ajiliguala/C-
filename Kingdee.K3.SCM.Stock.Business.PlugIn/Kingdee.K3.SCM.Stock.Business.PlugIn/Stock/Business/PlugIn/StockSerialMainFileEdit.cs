using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000086 RID: 134
	[Description("序列号主档表单插件")]
	public class StockSerialMainFileEdit : AbstractBillPlugIn
	{
		// Token: 0x06000664 RID: 1636 RVA: 0x0004DBD4 File Offset: 0x0004BDD4
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			if (e.EntityKey.ToUpper().Equals("FORGENTITY"))
			{
				string value = Convert.ToString(base.View.Model.GetValue("FStockStatus", e.Row));
				if (!string.IsNullOrWhiteSpace(value))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("库存状态有值,此行记录不能删除", "004023030002308", 5, new object[0]), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0004DC50 File Offset: 0x0004BE50
		public override void AfterBindData(EventArgs e)
		{
			base.View.GetControl<EntryGrid>("FEntityTrace").SetAllColHeaderAsText();
			string value = string.Empty;
			int entryRowCount = base.View.Model.GetEntryRowCount("FOrgEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				value = Convert.ToString(base.View.Model.GetValue("FSTOCKSTATUS", i));
				if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
				{
					base.View.GetFieldEditor("FOrgId", i).Enabled = true;
				}
				else
				{
					base.View.GetFieldEditor("FOrgId", i).Enabled = false;
				}
			}
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0004DD08 File Offset: 0x0004BF08
		public override void AfterLoadData(EventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FEntityTrace");
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			if (value.Count > 0)
			{
				value.Sort<long>((DynamicObject p) => Convert.ToInt64(p["Id"]), null);
				for (int i = 0; i < value.Count<DynamicObject>(); i++)
				{
					value[i]["Seq"] = i + 1;
				}
			}
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0004DDC8 File Offset: 0x0004BFC8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBACCESSORY"))
				{
					if (!(a == "TBVIEWSNRPT"))
					{
						return;
					}
					long num = 0L;
					Entity entity = base.View.BusinessInfo.GetEntity("FEntityTrace");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
					if (entityDataObject != null && entityDataObject.Count<DynamicObject>() > 0)
					{
						List<DynamicObject> list = (from p in entityDataObject
						where !string.IsNullOrWhiteSpace(Convert.ToString(p["InvId"]))
						select p).ToList<DynamicObject>();
						if (list != null && list.Count<DynamicObject>() > 0)
						{
							num = (from p in list
							select Convert.ToInt64(p["Id"])).ToList<long>().Max();
						}
					}
					SysReportShowParameter sysReportShowParameter = new SysReportShowParameter();
					sysReportShowParameter.ParentPageId = base.View.PageId;
					sysReportShowParameter.MultiSelect = false;
					sysReportShowParameter.FormId = "STK_InvSerialRpt";
					sysReportShowParameter.Height = 700;
					sysReportShowParameter.Width = 950;
					sysReportShowParameter.IsShowFilter = false;
					sysReportShowParameter.CustomParams.Add("BillFormId", base.View.BillBusinessInfo.GetForm().Id + "_Bill");
					sysReportShowParameter.CustomComplexParams.Add("BillIds", num);
					base.View.ShowForm(sysReportShowParameter);
				}
				else
				{
					if (base.View.BillBusinessInfo.GetForm().Id.Equals("BD_ArchivedSerial"))
					{
						this.ViewAttachment(e, "");
						e.Cancel = true;
						return;
					}
					base.BarItemClick(e);
					return;
				}
			}
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0004DF78 File Offset: 0x0004C178
		private void ViewAttachment(BarItemClickEventArgs e, string OperationObjectKey = "")
		{
			object pkvalue = base.View.Model.GetPKValue();
			if (!this.VaildatePermission("BD_ArchivedSerial", Convert.ToInt64(pkvalue), "e48e2e1e5eb94f058306a5e88a8019ed"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起，您没有归档序列号的附件管理权限!", "004023000014827", 5, new object[0]), "", 0);
				return;
			}
			string billNo = "";
			Field billNoField = base.View.BusinessInfo.GetBillNoField();
			if (billNoField != null)
			{
				object value = base.View.Model.GetValue(billNoField.Key);
				if (value != null)
				{
					billNo = value.ToString();
				}
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "BD_SerialMainFile", true) as FormMetadata;
			AttachmentKey attachmentKey = new AttachmentKey();
			attachmentKey.BillType = "BD_SerialMainFile";
			attachmentKey.BillNo = billNo;
			attachmentKey.BillInterID = pkvalue.ToString();
			attachmentKey.OperationStatus = base.View.OpenParameter.Status;
			if (string.IsNullOrEmpty(OperationObjectKey))
			{
				attachmentKey.EntryKey = " ";
				attachmentKey.EntryInterID = "-1";
				attachmentKey.RowIndex = 0;
			}
			else
			{
				int[] selectedRows = base.View.GetControl<EntryGrid>(OperationObjectKey).GetSelectedRows();
				if (selectedRows == null || selectedRows.Length <= 0)
				{
					base.View.ShowMessage(ResManager.LoadKDString("请选择单据体行！", "004023000014245", 5, new object[0]), 0);
					return;
				}
				Entity entity = base.View.BusinessInfo.GetEntity(OperationObjectKey);
				object entityDataObject = base.View.Model.GetEntityDataObject(entity, selectedRows[0]);
				if (entityDataObject == null)
				{
					base.View.ShowMessage(ResManager.LoadKDString("请选择单据体行！", "004023000014245", 5, new object[0]), 0);
					return;
				}
				DynamicObject dynamicObject = entityDataObject as DynamicObject;
				if (dynamicObject == null || dynamicObject["Id"].ToString() == "0" || string.IsNullOrWhiteSpace(dynamicObject["Id"].ToString()))
				{
					base.View.ShowMessage(ResManager.LoadKDString("请选择单据体行！", "004023000014245", 5, new object[0]), 0);
					return;
				}
				string entryInterID = dynamicObject["Id"].ToString();
				attachmentKey.EntryKey = OperationObjectKey;
				attachmentKey.EntryInterID = entryInterID;
				attachmentKey.RowIndex = selectedRows[0];
			}
			StockSerialMainFileEdit.ShowAttachmentList(base.View, formMetadata.BusinessInfo, attachmentKey);
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0004E1E4 File Offset: 0x0004C3E4
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FBILLNO"))
				{
					return;
				}
				this.ShowBizBillForm(e.Row);
				e.Cancel = true;
			}
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0004E224 File Offset: 0x0004C424
		private void ShowBizBillForm(int rowIndex)
		{
			if (rowIndex < 0)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FEntityTrace");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, rowIndex);
			if (entityDataObject == null)
			{
				return;
			}
			DynamicObject dynamicObject = entityDataObject["BillFormID"] as DynamicObject;
			string text = "";
			if (dynamicObject != null && dynamicObject["Id"] != null)
			{
				text = dynamicObject["Id"].ToString();
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			long num = Convert.ToInt64(entityDataObject["BillID"]);
			if (num < 1L)
			{
				return;
			}
			long num2;
			if (text.Equals("STK_TRANSFEROUT") || text.Equals("STK_TransferDirect"))
			{
				num2 = Convert.ToInt64(entityDataObject["SrcTraceStkOrgId_Id"]);
			}
			else
			{
				num2 = Convert.ToInt64(entityDataObject["DesTraceStkOrgId_Id"]);
			}
			if (num2 < 1L)
			{
				return;
			}
			SCMCommon.ShowBizBillForm(this, text, num, num2, 0L);
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0004E310 File Offset: 0x0004C510
		private bool VaildatePermission(string billFormId, long billId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = base.View.Model.SubSytemId,
				pkId = billId.ToString()
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x0004E3D4 File Offset: 0x0004C5D4
		internal static void ShowAttachmentList(IDynamicFormView view, BusinessInfo info, AttachmentKey attachmentKey)
		{
			if (string.IsNullOrWhiteSpace(attachmentKey.EntryKey))
			{
				using (List<Entity>.Enumerator enumerator = info.Entrys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Entity entity = enumerator.Current;
						if (entity.EntityType == 0)
						{
							foreach (Field field in entity.Fields)
							{
								if (field is AttachmentCountField)
								{
									attachmentKey.AttachmentCountFieldKeys.Add(field.Key);
								}
							}
						}
					}
					goto IL_116;
				}
			}
			Entity entity2 = info.GetEntity(attachmentKey.EntryKey);
			foreach (Field field2 in entity2.Fields)
			{
				if (field2 is AttachmentCountField)
				{
					attachmentKey.AttachmentCountFieldKeys.Add(field2.Key);
				}
			}
			IL_116:
			string filter = string.Format("FBILLTYPE='{0}' and FINTERID='{1}' and FENTRYKEY='{2}' and FENTRYINTERID='{3}'", new object[]
			{
				attachmentKey.BillType,
				attachmentKey.BillInterID,
				attachmentKey.EntryKey,
				attachmentKey.EntryInterID
			});
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.IsLookUp = false;
			listShowParameter.CustomParams.Add("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.Caption = ResManager.LoadKDString("附件管理", "004023000014246", 5, new object[0]);
			listShowParameter.FormId = "BOS_Attachment";
			listShowParameter.MultiSelect = false;
			listShowParameter.PageId = string.Format("{0}_{1}_F7", view.PageId, listShowParameter.FormId);
			listShowParameter.Width = 800;
			listShowParameter.Height = 500;
			listShowParameter.ListFilterParameter.Filter = filter;
			listShowParameter.IsShowQuickFilter = false;
			view.ShowForm(listShowParameter, delegate(FormResult result)
			{
				foreach (string text in attachmentKey.AttachmentCountFieldKeys)
				{
					view.UpdateView(text, attachmentKey.RowIndex);
				}
			});
		}

		// Token: 0x0400025E RID: 606
		private const string SNFILEFORMID = "BD_SerialMainFile";

		// Token: 0x0400025F RID: 607
		private const string ARCHIVESNFORMID = "BD_ArchivedSerial";

		// Token: 0x04000260 RID: 608
		private const string PERMETATTEHMENT = "e48e2e1e5eb94f058306a5e88a8019ed";
	}
}

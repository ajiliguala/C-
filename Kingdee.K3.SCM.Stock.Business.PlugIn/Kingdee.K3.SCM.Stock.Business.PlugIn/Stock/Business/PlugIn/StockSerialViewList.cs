using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200002A RID: 42
	[Description("已归档序列号列表插件")]
	public class StockSerialViewList : AbstractListPlugIn
	{
		// Token: 0x0600018C RID: 396 RVA: 0x000131F8 File Offset: 0x000113F8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ListPermitSN", "");
			if (systemProfile != null)
			{
				this._listPermitSN = Convert.ToBoolean(systemProfile);
			}
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00013240 File Offset: 0x00011440
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (this.usebatchSns && this._batchSns.Count > 0)
			{
				this.SwitchTransData();
				e.AppendQueryFilter(string.Format(" EXISTS(SELECT 1 FROM T_STK_SERIALBATCHQUERY TSQ WHERE T0.FNUMBER = TSQ.FNUMBER AND TSQ.FTRANID = '{0}')  ", this._tranId));
			}
			if (this._listPermitSN)
			{
				List<long> permissionViewOrg = Common.GetPermissionViewOrg(base.Context, "BD_ArchivedSerial", "21");
				if (permissionViewOrg == null || permissionViewOrg.Count < 1)
				{
					e.AppendQueryFilter(" 1 <> 1 ");
				}
				else
				{
					e.AppendQueryFilter(string.Format(" EXISTS (SELECT 1 FROM T_BD_SERIALMASTERORG_A TOG WHERE FSERIALID = TOG.FSERIALID AND TOG.FORGID IN ({0})) ", string.Join<long>(",", permissionViewOrg)));
				}
			}
			this.usebatchSns = false;
		}

		// Token: 0x0600018E RID: 398 RVA: 0x000132E0 File Offset: 0x000114E0
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (!string.IsNullOrWhiteSpace(this._oldtranId))
			{
				this.DeleteNoUseQueryDatas(this._oldtranId);
			}
			if (!string.IsNullOrWhiteSpace(this._tranId))
			{
				this.DeleteNoUseQueryDatas(this._tranId);
				this._tranId = "";
			}
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00013340 File Offset: 0x00011540
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBSNQUERY")
				{
					this.ShowSerialQueryForm();
					return;
				}
				if (!(a == "TBUNARCHIVE"))
				{
					if (a == "TBACCESSORY")
					{
						this.ViewAttachment();
						e.Cancel = true;
						return;
					}
					if (!(a == "TBVIEWSNRPT"))
					{
						return;
					}
					ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
					if (selectedRowsInfo.Count == 0)
					{
						this.View.ShowWarnningMessage(ResManager.LoadKDString("请先选择一条主档记录！", "004023030009237", 5, new object[0]), "", 0, null, 1);
						return;
					}
					List<long> value = (from p in selectedRowsInfo
					select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
					SysReportShowParameter sysReportShowParameter = new SysReportShowParameter();
					sysReportShowParameter.ParentPageId = this.View.PageId;
					sysReportShowParameter.MultiSelect = false;
					sysReportShowParameter.FormId = "STK_InvSerialRpt";
					sysReportShowParameter.Height = 700;
					sysReportShowParameter.Width = 950;
					sysReportShowParameter.IsShowFilter = false;
					sysReportShowParameter.CustomParams.Add("BillFormId", this.View.BillBusinessInfo.GetForm().Id);
					sysReportShowParameter.CustomComplexParams.Add("BillIds", value);
					this.View.ShowForm(sysReportShowParameter);
				}
				else
				{
					string operateName = ResManager.LoadKDString("还原", "004023000013917", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					if (!this.VaildatePermission("BD_ArchivedSerial", "00505694265cb6cf11e3b590d1da8712"))
					{
						this.View.ShowMessage(ResManager.LoadKDString("对不起，您没有归档序列号的还原权限!", "004023000014828", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					List<long> list = new List<long>();
					foreach (ListSelectedRow listSelectedRow in this.ListView.SelectedRowsInfo)
					{
						if (!list.Contains(Convert.ToInt64(listSelectedRow.PrimaryKeyValue)))
						{
							list.Add(Convert.ToInt64(listSelectedRow.PrimaryKeyValue));
						}
					}
					if (list.Count <= 0)
					{
						this.View.ShowMessage(ResManager.LoadKDString("请先选中要还原的序列号！", "004023000013907", 5, new object[0]), 0);
						return;
					}
					if (StockSerialMainFileList.ConvertSerials(this.View, true, list))
					{
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = ResManager.LoadKDString("序列号还原！", "004023000013908", 5, new object[0]),
							Environment = 3,
							OperateName = ResManager.LoadKDString("序列号还原", "004023000013909", 5, new object[0]),
							SubSystemId = "21"
						});
						this.View.Refresh();
						return;
					}
				}
			}
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00013664 File Offset: 0x00011864
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.View.GetControl<EntryGrid>("FList").SetAllColHeaderAsText();
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00013682 File Offset: 0x00011882
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			e.Cancel = this.ShowSerialFile(-1);
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00013694 File Offset: 0x00011894
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (a == "FBILLNO")
				{
					this.ShowBizBillForm(e.Row, e.FieldKey);
					e.Cancel = true;
					return;
				}
				if (!(a == "FNUMBER"))
				{
					return;
				}
				e.Cancel = this.ShowSerialFile(e.Row);
			}
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00013710 File Offset: 0x00011910
		private bool ShowSerialFile(int selRow)
		{
			bool result = true;
			ListSelectedRow listSelectedRow;
			if (selRow <= 0)
			{
				if (this.ListView.SelectedRowsInfo == null || this.ListView.SelectedRowsInfo.Count < 1)
				{
					return result;
				}
				listSelectedRow = this.ListView.SelectedRowsInfo[0];
			}
			else
			{
				listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == selRow);
			}
			if (listSelectedRow == null || string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
			{
				return result;
			}
			long num = Convert.ToInt64(listSelectedRow.PrimaryKeyValue);
			if (num < 1L)
			{
				return result;
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = this.View.BillBusinessInfo.GetForm().Id;
			billShowParameter.SyncCallBackAction = true;
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			billShowParameter.PKey = listSelectedRow.PrimaryKeyValue;
			billShowParameter.Status = 1;
			this.View.ShowForm(billShowParameter);
			return result;
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00013834 File Offset: 0x00011A34
		private void ShowSerialQueryForm()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = "STK_BatchSNSelector";
			dynamicFormShowParameter.Height = 650;
			dynamicFormShowParameter.Width = 550;
			this.View.Session["snDatas"] = this._batchSns;
			this.View.Session["tranId"] = this._tranId;
			this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.ApplyReturnData));
		}

		// Token: 0x06000195 RID: 405 RVA: 0x000138C8 File Offset: 0x00011AC8
		private void ApplyReturnData(FormResult ret)
		{
			this.usebatchSns = false;
			if (ret == null || ret.ReturnData == null)
			{
				return;
			}
			string text = ret.ReturnData.ToString();
			if (string.IsNullOrWhiteSpace(text) || !text.Equals("1"))
			{
				return;
			}
			object obj = null;
			this.View.Session.TryGetValue("tranId", out obj);
			this._tranId = obj.ToString();
			obj = null;
			this.View.Session.TryGetValue("snDatas", out obj);
			this._batchSns = (obj as List<string>);
			this.usebatchSns = true;
			this.ListView.RefreshByFilter();
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00013968 File Offset: 0x00011B68
		private void SwitchTransData()
		{
			if (this._oldtranId.Equals(this._tranId))
			{
				return;
			}
			if (!string.IsNullOrWhiteSpace(this._oldtranId))
			{
				this.DeleteNoUseQueryDatas(this._oldtranId);
			}
			this._oldtranId = this._tranId;
			if (!string.IsNullOrWhiteSpace(this._tranId))
			{
				this.InsertTranIdDatas(this._tranId);
			}
		}

		// Token: 0x06000197 RID: 407 RVA: 0x000139C7 File Offset: 0x00011BC7
		private void DeleteNoUseQueryDatas(string tranId)
		{
			StockServiceHelper.DeleteNoUseQueryDatas(this.View.Context, tranId);
		}

		// Token: 0x06000198 RID: 408 RVA: 0x000139DA File Offset: 0x00011BDA
		private void InsertTranIdDatas(string tranId)
		{
			StockServiceHelper.InsertTranIdDatas(this.View.Context, tranId, this._batchSns);
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000139F4 File Offset: 0x00011BF4
		private void ViewAttachment()
		{
			if (!this.VaildatePermission("BD_ArchivedSerial", "e48e2e1e5eb94f058306a5e88a8019ed"))
			{
				this.View.ShowMessage(ResManager.LoadKDString("对不起，您没有归档序列号的附件管理权限!", "004023000014827", 5, new object[0]), 0);
				return;
			}
			string billNo = "";
			if (this.ListView.CurrentSelectedRowInfo == null || string.IsNullOrWhiteSpace(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue))
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), "", 0, null, 1);
				return;
			}
			string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
			Field billNoField = this.View.BusinessInfo.GetBillNoField();
			if (billNoField != null)
			{
				object value = this.View.Model.GetValue(billNoField.Key);
				if (value != null)
				{
					billNo = value.ToString();
				}
			}
			AttachmentKey attachmentKey = new AttachmentKey
			{
				BillType = "BD_SerialMainFile",
				BillNo = billNo,
				BillInterID = primaryKeyValue,
				EntryKey = " ",
				EntryInterID = "-1",
				OperationStatus = 1
			};
			FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "BD_SerialMainFile", true) as FormMetadata;
			StockSerialMainFileEdit.ShowAttachmentList(this.View, formMetadata.BusinessInfo, attachmentKey);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00013B68 File Offset: 0x00011D68
		private void ShowBizBillForm(int selRow, string fieldKey)
		{
			if (selRow <= 0)
			{
				return;
			}
			ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == selRow);
			if (listSelectedRow == null || string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
			{
				return;
			}
			string serialTraceInfo = CommonServiceHelper.GetSerialTraceInfo(this.View.Context, "_A", Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue));
			if (string.IsNullOrWhiteSpace(serialTraceInfo))
			{
				return;
			}
			string[] array = serialTraceInfo.Split(new char[]
			{
				'.'
			});
			string text = array[0];
			long num = Convert.ToInt64(array[1]);
			long num2;
			if (text.Equals("STK_TRANSFEROUT") || text.Equals("STK_TransferDirect"))
			{
				num2 = Convert.ToInt64(array[2]);
			}
			else
			{
				num2 = Convert.ToInt64(array[3]);
			}
			if (num < 1L || num2 < 1L)
			{
				return;
			}
			SCMCommon.ShowBizBillForm(this, text, num, num2, 0L);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00013C7C File Offset: 0x00011E7C
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string permissionItemIdByMenuBar = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			return string.IsNullOrWhiteSpace(permissionItemIdByMenuBar) || this.VaildatePermission("BD_SerialMainFile", permissionItemIdByMenuBar);
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00013CF0 File Offset: 0x00011EF0
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = this.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x04000093 RID: 147
		private const string SNFILEFORMID = "BD_SerialMainFile";

		// Token: 0x04000094 RID: 148
		private const string ARCHIVESNFORMID = "BD_ArchivedSerial";

		// Token: 0x04000095 RID: 149
		private const string PERMETATTEHMENT = "e48e2e1e5eb94f058306a5e88a8019ed";

		// Token: 0x04000096 RID: 150
		private const string RESTORESNSECID = "00505694265cb6cf11e3b590d1da8712";

		// Token: 0x04000097 RID: 151
		private List<string> _batchSns;

		// Token: 0x04000098 RID: 152
		private bool usebatchSns;

		// Token: 0x04000099 RID: 153
		private string _tranId = "";

		// Token: 0x0400009A RID: 154
		private string _oldtranId = "";

		// Token: 0x0400009B RID: 155
		private bool _listPermitSN;
	}
}

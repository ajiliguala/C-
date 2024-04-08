using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000087 RID: 135
	[Description("序列号主档列表插件")]
	public class StockSerialMainFileList : AbstractListPlugIn
	{
		// Token: 0x06000671 RID: 1649 RVA: 0x0004E654 File Offset: 0x0004C854
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ListPermitSN", "");
			if (systemProfile != null)
			{
				this._listPermitSN = Convert.ToBoolean(systemProfile);
			}
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0004E69C File Offset: 0x0004C89C
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
				List<long> permissionViewOrg = Common.GetPermissionViewOrg(base.Context, "BD_SerialMainFile", "21");
				if (permissionViewOrg == null || permissionViewOrg.Count < 1)
				{
					e.AppendQueryFilter(" 1 <> 1 ");
				}
				else
				{
					e.AppendQueryFilter(string.Format(" EXISTS (SELECT 1 FROM T_BD_SERIALMASTERORG TOG WHERE FSERIALID = TOG.FSERIALID AND TOG.FORGID IN ({0})) ", string.Join<long>(",", permissionViewOrg)));
				}
			}
			this.usebatchSns = false;
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0004E73C File Offset: 0x0004C93C
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

		// Token: 0x06000674 RID: 1652 RVA: 0x0004E7B4 File Offset: 0x0004C9B4
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string key;
			switch (key = e.BarItemKey.ToUpperInvariant())
			{
			case "TBNEW":
			case "TBSPLITNEW":
			case "TBNEWHAND":
			{
				string operateName = (e.BarItemKey.ToUpperInvariant() == "TBNEWHAND") ? ResManager.LoadKDString("自定义新增", "004023030009248", 5, new object[0]) : ResManager.LoadKDString("按规则新增", "004023030009249", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.VaildatePermission("BD_SerialMainFile", "fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					this.View.ShowMessage(ResManager.LoadKDString("没有序列号主档的新增权限!", "004023030002311", 5, new object[0]), 0);
					return;
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "STK_SerialProduct", true) as FormMetadata;
				int num2 = 0;
				int.TryParse(formMetadata.GetLayoutInfo().GetFormAppearance().Height.ToString(), out num2);
				int num3 = 0;
				int.TryParse(formMetadata.GetLayoutInfo().GetFormAppearance().Width.ToString(), out num3);
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.Height = ((num2 < 50) ? 50 : num2);
				dynamicFormShowParameter.Width = ((num3 < 50) ? 50 : num3);
				dynamicFormShowParameter.FormId = "STK_SerialProduct";
				dynamicFormShowParameter.SyncCallBackAction = true;
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.OpenStyle.ShowType = 6;
				dynamicFormShowParameter.PageId = SequentialGuid.NewGuid().ToString();
				if (e.BarItemKey.ToUpperInvariant() == "TBNEWHAND")
				{
					dynamicFormShowParameter.CustomParams.Add("bType", "false");
				}
				else
				{
					dynamicFormShowParameter.CustomParams.Add("bType", "true");
				}
				this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
				{
					if (Convert.ToBoolean(result.ReturnData))
					{
						this.View.Refresh();
					}
				});
				return;
			}
			case "TBSNQUERY":
				this.ShowSerialQueryForm();
				return;
			case "TBSPLITFILE":
			case "TBARCHIVE":
			{
				string operateName = ResManager.LoadKDString("归档", "004023000013918", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.VaildatePermission("BD_SerialMainFile", "00505694265cb6cf11e3b590a99d8e2e"))
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("对不起，您没有序列号主档的归档权限!", "004023000013913", 5, new object[0]), "", 0);
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
					this.View.ShowMessage(ResManager.LoadKDString("请先选中要归档的序列号！", "004023000013914", 5, new object[0]), 0);
					return;
				}
				if (StockSerialMainFileList.ConvertSerials(this.View, false, list))
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
					return;
				}
				break;
			}
			case "TBBATCHARCHIVE":
			{
				string operateName = ResManager.LoadKDString("批量归档", "004023030009250", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.VaildatePermission("BD_SerialMainFile", "00505694265cb6cf11e3b590a99d8e2e"))
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("对不起，您没有序列号主档的归档权限!", "004023000013913", 5, new object[0]), "", 0);
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
				dynamicFormShowParameter2.OpenStyle.ShowType = 6;
				dynamicFormShowParameter2.FormId = "STK_SerialBatchFile";
				dynamicFormShowParameter2.PageId = SequentialGuid.NewGuid().ToString();
				dynamicFormShowParameter2.ParentPageId = this.View.PageId;
				IDynamicFormView view = this.View.GetView(base.Context.ConsolePageId);
				if (view != null)
				{
					view.ShowForm(dynamicFormShowParameter2);
					this.View.SendDynamicFormAction(view);
					return;
				}
				break;
			}
			case "TBVIEWSNRPT":
			{
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
				break;
			}

				return;
			}
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0004EE20 File Offset: 0x0004D020
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.View.GetControl<EntryGrid>("FList").SetAllColHeaderAsText();
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0004EE40 File Offset: 0x0004D040
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FBILLNO"))
				{
					return;
				}
				this.ShowBizBillForm(e.Row, e.FieldKey);
				e.Cancel = true;
			}
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0004EE84 File Offset: 0x0004D084
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

		// Token: 0x06000678 RID: 1656 RVA: 0x0004EF18 File Offset: 0x0004D118
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

		// Token: 0x06000679 RID: 1657 RVA: 0x0004EFB8 File Offset: 0x0004D1B8
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

		// Token: 0x0600067A RID: 1658 RVA: 0x0004F017 File Offset: 0x0004D217
		private void DeleteNoUseQueryDatas(string tranId)
		{
			StockServiceHelper.DeleteNoUseQueryDatas(this.View.Context, tranId);
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0004F02A File Offset: 0x0004D22A
		private void InsertTranIdDatas(string tranId)
		{
			StockServiceHelper.InsertTranIdDatas(this.View.Context, tranId, this._batchSns);
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0004F044 File Offset: 0x0004D244
		internal static bool ConvertSerials(IDynamicFormView view, bool isRestore, List<long> serialIds)
		{
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

		// Token: 0x0600067D RID: 1661 RVA: 0x0004F264 File Offset: 0x0004D464
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
			string serialTraceInfo = CommonServiceHelper.GetSerialTraceInfo(this.View.Context, "", Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue));
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

		// Token: 0x0600067E RID: 1662 RVA: 0x0004F358 File Offset: 0x0004D558
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = this.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x04000264 RID: 612
		private const string ARCHIVESECID = "00505694265cb6cf11e3b590a99d8e2e";

		// Token: 0x04000265 RID: 613
		private List<string> _batchSns;

		// Token: 0x04000266 RID: 614
		private bool usebatchSns;

		// Token: 0x04000267 RID: 615
		private string _tranId = "";

		// Token: 0x04000268 RID: 616
		private string _oldtranId = "";

		// Token: 0x04000269 RID: 617
		private bool _listPermitSN;
	}
}

using System;
using System.ComponentModel;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000029 RID: 41
	[Description("生产追溯序列号主档列表插件")]
	public class MTrackSerialMainFileList : AbstractListPlugIn
	{
		// Token: 0x06000185 RID: 389 RVA: 0x00012DB4 File Offset: 0x00010FB4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBNEWBYRULE") && !(a == "TBSPLITNEW") && !(a == "TBNEWHAND"))
				{
					return;
				}
				string operateName = (e.BarItemKey.ToUpperInvariant() == "TBNEWHAND") ? ResManager.LoadKDString("自定义新增", "004023030009248", 5, new object[0]) : ResManager.LoadKDString("按规则新增", "004023030009249", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.VaildatePermission("BD_MTSerialMainFile", "fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					this.View.ShowMessage(ResManager.LoadKDString("没有序列号的新增权限!", "004023030002311", 5, new object[0]), 0);
					return;
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "STK_SerialProduct", true) as FormMetadata;
				int num = 0;
				int.TryParse(formMetadata.GetLayoutInfo().GetFormAppearance().Height.ToString(), out num);
				int num2 = 0;
				int.TryParse(formMetadata.GetLayoutInfo().GetFormAppearance().Width.ToString(), out num2);
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.Height = ((num < 50) ? 50 : num);
				dynamicFormShowParameter.Width = ((num2 < 50) ? 50 : num2);
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
				dynamicFormShowParameter.CustomParams.Add("bMTrack", "true");
				this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
				{
					if (Convert.ToBoolean(result.ReturnData))
					{
						this.View.Refresh();
					}
				});
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00012FFC File Offset: 0x000111FC
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			e.Cancel = true;
			if (this.ListView.SelectedRowsInfo != null && this.ListView.SelectedRowsInfo.Count > 0)
			{
				long num = Convert.ToInt64(this.ListView.SelectedRowsInfo[0].DataRow["FSERIALID"]);
				if (num <= 0L)
				{
					return;
				}
				this.ShowMTrackSerialForm(num);
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00013064 File Offset: 0x00011264
		public void ShowMTrackSerialForm(long serialId)
		{
			OperationStatus status = 1;
			if (this.VaildatePermission("BD_MTSerialMainFile", "f323992d896745fbaab4a2717c79ce2e"))
			{
				status = 2;
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "BD_MTSerialMainFile";
			billShowParameter.SyncCallBackAction = true;
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			billShowParameter.PKey = serialId.ToString();
			billShowParameter.Status = status;
			this.SetFormOpenStyle(billShowParameter);
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000130F0 File Offset: 0x000112F0
		private void SetFormOpenStyle(DynamicFormShowParameter param)
		{
			if (param == null)
			{
				return;
			}
			if (this.View.ParentFormView != null)
			{
				if (StringUtils.EqualsIgnoreCase(this.View.ParentFormView.PageId, FormConst.MainPageId))
				{
					param.OpenStyle.ShowType = 7;
					return;
				}
				OpenStyle openStyle = this.View.OpenParameter.GetCustomParameter("openstyle") as OpenStyle;
				if (openStyle != null)
				{
					if (openStyle.ShowType == 7 || (openStyle.TagetKey != null && openStyle.TagetKey.ToUpper() == "FMAINTAB"))
					{
						param.OpenStyle.ShowType = 7;
					}
					param.OpenStyle.TagetKey = openStyle.TagetKey;
					return;
				}
				param.OpenStyle.ShowType = 0;
			}
		}

		// Token: 0x06000189 RID: 393 RVA: 0x000131AC File Offset: 0x000113AC
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = this.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}
	}
}

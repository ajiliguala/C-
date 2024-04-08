using System;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000052 RID: 82
	public class InventoryEdit : AbstractBillPlugIn
	{
		// Token: 0x0600039C RID: 924 RVA: 0x0002B5D4 File Offset: 0x000297D4
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = base.View.OpenParameter.GetCustomParameter("fid");
			if (customParameter != null)
			{
				this.isUpdate = false;
				this.fid = Convert.ToString(customParameter);
			}
		}

		// Token: 0x0600039D RID: 925 RVA: 0x0002B614 File Offset: 0x00029814
		public override void AfterBindData(EventArgs e)
		{
			if (!this.isUpdate && !string.IsNullOrWhiteSpace(this.fid))
			{
				base.View.SetFormTitle(new LocaleValue(ResManager.LoadKDString("保质期预警", "004023030005536", 5, new object[0])));
				base.View.Model.Load(this.fid);
				DateTime d = Convert.ToDateTime(base.View.Model.DataObject["ExpiryDate"]);
				DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
				int days = (d - systemDateTime).Days;
				base.View.Model.SetValue("FExpiryDays", days);
				this.isUpdate = true;
				base.View.UpdateView();
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600039E RID: 926 RVA: 0x0002B6E1 File Offset: 0x000298E1
		// (set) Token: 0x0600039F RID: 927 RVA: 0x0002B6E9 File Offset: 0x000298E9
		public bool isUpdate { get; set; }

		// Token: 0x0400013C RID: 316
		private string fid;
	}
}

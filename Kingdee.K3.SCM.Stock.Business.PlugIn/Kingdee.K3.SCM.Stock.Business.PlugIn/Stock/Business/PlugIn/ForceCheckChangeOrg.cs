using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Base.Organization;
using Kingdee.BOS.Core.Base.Organization.PlugIn;
using Kingdee.BOS.Core.Base.Organization.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004F RID: 79
	[Description("更改组织，（强制）检查组织的影响客户端插件")]
	public class ForceCheckChangeOrg : AbstractChangeOrgPlugIn
	{
		// Token: 0x06000387 RID: 903 RVA: 0x0002AA04 File Offset: 0x00028C04
		public override void OnValidate(OnCheckArgs e)
		{
			ChangeOrgResult orgResult = new ChangeOrgResult();
			List<long> list = base.Orgs.ToList<long>();
			if (list.Count<long>() > 0)
			{
				this.CheckAccountAndInvBal(e, orgResult, list);
				this.CheckCloseDateBill(e, orgResult, list);
				this.CheckTranferout(e, orgResult, list);
			}
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0002AA48 File Offset: 0x00028C48
		private void CheckTranferout(OnCheckArgs e, ChangeOrgResult orgResult, List<long> listOrgIds)
		{
			if (listOrgIds.Count <= 0)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = CommonServiceHelper.CheckTranferOutQty(base.Context, listOrgIds);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text = Convert.ToString(dynamicObject["FObjectTypeId"]);
					string text2 = Convert.ToString(dynamicObject["FBillNo"]);
					string arg = Convert.ToString(dynamicObject["FOrgname"]);
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
					if (formMetadata != null)
					{
						orgResult = new ChangeOrgResult();
						orgResult.OrgFieldName = formMetadata.BusinessInfo.MainOrgField.Name;
						orgResult.ObjectType = formMetadata.Name;
						orgResult.RecordNumber = text2;
						orgResult.ControlType = 2;
						orgResult.Description = string.Format(ResManager.LoadKDString("分步式调出单：{0}的调出库存组织为{1}，未完全接收。", "004023030005985", 5, new object[0]), text2, arg);
						e.ChangeOrgResult.Add(orgResult);
					}
				}
			}
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0002AB7C File Offset: 0x00028D7C
		private void CheckCloseDateBill(OnCheckArgs e, ChangeOrgResult orgResult, List<long> listOrgIds)
		{
			if (listOrgIds.Count <= 0)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = CommonServiceHelper.CheckOrgChange(base.Context, listOrgIds);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text = Convert.ToString(dynamicObject["FObjectTypeId"]);
					Convert.ToDateTime(dynamicObject["Fdate"]);
					DateTime dateTime = Convert.ToDateTime(dynamicObject["FClosedate"]);
					string arg = Convert.ToString(dynamicObject["FOrgname"]);
					string recordNumber = Convert.ToString(dynamicObject["FBillNo"]);
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
					if (formMetadata != null)
					{
						orgResult = new ChangeOrgResult();
						formMetadata.BusinessInfo.GetBillNoField();
						orgResult.OrgFieldName = formMetadata.BusinessInfo.MainOrgField.Name;
						orgResult.ObjectType = formMetadata.Name;
						orgResult.RecordNumber = recordNumber;
						orgResult.ControlType = 2;
						orgResult.Description = string.Format(ResManager.LoadKDString("库存组织({0})存在业务日期大于等于最后关账日期：{1}的库存单据", "004023030005986", 5, new object[0]), arg, dateTime);
						e.ChangeOrgResult.Add(orgResult);
					}
				}
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0002ACF8 File Offset: 0x00028EF8
		private void CheckAccountAndInvBal(OnCheckArgs e, ChangeOrgResult orgResult, List<long> listOrgIds)
		{
			Dictionary<long, string> orgCloseAccount = CommonServiceHelper.GetOrgCloseAccount(base.Context, listOrgIds);
			if (orgCloseAccount != null && orgCloseAccount.Count > 0)
			{
				string orgInfoList = ForceCheckChangeOrg.GetOrgInfoList(listOrgIds, orgCloseAccount);
				orgResult = new ChangeOrgResult();
				orgResult.Description = string.Format(ResManager.LoadKDString("所选组织({0})没有关账，不允许取消更改组织信息", "004023030005987", 5, new object[0]), orgInfoList);
			}
			Dictionary<long, string> orgInvbal = CommonServiceHelper.GetOrgInvbal(base.Context, listOrgIds);
			if (orgInvbal != null && orgInvbal.Count > 0)
			{
				string orgInfoList2 = ForceCheckChangeOrg.GetOrgInfoList(listOrgIds, orgInvbal);
				orgResult = new ChangeOrgResult();
				orgResult.ControlType = 2;
				orgResult.ObjectType = ResManager.LoadKDString("库存余额", "004023030005988", 5, new object[0]);
				orgResult.Description = string.Format(ResManager.LoadKDString("所选组织({0})存在库存余额不等于0，不允许取消更改组织信息", "004023030005989", 5, new object[0]), orgInfoList2);
				e.ChangeOrgResult.Add(orgResult);
			}
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0002ADC8 File Offset: 0x00028FC8
		private static string GetOrgInfoList(List<long> listOrgIds, Dictionary<long, string> dicOrgIds)
		{
			string text = string.Empty;
			foreach (KeyValuePair<long, string> keyValuePair in dicOrgIds)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					text = keyValuePair.Value;
				}
				else
				{
					text = text + "," + keyValuePair.Value;
				}
			}
			return text;
		}
	}
}

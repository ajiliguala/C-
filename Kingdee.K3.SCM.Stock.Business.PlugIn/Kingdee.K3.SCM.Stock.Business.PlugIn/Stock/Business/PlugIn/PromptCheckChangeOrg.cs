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
	// Token: 0x0200007B RID: 123
	[Description("更改组织，（提示）方式检查组织的影响客户端插件")]
	public class PromptCheckChangeOrg : AbstractChangeOrgPlugIn
	{
		// Token: 0x060005A9 RID: 1449 RVA: 0x000456EC File Offset: 0x000438EC
		public override void OnValidate(OnCheckArgs e)
		{
			ChangeOrgResult orgResult = new ChangeOrgResult();
			List<long> list = base.Orgs.ToList<long>();
			if (list.Count<long>() > 0)
			{
				this.CheckProducePPBOM(e, orgResult, list);
				this.CheckSubPPBOM(e, orgResult, list);
			}
			base.OnValidate(e);
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x00045730 File Offset: 0x00043930
		private void CheckProducePPBOM(OnCheckArgs e, ChangeOrgResult orgResult, List<long> listOrgIds)
		{
			DynamicObjectCollection dynamicObjectCollection = CommonServiceHelper.CheckProduceBOM(base.Context, listOrgIds);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text = Convert.ToString(dynamicObject["FObjectTypeId"]);
					string arg = Convert.ToString(dynamicObject["FOrgname"]);
					string text2 = Convert.ToString(dynamicObject["FBillNo"]);
					string arg2 = Convert.ToString(dynamicObject["FSEQ"]);
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
					if (formMetadata != null)
					{
						orgResult = new ChangeOrgResult();
						formMetadata.BusinessInfo.GetBillNoField();
						orgResult.OrgFieldName = formMetadata.BusinessInfo.MainOrgField.Name;
						orgResult.ObjectType = formMetadata.Name;
						orgResult.RecordNumber = text2;
						orgResult.ControlType = 1;
						orgResult.Description = string.Format(ResManager.LoadKDString("未结案的生产订单:{0},第{1}行分录存在发料组织({2})", "004023030005990", 5, new object[0]), text2, arg2, arg);
						e.ChangeOrgResult.Add(orgResult);
					}
				}
			}
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00045880 File Offset: 0x00043A80
		private void CheckSubPPBOM(OnCheckArgs e, ChangeOrgResult orgResult, List<long> listOrgIds)
		{
			DynamicObjectCollection dynamicObjectCollection = CommonServiceHelper.CheckOutSourceBom(base.Context, listOrgIds);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text = Convert.ToString(dynamicObject["FObjectTypeId"]);
					string arg = Convert.ToString(dynamicObject["FOrgname"]);
					string text2 = Convert.ToString(dynamicObject["FBillNo"]);
					string arg2 = Convert.ToString(dynamicObject["FSEQ"]);
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
					if (formMetadata != null)
					{
						orgResult = new ChangeOrgResult();
						formMetadata.BusinessInfo.GetBillNoField();
						orgResult.OrgFieldName = formMetadata.BusinessInfo.MainOrgField.Name;
						orgResult.ObjectType = formMetadata.Name;
						orgResult.RecordNumber = text2;
						orgResult.ControlType = 1;
						orgResult.Description = string.Format(ResManager.LoadKDString("未结案的的委外订单:{0},第{1}行分录存在发料组织({2})", "004023030006480", 5, new object[0]), text2, arg2, arg);
						e.ChangeOrgResult.Add(orgResult);
					}
				}
			}
		}
	}
}

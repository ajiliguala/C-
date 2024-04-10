using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000007 RID: 7
	[Description("财务应付审核修改SRM单据状态")]
	[HotUpdate]
	public class AP_Payable_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00003E02 File Offset: 0x00002002
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FSUPPLIERID");
			e.FieldKeys.Add("FTHIRDBILLNO");
			e.FieldKeys.Add("FSetAccountType");
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003E40 File Offset: 0x00002040
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string a = Helper.ToStr(dynamicObject["FSetAccountType"], 0);
					string text = Helper.ToStr(dynamicObject["FTHIRDBILLNO"], 0);
					bool flag2 = a == "3" && !string.IsNullOrEmpty(text);
					if (flag2)
					{
						SRMStatus srmstatus = new SRMStatus();
						srmstatus.exectime = "财务应付审核完成";
						srmstatus.company = "芜湖长信科技股份有限公司";
						DynamicObject dynamicObject2 = dynamicObject["SUPPLIERID"] as DynamicObject;
						bool flag3 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject2);
						if (flag3)
						{
							srmstatus.vendid = Helper.ToStr(dynamicObject2["Number"], 0);
						}
						srmstatus.stmtnums = text;
						srmstatus.paynums = "";
						UpdateSrmStatus_Service.SendToSrm(base.Context, srmstatus);
					}
				}
			}
		}
	}
}

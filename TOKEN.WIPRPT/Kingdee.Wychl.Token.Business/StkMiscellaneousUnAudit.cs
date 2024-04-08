﻿using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000025 RID: 37
	[Description("其他入库单反审核删除不良品拆解单插件")]
	public class StkMiscellaneousUnAudit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000084 RID: 132 RVA: 0x0000E9F9 File Offset: 0x0000CBF9
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("F_PAEZ_BILLID");
		}

		// Token: 0x06000085 RID: 133 RVA: 0x0000EA28 File Offset: 0x0000CC28
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			string text = string.Empty;
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["STK_MISCELLANEOUSENTRY"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					bool flag = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject2["F_PAEZ_BILLID"]);
					if (flag)
					{
						text = text + dynamicObject2["F_PAEZ_BILLID"].ToString() + ",";
						dynamicObject2["F_PAEZ_BILLID"] = "";
					}
				}
			}
			bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text);
			if (flag2)
			{
				K3CloudApiClient k3CloudApiClient = new K3CloudApiClient("http://localhost/k3cloud/");
				string dbid = base.Context.DBId;
				apiParameter parameterByDbid = serviceHelper.getParameterByDbid(dbid);
				string text2 = k3CloudApiClient.LoginByAppSecret(dbid, parameterByDbid.apiuser, parameterByDbid.appid, parameterByDbid.appSecret, 2052);
				JObject jobject = JObject.Parse(text2);
				int num = Extensions.Value<int>(jobject["LoginResultType"]);
				bool flag3 = num != 1 && num != -5;
				if (flag3)
				{
					throw new Exception("api登录失败");
				}
				text = text.TrimEnd(new char[]
				{
					','
				});
				JObject jobject2 = new JObject();
				jobject2["Ids"] = text;
				string text3 = JsonConvert.SerializeObject(jobject2);
				string text4 = "PAEZ_BLPCJ";
				string text5 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Delete", new object[]
				{
					text4,
					text3
				});
			}
		}
	}
}

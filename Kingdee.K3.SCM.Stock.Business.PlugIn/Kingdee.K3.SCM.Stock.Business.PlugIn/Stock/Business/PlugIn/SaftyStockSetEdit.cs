using System;
using System.ComponentModel;
using System.Web;
using Kingdee.BOS.Core.ClientIPSecurity;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.JSON;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.FileServer;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.Mobile.Utils;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000027 RID: 39
	[Description("安全库存参数设置表单插件")]
	public class SaftyStockSetEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600017C RID: 380 RVA: 0x00012878 File Offset: 0x00010A78
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (StringUtils.EqualsIgnoreCase(e.EventName, "$$MessageCustomerEvent"))
			{
				JSONObject jsonobject = JSONObject.Parse(e.EventArgs);
				string value = jsonobject.GetValue<string>("content", string.Empty);
				string value2 = jsonobject.GetValue<string>("customParams", string.Empty);
				if (StringUtils.IsEmpty(value))
				{
					return;
				}
				string a;
				if ((a = value) != null)
				{
					if (a == "SafetyStockSet")
					{
						this.OpenSafetyStockSetPage();
						return;
					}
					if (!(a == "AnalysisParams"))
					{
						return;
					}
					this.OpenAnalysisResultsPage(value2);
				}
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00012904 File Offset: 0x00010B04
		private void OpenSafetyStockSetPage()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "BAS_DynamicSafetyStockSet",
				PageId = Guid.NewGuid().ToString(),
				ParentPageId = this.View.Context.ConsolePageId
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0001296C File Offset: 0x00010B6C
		private void OpenAnalysisResultsPage(string customVal)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "BAS_DynamicSafetyStockResult",
				PageId = Guid.NewGuid().ToString(),
				ParentPageId = this.View.Context.ConsolePageId
			};
			dynamicFormShowParameter.CustomParams.Add("AnalysisParams", customVal ?? string.Empty);
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000129ED File Offset: 0x00010BED
		public override void OnInitialize(InitializeEventArgs e)
		{
			this._param = Convert.ToString(e.Paramter.GetCustomParameter("param"));
			this.VerifyIntelligenceDataLicense();
			SCMCommon.SaftStockEventTracking(base.Context, this.View.PageId);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00012A28 File Offset: 0x00010C28
		private void VerifyIntelligenceDataLicense()
		{
			string dbtype = SaftyStockSetServiceHelper.GetDBType(base.Context);
			if (StringUtils.EqualsIgnoreCase(dbtype, ""))
			{
				string text = IntelligenceDataServiceHelper.GetServiceHostUrl(base.Context);
				string userToken = LightAnalysisServiceHelper.GetUserToken(base.Context);
				string entryPageUrl = base.Context.ClientInfo.EntryPageUrl;
				Uri uri = new Uri(entryPageUrl);
				string text2 = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
				if (uri.Segments.Length > 1)
				{
					text2 = text2 + uri.Segments[0] + uri.Segments[1];
				}
				string serverUrl = text2;
				text += "/ids/k3cloud/license/info";
				IntelligenceDataLicenseRequest request = new IntelligenceDataLicenseRequest
				{
					userToken = userToken,
					serverUrl = serverUrl
				};
				IntelligenceDataLicense intelligenceDataLicense = IntelligenceDataLicense.VerifyIntelligenceDataLicense(base.Context, AppIdType.AppIdSaftStock, text, request);
				if (!string.IsNullOrWhiteSpace(intelligenceDataLicense.MainMsg))
				{
					if (intelligenceDataLicense.MsgInfos == null || intelligenceDataLicense.MsgInfos.Count <= 0)
					{
						this.View.ShowMessage(intelligenceDataLicense.MainMsg, 1);
						return;
					}
					this.View.ShowMessage(intelligenceDataLicense.MsgInfos, intelligenceDataLicense.MainMsg, 1);
				}
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00012B58 File Offset: 0x00010D58
		public override void AfterBindData(EventArgs e)
		{
			string appSiteOuterNetUrl = FileServerHelper.GetAppSiteOuterNetUrl(base.Context, HttpContext.Current.Request);
			string text = string.Empty;
			string text2 = string.Empty;
			if (this.View.UserParameterKey == "BAS_DynamicSafetyStockSet")
			{
				text2 = "WizardSettings";
			}
			else if (this.View.UserParameterKey == "BAS_DynamicSafetyStockResult")
			{
				text2 = "AnalysisResults";
			}
			else if (this.View.UserParameterKey == "BAS_DynamicSafetyStockBoard")
			{
				text2 = "AnalysisBoard";
			}
			IClientIPAuthentication currentClientIPAuthentication = ClientIPAuthentication.GetCurrentClientIPAuthentication();
			string clientIP = currentClientIPAuthentication.GetClientIP(HttpContext.Current.Request);
			string str = EncryptUtil.AesEncryptECB(base.Context.UserToken, clientIP);
			text = string.Format("{0}KDMobile/SafeStockSrv/index.html#/{1}?pageid={2}&mlendata={3}&serviceName={4}&UserID={5}", new object[]
			{
				appSiteOuterNetUrl,
				text2,
				this.View.PageId,
				HttpUtility.UrlEncode(str),
				"Kingdee.K3.SCM.WebApi.ServicesStub.SaftyStockSetService.GetSaftyStockSet.common.kdsvc",
				base.Context.UserId
			});
			object customParameter = this.View.OpenParameter.GetCustomParameter("AnalysisParams", true);
			if (!ObjectUtils.IsNullOrEmpty(customParameter))
			{
				text = text + "&customParams=" + customParameter.ToString();
			}
			Control control = this.View.GetControl("FPanelWebBrowse");
			control.SetCustomPropertyValue("Source", text);
			control.SetCustomPropertyValue("IsSetBrowseVisible", true);
			control.SetCustomPropertyValue("IsBrowserVisible", true);
			if (this.View.Context.ClientType != 2 && this.View.Context.ClientType != 16)
			{
				control.InvokeControlMethod("StartTime", new object[]
				{
					2
				});
			}
			this.View.AddAction("notShowMainFormHolder", true);
		}

		// Token: 0x04000091 RID: 145
		private const string SERVICENAME = "Kingdee.K3.SCM.WebApi.ServicesStub.SaftyStockSetService.GetSaftyStockSet.common.kdsvc";

		// Token: 0x04000092 RID: 146
		private string _param = string.Empty;
	}
}

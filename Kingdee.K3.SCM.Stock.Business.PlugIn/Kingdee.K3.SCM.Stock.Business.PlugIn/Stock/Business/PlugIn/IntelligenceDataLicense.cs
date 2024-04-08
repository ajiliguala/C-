using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.DataEntity;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Log;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000026 RID: 38
	public class IntelligenceDataLicense
	{
		// Token: 0x06000175 RID: 373 RVA: 0x000125DC File Offset: 0x000107DC
		public static IntelligenceDataLicense VerifyIntelligenceDataLicense(Context ctx, string appId, string url, IntelligenceDataLicenseRequest request)
		{
			IntelligenceDataLicense intelligenceDataLicense = new IntelligenceDataLicense();
			List<MsgInnerInfo> list = new List<MsgInnerInfo>();
			StringBuilder stringBuilder = new StringBuilder();
			string text = string.Empty;
			try
			{
				text = IntelligenceDataLicense.DoPost(url, JsonUtil.Serialize(request, true));
				IntelligenceDataLicenseResponse intelligenceDataLicenseResponse = KDObjectConverter.DeserializeObject<IntelligenceDataLicenseResponse>(text);
				if (intelligenceDataLicenseResponse.errcode == 0)
				{
					if (intelligenceDataLicenseResponse.data == null || intelligenceDataLicenseResponse.data.Count <= 0)
					{
						goto IL_EC;
					}
					using (List<IntelligenceDataLicenseResponsetInnerData>.Enumerator enumerator = intelligenceDataLicenseResponse.data.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							IntelligenceDataLicenseResponsetInnerData intelligenceDataLicenseResponsetInnerData = enumerator.Current;
							if (intelligenceDataLicenseResponsetInnerData.appId == appId)
							{
								if (intelligenceDataLicenseResponsetInnerData.enterpriseType == 1)
								{
									SaftyStockSetServiceHelper.UpdateDBType(ctx, "Manufacture");
									break;
								}
								if (intelligenceDataLicenseResponsetInnerData.enterpriseType == 0)
								{
									SaftyStockSetServiceHelper.UpdateDBType(ctx, "Business");
									break;
								}
								break;
							}
						}
						goto IL_EC;
					}
				}
				stringBuilder.AppendFormat(ResManager.LoadKDString("数据智能服务许可校验接口失败(errcode!=0)：", "00444538000013916", 5, new object[0]), intelligenceDataLicenseResponse.descriptionCn);
				IL_EC:;
			}
			catch (Exception ex)
			{
				stringBuilder.AppendFormat(ResManager.LoadKDString("数据智能服务许可校验接口失败：", "00444538000013917", 5, new object[0]) + ex.Message + text, new object[0]);
				Logger.Error("SAL", ex.Message + Environment.NewLine + text, ex);
			}
			intelligenceDataLicense.MainMsg = stringBuilder.ToString();
			if (list.Count > 0)
			{
				intelligenceDataLicense.MsgInfos = new Queue<MsgInnerInfo>(list);
			}
			return intelligenceDataLicense;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000176 RID: 374 RVA: 0x00012768 File Offset: 0x00010968
		// (set) Token: 0x06000177 RID: 375 RVA: 0x00012770 File Offset: 0x00010970
		public Queue<MsgInnerInfo> MsgInfos { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000178 RID: 376 RVA: 0x00012779 File Offset: 0x00010979
		// (set) Token: 0x06000179 RID: 377 RVA: 0x00012781 File Offset: 0x00010981
		public string MainMsg { get; set; }

		// Token: 0x0600017A RID: 378 RVA: 0x0001278C File Offset: 0x0001098C
		private static string DoPost(string url, string postJsonData)
		{
			string result = null;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/json";
			using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(postJsonData);
				streamWriter.Flush();
			}
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
			{
				using (Stream responseStream = httpWebResponse.GetResponseStream())
				{
					using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
					{
						result = streamReader.ReadToEnd();
					}
				}
			}
			return result;
		}
	}
}

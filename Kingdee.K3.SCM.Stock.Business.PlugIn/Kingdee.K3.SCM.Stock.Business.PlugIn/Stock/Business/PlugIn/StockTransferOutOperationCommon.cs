using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Purchase.Business.PlugIn;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.K3.SCM.Web.Client;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200005F RID: 95
	public class StockTransferOutOperationCommon : AbstractBillPlugIn
	{
		// Token: 0x0600043E RID: 1086 RVA: 0x0003231D File Offset: 0x0003051D
		public StockTransferOutOperationCommon(StockTransferOutEdit ctx, long billPrimaryKey)
		{
			this._context = ctx.Context;
			this._stockTransferOutEdit = ctx;
			this._billPrimaryKey = billPrimaryKey;
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x0003234A File Offset: 0x0003054A
		public StockTransferOutOperationCommon(StockTransferOutList ctx, long billPrimaryKey)
		{
			this._context = ctx.Context;
			this._stockTransferOutList = ctx;
			this._billPrimaryKey = billPrimaryKey;
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00032377 File Offset: 0x00030577
		public bool SettingDoNothingOperation(AfterDoOperationEventArgs e)
		{
			e.OperationResult.IsShowMessage = true;
			e.OperationResult.OperateResult.Clear();
			if (e.OperationResult.ValidationErrors.Count <= 0)
			{
				return true;
			}
			e.ExecuteResult = false;
			return false;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x000323B4 File Offset: 0x000305B4
		private void SyncToGy(List<StockTransferOutOperationCommon.StockGroup> stockGroup)
		{
			if (stockGroup == null || stockGroup.Count <= 0)
			{
				return;
			}
			DynamicObject billDynamicObject = this.GetBillDynamicObject();
			if (billDynamicObject == null)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = billDynamicObject[this._entryEntityKey] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count <= 0)
			{
				return;
			}
			Dictionary<long, KeyValuePair<string, string>> skunumberByBillId = SyncTransferOutServiceHelper.GetSKUNumberByBillId(this._context, this._billPrimaryKey);
			string text = "";
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				JSONArray jsonarray = new JSONArray();
				foreach (StockTransferOutOperationCommon.StockGroup stockGroup2 in stockGroup)
				{
					JSONObject jsonobject = new JSONObject();
					jsonobject["AuditDate"] = Convert.ToString(billDynamicObject["ApproveDate"]);
					DynamicObject dynamicObject = billDynamicObject["ApproverId"] as DynamicObject;
					if (dynamicObject != null)
					{
						jsonobject["AuditUser"] = Convert.ToString(dynamicObject["Name"]);
					}
					jsonobject["AllotDate"] = Convert.ToString(billDynamicObject["Date"]);
					DynamicObject dynamicObject2 = billDynamicObject["OwnerIdHead"] as DynamicObject;
					if (dynamicObject2 != null)
					{
						jsonobject["SellCode"] = Convert.ToString(dynamicObject2["Number"]);
					}
					jsonobject["SellType"] = Convert.ToString(billDynamicObject["OwnerTypeIdHead"]);
					jsonobject["Fromno"] = Convert.ToString(billDynamicObject["BillNo"]);
					jsonobject["EntryId"] = this._billPrimaryKey;
					jsonobject["ForderType"] = Convert.ToString(billDynamicObject["FTransferBizType"]);
					jsonobject["ForderMode"] = "";
					jsonobject["AllotMethod"] = "2";
					Logger.Info("STK", ResManager.LoadKDString("1.分布式调出单 单据头完成.", "004023000033335", 5, new object[0]));
					JSONArray jsonarray2 = new JSONArray();
					int num = 1;
					foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
					{
						DynamicObject dynamicObject4 = dynamicObject3["SrcStockID"] as DynamicObject;
						DynamicObject dynamicObject5 = dynamicObject3["DestStockID"] as DynamicObject;
						if (dynamicObject4 != null && dynamicObject5 != null)
						{
							string text2 = Convert.ToString(dynamicObject4["Number"]);
							string text3 = Convert.ToString(dynamicObject5["Number"]);
							if (StringUtils.EqualsIgnoreCase(text2, stockGroup2.SrcStockNumber) && StringUtils.EqualsIgnoreCase(text3, stockGroup2.DescStockNumber))
							{
								JSONObject jsonobject2 = new JSONObject();
								jsonobject2["DetailId"] = Convert.ToString(dynamicObject3["id"]);
								DynamicObject dynamicObject6 = dynamicObject3["MaterialID"] as DynamicObject;
								string value = (dynamicObject6 == null) ? "" : dynamicObject6["Number"].ToString();
								string text4 = (dynamicObject6 == null) ? "" : dynamicObject6["Name"].ToString();
								jsonobject2["ProductCode"] = value;
								jsonobject2["ProductName"] = text4;
								Logger.Info("STK", ResManager.LoadKDString("2.分布式调出单 物料转sku begin.", "004023000033336", 5, new object[0]));
								KeyValuePair<string, string> keyValuePair = skunumberByBillId[Convert.ToInt64(dynamicObject3["Id"])];
								if (default(KeyValuePair<string, string>).Equals(keyValuePair))
								{
									stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行分录物料：【{1}】对应的商品编码未找到.", "004023000032522", 5, new object[0]), num, text4));
								}
								Logger.Info("STK", ResManager.LoadKDString("3.分布式调出单 物料转sku end. sku", "004023000033337", 5, new object[0]) + keyValuePair);
								jsonobject2["SkuCode"] = ((!default(KeyValuePair<string, string>).Equals(keyValuePair)) ? keyValuePair.Key : "");
								jsonobject2["SkuName"] = ((!default(KeyValuePair<string, string>).Equals(keyValuePair)) ? keyValuePair.Value : "");
								jsonobject2["Quantity"] = Convert.ToDecimal(dynamicObject3["BaseQty"]);
								jsonobject2["BatchInfo"] = dynamicObject3["LOT_TEXT"].ToString();
								jsonobject2["ProductionDate"] = ((dynamicObject3["ProduceDate"] == null) ? "" : dynamicObject3["ProduceDate"].ToString());
								jsonobject2["MaturityDate"] = ((dynamicObject3["EXPIRYDATE"] == null) ? "" : dynamicObject3["EXPIRYDATE"].ToString());
								DynamicObjectCollection dynamicObjectCollection2 = dynamicObject3["STK_STKTRANSFEROUTSERIAL"] as DynamicObjectCollection;
								string text5 = "";
								if (dynamicObjectCollection2 != null)
								{
									foreach (DynamicObject dynamicObject7 in dynamicObjectCollection2)
									{
										text5 = text5 + Convert.ToString(dynamicObject7["SerialNo"]) + ",";
									}
								}
								text5 = ((text5.Length > 0) ? text5.Substring(0, text5.Length - 1) : "");
								jsonobject2["MachineCode"] = text5;
								jsonarray2.Add(jsonobject2);
							}
						}
						num++;
					}
					jsonobject["InWarehouseCode"] = stockGroup2.DescThirdStockNumber;
					jsonobject["InWarehouseName"] = stockGroup2.DescStockName;
					jsonobject["OutWarehouseCode"] = stockGroup2.SrcThirdStockNumber;
					jsonobject["OutWarehouseName"] = stockGroup2.SrcStockName;
					jsonobject["Detail"] = jsonarray2;
					if (jsonarray2.Count > 0 && stringBuilder.ToString().Length <= 0)
					{
						jsonarray.Add(jsonobject);
					}
				}
				Logger.Info("STK", ResManager.LoadKDString("4.分布式调出单 json构造完毕.", "004023000033338", 5, new object[0]));
				if (stringBuilder.ToString().Length > 0)
				{
					this.ShowMessage(stringBuilder.ToString());
				}
				else
				{
					string gyappId = GYUtils.GetGYAppId(this._context);
					string gyappKey = GYUtils.GetGYAppKey(this._context);
					string text6 = GYUtils.GetGYPubUrl(this._context);
					string text7 = jsonarray.ToString();
					text6 = (string.IsNullOrEmpty(text6) ? "" : (text6 + "/AllocationNotification"));
					Logger.Info("STK", string.Format(ResManager.LoadKDString("5.分布式调出单 url appId={0} appKey={1} url ={2}.", "004023000033339", 5, new object[0]), gyappId, gyappKey, text6));
					string signature = GYSignUtil.GetSignature(gyappKey, text7);
					string text8 = StockTransferOutOperationCommon.ParseParameter("1.0", gyappId, signature, StockTransferOutOperationCommon.GetEscapeDataString(text7, 5000));
					Logger.Info("STK", ResManager.LoadKDString("6.分布式调出单 Post begin postData=", "004023000033340", 5, new object[0]) + text7);
					text = GYClient.Post(text6, text8);
					Logger.Info("STK", ResManager.LoadKDString("7.分布式调出单 Post end returnJson=", "004023000033341", 5, new object[0]) + text);
					JSONObject jsonobject3 = JSONObject.Parse(text);
					if (jsonobject3 != null)
					{
						if (!Convert.ToBoolean(jsonobject3["result"]))
						{
							string msg = jsonobject3["exceptionMsg"].ToString();
							this.ShowMessage(msg);
						}
						else
						{
							bool flag = SyncTransferOutServiceHelper.UpdateStkTransferOutSyncStatus(this._context, this._billPrimaryKey.ToString(), "1");
							if (flag)
							{
								if (this._stockTransferOutEdit != null)
								{
									this._stockTransferOutEdit.View.Model.SetValue("FSyncStatus", 1);
									this._stockTransferOutEdit.View.UpdateView("FSyncStatus");
									this._stockTransferOutEdit.Model.DataChanged = false;
								}
								if (this._stockTransferOutList != null)
								{
									this._stockTransferOutList.View.Model.SetValue("FSyncStatus", 1);
									this._stockTransferOutList.View.UpdateView("FSyncStatus");
									this._stockTransferOutList.Model.DataChanged = false;
								}
							}
							string msg2 = ResManager.LoadKDString("推送到管易成功.", "004023000035399", 5, new object[0]);
							this.ShowMessage(msg2);
						}
					}
				}
			}
			catch (NullReferenceException)
			{
				this.ShowMessage(string.Format(ResManager.LoadKDString("推送到管易异常，管易返回Json数据解析失败：json={0}", "004023000035400", 5, new object[0]), text));
			}
			catch (WebException ex)
			{
				string arg = ResManager.LoadKDString("请检查Cloud管易网址端口设置.", "004023000032523", 5, new object[0]);
				WebExceptionStatus status = ex.Status;
				switch (status)
				{
				case WebExceptionStatus.ConnectFailure:
					this.ShowMessage(string.Format(ResManager.LoadKDString("网络连接失败，{0}", "004023000032525", 5, new object[0]), arg));
					break;
				case WebExceptionStatus.ReceiveFailure:
					this.ShowMessage(string.Format(ResManager.LoadKDString("接收数据失败，{0}", "004023000032527", 5, new object[0]), arg));
					break;
				case WebExceptionStatus.SendFailure:
					this.ShowMessage(string.Format(ResManager.LoadKDString("发送数据失败，{0}", "004023000032528", 5, new object[0]), arg));
					break;
				case WebExceptionStatus.PipelineFailure:
				case WebExceptionStatus.RequestCanceled:
				case WebExceptionStatus.ProtocolError:
					break;
				case WebExceptionStatus.ConnectionClosed:
					this.ShowMessage(string.Format(ResManager.LoadKDString("网络连接被关闭，{0}", "004023000032526", 5, new object[0]), arg));
					break;
				default:
					switch (status)
					{
					case WebExceptionStatus.Timeout:
						this.ShowMessage(string.Format(ResManager.LoadKDString("发送数据超时，{0}", "004023000032524", 5, new object[0]), arg));
						break;
					case WebExceptionStatus.UnknownError:
						this.ShowMessage(string.Format(ResManager.LoadKDString("网络发生未知错误，{0}", "004023000032529", 5, new object[0]), arg));
						break;
					}
					break;
				}
			}
			catch (Exception ex2)
			{
				this.ShowMessage(string.Format(ResManager.LoadKDString("推送到管易异常：{0}", "004023000035398", 5, new object[0]), ex2.Message));
				Logger.Error("STK", ex2.Message, ex2);
			}
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x00032E8C File Offset: 0x0003108C
		private static string ParseParameter(string v, string appId, string sign, string msg)
		{
			if (string.IsNullOrWhiteSpace(appId))
			{
				return string.Format("v={0}&sign=&message={1}", v, msg);
			}
			return string.Format("v={0}&appid={1}&sign={2}&message={3}", new object[]
			{
				v,
				appId,
				sign,
				msg
			});
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00032ED4 File Offset: 0x000310D4
		public void DoOperation(AfterDoOperationEventArgs e)
		{
			if (this._billPrimaryKey <= 0L)
			{
				return;
			}
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SYNCGYDIRECT"))
				{
					return;
				}
				if (!this.SettingDoNothingOperation(e))
				{
					return;
				}
				List<StockTransferOutOperationCommon.StockGroup> list = this.EntryIsThirdStorage();
				if (list == null)
				{
					return;
				}
				this.SyncToGy(list);
			}
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x00032F2C File Offset: 0x0003112C
		public DynamicObject GetBillDynamicObject()
		{
			if (this._billPrimaryKey <= 0L)
			{
				return null;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this._context, "STK_TRANSFEROUT", true);
			return BusinessDataServiceHelper.Load(this._context, new object[]
			{
				this._billPrimaryKey
			}, formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x00032F90 File Offset: 0x00031190
		public DynamicObject[] GetBillDynamicObject(List<long> billPrimaryKey)
		{
			if (billPrimaryKey.Count <= 0)
			{
				return null;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this._context, "STK_TRANSFEROUT", true);
			return BusinessDataServiceHelper.Load(this._context, billPrimaryKey.Cast<object>().ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00033014 File Offset: 0x00031214
		public List<StockTransferOutOperationCommon.StockGroup> EntryIsThirdStorage()
		{
			List<StockTransferOutOperationCommon.StockGroup> list = new List<StockTransferOutOperationCommon.StockGroup>();
			DynamicObject billDynamicObject = this.GetBillDynamicObject();
			if (billDynamicObject == null)
			{
				return null;
			}
			DynamicObjectCollection dynamicObjectCollection = billDynamicObject[this._entryEntityKey] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count <= 0)
			{
				return null;
			}
			int num = 0;
			string text = "";
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				bool flag = false;
				bool flag2 = false;
				DynamicObject dynamicObject2 = dynamicObject["SrcStockID"] as DynamicObject;
				DynamicObject dynamicObject3 = dynamicObject["DestStockID"] as DynamicObject;
				if (dynamicObject2 != null)
				{
					string text2 = dynamicObject2["StockProperty"].ToString();
					string text3 = dynamicObject2["ThirdStockType"].ToString();
					if (StringUtils.EqualsIgnoreCase(text2, "5") && StringUtils.EqualsIgnoreCase(text3, "1"))
					{
						flag = true;
					}
				}
				if (dynamicObject3 != null)
				{
					string text4 = dynamicObject3["StockProperty"].ToString();
					string text5 = dynamicObject3["ThirdStockType"].ToString();
					if (StringUtils.EqualsIgnoreCase(text4, "5") && StringUtils.EqualsIgnoreCase(text5, "1"))
					{
						flag2 = true;
					}
				}
				if ((flag && flag2) || (!flag && !flag2))
				{
					text += string.Format("【{0}】,", num + 1);
				}
				if (dynamicObject2 != null && dynamicObject3 != null)
				{
					string srcStockNumber = Convert.ToString(dynamicObject2["Number"]);
					string descStockNumber = Convert.ToString(dynamicObject3["Number"]);
					string srcStockName = Convert.ToString(dynamicObject2["Name"]);
					string descStockName = Convert.ToString(dynamicObject3["Name"]);
					string srcThirdStockNumber = Convert.ToString(dynamicObject2["ThirdStockNo"]);
					string descThirdStockNumber = Convert.ToString(dynamicObject3["ThirdStockNo"]);
					StockTransferOutOperationCommon.StockGroup item = new StockTransferOutOperationCommon.StockGroup
					{
						SrcStockNumber = srcStockNumber,
						SrcStockName = srcStockName,
						DescStockNumber = descStockNumber,
						DescStockName = descStockName,
						SrcThirdStockNumber = srcThirdStockNumber,
						DescThirdStockNumber = descThirdStockNumber
					};
					if (!list.Exists((StockTransferOutOperationCommon.StockGroup row) => row.SrcStockNumber == srcStockNumber && row.DescStockNumber == descStockNumber))
					{
						list.Add(item);
					}
				}
				num++;
			}
			text = ((text.Length > 0) ? text.Substring(0, text.Length - 1) : "");
			if (text.Length <= 0)
			{
				return list;
			}
			string msg = string.Format(ResManager.LoadKDString("第{0}行分录调入仓库和调出仓库必须有一个是Cloud仓另一个是管易仓，才能同步到管易！", "004023000035412", 5, new object[0]), text);
			this.ShowMessage(msg);
			return null;
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x000332E8 File Offset: 0x000314E8
		private void ShowMessage(string msg)
		{
			if (this._stockTransferOutEdit != null)
			{
				this._stockTransferOutEdit.View.ShowMessage(msg, 0);
			}
			if (this._stockTransferOutList != null)
			{
				this._stockTransferOutList.View.ShowMessage(msg, 0);
			}
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00033338 File Offset: 0x00031538
		private bool AutoPushStockTransterIn()
		{
			DynamicObject billDynamicObject = this.GetBillDynamicObject();
			if (billDynamicObject == null)
			{
				return false;
			}
			DynamicObjectCollection dynamicObjectCollection = billDynamicObject[this._entryEntityKey] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count <= 0)
			{
				return false;
			}
			if (dynamicObjectCollection.Count == 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("匹配源单，没有要生成下游单据的数据。", "004023000032517", 5, new object[0]), 0);
				return false;
			}
			List<ListSelectedRow> list = new List<ListSelectedRow>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				int num = Convert.ToInt32(dynamicObject["ID"]);
				if (num > 0)
				{
					ListSelectedRow item2 = new ListSelectedRow(this._billPrimaryKey.ToString(), num.ToString(), Convert.ToInt32(dynamicObject["Seq"]), "STK_TRANSFEROUT")
					{
						EntryEntityKey = this._entryEntityKey
					};
					list.Add(item2);
				}
			}
			if (list.Count == 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("匹配源单，没有要生成下游单据的数据。", "004023000032517", 5, new object[0]), 0);
				return false;
			}
			ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRules(this._context, "STK_TRANSFEROUT", "STK_TRANSFERIN").FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("源单和生成单据之间没有默认的单据转换路线。", "004023000032518", 5, new object[0]), "", 0);
				return false;
			}
			PushArgs pushArgs = new PushArgs(convertRuleElement, list.ToArray());
			DynamicObject dynamicObject2 = billDynamicObject["CreatedBillType"] as DynamicObject;
			string targetBillTypeId = (dynamicObject2 != null) ? (dynamicObject2["Id"] as string) : string.Empty;
			pushArgs.TargetBillTypeId = targetBillTypeId;
			ConvertOperationResult convertOperationResult = ConvertServiceHelper.Push(this._context, pushArgs, null);
			if (!this.ShowOperationResult(convertOperationResult))
			{
				return false;
			}
			DynamicObject[] array = (from p in convertOperationResult.TargetDataEntities
			select p.DataEntity).ToArray<DynamicObject>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this._context, "STK_TRANSFERIN", true);
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.View.Context, formMetadata.BusinessInfo, array, null, "");
			if (!this.ShowOperationResult(operationResult))
			{
				return false;
			}
			object[] array2 = (from item in operationResult.Rows
			select item["Id"]).ToList<object>().Distinct<object>().ToArray<object>();
			IOperationResult result = BusinessDataServiceHelper.Submit(base.View.Context, formMetadata.BusinessInfo, array2, "Submit", null);
			if (!this.ShowOperationResult(result))
			{
				return false;
			}
			IOperationResult result2 = BusinessDataServiceHelper.Audit(base.View.Context, formMetadata.BusinessInfo, array2, null);
			return this.ShowOperationResult(result2);
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00033618 File Offset: 0x00031818
		private bool ShowOperationResult(IOperationResult result)
		{
			if (result.IsSuccess)
			{
				return true;
			}
			string text = "";
			if (result.ValidationErrors.Count > 0)
			{
				text = result.ValidationErrors[0].Message;
			}
			base.View.ShowErrMessage(text, "", 0);
			return false;
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00033668 File Offset: 0x00031868
		private static string GetEscapeDataString(string value, int limit)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = value.Length / limit;
			for (int i = 0; i <= num; i++)
			{
				stringBuilder.Append((i < num) ? Uri.EscapeDataString(value.Substring(limit * i, limit)) : Uri.EscapeDataString(value.Substring(limit * i)));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04000193 RID: 403
		private readonly string _entryEntityKey = "STK_STKTRANSFEROUTENTRY";

		// Token: 0x04000194 RID: 404
		private readonly Context _context;

		// Token: 0x04000195 RID: 405
		private readonly StockTransferOutEdit _stockTransferOutEdit;

		// Token: 0x04000196 RID: 406
		private readonly StockTransferOutList _stockTransferOutList;

		// Token: 0x04000197 RID: 407
		private readonly long _billPrimaryKey;

		// Token: 0x02000060 RID: 96
		public class StockGroup
		{
			// Token: 0x0400019A RID: 410
			public string SrcStockNumber;

			// Token: 0x0400019B RID: 411
			public string SrcThirdStockNumber;

			// Token: 0x0400019C RID: 412
			public string SrcStockName;

			// Token: 0x0400019D RID: 413
			public string DescStockNumber;

			// Token: 0x0400019E RID: 414
			public string DescThirdStockNumber;

			// Token: 0x0400019F RID: 415
			public string DescStockName;
		}
	}
}

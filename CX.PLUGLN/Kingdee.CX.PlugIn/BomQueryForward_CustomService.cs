using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.ServicesStub;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000002 RID: 2
	[HotUpdate]
	[Description("物料清单正查-自定义接口")]
	public class BomQueryForward_CustomService : AbstractWebApiBusinessService
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public BomQueryForward_CustomService(KDServiceContext context) : base(context)
		{
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
		public string Query(string org, string number)
		{
			Context appContext = base.KDContext.Session.AppContext;
			JSONObject jsonobject = new JSONObject();
			JSONArray jsonarray = new JSONArray();
			try
			{
				bool flag = !string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(org);
				if (flag)
				{
					string text = "select FCUSTMATNO,FMATERIALID from t_Sal_CustMatMappingEntry";
					DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
					Dictionary<long, string> dictionary = new Dictionary<long, string>();
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						long key = Helper.ToLong(dynamicObject["FMATERIALID"]);
						string value = Helper.ToStr(dynamicObject["FCUSTMATNO"], 0);
						bool flag2 = !dictionary.ContainsKey(key);
						if (flag2)
						{
							dictionary.Add(key, value);
						}
					}
					DynamicObject bom = this.GetBom(org, number, appContext);
					bool flag3 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(bom);
					if (flag3)
					{
						long num = Helper.ToLong(bom["FID"]);
						text = string.Format("/*dialect*/ select a.FID,FROWID,FPARENTROWID,a.FNUMBER,a.FMATERIALID PID,b.FMATERIALID ZXWLID,FMATERIALTYPE ZXLX,FBOMID,b.FUNITID ZXDW,b.FFIXSCRAPQTY GDSH,b.FSCRAPRATE BDSHL,\r\n                        b.FNUMERATOR FZ,b.FDENOMINATOR FM,b.FQTY BZYL, b.FACTUALQTY SJYL,c.FISSKIP TC,c.FISKEYITEM TDZL,FEFFECTDATE, FEXPIREDATE,d.FMEMO BZ,FREPLACEGROUP,e.FNUMBER WLBM\r\n                        from T_ENG_BOM a\r\n                        join T_ENG_BOMCHILD b on a.FID=b.FID\r\n                        left join T_ENG_BOMCHILD_A c on c.FENTRYID=b.FENTRYID\r\n                        left join T_ENG_BOMCHILD_L d on d.FENTRYID=b.FENTRYID\r\n\t\t\t\t\t\tleft join T_BD_MATERIAL e on e.FMATERIALID=b.FMATERIALID\r\n                        where a.FID={0}", num);
						DynamicObjectCollection dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
						Dictionary<string, DynamicObject> dictionary2 = new Dictionary<string, DynamicObject>();
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
						{
							string key2 = Helper.ToStr(dynamicObject2["WLBM"], 0);
							bool flag4 = !dictionary2.ContainsKey(key2);
							if (flag4)
							{
								dictionary2.Add(key2, dynamicObject2);
							}
						}
						string text2 = Helper.ToStr(bom["FNUMBER"], 0);
						text = string.Format("EXECUTE PROC_WLZC  @wlbm='{0}',@orgbm='{1}',@bom='{2}' ", number, org, num);
						DynamicObjectCollection dynamicObjectCollection3 = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
						foreach (DynamicObject dynamicObject3 in dynamicObjectCollection3)
						{
							long num2 = Helper.ToLong(dynamicObject3["BOMID"]);
							string text3 = Helper.ToStr(dynamicObject3["WLBM"], 0);
							string text4 = Helper.ToStr(dynamicObject3["WLMC"], 0);
							string value2 = Helper.ToStr(dynamicObject3["ZXLX"], 0);
							double num3 = Helper.ToDouble(dynamicObject3["FZ"]);
							double num4 = Helper.ToDouble(dynamicObject3["FM"]);
							double num5 = Helper.ToDouble(dynamicObject3["GDSH"]);
							double num6 = Helper.ToDouble(dynamicObject3["BDSHL"]);
							string text5 = Helper.ToStr(dynamicObject3["PWLBM"], 0);
							int num7 = Helper.ToInt(dynamicObject3["BOMCJ"]);
							string text6 = Helper.ToStr(dynamicObject3["BOMBB"], 0);
							double num8 = Helper.ToDouble(dynamicObject3["BZYL"]);
							double num9 = Helper.ToDouble(dynamicObject3["SJSL"]);
							JSONObject jsonobject2 = new JSONObject();
							jsonobject2.Add("BOMCJ", num7);
							DynamicObject wlObj = this.GetWlObj(text3, org, appContext);
							bool flag5 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(wlObj);
							if (flag5)
							{
								long num10 = Helper.ToLong(wlObj["FMATERIALID"]);
								jsonobject2.Add("WLBM", Helper.ToStr(wlObj["FNUMBER"], 0));
								jsonobject2.Add("WLMC", Helper.ToStr(wlObj["WLMC"], 0));
								jsonobject2.Add("WLGGXH", Helper.ToStr(wlObj["GGXH"], 0));
								jsonobject2.Add("WLSX", this.GetWlsxName(Helper.ToStr(wlObj["FERPCLSID"], 0)));
								jsonobject2.Add("FZSX", Helper.ToStr(wlObj["FZSXMC"], 0));
								jsonobject2.Add("ZJM", Helper.ToStr(wlObj["ZJM"], 0));
								jsonobject2.Add("XXKHWLBM", Helper.ToStr(wlObj["F_KING_CUSTOMML"], 0));
								jsonobject2.Add("ZXLX", value2);
								jsonobject2.Add("PWLBM", (num7 == 0) ? "" : text5);
								jsonobject2.Add("ZXDW", Helper.ToStr(wlObj["UNITNAME"], 0));
							}
							jsonobject2.Add("BOMBB", text6);
							jsonobject2.Add("GDSH", num5);
							jsonobject2.Add("BDSHL", num6);
							jsonobject2.Add("FZ", num3);
							jsonobject2.Add("FM", num4);
							jsonobject2.Add("BZYL", num8);
							jsonobject2.Add("SJSL", num9);
							bool flag6 = !dictionary2.ContainsKey(text3);
							if (flag6)
							{
								bool flag7 = !string.IsNullOrEmpty(text6);
								if (flag7)
								{
									text = string.Format("/*dialect*/ select a.FID,FROWID,FPARENTROWID,a.FNUMBER,a.FMATERIALID PID,b.FMATERIALID ZXWLID,FMATERIALTYPE ZXLX,FBOMID,b.FUNITID ZXDW,b.FFIXSCRAPQTY GDSH,b.FSCRAPRATE BDSHL,\r\n                                    b.FNUMERATOR FZ,b.FDENOMINATOR FM,b.FQTY BZYL, b.FACTUALQTY SJYL,c.FISSKIP TC,c.FISKEYITEM TDZL,FEFFECTDATE, FEXPIREDATE,d.FMEMO BZ,FREPLACEGROUP,e.FNUMBER WLBM\r\n                                    from T_ENG_BOM a\r\n                                    join T_ENG_BOMCHILD b on a.FID=b.FID\r\n                                    left join T_ENG_BOMCHILD_A c on c.FENTRYID=b.FENTRYID\r\n                                    left join T_ENG_BOMCHILD_L d on d.FENTRYID=b.FENTRYID\r\n\t\t\t\t\t\t            left join T_BD_MATERIAL e on e.FMATERIALID=b.FMATERIALID\r\n\t\t\t\t\t\t            left join T_ORG_ORGANIZATIONS f on f.FORGID=a.FUSEORGID\r\n                                    where f.FNUMBER='{0}' and a.FNUMBER='{1}' ", org, text6);
									dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
									foreach (DynamicObject dynamicObject4 in dynamicObjectCollection2)
									{
										string key3 = Helper.ToStr(dynamicObject4["WLBM"], 0);
										bool flag8 = !dictionary2.ContainsKey(key3);
										if (flag8)
										{
											dictionary2.Add(key3, dynamicObject4);
										}
									}
								}
							}
							bool flag9 = dictionary2.ContainsKey(text3);
							if (flag9)
							{
								DynamicObject dynamicObject5 = dictionary2[text3];
								string a = Helper.ToStr(dynamicObject5["TC"], 0);
								string a2 = Helper.ToStr(dynamicObject5["TDZL"], 0);
								string value3 = Helper.ToDateTime(dynamicObject5["FEFFECTDATE"]).ToString("yyyy-MM-dd");
								string value4 = Helper.ToDateTime(dynamicObject5["FEXPIREDATE"]).ToString("yyyy-MM-dd");
								string value5 = Helper.ToStr(dynamicObject5["BZ"], 0);
								int num11 = Helper.ToInt(dynamicObject5["FREPLACEGROUP"]);
								bool flag10 = a == "1";
								if (flag10)
								{
									jsonobject2.Add("SFTC", "是");
								}
								else
								{
									jsonobject2.Add("SFTC", "否");
								}
								bool flag11 = a2 == "1";
								if (flag11)
								{
									jsonobject2.Add("TDZL", "是");
								}
								else
								{
									jsonobject2.Add("TDZL", "否");
								}
								jsonobject2.Add("FEFFECTDATE", value3);
								jsonobject2.Add("FEXPIREDATE", value4);
								jsonobject2.Add("BZ", value5);
							}
							jsonarray.Add(jsonobject2);
						}
						jsonobject.Add("IsSuccess", true);
						jsonobject.Add("DATA", jsonarray);
						jsonobject.Add("Msg", "");
					}
					else
					{
						jsonobject.Add("IsSuccess", false);
						jsonobject.Add("DATA", jsonarray);
						jsonobject.Add("Msg", "父项物料编码" + number + "不存在BOM！");
					}
				}
				else
				{
					jsonobject.Add("IsSuccess", false);
					jsonobject.Add("DATA", jsonarray);
					jsonobject.Add("Msg", "使用组织或父项物料编码不能为空！");
				}
			}
			catch (Exception ex)
			{
				jsonobject.Add("IsSuccess", false);
				jsonobject.Add("DATA", jsonarray);
				jsonobject.Add("Msg", ex.Message);
			}
			return KDObjectConverter.SerializeObject(jsonobject);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000028DC File Offset: 0x00000ADC
		public string Query2(string org, string number)
		{
			Context appContext = base.KDContext.Session.AppContext;
			JSONObject jsonobject = new JSONObject();
			JSONArray jsonarray = new JSONArray();
			try
			{
				bool flag = !string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(org);
				if (flag)
				{
					string text = "select a.FUNITID,FNUMBER,FNAME from T_BD_UNIT a join T_BD_UNIT_L b on a.FUNITID=b.FUNITID where FDOCUMENTSTATUS='C'";
					DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
					Dictionary<long, string> dictionary = new Dictionary<long, string>();
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						long key = Helper.ToLong(dynamicObject["FUNITID"]);
						string value = Helper.ToStr(dynamicObject["FNAME"], 0);
						bool flag2 = !dictionary.ContainsKey(key);
						if (flag2)
						{
							dictionary.Add(key, value);
						}
					}
					text = "select FCUSTMATNO,FMATERIALID from t_Sal_CustMatMappingEntry";
					DynamicObjectCollection dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
					Dictionary<long, string> dictionary2 = new Dictionary<long, string>();
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
					{
						long key2 = Helper.ToLong(dynamicObject2["FMATERIALID"]);
						string value2 = Helper.ToStr(dynamicObject2["FCUSTMATNO"], 0);
						bool flag3 = !dictionary2.ContainsKey(key2);
						if (flag3)
						{
							dictionary2.Add(key2, value2);
						}
					}
					DynamicObject bom = this.GetBom(org, number, appContext);
					bool flag4 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(bom);
					if (flag4)
					{
						string arg = Helper.ToStr(bom["FID"], 0);
						long pid = Helper.ToLong(bom["FMATERIALID"]);
						text = string.Format("/*dialect*/select a.FID,FROWID,FPARENTROWID,a.FNUMBER,a.FMATERIALID PID,b.FMATERIALID ZXWLID,FMATERIALTYPE ZXLX,FBOMID,b.FUNITID ZXDW,b.FFIXSCRAPQTY GDSH,b.FSCRAPRATE BDSHL,\r\n                        b.FNUMERATOR FZ,b.FDENOMINATOR FM,b.FQTY BZYL, b.FACTUALQTY SJYL,c.FISSKIP TC,c.FISKEYITEM TDZL,FEFFECTDATE, FEXPIREDATE,d.FMEMO BZ,FREPLACEGROUP\r\n                        from T_ENG_BOM a\r\n                        join T_ENG_BOMCHILD b on a.FID=b.FID\r\n                        left join T_ENG_BOMCHILD_A c on c.FENTRYID=b.FENTRYID\r\n                        left join T_ENG_BOMCHILD_L d on d.FENTRYID=b.FENTRYID\r\n                        where a.FID={0}", arg);
						DynamicObjectCollection dynamicObjects = DBUtils.ExecuteDynamicObject(appContext, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
						List<BomChild> list = new List<BomChild>();
						this.FillData(dynamicObjects, list, "", pid, 1);
						foreach (BomChild bomChild in list)
						{
							JSONObject jsonobject2 = new JSONObject();
							DynamicObject wlObj = this.GetWlObj(bomChild.FXWLID, appContext);
							bool flag5 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(wlObj);
							if (flag5)
							{
								jsonobject2.Add("FXWLBM", Helper.ToStr(wlObj["FNUMBER"], 0));
								jsonobject2.Add("FXWLMC", Helper.ToStr(wlObj["WLMC"], 0));
								bool flag6 = dictionary2.ContainsKey(bomChild.FXWLID);
								if (flag6)
								{
									jsonobject2.Add("KHLH", dictionary2[bomChild.FXWLID]);
								}
								else
								{
									jsonobject2.Add("KHLH", "");
								}
								jsonobject2.Add("ZJM", Helper.ToStr(wlObj["ZJM"], 0));
							}
							jsonobject2.Add("BOMCJ", bomChild.BOMCJ);
							wlObj = this.GetWlObj(bomChild.ZXWLID, appContext);
							bool flag7 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(wlObj);
							if (flag7)
							{
								jsonobject2.Add("ZXWLBM", Helper.ToStr(wlObj["FNUMBER"], 0));
								jsonobject2.Add("ZXWLMC", Helper.ToStr(wlObj["WLMC"], 0));
								jsonobject2.Add("ZXWLGGXH", Helper.ToStr(wlObj["GGXH"], 0));
								jsonobject2.Add("ZXWLSX", this.GetWlsxName(Helper.ToStr(wlObj["FERPCLSID"], 0)));
								jsonobject2.Add("FZSX", Helper.ToStr(wlObj["FZSXMC"], 0));
								jsonobject2.Add("ZXWLZJM", Helper.ToStr(wlObj["ZJM"], 0));
								jsonobject2.Add("XXKHWLBM", Helper.ToStr(wlObj["F_KING_CUSTOMML"], 0));
							}
							bool flag8 = bomChild.ZXLX == "1";
							if (flag8)
							{
								jsonobject2.Add("ZXLX", "标准件");
							}
							else
							{
								bool flag9 = bomChild.ZXLX == "2";
								if (flag9)
								{
									jsonobject2.Add("ZXLX", "返还件");
								}
								else
								{
									jsonobject2.Add("ZXLX", "替代件");
								}
							}
							jsonobject2.Add("BOMBB", bomChild.BOMBB);
							bool flag10 = dictionary.ContainsKey(bomChild.ZXDW);
							if (flag10)
							{
								jsonobject2.Add("ZXDW", dictionary[bomChild.ZXDW]);
							}
							jsonobject2.Add("GDSH", bomChild.GDSH);
							jsonobject2.Add("BDSHL", bomChild.BDSHL);
							jsonobject2.Add("FZ", bomChild.FZ);
							jsonobject2.Add("FM", bomChild.FM);
							jsonobject2.Add("BZYL", bomChild.BZYL);
							jsonobject2.Add("SJSL", bomChild.SJSL);
							jsonobject2.Add("SFTC", bomChild.SFTC);
							jsonobject2.Add("TDZL", bomChild.TDZL);
							jsonobject2.Add("FEFFECTDATE", bomChild.FEFFECTDATE);
							jsonobject2.Add("FEXPIREDATE", bomChild.FEXPIREDATE);
							jsonobject2.Add("BZ", bomChild.BZ);
							jsonarray.Add(jsonobject2);
						}
						jsonobject.Add("IsSuccess", true);
						jsonobject.Add("DATA", jsonarray);
						jsonobject.Add("Msg", "");
					}
				}
				else
				{
					jsonobject.Add("IsSuccess", false);
					jsonobject.Add("DATA", jsonarray);
					jsonobject.Add("Msg", "使用组织或父项物料编码不能为空！");
				}
			}
			catch (Exception ex)
			{
				jsonobject.Add("IsSuccess", false);
				jsonobject.Add("DATA", jsonarray);
				jsonobject.Add("Msg", ex.Message);
			}
			return KDObjectConverter.SerializeObject(jsonobject);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002FBC File Offset: 0x000011BC
		private DynamicObject GetBom(string org, string number, Context ctx)
		{
			string text = string.Format("/*dialect*/ select FID,a.FMATERIALID,a.FNUMBER from (\r\n\t\t\t\t\tselect FID,c.FNUMBER,c.FMATERIALID,RANK() over(partition by c.FMATERIALID order by c.fnumber desc) xh  from T_ENG_BOM c\r\n\t\t\t\t\tjoin T_BD_MATERIAL d on c.FMATERIALID=d.FMATERIALID\r\n                    where d.FNUMBER='{0}' and c.FUSEORGID=(select FORGID from T_ORG_ORGANIZATIONS  where FNUMBER='{1}')\r\n\t\t\t\t\t)a  where xh=1", number, org);
			return DBUtils.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002FF4 File Offset: 0x000011F4
		private DynamicObject GetWlObj(long wlid, Context ctx)
		{
			string text = string.Format("/*dialect*/ select a.FMATERIALID,FNUMBER,l.FNAME WLMC,FMNEMONICCODE ZJM,l.FSPECIFICATION GGXH,d.FNAME FZSXMC,b.FERPCLSID,F_KING_CUSTOMML from T_BD_MATERIAL a \r\n                            left join T_BD_MATERIAL_L l on l.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialBase b on b.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialAuxPty c on a.FMATERIALID=c.FMATERIALID\r\n                            left join T_BD_FLEXAUXPROPERTY_l d on c.FAUXPROPERTYID=d.FID\r\n                            where a.FMATERIALID={0}", wlid);
			return DBUtils.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00003030 File Offset: 0x00001230
		private DynamicObject GetWlObj(string wlbm, string org, Context ctx)
		{
			string text = string.Format("select a.FMATERIALID,a.FNUMBER,l.FNAME WLMC,FMNEMONICCODE ZJM,l.FSPECIFICATION GGXH,d.FNAME FZSXMC,b.FERPCLSID,F_KING_CUSTOMML,f.FNAME UNITNAME \r\n\t\t\t\t\tfrom T_BD_MATERIAL a \r\n                            left join T_BD_MATERIAL_L l on l.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialBase b on b.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialAuxPty c on a.FMATERIALID=c.FMATERIALID\r\n                            left join T_BD_FLEXAUXPROPERTY_l d on c.FAUXPROPERTYID=d.FID\r\n\t\t\t\t\t\t\tleft join T_ORG_ORGANIZATIONS e on e.FORGID=a.FUSEORGID\r\n                            left join T_BD_UNIT_L f on f.FUNITID=FBASEUNITID\r\n                            where a.FNUMBER='{0}' and e.FNUMBER='{1}'", wlbm, org);
			return DBUtils.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00003068 File Offset: 0x00001268
		private string GetWlsxName(string wlsx)
		{
			string result = "";
			bool flag = wlsx == "1";
			if (flag)
			{
				result = "外购";
			}
			else
			{
				bool flag2 = wlsx == "2";
				if (flag2)
				{
					result = "自制";
				}
				else
				{
					bool flag3 = wlsx == "3";
					if (flag3)
					{
						result = "委外";
					}
					else
					{
						bool flag4 = wlsx == "4";
						if (flag4)
						{
							result = "特征";
						}
						else
						{
							bool flag5 = wlsx == "5";
							if (flag5)
							{
								result = "虚拟";
							}
							else
							{
								bool flag6 = wlsx == "6";
								if (flag6)
								{
									result = "服务";
								}
								else
								{
									bool flag7 = wlsx == "7";
									if (flag7)
									{
										result = "一次性";
									}
									else
									{
										bool flag8 = wlsx == "9";
										if (flag8)
										{
											result = "配置";
										}
										else
										{
											bool flag9 = wlsx == "10";
											if (flag9)
											{
												result = "资产";
											}
											else
											{
												bool flag10 = wlsx == "11";
												if (flag10)
												{
													result = "费用";
												}
												else
												{
													bool flag11 = wlsx == "12";
													if (flag11)
													{
														result = "模型";
													}
													else
													{
														bool flag12 = wlsx == "13";
														if (flag12)
														{
															result = "产品系列";
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000031D4 File Offset: 0x000013D4
		private void FillData(DynamicObjectCollection dynamicObjects, List<BomChild> list, string prowid, long pid, int bomcj)
		{
			foreach (DynamicObject dynamicObject in dynamicObjects)
			{
				string prowid2 = Helper.ToStr(dynamicObject["FROWID"], 0);
				string b = Helper.ToStr(dynamicObject["FPARENTROWID"], 0);
				bool flag = prowid == b;
				if (flag)
				{
					BomChild bomChild = new BomChild();
					long num = Helper.ToLong(dynamicObject["FBOMID"]);
					long num2 = Helper.ToLong(dynamicObject["ZXWLID"]);
					string zxlx = Helper.ToStr(dynamicObject["ZXLX"], 0);
					long zxdw = Helper.ToLong(dynamicObject["ZXDW"]);
					double gdsh = Helper.ToDouble(dynamicObject["GDSH"]);
					double bdshl = Helper.ToDouble(dynamicObject["BDSHL"]);
					double fz = Helper.ToDouble(dynamicObject["FZ"]);
					double fm = Helper.ToDouble(dynamicObject["FM"]);
					double bzyl = Helper.ToDouble(dynamicObject["BZYL"]);
					double sjyl = Helper.ToDouble(dynamicObject["SJYL"]);
					string a = Helper.ToStr(dynamicObject["TC"], 0);
					string a2 = Helper.ToStr(dynamicObject["TDZL"], 0);
					string feffectdate = Helper.ToDateTime(dynamicObject["FEFFECTDATE"]).ToString("yyyy-MM-dd");
					string fexpiredate = Helper.ToDateTime(dynamicObject["FEXPIREDATE"]).ToString("yyyy-MM-dd");
					string bz = Helper.ToStr(dynamicObject["BZ"], 0);
					int num3 = Helper.ToInt(dynamicObject["FREPLACEGROUP"]);
					bomChild.BOMCJ = bomcj;
					bomChild.FXWLID = pid;
					bomChild.ZXWLID = num2;
					bomChild.ZXDW = zxdw;
					bomChild.ZXLX = zxlx;
					bomChild.GDSH = gdsh;
					bomChild.BDSHL = bdshl;
					bomChild.FZ = fz;
					bomChild.FM = fm;
					bomChild.BZYL = bzyl;
					bomChild.SJYL = sjyl;
					bomChild.FEFFECTDATE = feffectdate;
					bomChild.FEXPIREDATE = fexpiredate;
					bomChild.BOMBB = Helper.ToStr(dynamicObject["FNUMBER"], 0);
					bool flag2 = a == "1";
					if (flag2)
					{
						bomChild.SFTC = "是";
					}
					else
					{
						bomChild.SFTC = "否";
					}
					bool flag3 = a2 == "1";
					if (flag3)
					{
						bomChild.TDZL = "是";
					}
					else
					{
						bomChild.TDZL = "否";
					}
					bomChild.BZ = bz;
					list.Add(bomChild);
					this.FillData(dynamicObjects, list, prowid2, num2, bomcj + 1);
				}
			}
		}
	}
}

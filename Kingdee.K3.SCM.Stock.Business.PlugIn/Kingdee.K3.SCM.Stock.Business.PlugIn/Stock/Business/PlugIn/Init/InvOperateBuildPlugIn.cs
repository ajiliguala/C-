using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Init
{
	// Token: 0x02000006 RID: 6
	[Description("库存工作台构建插件")]
	public class InvOperateBuildPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x0600000A RID: 10 RVA: 0x000021C8 File Offset: 0x000003C8
		public override void CreateControl(CreateControlEventArgs e)
		{
			JSONObject jsonobject = new JSONObject();
			JSONObject jsonobject2 = new JSONObject();
			string key = e.ControlAppearance.Key;
			string a;
			if ((a = key.ToUpperInvariant()) != null)
			{
				if (a == "FBAR")
				{
					e.Control["stylekey"] = "KDProgressBarCircleStyle";
					jsonobject.Put("fontName", 6);
					jsonobject.Put("fontSize", this.FONT_MID_SIZE);
					e.Control["font"] = KDObjectConverter.SerializeObject(jsonobject);
					e.Control["forecolor"] = "#ffffff";
					e.Control["backcolor"] = "#ffffff";
					e.Control["isProgressBarCircleExtend"] = true;
					JSONObject jsonobject3 = new JSONObject();
					jsonobject3["borderColor"] = new string[]
					{
						"#5483FF",
						"#7F8CFF",
						"#A181FF"
					};
					jsonobject3["backgroundColor"] = "#FFF";
					jsonobject3["valueFontsize"] = "25px";
					e.Control["customParameters"] = jsonobject3;
					return;
				}
				if (a == "FPNLMID")
				{
					e.Control["backcolor"] = "#E8ECF6";
					return;
				}
				if (a == "FPNLTOPTITLE")
				{
					e.Control["backcolor"] = "#0086F1";
					return;
				}
				if (a == "FPNLCENTER" || a == "FPNLTOP")
				{
					e.Control["backcolor"] = "#ffffff";
					return;
				}
			}
			if (key.Contains("FLSeq"))
			{
				if (base.Context.ClientType == 16)
				{
					jsonobject2["background-image"] = "url(../images/biz/default/InitImplementation/GL_InitGuide/btnItem1.png)";
					e.Control["InlineStyle"] = jsonobject2.ToJSONString();
				}
			}
			else if (key.Contains("FBtn"))
			{
				e.Control["style"] = 4;
				jsonobject2["line-height"] = "45px";
				e.Control["InlineStyle"] = jsonobject2.ToJSONString();
				jsonobject.Put("fontName", "微软雅黑");
				jsonobject.Put("fontSize", this.FONT_MID_SIZE);
				e.Control["font"] = KDObjectConverter.SerializeObject(jsonobject);
				e.Control["forecolor"] = "#ffffff";
			}
			else if (key.Contains("FPnlItem"))
			{
				if (base.Context.ClientType == 16)
				{
					jsonobject2["background-image"] = "url(../images/biz/default/InitImplementation/GL_InitGuide/btnItem2.png)";
					e.Control["InlineStyle"] = jsonobject2.ToJSONString();
				}
			}
			else if (key.Contains("FLRemark"))
			{
				jsonobject.Put("fontName", "微软雅黑");
				jsonobject.Put("fontSize", this.FONT_MID_SIZE);
				e.Control["font"] = KDObjectConverter.SerializeObject(jsonobject);
				e.Control["forecolor"] = this.Font_Dark_Gray;
			}
			else if (key.Contains("FLMin"))
			{
				jsonobject.Put("fontName", "微软雅黑");
				jsonobject.Put("fontSize", this.FONT_MID_SIZE);
				e.Control["font"] = KDObjectConverter.SerializeObject(jsonobject);
				e.Control["forecolor"] = this.Font_Light_Gray;
			}
			else if (key.Contains("FLKItem"))
			{
				jsonobject.Put("fontName", "微软雅黑");
				jsonobject.Put("fontSize", this.FONT_MID_SIZE);
				jsonobject.Put("InlineStyle", "underline");
				e.Control["font"] = KDObjectConverter.SerializeObject(jsonobject);
				e.Control["lineWrap"] = true;
				jsonobject2["text-decoration"] = "underline";
				e.Control["InlineStyle"] = jsonobject2.ToJSONString();
			}
			base.CreateControl(e);
		}

		// Token: 0x04000001 RID: 1
		private const string COLOR_WHITE = "#ffffff";

		// Token: 0x04000002 RID: 2
		private const string COLOR_GRAY = "#A3A9B1";

		// Token: 0x04000003 RID: 3
		private const string FONT_MSYH = "微软雅黑";

		// Token: 0x04000004 RID: 4
		private int FONT_BIG_SIZE = 20;

		// Token: 0x04000005 RID: 5
		private int FONT_MID_SIZE = 14;

		// Token: 0x04000006 RID: 6
		private int FONT_SMALL_SIZE = 13;

		// Token: 0x04000007 RID: 7
		private string Font_Dark_Gray = "#717171";

		// Token: 0x04000008 RID: 8
		private string Font_Light_Gray = "#C7C7C7";
	}
}

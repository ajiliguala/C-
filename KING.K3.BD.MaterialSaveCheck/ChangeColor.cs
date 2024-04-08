using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace KING.K3.BD.MaterialSaveCheck
{
	// Token: 0x02000002 RID: 2
	[HotUpdate]
	[Description("物料重复改变颜色")]
	public class ChangeColor : AbstractListPlugIn
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override void OnFormatRowConditions(ListFormatConditionArgs args)
		{
			base.OnFormatRowConditions(args);
			FormatCondition formatCondition = new FormatCondition();
			formatCondition.ApplayRow = true;
			int num = 0;
			bool flag = args.DataRow["FDOCUMENTSTATUS"].ToString() != "C";
			if (flag)
			{
				bool flag2 = args.DataRow["FUseOrgId_Id"].ToString() == "100229";
				if (flag2)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("SELECT 1 FROM T_BD_MATERIAL ml  ");
					stringBuilder.AppendLine("INNER JOIN T_BD_MATERIAL_L AS ml_l ON ml.FMATERIALID = ml_l.FMATERIALID");
					stringBuilder.AppendLine("INNER JOIN T_BD_MATERIALBASE AS mlbs ON ml.FMATERIALID = mlbs.FMATERIALID");
					StringBuilder stringBuilder2 = stringBuilder;
					string str = "WHERE '";
					object obj = args.DataRow["FMATERIALID"];
					stringBuilder2.AppendLine(str + ((obj != null) ? obj.ToString() : null) + "' <> ml.FMATERIALID ");
					StringBuilder stringBuilder3 = stringBuilder;
					string str2 = "AND ml_l.FNAME = (SELECT FNAME FROM T_BD_MATERIAL_L WHERE FMATERIALID = ";
					object obj2 = args.DataRow["FMATERIALID"];
					stringBuilder3.AppendLine(str2 + ((obj2 != null) ? obj2.ToString() : null) + " )");
					StringBuilder stringBuilder4 = stringBuilder;
					string str3 = "AND ml_l.FSPECIFICATION = (SELECT FSPECIFICATION FROM T_BD_MATERIAL_L WHERE FMATERIALID = ";
					object obj3 = args.DataRow["FMATERIALID"];
					stringBuilder4.AppendLine(str3 + ((obj3 != null) ? obj3.ToString() : null) + " )");
					StringBuilder stringBuilder5 = stringBuilder;
					string str4 = "AND ml.FMNEMONICCODE = (SELECT FMNEMONICCODE FROM T_BD_MATERIAL WHERE FMATERIALID = ";
					object obj4 = args.DataRow["FMATERIALID"];
					stringBuilder5.AppendLine(str4 + ((obj4 != null) ? obj4.ToString() : null) + " )");
					StringBuilder stringBuilder6 = stringBuilder;
					string str5 = "AND mlbs.FBASEUNITID = (SELECT FBASEUNITID FROM T_BD_MATERIALBASE WHERE FMATERIALID = ";
					object obj5 = args.DataRow["FMATERIALID"];
					stringBuilder6.AppendLine(str5 + ((obj5 != null) ? obj5.ToString() : null) + " )");
					StringBuilder stringBuilder7 = stringBuilder;
					string str6 = "AND ml.FUSEORGID = (SELECT FUSEORGID FROM T_BD_MATERIAL WHERE FMATERIALID = ";
					object obj6 = args.DataRow["FMATERIALID"];
					stringBuilder7.AppendLine(str6 + ((obj6 != null) ? obj6.ToString() : null) + " )");
					StringBuilder stringBuilder8 = stringBuilder;
					string str7 = "AND ml.FFORBIDSTATUS = (SELECT FFORBIDSTATUS FROM T_BD_MATERIAL WHERE FMATERIALID = ";
					object obj7 = args.DataRow["FMATERIALID"];
					stringBuilder8.AppendLine(str7 + ((obj7 != null) ? obj7.ToString() : null) + " )");
					DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(base.Context, stringBuilder.ToString(), null, null, CommandType.Text, new SqlParam[0]);
					bool flag3 = dynamicObjectCollection.Count > 0;
					if (flag3)
					{
						num = 1;
					}
					stringBuilder.Clear();
					bool flag4 = num == 1;
					if (flag4)
					{
						formatCondition.BackColor = "#FF0000";
					}
				}
			}
			args.FormatConditions.Add(formatCondition);
		}
	}
}

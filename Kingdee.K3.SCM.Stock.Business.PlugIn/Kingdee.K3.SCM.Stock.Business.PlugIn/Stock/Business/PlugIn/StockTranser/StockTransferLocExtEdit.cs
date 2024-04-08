using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockTranser
{
	// Token: 0x02000061 RID: 97
	public class StockTransferLocExtEdit : AbstractBillPlugIn
	{
		// Token: 0x0600044E RID: 1102 RVA: 0x000336F4 File Offset: 0x000318F4
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			string text = "";
			string a;
			if ((a = base.View.BusinessInfo.GetForm().Id.ToUpper()) != null)
			{
				if (!(a == "STK_TRANSFEROUT"))
				{
					if (!(a == "STK_TRANSFERIN"))
					{
						if (!(a == "STK_TRANSFERAPPLY"))
						{
							if (a == "STK_TRANSFERDIRECT")
							{
								text = "FBillEntry";
							}
						}
						else
						{
							text = "FEntity";
						}
					}
					else
					{
						text = "FSTKTRSINENTRY";
					}
				}
				else
				{
					text = "FSTKTRSOUTENTRY";
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(text);
			if (entryEntity == null)
			{
				return;
			}
			List<RelatedFlexGroupField> list = (from x in entryEntity.Fields.OfType<RelatedFlexGroupField>()
			where x.BDFlexType != null && x.BDFlexType.FormId == "BD_FLEXVALUESDETAIL" && x.FlexDisplayFormat == 2
			select x).ToList<RelatedFlexGroupField>();
			if (list == null || list.Count < 1)
			{
				return;
			}
			EntryGrid entryGrid = base.View.GetControl(entryEntity.Key) as EntryGrid;
			foreach (RelatedFlexGroupField relatedFlexGroupField in list)
			{
				RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = base.View.LayoutInfo.GetAppearance(relatedFlexGroupField.Key) as RelatedFlexGroupFieldAppearance;
				if (relatedFlexGroupFieldAppearance != null)
				{
					List<FieldAppearance> fieldAppearances = relatedFlexGroupFieldAppearance.RelateFlexLayoutInfo.GetFieldAppearances();
					foreach (FieldAppearance fieldAppearance in fieldAppearances)
					{
						string text2 = string.Format("$${0}__{1}", relatedFlexGroupField.Key.ToUpper(), fieldAppearance.Field.FieldName);
						string text3 = string.Format("{0}.{1}", relatedFlexGroupFieldAppearance.Caption, fieldAppearance.Caption);
						entryGrid.UpdateHeader(text2, text3);
					}
				}
			}
		}
	}
}

using System;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000049 RID: 73
	public class BomExpandSourceRowView : DynamicObjectView
	{
		// Token: 0x060002E7 RID: 743 RVA: 0x0002346B File Offset: 0x0002166B
		public BomExpandSourceRowView(DynamicObject obj) : base(obj)
		{
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00023474 File Offset: 0x00021674
		public static implicit operator BomExpandSourceRowView(DynamicObject obj)
		{
			return new BomExpandSourceRowView(obj);
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060002E9 RID: 745 RVA: 0x0002347C File Offset: 0x0002167C
		// (set) Token: 0x060002EA RID: 746 RVA: 0x000234A6 File Offset: 0x000216A6
		public long EntryId
		{
			get
			{
				object obj = base.DataEntity["Id"];
				if (obj != null)
				{
					return Convert.ToInt64(obj);
				}
				return 0L;
			}
			set
			{
				base.DataEntity["Id"] = value;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060002EB RID: 747 RVA: 0x000234C0 File Offset: 0x000216C0
		// (set) Token: 0x060002EC RID: 748 RVA: 0x00023508 File Offset: 0x00021708
		public long WorkCalId
		{
			get
			{
				object obj = base.DataEntity["WorkCalId"];
				if (obj == null)
				{
					return 0L;
				}
				if (obj is DynamicObject)
				{
					return Convert.ToInt64(((DynamicObject)obj)["Id"]);
				}
				return Convert.ToInt64(obj);
			}
			set
			{
				object obj = base.DataEntity["WorkCalId"];
				if (obj != null && obj is DynamicObject)
				{
					((DynamicObject)obj)["Id"] = value;
				}
				base.DataEntity["WorkCalId"] = obj;
				base.DataEntity["WorkCalId_Id"] = value;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060002ED RID: 749 RVA: 0x00023570 File Offset: 0x00021770
		// (set) Token: 0x060002EE RID: 750 RVA: 0x000235B8 File Offset: 0x000217B8
		public long MaterialId
		{
			get
			{
				object obj = base.DataEntity["MaterialId"];
				if (obj == null)
				{
					return 0L;
				}
				if (obj is DynamicObject)
				{
					return Convert.ToInt64(((DynamicObject)obj)["Id"]);
				}
				return Convert.ToInt64(obj);
			}
			set
			{
				object obj = base.DataEntity["MaterialId"];
				if (obj != null && obj is DynamicObject)
				{
					((DynamicObject)obj)["Id"] = value;
				}
				base.DataEntity["MaterialId"] = obj;
				base.DataEntity["MaterialId_Id"] = value;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060002EF RID: 751 RVA: 0x00023620 File Offset: 0x00021820
		// (set) Token: 0x060002F0 RID: 752 RVA: 0x00023668 File Offset: 0x00021868
		public long BomId
		{
			get
			{
				object obj = base.DataEntity["BomId"];
				if (obj == null)
				{
					return 0L;
				}
				if (obj is DynamicObject)
				{
					return Convert.ToInt64(((DynamicObject)obj)["Id"]);
				}
				return Convert.ToInt64(obj);
			}
			set
			{
				object obj = base.DataEntity["BomId"];
				if (obj != null && obj is DynamicObject)
				{
					((DynamicObject)obj)["Id"] = value;
				}
				base.DataEntity["BomId"] = obj;
				base.DataEntity["BomId_Id"] = value;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x000236D0 File Offset: 0x000218D0
		// (set) Token: 0x060002F2 RID: 754 RVA: 0x00023718 File Offset: 0x00021918
		public long UnitId
		{
			get
			{
				object obj = base.DataEntity["UnitId"];
				if (obj == null)
				{
					return 0L;
				}
				if (obj is DynamicObject)
				{
					return Convert.ToInt64(((DynamicObject)obj)["Id"]);
				}
				return Convert.ToInt64(obj);
			}
			set
			{
				object obj = base.DataEntity["UnitId"];
				if (obj != null && obj is DynamicObject)
				{
					((DynamicObject)obj)["Id"] = value;
				}
				base.DataEntity["UnitId"] = obj;
				base.DataEntity["UnitId_Id"] = value;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x00023780 File Offset: 0x00021980
		// (set) Token: 0x060002F4 RID: 756 RVA: 0x000237AE File Offset: 0x000219AE
		public decimal NeedQty
		{
			get
			{
				object obj = base.DataEntity["NeedQty"];
				if (obj != null)
				{
					return Convert.ToDecimal(obj);
				}
				return 0m;
			}
			set
			{
				base.DataEntity["NeedQty"] = value;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x000237C8 File Offset: 0x000219C8
		// (set) Token: 0x060002F6 RID: 758 RVA: 0x000237F5 File Offset: 0x000219F5
		public DateTime NeedDate
		{
			get
			{
				object obj = base.DataEntity["NeedDate"];
				if (obj != null)
				{
					return Convert.ToDateTime(obj);
				}
				return DateTime.Now;
			}
			set
			{
				base.DataEntity["NeedDate"] = value;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x00023810 File Offset: 0x00021A10
		// (set) Token: 0x060002F8 RID: 760 RVA: 0x0002383A File Offset: 0x00021A3A
		public long TimeUnit
		{
			get
			{
				object obj = base.DataEntity["TimeUnit"];
				if (obj != null)
				{
					return Convert.ToInt64(obj);
				}
				return 0L;
			}
			set
			{
				base.DataEntity["TimeUnit"] = value;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x00023854 File Offset: 0x00021A54
		// (set) Token: 0x060002FA RID: 762 RVA: 0x0002387E File Offset: 0x00021A7E
		public long SrcInterId
		{
			get
			{
				object obj = base.DataEntity["SrcInterId"];
				if (obj != null)
				{
					return Convert.ToInt64(obj);
				}
				return 0L;
			}
			set
			{
				base.DataEntity["SrcInterId"] = value;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060002FB RID: 763 RVA: 0x00023898 File Offset: 0x00021A98
		// (set) Token: 0x060002FC RID: 764 RVA: 0x000238C2 File Offset: 0x00021AC2
		public long SrcEntryId
		{
			get
			{
				object obj = base.DataEntity["SrcEntryId"];
				if (obj != null)
				{
					return Convert.ToInt64(obj);
				}
				return 0L;
			}
			set
			{
				base.DataEntity["SrcEntryId"] = value;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060002FD RID: 765 RVA: 0x000238DC File Offset: 0x00021ADC
		// (set) Token: 0x060002FE RID: 766 RVA: 0x00023906 File Offset: 0x00021B06
		public long SrcSeqNo
		{
			get
			{
				object obj = base.DataEntity["SrcSeqNo"];
				if (obj != null)
				{
					return Convert.ToInt64(obj);
				}
				return 0L;
			}
			set
			{
				base.DataEntity["SrcSeqNo"] = value;
			}
		}
	}
}

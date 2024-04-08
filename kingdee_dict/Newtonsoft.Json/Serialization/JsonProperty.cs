using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000086 RID: 134
	public class JsonProperty
	{
		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000625 RID: 1573 RVA: 0x000150C2 File Offset: 0x000132C2
		// (set) Token: 0x06000626 RID: 1574 RVA: 0x000150CA File Offset: 0x000132CA
		public string PropertyName { get; set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000627 RID: 1575 RVA: 0x000150D3 File Offset: 0x000132D3
		// (set) Token: 0x06000628 RID: 1576 RVA: 0x000150DB File Offset: 0x000132DB
		public IValueProvider ValueProvider { get; set; }

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000629 RID: 1577 RVA: 0x000150E4 File Offset: 0x000132E4
		// (set) Token: 0x0600062A RID: 1578 RVA: 0x000150EC File Offset: 0x000132EC
		public Type PropertyType { get; set; }

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600062B RID: 1579 RVA: 0x000150F5 File Offset: 0x000132F5
		// (set) Token: 0x0600062C RID: 1580 RVA: 0x000150FD File Offset: 0x000132FD
		public JsonConverter Converter { get; set; }

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x0600062D RID: 1581 RVA: 0x00015106 File Offset: 0x00013306
		// (set) Token: 0x0600062E RID: 1582 RVA: 0x0001510E File Offset: 0x0001330E
		public bool Ignored { get; set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600062F RID: 1583 RVA: 0x00015117 File Offset: 0x00013317
		// (set) Token: 0x06000630 RID: 1584 RVA: 0x0001511F File Offset: 0x0001331F
		public bool Readable { get; set; }

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000631 RID: 1585 RVA: 0x00015128 File Offset: 0x00013328
		// (set) Token: 0x06000632 RID: 1586 RVA: 0x00015130 File Offset: 0x00013330
		public bool Writable { get; set; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x00015139 File Offset: 0x00013339
		// (set) Token: 0x06000634 RID: 1588 RVA: 0x00015141 File Offset: 0x00013341
		public JsonConverter MemberConverter { get; set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000635 RID: 1589 RVA: 0x0001514A File Offset: 0x0001334A
		// (set) Token: 0x06000636 RID: 1590 RVA: 0x00015152 File Offset: 0x00013352
		public object DefaultValue { get; set; }

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x06000637 RID: 1591 RVA: 0x0001515B File Offset: 0x0001335B
		// (set) Token: 0x06000638 RID: 1592 RVA: 0x00015163 File Offset: 0x00013363
		public Required Required { get; set; }

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000639 RID: 1593 RVA: 0x0001516C File Offset: 0x0001336C
		// (set) Token: 0x0600063A RID: 1594 RVA: 0x00015174 File Offset: 0x00013374
		public bool? IsReference { get; set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x0600063B RID: 1595 RVA: 0x0001517D File Offset: 0x0001337D
		// (set) Token: 0x0600063C RID: 1596 RVA: 0x00015185 File Offset: 0x00013385
		public NullValueHandling? NullValueHandling { get; set; }

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x0600063D RID: 1597 RVA: 0x0001518E File Offset: 0x0001338E
		// (set) Token: 0x0600063E RID: 1598 RVA: 0x00015196 File Offset: 0x00013396
		public DefaultValueHandling? DefaultValueHandling { get; set; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x0600063F RID: 1599 RVA: 0x0001519F File Offset: 0x0001339F
		// (set) Token: 0x06000640 RID: 1600 RVA: 0x000151A7 File Offset: 0x000133A7
		public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000641 RID: 1601 RVA: 0x000151B0 File Offset: 0x000133B0
		// (set) Token: 0x06000642 RID: 1602 RVA: 0x000151B8 File Offset: 0x000133B8
		public ObjectCreationHandling? ObjectCreationHandling { get; set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000643 RID: 1603 RVA: 0x000151C1 File Offset: 0x000133C1
		// (set) Token: 0x06000644 RID: 1604 RVA: 0x000151C9 File Offset: 0x000133C9
		public TypeNameHandling? TypeNameHandling { get; set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000645 RID: 1605 RVA: 0x000151D2 File Offset: 0x000133D2
		// (set) Token: 0x06000646 RID: 1606 RVA: 0x000151DA File Offset: 0x000133DA
		public Predicate<object> ShouldSerialize { get; set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000647 RID: 1607 RVA: 0x000151E3 File Offset: 0x000133E3
		// (set) Token: 0x06000648 RID: 1608 RVA: 0x000151EB File Offset: 0x000133EB
		public Predicate<object> GetIsSpecified { get; set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000649 RID: 1609 RVA: 0x000151F4 File Offset: 0x000133F4
		// (set) Token: 0x0600064A RID: 1610 RVA: 0x000151FC File Offset: 0x000133FC
		public Action<object, object> SetIsSpecified { get; set; }

		// Token: 0x0600064B RID: 1611 RVA: 0x00015205 File Offset: 0x00013405
		public override string ToString()
		{
			return this.PropertyName;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x0200008B RID: 139
	public class JsonSchema
	{
		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000652 RID: 1618 RVA: 0x0001532C File Offset: 0x0001352C
		// (set) Token: 0x06000653 RID: 1619 RVA: 0x00015334 File Offset: 0x00013534
		public string Id { get; set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000654 RID: 1620 RVA: 0x0001533D File Offset: 0x0001353D
		// (set) Token: 0x06000655 RID: 1621 RVA: 0x00015345 File Offset: 0x00013545
		public string Title { get; set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000656 RID: 1622 RVA: 0x0001534E File Offset: 0x0001354E
		// (set) Token: 0x06000657 RID: 1623 RVA: 0x00015356 File Offset: 0x00013556
		public bool? Required { get; set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000658 RID: 1624 RVA: 0x0001535F File Offset: 0x0001355F
		// (set) Token: 0x06000659 RID: 1625 RVA: 0x00015367 File Offset: 0x00013567
		public bool? ReadOnly { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x0600065A RID: 1626 RVA: 0x00015370 File Offset: 0x00013570
		// (set) Token: 0x0600065B RID: 1627 RVA: 0x00015378 File Offset: 0x00013578
		public bool? Hidden { get; set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x0600065C RID: 1628 RVA: 0x00015381 File Offset: 0x00013581
		// (set) Token: 0x0600065D RID: 1629 RVA: 0x00015389 File Offset: 0x00013589
		public bool? Transient { get; set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x0600065E RID: 1630 RVA: 0x00015392 File Offset: 0x00013592
		// (set) Token: 0x0600065F RID: 1631 RVA: 0x0001539A File Offset: 0x0001359A
		public string Description { get; set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000660 RID: 1632 RVA: 0x000153A3 File Offset: 0x000135A3
		// (set) Token: 0x06000661 RID: 1633 RVA: 0x000153AB File Offset: 0x000135AB
		public JsonSchemaType? Type { get; set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000662 RID: 1634 RVA: 0x000153B4 File Offset: 0x000135B4
		// (set) Token: 0x06000663 RID: 1635 RVA: 0x000153BC File Offset: 0x000135BC
		public string Pattern { get; set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000664 RID: 1636 RVA: 0x000153C5 File Offset: 0x000135C5
		// (set) Token: 0x06000665 RID: 1637 RVA: 0x000153CD File Offset: 0x000135CD
		public int? MinimumLength { get; set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x000153D6 File Offset: 0x000135D6
		// (set) Token: 0x06000667 RID: 1639 RVA: 0x000153DE File Offset: 0x000135DE
		public int? MaximumLength { get; set; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x000153E7 File Offset: 0x000135E7
		// (set) Token: 0x06000669 RID: 1641 RVA: 0x000153EF File Offset: 0x000135EF
		public double? DivisibleBy { get; set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x0600066A RID: 1642 RVA: 0x000153F8 File Offset: 0x000135F8
		// (set) Token: 0x0600066B RID: 1643 RVA: 0x00015400 File Offset: 0x00013600
		public double? Minimum { get; set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x0600066C RID: 1644 RVA: 0x00015409 File Offset: 0x00013609
		// (set) Token: 0x0600066D RID: 1645 RVA: 0x00015411 File Offset: 0x00013611
		public double? Maximum { get; set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600066E RID: 1646 RVA: 0x0001541A File Offset: 0x0001361A
		// (set) Token: 0x0600066F RID: 1647 RVA: 0x00015422 File Offset: 0x00013622
		public bool? ExclusiveMinimum { get; set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000670 RID: 1648 RVA: 0x0001542B File Offset: 0x0001362B
		// (set) Token: 0x06000671 RID: 1649 RVA: 0x00015433 File Offset: 0x00013633
		public bool? ExclusiveMaximum { get; set; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x0001543C File Offset: 0x0001363C
		// (set) Token: 0x06000673 RID: 1651 RVA: 0x00015444 File Offset: 0x00013644
		public int? MinimumItems { get; set; }

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000674 RID: 1652 RVA: 0x0001544D File Offset: 0x0001364D
		// (set) Token: 0x06000675 RID: 1653 RVA: 0x00015455 File Offset: 0x00013655
		public int? MaximumItems { get; set; }

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000676 RID: 1654 RVA: 0x0001545E File Offset: 0x0001365E
		// (set) Token: 0x06000677 RID: 1655 RVA: 0x00015466 File Offset: 0x00013666
		public IList<JsonSchema> Items { get; set; }

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000678 RID: 1656 RVA: 0x0001546F File Offset: 0x0001366F
		// (set) Token: 0x06000679 RID: 1657 RVA: 0x00015477 File Offset: 0x00013677
		public IDictionary<string, JsonSchema> Properties { get; set; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x0600067A RID: 1658 RVA: 0x00015480 File Offset: 0x00013680
		// (set) Token: 0x0600067B RID: 1659 RVA: 0x00015488 File Offset: 0x00013688
		public JsonSchema AdditionalProperties { get; set; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x00015491 File Offset: 0x00013691
		// (set) Token: 0x0600067D RID: 1661 RVA: 0x00015499 File Offset: 0x00013699
		public IDictionary<string, JsonSchema> PatternProperties { get; set; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x0600067E RID: 1662 RVA: 0x000154A2 File Offset: 0x000136A2
		// (set) Token: 0x0600067F RID: 1663 RVA: 0x000154AA File Offset: 0x000136AA
		public bool AllowAdditionalProperties { get; set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x000154B3 File Offset: 0x000136B3
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x000154BB File Offset: 0x000136BB
		public string Requires { get; set; }

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x000154C4 File Offset: 0x000136C4
		// (set) Token: 0x06000683 RID: 1667 RVA: 0x000154CC File Offset: 0x000136CC
		public IList<string> Identity { get; set; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000684 RID: 1668 RVA: 0x000154D5 File Offset: 0x000136D5
		// (set) Token: 0x06000685 RID: 1669 RVA: 0x000154DD File Offset: 0x000136DD
		public IList<JToken> Enum { get; set; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000686 RID: 1670 RVA: 0x000154E6 File Offset: 0x000136E6
		// (set) Token: 0x06000687 RID: 1671 RVA: 0x000154EE File Offset: 0x000136EE
		public IDictionary<JToken, string> Options { get; set; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000688 RID: 1672 RVA: 0x000154F7 File Offset: 0x000136F7
		// (set) Token: 0x06000689 RID: 1673 RVA: 0x000154FF File Offset: 0x000136FF
		public JsonSchemaType? Disallow { get; set; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x00015508 File Offset: 0x00013708
		// (set) Token: 0x0600068B RID: 1675 RVA: 0x00015510 File Offset: 0x00013710
		public JToken Default { get; set; }

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x0600068C RID: 1676 RVA: 0x00015519 File Offset: 0x00013719
		// (set) Token: 0x0600068D RID: 1677 RVA: 0x00015521 File Offset: 0x00013721
		public JsonSchema Extends { get; set; }

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600068E RID: 1678 RVA: 0x0001552A File Offset: 0x0001372A
		// (set) Token: 0x0600068F RID: 1679 RVA: 0x00015532 File Offset: 0x00013732
		public string Format { get; set; }

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000690 RID: 1680 RVA: 0x0001553B File Offset: 0x0001373B
		internal string InternalId
		{
			get
			{
				return this._internalId;
			}
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00015544 File Offset: 0x00013744
		public JsonSchema()
		{
			this.AllowAdditionalProperties = true;
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00015576 File Offset: 0x00013776
		public static JsonSchema Read(JsonReader reader)
		{
			return JsonSchema.Read(reader, new JsonSchemaResolver());
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00015584 File Offset: 0x00013784
		public static JsonSchema Read(JsonReader reader, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			ValidationUtils.ArgumentNotNull(resolver, "resolver");
			JsonSchemaBuilder jsonSchemaBuilder = new JsonSchemaBuilder(resolver);
			return jsonSchemaBuilder.Parse(reader);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x000155B5 File Offset: 0x000137B5
		public static JsonSchema Parse(string json)
		{
			return JsonSchema.Parse(json, new JsonSchemaResolver());
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x000155C4 File Offset: 0x000137C4
		public static JsonSchema Parse(string json, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(json, "json");
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return JsonSchema.Read(reader, resolver);
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x000155EF File Offset: 0x000137EF
		public void WriteTo(JsonWriter writer)
		{
			this.WriteTo(writer, new JsonSchemaResolver());
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00015600 File Offset: 0x00013800
		public void WriteTo(JsonWriter writer, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(writer, "writer");
			ValidationUtils.ArgumentNotNull(resolver, "resolver");
			JsonSchemaWriter jsonSchemaWriter = new JsonSchemaWriter(writer, resolver);
			jsonSchemaWriter.WriteSchema(this);
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00015634 File Offset: 0x00013834
		public override string ToString()
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.WriteTo(new JsonTextWriter(stringWriter)
			{
				Formatting = Formatting.Indented
			});
			return stringWriter.ToString();
		}

		// Token: 0x040001C6 RID: 454
		private readonly string _internalId = Guid.NewGuid().ToString("N");
	}
}

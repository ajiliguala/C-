using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x02000074 RID: 116
	internal class JsonSchemaModel
	{
		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000580 RID: 1408 RVA: 0x0001296E File Offset: 0x00010B6E
		// (set) Token: 0x06000581 RID: 1409 RVA: 0x00012976 File Offset: 0x00010B76
		public bool Required { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000582 RID: 1410 RVA: 0x0001297F File Offset: 0x00010B7F
		// (set) Token: 0x06000583 RID: 1411 RVA: 0x00012987 File Offset: 0x00010B87
		public JsonSchemaType Type { get; set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000584 RID: 1412 RVA: 0x00012990 File Offset: 0x00010B90
		// (set) Token: 0x06000585 RID: 1413 RVA: 0x00012998 File Offset: 0x00010B98
		public int? MinimumLength { get; set; }

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x06000586 RID: 1414 RVA: 0x000129A1 File Offset: 0x00010BA1
		// (set) Token: 0x06000587 RID: 1415 RVA: 0x000129A9 File Offset: 0x00010BA9
		public int? MaximumLength { get; set; }

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000588 RID: 1416 RVA: 0x000129B2 File Offset: 0x00010BB2
		// (set) Token: 0x06000589 RID: 1417 RVA: 0x000129BA File Offset: 0x00010BBA
		public double? DivisibleBy { get; set; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x0600058A RID: 1418 RVA: 0x000129C3 File Offset: 0x00010BC3
		// (set) Token: 0x0600058B RID: 1419 RVA: 0x000129CB File Offset: 0x00010BCB
		public double? Minimum { get; set; }

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600058C RID: 1420 RVA: 0x000129D4 File Offset: 0x00010BD4
		// (set) Token: 0x0600058D RID: 1421 RVA: 0x000129DC File Offset: 0x00010BDC
		public double? Maximum { get; set; }

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x0600058E RID: 1422 RVA: 0x000129E5 File Offset: 0x00010BE5
		// (set) Token: 0x0600058F RID: 1423 RVA: 0x000129ED File Offset: 0x00010BED
		public bool ExclusiveMinimum { get; set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000590 RID: 1424 RVA: 0x000129F6 File Offset: 0x00010BF6
		// (set) Token: 0x06000591 RID: 1425 RVA: 0x000129FE File Offset: 0x00010BFE
		public bool ExclusiveMaximum { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000592 RID: 1426 RVA: 0x00012A07 File Offset: 0x00010C07
		// (set) Token: 0x06000593 RID: 1427 RVA: 0x00012A0F File Offset: 0x00010C0F
		public int? MinimumItems { get; set; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000594 RID: 1428 RVA: 0x00012A18 File Offset: 0x00010C18
		// (set) Token: 0x06000595 RID: 1429 RVA: 0x00012A20 File Offset: 0x00010C20
		public int? MaximumItems { get; set; }

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000596 RID: 1430 RVA: 0x00012A29 File Offset: 0x00010C29
		// (set) Token: 0x06000597 RID: 1431 RVA: 0x00012A31 File Offset: 0x00010C31
		public IList<string> Patterns { get; set; }

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000598 RID: 1432 RVA: 0x00012A3A File Offset: 0x00010C3A
		// (set) Token: 0x06000599 RID: 1433 RVA: 0x00012A42 File Offset: 0x00010C42
		public IList<JsonSchemaModel> Items { get; set; }

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x00012A4B File Offset: 0x00010C4B
		// (set) Token: 0x0600059B RID: 1435 RVA: 0x00012A53 File Offset: 0x00010C53
		public IDictionary<string, JsonSchemaModel> Properties { get; set; }

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x0600059C RID: 1436 RVA: 0x00012A5C File Offset: 0x00010C5C
		// (set) Token: 0x0600059D RID: 1437 RVA: 0x00012A64 File Offset: 0x00010C64
		public IDictionary<string, JsonSchemaModel> PatternProperties { get; set; }

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x00012A6D File Offset: 0x00010C6D
		// (set) Token: 0x0600059F RID: 1439 RVA: 0x00012A75 File Offset: 0x00010C75
		public JsonSchemaModel AdditionalProperties { get; set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060005A0 RID: 1440 RVA: 0x00012A7E File Offset: 0x00010C7E
		// (set) Token: 0x060005A1 RID: 1441 RVA: 0x00012A86 File Offset: 0x00010C86
		public bool AllowAdditionalProperties { get; set; }

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x00012A8F File Offset: 0x00010C8F
		// (set) Token: 0x060005A3 RID: 1443 RVA: 0x00012A97 File Offset: 0x00010C97
		public IList<JToken> Enum { get; set; }

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x00012AA0 File Offset: 0x00010CA0
		// (set) Token: 0x060005A5 RID: 1445 RVA: 0x00012AA8 File Offset: 0x00010CA8
		public JsonSchemaType Disallow { get; set; }

		// Token: 0x060005A6 RID: 1446 RVA: 0x00012AB1 File Offset: 0x00010CB1
		public JsonSchemaModel()
		{
			this.Type = JsonSchemaType.Any;
			this.AllowAdditionalProperties = true;
			this.Required = false;
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x00012AD0 File Offset: 0x00010CD0
		public static JsonSchemaModel Create(IList<JsonSchema> schemata)
		{
			JsonSchemaModel jsonSchemaModel = new JsonSchemaModel();
			foreach (JsonSchema schema in schemata)
			{
				JsonSchemaModel.Combine(jsonSchemaModel, schema);
			}
			return jsonSchemaModel;
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x00012B20 File Offset: 0x00010D20
		private static void Combine(JsonSchemaModel model, JsonSchema schema)
		{
			model.Required = (model.Required || (schema.Required ?? false));
			model.Type &= (schema.Type ?? JsonSchemaType.Any);
			model.MinimumLength = MathUtils.Max(model.MinimumLength, schema.MinimumLength);
			model.MaximumLength = MathUtils.Min(model.MaximumLength, schema.MaximumLength);
			model.DivisibleBy = MathUtils.Max(model.DivisibleBy, schema.DivisibleBy);
			model.Minimum = MathUtils.Max(model.Minimum, schema.Minimum);
			model.Maximum = MathUtils.Max(model.Maximum, schema.Maximum);
			model.ExclusiveMinimum = (model.ExclusiveMinimum || (schema.ExclusiveMinimum ?? false));
			model.ExclusiveMaximum = (model.ExclusiveMaximum || (schema.ExclusiveMaximum ?? false));
			model.MinimumItems = MathUtils.Max(model.MinimumItems, schema.MinimumItems);
			model.MaximumItems = MathUtils.Min(model.MaximumItems, schema.MaximumItems);
			model.AllowAdditionalProperties = (model.AllowAdditionalProperties && schema.AllowAdditionalProperties);
			if (schema.Enum != null)
			{
				if (model.Enum == null)
				{
					model.Enum = new List<JToken>();
				}
				model.Enum.AddRangeDistinct(schema.Enum, new JTokenEqualityComparer());
			}
			model.Disallow |= (schema.Disallow ?? JsonSchemaType.None);
			if (schema.Pattern != null)
			{
				if (model.Patterns == null)
				{
					model.Patterns = new List<string>();
				}
				model.Patterns.AddDistinct(schema.Pattern);
			}
		}
	}
}

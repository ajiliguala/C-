using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x02000077 RID: 119
	internal class JsonSchemaNode
	{
		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x0001310A File Offset: 0x0001130A
		// (set) Token: 0x060005B4 RID: 1460 RVA: 0x00013112 File Offset: 0x00011312
		public string Id { get; private set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x0001311B File Offset: 0x0001131B
		// (set) Token: 0x060005B6 RID: 1462 RVA: 0x00013123 File Offset: 0x00011323
		public ReadOnlyCollection<JsonSchema> Schemas { get; private set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x0001312C File Offset: 0x0001132C
		// (set) Token: 0x060005B8 RID: 1464 RVA: 0x00013134 File Offset: 0x00011334
		public Dictionary<string, JsonSchemaNode> Properties { get; private set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x060005B9 RID: 1465 RVA: 0x0001313D File Offset: 0x0001133D
		// (set) Token: 0x060005BA RID: 1466 RVA: 0x00013145 File Offset: 0x00011345
		public Dictionary<string, JsonSchemaNode> PatternProperties { get; private set; }

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x060005BB RID: 1467 RVA: 0x0001314E File Offset: 0x0001134E
		// (set) Token: 0x060005BC RID: 1468 RVA: 0x00013156 File Offset: 0x00011356
		public List<JsonSchemaNode> Items { get; private set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x060005BD RID: 1469 RVA: 0x0001315F File Offset: 0x0001135F
		// (set) Token: 0x060005BE RID: 1470 RVA: 0x00013167 File Offset: 0x00011367
		public JsonSchemaNode AdditionalProperties { get; set; }

		// Token: 0x060005BF RID: 1471 RVA: 0x00013170 File Offset: 0x00011370
		public JsonSchemaNode(JsonSchema schema)
		{
			this.Schemas = new ReadOnlyCollection<JsonSchema>(new JsonSchema[]
			{
				schema
			});
			this.Properties = new Dictionary<string, JsonSchemaNode>();
			this.PatternProperties = new Dictionary<string, JsonSchemaNode>();
			this.Items = new List<JsonSchemaNode>();
			this.Id = JsonSchemaNode.GetId(this.Schemas);
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x000131CC File Offset: 0x000113CC
		private JsonSchemaNode(JsonSchemaNode source, JsonSchema schema)
		{
			this.Schemas = new ReadOnlyCollection<JsonSchema>(source.Schemas.Union(new JsonSchema[]
			{
				schema
			}).ToList<JsonSchema>());
			this.Properties = new Dictionary<string, JsonSchemaNode>(source.Properties);
			this.PatternProperties = new Dictionary<string, JsonSchemaNode>(source.PatternProperties);
			this.Items = new List<JsonSchemaNode>(source.Items);
			this.AdditionalProperties = source.AdditionalProperties;
			this.Id = JsonSchemaNode.GetId(this.Schemas);
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00013256 File Offset: 0x00011456
		public JsonSchemaNode Combine(JsonSchema schema)
		{
			return new JsonSchemaNode(this, schema);
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x0001326C File Offset: 0x0001146C
		public static string GetId(IEnumerable<JsonSchema> schemata)
		{
			return string.Join("-", (from s in schemata
			select s.InternalId).OrderBy((string id) => id, StringComparer.Ordinal).ToArray<string>());
		}
	}
}

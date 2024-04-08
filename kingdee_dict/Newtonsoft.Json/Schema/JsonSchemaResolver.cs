using System;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x02000078 RID: 120
	public class JsonSchemaResolver
	{
		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060005C5 RID: 1477 RVA: 0x000132D2 File Offset: 0x000114D2
		// (set) Token: 0x060005C6 RID: 1478 RVA: 0x000132DA File Offset: 0x000114DA
		public IList<JsonSchema> LoadedSchemas { get; protected set; }

		// Token: 0x060005C7 RID: 1479 RVA: 0x000132E3 File Offset: 0x000114E3
		public JsonSchemaResolver()
		{
			this.LoadedSchemas = new List<JsonSchema>();
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00013314 File Offset: 0x00011514
		public virtual JsonSchema GetSchema(string id)
		{
			return this.LoadedSchemas.SingleOrDefault((JsonSchema s) => s.Id == id);
		}
	}
}

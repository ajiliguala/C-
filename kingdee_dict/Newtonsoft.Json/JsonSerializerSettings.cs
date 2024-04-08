using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
	// Token: 0x02000040 RID: 64
	public class JsonSerializerSettings
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000244 RID: 580 RVA: 0x000087D9 File Offset: 0x000069D9
		// (set) Token: 0x06000245 RID: 581 RVA: 0x000087E1 File Offset: 0x000069E1
		public ReferenceLoopHandling ReferenceLoopHandling { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000246 RID: 582 RVA: 0x000087EA File Offset: 0x000069EA
		// (set) Token: 0x06000247 RID: 583 RVA: 0x000087F2 File Offset: 0x000069F2
		public MissingMemberHandling MissingMemberHandling { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000248 RID: 584 RVA: 0x000087FB File Offset: 0x000069FB
		// (set) Token: 0x06000249 RID: 585 RVA: 0x00008803 File Offset: 0x00006A03
		public ObjectCreationHandling ObjectCreationHandling { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600024A RID: 586 RVA: 0x0000880C File Offset: 0x00006A0C
		// (set) Token: 0x0600024B RID: 587 RVA: 0x00008814 File Offset: 0x00006A14
		public NullValueHandling NullValueHandling { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600024C RID: 588 RVA: 0x0000881D File Offset: 0x00006A1D
		// (set) Token: 0x0600024D RID: 589 RVA: 0x00008825 File Offset: 0x00006A25
		public DefaultValueHandling DefaultValueHandling { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600024E RID: 590 RVA: 0x0000882E File Offset: 0x00006A2E
		// (set) Token: 0x0600024F RID: 591 RVA: 0x00008836 File Offset: 0x00006A36
		public IList<JsonConverter> Converters { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000250 RID: 592 RVA: 0x0000883F File Offset: 0x00006A3F
		// (set) Token: 0x06000251 RID: 593 RVA: 0x00008847 File Offset: 0x00006A47
		public PreserveReferencesHandling PreserveReferencesHandling { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00008850 File Offset: 0x00006A50
		// (set) Token: 0x06000253 RID: 595 RVA: 0x00008858 File Offset: 0x00006A58
		public TypeNameHandling TypeNameHandling { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000254 RID: 596 RVA: 0x00008861 File Offset: 0x00006A61
		// (set) Token: 0x06000255 RID: 597 RVA: 0x00008869 File Offset: 0x00006A69
		public FormatterAssemblyStyle TypeNameAssemblyFormat { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000256 RID: 598 RVA: 0x00008872 File Offset: 0x00006A72
		// (set) Token: 0x06000257 RID: 599 RVA: 0x0000887A File Offset: 0x00006A7A
		public ConstructorHandling ConstructorHandling { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00008883 File Offset: 0x00006A83
		// (set) Token: 0x06000259 RID: 601 RVA: 0x0000888B File Offset: 0x00006A8B
		public IContractResolver ContractResolver { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600025A RID: 602 RVA: 0x00008894 File Offset: 0x00006A94
		// (set) Token: 0x0600025B RID: 603 RVA: 0x0000889C File Offset: 0x00006A9C
		public IReferenceResolver ReferenceResolver { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600025C RID: 604 RVA: 0x000088A5 File Offset: 0x00006AA5
		// (set) Token: 0x0600025D RID: 605 RVA: 0x000088AD File Offset: 0x00006AAD
		public SerializationBinder Binder { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600025E RID: 606 RVA: 0x000088B6 File Offset: 0x00006AB6
		// (set) Token: 0x0600025F RID: 607 RVA: 0x000088BE File Offset: 0x00006ABE
		public EventHandler<ErrorEventArgs> Error { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000260 RID: 608 RVA: 0x000088C7 File Offset: 0x00006AC7
		// (set) Token: 0x06000261 RID: 609 RVA: 0x000088CF File Offset: 0x00006ACF
		public StreamingContext Context { get; set; }

		// Token: 0x06000262 RID: 610 RVA: 0x000088D8 File Offset: 0x00006AD8
		public JsonSerializerSettings()
		{
			this.ReferenceLoopHandling = ReferenceLoopHandling.Error;
			this.MissingMemberHandling = MissingMemberHandling.Ignore;
			this.ObjectCreationHandling = ObjectCreationHandling.Auto;
			this.NullValueHandling = NullValueHandling.Include;
			this.DefaultValueHandling = DefaultValueHandling.Include;
			this.PreserveReferencesHandling = PreserveReferencesHandling.None;
			this.TypeNameHandling = TypeNameHandling.None;
			this.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
			this.Context = JsonSerializerSettings.DefaultContext;
			this.Converters = new List<JsonConverter>();
		}

		// Token: 0x040000B0 RID: 176
		internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;

		// Token: 0x040000B1 RID: 177
		internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;

		// Token: 0x040000B2 RID: 178
		internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;

		// Token: 0x040000B3 RID: 179
		internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;

		// Token: 0x040000B4 RID: 180
		internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;

		// Token: 0x040000B5 RID: 181
		internal const PreserveReferencesHandling DefaultPreserveReferencesHandling = PreserveReferencesHandling.None;

		// Token: 0x040000B6 RID: 182
		internal const ConstructorHandling DefaultConstructorHandling = ConstructorHandling.Default;

		// Token: 0x040000B7 RID: 183
		internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;

		// Token: 0x040000B8 RID: 184
		internal const FormatterAssemblyStyle DefaultTypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;

		// Token: 0x040000B9 RID: 185
		internal static readonly StreamingContext DefaultContext = default(StreamingContext);
	}
}

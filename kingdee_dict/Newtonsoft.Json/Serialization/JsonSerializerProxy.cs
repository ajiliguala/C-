using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000096 RID: 150
	internal class JsonSerializerProxy : JsonSerializer
	{
		// Token: 0x14000008 RID: 8
		// (add) Token: 0x0600071C RID: 1820 RVA: 0x00019B2C File Offset: 0x00017D2C
		// (remove) Token: 0x0600071D RID: 1821 RVA: 0x00019B3A File Offset: 0x00017D3A
		public override event EventHandler<ErrorEventArgs> Error
		{
			add
			{
				this._serializer.Error += value;
			}
			remove
			{
				this._serializer.Error -= value;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x0600071E RID: 1822 RVA: 0x00019B48 File Offset: 0x00017D48
		// (set) Token: 0x0600071F RID: 1823 RVA: 0x00019B55 File Offset: 0x00017D55
		public override IReferenceResolver ReferenceResolver
		{
			get
			{
				return this._serializer.ReferenceResolver;
			}
			set
			{
				this._serializer.ReferenceResolver = value;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x00019B63 File Offset: 0x00017D63
		public override JsonConverterCollection Converters
		{
			get
			{
				return this._serializer.Converters;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000721 RID: 1825 RVA: 0x00019B70 File Offset: 0x00017D70
		// (set) Token: 0x06000722 RID: 1826 RVA: 0x00019B7D File Offset: 0x00017D7D
		public override DefaultValueHandling DefaultValueHandling
		{
			get
			{
				return this._serializer.DefaultValueHandling;
			}
			set
			{
				this._serializer.DefaultValueHandling = value;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000723 RID: 1827 RVA: 0x00019B8B File Offset: 0x00017D8B
		// (set) Token: 0x06000724 RID: 1828 RVA: 0x00019B98 File Offset: 0x00017D98
		public override IContractResolver ContractResolver
		{
			get
			{
				return this._serializer.ContractResolver;
			}
			set
			{
				this._serializer.ContractResolver = value;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x00019BA6 File Offset: 0x00017DA6
		// (set) Token: 0x06000726 RID: 1830 RVA: 0x00019BB3 File Offset: 0x00017DB3
		public override MissingMemberHandling MissingMemberHandling
		{
			get
			{
				return this._serializer.MissingMemberHandling;
			}
			set
			{
				this._serializer.MissingMemberHandling = value;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000727 RID: 1831 RVA: 0x00019BC1 File Offset: 0x00017DC1
		// (set) Token: 0x06000728 RID: 1832 RVA: 0x00019BCE File Offset: 0x00017DCE
		public override NullValueHandling NullValueHandling
		{
			get
			{
				return this._serializer.NullValueHandling;
			}
			set
			{
				this._serializer.NullValueHandling = value;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000729 RID: 1833 RVA: 0x00019BDC File Offset: 0x00017DDC
		// (set) Token: 0x0600072A RID: 1834 RVA: 0x00019BE9 File Offset: 0x00017DE9
		public override ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return this._serializer.ObjectCreationHandling;
			}
			set
			{
				this._serializer.ObjectCreationHandling = value;
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600072B RID: 1835 RVA: 0x00019BF7 File Offset: 0x00017DF7
		// (set) Token: 0x0600072C RID: 1836 RVA: 0x00019C04 File Offset: 0x00017E04
		public override ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return this._serializer.ReferenceLoopHandling;
			}
			set
			{
				this._serializer.ReferenceLoopHandling = value;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x0600072D RID: 1837 RVA: 0x00019C12 File Offset: 0x00017E12
		// (set) Token: 0x0600072E RID: 1838 RVA: 0x00019C1F File Offset: 0x00017E1F
		public override PreserveReferencesHandling PreserveReferencesHandling
		{
			get
			{
				return this._serializer.PreserveReferencesHandling;
			}
			set
			{
				this._serializer.PreserveReferencesHandling = value;
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x0600072F RID: 1839 RVA: 0x00019C2D File Offset: 0x00017E2D
		// (set) Token: 0x06000730 RID: 1840 RVA: 0x00019C3A File Offset: 0x00017E3A
		public override TypeNameHandling TypeNameHandling
		{
			get
			{
				return this._serializer.TypeNameHandling;
			}
			set
			{
				this._serializer.TypeNameHandling = value;
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000731 RID: 1841 RVA: 0x00019C48 File Offset: 0x00017E48
		// (set) Token: 0x06000732 RID: 1842 RVA: 0x00019C55 File Offset: 0x00017E55
		public override FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			get
			{
				return this._serializer.TypeNameAssemblyFormat;
			}
			set
			{
				this._serializer.TypeNameAssemblyFormat = value;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000733 RID: 1843 RVA: 0x00019C63 File Offset: 0x00017E63
		// (set) Token: 0x06000734 RID: 1844 RVA: 0x00019C70 File Offset: 0x00017E70
		public override ConstructorHandling ConstructorHandling
		{
			get
			{
				return this._serializer.ConstructorHandling;
			}
			set
			{
				this._serializer.ConstructorHandling = value;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000735 RID: 1845 RVA: 0x00019C7E File Offset: 0x00017E7E
		// (set) Token: 0x06000736 RID: 1846 RVA: 0x00019C8B File Offset: 0x00017E8B
		public override SerializationBinder Binder
		{
			get
			{
				return this._serializer.Binder;
			}
			set
			{
				this._serializer.Binder = value;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000737 RID: 1847 RVA: 0x00019C99 File Offset: 0x00017E99
		// (set) Token: 0x06000738 RID: 1848 RVA: 0x00019CA6 File Offset: 0x00017EA6
		public override StreamingContext Context
		{
			get
			{
				return this._serializer.Context;
			}
			set
			{
				this._serializer.Context = value;
			}
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x00019CB4 File Offset: 0x00017EB4
		public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
		{
			ValidationUtils.ArgumentNotNull(serializerReader, "serializerReader");
			this._serializerReader = serializerReader;
			this._serializer = serializerReader.Serializer;
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00019CDA File Offset: 0x00017EDA
		public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
		{
			ValidationUtils.ArgumentNotNull(serializerWriter, "serializerWriter");
			this._serializerWriter = serializerWriter;
			this._serializer = serializerWriter.Serializer;
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x00019D00 File Offset: 0x00017F00
		internal override object DeserializeInternal(JsonReader reader, Type objectType)
		{
			if (this._serializerReader != null)
			{
				return this._serializerReader.Deserialize(reader, objectType);
			}
			return this._serializer.Deserialize(reader, objectType);
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x00019D25 File Offset: 0x00017F25
		internal override void PopulateInternal(JsonReader reader, object target)
		{
			if (this._serializerReader != null)
			{
				this._serializerReader.Populate(reader, target);
				return;
			}
			this._serializer.Populate(reader, target);
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x00019D4A File Offset: 0x00017F4A
		internal override void SerializeInternal(JsonWriter jsonWriter, object value)
		{
			if (this._serializerWriter != null)
			{
				this._serializerWriter.Serialize(jsonWriter, value);
				return;
			}
			this._serializer.Serialize(jsonWriter, value);
		}

		// Token: 0x04000234 RID: 564
		private readonly JsonSerializerInternalReader _serializerReader;

		// Token: 0x04000235 RID: 565
		private readonly JsonSerializerInternalWriter _serializerWriter;

		// Token: 0x04000236 RID: 566
		private readonly JsonSerializer _serializer;
	}
}

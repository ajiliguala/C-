using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x02000065 RID: 101
	public class JsonSerializer
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000409 RID: 1033 RVA: 0x0000E8A4 File Offset: 0x0000CAA4
		// (remove) Token: 0x0600040A RID: 1034 RVA: 0x0000E8DC File Offset: 0x0000CADC
		public virtual event EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> Error;

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x0000E911 File Offset: 0x0000CB11
		// (set) Token: 0x0600040C RID: 1036 RVA: 0x0000E92C File Offset: 0x0000CB2C
		public virtual IReferenceResolver ReferenceResolver
		{
			get
			{
				if (this._referenceResolver == null)
				{
					this._referenceResolver = new DefaultReferenceResolver();
				}
				return this._referenceResolver;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Reference resolver cannot be null.");
				}
				this._referenceResolver = value;
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x0000E948 File Offset: 0x0000CB48
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x0000E950 File Offset: 0x0000CB50
		public virtual SerializationBinder Binder
		{
			get
			{
				return this._binder;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Serialization binder cannot be null.");
				}
				this._binder = value;
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x0000E96C File Offset: 0x0000CB6C
		// (set) Token: 0x06000410 RID: 1040 RVA: 0x0000E974 File Offset: 0x0000CB74
		public virtual TypeNameHandling TypeNameHandling
		{
			get
			{
				return this._typeNameHandling;
			}
			set
			{
				if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._typeNameHandling = value;
			}
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000411 RID: 1041 RVA: 0x0000E990 File Offset: 0x0000CB90
		// (set) Token: 0x06000412 RID: 1042 RVA: 0x0000E998 File Offset: 0x0000CB98
		public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			get
			{
				return this._typeNameAssemblyFormat;
			}
			set
			{
				if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._typeNameAssemblyFormat = value;
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000413 RID: 1043 RVA: 0x0000E9B4 File Offset: 0x0000CBB4
		// (set) Token: 0x06000414 RID: 1044 RVA: 0x0000E9BC File Offset: 0x0000CBBC
		public virtual PreserveReferencesHandling PreserveReferencesHandling
		{
			get
			{
				return this._preserveReferencesHandling;
			}
			set
			{
				if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._preserveReferencesHandling = value;
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000415 RID: 1045 RVA: 0x0000E9D8 File Offset: 0x0000CBD8
		// (set) Token: 0x06000416 RID: 1046 RVA: 0x0000E9E0 File Offset: 0x0000CBE0
		public virtual ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return this._referenceLoopHandling;
			}
			set
			{
				if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._referenceLoopHandling = value;
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000417 RID: 1047 RVA: 0x0000E9FC File Offset: 0x0000CBFC
		// (set) Token: 0x06000418 RID: 1048 RVA: 0x0000EA04 File Offset: 0x0000CC04
		public virtual MissingMemberHandling MissingMemberHandling
		{
			get
			{
				return this._missingMemberHandling;
			}
			set
			{
				if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._missingMemberHandling = value;
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x0000EA20 File Offset: 0x0000CC20
		// (set) Token: 0x0600041A RID: 1050 RVA: 0x0000EA28 File Offset: 0x0000CC28
		public virtual NullValueHandling NullValueHandling
		{
			get
			{
				return this._nullValueHandling;
			}
			set
			{
				if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._nullValueHandling = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600041B RID: 1051 RVA: 0x0000EA44 File Offset: 0x0000CC44
		// (set) Token: 0x0600041C RID: 1052 RVA: 0x0000EA4C File Offset: 0x0000CC4C
		public virtual DefaultValueHandling DefaultValueHandling
		{
			get
			{
				return this._defaultValueHandling;
			}
			set
			{
				if (value < DefaultValueHandling.Include || value > DefaultValueHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._defaultValueHandling = value;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x0600041D RID: 1053 RVA: 0x0000EA68 File Offset: 0x0000CC68
		// (set) Token: 0x0600041E RID: 1054 RVA: 0x0000EA70 File Offset: 0x0000CC70
		public virtual ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return this._objectCreationHandling;
			}
			set
			{
				if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._objectCreationHandling = value;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600041F RID: 1055 RVA: 0x0000EA8C File Offset: 0x0000CC8C
		// (set) Token: 0x06000420 RID: 1056 RVA: 0x0000EA94 File Offset: 0x0000CC94
		public virtual ConstructorHandling ConstructorHandling
		{
			get
			{
				return this._constructorHandling;
			}
			set
			{
				if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._constructorHandling = value;
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x0000EAB0 File Offset: 0x0000CCB0
		public virtual JsonConverterCollection Converters
		{
			get
			{
				if (this._converters == null)
				{
					this._converters = new JsonConverterCollection();
				}
				return this._converters;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x0000EACB File Offset: 0x0000CCCB
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x0000EAE6 File Offset: 0x0000CCE6
		public virtual IContractResolver ContractResolver
		{
			get
			{
				if (this._contractResolver == null)
				{
					this._contractResolver = DefaultContractResolver.Instance;
				}
				return this._contractResolver;
			}
			set
			{
				this._contractResolver = value;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x0000EAEF File Offset: 0x0000CCEF
		// (set) Token: 0x06000425 RID: 1061 RVA: 0x0000EAF7 File Offset: 0x0000CCF7
		public virtual StreamingContext Context
		{
			get
			{
				return this._context;
			}
			set
			{
				this._context = value;
			}
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x0000EB00 File Offset: 0x0000CD00
		public JsonSerializer()
		{
			this._referenceLoopHandling = ReferenceLoopHandling.Error;
			this._missingMemberHandling = MissingMemberHandling.Ignore;
			this._nullValueHandling = NullValueHandling.Include;
			this._defaultValueHandling = DefaultValueHandling.Include;
			this._objectCreationHandling = ObjectCreationHandling.Auto;
			this._preserveReferencesHandling = PreserveReferencesHandling.None;
			this._constructorHandling = ConstructorHandling.Default;
			this._typeNameHandling = TypeNameHandling.None;
			this._context = JsonSerializerSettings.DefaultContext;
			this._binder = DefaultSerializationBinder.Instance;
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x0000EB64 File Offset: 0x0000CD64
		public static JsonSerializer Create(JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			if (settings != null)
			{
				if (!CollectionUtils.IsNullOrEmpty<JsonConverter>(settings.Converters))
				{
					jsonSerializer.Converters.AddRange(settings.Converters);
				}
				jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
				jsonSerializer.TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat;
				jsonSerializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
				jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
				jsonSerializer.MissingMemberHandling = settings.MissingMemberHandling;
				jsonSerializer.ObjectCreationHandling = settings.ObjectCreationHandling;
				jsonSerializer.NullValueHandling = settings.NullValueHandling;
				jsonSerializer.DefaultValueHandling = settings.DefaultValueHandling;
				jsonSerializer.ConstructorHandling = settings.ConstructorHandling;
				jsonSerializer.Context = settings.Context;
				if (settings.Error != null)
				{
					jsonSerializer.Error += settings.Error;
				}
				if (settings.ContractResolver != null)
				{
					jsonSerializer.ContractResolver = settings.ContractResolver;
				}
				if (settings.ReferenceResolver != null)
				{
					jsonSerializer.ReferenceResolver = settings.ReferenceResolver;
				}
				if (settings.Binder != null)
				{
					jsonSerializer.Binder = settings.Binder;
				}
			}
			return jsonSerializer;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x0000EC64 File Offset: 0x0000CE64
		public void Populate(TextReader reader, object target)
		{
			this.Populate(new JsonTextReader(reader), target);
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x0000EC73 File Offset: 0x0000CE73
		public void Populate(JsonReader reader, object target)
		{
			this.PopulateInternal(reader, target);
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0000EC80 File Offset: 0x0000CE80
		internal virtual void PopulateInternal(JsonReader reader, object target)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			ValidationUtils.ArgumentNotNull(target, "target");
			JsonSerializerInternalReader jsonSerializerInternalReader = new JsonSerializerInternalReader(this);
			jsonSerializerInternalReader.Populate(reader, target);
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x0000ECB2 File Offset: 0x0000CEB2
		public object Deserialize(JsonReader reader)
		{
			return this.Deserialize(reader, null);
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x0000ECBC File Offset: 0x0000CEBC
		public object Deserialize(TextReader reader, Type objectType)
		{
			return this.Deserialize(new JsonTextReader(reader), objectType);
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0000ECCB File Offset: 0x0000CECB
		public T Deserialize<T>(JsonReader reader)
		{
			return (T)((object)this.Deserialize(reader, typeof(T)));
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0000ECE3 File Offset: 0x0000CEE3
		public object Deserialize(JsonReader reader, Type objectType)
		{
			return this.DeserializeInternal(reader, objectType);
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0000ECF0 File Offset: 0x0000CEF0
		internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			JsonSerializerInternalReader jsonSerializerInternalReader = new JsonSerializerInternalReader(this);
			return jsonSerializerInternalReader.Deserialize(reader, objectType);
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x0000ED17 File Offset: 0x0000CF17
		public void Serialize(TextWriter textWriter, object value)
		{
			this.Serialize(new JsonTextWriter(textWriter), value);
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0000ED26 File Offset: 0x0000CF26
		public void Serialize(JsonWriter jsonWriter, object value)
		{
			this.SerializeInternal(jsonWriter, value);
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x0000ED30 File Offset: 0x0000CF30
		internal virtual void SerializeInternal(JsonWriter jsonWriter, object value)
		{
			ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");
			JsonSerializerInternalWriter jsonSerializerInternalWriter = new JsonSerializerInternalWriter(this);
			jsonSerializerInternalWriter.Serialize(jsonWriter, value);
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0000ED57 File Offset: 0x0000CF57
		internal JsonConverter GetMatchingConverter(Type type)
		{
			return JsonSerializer.GetMatchingConverter(this._converters, type);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x0000ED68 File Offset: 0x0000CF68
		internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(objectType, "objectType");
			if (converters != null)
			{
				for (int i = 0; i < converters.Count; i++)
				{
					JsonConverter jsonConverter = converters[i];
					if (jsonConverter.CanConvert(objectType))
					{
						return jsonConverter;
					}
				}
			}
			return null;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0000EDA8 File Offset: 0x0000CFA8
		internal void OnError(Newtonsoft.Json.Serialization.ErrorEventArgs e)
		{
			EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> error = this.Error;
			if (error != null)
			{
				error(this, e);
			}
		}

		// Token: 0x04000124 RID: 292
		private TypeNameHandling _typeNameHandling;

		// Token: 0x04000125 RID: 293
		private FormatterAssemblyStyle _typeNameAssemblyFormat;

		// Token: 0x04000126 RID: 294
		private PreserveReferencesHandling _preserveReferencesHandling;

		// Token: 0x04000127 RID: 295
		private ReferenceLoopHandling _referenceLoopHandling;

		// Token: 0x04000128 RID: 296
		private MissingMemberHandling _missingMemberHandling;

		// Token: 0x04000129 RID: 297
		private ObjectCreationHandling _objectCreationHandling;

		// Token: 0x0400012A RID: 298
		private NullValueHandling _nullValueHandling;

		// Token: 0x0400012B RID: 299
		private DefaultValueHandling _defaultValueHandling;

		// Token: 0x0400012C RID: 300
		private ConstructorHandling _constructorHandling;

		// Token: 0x0400012D RID: 301
		private JsonConverterCollection _converters;

		// Token: 0x0400012E RID: 302
		private IContractResolver _contractResolver;

		// Token: 0x0400012F RID: 303
		private IReferenceResolver _referenceResolver;

		// Token: 0x04000130 RID: 304
		private SerializationBinder _binder;

		// Token: 0x04000131 RID: 305
		private StreamingContext _context;
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x0200008E RID: 142
	public class JsonSchemaGenerator
	{
		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060006AD RID: 1709 RVA: 0x000165B1 File Offset: 0x000147B1
		// (set) Token: 0x060006AE RID: 1710 RVA: 0x000165B9 File Offset: 0x000147B9
		public UndefinedSchemaIdHandling UndefinedSchemaIdHandling { get; set; }

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x060006AF RID: 1711 RVA: 0x000165C2 File Offset: 0x000147C2
		// (set) Token: 0x060006B0 RID: 1712 RVA: 0x000165D8 File Offset: 0x000147D8
		public IContractResolver ContractResolver
		{
			get
			{
				if (this._contractResolver == null)
				{
					return DefaultContractResolver.Instance;
				}
				return this._contractResolver;
			}
			set
			{
				this._contractResolver = value;
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060006B1 RID: 1713 RVA: 0x000165E1 File Offset: 0x000147E1
		private JsonSchema CurrentSchema
		{
			get
			{
				return this._currentSchema;
			}
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x000165E9 File Offset: 0x000147E9
		private void Push(JsonSchemaGenerator.TypeSchema typeSchema)
		{
			this._currentSchema = typeSchema.Schema;
			this._stack.Add(typeSchema);
			this._resolver.LoadedSchemas.Add(typeSchema.Schema);
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0001661C File Offset: 0x0001481C
		private JsonSchemaGenerator.TypeSchema Pop()
		{
			JsonSchemaGenerator.TypeSchema result = this._stack[this._stack.Count - 1];
			this._stack.RemoveAt(this._stack.Count - 1);
			JsonSchemaGenerator.TypeSchema typeSchema = this._stack.LastOrDefault<JsonSchemaGenerator.TypeSchema>();
			if (typeSchema != null)
			{
				this._currentSchema = typeSchema.Schema;
			}
			else
			{
				this._currentSchema = null;
			}
			return result;
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0001667F File Offset: 0x0001487F
		public JsonSchema Generate(Type type)
		{
			return this.Generate(type, new JsonSchemaResolver(), false);
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0001668E File Offset: 0x0001488E
		public JsonSchema Generate(Type type, JsonSchemaResolver resolver)
		{
			return this.Generate(type, resolver, false);
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00016699 File Offset: 0x00014899
		public JsonSchema Generate(Type type, bool rootSchemaNullable)
		{
			return this.Generate(type, new JsonSchemaResolver(), rootSchemaNullable);
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x000166A8 File Offset: 0x000148A8
		public JsonSchema Generate(Type type, JsonSchemaResolver resolver, bool rootSchemaNullable)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			ValidationUtils.ArgumentNotNull(resolver, "resolver");
			this._resolver = resolver;
			return this.GenerateInternal(type, (!rootSchemaNullable) ? Required.Always : Required.Default, false);
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x000166D8 File Offset: 0x000148D8
		private string GetTitle(Type type)
		{
			JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);
			if (jsonContainerAttribute != null && !string.IsNullOrEmpty(jsonContainerAttribute.Title))
			{
				return jsonContainerAttribute.Title;
			}
			return null;
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x00016704 File Offset: 0x00014904
		private string GetDescription(Type type)
		{
			JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);
			if (jsonContainerAttribute != null && !string.IsNullOrEmpty(jsonContainerAttribute.Description))
			{
				return jsonContainerAttribute.Description;
			}
			DescriptionAttribute attribute = ReflectionUtils.GetAttribute<DescriptionAttribute>(type);
			if (attribute != null)
			{
				return attribute.Description;
			}
			return null;
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x00016744 File Offset: 0x00014944
		private string GetTypeId(Type type, bool explicitOnly)
		{
			JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);
			if (jsonContainerAttribute != null && !string.IsNullOrEmpty(jsonContainerAttribute.Id))
			{
				return jsonContainerAttribute.Id;
			}
			if (explicitOnly)
			{
				return null;
			}
			switch (this.UndefinedSchemaIdHandling)
			{
			case UndefinedSchemaIdHandling.UseTypeName:
				return type.FullName;
			case UndefinedSchemaIdHandling.UseAssemblyQualifiedName:
				return type.AssemblyQualifiedName;
			default:
				return null;
			}
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x000167B8 File Offset: 0x000149B8
		private JsonSchema GenerateInternal(Type type, Required valueRequired, bool required)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			string typeId = this.GetTypeId(type, false);
			string typeId2 = this.GetTypeId(type, true);
			if (!string.IsNullOrEmpty(typeId))
			{
				JsonSchema schema = this._resolver.GetSchema(typeId);
				if (schema != null)
				{
					if (valueRequired != Required.Always && !JsonSchemaGenerator.HasFlag(schema.Type, JsonSchemaType.Null))
					{
						schema.Type |= JsonSchemaType.Null;
					}
					if (required && schema.Required != true)
					{
						schema.Required = new bool?(true);
					}
					return schema;
				}
			}
			if (this._stack.Any((JsonSchemaGenerator.TypeSchema tc) => tc.Type == type))
			{
				throw new Exception("Unresolved circular reference for type '{0}'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}));
			}
			JsonContract jsonContract = this.ContractResolver.ResolveContract(type);
			JsonConverter jsonConverter;
			if ((jsonConverter = jsonContract.Converter) != null || (jsonConverter = jsonContract.InternalConverter) != null)
			{
				JsonSchema schema2 = jsonConverter.GetSchema();
				if (schema2 != null)
				{
					return schema2;
				}
			}
			this.Push(new JsonSchemaGenerator.TypeSchema(type, new JsonSchema()));
			if (typeId2 != null)
			{
				this.CurrentSchema.Id = typeId2;
			}
			if (required)
			{
				this.CurrentSchema.Required = new bool?(true);
			}
			this.CurrentSchema.Title = this.GetTitle(type);
			this.CurrentSchema.Description = this.GetDescription(type);
			if (jsonConverter != null)
			{
				this.CurrentSchema.Type = new JsonSchemaType?(JsonSchemaType.Any);
			}
			else if (jsonContract is JsonDictionaryContract)
			{
				this.CurrentSchema.Type = new JsonSchemaType?(this.AddNullType(JsonSchemaType.Object, valueRequired));
				Type type2;
				Type type3;
				ReflectionUtils.GetDictionaryKeyValueTypes(type, out type2, out type3);
				if (type2 != null && typeof(IConvertible).IsAssignableFrom(type2))
				{
					this.CurrentSchema.AdditionalProperties = this.GenerateInternal(type3, Required.Default, false);
				}
			}
			else if (jsonContract is JsonArrayContract)
			{
				this.CurrentSchema.Type = new JsonSchemaType?(this.AddNullType(JsonSchemaType.Array, valueRequired));
				this.CurrentSchema.Id = this.GetTypeId(type, false);
				JsonArrayAttribute jsonArrayAttribute = JsonTypeReflector.GetJsonContainerAttribute(type) as JsonArrayAttribute;
				bool flag = jsonArrayAttribute == null || jsonArrayAttribute.AllowNullItems;
				Type collectionItemType = ReflectionUtils.GetCollectionItemType(type);
				if (collectionItemType != null)
				{
					this.CurrentSchema.Items = new List<JsonSchema>();
					this.CurrentSchema.Items.Add(this.GenerateInternal(collectionItemType, (!flag) ? Required.Always : Required.Default, false));
				}
			}
			else
			{
				if (jsonContract is JsonPrimitiveContract)
				{
					this.CurrentSchema.Type = new JsonSchemaType?(this.GetJsonSchemaType(type, valueRequired));
					if (!(this.CurrentSchema.Type == JsonSchemaType.Integer) || !type.IsEnum || type.IsDefined(typeof(FlagsAttribute), true))
					{
						goto IL_51D;
					}
					this.CurrentSchema.Enum = new List<JToken>();
					this.CurrentSchema.Options = new Dictionary<JToken, string>();
					EnumValues<long> namesAndValues = EnumUtils.GetNamesAndValues<long>(type);
					using (IEnumerator<EnumValue<long>> enumerator = namesAndValues.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							EnumValue<long> enumValue = enumerator.Current;
							JToken jtoken = JToken.FromObject(enumValue.Value);
							this.CurrentSchema.Enum.Add(jtoken);
							this.CurrentSchema.Options.Add(jtoken, enumValue.Name);
						}
						goto IL_51D;
					}
				}
				if (jsonContract is JsonObjectContract)
				{
					this.CurrentSchema.Type = new JsonSchemaType?(this.AddNullType(JsonSchemaType.Object, valueRequired));
					this.CurrentSchema.Id = this.GetTypeId(type, false);
					this.GenerateObjectSchema(type, (JsonObjectContract)jsonContract);
				}
				else if (jsonContract is JsonISerializableContract)
				{
					this.CurrentSchema.Type = new JsonSchemaType?(this.AddNullType(JsonSchemaType.Object, valueRequired));
					this.CurrentSchema.Id = this.GetTypeId(type, false);
					this.GenerateISerializableContract(type, (JsonISerializableContract)jsonContract);
				}
				else if (jsonContract is JsonStringContract)
				{
					JsonSchemaType value = (!ReflectionUtils.IsNullable(jsonContract.UnderlyingType)) ? JsonSchemaType.String : this.AddNullType(JsonSchemaType.String, valueRequired);
					this.CurrentSchema.Type = new JsonSchemaType?(value);
				}
				else
				{
					if (!(jsonContract is JsonLinqContract))
					{
						throw new Exception("Unexpected contract type: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							jsonContract
						}));
					}
					this.CurrentSchema.Type = new JsonSchemaType?(JsonSchemaType.Any);
				}
			}
			IL_51D:
			return this.Pop().Schema;
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00016D00 File Offset: 0x00014F00
		private JsonSchemaType AddNullType(JsonSchemaType type, Required valueRequired)
		{
			if (valueRequired != Required.Always)
			{
				return type | JsonSchemaType.Null;
			}
			return type;
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00016D0C File Offset: 0x00014F0C
		private void GenerateObjectSchema(Type type, JsonObjectContract contract)
		{
			this.CurrentSchema.Properties = new Dictionary<string, JsonSchema>();
			foreach (JsonProperty jsonProperty in contract.Properties)
			{
				if (!jsonProperty.Ignored)
				{
					bool flag = jsonProperty.NullValueHandling == NullValueHandling.Ignore || jsonProperty.DefaultValueHandling == DefaultValueHandling.Ignore || jsonProperty.ShouldSerialize != null || jsonProperty.GetIsSpecified != null;
					JsonSchema jsonSchema = this.GenerateInternal(jsonProperty.PropertyType, jsonProperty.Required, !flag);
					if (jsonProperty.DefaultValue != null)
					{
						jsonSchema.Default = JToken.FromObject(jsonProperty.DefaultValue);
					}
					this.CurrentSchema.Properties.Add(jsonProperty.PropertyName, jsonSchema);
				}
			}
			if (type.IsSealed)
			{
				this.CurrentSchema.AllowAdditionalProperties = false;
			}
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00016E24 File Offset: 0x00015024
		private void GenerateISerializableContract(Type type, JsonISerializableContract contract)
		{
			this.CurrentSchema.AllowAdditionalProperties = true;
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00016E34 File Offset: 0x00015034
		internal static bool HasFlag(JsonSchemaType? value, JsonSchemaType flag)
		{
			return value == null || (value & flag) == flag;
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x00016E8C File Offset: 0x0001508C
		private JsonSchemaType GetJsonSchemaType(Type type, Required valueRequired)
		{
			JsonSchemaType jsonSchemaType = JsonSchemaType.None;
			if (valueRequired != Required.Always && ReflectionUtils.IsNullable(type))
			{
				jsonSchemaType = JsonSchemaType.Null;
				if (ReflectionUtils.IsNullableType(type))
				{
					type = Nullable.GetUnderlyingType(type);
				}
			}
			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
			case TypeCode.Empty:
			case TypeCode.Object:
				return jsonSchemaType | JsonSchemaType.String;
			case TypeCode.DBNull:
				return jsonSchemaType | JsonSchemaType.Null;
			case TypeCode.Boolean:
				return jsonSchemaType | JsonSchemaType.Boolean;
			case TypeCode.Char:
				return jsonSchemaType | JsonSchemaType.String;
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				return jsonSchemaType | JsonSchemaType.Integer;
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return jsonSchemaType | JsonSchemaType.Float;
			case TypeCode.DateTime:
				return jsonSchemaType | JsonSchemaType.String;
			case TypeCode.String:
				return jsonSchemaType | JsonSchemaType.String;
			}
			throw new Exception("Unexpected type code '{0}' for type '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeCode,
				type
			}));
		}

		// Token: 0x0400020C RID: 524
		private IContractResolver _contractResolver;

		// Token: 0x0400020D RID: 525
		private JsonSchemaResolver _resolver;

		// Token: 0x0400020E RID: 526
		private IList<JsonSchemaGenerator.TypeSchema> _stack = new List<JsonSchemaGenerator.TypeSchema>();

		// Token: 0x0400020F RID: 527
		private JsonSchema _currentSchema;

		// Token: 0x0200008F RID: 143
		private class TypeSchema
		{
			// Token: 0x17000163 RID: 355
			// (get) Token: 0x060006C2 RID: 1730 RVA: 0x00016F74 File Offset: 0x00015174
			// (set) Token: 0x060006C3 RID: 1731 RVA: 0x00016F7C File Offset: 0x0001517C
			public Type Type { get; private set; }

			// Token: 0x17000164 RID: 356
			// (get) Token: 0x060006C4 RID: 1732 RVA: 0x00016F85 File Offset: 0x00015185
			// (set) Token: 0x060006C5 RID: 1733 RVA: 0x00016F8D File Offset: 0x0001518D
			public JsonSchema Schema { get; private set; }

			// Token: 0x060006C6 RID: 1734 RVA: 0x00016F96 File Offset: 0x00015196
			public TypeSchema(Type type, JsonSchema schema)
			{
				ValidationUtils.ArgumentNotNull(type, "type");
				ValidationUtils.ArgumentNotNull(schema, "schema");
				this.Type = type;
				this.Schema = schema;
			}
		}
	}
}

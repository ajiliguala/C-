using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000093 RID: 147
	internal class JsonSerializerInternalReader : JsonSerializerInternalBase
	{
		// Token: 0x060006D6 RID: 1750 RVA: 0x000170EB File Offset: 0x000152EB
		public JsonSerializerInternalReader(JsonSerializer serializer) : base(serializer)
		{
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x000170F4 File Offset: 0x000152F4
		public void Populate(JsonReader reader, object target)
		{
			ValidationUtils.ArgumentNotNull(target, "target");
			Type type = target.GetType();
			JsonContract jsonContract = base.Serializer.ContractResolver.ResolveContract(type);
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.StartArray)
			{
				if (jsonContract is JsonArrayContract)
				{
					this.PopulateList(CollectionUtils.CreateCollectionWrapper(target), reader, null, (JsonArrayContract)jsonContract);
					return;
				}
				throw new JsonSerializationException("Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}));
			}
			else
			{
				if (reader.TokenType != JsonToken.StartObject)
				{
					throw new JsonSerializationException("Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.TokenType
					}));
				}
				this.CheckedRead(reader);
				string id = null;
				if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), "$id", StringComparison.Ordinal))
				{
					this.CheckedRead(reader);
					id = reader.Value.ToString();
					this.CheckedRead(reader);
				}
				if (jsonContract is JsonDictionaryContract)
				{
					this.PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader, (JsonDictionaryContract)jsonContract, id);
					return;
				}
				if (jsonContract is JsonObjectContract)
				{
					this.PopulateObject(target, reader, (JsonObjectContract)jsonContract, id);
					return;
				}
				throw new JsonSerializationException("Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}));
			}
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x00017254 File Offset: 0x00015454
		private JsonContract GetContractSafe(Type type)
		{
			if (type == null)
			{
				return null;
			}
			return base.Serializer.ContractResolver.ResolveContract(type);
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x00017272 File Offset: 0x00015472
		private JsonContract GetContractSafe(Type type, object value)
		{
			if (value == null)
			{
				return this.GetContractSafe(type);
			}
			return base.Serializer.ContractResolver.ResolveContract(value.GetType());
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x00017295 File Offset: 0x00015495
		public object Deserialize(JsonReader reader, Type objectType)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (reader.TokenType == JsonToken.None && !this.ReadForType(reader, objectType, null))
			{
				return null;
			}
			return this.CreateValueNonProperty(reader, objectType, this.GetContractSafe(objectType));
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x000172C9 File Offset: 0x000154C9
		private JsonSerializerProxy GetInternalSerializer()
		{
			if (this._internalSerializer == null)
			{
				this._internalSerializer = new JsonSerializerProxy(this);
			}
			return this._internalSerializer;
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x000172E5 File Offset: 0x000154E5
		private JsonFormatterConverter GetFormatterConverter()
		{
			if (this._formatterConverter == null)
			{
				this._formatterConverter = new JsonFormatterConverter(this.GetInternalSerializer());
			}
			return this._formatterConverter;
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x00017308 File Offset: 0x00015508
		private JToken CreateJToken(JsonReader reader, JsonContract contract)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (contract != null && contract.UnderlyingType == typeof(JRaw))
			{
				return JRaw.Create(reader);
			}
			JToken token;
			using (JTokenWriter jtokenWriter = new JTokenWriter())
			{
				jtokenWriter.WriteToken(reader);
				token = jtokenWriter.Token;
			}
			return token;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00017374 File Offset: 0x00015574
		private JToken CreateJObject(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			JToken token;
			using (JTokenWriter jtokenWriter = new JTokenWriter())
			{
				jtokenWriter.WriteStartObject();
				if (reader.TokenType == JsonToken.PropertyName)
				{
					jtokenWriter.WriteToken(reader, reader.Depth - 1);
				}
				else
				{
					jtokenWriter.WriteEndObject();
				}
				token = jtokenWriter.Token;
			}
			return token;
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x000173DC File Offset: 0x000155DC
		private object CreateValueProperty(JsonReader reader, JsonProperty property, object target, bool gottenCurrentValue, object currentValue)
		{
			JsonContract contractSafe = this.GetContractSafe(property.PropertyType, currentValue);
			Type propertyType = property.PropertyType;
			JsonConverter converter = this.GetConverter(contractSafe, property.MemberConverter);
			if (converter != null && converter.CanRead)
			{
				if (!gottenCurrentValue && target != null && property.Readable)
				{
					currentValue = property.ValueProvider.GetValue(target);
				}
				return converter.ReadJson(reader, propertyType, currentValue, this.GetInternalSerializer());
			}
			return this.CreateValueInternal(reader, propertyType, contractSafe, property, currentValue);
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x00017454 File Offset: 0x00015654
		private object CreateValueNonProperty(JsonReader reader, Type objectType, JsonContract contract)
		{
			JsonConverter converter = this.GetConverter(contract, null);
			if (converter != null && converter.CanRead)
			{
				return converter.ReadJson(reader, objectType, null, this.GetInternalSerializer());
			}
			return this.CreateValueInternal(reader, objectType, contract, null, null);
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x00017490 File Offset: 0x00015690
		private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue)
		{
			if (contract is JsonLinqContract)
			{
				return this.CreateJToken(reader, contract);
			}
			for (;;)
			{
				switch (reader.TokenType)
				{
				case JsonToken.StartObject:
					goto IL_69;
				case JsonToken.StartArray:
					goto IL_77;
				case JsonToken.StartConstructor:
				case JsonToken.EndConstructor:
					goto IL_F4;
				case JsonToken.Comment:
					if (!reader.Read())
					{
						goto Block_8;
					}
					continue;
				case JsonToken.Raw:
					goto IL_12D;
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
					goto IL_86;
				case JsonToken.String:
					goto IL_99;
				case JsonToken.Null:
				case JsonToken.Undefined:
					goto IL_102;
				}
				break;
			}
			goto IL_13E;
			IL_69:
			return this.CreateObject(reader, objectType, contract, member, existingValue);
			IL_77:
			return this.CreateList(reader, objectType, contract, member, existingValue, null);
			IL_86:
			return this.EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
			IL_99:
			if (string.IsNullOrEmpty((string)reader.Value) && objectType != null && ReflectionUtils.IsNullableType(objectType))
			{
				return null;
			}
			if (objectType == typeof(byte[]))
			{
				return Convert.FromBase64String((string)reader.Value);
			}
			return this.EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
			IL_F4:
			return reader.Value.ToString();
			IL_102:
			if (objectType == typeof(DBNull))
			{
				return DBNull.Value;
			}
			return this.EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
			IL_12D:
			return new JRaw((string)reader.Value);
			IL_13E:
			throw new JsonSerializationException("Unexpected token while deserializing object: " + reader.TokenType);
			Block_8:
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0001760C File Offset: 0x0001580C
		private JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter)
		{
			JsonConverter result = null;
			if (memberConverter != null)
			{
				result = memberConverter;
			}
			else if (contract != null)
			{
				JsonConverter matchingConverter;
				if (contract.Converter != null)
				{
					result = contract.Converter;
				}
				else if ((matchingConverter = base.Serializer.GetMatchingConverter(contract.UnderlyingType)) != null)
				{
					result = matchingConverter;
				}
				else if (contract.InternalConverter != null)
				{
					result = contract.InternalConverter;
				}
			}
			return result;
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x00017660 File Offset: 0x00015860
		private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue)
		{
			this.CheckedRead(reader);
			string text = null;
			if (reader.TokenType == JsonToken.PropertyName)
			{
				string text2;
				Type type;
				for (;;)
				{
					string a = reader.Value.ToString();
					if (string.Equals(a, "$ref", StringComparison.Ordinal))
					{
						break;
					}
					bool flag;
					if (string.Equals(a, "$type", StringComparison.Ordinal))
					{
						this.CheckedRead(reader);
						text2 = reader.Value.ToString();
						this.CheckedRead(reader);
						if ((((member != null) ? member.TypeNameHandling : null) ?? base.Serializer.TypeNameHandling) != TypeNameHandling.None)
						{
							string typeName;
							string assemblyName;
							ReflectionUtils.SplitFullyQualifiedTypeName(text2, out typeName, out assemblyName);
							try
							{
								type = base.Serializer.Binder.BindToType(assemblyName, typeName);
							}
							catch (Exception innerException)
							{
								throw new JsonSerializationException("Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
								{
									text2
								}), innerException);
							}
							if (type == null)
							{
								goto Block_9;
							}
							if (objectType != null && !objectType.IsAssignableFrom(type))
							{
								goto Block_11;
							}
							objectType = type;
							contract = this.GetContractSafe(type);
						}
						flag = true;
					}
					else if (string.Equals(a, "$id", StringComparison.Ordinal))
					{
						this.CheckedRead(reader);
						text = reader.Value.ToString();
						this.CheckedRead(reader);
						flag = true;
					}
					else
					{
						if (string.Equals(a, "$values", StringComparison.Ordinal))
						{
							goto Block_13;
						}
						flag = false;
					}
					if (!flag || reader.TokenType != JsonToken.PropertyName)
					{
						goto IL_268;
					}
				}
				this.CheckedRead(reader);
				if (reader.TokenType != JsonToken.String)
				{
					throw new JsonSerializationException("JSON reference {0} property must have a string value.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						"$ref"
					}));
				}
				string reference = reader.Value.ToString();
				this.CheckedRead(reader);
				if (reader.TokenType == JsonToken.PropertyName)
				{
					throw new JsonSerializationException("Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						"$ref"
					}));
				}
				return base.Serializer.ReferenceResolver.ResolveReference(reference);
				Block_9:
				throw new JsonSerializationException("Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text2
				}));
				Block_11:
				throw new JsonSerializationException("Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type.AssemblyQualifiedName,
					objectType.AssemblyQualifiedName
				}));
				Block_13:
				this.CheckedRead(reader);
				object result = this.CreateList(reader, objectType, contract, member, existingValue, text);
				this.CheckedRead(reader);
				return result;
			}
			IL_268:
			if (!this.HasDefinedType(objectType))
			{
				return this.CreateJObject(reader);
			}
			if (contract == null)
			{
				throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					objectType
				}));
			}
			JsonDictionaryContract jsonDictionaryContract = contract as JsonDictionaryContract;
			if (jsonDictionaryContract != null)
			{
				if (existingValue == null)
				{
					return this.CreateAndPopulateDictionary(reader, jsonDictionaryContract, text);
				}
				return this.PopulateDictionary(jsonDictionaryContract.CreateWrapper(existingValue), reader, jsonDictionaryContract, text);
			}
			else
			{
				JsonObjectContract jsonObjectContract = contract as JsonObjectContract;
				if (jsonObjectContract != null)
				{
					if (existingValue == null)
					{
						return this.CreateAndPopulateObject(reader, jsonObjectContract, text);
					}
					return this.PopulateObject(existingValue, reader, jsonObjectContract, text);
				}
				else
				{
					JsonISerializableContract jsonISerializableContract = contract as JsonISerializableContract;
					if (jsonISerializableContract != null)
					{
						return this.CreateISerializable(reader, jsonISerializableContract, text);
					}
					JsonDynamicContract jsonDynamicContract = contract as JsonDynamicContract;
					if (jsonDynamicContract != null)
					{
						return this.CreateDynamic(reader, jsonDynamicContract, text);
					}
					throw new JsonSerializationException("Cannot deserialize JSON object into type '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						objectType
					}));
				}
			}
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x000179C8 File Offset: 0x00015BC8
		private JsonArrayContract EnsureArrayContract(Type objectType, JsonContract contract)
		{
			if (contract == null)
			{
				throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					objectType
				}));
			}
			JsonArrayContract jsonArrayContract = contract as JsonArrayContract;
			if (jsonArrayContract == null)
			{
				throw new JsonSerializationException("Cannot deserialize JSON array into type '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					objectType
				}));
			}
			return jsonArrayContract;
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00017A25 File Offset: 0x00015C25
		private void CheckedRead(JsonReader reader)
		{
			if (!reader.Read())
			{
				throw new JsonSerializationException("Unexpected end when deserializing object.");
			}
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00017A3C File Offset: 0x00015C3C
		private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string reference)
		{
			object result;
			if (this.HasDefinedType(objectType))
			{
				JsonArrayContract jsonArrayContract = this.EnsureArrayContract(objectType, contract);
				if (existingValue == null)
				{
					result = this.CreateAndPopulateList(reader, reference, jsonArrayContract);
				}
				else
				{
					result = this.PopulateList(jsonArrayContract.CreateWrapper(existingValue), reader, reference, jsonArrayContract);
				}
			}
			else
			{
				result = this.CreateJToken(reader, contract);
			}
			return result;
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x00017A8C File Offset: 0x00015C8C
		private bool HasDefinedType(Type type)
		{
			return type != null && type != typeof(object) && !typeof(JToken).IsAssignableFrom(type) && type != typeof(IDynamicMetaObjectProvider);
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x00017AD8 File Offset: 0x00015CD8
		private object EnsureType(object value, CultureInfo culture, Type targetType)
		{
			if (targetType == null)
			{
				return value;
			}
			Type objectType = ReflectionUtils.GetObjectType(value);
			if (objectType != targetType)
			{
				try
				{
					return ConvertUtils.ConvertOrCast(value, culture, targetType);
				}
				catch (Exception innerException)
				{
					throw new JsonSerializationException("Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						this.FormatValueForPrint(value),
						targetType
					}), innerException);
				}
				return value;
			}
			return value;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00017B4C File Offset: 0x00015D4C
		private string FormatValueForPrint(object value)
		{
			if (value == null)
			{
				return "{null}";
			}
			if (value is string)
			{
				return "\"" + value + "\"";
			}
			return value.ToString();
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x00017B78 File Offset: 0x00015D78
		private void SetPropertyValue(JsonProperty property, JsonReader reader, object target)
		{
			if (property.Ignored)
			{
				reader.Skip();
				return;
			}
			object obj = null;
			bool flag = false;
			bool gottenCurrentValue = false;
			ObjectCreationHandling valueOrDefault = property.ObjectCreationHandling.GetValueOrDefault(base.Serializer.ObjectCreationHandling);
			if ((valueOrDefault == ObjectCreationHandling.Auto || valueOrDefault == ObjectCreationHandling.Reuse) && (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject) && property.Readable)
			{
				obj = property.ValueProvider.GetValue(target);
				gottenCurrentValue = true;
				flag = (obj != null && !property.PropertyType.IsArray && !ReflectionUtils.InheritsGenericDefinition(property.PropertyType, typeof(ReadOnlyCollection<>)) && !property.PropertyType.IsValueType);
			}
			if (!property.Writable && !flag)
			{
				reader.Skip();
				return;
			}
			if (property.NullValueHandling.GetValueOrDefault(base.Serializer.NullValueHandling) == NullValueHandling.Ignore && reader.TokenType == JsonToken.Null)
			{
				reader.Skip();
				return;
			}
			if (property.DefaultValueHandling.GetValueOrDefault(base.Serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && JsonReader.IsPrimitiveToken(reader.TokenType) && object.Equals(reader.Value, property.DefaultValue))
			{
				reader.Skip();
				return;
			}
			object currentValue = flag ? obj : null;
			object obj2 = this.CreateValueProperty(reader, property, target, gottenCurrentValue, currentValue);
			if ((!flag || obj2 != obj) && this.ShouldSetPropertyValue(property, obj2))
			{
				property.ValueProvider.SetValue(target, obj2);
				if (property.SetIsSpecified != null)
				{
					property.SetIsSpecified(target, true);
				}
			}
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x00017CF4 File Offset: 0x00015EF4
		private bool ShouldSetPropertyValue(JsonProperty property, object value)
		{
			return (property.NullValueHandling.GetValueOrDefault(base.Serializer.NullValueHandling) != NullValueHandling.Ignore || value != null) && (property.DefaultValueHandling.GetValueOrDefault(base.Serializer.DefaultValueHandling) != DefaultValueHandling.Ignore || !object.Equals(value, property.DefaultValue)) && property.Writable;
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x00017D5C File Offset: 0x00015F5C
		private object CreateAndPopulateDictionary(JsonReader reader, JsonDictionaryContract contract, string id)
		{
			if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || base.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
			{
				object dictionary = contract.DefaultCreator();
				IWrappedDictionary wrappedDictionary = contract.CreateWrapper(dictionary);
				this.PopulateDictionary(wrappedDictionary, reader, contract, id);
				return wrappedDictionary.UnderlyingDictionary;
			}
			throw new JsonSerializationException("Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				contract.UnderlyingType
			}));
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x00017DD4 File Offset: 0x00015FD4
		private object PopulateDictionary(IWrappedDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, string id)
		{
			if (id != null)
			{
				base.Serializer.ReferenceResolver.AddReference(id, dictionary.UnderlyingDictionary);
			}
			contract.InvokeOnDeserializing(dictionary.UnderlyingDictionary, base.Serializer.Context);
			int depth = reader.Depth;
			JsonToken tokenType;
			for (;;)
			{
				tokenType = reader.TokenType;
				if (tokenType != JsonToken.PropertyName)
				{
					break;
				}
				object obj;
				try
				{
					obj = this.EnsureType(reader.Value, CultureInfo.InvariantCulture, contract.DictionaryKeyType);
				}
				catch (Exception innerException)
				{
					throw new JsonSerializationException("Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						reader.Value,
						contract.DictionaryKeyType
					}), innerException);
				}
				if (!this.ReadForType(reader, contract.DictionaryValueType, null))
				{
					goto Block_5;
				}
				try
				{
					dictionary[obj] = this.CreateValueNonProperty(reader, contract.DictionaryValueType, this.GetContractSafe(contract.DictionaryValueType));
					goto IL_135;
				}
				catch (Exception ex)
				{
					if (base.IsErrorHandled(dictionary, contract, obj, ex))
					{
						this.HandleError(reader, depth);
						goto IL_135;
					}
					throw;
				}
				goto IL_FC;
				IL_135:
				if (!reader.Read())
				{
					goto Block_7;
				}
			}
			if (tokenType != JsonToken.EndObject)
			{
				throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
			}
			goto IL_FC;
			Block_5:
			throw new JsonSerializationException("Unexpected end when deserializing object.");
			IL_FC:
			contract.InvokeOnDeserialized(dictionary.UnderlyingDictionary, base.Serializer.Context);
			return dictionary.UnderlyingDictionary;
			Block_7:
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x00018040 File Offset: 0x00016240
		private object CreateAndPopulateList(JsonReader reader, string reference, JsonArrayContract contract)
		{
			return CollectionUtils.CreateAndPopulateList(contract.CreatedType, delegate(IList l, bool isTemporaryListReference)
			{
				if (reference != null && isTemporaryListReference)
				{
					throw new JsonSerializationException("Cannot preserve reference to array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						contract.UnderlyingType
					}));
				}
				if (contract.OnSerializing != null && isTemporaryListReference)
				{
					throw new JsonSerializationException("Cannot call OnSerializing on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						contract.UnderlyingType
					}));
				}
				if (contract.OnError != null && isTemporaryListReference)
				{
					throw new JsonSerializationException("Cannot call OnError on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						contract.UnderlyingType
					}));
				}
				this.PopulateList(contract.CreateWrapper(l), reader, reference, contract);
			});
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x0001808C File Offset: 0x0001628C
		private bool ReadForTypeArrayHack(JsonReader reader, Type t)
		{
			bool result;
			try
			{
				result = this.ReadForType(reader, t, null);
			}
			catch (JsonReaderException)
			{
				if (reader.TokenType != JsonToken.EndArray)
				{
					throw;
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x000180C8 File Offset: 0x000162C8
		private object PopulateList(IWrappedCollection wrappedList, JsonReader reader, string reference, JsonArrayContract contract)
		{
			object underlyingCollection = wrappedList.UnderlyingCollection;
			if (reference != null)
			{
				base.Serializer.ReferenceResolver.AddReference(reference, underlyingCollection);
			}
			contract.InvokeOnDeserializing(underlyingCollection, base.Serializer.Context);
			int depth = reader.Depth;
			while (this.ReadForTypeArrayHack(reader, contract.CollectionItemType))
			{
				JsonToken tokenType = reader.TokenType;
				if (tokenType != JsonToken.Comment)
				{
					if (tokenType == JsonToken.EndArray)
					{
						contract.InvokeOnDeserialized(underlyingCollection, base.Serializer.Context);
						return wrappedList.UnderlyingCollection;
					}
					try
					{
						object value = this.CreateValueNonProperty(reader, contract.CollectionItemType, this.GetContractSafe(contract.CollectionItemType));
						wrappedList.Add(value);
					}
					catch (Exception ex)
					{
						if (!base.IsErrorHandled(underlyingCollection, contract, wrappedList.Count, ex))
						{
							throw;
						}
						this.HandleError(reader, depth);
					}
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing array.");
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x000181B4 File Offset: 0x000163B4
		private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, string id)
		{
			Type underlyingType = contract.UnderlyingType;
			SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, this.GetFormatterConverter());
			bool flag = false;
			string text;
			for (;;)
			{
				JsonToken tokenType = reader.TokenType;
				if (tokenType != JsonToken.PropertyName)
				{
					if (tokenType != JsonToken.EndObject)
					{
						break;
					}
					flag = true;
				}
				else
				{
					text = reader.Value.ToString();
					if (!reader.Read())
					{
						goto Block_3;
					}
					serializationInfo.AddValue(text, JToken.ReadFrom(reader));
				}
				if (flag || !reader.Read())
				{
					goto IL_A4;
				}
			}
			throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
			Block_3:
			throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text
			}));
			IL_A4:
			if (contract.ISerializableCreator == null)
			{
				throw new JsonSerializationException("ISerializable type '{0}' does not have a valid constructor.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					underlyingType
				}));
			}
			object obj = contract.ISerializableCreator(new object[]
			{
				serializationInfo,
				base.Serializer.Context
			});
			if (id != null)
			{
				base.Serializer.ReferenceResolver.AddReference(id, obj);
			}
			contract.InvokeOnDeserializing(obj, base.Serializer.Context);
			contract.InvokeOnDeserialized(obj, base.Serializer.Context);
			return obj;
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x00018300 File Offset: 0x00016500
		private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, string id)
		{
			if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
			{
				throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					contract.UnderlyingType
				}));
			}
			if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || base.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
			{
				IDynamicMetaObjectProvider dynamicMetaObjectProvider = (IDynamicMetaObjectProvider)contract.DefaultCreator();
				if (id != null)
				{
					base.Serializer.ReferenceResolver.AddReference(id, dynamicMetaObjectProvider);
				}
				contract.InvokeOnDeserializing(dynamicMetaObjectProvider, base.Serializer.Context);
				bool flag = false;
				string text;
				for (;;)
				{
					JsonToken tokenType = reader.TokenType;
					if (tokenType != JsonToken.PropertyName)
					{
						if (tokenType != JsonToken.EndObject)
						{
							break;
						}
						flag = true;
					}
					else
					{
						text = reader.Value.ToString();
						if (!reader.Read())
						{
							goto Block_7;
						}
						JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
						if (closestMatchProperty != null && closestMatchProperty.Writable && !closestMatchProperty.Ignored)
						{
							this.SetPropertyValue(closestMatchProperty, reader, dynamicMetaObjectProvider);
						}
						else
						{
							Type type = JsonReader.IsPrimitiveToken(reader.TokenType) ? reader.ValueType : typeof(IDynamicMetaObjectProvider);
							object value = this.CreateValueNonProperty(reader, type, this.GetContractSafe(type, null));
							dynamicMetaObjectProvider.TrySetMember(text, value);
						}
					}
					if (flag || !reader.Read())
					{
						goto IL_1B4;
					}
				}
				throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
				Block_7:
				throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text
				}));
				IL_1B4:
				contract.InvokeOnDeserialized(dynamicMetaObjectProvider, base.Serializer.Context);
				return dynamicMetaObjectProvider;
			}
			throw new JsonSerializationException("Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				contract.UnderlyingType
			}));
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x000184D4 File Offset: 0x000166D4
		private object CreateAndPopulateObject(JsonReader reader, JsonObjectContract contract, string id)
		{
			object obj = null;
			if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
			{
				throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					contract.UnderlyingType
				}));
			}
			if (contract.OverrideConstructor != null)
			{
				if (contract.OverrideConstructor.GetParameters().Length > 0)
				{
					return this.CreateObjectFromNonDefaultConstructor(reader, contract, contract.OverrideConstructor, id);
				}
				obj = contract.OverrideConstructor.Invoke(null);
			}
			else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || base.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
			{
				obj = contract.DefaultCreator();
			}
			else if (contract.ParametrizedConstructor != null)
			{
				return this.CreateObjectFromNonDefaultConstructor(reader, contract, contract.ParametrizedConstructor, id);
			}
			if (obj == null)
			{
				throw new JsonSerializationException("Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					contract.UnderlyingType
				}));
			}
			this.PopulateObject(obj, reader, contract, id);
			return obj;
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x00018604 File Offset: 0x00016804
		private object CreateObjectFromNonDefaultConstructor(JsonReader reader, JsonObjectContract contract, ConstructorInfo constructorInfo, string id)
		{
			ValidationUtils.ArgumentNotNull(constructorInfo, "constructorInfo");
			Type underlyingType = contract.UnderlyingType;
			IDictionary<JsonProperty, object> dictionary = (from p in contract.Properties
			where !p.Ignored
			select p).ToDictionary((JsonProperty kv) => kv, (JsonProperty kv) => null);
			bool flag = false;
			string text;
			for (;;)
			{
				JsonToken tokenType = reader.TokenType;
				if (tokenType != JsonToken.PropertyName)
				{
					if (tokenType != JsonToken.EndObject)
					{
						break;
					}
					flag = true;
				}
				else
				{
					text = reader.Value.ToString();
					JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
					if (closestMatchProperty != null)
					{
						if (!this.ReadForType(reader, closestMatchProperty.PropertyType, closestMatchProperty.Converter))
						{
							goto Block_7;
						}
						if (!closestMatchProperty.Ignored)
						{
							dictionary[closestMatchProperty] = this.CreateValueProperty(reader, closestMatchProperty, null, true, null);
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						if (!reader.Read())
						{
							goto Block_9;
						}
						if (base.Serializer.MissingMemberHandling == MissingMemberHandling.Error)
						{
							goto Block_10;
						}
						reader.Skip();
					}
				}
				if (flag || !reader.Read())
				{
					goto IL_1BA;
				}
			}
			throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
			Block_7:
			throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text
			}));
			Block_9:
			throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text
			}));
			Block_10:
			throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text,
				underlyingType.Name
			}));
			IL_1BA:
			IDictionary<ParameterInfo, object> dictionary2 = constructorInfo.GetParameters().ToDictionary((ParameterInfo p) => p, (ParameterInfo p) => null);
			IDictionary<JsonProperty, object> dictionary3 = new Dictionary<JsonProperty, object>();
			foreach (KeyValuePair<JsonProperty, object> item in dictionary)
			{
				ParameterInfo key = dictionary2.ForgivingCaseSensitiveFind((KeyValuePair<ParameterInfo, object> kv) => kv.Key.Name, item.Key.PropertyName).Key;
				if (key != null)
				{
					dictionary2[key] = item.Value;
				}
				else
				{
					dictionary3.Add(item);
				}
			}
			object obj = constructorInfo.Invoke(dictionary2.Values.ToArray<object>());
			if (id != null)
			{
				base.Serializer.ReferenceResolver.AddReference(id, obj);
			}
			contract.InvokeOnDeserializing(obj, base.Serializer.Context);
			foreach (KeyValuePair<JsonProperty, object> keyValuePair in dictionary3)
			{
				JsonProperty key2 = keyValuePair.Key;
				object value = keyValuePair.Value;
				if (this.ShouldSetPropertyValue(keyValuePair.Key, keyValuePair.Value))
				{
					key2.ValueProvider.SetValue(obj, value);
				}
			}
			contract.InvokeOnDeserialized(obj, base.Serializer.Context);
			return obj;
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x00018970 File Offset: 0x00016B70
		private bool ReadForType(JsonReader reader, Type t, JsonConverter propertyConverter)
		{
			bool flag = this.GetConverter(this.GetContractSafe(t), propertyConverter) != null;
			if (flag)
			{
				return reader.Read();
			}
			if (t == typeof(byte[]))
			{
				reader.ReadAsBytes();
				return true;
			}
			if (t == typeof(decimal) || t == typeof(decimal?))
			{
				reader.ReadAsDecimal();
				return true;
			}
			if (t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?))
			{
				reader.ReadAsDateTimeOffset();
				return true;
			}
			return reader.Read();
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00018A2C File Offset: 0x00016C2C
		private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, string id)
		{
			contract.InvokeOnDeserializing(newObject, base.Serializer.Context);
			Dictionary<JsonProperty, JsonSerializerInternalReader.RequiredValue> dictionary = (from m in contract.Properties
			where m.Required != Required.Default
			select m).ToDictionary((JsonProperty m) => m, (JsonProperty m) => JsonSerializerInternalReader.RequiredValue.None);
			if (id != null)
			{
				base.Serializer.ReferenceResolver.AddReference(id, newObject);
			}
			int depth = reader.Depth;
			JsonToken tokenType;
			string text;
			for (;;)
			{
				tokenType = reader.TokenType;
				switch (tokenType)
				{
				case JsonToken.PropertyName:
				{
					text = reader.Value.ToString();
					JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
					if (closestMatchProperty == null)
					{
						if (base.Serializer.MissingMemberHandling == MissingMemberHandling.Error)
						{
							goto Block_8;
						}
						reader.Skip();
						goto IL_278;
					}
					else
					{
						if (!this.ReadForType(reader, closestMatchProperty.PropertyType, closestMatchProperty.Converter))
						{
							goto Block_9;
						}
						this.SetRequiredProperty(reader, closestMatchProperty, dictionary);
						try
						{
							this.SetPropertyValue(closestMatchProperty, reader, newObject);
							goto IL_278;
						}
						catch (Exception ex)
						{
							if (base.IsErrorHandled(newObject, contract, text, ex))
							{
								this.HandleError(reader, depth);
								goto IL_278;
							}
							throw;
						}
						goto IL_197;
					}
					break;
				}
				case JsonToken.Comment:
					goto IL_278;
				}
				break;
				IL_278:
				if (!reader.Read())
				{
					goto Block_12;
				}
			}
			if (tokenType != JsonToken.EndObject)
			{
				throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
			}
			goto IL_197;
			Block_8:
			throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text,
				contract.UnderlyingType.Name
			}));
			Block_9:
			throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				text
			}));
			IL_197:
			foreach (KeyValuePair<JsonProperty, JsonSerializerInternalReader.RequiredValue> keyValuePair in dictionary)
			{
				if (keyValuePair.Value == JsonSerializerInternalReader.RequiredValue.None)
				{
					throw new JsonSerializationException("Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						keyValuePair.Key.PropertyName
					}));
				}
				if (keyValuePair.Key.Required == Required.Always && keyValuePair.Value == JsonSerializerInternalReader.RequiredValue.Null)
				{
					throw new JsonSerializationException("Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						keyValuePair.Key.PropertyName
					}));
				}
			}
			contract.InvokeOnDeserialized(newObject, base.Serializer.Context);
			return newObject;
			Block_12:
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x00018CE4 File Offset: 0x00016EE4
		private void SetRequiredProperty(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, JsonSerializerInternalReader.RequiredValue> requiredProperties)
		{
			if (property != null)
			{
				requiredProperties[property] = ((reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined) ? JsonSerializerInternalReader.RequiredValue.Null : JsonSerializerInternalReader.RequiredValue.Value);
			}
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x00018D08 File Offset: 0x00016F08
		private void HandleError(JsonReader reader, int initialDepth)
		{
			base.ClearErrorContext();
			reader.Skip();
			while (reader.Depth > initialDepth + 1)
			{
				reader.Read();
			}
		}

		// Token: 0x04000223 RID: 547
		private JsonSerializerProxy _internalSerializer;

		// Token: 0x04000224 RID: 548
		private JsonFormatterConverter _formatterConverter;

		// Token: 0x02000094 RID: 148
		internal enum RequiredValue
		{
			// Token: 0x0400022F RID: 559
			None,
			// Token: 0x04000230 RID: 560
			Null,
			// Token: 0x04000231 RID: 561
			Value
		}
	}
}

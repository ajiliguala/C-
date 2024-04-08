using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000095 RID: 149
	internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
	{
		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000702 RID: 1794 RVA: 0x00018D2A File Offset: 0x00016F2A
		private List<object> SerializeStack
		{
			get
			{
				if (this._serializeStack == null)
				{
					this._serializeStack = new List<object>();
				}
				return this._serializeStack;
			}
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x00018D45 File Offset: 0x00016F45
		public JsonSerializerInternalWriter(JsonSerializer serializer) : base(serializer)
		{
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x00018D4E File Offset: 0x00016F4E
		public void Serialize(JsonWriter jsonWriter, object value)
		{
			if (jsonWriter == null)
			{
				throw new ArgumentNullException("jsonWriter");
			}
			this.SerializeValue(jsonWriter, value, this.GetContractSafe(value), null, null);
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x00018D6F File Offset: 0x00016F6F
		private JsonSerializerProxy GetInternalSerializer()
		{
			if (this._internalSerializer == null)
			{
				this._internalSerializer = new JsonSerializerProxy(this);
			}
			return this._internalSerializer;
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x00018D8B File Offset: 0x00016F8B
		private JsonContract GetContractSafe(object value)
		{
			if (value == null)
			{
				return null;
			}
			return base.Serializer.ContractResolver.ResolveContract(value.GetType());
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x00018DA8 File Offset: 0x00016FA8
		private void SerializeValue(JsonWriter writer, object value, JsonContract valueContract, JsonProperty member, JsonContract collectionValueContract)
		{
			JsonConverter jsonConverter = (member != null) ? member.Converter : null;
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			if ((jsonConverter != null || (jsonConverter = valueContract.Converter) != null || (jsonConverter = base.Serializer.GetMatchingConverter(valueContract.UnderlyingType)) != null || (jsonConverter = valueContract.InternalConverter) != null) && jsonConverter.CanWrite)
			{
				this.SerializeConvertable(writer, jsonConverter, value, valueContract);
				return;
			}
			if (valueContract is JsonPrimitiveContract)
			{
				writer.WriteValue(value);
				return;
			}
			if (valueContract is JsonStringContract)
			{
				this.SerializeString(writer, value, (JsonStringContract)valueContract);
				return;
			}
			if (valueContract is JsonObjectContract)
			{
				this.SerializeObject(writer, value, (JsonObjectContract)valueContract, member, collectionValueContract);
				return;
			}
			if (valueContract is JsonDictionaryContract)
			{
				JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)valueContract;
				this.SerializeDictionary(writer, jsonDictionaryContract.CreateWrapper(value), jsonDictionaryContract, member, collectionValueContract);
				return;
			}
			if (valueContract is JsonArrayContract)
			{
				JsonArrayContract jsonArrayContract = (JsonArrayContract)valueContract;
				this.SerializeList(writer, jsonArrayContract.CreateWrapper(value), jsonArrayContract, member, collectionValueContract);
				return;
			}
			if (valueContract is JsonLinqContract)
			{
				((JToken)value).WriteTo(writer, (base.Serializer.Converters != null) ? base.Serializer.Converters.ToArray<JsonConverter>() : null);
				return;
			}
			if (valueContract is JsonISerializableContract)
			{
				this.SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract);
				return;
			}
			if (valueContract is JsonDynamicContract)
			{
				this.SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract);
			}
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x00018F04 File Offset: 0x00017104
		private bool ShouldWriteReference(object value, JsonProperty property, JsonContract contract)
		{
			if (value == null)
			{
				return false;
			}
			if (contract is JsonPrimitiveContract)
			{
				return false;
			}
			bool? flag = null;
			if (property != null)
			{
				flag = property.IsReference;
			}
			if (flag == null)
			{
				flag = contract.IsReference;
			}
			if (flag == null)
			{
				if (contract is JsonArrayContract)
				{
					flag = new bool?(this.HasFlag(base.Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays));
				}
				else
				{
					flag = new bool?(this.HasFlag(base.Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects));
				}
			}
			return flag.Value && base.Serializer.ReferenceResolver.IsReferenced(value);
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x00018FA4 File Offset: 0x000171A4
		private void WriteMemberInfoProperty(JsonWriter writer, object memberValue, JsonProperty property, JsonContract contract)
		{
			string propertyName = property.PropertyName;
			object defaultValue = property.DefaultValue;
			if (property.NullValueHandling.GetValueOrDefault(base.Serializer.NullValueHandling) == NullValueHandling.Ignore && memberValue == null)
			{
				return;
			}
			if (property.DefaultValueHandling.GetValueOrDefault(base.Serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && object.Equals(memberValue, defaultValue))
			{
				return;
			}
			if (this.ShouldWriteReference(memberValue, property, contract))
			{
				writer.WritePropertyName(propertyName);
				this.WriteReference(writer, memberValue);
				return;
			}
			if (!this.CheckForCircularReference(memberValue, property.ReferenceLoopHandling, contract))
			{
				return;
			}
			if (memberValue == null && property.Required == Required.Always)
			{
				throw new JsonSerializationException("Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					property.PropertyName
				}));
			}
			writer.WritePropertyName(propertyName);
			this.SerializeValue(writer, memberValue, contract, property, null);
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x0001907C File Offset: 0x0001727C
		private bool CheckForCircularReference(object value, ReferenceLoopHandling? referenceLoopHandling, JsonContract contract)
		{
			if (value == null || contract is JsonPrimitiveContract)
			{
				return true;
			}
			if (this.SerializeStack.IndexOf(value) == -1)
			{
				return true;
			}
			switch (referenceLoopHandling.GetValueOrDefault(base.Serializer.ReferenceLoopHandling))
			{
			case ReferenceLoopHandling.Error:
				throw new JsonSerializationException("Self referencing loop detected for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					value.GetType()
				}));
			case ReferenceLoopHandling.Ignore:
				return false;
			case ReferenceLoopHandling.Serialize:
				return true;
			default:
				throw new InvalidOperationException("Unexpected ReferenceLoopHandling value: '{0}'".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.Serializer.ReferenceLoopHandling
				}));
			}
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x0001912A File Offset: 0x0001732A
		private void WriteReference(JsonWriter writer, object value)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("$ref");
			writer.WriteValue(base.Serializer.ReferenceResolver.GetReference(value));
			writer.WriteEndObject();
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x0001915C File Offset: 0x0001735C
		internal static bool TryConvertToString(object value, Type type, out string s)
		{
			TypeConverter converter = ConvertUtils.GetConverter(type);
			if (converter != null && !(converter is ComponentConverter) && converter.GetType() != typeof(TypeConverter) && converter.CanConvertTo(typeof(string)))
			{
				s = converter.ConvertToInvariantString(value);
				return true;
			}
			if (value is Type)
			{
				s = ((Type)value).AssemblyQualifiedName;
				return true;
			}
			s = null;
			return false;
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x000191CC File Offset: 0x000173CC
		private void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
		{
			contract.InvokeOnSerializing(value, base.Serializer.Context);
			string value2;
			JsonSerializerInternalWriter.TryConvertToString(value, contract.UnderlyingType, out value2);
			writer.WriteValue(value2);
			contract.InvokeOnSerialized(value, base.Serializer.Context);
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x00019214 File Offset: 0x00017414
		private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty member, JsonContract collectionValueContract)
		{
			contract.InvokeOnSerializing(value, base.Serializer.Context);
			this.SerializeStack.Add(value);
			writer.WriteStartObject();
			bool flag = contract.IsReference ?? this.HasFlag(base.Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
			if (flag)
			{
				writer.WritePropertyName("$id");
				writer.WriteValue(base.Serializer.ReferenceResolver.GetReference(value));
			}
			if (this.ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionValueContract))
			{
				this.WriteTypeProperty(writer, contract.UnderlyingType);
			}
			int top = writer.Top;
			foreach (JsonProperty jsonProperty in contract.Properties)
			{
				try
				{
					if (!jsonProperty.Ignored && jsonProperty.Readable && this.ShouldSerialize(jsonProperty, value) && this.IsSpecified(jsonProperty, value))
					{
						object value2 = jsonProperty.ValueProvider.GetValue(value);
						JsonContract contractSafe = this.GetContractSafe(value2);
						this.WriteMemberInfoProperty(writer, value2, jsonProperty, contractSafe);
					}
				}
				catch (Exception ex)
				{
					if (!base.IsErrorHandled(value, contract, jsonProperty.PropertyName, ex))
					{
						throw;
					}
					this.HandleError(writer, top);
				}
			}
			writer.WriteEndObject();
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
			contract.InvokeOnSerialized(value, base.Serializer.Context);
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0001939C File Offset: 0x0001759C
		private void WriteTypeProperty(JsonWriter writer, Type type)
		{
			writer.WritePropertyName("$type");
			writer.WriteValue(ReflectionUtils.GetTypeName(type, base.Serializer.TypeNameAssemblyFormat));
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x000193C0 File Offset: 0x000175C0
		private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
		{
			return (value & flag) == flag;
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000193C8 File Offset: 0x000175C8
		private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
		{
			return (value & flag) == flag;
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x000193D0 File Offset: 0x000175D0
		private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract)
		{
			if (this.ShouldWriteReference(value, null, contract))
			{
				this.WriteReference(writer, value);
				return;
			}
			if (!this.CheckForCircularReference(value, null, contract))
			{
				return;
			}
			this.SerializeStack.Add(value);
			converter.WriteJson(writer, value, this.GetInternalSerializer());
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0001943C File Offset: 0x0001763C
		private void SerializeList(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContract collectionValueContract)
		{
			contract.InvokeOnSerializing(values.UnderlyingCollection, base.Serializer.Context);
			this.SerializeStack.Add(values.UnderlyingCollection);
			bool flag = contract.IsReference ?? this.HasFlag(base.Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
			bool flag2 = this.ShouldWriteType(TypeNameHandling.Arrays, contract, member, collectionValueContract);
			if (flag || flag2)
			{
				writer.WriteStartObject();
				if (flag)
				{
					writer.WritePropertyName("$id");
					writer.WriteValue(base.Serializer.ReferenceResolver.GetReference(values.UnderlyingCollection));
				}
				if (flag2)
				{
					this.WriteTypeProperty(writer, values.UnderlyingCollection.GetType());
				}
				writer.WritePropertyName("$values");
			}
			JsonContract collectionValueContract2 = base.Serializer.ContractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));
			writer.WriteStartArray();
			int top = writer.Top;
			int num = 0;
			foreach (object value in values)
			{
				try
				{
					JsonContract contractSafe = this.GetContractSafe(value);
					if (this.ShouldWriteReference(value, null, contractSafe))
					{
						this.WriteReference(writer, value);
					}
					else if (this.CheckForCircularReference(value, null, contract))
					{
						this.SerializeValue(writer, value, contractSafe, null, collectionValueContract2);
					}
				}
				catch (Exception ex)
				{
					if (!base.IsErrorHandled(values.UnderlyingCollection, contract, num, ex))
					{
						throw;
					}
					this.HandleError(writer, top);
				}
				finally
				{
					num++;
				}
			}
			writer.WriteEndArray();
			if (flag || flag2)
			{
				writer.WriteEndObject();
			}
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
			contract.InvokeOnSerialized(values.UnderlyingCollection, base.Serializer.Context);
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00019650 File Offset: 0x00017850
		[SecuritySafeCritical]
		private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract)
		{
			contract.InvokeOnSerializing(value, base.Serializer.Context);
			this.SerializeStack.Add(value);
			writer.WriteStartObject();
			SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
			value.GetObjectData(serializationInfo, base.Serializer.Context);
			foreach (SerializationEntry serializationEntry in serializationInfo)
			{
				writer.WritePropertyName(serializationEntry.Name);
				this.SerializeValue(writer, serializationEntry.Value, this.GetContractSafe(serializationEntry.Value), null, null);
			}
			writer.WriteEndObject();
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
			contract.InvokeOnSerialized(value, base.Serializer.Context);
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x00019718 File Offset: 0x00017918
		private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract)
		{
			contract.InvokeOnSerializing(value, base.Serializer.Context);
			this.SerializeStack.Add(value);
			writer.WriteStartObject();
			foreach (string name in value.GetDynamicMemberNames())
			{
				object value2;
				if (value.TryGetMember(name, out value2))
				{
					writer.WritePropertyName(name);
					this.SerializeValue(writer, value2, this.GetContractSafe(value2), null, null);
				}
			}
			writer.WriteEndObject();
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
			contract.InvokeOnSerialized(value, base.Serializer.Context);
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x000197D8 File Offset: 0x000179D8
		private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty member, JsonContract collectionValueContract)
		{
			if (this.HasFlag(((member != null) ? member.TypeNameHandling : null) ?? base.Serializer.TypeNameHandling, typeNameHandlingFlag))
			{
				return true;
			}
			if (member != null)
			{
				if ((member.TypeNameHandling ?? base.Serializer.TypeNameHandling) == TypeNameHandling.Auto && contract.UnderlyingType != member.PropertyType)
				{
					return true;
				}
			}
			else if (collectionValueContract != null && base.Serializer.TypeNameHandling == TypeNameHandling.Auto && contract.UnderlyingType != collectionValueContract.UnderlyingType)
			{
				return true;
			}
			return false;
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x0001988C File Offset: 0x00017A8C
		private void SerializeDictionary(JsonWriter writer, IWrappedDictionary values, JsonDictionaryContract contract, JsonProperty member, JsonContract collectionValueContract)
		{
			contract.InvokeOnSerializing(values.UnderlyingDictionary, base.Serializer.Context);
			this.SerializeStack.Add(values.UnderlyingDictionary);
			writer.WriteStartObject();
			bool flag = contract.IsReference ?? this.HasFlag(base.Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
			if (flag)
			{
				writer.WritePropertyName("$id");
				writer.WriteValue(base.Serializer.ReferenceResolver.GetReference(values.UnderlyingDictionary));
			}
			if (this.ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionValueContract))
			{
				this.WriteTypeProperty(writer, values.UnderlyingDictionary.GetType());
			}
			JsonContract collectionValueContract2 = base.Serializer.ContractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));
			int top = writer.Top;
			foreach (object obj in values)
			{
				DictionaryEntry entry = (DictionaryEntry)obj;
				string propertyName = this.GetPropertyName(entry);
				try
				{
					object value = entry.Value;
					JsonContract contractSafe = this.GetContractSafe(value);
					if (this.ShouldWriteReference(value, null, contractSafe))
					{
						writer.WritePropertyName(propertyName);
						this.WriteReference(writer, value);
					}
					else if (this.CheckForCircularReference(value, null, contract))
					{
						writer.WritePropertyName(propertyName);
						this.SerializeValue(writer, value, contractSafe, null, collectionValueContract2);
					}
				}
				catch (Exception ex)
				{
					if (!base.IsErrorHandled(values.UnderlyingDictionary, contract, propertyName, ex))
					{
						throw;
					}
					this.HandleError(writer, top);
				}
			}
			writer.WriteEndObject();
			this.SerializeStack.RemoveAt(this.SerializeStack.Count - 1);
			contract.InvokeOnSerialized(values.UnderlyingDictionary, base.Serializer.Context);
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x00019A8C File Offset: 0x00017C8C
		private string GetPropertyName(DictionaryEntry entry)
		{
			if (entry.Key is IConvertible)
			{
				return Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
			}
			string result;
			if (JsonSerializerInternalWriter.TryConvertToString(entry.Key, entry.Key.GetType(), out result))
			{
				return result;
			}
			return entry.Key.ToString();
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x00019AE3 File Offset: 0x00017CE3
		private void HandleError(JsonWriter writer, int initialDepth)
		{
			base.ClearErrorContext();
			while (writer.Top > initialDepth)
			{
				writer.WriteEnd();
			}
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00019AFC File Offset: 0x00017CFC
		private bool ShouldSerialize(JsonProperty property, object target)
		{
			return property.ShouldSerialize == null || property.ShouldSerialize(target);
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00019B14 File Offset: 0x00017D14
		private bool IsSpecified(JsonProperty property, object target)
		{
			return property.GetIsSpecified == null || property.GetIsSpecified(target);
		}

		// Token: 0x04000232 RID: 562
		private JsonSerializerProxy _internalSerializer;

		// Token: 0x04000233 RID: 563
		private List<object> _serializeStack;
	}
}

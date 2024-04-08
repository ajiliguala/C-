using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200007E RID: 126
	public class DefaultContractResolver : IContractResolver
	{
		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060005D9 RID: 1497 RVA: 0x00013ACE File Offset: 0x00011CCE
		public bool DynamicCodeGeneration
		{
			get
			{
				return JsonTypeReflector.DynamicCodeGeneration;
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x00013AD5 File Offset: 0x00011CD5
		// (set) Token: 0x060005DB RID: 1499 RVA: 0x00013ADD File Offset: 0x00011CDD
		public BindingFlags DefaultMembersSearchFlags { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x00013AE6 File Offset: 0x00011CE6
		// (set) Token: 0x060005DD RID: 1501 RVA: 0x00013AEE File Offset: 0x00011CEE
		public bool SerializeCompilerGeneratedMembers { get; set; }

		// Token: 0x060005DE RID: 1502 RVA: 0x00013AF7 File Offset: 0x00011CF7
		public DefaultContractResolver() : this(false)
		{
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x00013B00 File Offset: 0x00011D00
		public DefaultContractResolver(bool shareCache)
		{
			this.DefaultMembersSearchFlags = (BindingFlags.Instance | BindingFlags.Public);
			this._sharedCache = shareCache;
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x00013B17 File Offset: 0x00011D17
		private Dictionary<ResolverContractKey, JsonContract> GetCache()
		{
			if (this._sharedCache)
			{
				return DefaultContractResolver._sharedContractCache;
			}
			return this._instanceContractCache;
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x00013B2D File Offset: 0x00011D2D
		private void UpdateCache(Dictionary<ResolverContractKey, JsonContract> cache)
		{
			if (this._sharedCache)
			{
				DefaultContractResolver._sharedContractCache = cache;
				return;
			}
			this._instanceContractCache = cache;
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x00013B48 File Offset: 0x00011D48
		public virtual JsonContract ResolveContract(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			ResolverContractKey key = new ResolverContractKey(base.GetType(), type);
			Dictionary<ResolverContractKey, JsonContract> cache = this.GetCache();
			JsonContract jsonContract;
			if (cache == null || !cache.TryGetValue(key, out jsonContract))
			{
				jsonContract = this.CreateContract(type);
				lock (DefaultContractResolver._typeContractCacheLock)
				{
					cache = this.GetCache();
					Dictionary<ResolverContractKey, JsonContract> dictionary = (cache != null) ? new Dictionary<ResolverContractKey, JsonContract>(cache) : new Dictionary<ResolverContractKey, JsonContract>();
					dictionary[key] = jsonContract;
					this.UpdateCache(dictionary);
				}
			}
			return jsonContract;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x00013C04 File Offset: 0x00011E04
		protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
			List<MemberInfo> list = (from m in ReflectionUtils.GetFieldsAndProperties(objectType, this.DefaultMembersSearchFlags)
			where !ReflectionUtils.IsIndexedProperty(m)
			select m).ToList<MemberInfo>();
			List<MemberInfo> list2 = (from m in ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where !ReflectionUtils.IsIndexedProperty(m)
			select m).ToList<MemberInfo>();
			List<MemberInfo> list3 = new List<MemberInfo>();
			foreach (MemberInfo memberInfo in list2)
			{
				if (this.SerializeCompilerGeneratedMembers || !memberInfo.IsDefined(typeof(CompilerGeneratedAttribute), true))
				{
					if (list.Contains(memberInfo))
					{
						list3.Add(memberInfo);
					}
					else if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(memberInfo) != null)
					{
						list3.Add(memberInfo);
					}
					else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(memberInfo) != null)
					{
						list3.Add(memberInfo);
					}
				}
			}
			Type type;
			if (objectType.AssignableToTypeName("System.Data.Objects.DataClasses.EntityObject", out type))
			{
				list3 = list3.Where(new Func<MemberInfo, bool>(this.ShouldSerializeEntityMember)).ToList<MemberInfo>();
			}
			return list3;
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00013D40 File Offset: 0x00011F40
		private bool ShouldSerializeEntityMember(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			return !(propertyInfo != null) || !propertyInfo.PropertyType.IsGenericType || !(propertyInfo.PropertyType.GetGenericTypeDefinition().FullName == "System.Data.Objects.DataClasses.EntityReference`1");
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00013D9C File Offset: 0x00011F9C
		protected virtual JsonObjectContract CreateObjectContract(Type objectType)
		{
			JsonObjectContract jsonObjectContract = new JsonObjectContract(objectType);
			this.InitializeContract(jsonObjectContract);
			jsonObjectContract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType);
			jsonObjectContract.Properties.AddRange(this.CreateProperties(jsonObjectContract.UnderlyingType, jsonObjectContract.MemberSerialization));
			if (objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any((ConstructorInfo c) => c.IsDefined(typeof(JsonConstructorAttribute), true)))
			{
				jsonObjectContract.OverrideConstructor = this.GetAttributeConstructor(objectType);
			}
			else if (jsonObjectContract.DefaultCreator == null || jsonObjectContract.DefaultCreatorNonPublic)
			{
				jsonObjectContract.ParametrizedConstructor = this.GetParametrizedConstructor(objectType);
			}
			return jsonObjectContract;
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x00013E4C File Offset: 0x0001204C
		private ConstructorInfo GetAttributeConstructor(Type objectType)
		{
			IList<ConstructorInfo> list = (from c in objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where c.IsDefined(typeof(JsonConstructorAttribute), true)
			select c).ToList<ConstructorInfo>();
			if (list.Count > 1)
			{
				throw new Exception("Multiple constructors with the JsonConstructorAttribute.");
			}
			if (list.Count == 1)
			{
				return list[0];
			}
			return null;
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00013EB0 File Offset: 0x000120B0
		private ConstructorInfo GetParametrizedConstructor(Type objectType)
		{
			IList<ConstructorInfo> constructors = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if (constructors.Count == 1)
			{
				return constructors[0];
			}
			return null;
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x00013ED8 File Offset: 0x000120D8
		protected virtual JsonConverter ResolveContractConverter(Type objectType)
		{
			return JsonTypeReflector.GetJsonConverter(objectType, objectType);
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x00013EE1 File Offset: 0x000120E1
		private Func<object> GetDefaultCreator(Type createdType)
		{
			return JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00013EF0 File Offset: 0x000120F0
		private void InitializeContract(JsonContract contract)
		{
			JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetJsonContainerAttribute(contract.UnderlyingType);
			if (jsonContainerAttribute != null)
			{
				contract.IsReference = jsonContainerAttribute._isReference;
			}
			else
			{
				DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.UnderlyingType);
				if (dataContractAttribute != null && dataContractAttribute.IsReference)
				{
					contract.IsReference = new bool?(true);
				}
			}
			contract.Converter = this.ResolveContractConverter(contract.UnderlyingType);
			contract.InternalConverter = JsonSerializer.GetMatchingConverter(DefaultContractResolver.BuiltInConverters, contract.UnderlyingType);
			if (ReflectionUtils.HasDefaultConstructor(contract.CreatedType, true) || contract.CreatedType.IsValueType)
			{
				contract.DefaultCreator = this.GetDefaultCreator(contract.CreatedType);
				contract.DefaultCreatorNonPublic = (!contract.CreatedType.IsValueType && ReflectionUtils.GetDefaultConstructor(contract.CreatedType) == null);
			}
			foreach (MethodInfo methodInfo in contract.UnderlyingType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (!methodInfo.ContainsGenericParameters)
				{
					Type type = null;
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (DefaultContractResolver.IsValidCallback(methodInfo, parameters, typeof(OnSerializingAttribute), contract.OnSerializing, ref type))
					{
						contract.OnSerializing = methodInfo;
					}
					if (DefaultContractResolver.IsValidCallback(methodInfo, parameters, typeof(OnSerializedAttribute), contract.OnSerialized, ref type))
					{
						contract.OnSerialized = methodInfo;
					}
					if (DefaultContractResolver.IsValidCallback(methodInfo, parameters, typeof(OnDeserializingAttribute), contract.OnDeserializing, ref type))
					{
						contract.OnDeserializing = methodInfo;
					}
					if (DefaultContractResolver.IsValidCallback(methodInfo, parameters, typeof(OnDeserializedAttribute), contract.OnDeserialized, ref type))
					{
						contract.OnDeserialized = methodInfo;
					}
					if (DefaultContractResolver.IsValidCallback(methodInfo, parameters, typeof(OnErrorAttribute), contract.OnError, ref type))
					{
						contract.OnError = methodInfo;
					}
				}
			}
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x000140AC File Offset: 0x000122AC
		protected virtual JsonDictionaryContract CreateDictionaryContract(Type objectType)
		{
			JsonDictionaryContract jsonDictionaryContract = new JsonDictionaryContract(objectType);
			this.InitializeContract(jsonDictionaryContract);
			return jsonDictionaryContract;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x000140C8 File Offset: 0x000122C8
		protected virtual JsonArrayContract CreateArrayContract(Type objectType)
		{
			JsonArrayContract jsonArrayContract = new JsonArrayContract(objectType);
			this.InitializeContract(jsonArrayContract);
			return jsonArrayContract;
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x000140E4 File Offset: 0x000122E4
		protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type objectType)
		{
			JsonPrimitiveContract jsonPrimitiveContract = new JsonPrimitiveContract(objectType);
			this.InitializeContract(jsonPrimitiveContract);
			return jsonPrimitiveContract;
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00014100 File Offset: 0x00012300
		protected virtual JsonLinqContract CreateLinqContract(Type objectType)
		{
			JsonLinqContract jsonLinqContract = new JsonLinqContract(objectType);
			this.InitializeContract(jsonLinqContract);
			return jsonLinqContract;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x00014134 File Offset: 0x00012334
		protected virtual JsonISerializableContract CreateISerializableContract(Type objectType)
		{
			JsonISerializableContract jsonISerializableContract = new JsonISerializableContract(objectType);
			this.InitializeContract(jsonISerializableContract);
			ConstructorInfo constructor = objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
			{
				typeof(SerializationInfo),
				typeof(StreamingContext)
			}, null);
			if (constructor != null)
			{
				MethodCall<object, object> methodCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructor);
				jsonISerializableContract.ISerializableCreator = ((object[] args) => methodCall(null, args));
			}
			return jsonISerializableContract;
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x000141B0 File Offset: 0x000123B0
		protected virtual JsonDynamicContract CreateDynamicContract(Type objectType)
		{
			JsonDynamicContract jsonDynamicContract = new JsonDynamicContract(objectType);
			this.InitializeContract(jsonDynamicContract);
			jsonDynamicContract.Properties.AddRange(this.CreateProperties(objectType, MemberSerialization.OptOut));
			return jsonDynamicContract;
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x000141E0 File Offset: 0x000123E0
		protected virtual JsonStringContract CreateStringContract(Type objectType)
		{
			JsonStringContract jsonStringContract = new JsonStringContract(objectType);
			this.InitializeContract(jsonStringContract);
			return jsonStringContract;
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x000141FC File Offset: 0x000123FC
		protected virtual JsonContract CreateContract(Type objectType)
		{
			Type type = ReflectionUtils.EnsureNotNullableType(objectType);
			if (JsonConvert.IsJsonPrimitiveType(type))
			{
				return this.CreatePrimitiveContract(type);
			}
			if (JsonTypeReflector.GetJsonObjectAttribute(type) != null)
			{
				return this.CreateObjectContract(type);
			}
			if (JsonTypeReflector.GetJsonArrayAttribute(type) != null)
			{
				return this.CreateArrayContract(type);
			}
			if (type == typeof(JToken) || type.IsSubclassOf(typeof(JToken)))
			{
				return this.CreateLinqContract(type);
			}
			if (CollectionUtils.IsDictionaryType(type))
			{
				return this.CreateDictionaryContract(type);
			}
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				return this.CreateArrayContract(type);
			}
			if (DefaultContractResolver.CanConvertToString(type))
			{
				return this.CreateStringContract(type);
			}
			if (typeof(ISerializable).IsAssignableFrom(type))
			{
				return this.CreateISerializableContract(type);
			}
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
			{
				return this.CreateDynamicContract(type);
			}
			return this.CreateObjectContract(type);
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x000142E4 File Offset: 0x000124E4
		internal static bool CanConvertToString(Type type)
		{
			TypeConverter converter = ConvertUtils.GetConverter(type);
			return (converter != null && !(converter is ComponentConverter) && !(converter is ReferenceConverter) && converter.GetType() != typeof(TypeConverter) && converter.CanConvertTo(typeof(string))) || (type == typeof(Type) || type.IsSubclassOf(typeof(Type)));
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00014360 File Offset: 0x00012560
		private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo currentCallback, ref Type prevAttributeType)
		{
			if (!method.IsDefined(attributeType, false))
			{
				return false;
			}
			if (currentCallback != null)
			{
				throw new Exception("Invalid attribute. Both '{0}' and '{1}' in type '{2}' have '{3}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					method,
					currentCallback,
					DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
					attributeType
				}));
			}
			if (prevAttributeType != null)
			{
				throw new Exception("Invalid Callback. Method '{3}' in type '{2}' has both '{0}' and '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					prevAttributeType,
					attributeType,
					DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
					method
				}));
			}
			if (method.IsVirtual)
			{
				throw new Exception("Virtual Method '{0}' of type '{1}' cannot be marked with '{2}' attribute.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					method,
					DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
					attributeType
				}));
			}
			if (method.ReturnType != typeof(void))
			{
				throw new Exception("Serialization Callback '{1}' in type '{0}' must return void.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
					method
				}));
			}
			if (attributeType == typeof(OnErrorAttribute))
			{
				if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
				{
					throw new Exception("Serialization Error Callback '{1}' in type '{0}' must have two parameters of type '{2}' and '{3}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
						method,
						typeof(StreamingContext),
						typeof(ErrorContext)
					}));
				}
			}
			else if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
			{
				throw new Exception("Serialization Callback '{1}' in type '{0}' must have a single parameter of type '{2}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					DefaultContractResolver.GetClrTypeFullName(method.DeclaringType),
					method,
					typeof(StreamingContext)
				}));
			}
			prevAttributeType = attributeType;
			return true;
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00014588 File Offset: 0x00012788
		internal static string GetClrTypeFullName(Type type)
		{
			if (type.IsGenericTypeDefinition || !type.ContainsGenericParameters)
			{
				return type.FullName;
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[]
			{
				type.Namespace,
				type.Name
			});
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x000145D8 File Offset: 0x000127D8
		protected virtual IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			List<MemberInfo> serializableMembers = this.GetSerializableMembers(type);
			if (serializableMembers == null)
			{
				throw new JsonSerializationException("Null collection of seralizable members returned.");
			}
			JsonPropertyCollection jsonPropertyCollection = new JsonPropertyCollection(type);
			foreach (MemberInfo member in serializableMembers)
			{
				JsonProperty jsonProperty = this.CreateProperty(member, memberSerialization);
				if (jsonProperty != null)
				{
					jsonPropertyCollection.AddProperty(jsonProperty);
				}
			}
			return jsonPropertyCollection;
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00014654 File Offset: 0x00012854
		protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
		{
			if (this.DynamicCodeGeneration)
			{
				return new DynamicValueProvider(member);
			}
			return new ReflectionValueProvider(member);
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x0001466C File Offset: 0x0001286C
		protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty jsonProperty = new JsonProperty();
			jsonProperty.PropertyType = ReflectionUtils.GetMemberUnderlyingType(member);
			jsonProperty.ValueProvider = this.CreateMemberValueProvider(member);
			jsonProperty.Converter = JsonTypeReflector.GetJsonConverter(member, jsonProperty.PropertyType);
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(member.DeclaringType);
			DataMemberAttribute dataMemberAttribute;
			if (dataContractAttribute != null)
			{
				dataMemberAttribute = JsonTypeReflector.GetAttribute<DataMemberAttribute>(member);
			}
			else
			{
				dataMemberAttribute = null;
			}
			JsonPropertyAttribute attribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member);
			bool flag = JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(member) != null;
			string propertyName;
			if (attribute != null && attribute.PropertyName != null)
			{
				propertyName = attribute.PropertyName;
			}
			else if (dataMemberAttribute != null && dataMemberAttribute.Name != null)
			{
				propertyName = dataMemberAttribute.Name;
			}
			else
			{
				propertyName = member.Name;
			}
			jsonProperty.PropertyName = this.ResolvePropertyName(propertyName);
			if (attribute != null)
			{
				jsonProperty.Required = attribute.Required;
			}
			else if (dataMemberAttribute != null)
			{
				jsonProperty.Required = (dataMemberAttribute.IsRequired ? Required.AllowNull : Required.Default);
			}
			else
			{
				jsonProperty.Required = Required.Default;
			}
			jsonProperty.Ignored = (flag || (memberSerialization == MemberSerialization.OptIn && attribute == null && dataMemberAttribute == null));
			bool nonPublic = false;
			if ((this.DefaultMembersSearchFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
			{
				nonPublic = true;
			}
			if (attribute != null)
			{
				nonPublic = true;
			}
			if (dataMemberAttribute != null)
			{
				nonPublic = true;
			}
			jsonProperty.Readable = ReflectionUtils.CanReadMemberValue(member, nonPublic);
			jsonProperty.Writable = ReflectionUtils.CanSetMemberValue(member, nonPublic);
			jsonProperty.MemberConverter = JsonTypeReflector.GetJsonConverter(member, ReflectionUtils.GetMemberUnderlyingType(member));
			DefaultValueAttribute attribute2 = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(member);
			jsonProperty.DefaultValue = ((attribute2 != null) ? attribute2.Value : null);
			jsonProperty.NullValueHandling = ((attribute != null) ? attribute._nullValueHandling : null);
			jsonProperty.DefaultValueHandling = ((attribute != null) ? attribute._defaultValueHandling : null);
			jsonProperty.ReferenceLoopHandling = ((attribute != null) ? attribute._referenceLoopHandling : null);
			jsonProperty.ObjectCreationHandling = ((attribute != null) ? attribute._objectCreationHandling : null);
			jsonProperty.TypeNameHandling = ((attribute != null) ? attribute._typeNameHandling : null);
			jsonProperty.IsReference = ((attribute != null) ? attribute._isReference : null);
			jsonProperty.ShouldSerialize = this.CreateShouldSerializeTest(member);
			this.SetIsSpecifiedActions(jsonProperty, member);
			return jsonProperty;
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x000148A8 File Offset: 0x00012AA8
		private Predicate<object> CreateShouldSerializeTest(MemberInfo member)
		{
			MethodInfo method = member.DeclaringType.GetMethod("ShouldSerialize" + member.Name, new Type[0]);
			if (method == null || method.ReturnType != typeof(bool))
			{
				return null;
			}
			MethodCall<object, object> shouldSerializeCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
			return (object o) => (bool)shouldSerializeCall(o, new object[0]);
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00014938 File Offset: 0x00012B38
		private void SetIsSpecifiedActions(JsonProperty property, MemberInfo member)
		{
			MemberInfo memberInfo = member.DeclaringType.GetProperty(member.Name + "Specified");
			if (memberInfo == null)
			{
				memberInfo = member.DeclaringType.GetField(member.Name + "Specified");
			}
			if (memberInfo == null || ReflectionUtils.GetMemberUnderlyingType(memberInfo) != typeof(bool))
			{
				return;
			}
			Func<object, object> specifiedPropertyGet = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(memberInfo);
			property.GetIsSpecified = ((object o) => (bool)specifiedPropertyGet(o));
			property.SetIsSpecified = JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(memberInfo);
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x000149E1 File Offset: 0x00012BE1
		protected virtual string ResolvePropertyName(string propertyName)
		{
			return propertyName;
		}

		// Token: 0x04000188 RID: 392
		internal static readonly IContractResolver Instance = new DefaultContractResolver(true);

		// Token: 0x04000189 RID: 393
		private static readonly IList<JsonConverter> BuiltInConverters = new List<JsonConverter>
		{
			new EntityKeyMemberConverter(),
			new BinaryConverter(),
			new KeyValuePairConverter(),
			new XmlNodeConverter(),
			new DataSetConverter(),
			new DataTableConverter(),
			new BsonObjectIdConverter()
		};

		// Token: 0x0400018A RID: 394
		private static Dictionary<ResolverContractKey, JsonContract> _sharedContractCache;

		// Token: 0x0400018B RID: 395
		private static readonly object _typeContractCacheLock = new object();

		// Token: 0x0400018C RID: 396
		private Dictionary<ResolverContractKey, JsonContract> _instanceContractCache;

		// Token: 0x0400018D RID: 397
		private readonly bool _sharedCache;
	}
}

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000099 RID: 153
	internal static class JsonTypeReflector
	{
		// Token: 0x06000740 RID: 1856 RVA: 0x00019D78 File Offset: 0x00017F78
		public static JsonContainerAttribute GetJsonContainerAttribute(Type type)
		{
			return CachedAttributeGetter<JsonContainerAttribute>.GetAttribute(type);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x00019D80 File Offset: 0x00017F80
		public static JsonObjectAttribute GetJsonObjectAttribute(Type type)
		{
			return JsonTypeReflector.GetJsonContainerAttribute(type) as JsonObjectAttribute;
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x00019D8D File Offset: 0x00017F8D
		public static JsonArrayAttribute GetJsonArrayAttribute(Type type)
		{
			return JsonTypeReflector.GetJsonContainerAttribute(type) as JsonArrayAttribute;
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00019D9A File Offset: 0x00017F9A
		public static DataContractAttribute GetDataContractAttribute(Type type)
		{
			return CachedAttributeGetter<DataContractAttribute>.GetAttribute(type);
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x00019DA4 File Offset: 0x00017FA4
		public static MemberSerialization GetObjectMemberSerialization(Type objectType)
		{
			JsonObjectAttribute jsonObjectAttribute = JsonTypeReflector.GetJsonObjectAttribute(objectType);
			if (jsonObjectAttribute != null)
			{
				return jsonObjectAttribute.MemberSerialization;
			}
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
			if (dataContractAttribute != null)
			{
				return MemberSerialization.OptIn;
			}
			return MemberSerialization.OptOut;
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x00019DCF File Offset: 0x00017FCF
		private static Type GetJsonConverterType(ICustomAttributeProvider attributeProvider)
		{
			return JsonTypeReflector.JsonConverterTypeCache.Get(attributeProvider);
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00019DDC File Offset: 0x00017FDC
		private static Type GetJsonConverterTypeFromAttribute(ICustomAttributeProvider attributeProvider)
		{
			JsonConverterAttribute attribute = JsonTypeReflector.GetAttribute<JsonConverterAttribute>(attributeProvider);
			if (attribute == null)
			{
				return null;
			}
			return attribute.ConverterType;
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00019DFC File Offset: 0x00017FFC
		public static JsonConverter GetJsonConverter(ICustomAttributeProvider attributeProvider, Type targetConvertedType)
		{
			Type jsonConverterType = JsonTypeReflector.GetJsonConverterType(attributeProvider);
			if (!(jsonConverterType != null))
			{
				return null;
			}
			JsonConverter jsonConverter = JsonConverterAttribute.CreateJsonConverterInstance(jsonConverterType);
			if (!jsonConverter.CanConvert(targetConvertedType))
			{
				throw new JsonSerializationException("JsonConverter {0} on {1} is not compatible with member type {2}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					jsonConverter.GetType().Name,
					attributeProvider,
					targetConvertedType.Name
				}));
			}
			return jsonConverter;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00019E64 File Offset: 0x00018064
		public static TypeConverter GetTypeConverter(Type type)
		{
			return TypeDescriptor.GetConverter(type);
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x00019E6C File Offset: 0x0001806C
		private static Type GetAssociatedMetadataType(Type type)
		{
			return JsonTypeReflector.AssociatedMetadataTypesCache.Get(type);
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x00019E7C File Offset: 0x0001807C
		private static Type GetAssociateMetadataTypeFromAttribute(Type type)
		{
			Type metadataTypeAttributeType = JsonTypeReflector.GetMetadataTypeAttributeType();
			if (metadataTypeAttributeType == null)
			{
				return null;
			}
			object obj = type.GetCustomAttributes(metadataTypeAttributeType, true).SingleOrDefault<object>();
			if (obj == null)
			{
				return null;
			}
			IMetadataTypeAttribute metadataTypeAttribute = JsonTypeReflector.DynamicCodeGeneration ? DynamicWrapper.CreateWrapper<IMetadataTypeAttribute>(obj) : new LateBoundMetadataTypeAttribute(obj);
			return metadataTypeAttribute.MetadataClassType;
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00019ECC File Offset: 0x000180CC
		private static Type GetMetadataTypeAttributeType()
		{
			if (JsonTypeReflector._cachedMetadataTypeAttributeType == null)
			{
				Type type = Type.GetType("System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
				if (!(type != null))
				{
					return null;
				}
				JsonTypeReflector._cachedMetadataTypeAttributeType = type;
			}
			return JsonTypeReflector._cachedMetadataTypeAttributeType;
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00019F0C File Offset: 0x0001810C
		private static T GetAttribute<T>(Type type) where T : Attribute
		{
			Type associatedMetadataType = JsonTypeReflector.GetAssociatedMetadataType(type);
			if (associatedMetadataType != null)
			{
				T attribute = ReflectionUtils.GetAttribute<T>(associatedMetadataType, true);
				if (attribute != null)
				{
					return attribute;
				}
			}
			return ReflectionUtils.GetAttribute<T>(type, true);
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00019F44 File Offset: 0x00018144
		private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
		{
			Type associatedMetadataType = JsonTypeReflector.GetAssociatedMetadataType(memberInfo.DeclaringType);
			if (associatedMetadataType != null)
			{
				MemberInfo memberInfo2 = associatedMetadataType.GetMember(memberInfo.Name, memberInfo.MemberType, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).SingleOrDefault<MemberInfo>();
				if (memberInfo2 != null)
				{
					T attribute = ReflectionUtils.GetAttribute<T>(memberInfo2, true);
					if (attribute != null)
					{
						return attribute;
					}
				}
			}
			return ReflectionUtils.GetAttribute<T>(memberInfo, true);
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00019FA4 File Offset: 0x000181A4
		public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
		{
			Type type = attributeProvider as Type;
			if (type != null)
			{
				return JsonTypeReflector.GetAttribute<T>(type);
			}
			MemberInfo memberInfo = attributeProvider as MemberInfo;
			if (memberInfo != null)
			{
				return JsonTypeReflector.GetAttribute<T>(memberInfo);
			}
			return ReflectionUtils.GetAttribute<T>(attributeProvider, true);
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x0600074F RID: 1871 RVA: 0x00019FE8 File Offset: 0x000181E8
		public static bool DynamicCodeGeneration
		{
			get
			{
				if (JsonTypeReflector._dynamicCodeGeneration == null)
				{
					try
					{
						new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
						new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Demand();
						new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
						JsonTypeReflector._dynamicCodeGeneration = new bool?(true);
					}
					catch (Exception)
					{
						JsonTypeReflector._dynamicCodeGeneration = new bool?(false);
					}
				}
				return JsonTypeReflector._dynamicCodeGeneration.Value;
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000750 RID: 1872 RVA: 0x0001A058 File Offset: 0x00018258
		public static ReflectionDelegateFactory ReflectionDelegateFactory
		{
			get
			{
				if (JsonTypeReflector.DynamicCodeGeneration)
				{
					return DynamicReflectionDelegateFactory.Instance;
				}
				return LateBoundReflectionDelegateFactory.Instance;
			}
		}

		// Token: 0x04000237 RID: 567
		public const string IdPropertyName = "$id";

		// Token: 0x04000238 RID: 568
		public const string RefPropertyName = "$ref";

		// Token: 0x04000239 RID: 569
		public const string TypePropertyName = "$type";

		// Token: 0x0400023A RID: 570
		public const string ArrayValuesPropertyName = "$values";

		// Token: 0x0400023B RID: 571
		public const string ShouldSerializePrefix = "ShouldSerialize";

		// Token: 0x0400023C RID: 572
		public const string SpecifiedPostfix = "Specified";

		// Token: 0x0400023D RID: 573
		private const string MetadataTypeAttributeTypeName = "System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

		// Token: 0x0400023E RID: 574
		private static readonly ThreadSafeStore<ICustomAttributeProvider, Type> JsonConverterTypeCache = new ThreadSafeStore<ICustomAttributeProvider, Type>(new Func<ICustomAttributeProvider, Type>(JsonTypeReflector.GetJsonConverterTypeFromAttribute));

		// Token: 0x0400023F RID: 575
		private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(new Func<Type, Type>(JsonTypeReflector.GetAssociateMetadataTypeFromAttribute));

		// Token: 0x04000240 RID: 576
		private static Type _cachedMetadataTypeAttributeType;

		// Token: 0x04000241 RID: 577
		private static bool? _dynamicCodeGeneration;
	}
}

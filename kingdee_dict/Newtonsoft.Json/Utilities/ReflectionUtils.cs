using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000CB RID: 203
	internal static class ReflectionUtils
	{
		// Token: 0x060008AB RID: 2219 RVA: 0x0001F920 File Offset: 0x0001DB20
		public static Type GetObjectType(object v)
		{
			if (v == null)
			{
				return null;
			}
			return v.GetType();
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x0001F930 File Offset: 0x0001DB30
		public static string GetTypeName(Type t, FormatterAssemblyStyle assemblyFormat)
		{
			switch (assemblyFormat)
			{
			case FormatterAssemblyStyle.Simple:
				return ReflectionUtils.GetSimpleTypeName(t);
			case FormatterAssemblyStyle.Full:
				return t.AssemblyQualifiedName;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x0001F964 File Offset: 0x0001DB64
		private static string GetSimpleTypeName(Type type)
		{
			string text = type.FullName + ", " + type.Assembly.GetName().Name;
			if (!type.IsGenericType || type.IsGenericTypeDefinition)
			{
				return text;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			bool flag2 = false;
			foreach (char c in text)
			{
				char c2 = c;
				if (c2 != ',')
				{
					switch (c2)
					{
					case '[':
						flag = false;
						flag2 = false;
						stringBuilder.Append(c);
						goto IL_AC;
					case ']':
						flag = false;
						flag2 = false;
						stringBuilder.Append(c);
						goto IL_AC;
					}
					if (!flag2)
					{
						stringBuilder.Append(c);
					}
				}
				else if (!flag)
				{
					flag = true;
					stringBuilder.Append(c);
				}
				else
				{
					flag2 = true;
				}
				IL_AC:;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x0001FA34 File Offset: 0x0001DC34
		public static bool IsInstantiatableType(Type t)
		{
			ValidationUtils.ArgumentNotNull(t, "t");
			return !t.IsAbstract && !t.IsInterface && !t.IsArray && !t.IsGenericTypeDefinition && !(t == typeof(void)) && ReflectionUtils.HasDefaultConstructor(t);
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x0001FA8B File Offset: 0x0001DC8B
		public static bool HasDefaultConstructor(Type t)
		{
			return ReflectionUtils.HasDefaultConstructor(t, false);
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x0001FA94 File Offset: 0x0001DC94
		public static bool HasDefaultConstructor(Type t, bool nonPublic)
		{
			ValidationUtils.ArgumentNotNull(t, "t");
			return t.IsValueType || ReflectionUtils.GetDefaultConstructor(t, nonPublic) != null;
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x0001FAB8 File Offset: 0x0001DCB8
		public static ConstructorInfo GetDefaultConstructor(Type t)
		{
			return ReflectionUtils.GetDefaultConstructor(t, false);
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x0001FAC4 File Offset: 0x0001DCC4
		public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
		{
			BindingFlags bindingFlags = BindingFlags.Public;
			if (nonPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return t.GetConstructor(bindingFlags | BindingFlags.Instance, null, new Type[0], null);
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x0001FAED File Offset: 0x0001DCED
		public static bool IsNullable(Type t)
		{
			ValidationUtils.ArgumentNotNull(t, "t");
			return !t.IsValueType || ReflectionUtils.IsNullableType(t);
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0001FB0A File Offset: 0x0001DD0A
		public static bool IsNullableType(Type t)
		{
			ValidationUtils.ArgumentNotNull(t, "t");
			return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x0001FB36 File Offset: 0x0001DD36
		public static Type EnsureNotNullableType(Type t)
		{
			if (!ReflectionUtils.IsNullableType(t))
			{
				return t;
			}
			return Nullable.GetUnderlyingType(t);
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0001FB48 File Offset: 0x0001DD48
		public static bool IsUnitializedValue(object value)
		{
			if (value == null)
			{
				return true;
			}
			object obj = ReflectionUtils.CreateUnitializedValue(value.GetType());
			return value.Equals(obj);
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0001FB70 File Offset: 0x0001DD70
		public static object CreateUnitializedValue(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			if (type.IsGenericTypeDefinition)
			{
				throw new ArgumentException("Type {0} is a generic type definition and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}), "type");
			}
			if (type.IsClass || type.IsInterface || type == typeof(void))
			{
				return null;
			}
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			throw new ArgumentException("Type {0} cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				type
			}), "type");
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x0001FC0E File Offset: 0x0001DE0E
		public static bool IsPropertyIndexed(PropertyInfo property)
		{
			ValidationUtils.ArgumentNotNull(property, "property");
			return !CollectionUtils.IsNullOrEmpty<ParameterInfo>(property.GetIndexParameters());
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x0001FC2C File Offset: 0x0001DE2C
		public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
		{
			Type type2;
			return ReflectionUtils.ImplementsGenericDefinition(type, genericInterfaceDefinition, out type2);
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x0001FC44 File Offset: 0x0001DE44
		public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, "genericInterfaceDefinition");
			if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
			{
				throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					genericInterfaceDefinition
				}));
			}
			if (type.IsInterface && type.IsGenericType)
			{
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericInterfaceDefinition == genericTypeDefinition)
				{
					implementingType = type;
					return true;
				}
			}
			foreach (Type type2 in type.GetInterfaces())
			{
				if (type2.IsGenericType)
				{
					Type genericTypeDefinition2 = type2.GetGenericTypeDefinition();
					if (genericInterfaceDefinition == genericTypeDefinition2)
					{
						implementingType = type2;
						return true;
					}
				}
			}
			implementingType = null;
			return false;
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x0001FD08 File Offset: 0x0001DF08
		public static bool AssignableToTypeName(this Type type, string fullTypeName, out Type match)
		{
			Type type2 = type;
			while (type2 != null)
			{
				if (string.Equals(type2.FullName, fullTypeName, StringComparison.Ordinal))
				{
					match = type2;
					return true;
				}
				type2 = type2.BaseType;
			}
			foreach (Type type3 in type.GetInterfaces())
			{
				if (string.Equals(type3.Name, fullTypeName, StringComparison.Ordinal))
				{
					match = type;
					return true;
				}
			}
			match = null;
			return false;
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0001FD78 File Offset: 0x0001DF78
		public static bool AssignableToTypeName(this Type type, string fullTypeName)
		{
			Type type2;
			return type.AssignableToTypeName(fullTypeName, out type2);
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x0001FD90 File Offset: 0x0001DF90
		public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
		{
			Type type2;
			return ReflectionUtils.InheritsGenericDefinition(type, genericClassDefinition, out type2);
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x0001FDA8 File Offset: 0x0001DFA8
		public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type implementingType)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			ValidationUtils.ArgumentNotNull(genericClassDefinition, "genericClassDefinition");
			if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
			{
				throw new ArgumentNullException("'{0}' is not a generic class definition.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					genericClassDefinition
				}));
			}
			return ReflectionUtils.InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x0001FE04 File Offset: 0x0001E004
		private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type implementingType)
		{
			if (currentType.IsGenericType)
			{
				Type genericTypeDefinition = currentType.GetGenericTypeDefinition();
				if (genericClassDefinition == genericTypeDefinition)
				{
					implementingType = currentType;
					return true;
				}
			}
			if (currentType.BaseType == null)
			{
				implementingType = null;
				return false;
			}
			return ReflectionUtils.InheritsGenericDefinitionInternal(currentType.BaseType, genericClassDefinition, out implementingType);
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0001FE50 File Offset: 0x0001E050
		public static Type GetCollectionItemType(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			Type type2;
			if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IEnumerable<>), out type2))
			{
				if (type2.IsGenericTypeDefinition)
				{
					throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						type
					}));
				}
				return type2.GetGenericArguments()[0];
			}
			else
			{
				if (typeof(IEnumerable).IsAssignableFrom(type))
				{
					return null;
				}
				throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}));
			}
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x0001FEF4 File Offset: 0x0001E0F4
		public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
		{
			ValidationUtils.ArgumentNotNull(dictionaryType, "type");
			Type type;
			if (ReflectionUtils.ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<, >), out type))
			{
				if (type.IsGenericTypeDefinition)
				{
					throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						dictionaryType
					}));
				}
				Type[] genericArguments = type.GetGenericArguments();
				keyType = genericArguments[0];
				valueType = genericArguments[1];
				return;
			}
			else
			{
				if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
				{
					keyType = null;
					valueType = null;
					return;
				}
				throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					dictionaryType
				}));
			}
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x0001FF94 File Offset: 0x0001E194
		public static Type GetDictionaryValueType(Type dictionaryType)
		{
			Type type;
			Type result;
			ReflectionUtils.GetDictionaryKeyValueTypes(dictionaryType, out type, out result);
			return result;
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x0001FFAC File Offset: 0x0001E1AC
		public static Type GetDictionaryKeyType(Type dictionaryType)
		{
			Type result;
			Type type;
			ReflectionUtils.GetDictionaryKeyValueTypes(dictionaryType, out result, out type);
			return result;
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x0001FFC4 File Offset: 0x0001E1C4
		public static bool ItemsUnitializedValue<T>(IList<T> list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			Type collectionItemType = ReflectionUtils.GetCollectionItemType(list.GetType());
			if (collectionItemType.IsValueType)
			{
				object obj = ReflectionUtils.CreateUnitializedValue(collectionItemType);
				for (int i = 0; i < list.Count; i++)
				{
					T t = list[i];
					if (!t.Equals(obj))
					{
						return false;
					}
				}
			}
			else
			{
				if (!collectionItemType.IsClass)
				{
					throw new Exception("Type {0} is neither a ValueType or a Class.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						collectionItemType
					}));
				}
				for (int j = 0; j < list.Count; j++)
				{
					object obj2 = list[j];
					if (obj2 != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x0002007C File Offset: 0x0001E27C
		public static Type GetMemberUnderlyingType(MemberInfo member)
		{
			ValidationUtils.ArgumentNotNull(member, "member");
			MemberTypes memberType = member.MemberType;
			switch (memberType)
			{
			case MemberTypes.Event:
				return ((EventInfo)member).EventHandlerType;
			case MemberTypes.Constructor | MemberTypes.Event:
				break;
			case MemberTypes.Field:
				return ((FieldInfo)member).FieldType;
			default:
				if (memberType == MemberTypes.Property)
				{
					return ((PropertyInfo)member).PropertyType;
				}
				break;
			}
			throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member");
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x000200EC File Offset: 0x0001E2EC
		public static bool IsIndexedProperty(MemberInfo member)
		{
			ValidationUtils.ArgumentNotNull(member, "member");
			PropertyInfo propertyInfo = member as PropertyInfo;
			return propertyInfo != null && ReflectionUtils.IsIndexedProperty(propertyInfo);
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x0002011C File Offset: 0x0001E31C
		public static bool IsIndexedProperty(PropertyInfo property)
		{
			ValidationUtils.ArgumentNotNull(property, "property");
			return property.GetIndexParameters().Length > 0;
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00020134 File Offset: 0x0001E334
		public static object GetMemberValue(MemberInfo member, object target)
		{
			ValidationUtils.ArgumentNotNull(member, "member");
			ValidationUtils.ArgumentNotNull(target, "target");
			MemberTypes memberType = member.MemberType;
			if (memberType != MemberTypes.Field)
			{
				if (memberType == MemberTypes.Property)
				{
					try
					{
						return ((PropertyInfo)member).GetValue(target, null);
					}
					catch (TargetParameterCountException innerException)
					{
						throw new ArgumentException("MemberInfo '{0}' has index parameters".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							member.Name
						}), innerException);
					}
				}
				throw new ArgumentException("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					CultureInfo.InvariantCulture,
					member.Name
				}), "member");
			}
			return ((FieldInfo)member).GetValue(target);
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x000201F8 File Offset: 0x0001E3F8
		public static void SetMemberValue(MemberInfo member, object target, object value)
		{
			ValidationUtils.ArgumentNotNull(member, "member");
			ValidationUtils.ArgumentNotNull(target, "target");
			object value2 = value;
			if (value is decimal)
			{
				if (value.ToString().IndexOf('.') < 0)
				{
					value2 = MyConvert.ToInt32(value, null);
				}
				else
				{
					value2 = MyConvert.ConvertTo((decimal)value, member.DeclaringType);
				}
			}
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				((FieldInfo)member).SetValue(target, value2);
				return;
			}
			if (memberType != MemberTypes.Property)
			{
				throw new ArgumentException("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					member.Name
				}), "member");
			}
			((PropertyInfo)member).SetValue(target, value2, null);
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x000202B0 File Offset: 0x0001E4B0
		public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				FieldInfo fieldInfo = (FieldInfo)member;
				return nonPublic || fieldInfo.IsPublic;
			}
			if (memberType != MemberTypes.Property)
			{
				return false;
			}
			PropertyInfo propertyInfo = (PropertyInfo)member;
			return propertyInfo.CanRead && (nonPublic || propertyInfo.GetGetMethod(nonPublic) != null);
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0002030C File Offset: 0x0001E50C
		public static bool CanSetMemberValue(MemberInfo member, bool nonPublic)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				FieldInfo fieldInfo = (FieldInfo)member;
				return !fieldInfo.IsInitOnly && (nonPublic || fieldInfo.IsPublic);
			}
			if (memberType != MemberTypes.Property)
			{
				return false;
			}
			PropertyInfo propertyInfo = (PropertyInfo)member;
			return propertyInfo.CanWrite && (nonPublic || propertyInfo.GetSetMethod(nonPublic) != null);
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x00020372 File Offset: 0x0001E572
		public static List<MemberInfo> GetFieldsAndProperties<T>(BindingFlags bindingAttr)
		{
			return ReflectionUtils.GetFieldsAndProperties(typeof(T), bindingAttr);
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x000204E4 File Offset: 0x0001E6E4
		public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			list.AddRange(ReflectionUtils.GetFields(type, bindingAttr));
			list.AddRange(ReflectionUtils.GetProperties(type, bindingAttr));
			List<MemberInfo> list2 = new List<MemberInfo>(list.Count);
			var enumerable = from m in list
			group m by m.Name into g
			select new
			{
				Count = g.Count<MemberInfo>(),
				Members = g.Cast<MemberInfo>()
			};
			foreach (var <>f__AnonymousType in enumerable)
			{
				if (<>f__AnonymousType.Count == 1)
				{
					list2.Add(<>f__AnonymousType.Members.First<MemberInfo>());
				}
				else
				{
					IEnumerable<MemberInfo> collection = from m in <>f__AnonymousType.Members
					where !ReflectionUtils.IsOverridenGenericMember(m, bindingAttr) || m.Name == "Item"
					select m;
					list2.AddRange(collection);
				}
			}
			return list2;
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00020604 File Offset: 0x0001E804
		private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
		{
			if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
			{
				throw new ArgumentException("Member must be a field or property.");
			}
			Type declaringType = memberInfo.DeclaringType;
			if (!declaringType.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
			if (genericTypeDefinition == null)
			{
				return false;
			}
			MemberInfo[] member = genericTypeDefinition.GetMember(memberInfo.Name, bindingAttr);
			if (member.Length == 0)
			{
				return false;
			}
			Type memberUnderlyingType = ReflectionUtils.GetMemberUnderlyingType(member[0]);
			return memberUnderlyingType.IsGenericParameter;
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x0002067B File Offset: 0x0001E87B
		public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
		{
			return ReflectionUtils.GetAttribute<T>(attributeProvider, true);
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x00020684 File Offset: 0x0001E884
		public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
		{
			T[] attributes = ReflectionUtils.GetAttributes<T>(attributeProvider, inherit);
			return CollectionUtils.GetSingleItem<T>(attributes, true);
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x000206A0 File Offset: 0x0001E8A0
		public static T[] GetAttributes<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
		{
			ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");
			if (attributeProvider is Assembly)
			{
				return (T[])Attribute.GetCustomAttributes((Assembly)attributeProvider, typeof(T), inherit);
			}
			if (attributeProvider is MemberInfo)
			{
				return (T[])Attribute.GetCustomAttributes((MemberInfo)attributeProvider, typeof(T), inherit);
			}
			if (attributeProvider is Module)
			{
				return (T[])Attribute.GetCustomAttributes((Module)attributeProvider, typeof(T), inherit);
			}
			if (attributeProvider is ParameterInfo)
			{
				return (T[])Attribute.GetCustomAttributes((ParameterInfo)attributeProvider, typeof(T), inherit);
			}
			return (T[])attributeProvider.GetCustomAttributes(typeof(T), inherit);
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x0002075E File Offset: 0x0001E95E
		public static string GetNameAndAssessmblyName(Type t)
		{
			ValidationUtils.ArgumentNotNull(t, "t");
			return t.FullName + ", " + t.Assembly.GetName().Name;
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0002078C File Offset: 0x0001E98C
		public static Type MakeGenericType(Type genericTypeDefinition, params Type[] innerTypes)
		{
			ValidationUtils.ArgumentNotNull(genericTypeDefinition, "genericTypeDefinition");
			ValidationUtils.ArgumentNotNullOrEmpty<Type>(innerTypes, "innerTypes");
			ValidationUtils.ArgumentConditionTrue(genericTypeDefinition.IsGenericTypeDefinition, "genericTypeDefinition", "Type {0} is not a generic type definition.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				genericTypeDefinition
			}));
			return genericTypeDefinition.MakeGenericType(innerTypes);
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x000207E4 File Offset: 0x0001E9E4
		public static object CreateGeneric(Type genericTypeDefinition, Type innerType, params object[] args)
		{
			return ReflectionUtils.CreateGeneric(genericTypeDefinition, new Type[]
			{
				innerType
			}, args);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00020812 File Offset: 0x0001EA12
		public static object CreateGeneric(Type genericTypeDefinition, IList<Type> innerTypes, params object[] args)
		{
			return ReflectionUtils.CreateGeneric(genericTypeDefinition, innerTypes, (Type t, IList<object> a) => ReflectionUtils.CreateInstance(t, a.ToArray<object>()), args);
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x0002083C File Offset: 0x0001EA3C
		public static object CreateGeneric(Type genericTypeDefinition, IList<Type> innerTypes, Func<Type, IList<object>, object> instanceCreator, params object[] args)
		{
			ValidationUtils.ArgumentNotNull(genericTypeDefinition, "genericTypeDefinition");
			ValidationUtils.ArgumentNotNullOrEmpty<Type>(innerTypes, "innerTypes");
			ValidationUtils.ArgumentNotNull(instanceCreator, "createInstance");
			Type arg = ReflectionUtils.MakeGenericType(genericTypeDefinition, innerTypes.ToArray<Type>());
			return instanceCreator(arg, args);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0002087F File Offset: 0x0001EA7F
		public static bool IsCompatibleValue(object value, Type type)
		{
			if (value == null)
			{
				return ReflectionUtils.IsNullable(type);
			}
			return type.IsAssignableFrom(value.GetType());
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0002089C File Offset: 0x0001EA9C
		public static object CreateInstance(Type type, params object[] args)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			return Activator.CreateInstance(type, args);
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x000208B0 File Offset: 0x0001EAB0
		public static void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName)
		{
			int? assemblyDelimiterIndex = ReflectionUtils.GetAssemblyDelimiterIndex(fullyQualifiedTypeName);
			if (assemblyDelimiterIndex != null)
			{
				typeName = fullyQualifiedTypeName.Substring(0, assemblyDelimiterIndex.Value).Trim();
				assemblyName = fullyQualifiedTypeName.Substring(assemblyDelimiterIndex.Value + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.Value - 1).Trim();
				return;
			}
			typeName = fullyQualifiedTypeName;
			assemblyName = null;
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x00020910 File Offset: 0x0001EB10
		private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
		{
			int num = 0;
			for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
			{
				char c = fullyQualifiedTypeName[i];
				char c2 = c;
				if (c2 != ',')
				{
					switch (c2)
					{
					case '[':
						num++;
						break;
					case ']':
						num--;
						break;
					}
				}
				else if (num == 0)
				{
					return new int?(i);
				}
			}
			return null;
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x00020978 File Offset: 0x0001EB78
		public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
		{
			ValidationUtils.ArgumentNotNull(targetType, "targetType");
			List<MemberInfo> list = new List<MemberInfo>(targetType.GetFields(bindingAttr));
			ReflectionUtils.GetChildPrivateFields(list, targetType, bindingAttr);
			return list.Cast<FieldInfo>();
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x000209B4 File Offset: 0x0001EBB4
		private static void GetChildPrivateFields(IList<MemberInfo> initialFields, Type targetType, BindingFlags bindingAttr)
		{
			if ((bindingAttr & BindingFlags.NonPublic) != BindingFlags.Default)
			{
				BindingFlags bindingAttr2 = bindingAttr.RemoveFlag(BindingFlags.Public);
				while ((targetType = targetType.BaseType) != null)
				{
					IEnumerable<MemberInfo> collection = (from f in targetType.GetFields(bindingAttr2)
					where f.IsPrivate
					select f).Cast<MemberInfo>();
					initialFields.AddRange(collection);
				}
			}
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00020A1C File Offset: 0x0001EC1C
		public static IEnumerable<PropertyInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
		{
			ValidationUtils.ArgumentNotNull(targetType, "targetType");
			List<PropertyInfo> list = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));
			ReflectionUtils.GetChildPrivateProperties(list, targetType, bindingAttr);
			for (int i = 0; i < list.Count; i++)
			{
				PropertyInfo propertyInfo = list[i];
				if (propertyInfo.DeclaringType != targetType)
				{
					PropertyInfo property = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, bindingAttr);
					list[i] = property;
				}
			}
			return list;
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00020A8C File Offset: 0x0001EC8C
		public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
		{
			if ((bindingAttr & flag) != flag)
			{
				return bindingAttr;
			}
			return bindingAttr ^ flag;
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x00020ABC File Offset: 0x0001ECBC
		private static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
		{
			if ((bindingAttr & BindingFlags.NonPublic) != BindingFlags.Default)
			{
				BindingFlags bindingAttr2 = bindingAttr.RemoveFlag(BindingFlags.Public);
				while ((targetType = targetType.BaseType) != null)
				{
					PropertyInfo[] properties = targetType.GetProperties(bindingAttr2);
					for (int i = 0; i < properties.Length; i++)
					{
						PropertyInfo nonPublicProperty2 = properties[i];
						PropertyInfo nonPublicProperty = nonPublicProperty2;
						int num = initialProperties.IndexOf((PropertyInfo p) => p.Name == nonPublicProperty.Name);
						if (num == -1)
						{
							initialProperties.Add(nonPublicProperty);
						}
						else
						{
							initialProperties[num] = nonPublicProperty;
						}
					}
				}
			}
		}
	}
}

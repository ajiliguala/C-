using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000C5 RID: 197
	internal static class CollectionUtils
	{
		// Token: 0x0600085D RID: 2141 RVA: 0x0001E568 File Offset: 0x0001C768
		public static IEnumerable<T> CastValid<T>(this IEnumerable enumerable)
		{
			ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
			return (from object o in enumerable
			where o is T
			select o).Cast<T>();
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x0001E591 File Offset: 0x0001C791
		public static List<T> CreateList<T>(params T[] values)
		{
			return new List<T>(values);
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x0001E599 File Offset: 0x0001C799
		public static bool IsNullOrEmpty(ICollection collection)
		{
			return collection == null || collection.Count == 0;
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x0001E5A9 File Offset: 0x0001C7A9
		public static bool IsNullOrEmpty<T>(ICollection<T> collection)
		{
			return collection == null || collection.Count == 0;
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x0001E5B9 File Offset: 0x0001C7B9
		public static bool IsNullOrEmptyOrDefault<T>(IList<T> list)
		{
			return CollectionUtils.IsNullOrEmpty<T>(list) || ReflectionUtils.ItemsUnitializedValue<T>(list);
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x0001E5CC File Offset: 0x0001C7CC
		public static IList<T> Slice<T>(IList<T> list, int? start, int? end)
		{
			return CollectionUtils.Slice<T>(list, start, end, null);
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x0001E5EC File Offset: 0x0001C7EC
		public static IList<T> Slice<T>(IList<T> list, int? start, int? end, int? step)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (step == 0)
			{
				throw new ArgumentException("Step cannot be zero.", "step");
			}
			List<T> list2 = new List<T>();
			if (list.Count == 0)
			{
				return list2;
			}
			int num = step ?? 1;
			int num2 = start ?? 0;
			int num3 = end ?? list.Count;
			num2 = ((num2 < 0) ? (list.Count + num2) : num2);
			num3 = ((num3 < 0) ? (list.Count + num3) : num3);
			num2 = Math.Max(num2, 0);
			num3 = Math.Min(num3, list.Count - 1);
			for (int i = num2; i < num3; i += num)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0001E6E0 File Offset: 0x0001C8E0
		public static Dictionary<K, List<V>> GroupBy<K, V>(ICollection<V> source, Func<V, K> keySelector)
		{
			if (keySelector == null)
			{
				throw new ArgumentNullException("keySelector");
			}
			Dictionary<K, List<V>> dictionary = new Dictionary<K, List<V>>();
			foreach (V v in source)
			{
				K key = keySelector(v);
				List<V> list;
				if (!dictionary.TryGetValue(key, out list))
				{
					list = new List<V>();
					dictionary.Add(key, list);
				}
				list.Add(v);
			}
			return dictionary;
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0001E764 File Offset: 0x0001C964
		public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
		{
			if (initial == null)
			{
				throw new ArgumentNullException("initial");
			}
			if (collection == null)
			{
				return;
			}
			foreach (T item in collection)
			{
				initial.Add(item);
			}
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x0001E7C0 File Offset: 0x0001C9C0
		public static void AddRange(this IList initial, IEnumerable collection)
		{
			ValidationUtils.ArgumentNotNull(initial, "initial");
			ListWrapper<object> initial2 = new ListWrapper<object>(initial);
			initial2.AddRange(collection.Cast<object>());
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x0001E7EC File Offset: 0x0001C9EC
		public static List<T> Distinct<T>(List<T> collection)
		{
			List<T> list = new List<T>();
			foreach (T item in collection)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0001E84C File Offset: 0x0001CA4C
		public static List<List<T>> Flatten<T>(params IList<T>[] lists)
		{
			List<List<T>> list = new List<List<T>>();
			Dictionary<int, T> currentSet = new Dictionary<int, T>();
			CollectionUtils.Recurse<T>(new List<IList<T>>(lists), 0, currentSet, list);
			return list;
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0001E874 File Offset: 0x0001CA74
		private static void Recurse<T>(IList<IList<T>> global, int current, Dictionary<int, T> currentSet, List<List<T>> flattenedResult)
		{
			IList<T> list = global[current];
			for (int i = 0; i < list.Count; i++)
			{
				currentSet[current] = list[i];
				if (current == global.Count - 1)
				{
					List<T> list2 = new List<T>();
					for (int j = 0; j < currentSet.Count; j++)
					{
						list2.Add(currentSet[j]);
					}
					flattenedResult.Add(list2);
				}
				else
				{
					CollectionUtils.Recurse<T>(global, current + 1, currentSet, flattenedResult);
				}
			}
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x0001E8EC File Offset: 0x0001CAEC
		public static List<T> CreateList<T>(ICollection collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			T[] array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return new List<T>(array);
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x0001E924 File Offset: 0x0001CB24
		public static bool ListEquals<T>(IList<T> a, IList<T> b)
		{
			if (a == null || b == null)
			{
				return a == null && b == null;
			}
			if (a.Count != b.Count)
			{
				return false;
			}
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int i = 0; i < a.Count; i++)
			{
				if (!@default.Equals(a[i], b[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x0001E981 File Offset: 0x0001CB81
		public static bool TryGetSingleItem<T>(IList<T> list, out T value)
		{
			return CollectionUtils.TryGetSingleItem<T>(list, false, out value);
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x0001E9A8 File Offset: 0x0001CBA8
		public static bool TryGetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty, out T value)
		{
			return MiscellaneousUtils.TryAction<T>(() => CollectionUtils.GetSingleItem<T>(list, returnDefaultIfEmpty), out value);
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x0001E9DB File Offset: 0x0001CBDB
		public static T GetSingleItem<T>(IList<T> list)
		{
			return CollectionUtils.GetSingleItem<T>(list, false);
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x0001E9E4 File Offset: 0x0001CBE4
		public static T GetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty)
		{
			if (list.Count == 1)
			{
				return list[0];
			}
			if (returnDefaultIfEmpty && list.Count == 0)
			{
				return default(T);
			}
			throw new Exception("Expected single {0} in list but got {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				typeof(T),
				list.Count
			}));
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x0001EA50 File Offset: 0x0001CC50
		public static IList<T> Minus<T>(IList<T> list, IList<T> minus)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			List<T> list2 = new List<T>(list.Count);
			foreach (T item in list)
			{
				if (minus == null || !minus.Contains(item))
				{
					list2.Add(item);
				}
			}
			return list2;
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x0001EABC File Offset: 0x0001CCBC
		public static IList CreateGenericList(Type listType)
		{
			ValidationUtils.ArgumentNotNull(listType, "listType");
			return (IList)ReflectionUtils.CreateGeneric(typeof(List<>), listType, new object[0]);
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x0001EAE4 File Offset: 0x0001CCE4
		public static IDictionary CreateGenericDictionary(Type keyType, Type valueType)
		{
			ValidationUtils.ArgumentNotNull(keyType, "keyType");
			ValidationUtils.ArgumentNotNull(valueType, "valueType");
			return (IDictionary)ReflectionUtils.CreateGeneric(typeof(Dictionary<, >), keyType, new object[]
			{
				valueType
			});
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x0001EB28 File Offset: 0x0001CD28
		public static bool IsListType(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			return type.IsArray || typeof(IList).IsAssignableFrom(type) || ReflectionUtils.ImplementsGenericDefinition(type, typeof(IList<>));
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x0001EB68 File Offset: 0x0001CD68
		public static bool IsCollectionType(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			return type.IsArray || typeof(ICollection).IsAssignableFrom(type) || ReflectionUtils.ImplementsGenericDefinition(type, typeof(ICollection<>));
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x0001EBA8 File Offset: 0x0001CDA8
		public static bool IsDictionaryType(Type type)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			return typeof(IDictionary).IsAssignableFrom(type) || ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<, >));
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x0001EC24 File Offset: 0x0001CE24
		public static IWrappedCollection CreateCollectionWrapper(object list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			Type collectionDefinition;
			if (ReflectionUtils.ImplementsGenericDefinition(list.GetType(), typeof(ICollection<>), out collectionDefinition))
			{
				Type collectionItemType = ReflectionUtils.GetCollectionItemType(collectionDefinition);
				Func<Type, IList<object>, object> instanceCreator = delegate(Type t, IList<object> a)
				{
					ConstructorInfo constructor = t.GetConstructor(new Type[]
					{
						collectionDefinition
					});
					return constructor.Invoke(new object[]
					{
						list
					});
				};
				return (IWrappedCollection)ReflectionUtils.CreateGeneric(typeof(CollectionWrapper<>), new Type[]
				{
					collectionItemType
				}, instanceCreator, new object[]
				{
					list
				});
			}
			if (list is IList)
			{
				return new CollectionWrapper<object>((IList)list);
			}
			throw new Exception("Can not create ListWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				list.GetType()
			}));
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x0001ED58 File Offset: 0x0001CF58
		public static IWrappedList CreateListWrapper(object list)
		{
			ValidationUtils.ArgumentNotNull(list, "list");
			Type listDefinition;
			if (ReflectionUtils.ImplementsGenericDefinition(list.GetType(), typeof(IList<>), out listDefinition))
			{
				Type collectionItemType = ReflectionUtils.GetCollectionItemType(listDefinition);
				Func<Type, IList<object>, object> instanceCreator = delegate(Type t, IList<object> a)
				{
					ConstructorInfo constructor = t.GetConstructor(new Type[]
					{
						listDefinition
					});
					return constructor.Invoke(new object[]
					{
						list
					});
				};
				return (IWrappedList)ReflectionUtils.CreateGeneric(typeof(ListWrapper<>), new Type[]
				{
					collectionItemType
				}, instanceCreator, new object[]
				{
					list
				});
			}
			if (list is IList)
			{
				return new ListWrapper<object>((IList)list);
			}
			throw new Exception("Can not create ListWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				list.GetType()
			}));
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x0001EE8C File Offset: 0x0001D08C
		public static IWrappedDictionary CreateDictionaryWrapper(object dictionary)
		{
			ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
			Type dictionaryDefinition;
			if (ReflectionUtils.ImplementsGenericDefinition(dictionary.GetType(), typeof(IDictionary<, >), out dictionaryDefinition))
			{
				Type dictionaryKeyType = ReflectionUtils.GetDictionaryKeyType(dictionaryDefinition);
				Type dictionaryValueType = ReflectionUtils.GetDictionaryValueType(dictionaryDefinition);
				Func<Type, IList<object>, object> instanceCreator = delegate(Type t, IList<object> a)
				{
					ConstructorInfo constructor = t.GetConstructor(new Type[]
					{
						dictionaryDefinition
					});
					return constructor.Invoke(new object[]
					{
						dictionary
					});
				};
				return (IWrappedDictionary)ReflectionUtils.CreateGeneric(typeof(DictionaryWrapper<, >), new Type[]
				{
					dictionaryKeyType,
					dictionaryValueType
				}, instanceCreator, new object[]
				{
					dictionary
				});
			}
			if (dictionary is IDictionary)
			{
				return new DictionaryWrapper<object, object>((IDictionary)dictionary);
			}
			throw new Exception("Can not create DictionaryWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				dictionary.GetType()
			}));
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x0001EF9C File Offset: 0x0001D19C
		public static object CreateAndPopulateList(Type listType, Action<IList, bool> populateList)
		{
			ValidationUtils.ArgumentNotNull(listType, "listType");
			ValidationUtils.ArgumentNotNull(populateList, "populateList");
			bool flag = false;
			IList list;
			Type type;
			if (listType.IsArray)
			{
				list = new List<object>();
				flag = true;
			}
			else if (ReflectionUtils.InheritsGenericDefinition(listType, typeof(ReadOnlyCollection<>), out type))
			{
				Type type2 = type.GetGenericArguments()[0];
				Type type3 = ReflectionUtils.MakeGenericType(typeof(IEnumerable<>), new Type[]
				{
					type2
				});
				bool flag2 = false;
				foreach (ConstructorInfo constructorInfo in listType.GetConstructors())
				{
					IList<ParameterInfo> parameters = constructorInfo.GetParameters();
					if (parameters.Count == 1 && type3.IsAssignableFrom(parameters[0].ParameterType))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					throw new Exception("Read-only type {0} does not have a public constructor that takes a type that implements {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						listType,
						type3
					}));
				}
				list = CollectionUtils.CreateGenericList(type2);
				flag = true;
			}
			else if (typeof(IList).IsAssignableFrom(listType))
			{
				if (ReflectionUtils.IsInstantiatableType(listType))
				{
					list = (IList)Activator.CreateInstance(listType);
				}
				else if (listType == typeof(IList))
				{
					list = new List<object>();
				}
				else
				{
					list = null;
				}
			}
			else if (ReflectionUtils.ImplementsGenericDefinition(listType, typeof(ICollection<>)))
			{
				if (ReflectionUtils.IsInstantiatableType(listType))
				{
					list = CollectionUtils.CreateCollectionWrapper(Activator.CreateInstance(listType));
				}
				else
				{
					list = null;
				}
			}
			else
			{
				list = null;
			}
			if (list == null)
			{
				throw new Exception("Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					listType
				}));
			}
			populateList(list, flag);
			if (flag)
			{
				if (listType.IsArray)
				{
					list = CollectionUtils.ToArray(((List<object>)list).ToArray(), ReflectionUtils.GetCollectionItemType(listType));
				}
				else if (ReflectionUtils.InheritsGenericDefinition(listType, typeof(ReadOnlyCollection<>)))
				{
					list = (IList)ReflectionUtils.CreateInstance(listType, new object[]
					{
						list
					});
				}
			}
			else if (list is IWrappedCollection)
			{
				return ((IWrappedCollection)list).UnderlyingCollection;
			}
			return list;
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x0001F1B0 File Offset: 0x0001D3B0
		public static Array ToArray(Array initial, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Array array = Array.CreateInstance(type, initial.Length);
			Array.Copy(initial, 0, array, 0, initial.Length);
			return array;
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x0001F1EE File Offset: 0x0001D3EE
		public static bool AddDistinct<T>(this IList<T> list, T value)
		{
			return list.AddDistinct(value, EqualityComparer<T>.Default);
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x0001F1FC File Offset: 0x0001D3FC
		public static bool AddDistinct<T>(this IList<T> list, T value, IEqualityComparer<T> comparer)
		{
			if (list.ContainsValue(value, comparer))
			{
				return false;
			}
			list.Add(value);
			return true;
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x0001F214 File Offset: 0x0001D414
		public static bool ContainsValue<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
		{
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			foreach (TSource x in source)
			{
				if (comparer.Equals(x, value))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x0001F280 File Offset: 0x0001D480
		public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values)
		{
			return list.AddRangeDistinct(values, EqualityComparer<T>.Default);
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x0001F290 File Offset: 0x0001D490
		public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer)
		{
			bool result = true;
			foreach (T value in values)
			{
				if (!list.AddDistinct(value, comparer))
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x0001F2E0 File Offset: 0x0001D4E0
		public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
		{
			int num = 0;
			foreach (T arg in collection)
			{
				if (predicate(arg))
				{
					return num;
				}
				num++;
			}
			return -1;
		}
	}
}

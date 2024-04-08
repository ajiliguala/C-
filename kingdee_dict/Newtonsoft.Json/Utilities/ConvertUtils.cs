using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000B5 RID: 181
	internal static class ConvertUtils
	{
		// Token: 0x060007DB RID: 2011 RVA: 0x0001C8D0 File Offset: 0x0001AAD0
		private static Func<object, object> CreateCastConverter(ConvertUtils.TypeConvertKey t)
		{
			MethodInfo method = t.TargetType.GetMethod("op_Implicit", new Type[]
			{
				t.InitialType
			});
			if (method == null)
			{
				method = t.TargetType.GetMethod("op_Explicit", new Type[]
				{
					t.InitialType
				});
			}
			if (method == null)
			{
				return null;
			}
			MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
			return (object o) => call(null, new object[]
			{
				o
			});
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x0001C95C File Offset: 0x0001AB5C
		public static bool CanConvertType(Type initialType, Type targetType, bool allowTypeNameToString)
		{
			ValidationUtils.ArgumentNotNull(initialType, "initialType");
			ValidationUtils.ArgumentNotNull(targetType, "targetType");
			if (ReflectionUtils.IsNullableType(targetType))
			{
				targetType = Nullable.GetUnderlyingType(targetType);
			}
			if (targetType == initialType)
			{
				return true;
			}
			if (typeof(IConvertible).IsAssignableFrom(initialType) && typeof(IConvertible).IsAssignableFrom(targetType))
			{
				return true;
			}
			if (initialType == typeof(DateTime) && targetType == typeof(DateTimeOffset))
			{
				return true;
			}
			if (initialType == typeof(Guid) && (targetType == typeof(Guid) || targetType == typeof(string)))
			{
				return true;
			}
			if (initialType == typeof(Type) && targetType == typeof(string))
			{
				return true;
			}
			TypeConverter converter = ConvertUtils.GetConverter(initialType);
			if (converter != null && !ConvertUtils.IsComponentConverter(converter) && converter.CanConvertTo(targetType) && (allowTypeNameToString || converter.GetType() != typeof(TypeConverter)))
			{
				return true;
			}
			TypeConverter converter2 = ConvertUtils.GetConverter(targetType);
			return (converter2 != null && !ConvertUtils.IsComponentConverter(converter2) && converter2.CanConvertFrom(initialType)) || (initialType == typeof(DBNull) && ReflectionUtils.IsNullable(targetType));
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x0001CAB5 File Offset: 0x0001ACB5
		private static bool IsComponentConverter(TypeConverter converter)
		{
			return converter is ComponentConverter;
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x0001CAC0 File Offset: 0x0001ACC0
		public static T Convert<T>(object initialValue)
		{
			return ConvertUtils.Convert<T>(initialValue, CultureInfo.CurrentCulture);
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x0001CACD File Offset: 0x0001ACCD
		public static T Convert<T>(object initialValue, CultureInfo culture)
		{
			return (T)((object)ConvertUtils.Convert(initialValue, culture, typeof(T)));
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x0001CAE8 File Offset: 0x0001ACE8
		public static object Convert(object initialValue, CultureInfo culture, Type targetType)
		{
			if (initialValue == null)
			{
				throw new ArgumentNullException("initialValue");
			}
			if (ReflectionUtils.IsNullableType(targetType))
			{
				targetType = Nullable.GetUnderlyingType(targetType);
			}
			Type type = initialValue.GetType();
			if (targetType == type)
			{
				return initialValue;
			}
			if (initialValue is string && typeof(Type).IsAssignableFrom(targetType))
			{
				return Type.GetType((string)initialValue, true);
			}
			if (targetType.IsInterface || targetType.IsGenericTypeDefinition || targetType.IsAbstract)
			{
				throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					targetType
				}), "targetType");
			}
			if (initialValue is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
			{
				if (targetType.IsEnum)
				{
					if (initialValue is string)
					{
						return Enum.Parse(targetType, initialValue.ToString(), true);
					}
					if (ConvertUtils.IsInteger(initialValue))
					{
						return Enum.ToObject(targetType, initialValue);
					}
				}
				if (initialValue is decimal)
				{
					return MyConvert.ConvertTo((decimal)initialValue, targetType);
				}
				return System.Convert.ChangeType(initialValue, targetType, culture);
			}
			else
			{
				if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
				{
					return new DateTimeOffset((DateTime)initialValue);
				}
				if (initialValue is string)
				{
					if (targetType == typeof(Guid))
					{
						return new Guid((string)initialValue);
					}
					if (targetType == typeof(Uri))
					{
						return new Uri((string)initialValue);
					}
					if (targetType == typeof(TimeSpan))
					{
						return TimeSpan.Parse((string)initialValue, CultureInfo.InvariantCulture);
					}
				}
				TypeConverter converter = ConvertUtils.GetConverter(type);
				if (converter != null && converter.CanConvertTo(targetType))
				{
					return converter.ConvertTo(null, culture, initialValue, targetType);
				}
				TypeConverter converter2 = ConvertUtils.GetConverter(targetType);
				if (converter2 != null && converter2.CanConvertFrom(type))
				{
					return converter2.ConvertFrom(null, culture, initialValue);
				}
				if (initialValue == DBNull.Value)
				{
					if (ReflectionUtils.IsNullable(targetType))
					{
						return ConvertUtils.EnsureTypeAssignable(null, type, targetType);
					}
					throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						type,
						targetType
					}));
				}
				else
				{
					if (initialValue is INullable)
					{
						return ConvertUtils.EnsureTypeAssignable(ConvertUtils.ToValue((INullable)initialValue), type, targetType);
					}
					throw new Exception("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						type,
						targetType
					}));
				}
			}
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x0001CD4C File Offset: 0x0001AF4C
		public static bool TryConvert<T>(object initialValue, out T convertedValue)
		{
			return ConvertUtils.TryConvert<T>(initialValue, CultureInfo.CurrentCulture, out convertedValue);
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x0001CD94 File Offset: 0x0001AF94
		public static bool TryConvert<T>(object initialValue, CultureInfo culture, out T convertedValue)
		{
			return MiscellaneousUtils.TryAction<T>(delegate
			{
				object obj;
				ConvertUtils.TryConvert(initialValue, CultureInfo.CurrentCulture, typeof(T), out obj);
				return (T)((object)obj);
			}, out convertedValue);
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x0001CDE4 File Offset: 0x0001AFE4
		public static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
		{
			return MiscellaneousUtils.TryAction<object>(() => ConvertUtils.Convert(initialValue, culture, targetType), out convertedValue);
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x0001CE1E File Offset: 0x0001B01E
		public static T ConvertOrCast<T>(object initialValue)
		{
			return ConvertUtils.ConvertOrCast<T>(initialValue, CultureInfo.CurrentCulture);
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x0001CE2B File Offset: 0x0001B02B
		public static T ConvertOrCast<T>(object initialValue, CultureInfo culture)
		{
			return (T)((object)ConvertUtils.ConvertOrCast(initialValue, culture, typeof(T)));
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x0001CE44 File Offset: 0x0001B044
		public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
		{
			if (targetType == typeof(object))
			{
				return initialValue;
			}
			if (initialValue == null && ReflectionUtils.IsNullable(targetType))
			{
				return null;
			}
			object result;
			if (ConvertUtils.TryConvert(initialValue, culture, targetType, out result))
			{
				return result;
			}
			return ConvertUtils.EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x0001CE8D File Offset: 0x0001B08D
		public static bool TryConvertOrCast<T>(object initialValue, out T convertedValue)
		{
			return ConvertUtils.TryConvertOrCast<T>(initialValue, CultureInfo.CurrentCulture, out convertedValue);
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x0001CED4 File Offset: 0x0001B0D4
		public static bool TryConvertOrCast<T>(object initialValue, CultureInfo culture, out T convertedValue)
		{
			return MiscellaneousUtils.TryAction<T>(delegate
			{
				object obj;
				ConvertUtils.TryConvertOrCast(initialValue, CultureInfo.CurrentCulture, typeof(T), out obj);
				return (T)((object)obj);
			}, out convertedValue);
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x0001CF24 File Offset: 0x0001B124
		public static bool TryConvertOrCast(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
		{
			return MiscellaneousUtils.TryAction<object>(() => ConvertUtils.ConvertOrCast(initialValue, culture, targetType), out convertedValue);
		}

		// Token: 0x060007EA RID: 2026 RVA: 0x0001CF60 File Offset: 0x0001B160
		private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
		{
			Type type = (value != null) ? value.GetType() : null;
			if (value != null)
			{
				if (targetType.IsAssignableFrom(type))
				{
					return value;
				}
				Func<object, object> func = ConvertUtils.CastConverters.Get(new ConvertUtils.TypeConvertKey(type, targetType));
				if (func != null)
				{
					return func(value);
				}
			}
			else if (ReflectionUtils.IsNullable(targetType))
			{
				return null;
			}
			throw new Exception("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				(initialType != null) ? initialType.ToString() : "{null}",
				targetType
			}));
		}

		// Token: 0x060007EB RID: 2027 RVA: 0x0001CFE8 File Offset: 0x0001B1E8
		public static object ToValue(INullable nullableValue)
		{
			if (nullableValue == null)
			{
				return null;
			}
			if (nullableValue is SqlInt32)
			{
				return ConvertUtils.ToValue((SqlInt32)nullableValue);
			}
			if (nullableValue is SqlInt64)
			{
				return ConvertUtils.ToValue((SqlInt64)nullableValue);
			}
			if (nullableValue is SqlBoolean)
			{
				return ConvertUtils.ToValue((SqlBoolean)nullableValue);
			}
			if (nullableValue is SqlString)
			{
				return ConvertUtils.ToValue((SqlString)nullableValue);
			}
			if (nullableValue is SqlDateTime)
			{
				return ConvertUtils.ToValue((SqlDateTime)nullableValue);
			}
			throw new Exception("Unsupported INullable type: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				nullableValue.GetType()
			}));
		}

		// Token: 0x060007EC RID: 2028 RVA: 0x0001D09C File Offset: 0x0001B29C
		internal static TypeConverter GetConverter(Type t)
		{
			return JsonTypeReflector.GetTypeConverter(t);
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x0001D0A4 File Offset: 0x0001B2A4
		public static bool IsInteger(object value)
		{
			switch (System.Convert.GetTypeCode(value))
			{
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				return true;
			default:
				return false;
			}
		}

		// Token: 0x04000277 RID: 631
		private static readonly ThreadSafeStore<ConvertUtils.TypeConvertKey, Func<object, object>> CastConverters = new ThreadSafeStore<ConvertUtils.TypeConvertKey, Func<object, object>>(new Func<ConvertUtils.TypeConvertKey, Func<object, object>>(ConvertUtils.CreateCastConverter));

		// Token: 0x020000B6 RID: 182
		internal struct TypeConvertKey : IEquatable<ConvertUtils.TypeConvertKey>
		{
			// Token: 0x1700017F RID: 383
			// (get) Token: 0x060007EF RID: 2031 RVA: 0x0001D0FD File Offset: 0x0001B2FD
			public Type InitialType
			{
				get
				{
					return this._initialType;
				}
			}

			// Token: 0x17000180 RID: 384
			// (get) Token: 0x060007F0 RID: 2032 RVA: 0x0001D105 File Offset: 0x0001B305
			public Type TargetType
			{
				get
				{
					return this._targetType;
				}
			}

			// Token: 0x060007F1 RID: 2033 RVA: 0x0001D10D File Offset: 0x0001B30D
			public TypeConvertKey(Type initialType, Type targetType)
			{
				this._initialType = initialType;
				this._targetType = targetType;
			}

			// Token: 0x060007F2 RID: 2034 RVA: 0x0001D11D File Offset: 0x0001B31D
			public override int GetHashCode()
			{
				return this._initialType.GetHashCode() ^ this._targetType.GetHashCode();
			}

			// Token: 0x060007F3 RID: 2035 RVA: 0x0001D136 File Offset: 0x0001B336
			public override bool Equals(object obj)
			{
				return obj is ConvertUtils.TypeConvertKey && this.Equals((ConvertUtils.TypeConvertKey)obj);
			}

			// Token: 0x060007F4 RID: 2036 RVA: 0x0001D14E File Offset: 0x0001B34E
			public bool Equals(ConvertUtils.TypeConvertKey other)
			{
				return this._initialType == other._initialType && this._targetType == other._targetType;
			}

			// Token: 0x04000278 RID: 632
			private readonly Type _initialType;

			// Token: 0x04000279 RID: 633
			private readonly Type _targetType;
		}
	}
}

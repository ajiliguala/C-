using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000BD RID: 189
	internal static class EnumUtils
	{
		// Token: 0x0600083B RID: 2107 RVA: 0x0001DC85 File Offset: 0x0001BE85
		public static T Parse<T>(string enumMemberName) where T : struct
		{
			return EnumUtils.Parse<T>(enumMemberName, false);
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0001DC8E File Offset: 0x0001BE8E
		public static T Parse<T>(string enumMemberName, bool ignoreCase) where T : struct
		{
			ValidationUtils.ArgumentTypeIsEnum(typeof(T), "T");
			return (T)((object)Enum.Parse(typeof(T), enumMemberName, ignoreCase));
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0001DCD8 File Offset: 0x0001BED8
		public static bool TryParse<T>(string enumMemberName, bool ignoreCase, out T value) where T : struct
		{
			ValidationUtils.ArgumentTypeIsEnum(typeof(T), "T");
			return MiscellaneousUtils.TryAction<T>(() => EnumUtils.Parse<T>(enumMemberName, ignoreCase), out value);
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0001DD2C File Offset: 0x0001BF2C
		public static IList<T> GetFlagsValues<T>(T value) where T : struct
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsDefined(typeof(FlagsAttribute), false))
			{
				throw new Exception("Enum type {0} is not a set of flags.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					typeFromHandle
				}));
			}
			Type underlyingType = Enum.GetUnderlyingType(value.GetType());
			ulong num = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			EnumValues<ulong> namesAndValues = EnumUtils.GetNamesAndValues<T>();
			IList<T> list = new List<T>();
			foreach (EnumValue<ulong> enumValue in namesAndValues)
			{
				if ((num & enumValue.Value) == enumValue.Value && enumValue.Value != 0UL)
				{
					list.Add((T)((object)Convert.ChangeType(enumValue.Value, underlyingType, CultureInfo.CurrentCulture)));
				}
			}
			if (list.Count == 0 && namesAndValues.SingleOrDefault((EnumValue<ulong> v) => v.Value == 0UL) != null)
			{
				list.Add(default(T));
			}
			return list;
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0001DE58 File Offset: 0x0001C058
		public static EnumValues<ulong> GetNamesAndValues<T>() where T : struct
		{
			return EnumUtils.GetNamesAndValues<ulong>(typeof(T));
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0001DE69 File Offset: 0x0001C069
		public static EnumValues<TUnderlyingType> GetNamesAndValues<TEnum, TUnderlyingType>() where TEnum : struct where TUnderlyingType : struct
		{
			return EnumUtils.GetNamesAndValues<TUnderlyingType>(typeof(TEnum));
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0001DE7C File Offset: 0x0001C07C
		public static EnumValues<TUnderlyingType> GetNamesAndValues<TUnderlyingType>(Type enumType) where TUnderlyingType : struct
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			ValidationUtils.ArgumentTypeIsEnum(enumType, "enumType");
			IList<object> values = EnumUtils.GetValues(enumType);
			IList<string> names = EnumUtils.GetNames(enumType);
			EnumValues<TUnderlyingType> enumValues = new EnumValues<TUnderlyingType>();
			for (int i = 0; i < values.Count; i++)
			{
				try
				{
					enumValues.Add(new EnumValue<TUnderlyingType>(names[i], (TUnderlyingType)((object)Convert.ChangeType(values[i], typeof(TUnderlyingType), CultureInfo.CurrentCulture))));
				}
				catch (OverflowException innerException)
				{
					throw new Exception(string.Format(CultureInfo.InvariantCulture, "Value from enum with the underlying type of {0} cannot be added to dictionary with a value type of {1}. Value was too large: {2}", new object[]
					{
						Enum.GetUnderlyingType(enumType),
						typeof(TUnderlyingType),
						Convert.ToUInt64(values[i], CultureInfo.InvariantCulture)
					}), innerException);
				}
			}
			return enumValues;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0001DF70 File Offset: 0x0001C170
		public static IList<T> GetValues<T>()
		{
			return EnumUtils.GetValues(typeof(T)).Cast<T>().ToList<T>();
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x0001DF94 File Offset: 0x0001C194
		public static IList<object> GetValues(Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");
			}
			List<object> list = new List<object>();
			IEnumerable<FieldInfo> enumerable = from field in enumType.GetFields()
			where field.IsLiteral
			select field;
			foreach (FieldInfo fieldInfo in enumerable)
			{
				object value = fieldInfo.GetValue(enumType);
				list.Add(value);
			}
			return list;
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0001E03C File Offset: 0x0001C23C
		public static IList<string> GetNames<T>()
		{
			return EnumUtils.GetNames(typeof(T));
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0001E058 File Offset: 0x0001C258
		public static IList<string> GetNames(Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");
			}
			List<string> list = new List<string>();
			IEnumerable<FieldInfo> enumerable = from field in enumType.GetFields()
			where field.IsLiteral
			select field;
			foreach (FieldInfo fieldInfo in enumerable)
			{
				list.Add(fieldInfo.Name);
			}
			return list;
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0001E0F8 File Offset: 0x0001C2F8
		public static TEnumType GetMaximumValue<TEnumType>(Type enumType) where TEnumType : IConvertible, IComparable<TEnumType>
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			Type underlyingType = Enum.GetUnderlyingType(enumType);
			if (!typeof(TEnumType).IsAssignableFrom(underlyingType))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "TEnumType is not assignable from the enum's underlying type of {0}.", new object[]
				{
					underlyingType.Name
				}));
			}
			ulong num = 0UL;
			IList<object> values = EnumUtils.GetValues(enumType);
			if (enumType.IsDefined(typeof(FlagsAttribute), false))
			{
				using (IEnumerator<object> enumerator = values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						TEnumType tenumType = (TEnumType)((object)obj);
						num |= tenumType.ToUInt64(CultureInfo.InvariantCulture);
					}
					goto IL_108;
				}
			}
			foreach (object obj2 in values)
			{
				TEnumType tenumType2 = (TEnumType)((object)obj2);
				ulong num2 = tenumType2.ToUInt64(CultureInfo.InvariantCulture);
				if (num.CompareTo(num2) == -1)
				{
					num = num2;
				}
			}
			IL_108:
			return (TEnumType)((object)Convert.ChangeType(num, typeof(TEnumType), CultureInfo.InvariantCulture));
		}
	}
}

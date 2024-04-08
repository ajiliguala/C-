using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000CE RID: 206
	internal static class ValidationUtils
	{
		// Token: 0x060008FD RID: 2301 RVA: 0x00021150 File Offset: 0x0001F350
		public static void ArgumentNotNullOrEmpty(string value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("'{0}' cannot be empty.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					parameterName
				}), parameterName);
			}
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x00021194 File Offset: 0x0001F394
		public static void ArgumentNotNullOrEmptyOrWhitespace(string value, string parameterName)
		{
			ValidationUtils.ArgumentNotNullOrEmpty(value, parameterName);
			if (StringUtils.IsWhiteSpace(value))
			{
				throw new ArgumentException("'{0}' cannot only be whitespace.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					parameterName
				}), parameterName);
			}
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x000211D4 File Offset: 0x0001F3D4
		public static void ArgumentTypeIsEnum(Type enumType, string parameterName)
		{
			ValidationUtils.ArgumentNotNull(enumType, "enumType");
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type {0} is not an Enum.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					enumType
				}), parameterName);
			}
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x00021218 File Offset: 0x0001F418
		public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName)
		{
			ValidationUtils.ArgumentNotNullOrEmpty<T>(collection, parameterName, "Collection '{0}' cannot be empty.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				parameterName
			}));
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x00021247 File Offset: 0x0001F447
		public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName, string message)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			if (collection.Count == 0)
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x00021264 File Offset: 0x0001F464
		public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName)
		{
			ValidationUtils.ArgumentNotNullOrEmpty(collection, parameterName, "Collection '{0}' cannot be empty.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				parameterName
			}));
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x00021293 File Offset: 0x0001F493
		public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName, string message)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			if (collection.Count == 0)
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x000212AF File Offset: 0x0001F4AF
		public static void ArgumentNotNull(object value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x000212BB File Offset: 0x0001F4BB
		public static void ArgumentNotNegative(int value, string parameterName)
		{
			if (value <= 0)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, "Argument cannot be negative.");
			}
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x000212D3 File Offset: 0x0001F4D3
		public static void ArgumentNotNegative(int value, string parameterName, string message)
		{
			if (value <= 0)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, message);
			}
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x000212E7 File Offset: 0x0001F4E7
		public static void ArgumentNotZero(int value, string parameterName)
		{
			if (value == 0)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, "Argument cannot be zero.");
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x000212FE File Offset: 0x0001F4FE
		public static void ArgumentNotZero(int value, string parameterName, string message)
		{
			if (value == 0)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, message);
			}
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x00021314 File Offset: 0x0001F514
		public static void ArgumentIsPositive<T>(T value, string parameterName) where T : struct, IComparable<T>
		{
			if (value.CompareTo(default(T)) != 1)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, "Positive number required.");
			}
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0002134C File Offset: 0x0001F54C
		public static void ArgumentIsPositive(int value, string parameterName, string message)
		{
			if (value > 0)
			{
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException(parameterName, value, message);
			}
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x00021360 File Offset: 0x0001F560
		public static void ObjectNotDisposed(bool disposed, Type objectType)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(objectType.Name);
			}
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x00021371 File Offset: 0x0001F571
		public static void ArgumentConditionTrue(bool condition, string parameterName, string message)
		{
			if (!condition)
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		// Token: 0x040002B1 RID: 689
		public const string EmailAddressRegex = "^([a-zA-Z0-9_'+*$%\\^&!\\.\\-])+\\@(([a-zA-Z0-9\\-])+\\.)+([a-zA-Z0-9:]{2,4})+$";

		// Token: 0x040002B2 RID: 690
		public const string CurrencyRegex = "(^\\$?(?!0,?\\d)\\d{1,3}(,?\\d{3})*(\\.\\d\\d)?)$";

		// Token: 0x040002B3 RID: 691
		public const string DateRegex = "^(((0?[1-9]|[12]\\d|3[01])[\\.\\-\\/](0?[13578]|1[02])[\\.\\-\\/]((1[6-9]|[2-9]\\d)?\\d{2}|\\d))|((0?[1-9]|[12]\\d|30)[\\.\\-\\/](0?[13456789]|1[012])[\\.\\-\\/]((1[6-9]|[2-9]\\d)?\\d{2}|\\d))|((0?[1-9]|1\\d|2[0-8])[\\.\\-\\/]0?2[\\.\\-\\/]((1[6-9]|[2-9]\\d)?\\d{2}|\\d))|(29[\\.\\-\\/]0?2[\\.\\-\\/]((1[6-9]|[2-9]\\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00|[048])))$";

		// Token: 0x040002B4 RID: 692
		public const string NumericRegex = "\\d*";
	}
}

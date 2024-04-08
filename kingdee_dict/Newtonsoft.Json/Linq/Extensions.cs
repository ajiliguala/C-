using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000066 RID: 102
	public static class Extensions
	{
		// Token: 0x06000436 RID: 1078 RVA: 0x0000EDD6 File Offset: 0x0000CFD6
		public static IJEnumerable<JToken> Ancestors<T>(this IEnumerable<T> source) where T : JToken
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			return source.SelectMany((T j) => j.Ancestors()).AsJEnumerable();
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x0000EE09 File Offset: 0x0000D009
		public static IJEnumerable<JToken> Descendants<T>(this IEnumerable<T> source) where T : JContainer
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			return source.SelectMany((T j) => j.Descendants()).AsJEnumerable();
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x0000EE35 File Offset: 0x0000D035
		public static IJEnumerable<JProperty> Properties(this IEnumerable<JObject> source)
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			return source.SelectMany((JObject d) => d.Properties()).AsJEnumerable<JProperty>();
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0000EE6A File Offset: 0x0000D06A
		public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source, object key)
		{
			return source.Values(key).AsJEnumerable();
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x0000EE78 File Offset: 0x0000D078
		public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source)
		{
			return source.Values(null);
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x0000EE81 File Offset: 0x0000D081
		public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source, object key)
		{
			return source.Values(key);
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x0000EE8A File Offset: 0x0000D08A
		public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source)
		{
			return source.Values(null);
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x0000EE93 File Offset: 0x0000D093
		public static U Value<U>(this IEnumerable<JToken> value)
		{
			return value.Value<JToken, U>();
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x0000EE9C File Offset: 0x0000D09C
		public static U Value<T, U>(this IEnumerable<T> value) where T : JToken
		{
			ValidationUtils.ArgumentNotNull(value, "source");
			JToken jtoken = value as JToken;
			if (jtoken == null)
			{
				throw new ArgumentException("Source value must be a JToken.");
			}
			return jtoken.Convert<JToken, U>();
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x0000F1CC File Offset: 0x0000D3CC
		internal static IEnumerable<U> Values<T, U>(this IEnumerable<T> source, object key) where T : JToken
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			foreach (T t2 in source)
			{
				JToken token = t2;
				if (key == null)
				{
					if (token is JValue)
					{
						yield return ((JValue)token).Convert<JValue, U>();
					}
					else
					{
						foreach (JToken t in token.Children())
						{
							yield return t.Convert<JToken, U>();
						}
					}
				}
				else
				{
					JToken value = token[key];
					if (value != null)
					{
						yield return value.Convert<JToken, U>();
					}
				}
			}
			yield break;
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x0000F1F0 File Offset: 0x0000D3F0
		public static IJEnumerable<JToken> Children<T>(this IEnumerable<T> source) where T : JToken
		{
			return source.Children<T, JToken>().AsJEnumerable();
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0000F211 File Offset: 0x0000D411
		public static IEnumerable<U> Children<T, U>(this IEnumerable<T> source) where T : JToken
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			return source.SelectMany((T c) => c.Children()).Convert<JToken, U>();
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x0000F40C File Offset: 0x0000D60C
		internal static IEnumerable<U> Convert<T, U>(this IEnumerable<T> source) where T : JToken
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			bool cast = typeof(JToken).IsAssignableFrom(typeof(U));
			foreach (T t in source)
			{
				JToken token = t;
				yield return token.Convert(cast);
			}
			yield break;
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x0000F42C File Offset: 0x0000D62C
		internal static U Convert<T, U>(this T token) where T : JToken
		{
			bool cast = typeof(JToken).IsAssignableFrom(typeof(U));
			return token.Convert(cast);
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x0000F45C File Offset: 0x0000D65C
		internal static U Convert<T, U>(this T token, bool cast) where T : JToken
		{
			if (cast)
			{
				return (U)((object)token);
			}
			if (token == null)
			{
				return default(U);
			}
			JValue jvalue = token as JValue;
			if (jvalue == null)
			{
				throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					token.GetType(),
					typeof(T)
				}));
			}
			if (jvalue.Value is U)
			{
				return (U)((object)jvalue.Value);
			}
			Type type = typeof(U);
			if (ReflectionUtils.IsNullableType(type))
			{
				if (jvalue.Value == null)
				{
					return default(U);
				}
				type = Nullable.GetUnderlyingType(type);
			}
			return (U)((object)System.Convert.ChangeType(jvalue.Value, type, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x0000F52F File Offset: 0x0000D72F
		public static IJEnumerable<JToken> AsJEnumerable(this IEnumerable<JToken> source)
		{
			return source.AsJEnumerable<JToken>();
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x0000F537 File Offset: 0x0000D737
		public static IJEnumerable<T> AsJEnumerable<T>(this IEnumerable<T> source) where T : JToken
		{
			if (source == null)
			{
				return null;
			}
			if (source is IJEnumerable<T>)
			{
				return (IJEnumerable<T>)source;
			}
			return new JEnumerable<T>(source);
		}
	}
}

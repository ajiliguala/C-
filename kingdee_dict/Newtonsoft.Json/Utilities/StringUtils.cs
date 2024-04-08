using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000CC RID: 204
	internal static class StringUtils
	{
		// Token: 0x060008E4 RID: 2276 RVA: 0x00020B4C File Offset: 0x0001ED4C
		public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
		{
			ValidationUtils.ArgumentNotNull(format, "format");
			return string.Format(provider, format, args);
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x00020B64 File Offset: 0x0001ED64
		public static bool ContainsWhiteSpace(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (char.IsWhiteSpace(s[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x00020BA4 File Offset: 0x0001EDA4
		public static bool IsWhiteSpace(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (s.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsWhiteSpace(s[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x00020BEC File Offset: 0x0001EDEC
		public static string EnsureEndsWith(string target, string value)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (target.Length >= value.Length)
			{
				if (string.Compare(target, target.Length - value.Length, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return target;
				}
				string text = target.TrimEnd(null);
				if (string.Compare(text, text.Length - value.Length, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return target;
				}
			}
			return target + value;
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x00020C72 File Offset: 0x0001EE72
		public static bool IsNullOrEmptyOrWhiteSpace(string s)
		{
			return string.IsNullOrEmpty(s) || StringUtils.IsWhiteSpace(s);
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x00020C89 File Offset: 0x0001EE89
		public static void IfNotNullOrEmpty(string value, Action<string> action)
		{
			StringUtils.IfNotNullOrEmpty(value, action, null);
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x00020C93 File Offset: 0x0001EE93
		private static void IfNotNullOrEmpty(string value, Action<string> trueAction, Action<string> falseAction)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (trueAction != null)
				{
					trueAction(value);
					return;
				}
			}
			else if (falseAction != null)
			{
				falseAction(value);
			}
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x00020CB2 File Offset: 0x0001EEB2
		public static string Indent(string s, int indentation)
		{
			return StringUtils.Indent(s, indentation, ' ');
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x00020CE8 File Offset: 0x0001EEE8
		public static string Indent(string s, int indentation, char indentChar)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (indentation <= 0)
			{
				throw new ArgumentException("Must be greater than zero.", "indentation");
			}
			StringReader textReader = new StringReader(s);
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			StringUtils.ActionTextReaderLine(textReader, stringWriter, delegate(TextWriter tw, string line)
			{
				tw.Write(new string(indentChar, indentation));
				tw.Write(line);
			});
			return stringWriter.ToString();
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x00020D5C File Offset: 0x0001EF5C
		private static void ActionTextReaderLine(TextReader textReader, TextWriter textWriter, StringUtils.ActionLine lineAction)
		{
			bool flag = true;
			string line;
			while ((line = textReader.ReadLine()) != null)
			{
				if (!flag)
				{
					textWriter.WriteLine();
				}
				else
				{
					flag = false;
				}
				lineAction(textWriter, line);
			}
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x00020DD4 File Offset: 0x0001EFD4
		public static string NumberLines(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			StringReader textReader = new StringReader(s);
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			int lineNumber = 1;
			StringUtils.ActionTextReaderLine(textReader, stringWriter, delegate(TextWriter tw, string line)
			{
				tw.Write(lineNumber.ToString(CultureInfo.InvariantCulture).PadLeft(4));
				tw.Write(". ");
				tw.Write(line);
				lineNumber++;
			});
			return stringWriter.ToString();
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x00020E27 File Offset: 0x0001F027
		public static string NullEmptyString(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				return s;
			}
			return null;
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x00020E34 File Offset: 0x0001F034
		public static string ReplaceNewLines(string s, string replacement)
		{
			StringReader stringReader = new StringReader(s);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			string value;
			while ((value = stringReader.ReadLine()) != null)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(replacement);
				}
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x00020E79 File Offset: 0x0001F079
		public static string Truncate(string s, int maximumLength)
		{
			return StringUtils.Truncate(s, maximumLength, "...");
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x00020E88 File Offset: 0x0001F088
		public static string Truncate(string s, int maximumLength, string suffix)
		{
			if (suffix == null)
			{
				throw new ArgumentNullException("suffix");
			}
			if (maximumLength <= 0)
			{
				throw new ArgumentException("Maximum length must be greater than zero.", "maximumLength");
			}
			int num = maximumLength - suffix.Length;
			if (num <= 0)
			{
				throw new ArgumentException("Length of suffix string is greater or equal to maximumLength");
			}
			if (s != null && s.Length > maximumLength)
			{
				string text = s.Substring(0, num);
				text = text.Trim();
				return text + suffix;
			}
			return s;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x00020EF8 File Offset: 0x0001F0F8
		public static StringWriter CreateStringWriter(int capacity)
		{
			StringBuilder sb = new StringBuilder(capacity);
			return new StringWriter(sb, CultureInfo.InvariantCulture);
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x00020F1C File Offset: 0x0001F11C
		public static int? GetLength(string value)
		{
			if (value == null)
			{
				return null;
			}
			return new int?(value.Length);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x00020F44 File Offset: 0x0001F144
		public static string ToCharAsUnicode(char c)
		{
			char c2 = MathUtils.IntToHex((int)(c >> 12 & '\u000f'));
			char c3 = MathUtils.IntToHex((int)(c >> 8 & '\u000f'));
			char c4 = MathUtils.IntToHex((int)(c >> 4 & '\u000f'));
			char c5 = MathUtils.IntToHex((int)(c & '\u000f'));
			return new string(new char[]
			{
				'\\',
				'u',
				c2,
				c3,
				c4,
				c5
			});
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x00020FB0 File Offset: 0x0001F1B0
		public static void WriteCharAsUnicode(TextWriter writer, char c)
		{
			ValidationUtils.ArgumentNotNull(writer, "writer");
			char value = MathUtils.IntToHex((int)(c >> 12 & '\u000f'));
			char value2 = MathUtils.IntToHex((int)(c >> 8 & '\u000f'));
			char value3 = MathUtils.IntToHex((int)(c >> 4 & '\u000f'));
			char value4 = MathUtils.IntToHex((int)(c & '\u000f'));
			writer.Write('\\');
			writer.Write('u');
			writer.Write(value);
			writer.Write(value2);
			writer.Write(value3);
			writer.Write(value4);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x00021068 File Offset: 0x0001F268
		public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (valueSelector == null)
			{
				throw new ArgumentNullException("valueSelector");
			}
			IEnumerable<TSource> source2 = from s in source
			where string.Compare(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase) == 0
			select s;
			if (source2.Count<TSource>() <= 1)
			{
				return source2.SingleOrDefault<TSource>();
			}
			IEnumerable<TSource> source3 = from s in source
			where string.Compare(valueSelector(s), testValue, StringComparison.Ordinal) == 0
			select s;
			return source3.SingleOrDefault<TSource>();
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x000210F0 File Offset: 0x0001F2F0
		public static string ToCamelCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return s;
			}
			if (!char.IsUpper(s[0]))
			{
				return s;
			}
			string text = char.ToLower(s[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
			if (s.Length > 1)
			{
				text += s.Substring(1);
			}
			return text;
		}

		// Token: 0x040002AC RID: 684
		public const string CarriageReturnLineFeed = "\r\n";

		// Token: 0x040002AD RID: 685
		public const string Empty = "";

		// Token: 0x040002AE RID: 686
		public const char CarriageReturn = '\r';

		// Token: 0x040002AF RID: 687
		public const char LineFeed = '\n';

		// Token: 0x040002B0 RID: 688
		public const char Tab = '\t';

		// Token: 0x020000CD RID: 205
		// (Invoke) Token: 0x060008FA RID: 2298
		private delegate void ActionLine(TextWriter textWriter, string line);
	}
}

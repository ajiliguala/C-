using System;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000CA RID: 202
	internal static class MiscellaneousUtils
	{
		// Token: 0x060008A1 RID: 2209 RVA: 0x0001F6E8 File Offset: 0x0001D8E8
		public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
		{
			string message2 = message + Environment.NewLine + "Actual value was {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				actualValue
			});
			return new ArgumentOutOfRangeException(paramName, message2);
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x0001F724 File Offset: 0x0001D924
		public static bool TryAction<T>(Creator<T> creator, out T output)
		{
			ValidationUtils.ArgumentNotNull(creator, "creator");
			bool result;
			try
			{
				output = creator();
				result = true;
			}
			catch
			{
				output = default(T);
				result = false;
			}
			return result;
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x0001F76C File Offset: 0x0001D96C
		public static string ToString(object value)
		{
			if (value == null)
			{
				return "{null}";
			}
			if (!(value is string))
			{
				return value.ToString();
			}
			return "\"" + value.ToString() + "\"";
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x0001F79C File Offset: 0x0001D99C
		public static byte[] HexToBytes(string hex)
		{
			string text = hex.Replace("-", string.Empty);
			byte[] array = new byte[text.Length / 2];
			int num = 4;
			int num2 = 0;
			foreach (char c in text)
			{
				int num3 = (int)((c - '0') % ' ');
				if (num3 > 9)
				{
					num3 -= 7;
				}
				byte[] array2 = array;
				int num4 = num2;
				array2[num4] |= (byte)(num3 << num);
				num ^= 4;
				if (num != 0)
				{
					num2++;
				}
			}
			return array;
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x0001F82E File Offset: 0x0001DA2E
		public static string BytesToHex(byte[] bytes)
		{
			return MiscellaneousUtils.BytesToHex(bytes, false);
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x0001F838 File Offset: 0x0001DA38
		public static string BytesToHex(byte[] bytes, bool removeDashes)
		{
			string text = BitConverter.ToString(bytes);
			if (removeDashes)
			{
				text = text.Replace("-", "");
			}
			return text;
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x0001F864 File Offset: 0x0001DA64
		public static int ByteArrayCompare(byte[] a1, byte[] a2)
		{
			int num = a1.Length.CompareTo(a2.Length);
			if (num != 0)
			{
				return num;
			}
			for (int i = 0; i < a1.Length; i++)
			{
				int num2 = a1[i].CompareTo(a2[i]);
				if (num2 != 0)
				{
					return num2;
				}
			}
			return 0;
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x0001F8AC File Offset: 0x0001DAAC
		public static string GetPrefix(string qualifiedName)
		{
			string result;
			string text;
			MiscellaneousUtils.GetQualifiedNameParts(qualifiedName, out result, out text);
			return result;
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x0001F8C4 File Offset: 0x0001DAC4
		public static string GetLocalName(string qualifiedName)
		{
			string text;
			string result;
			MiscellaneousUtils.GetQualifiedNameParts(qualifiedName, out text, out result);
			return result;
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x0001F8DC File Offset: 0x0001DADC
		public static void GetQualifiedNameParts(string qualifiedName, out string prefix, out string localName)
		{
			int num = qualifiedName.IndexOf(':');
			if (num == -1 || num == 0 || qualifiedName.Length - 1 == num)
			{
				prefix = null;
				localName = qualifiedName;
				return;
			}
			prefix = qualifiedName.Substring(0, num);
			localName = qualifiedName.Substring(num + 1);
		}
	}
}

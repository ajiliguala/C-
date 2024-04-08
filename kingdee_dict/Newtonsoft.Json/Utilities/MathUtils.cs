using System;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000C8 RID: 200
	internal class MathUtils
	{
		// Token: 0x06000893 RID: 2195 RVA: 0x0001F545 File Offset: 0x0001D745
		public static int IntLength(int i)
		{
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (i == 0)
			{
				return 1;
			}
			return (int)Math.Floor(Math.Log10((double)i)) + 1;
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x0001F565 File Offset: 0x0001D765
		public static int HexToInt(char h)
		{
			if (h >= '0' && h <= '9')
			{
				return (int)(h - '0');
			}
			if (h >= 'a' && h <= 'f')
			{
				return (int)(h - 'a' + '\n');
			}
			if (h >= 'A' && h <= 'F')
			{
				return (int)(h - 'A' + '\n');
			}
			return -1;
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x0001F59B File Offset: 0x0001D79B
		public static char IntToHex(int n)
		{
			if (n <= 9)
			{
				return (char)(n + 48);
			}
			return (char)(n - 10 + 97);
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x0001F5B0 File Offset: 0x0001D7B0
		public static int GetDecimalPlaces(double value)
		{
			int num = 10;
			double num2 = Math.Pow(0.1, (double)num);
			if (value == 0.0)
			{
				return 0;
			}
			int num3 = 0;
			while (value - Math.Floor(value) > num2 && num3 < num)
			{
				value *= 10.0;
				num3++;
			}
			return num3;
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x0001F604 File Offset: 0x0001D804
		public static int? Min(int? val1, int? val2)
		{
			if (val1 == null)
			{
				return val2;
			}
			if (val2 == null)
			{
				return val1;
			}
			return new int?(Math.Min(val1.Value, val2.Value));
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x0001F634 File Offset: 0x0001D834
		public static int? Max(int? val1, int? val2)
		{
			if (val1 == null)
			{
				return val2;
			}
			if (val2 == null)
			{
				return val1;
			}
			return new int?(Math.Max(val1.Value, val2.Value));
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x0001F664 File Offset: 0x0001D864
		public static double? Min(double? val1, double? val2)
		{
			if (val1 == null)
			{
				return val2;
			}
			if (val2 == null)
			{
				return val1;
			}
			return new double?(Math.Min(val1.Value, val2.Value));
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x0001F694 File Offset: 0x0001D894
		public static double? Max(double? val1, double? val2)
		{
			if (val1 == null)
			{
				return val2;
			}
			if (val2 == null)
			{
				return val1;
			}
			return new double?(Math.Max(val1.Value, val2.Value));
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x0001F6C4 File Offset: 0x0001D8C4
		public static bool ApproxEquals(double d1, double d2)
		{
			return Math.Abs(d1 - d2) < Math.Abs(d1) * 1E-06;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200006F RID: 111
	public static class MyConvert
	{
		// Token: 0x0600054A RID: 1354 RVA: 0x000120F8 File Offset: 0x000102F8
		static MyConvert()
		{
			MyConvert._maps[typeof(int)] = ((object o) => MyConvert.ToInt32(o, null));
			MyConvert._maps[typeof(decimal)] = ((object o) => MyConvert.ToDecimal(o, null));
			MyConvert._maps[typeof(long)] = ((object o) => MyConvert.ToLong(o, null));
			MyConvert._maps[typeof(double)] = ((object o) => MyConvert.ToDouble(o, null));
			MyConvert._maps[typeof(float)] = ((object o) => MyConvert.ToSingle(o, null));
			MyConvert._maps[typeof(string)] = ((object o) => MyConvert.ToString(o, null));
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00012238 File Offset: 0x00010438
		public static object ConvertTo(decimal d, Type type)
		{
			Func<object, object> func = null;
			if (MyConvert._maps.TryGetValue(type, out func))
			{
				return func(d);
			}
			return d;
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00012269 File Offset: 0x00010469
		public static decimal ToDecimal(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			return Convert.ToDecimal(v, provide);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0001227C File Offset: 0x0001047C
		public static long ToLong(object v, IFormatProvider provide = null)
		{
			return MyConvert.ToInt64(v, provide);
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x00012285 File Offset: 0x00010485
		public static long ToInt64(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToInt64((decimal)v);
			}
			return Convert.ToInt64(v, provide);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x000122AC File Offset: 0x000104AC
		public static int ToInt32(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToInt32((decimal)v);
			}
			return Convert.ToInt32(v, provide);
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x000122D3 File Offset: 0x000104D3
		public static short ToInt16(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToInt16((decimal)v);
			}
			return Convert.ToInt16(v, provide);
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x000122FA File Offset: 0x000104FA
		public static float ToSingle(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToSingle((decimal)v);
			}
			return Convert.ToSingle(v, provide);
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x00012321 File Offset: 0x00010521
		public static ushort ToUInt16(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToUInt16((decimal)v);
			}
			return Convert.ToUInt16(v, provide);
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x00012348 File Offset: 0x00010548
		public static uint ToUInt32(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToUInt32((decimal)v);
			}
			return Convert.ToUInt32(v, provide);
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0001236F File Offset: 0x0001056F
		public static ulong ToUInt64(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToUInt64((decimal)v);
			}
			return Convert.ToUInt64(v, provide);
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x00012396 File Offset: 0x00010596
		public static double ToDouble(object v, IFormatProvider provide = null)
		{
			if (provide == null)
			{
				provide = CultureInfo.InvariantCulture;
			}
			if (v is decimal)
			{
				return decimal.ToDouble((decimal)v);
			}
			return Convert.ToDouble(v, provide);
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x000123BD File Offset: 0x000105BD
		public static string ToString(object v, IFormatProvider provide = null)
		{
			if (v == null)
			{
				return "";
			}
			return v.ToString();
		}

		// Token: 0x04000147 RID: 327
		private static Dictionary<Type, Func<object, object>> _maps = new Dictionary<Type, Func<object, object>>();
	}
}

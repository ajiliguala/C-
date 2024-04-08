using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000028 RID: 40
	public abstract class JToken : IJEnumerable<JToken>, IEnumerable<JToken>, IEnumerable, IJsonLineInfo, ICloneable, IDynamicMetaObjectProvider
	{
		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000137 RID: 311 RVA: 0x00006024 File Offset: 0x00004224
		public static JTokenEqualityComparer EqualityComparer
		{
			get
			{
				if (JToken._equalityComparer == null)
				{
					JToken._equalityComparer = new JTokenEqualityComparer();
				}
				return JToken._equalityComparer;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000138 RID: 312 RVA: 0x0000603C File Offset: 0x0000423C
		// (set) Token: 0x06000139 RID: 313 RVA: 0x00006044 File Offset: 0x00004244
		public JContainer Parent
		{
			[DebuggerStepThrough]
			get
			{
				return this._parent;
			}
			internal set
			{
				this._parent = value;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600013A RID: 314 RVA: 0x00006050 File Offset: 0x00004250
		public JToken Root
		{
			get
			{
				JContainer parent = this.Parent;
				if (parent == null)
				{
					return this;
				}
				while (parent.Parent != null)
				{
					parent = parent.Parent;
				}
				return parent;
			}
		}

		// Token: 0x0600013B RID: 315
		internal abstract JToken CloneToken();

		// Token: 0x0600013C RID: 316
		internal abstract bool DeepEquals(JToken node);

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600013D RID: 317
		public abstract JTokenType Type { get; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600013E RID: 318
		public abstract bool HasValues { get; }

		// Token: 0x0600013F RID: 319 RVA: 0x00006079 File Offset: 0x00004279
		public static bool DeepEquals(JToken t1, JToken t2)
		{
			return t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2));
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000140 RID: 320 RVA: 0x00006090 File Offset: 0x00004290
		// (set) Token: 0x06000141 RID: 321 RVA: 0x000060B5 File Offset: 0x000042B5
		public JToken Next
		{
			get
			{
				if (this._parent != null && this._next != this._parent.First)
				{
					return this._next;
				}
				return null;
			}
			internal set
			{
				this._next = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000142 RID: 322 RVA: 0x000060C0 File Offset: 0x000042C0
		public JToken Previous
		{
			get
			{
				if (this._parent == null)
				{
					return null;
				}
				JToken next = this._parent.Content._next;
				JToken result = null;
				while (next != this)
				{
					result = next;
					next = next.Next;
				}
				return result;
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x000060FA File Offset: 0x000042FA
		internal JToken()
		{
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00006102 File Offset: 0x00004302
		public void AddAfterSelf(object content)
		{
			if (this._parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			this._parent.AddInternal(this.Next == null, this, content);
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00006130 File Offset: 0x00004330
		public void AddBeforeSelf(object content)
		{
			if (this._parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			JToken jtoken = this.Previous;
			if (jtoken == null)
			{
				jtoken = this._parent.Last;
			}
			this._parent.AddInternal(false, jtoken, content);
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00006270 File Offset: 0x00004470
		public IEnumerable<JToken> Ancestors()
		{
			for (JToken parent = this.Parent; parent != null; parent = parent.Parent)
			{
				yield return parent;
			}
			yield break;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000639C File Offset: 0x0000459C
		public IEnumerable<JToken> AfterSelf()
		{
			if (this.Parent != null)
			{
				for (JToken o = this.Next; o != null; o = o.Next)
				{
					yield return o;
				}
			}
			yield break;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x000064C4 File Offset: 0x000046C4
		public IEnumerable<JToken> BeforeSelf()
		{
			for (JToken o = this.Parent.First; o != this; o = o.Next)
			{
				yield return o;
			}
			yield break;
		}

		// Token: 0x1700002F RID: 47
		public virtual JToken this[object key]
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
			set
			{
				throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000654C File Offset: 0x0000474C
		public virtual T Value<T>(object key)
		{
			JToken token = this[key];
			return token.Convert<JToken, T>();
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00006568 File Offset: 0x00004768
		public virtual JToken First
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600014D RID: 333 RVA: 0x0000659C File Offset: 0x0000479C
		public virtual JToken Last
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					base.GetType()
				}));
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000065D0 File Offset: 0x000047D0
		public virtual JEnumerable<JToken> Children()
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				base.GetType()
			}));
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00006602 File Offset: 0x00004802
		public JEnumerable<T> Children<T>() where T : JToken
		{
			return new JEnumerable<T>(this.Children().OfType<T>());
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000661C File Offset: 0x0000481C
		public virtual IEnumerable<T> Values<T>()
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				base.GetType()
			}));
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000664E File Offset: 0x0000484E
		public void Remove()
		{
			if (this._parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			this._parent.RemoveItem(this);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00006670 File Offset: 0x00004870
		public void Replace(JToken value)
		{
			if (this._parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			this._parent.ReplaceItem(this, value);
		}

		// Token: 0x06000153 RID: 339
		public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

		// Token: 0x06000154 RID: 340 RVA: 0x00006692 File Offset: 0x00004892
		public override string ToString()
		{
			return this.ToString(Formatting.Indented, new JsonConverter[0]);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x000066A4 File Offset: 0x000048A4
		public string ToString(Formatting formatting, params JsonConverter[] converters)
		{
			string result;
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				this.WriteTo(new JsonTextWriter(stringWriter)
				{
					Formatting = formatting
				}, converters);
				result = stringWriter.ToString();
			}
			return result;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000066F8 File Offset: 0x000048F8
		private static JValue EnsureValue(JToken value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value is JProperty)
			{
				value = ((JProperty)value).Value;
			}
			return value as JValue;
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00006730 File Offset: 0x00004930
		private static string GetType(JToken token)
		{
			ValidationUtils.ArgumentNotNull(token, "token");
			if (token is JProperty)
			{
				token = ((JProperty)token).Value;
			}
			return token.Type.ToString();
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00006762 File Offset: 0x00004962
		private static bool IsNullable(JToken o)
		{
			return o.Type == JTokenType.Undefined || o.Type == JTokenType.Null;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000677A File Offset: 0x0000497A
		private static bool ValidateFloat(JToken o, bool nullable)
		{
			return o.Type == JTokenType.Float || o.Type == JTokenType.Integer || (nullable && JToken.IsNullable(o));
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000679B File Offset: 0x0000499B
		private static bool ValidateInteger(JToken o, bool nullable)
		{
			return o.Type == JTokenType.Integer || o.Type == JTokenType.Float || (nullable && JToken.IsNullable(o));
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000067BC File Offset: 0x000049BC
		private static bool ValidateDate(JToken o, bool nullable)
		{
			return o.Type == JTokenType.Date || (nullable && JToken.IsNullable(o));
		}

		// Token: 0x0600015C RID: 348 RVA: 0x000067D5 File Offset: 0x000049D5
		private static bool ValidateBoolean(JToken o, bool nullable)
		{
			return o.Type == JTokenType.Boolean || (nullable && JToken.IsNullable(o));
		}

		// Token: 0x0600015D RID: 349 RVA: 0x000067EE File Offset: 0x000049EE
		private static bool ValidateString(JToken o)
		{
			return o.Type == JTokenType.String || o.Type == JTokenType.Float || o.Type == JTokenType.Integer || o.Type == JTokenType.Comment || o.Type == JTokenType.Raw || JToken.IsNullable(o);
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00006826 File Offset: 0x00004A26
		private static bool ValidateBytes(JToken o)
		{
			return o.Type == JTokenType.Bytes || JToken.IsNullable(o);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000683C File Offset: 0x00004A3C
		public static explicit operator bool(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateBoolean(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (bool)jvalue.Value;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00006890 File Offset: 0x00004A90
		public static explicit operator DateTimeOffset(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateDate(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (DateTimeOffset)jvalue.Value;
		}

		// Token: 0x06000161 RID: 353 RVA: 0x000068E4 File Offset: 0x00004AE4
		public static explicit operator bool?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateBoolean(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (bool?)jvalue.Value;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00006944 File Offset: 0x00004B44
		public static explicit operator long(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (long)jvalue.Value;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00006998 File Offset: 0x00004B98
		public static explicit operator DateTime?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateDate(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (DateTime?)jvalue.Value;
		}

		// Token: 0x06000164 RID: 356 RVA: 0x000069F8 File Offset: 0x00004BF8
		public static explicit operator DateTimeOffset?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateDate(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (DateTimeOffset?)jvalue.Value;
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00006A58 File Offset: 0x00004C58
		public static explicit operator decimal?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			if (jvalue.Value == null)
			{
				return null;
			}
			return new decimal?(Convert.ToDecimal(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00006AD4 File Offset: 0x00004CD4
		public static explicit operator double?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return new double?(MyConvert.ToDouble(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00006B3C File Offset: 0x00004D3C
		public static explicit operator int(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToInt32(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00006B94 File Offset: 0x00004D94
		public static explicit operator short(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToInt16(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00006BEC File Offset: 0x00004DEC
		[CLSCompliant(false)]
		public static explicit operator ushort(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToUInt16(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00006C44 File Offset: 0x00004E44
		public static explicit operator int?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			if (jvalue.Value == null)
			{
				return null;
			}
			return new int?(MyConvert.ToInt32(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00006CC0 File Offset: 0x00004EC0
		public static explicit operator short?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			if (jvalue.Value == null)
			{
				return null;
			}
			return new short?(MyConvert.ToInt16(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00006D3C File Offset: 0x00004F3C
		[CLSCompliant(false)]
		public static explicit operator ushort?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			if (jvalue.Value == null)
			{
				return null;
			}
			return new ushort?((ushort)MyConvert.ToInt16(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00006DB8 File Offset: 0x00004FB8
		public static explicit operator DateTime(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateDate(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (DateTime)jvalue.Value;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00006E0C File Offset: 0x0000500C
		public static explicit operator long?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return new long?(MyConvert.ToInt64(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00006E74 File Offset: 0x00005074
		public static explicit operator float?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			if (jvalue.Value == null)
			{
				return null;
			}
			return new float?(MyConvert.ToSingle(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00006EF0 File Offset: 0x000050F0
		public static explicit operator decimal(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToDecimal(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00006F48 File Offset: 0x00005148
		[CLSCompliant(false)]
		public static explicit operator uint?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return new uint?(MyConvert.ToUInt32(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00006FB0 File Offset: 0x000051B0
		[CLSCompliant(false)]
		public static explicit operator ulong?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return new ulong?((ulong)MyConvert.ToSingle(jvalue.Value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000701C File Offset: 0x0000521C
		public static explicit operator double(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToDouble(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00007074 File Offset: 0x00005274
		public static explicit operator float(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateFloat(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToSingle(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000070CC File Offset: 0x000052CC
		public static explicit operator string(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateString(jvalue))
			{
				throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToString(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00007128 File Offset: 0x00005328
		[CLSCompliant(false)]
		public static explicit operator uint(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToUInt32(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00007180 File Offset: 0x00005380
		[CLSCompliant(false)]
		public static explicit operator ulong(JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateInteger(jvalue, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return MyConvert.ToUInt64(jvalue.Value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x000071D8 File Offset: 0x000053D8
		public static explicit operator byte[](JToken value)
		{
			JValue jvalue = JToken.EnsureValue(value);
			if (jvalue == null || !JToken.ValidateBytes(jvalue))
			{
				throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JToken.GetType(value)
				}));
			}
			return (byte[])jvalue.Value;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00007228 File Offset: 0x00005428
		public static implicit operator JToken(bool value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00007230 File Offset: 0x00005430
		public static implicit operator JToken(DateTimeOffset value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000723D File Offset: 0x0000543D
		public static implicit operator JToken(bool? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000724A File Offset: 0x0000544A
		public static implicit operator JToken(long value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00007252 File Offset: 0x00005452
		public static implicit operator JToken(DateTime? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000725F File Offset: 0x0000545F
		public static implicit operator JToken(DateTimeOffset? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000726C File Offset: 0x0000546C
		public static implicit operator JToken(decimal? value)
		{
			return new JValue(value);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00007279 File Offset: 0x00005479
		public static implicit operator JToken(double? value)
		{
			return new JValue(value);
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00007286 File Offset: 0x00005486
		[CLSCompliant(false)]
		public static implicit operator JToken(short value)
		{
			return new JValue((long)value);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000728F File Offset: 0x0000548F
		[CLSCompliant(false)]
		public static implicit operator JToken(ushort value)
		{
			return new JValue((long)((ulong)value));
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00007298 File Offset: 0x00005498
		public static implicit operator JToken(int value)
		{
			return new JValue((long)value);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x000072A1 File Offset: 0x000054A1
		public static implicit operator JToken(int? value)
		{
			return new JValue(value);
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000072AE File Offset: 0x000054AE
		public static implicit operator JToken(DateTime value)
		{
			return new JValue(value);
		}

		// Token: 0x06000186 RID: 390 RVA: 0x000072B6 File Offset: 0x000054B6
		public static implicit operator JToken(long? value)
		{
			return new JValue(value);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000072C3 File Offset: 0x000054C3
		public static implicit operator JToken(float? value)
		{
			return new JValue(value);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000072D0 File Offset: 0x000054D0
		public static implicit operator JToken(decimal value)
		{
			return new JValue(value);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x000072DD File Offset: 0x000054DD
		[CLSCompliant(false)]
		public static implicit operator JToken(short? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x000072EA File Offset: 0x000054EA
		[CLSCompliant(false)]
		public static implicit operator JToken(ushort? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x000072F7 File Offset: 0x000054F7
		[CLSCompliant(false)]
		public static implicit operator JToken(uint? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00007304 File Offset: 0x00005504
		[CLSCompliant(false)]
		public static implicit operator JToken(ulong? value)
		{
			return new JValue(value);
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00007311 File Offset: 0x00005511
		public static implicit operator JToken(double value)
		{
			return new JValue(value);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00007319 File Offset: 0x00005519
		public static implicit operator JToken(float value)
		{
			return new JValue((double)value);
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00007322 File Offset: 0x00005522
		public static implicit operator JToken(string value)
		{
			return new JValue(value);
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000732A File Offset: 0x0000552A
		[CLSCompliant(false)]
		public static implicit operator JToken(uint value)
		{
			return new JValue((long)((ulong)value));
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00007333 File Offset: 0x00005533
		[CLSCompliant(false)]
		public static implicit operator JToken(ulong value)
		{
			return new JValue(value);
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000733B File Offset: 0x0000553B
		public static implicit operator JToken(byte[] value)
		{
			return new JValue(value);
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00007343 File Offset: 0x00005543
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<JToken>)this).GetEnumerator();
		}

		// Token: 0x06000194 RID: 404 RVA: 0x0000734C File Offset: 0x0000554C
		IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
		{
			return this.Children().GetEnumerator();
		}

		// Token: 0x06000195 RID: 405
		internal abstract int GetDeepHashCode();

		// Token: 0x17000032 RID: 50
		IJEnumerable<JToken> IJEnumerable<JToken>.this[object key]
		{
			get
			{
				return this[key];
			}
		}

		// Token: 0x06000197 RID: 407 RVA: 0x00007370 File Offset: 0x00005570
		public JsonReader CreateReader()
		{
			return new JTokenReader(this);
		}

		// Token: 0x06000198 RID: 408 RVA: 0x00007378 File Offset: 0x00005578
		internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
			JToken token;
			using (JTokenWriter jtokenWriter = new JTokenWriter())
			{
				jsonSerializer.Serialize(jtokenWriter, o);
				token = jtokenWriter.Token;
			}
			return token;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000073D0 File Offset: 0x000055D0
		public static JToken FromObject(object o)
		{
			return JToken.FromObjectInternal(o, new JsonSerializer());
		}

		// Token: 0x0600019A RID: 410 RVA: 0x000073DD File Offset: 0x000055DD
		public static JToken FromObject(object o, JsonSerializer jsonSerializer)
		{
			return JToken.FromObjectInternal(o, jsonSerializer);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x000073E8 File Offset: 0x000055E8
		public static JToken ReadFrom(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JToken from JsonReader.");
			}
			if (reader.TokenType == JsonToken.StartObject)
			{
				return JObject.Load(reader);
			}
			if (reader.TokenType == JsonToken.StartArray)
			{
				return JArray.Load(reader);
			}
			if (reader.TokenType == JsonToken.PropertyName)
			{
				return JProperty.Load(reader);
			}
			if (reader.TokenType == JsonToken.StartConstructor)
			{
				return JConstructor.Load(reader);
			}
			if (!JsonReader.IsStartToken(reader.TokenType))
			{
				return new JValue(reader.Value);
			}
			throw new Exception("Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				reader.TokenType
			}));
		}

		// Token: 0x0600019C RID: 412 RVA: 0x000074A0 File Offset: 0x000056A0
		public static JToken Parse(string json)
		{
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return JToken.Load(reader);
		}

		// Token: 0x0600019D RID: 413 RVA: 0x000074BF File Offset: 0x000056BF
		public static JToken Load(JsonReader reader)
		{
			return JToken.ReadFrom(reader);
		}

		// Token: 0x0600019E RID: 414 RVA: 0x000074C7 File Offset: 0x000056C7
		internal void SetLineInfo(IJsonLineInfo lineInfo)
		{
			if (lineInfo == null || !lineInfo.HasLineInfo())
			{
				return;
			}
			this.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x000074E7 File Offset: 0x000056E7
		internal void SetLineInfo(int lineNumber, int linePosition)
		{
			this._lineNumber = new int?(lineNumber);
			this._linePosition = new int?(linePosition);
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00007501 File Offset: 0x00005701
		bool IJsonLineInfo.HasLineInfo()
		{
			return this._lineNumber != null && this._linePosition != null;
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x00007520 File Offset: 0x00005720
		int IJsonLineInfo.LineNumber
		{
			get
			{
				int? lineNumber = this._lineNumber;
				if (lineNumber == null)
				{
					return 0;
				}
				return lineNumber.GetValueOrDefault();
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x00007548 File Offset: 0x00005748
		int IJsonLineInfo.LinePosition
		{
			get
			{
				int? linePosition = this._linePosition;
				if (linePosition == null)
				{
					return 0;
				}
				return linePosition.GetValueOrDefault();
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000756E File Offset: 0x0000576E
		public JToken SelectToken(string path)
		{
			return this.SelectToken(path, false);
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00007578 File Offset: 0x00005778
		public JToken SelectToken(string path, bool errorWhenNoMatch)
		{
			JPath jpath = new JPath(path);
			return jpath.Evaluate(this, errorWhenNoMatch);
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00007594 File Offset: 0x00005794
		protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new DynamicProxyMetaObject<JToken>(parameter, this, new DynamicProxy<JToken>(), true);
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x000075A3 File Offset: 0x000057A3
		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return this.GetMetaObject(parameter);
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000075AC File Offset: 0x000057AC
		object ICloneable.Clone()
		{
			return this.DeepClone();
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x000075B4 File Offset: 0x000057B4
		public JToken DeepClone()
		{
			return this.CloneToken();
		}

		// Token: 0x0400007F RID: 127
		private JContainer _parent;

		// Token: 0x04000080 RID: 128
		internal JToken _next;

		// Token: 0x04000081 RID: 129
		private static JTokenEqualityComparer _equalityComparer;

		// Token: 0x04000082 RID: 130
		private int? _lineNumber;

		// Token: 0x04000083 RID: 131
		private int? _linePosition;
	}
}

using System;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000029 RID: 41
	public class JValue : JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>
	{
		// Token: 0x060001A9 RID: 425 RVA: 0x000075BC File Offset: 0x000057BC
		internal JValue(object value, JTokenType type)
		{
			this._value = value;
			this._valueType = type;
		}

		// Token: 0x060001AA RID: 426 RVA: 0x000075D2 File Offset: 0x000057D2
		public JValue(JValue other) : this(other.Value, other.Type)
		{
		}

		// Token: 0x060001AB RID: 427 RVA: 0x000075E6 File Offset: 0x000057E6
		public JValue(long value) : this(value, JTokenType.Integer)
		{
		}

		// Token: 0x060001AC RID: 428 RVA: 0x000075F5 File Offset: 0x000057F5
		[CLSCompliant(false)]
		public JValue(ulong value) : this(value, JTokenType.Integer)
		{
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00007604 File Offset: 0x00005804
		public JValue(double value) : this(value, JTokenType.Float)
		{
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00007613 File Offset: 0x00005813
		public JValue(DateTime value) : this(value, JTokenType.Date)
		{
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00007623 File Offset: 0x00005823
		public JValue(bool value) : this(value, JTokenType.Boolean)
		{
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00007633 File Offset: 0x00005833
		public JValue(string value) : this(value, JTokenType.String)
		{
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00007640 File Offset: 0x00005840
		public JValue(object value) : this(value, JValue.GetValueType(null, value))
		{
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00007664 File Offset: 0x00005864
		internal override bool DeepEquals(JToken node)
		{
			JValue jvalue = node as JValue;
			return jvalue != null && JValue.ValuesEquals(this, jvalue);
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x00007684 File Offset: 0x00005884
		public override bool HasValues
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00007688 File Offset: 0x00005888
		private static int Compare(JTokenType valueType, object objA, object objB)
		{
			if (objA == null && objB == null)
			{
				return 0;
			}
			if (objA != null && objB == null)
			{
				return 1;
			}
			if (objA == null && objB != null)
			{
				return -1;
			}
			switch (valueType)
			{
			case JTokenType.Comment:
			case JTokenType.String:
			case JTokenType.Raw:
			{
				string text = Convert.ToString(objA, CultureInfo.InvariantCulture);
				string strB = Convert.ToString(objB, CultureInfo.InvariantCulture);
				return text.CompareTo(strB);
			}
			case JTokenType.Integer:
				if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
				{
					return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
				}
				if (objA is float || objB is float || objA is double || objB is double)
				{
					return JValue.CompareFloat(objA, objB);
				}
				return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
			case JTokenType.Float:
				return JValue.CompareFloat(objA, objB);
			case JTokenType.Boolean:
			{
				bool flag = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
				bool value = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);
				return flag.CompareTo(value);
			}
			case JTokenType.Date:
			{
				if (objA is DateTime)
				{
					DateTime dateTime = Convert.ToDateTime(objA, CultureInfo.InvariantCulture);
					DateTime value2 = Convert.ToDateTime(objB, CultureInfo.InvariantCulture);
					return dateTime.CompareTo(value2);
				}
				if (!(objB is DateTimeOffset))
				{
					throw new ArgumentException("Object must be of type DateTimeOffset.");
				}
				DateTimeOffset dateTimeOffset = (DateTimeOffset)objA;
				DateTimeOffset other = (DateTimeOffset)objB;
				return dateTimeOffset.CompareTo(other);
			}
			case JTokenType.Bytes:
			{
				if (!(objB is byte[]))
				{
					throw new ArgumentException("Object must be of type byte[].");
				}
				byte[] array = objA as byte[];
				byte[] array2 = objB as byte[];
				if (array == null)
				{
					return -1;
				}
				if (array2 == null)
				{
					return 1;
				}
				return MiscellaneousUtils.ByteArrayCompare(array, array2);
			}
			}
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				valueType
			}));
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000787C File Offset: 0x00005A7C
		private static int CompareFloat(object objA, object objB)
		{
			double d = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
			double num = Convert.ToDouble(objB, CultureInfo.InvariantCulture);
			if (MathUtils.ApproxEquals(d, num))
			{
				return 0;
			}
			return d.CompareTo(num);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x000078B4 File Offset: 0x00005AB4
		internal override JToken CloneToken()
		{
			return new JValue(this);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x000078BC File Offset: 0x00005ABC
		public static JValue CreateComment(string value)
		{
			return new JValue(value, JTokenType.Comment);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x000078C5 File Offset: 0x00005AC5
		public static JValue CreateString(string value)
		{
			return new JValue(value, JTokenType.String);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x000078D0 File Offset: 0x00005AD0
		private static JTokenType GetValueType(JTokenType? current, object value)
		{
			if (value == null)
			{
				return JTokenType.Null;
			}
			if (value == DBNull.Value)
			{
				return JTokenType.Null;
			}
			if (value is string)
			{
				return JValue.GetStringValueType(current);
			}
			if (value is long || value is int || value is short || value is sbyte || value is ulong || value is uint || value is ushort || value is byte)
			{
				return JTokenType.Integer;
			}
			if (value is Enum)
			{
				return JTokenType.Integer;
			}
			if (value is double || value is float || value is decimal)
			{
				return JTokenType.Float;
			}
			if (value is DateTime)
			{
				return JTokenType.Date;
			}
			if (value is DateTimeOffset)
			{
				return JTokenType.Date;
			}
			if (value is byte[])
			{
				return JTokenType.Bytes;
			}
			if (value is bool)
			{
				return JTokenType.Boolean;
			}
			throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				value.GetType()
			}));
		}

		// Token: 0x060001BA RID: 442 RVA: 0x000079B4 File Offset: 0x00005BB4
		private static JTokenType GetStringValueType(JTokenType? current)
		{
			if (current == null)
			{
				return JTokenType.String;
			}
			JTokenType value = current.Value;
			if (value == JTokenType.Comment || value == JTokenType.String || value == JTokenType.Raw)
			{
				return current.Value;
			}
			return JTokenType.String;
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060001BB RID: 443 RVA: 0x000079EA File Offset: 0x00005BEA
		public override JTokenType Type
		{
			get
			{
				return this._valueType;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060001BC RID: 444 RVA: 0x000079F2 File Offset: 0x00005BF2
		// (set) Token: 0x060001BD RID: 445 RVA: 0x000079FC File Offset: 0x00005BFC
		public new object Value
		{
			get
			{
				return this._value;
			}
			set
			{
				Type left = (this._value != null) ? this._value.GetType() : null;
				Type right = (value != null) ? value.GetType() : null;
				if (left != right)
				{
					this._valueType = JValue.GetValueType(new JTokenType?(this._valueType), value);
				}
				this._value = value;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00007A54 File Offset: 0x00005C54
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			JTokenType valueType = this._valueType;
			if (valueType == JTokenType.Comment)
			{
				writer.WriteComment(this._value.ToString());
				return;
			}
			switch (valueType)
			{
			case JTokenType.Null:
				writer.WriteNull();
				return;
			case JTokenType.Undefined:
				writer.WriteUndefined();
				return;
			case JTokenType.Raw:
				writer.WriteRawValue((this._value != null) ? this._value.ToString() : null);
				return;
			}
			JsonConverter matchingConverter;
			if (this._value != null && (matchingConverter = JsonSerializer.GetMatchingConverter(converters, this._value.GetType())) != null)
			{
				matchingConverter.WriteJson(writer, this._value, new JsonSerializer());
				return;
			}
			switch (this._valueType)
			{
			case JTokenType.Integer:
				writer.WriteValue(Convert.ToInt64(this._value, CultureInfo.InvariantCulture));
				return;
			case JTokenType.Float:
				if (this._value is decimal)
				{
					writer.WriteValue(Convert.ToDecimal(this._value, CultureInfo.InvariantCulture));
					return;
				}
				writer.WriteValue(Convert.ToDouble(this._value, CultureInfo.InvariantCulture));
				return;
			case JTokenType.String:
				writer.WriteValue((this._value != null) ? this._value.ToString() : null);
				return;
			case JTokenType.Boolean:
				writer.WriteValue(Convert.ToBoolean(this._value, CultureInfo.InvariantCulture));
				return;
			case JTokenType.Date:
				if (this._value is DateTimeOffset)
				{
					writer.WriteValue((DateTimeOffset)this._value);
					return;
				}
				writer.WriteValue(Convert.ToDateTime(this._value, CultureInfo.InvariantCulture));
				return;
			case JTokenType.Bytes:
				writer.WriteValue((byte[])this._value);
				return;
			}
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", this._valueType, "Unexpected token type.");
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00007C14 File Offset: 0x00005E14
		internal override int GetDeepHashCode()
		{
			int num = (this._value != null) ? this._value.GetHashCode() : 0;
			return this._valueType.GetHashCode() ^ num;
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00007C4A File Offset: 0x00005E4A
		private static bool ValuesEquals(JValue v1, JValue v2)
		{
			return v1 == v2 || (v1._valueType == v2._valueType && JValue.Compare(v1._valueType, v1._value, v2._value) == 0);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00007C7C File Offset: 0x00005E7C
		public bool Equals(JValue other)
		{
			return other != null && JValue.ValuesEquals(this, other);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00007C8C File Offset: 0x00005E8C
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			JValue jvalue = obj as JValue;
			if (jvalue != null)
			{
				return this.Equals(jvalue);
			}
			return base.Equals(obj);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00007CB7 File Offset: 0x00005EB7
		public override int GetHashCode()
		{
			if (this._value == null)
			{
				return 0;
			}
			return this._value.GetHashCode();
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00007CCE File Offset: 0x00005ECE
		public override string ToString()
		{
			if (this._value == null)
			{
				return string.Empty;
			}
			return this._value.ToString();
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00007CE9 File Offset: 0x00005EE9
		public string ToString(string format)
		{
			return this.ToString(format, CultureInfo.CurrentCulture);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x00007CF7 File Offset: 0x00005EF7
		public string ToString(IFormatProvider formatProvider)
		{
			return this.ToString(null, formatProvider);
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00007D04 File Offset: 0x00005F04
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (this._value == null)
			{
				return string.Empty;
			}
			IFormattable formattable = this._value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(format, formatProvider);
			}
			return this._value.ToString();
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00007D42 File Offset: 0x00005F42
		protected override DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new DynamicProxyMetaObject<JValue>(parameter, this, new JValue.JValueDynamicProxy(), true);
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00007D54 File Offset: 0x00005F54
		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			object objB = (obj is JValue) ? ((JValue)obj).Value : obj;
			return JValue.Compare(this._valueType, this._value, objB);
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00007D8F File Offset: 0x00005F8F
		public int CompareTo(JValue obj)
		{
			if (obj == null)
			{
				return 1;
			}
			return JValue.Compare(this._valueType, this._value, obj._value);
		}

		// Token: 0x04000084 RID: 132
		private JTokenType _valueType;

		// Token: 0x04000085 RID: 133
		private object _value;

		// Token: 0x0200002A RID: 42
		private class JValueDynamicProxy : DynamicProxy<JValue>
		{
			// Token: 0x060001CB RID: 459 RVA: 0x00007DB0 File Offset: 0x00005FB0
			public override bool TryConvert(JValue instance, ConvertBinder binder, out object result)
			{
				if (binder.Type == typeof(JValue))
				{
					result = instance;
					return true;
				}
				if (instance.Value == null)
				{
					result = null;
					return ReflectionUtils.IsNullable(binder.Type);
				}
				result = ConvertUtils.Convert(instance.Value, CultureInfo.InvariantCulture, binder.Type);
				return true;
			}

			// Token: 0x060001CC RID: 460 RVA: 0x00007E0C File Offset: 0x0000600C
			public override bool TryBinaryOperation(JValue instance, BinaryOperationBinder binder, object arg, out object result)
			{
				object objB = (arg is JValue) ? ((JValue)arg).Value : arg;
				ExpressionType operation = binder.Operation;
				switch (operation)
				{
				case ExpressionType.Equal:
					result = (JValue.Compare(instance.Type, instance.Value, objB) == 0);
					return true;
				case ExpressionType.ExclusiveOr:
				case ExpressionType.Invoke:
				case ExpressionType.Lambda:
				case ExpressionType.LeftShift:
					break;
				case ExpressionType.GreaterThan:
					result = (JValue.Compare(instance.Type, instance.Value, objB) > 0);
					return true;
				case ExpressionType.GreaterThanOrEqual:
					result = (JValue.Compare(instance.Type, instance.Value, objB) >= 0);
					return true;
				case ExpressionType.LessThan:
					result = (JValue.Compare(instance.Type, instance.Value, objB) < 0);
					return true;
				case ExpressionType.LessThanOrEqual:
					result = (JValue.Compare(instance.Type, instance.Value, objB) <= 0);
					return true;
				default:
					if (operation == ExpressionType.NotEqual)
					{
						result = (JValue.Compare(instance.Type, instance.Value, objB) != 0);
						return true;
					}
					break;
				}
				result = null;
				return false;
			}
		}
	}
}

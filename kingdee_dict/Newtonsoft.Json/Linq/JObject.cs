using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq.ComponentModel;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200006A RID: 106
	public class JObject : JContainer, IDictionary<string, JToken>, ICollection<KeyValuePair<string, JToken>>, IEnumerable<KeyValuePair<string, JToken>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
	{
		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060004BE RID: 1214 RVA: 0x00010B08 File Offset: 0x0000ED08
		// (remove) Token: 0x060004BF RID: 1215 RVA: 0x00010B40 File Offset: 0x0000ED40
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060004C0 RID: 1216 RVA: 0x00010B78 File Offset: 0x0000ED78
		// (remove) Token: 0x060004C1 RID: 1217 RVA: 0x00010BB0 File Offset: 0x0000EDB0
		public event PropertyChangingEventHandler PropertyChanging;

		// Token: 0x060004C2 RID: 1218 RVA: 0x00010BE5 File Offset: 0x0000EDE5
		public JObject()
		{
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00010BED File Offset: 0x0000EDED
		public JObject(JObject other) : base(other)
		{
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00010BF6 File Offset: 0x0000EDF6
		public JObject(params object[] content) : this(content)
		{
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00010BFF File Offset: 0x0000EDFF
		public JObject(object content)
		{
			base.Add(content);
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x00010C10 File Offset: 0x0000EE10
		internal override bool DeepEquals(JToken node)
		{
			JObject jobject = node as JObject;
			return jobject != null && base.ContentsEqual(jobject);
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x00010C30 File Offset: 0x0000EE30
		internal override void ValidateToken(JToken o, JToken existing)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			if (o.Type != JTokenType.Property)
			{
				throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					o.GetType(),
					base.GetType()
				}));
			}
			JProperty jproperty = (JProperty)o;
			foreach (JToken jtoken in this.Children())
			{
				JProperty jproperty2 = (JProperty)jtoken;
				if (jproperty2 != existing && string.Equals(jproperty2.Name, jproperty.Name, StringComparison.Ordinal))
				{
					throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						jproperty.Name,
						base.GetType()
					}));
				}
			}
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x00010D18 File Offset: 0x0000EF18
		internal void InternalPropertyChanged(JProperty childProperty)
		{
			this.OnPropertyChanged(childProperty.Name);
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, base.IndexOfItem(childProperty)));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, childProperty, childProperty, base.IndexOfItem(childProperty)));
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x00010D4E File Offset: 0x0000EF4E
		internal void InternalPropertyChanging(JProperty childProperty)
		{
			this.OnPropertyChanging(childProperty.Name);
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x00010D5C File Offset: 0x0000EF5C
		internal override JToken CloneToken()
		{
			return new JObject(this);
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060004CB RID: 1227 RVA: 0x00010D64 File Offset: 0x0000EF64
		public override JTokenType Type
		{
			get
			{
				return JTokenType.Object;
			}
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x00010D67 File Offset: 0x0000EF67
		public IEnumerable<JProperty> Properties()
		{
			return this.Children().Cast<JProperty>();
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x00010D98 File Offset: 0x0000EF98
		public JProperty Property(string name)
		{
			return (from p in this.Properties()
			where string.Equals(p.Name, name, StringComparison.Ordinal)
			select p).SingleOrDefault<JProperty>();
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00010DD6 File Offset: 0x0000EFD6
		public JEnumerable<JToken> PropertyValues()
		{
			return new JEnumerable<JToken>(from p in this.Properties()
			select p.Value);
		}

		// Token: 0x170000F2 RID: 242
		public override JToken this[object key]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				string text = key as string;
				if (text == null)
				{
					throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				return this[text];
			}
			set
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				string text = key as string;
				if (text == null)
				{
					throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				this[text] = value;
			}
		}

		// Token: 0x170000F3 RID: 243
		public JToken this[string propertyName]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
				JProperty jproperty = this.Property(propertyName);
				if (jproperty == null)
				{
					return null;
				}
				return jproperty.Value;
			}
			set
			{
				JProperty jproperty = this.Property(propertyName);
				if (jproperty != null)
				{
					jproperty.Value = value;
					return;
				}
				this.OnPropertyChanging(propertyName);
				base.Add(new JProperty(propertyName, value));
				this.OnPropertyChanged(propertyName);
			}
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x00010F10 File Offset: 0x0000F110
		public new static JObject Load(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JObject from JsonReader.");
			}
			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
			JObject jobject = new JObject();
			jobject.SetLineInfo(reader as IJsonLineInfo);
			jobject.ReadTokenFrom(reader);
			return jobject;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x00010F94 File Offset: 0x0000F194
		public new static JObject Parse(string json)
		{
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return JObject.Load(reader);
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00010FB3 File Offset: 0x0000F1B3
		public new static JObject FromObject(object o)
		{
			return JObject.FromObject(o, new JsonSerializer());
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x00010FC0 File Offset: 0x0000F1C0
		public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
		{
			JToken jtoken = JToken.FromObjectInternal(o, jsonSerializer);
			if (jtoken != null && jtoken.Type != JTokenType.Object)
			{
				throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					jtoken.Type
				}));
			}
			return (JObject)jtoken;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00011014 File Offset: 0x0000F214
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartObject();
			foreach (JToken jtoken in base.ChildrenInternal())
			{
				JProperty jproperty = (JProperty)jtoken;
				jproperty.WriteTo(writer, converters);
			}
			writer.WriteEndObject();
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00011074 File Offset: 0x0000F274
		public void Add(string propertyName, JToken value)
		{
			base.Add(new JProperty(propertyName, value));
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00011083 File Offset: 0x0000F283
		bool IDictionary<string, JToken>.ContainsKey(string key)
		{
			return this.Property(key) != null;
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060004DA RID: 1242 RVA: 0x00011092 File Offset: 0x0000F292
		ICollection<string> IDictionary<string, JToken>.Keys
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0001109C File Offset: 0x0000F29C
		public bool Remove(string propertyName)
		{
			JProperty jproperty = this.Property(propertyName);
			if (jproperty == null)
			{
				return false;
			}
			jproperty.Remove();
			return true;
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x000110C0 File Offset: 0x0000F2C0
		public bool TryGetValue(string propertyName, out JToken value)
		{
			JProperty jproperty = this.Property(propertyName);
			if (jproperty == null)
			{
				value = null;
				return false;
			}
			value = jproperty.Value;
			return true;
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060004DD RID: 1245 RVA: 0x000110E6 File Offset: 0x0000F2E6
		ICollection<JToken> IDictionary<string, JToken>.Values
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x000110ED File Offset: 0x0000F2ED
		void ICollection<KeyValuePair<string, JToken>>.Add(KeyValuePair<string, JToken> item)
		{
			base.Add(new JProperty(item.Key, item.Value));
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x00011108 File Offset: 0x0000F308
		void ICollection<KeyValuePair<string, JToken>>.Clear()
		{
			base.RemoveAll();
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x00011110 File Offset: 0x0000F310
		bool ICollection<KeyValuePair<string, JToken>>.Contains(KeyValuePair<string, JToken> item)
		{
			JProperty jproperty = this.Property(item.Key);
			return jproperty != null && jproperty.Value == item.Value;
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00011140 File Offset: 0x0000F340
		void ICollection<KeyValuePair<string, JToken>>.CopyTo(KeyValuePair<string, JToken>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
			}
			if (arrayIndex >= array.Length)
			{
				throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
			}
			if (this.Count > array.Length - arrayIndex)
			{
				throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
			}
			int num = 0;
			foreach (JProperty jproperty in this.Properties())
			{
				array[arrayIndex + num] = new KeyValuePair<string, JToken>(jproperty.Name, jproperty.Value);
				num++;
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x000111F8 File Offset: 0x0000F3F8
		public int Count
		{
			get
			{
				return this.Children().Count<JToken>();
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x0001120A File Offset: 0x0000F40A
		bool ICollection<KeyValuePair<string, JToken>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0001120D File Offset: 0x0000F40D
		bool ICollection<KeyValuePair<string, JToken>>.Remove(KeyValuePair<string, JToken> item)
		{
			if (!((ICollection<KeyValuePair<string, JToken>>)this).Contains(item))
			{
				return false;
			}
			((IDictionary<string, JToken>)this).Remove(item.Key);
			return true;
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x00011229 File Offset: 0x0000F429
		internal override int GetDeepHashCode()
		{
			return base.ContentsHashCode();
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x00011380 File Offset: 0x0000F580
		public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()
		{
			foreach (JProperty property in this.Properties())
			{
				yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
			}
			yield break;
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0001139C File Offset: 0x0000F59C
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x000113B8 File Offset: 0x0000F5B8
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (this.PropertyChanging != null)
			{
				this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x000113D4 File Offset: 0x0000F5D4
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(null);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x000113E0 File Offset: 0x0000F5E0
		private static Type GetTokenPropertyType(JToken token)
		{
			if (!(token is JValue))
			{
				return token.GetType();
			}
			JValue jvalue = (JValue)token;
			if (jvalue.Value == null)
			{
				return typeof(object);
			}
			return jvalue.Value.GetType();
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00011424 File Offset: 0x0000F624
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);
			foreach (KeyValuePair<string, JToken> keyValuePair in this)
			{
				propertyDescriptorCollection.Add(new JPropertyDescriptor(keyValuePair.Key, JObject.GetTokenPropertyType(keyValuePair.Value)));
			}
			return propertyDescriptorCollection;
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0001148C File Offset: 0x0000F68C
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return AttributeCollection.Empty;
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x00011493 File Offset: 0x0000F693
		string ICustomTypeDescriptor.GetClassName()
		{
			return null;
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x00011496 File Offset: 0x0000F696
		string ICustomTypeDescriptor.GetComponentName()
		{
			return null;
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x00011499 File Offset: 0x0000F699
		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return new TypeConverter();
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x000114A0 File Offset: 0x0000F6A0
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return null;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x000114A3 File Offset: 0x0000F6A3
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return null;
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x000114A6 File Offset: 0x0000F6A6
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return null;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x000114A9 File Offset: 0x0000F6A9
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return EventDescriptorCollection.Empty;
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x000114B0 File Offset: 0x0000F6B0
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return EventDescriptorCollection.Empty;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x000114B7 File Offset: 0x0000F6B7
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return null;
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x000114BA File Offset: 0x0000F6BA
		protected override DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new DynamicProxyMetaObject<JObject>(parameter, this, new JObject.JObjectDynamicProxy(), true);
		}

		// Token: 0x0200006B RID: 107
		private class JObjectDynamicProxy : DynamicProxy<JObject>
		{
			// Token: 0x060004F8 RID: 1272 RVA: 0x000114C9 File Offset: 0x0000F6C9
			public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object result)
			{
				result = instance[binder.Name];
				return true;
			}

			// Token: 0x060004F9 RID: 1273 RVA: 0x000114DC File Offset: 0x0000F6DC
			public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
			{
				JToken jtoken = value as JToken;
				if (jtoken == null)
				{
					jtoken = new JValue(value);
				}
				instance[binder.Name] = jtoken;
				return true;
			}

			// Token: 0x060004FA RID: 1274 RVA: 0x00011510 File Offset: 0x0000F710
			public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
			{
				return from p in instance.Properties()
				select p.Name;
			}
		}
	}
}

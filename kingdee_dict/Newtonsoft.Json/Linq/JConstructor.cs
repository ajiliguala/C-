using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000068 RID: 104
	public class JConstructor : JContainer
	{
		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060004A8 RID: 1192 RVA: 0x00010841 File Offset: 0x0000EA41
		// (set) Token: 0x060004A9 RID: 1193 RVA: 0x00010849 File Offset: 0x0000EA49
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060004AA RID: 1194 RVA: 0x00010852 File Offset: 0x0000EA52
		public override JTokenType Type
		{
			get
			{
				return JTokenType.Constructor;
			}
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x00010855 File Offset: 0x0000EA55
		public JConstructor()
		{
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x0001085D File Offset: 0x0000EA5D
		public JConstructor(JConstructor other) : base(other)
		{
			this._name = other.Name;
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00010872 File Offset: 0x0000EA72
		public JConstructor(string name, params object[] content) : this(name, content)
		{
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x0001087C File Offset: 0x0000EA7C
		public JConstructor(string name, object content) : this(name)
		{
			base.Add(content);
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0001088C File Offset: 0x0000EA8C
		public JConstructor(string name)
		{
			ValidationUtils.ArgumentNotNullOrEmpty(name, "name");
			this._name = name;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x000108A8 File Offset: 0x0000EAA8
		internal override bool DeepEquals(JToken node)
		{
			JConstructor jconstructor = node as JConstructor;
			return jconstructor != null && this._name == jconstructor.Name && base.ContentsEqual(jconstructor);
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x000108DB File Offset: 0x0000EADB
		internal override JToken CloneToken()
		{
			return new JConstructor(this);
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x000108E4 File Offset: 0x0000EAE4
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartConstructor(this._name);
			foreach (JToken jtoken in this.Children())
			{
				jtoken.WriteTo(writer, converters);
			}
			writer.WriteEndConstructor();
		}

		// Token: 0x170000EF RID: 239
		public override JToken this[object key]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				if (!(key is int))
				{
					throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				return this.GetItem((int)key);
			}
			set
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				if (!(key is int))
				{
					throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						MiscellaneousUtils.ToString(key)
					}));
				}
				this.SetItem((int)key, value);
			}
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x000109EF File Offset: 0x0000EBEF
		internal override int GetDeepHashCode()
		{
			return this._name.GetHashCode() ^ base.ContentsHashCode();
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x00010A04 File Offset: 0x0000EC04
		public new static JConstructor Load(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JConstructor from JsonReader.");
			}
			if (reader.TokenType != JsonToken.StartConstructor)
			{
				throw new Exception("Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					reader.TokenType
				}));
			}
			JConstructor jconstructor = new JConstructor((string)reader.Value);
			jconstructor.SetLineInfo(reader as IJsonLineInfo);
			jconstructor.ReadTokenFrom(reader);
			return jconstructor;
		}

		// Token: 0x0400013A RID: 314
		private string _name;
	}
}

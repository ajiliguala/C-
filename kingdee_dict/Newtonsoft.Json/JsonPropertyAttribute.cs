using System;

namespace Newtonsoft.Json
{
	// Token: 0x0200005D RID: 93
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class JsonPropertyAttribute : Attribute
	{
		// Token: 0x170000BA RID: 186
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0000D780 File Offset: 0x0000B980
		// (set) Token: 0x0600037D RID: 893 RVA: 0x0000D7A6 File Offset: 0x0000B9A6
		public NullValueHandling NullValueHandling
		{
			get
			{
				NullValueHandling? nullValueHandling = this._nullValueHandling;
				if (nullValueHandling == null)
				{
					return NullValueHandling.Include;
				}
				return nullValueHandling.GetValueOrDefault();
			}
			set
			{
				this._nullValueHandling = new NullValueHandling?(value);
			}
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0000D7B4 File Offset: 0x0000B9B4
		// (set) Token: 0x0600037F RID: 895 RVA: 0x0000D7DA File Offset: 0x0000B9DA
		public DefaultValueHandling DefaultValueHandling
		{
			get
			{
				DefaultValueHandling? defaultValueHandling = this._defaultValueHandling;
				if (defaultValueHandling == null)
				{
					return DefaultValueHandling.Include;
				}
				return defaultValueHandling.GetValueOrDefault();
			}
			set
			{
				this._defaultValueHandling = new DefaultValueHandling?(value);
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0000D7E8 File Offset: 0x0000B9E8
		// (set) Token: 0x06000381 RID: 897 RVA: 0x0000D80E File Offset: 0x0000BA0E
		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				ReferenceLoopHandling? referenceLoopHandling = this._referenceLoopHandling;
				if (referenceLoopHandling == null)
				{
					return ReferenceLoopHandling.Error;
				}
				return referenceLoopHandling.GetValueOrDefault();
			}
			set
			{
				this._referenceLoopHandling = new ReferenceLoopHandling?(value);
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0000D81C File Offset: 0x0000BA1C
		// (set) Token: 0x06000383 RID: 899 RVA: 0x0000D842 File Offset: 0x0000BA42
		public ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				ObjectCreationHandling? objectCreationHandling = this._objectCreationHandling;
				if (objectCreationHandling == null)
				{
					return ObjectCreationHandling.Auto;
				}
				return objectCreationHandling.GetValueOrDefault();
			}
			set
			{
				this._objectCreationHandling = new ObjectCreationHandling?(value);
			}
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0000D850 File Offset: 0x0000BA50
		// (set) Token: 0x06000385 RID: 901 RVA: 0x0000D876 File Offset: 0x0000BA76
		public TypeNameHandling TypeNameHandling
		{
			get
			{
				TypeNameHandling? typeNameHandling = this._typeNameHandling;
				if (typeNameHandling == null)
				{
					return TypeNameHandling.None;
				}
				return typeNameHandling.GetValueOrDefault();
			}
			set
			{
				this._typeNameHandling = new TypeNameHandling?(value);
			}
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000386 RID: 902 RVA: 0x0000D884 File Offset: 0x0000BA84
		// (set) Token: 0x06000387 RID: 903 RVA: 0x0000D8AA File Offset: 0x0000BAAA
		public bool IsReference
		{
			get
			{
				return this._isReference ?? false;
			}
			set
			{
				this._isReference = new bool?(value);
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000388 RID: 904 RVA: 0x0000D8B8 File Offset: 0x0000BAB8
		// (set) Token: 0x06000389 RID: 905 RVA: 0x0000D8C0 File Offset: 0x0000BAC0
		public string PropertyName { get; set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x0600038A RID: 906 RVA: 0x0000D8C9 File Offset: 0x0000BAC9
		// (set) Token: 0x0600038B RID: 907 RVA: 0x0000D8D1 File Offset: 0x0000BAD1
		public Required Required { get; set; }

		// Token: 0x0600038C RID: 908 RVA: 0x0000D8DA File Offset: 0x0000BADA
		public JsonPropertyAttribute()
		{
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0000D8E2 File Offset: 0x0000BAE2
		public JsonPropertyAttribute(string propertyName)
		{
			this.PropertyName = propertyName;
		}

		// Token: 0x0400010C RID: 268
		internal NullValueHandling? _nullValueHandling;

		// Token: 0x0400010D RID: 269
		internal DefaultValueHandling? _defaultValueHandling;

		// Token: 0x0400010E RID: 270
		internal ReferenceLoopHandling? _referenceLoopHandling;

		// Token: 0x0400010F RID: 271
		internal ObjectCreationHandling? _objectCreationHandling;

		// Token: 0x04000110 RID: 272
		internal TypeNameHandling? _typeNameHandling;

		// Token: 0x04000111 RID: 273
		internal bool? _isReference;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000085 RID: 133
	public class JsonDictionaryContract : JsonContract
	{
		// Token: 0x1700012A RID: 298
		// (get) Token: 0x0600061E RID: 1566 RVA: 0x00014F28 File Offset: 0x00013128
		// (set) Token: 0x0600061F RID: 1567 RVA: 0x00014F30 File Offset: 0x00013130
		internal Type DictionaryKeyType { get; private set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000620 RID: 1568 RVA: 0x00014F39 File Offset: 0x00013139
		// (set) Token: 0x06000621 RID: 1569 RVA: 0x00014F41 File Offset: 0x00013141
		internal Type DictionaryValueType { get; private set; }

		// Token: 0x06000622 RID: 1570 RVA: 0x00014F4C File Offset: 0x0001314C
		public JsonDictionaryContract(Type underlyingType) : base(underlyingType)
		{
			Type type;
			Type type2;
			if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<, >), out this._genericCollectionDefinitionType))
			{
				type = this._genericCollectionDefinitionType.GetGenericArguments()[0];
				type2 = this._genericCollectionDefinitionType.GetGenericArguments()[1];
			}
			else
			{
				ReflectionUtils.GetDictionaryKeyValueTypes(base.UnderlyingType, out type, out type2);
			}
			this.DictionaryKeyType = type;
			this.DictionaryValueType = type2;
			if (this.IsTypeGenericDictionaryInterface(base.UnderlyingType))
			{
				base.CreatedType = ReflectionUtils.MakeGenericType(typeof(Dictionary<, >), new Type[]
				{
					type,
					type2
				});
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00014FE8 File Offset: 0x000131E8
		internal IWrappedDictionary CreateWrapper(object dictionary)
		{
			if (dictionary is IDictionary)
			{
				return new DictionaryWrapper<object, object>((IDictionary)dictionary);
			}
			if (this._genericWrapperType == null)
			{
				this._genericWrapperType = ReflectionUtils.MakeGenericType(typeof(DictionaryWrapper<, >), new Type[]
				{
					this.DictionaryKeyType,
					this.DictionaryValueType
				});
				ConstructorInfo constructor = this._genericWrapperType.GetConstructor(new Type[]
				{
					this._genericCollectionDefinitionType
				});
				this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructor);
			}
			return (IWrappedDictionary)this._genericWrapperCreator(null, new object[]
			{
				dictionary
			});
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00015094 File Offset: 0x00013294
		private bool IsTypeGenericDictionaryInterface(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IDictionary<, >);
		}

		// Token: 0x040001A3 RID: 419
		private readonly Type _genericCollectionDefinitionType;

		// Token: 0x040001A4 RID: 420
		private Type _genericWrapperType;

		// Token: 0x040001A5 RID: 421
		private MethodCall<object, object> _genericWrapperCreator;
	}
}

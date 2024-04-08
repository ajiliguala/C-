using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000084 RID: 132
	public class JsonArrayContract : JsonContract
	{
		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000618 RID: 1560 RVA: 0x00014C98 File Offset: 0x00012E98
		// (set) Token: 0x06000619 RID: 1561 RVA: 0x00014CA0 File Offset: 0x00012EA0
		internal Type CollectionItemType { get; private set; }

		// Token: 0x0600061A RID: 1562 RVA: 0x00014CAC File Offset: 0x00012EAC
		public JsonArrayContract(Type underlyingType) : base(underlyingType)
		{
			if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out this._genericCollectionDefinitionType))
			{
				this.CollectionItemType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
			}
			else
			{
				this.CollectionItemType = ReflectionUtils.GetCollectionItemType(base.UnderlyingType);
			}
			if (this.CollectionItemType != null)
			{
				this._isCollectionItemTypeNullableType = ReflectionUtils.IsNullableType(this.CollectionItemType);
			}
			if (this.IsTypeGenericCollectionInterface(base.UnderlyingType))
			{
				base.CreatedType = ReflectionUtils.MakeGenericType(typeof(List<>), new Type[]
				{
					this.CollectionItemType
				});
			}
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00014D54 File Offset: 0x00012F54
		internal IWrappedCollection CreateWrapper(object list)
		{
			if ((list is IList && (this.CollectionItemType == null || !this._isCollectionItemTypeNullableType)) || base.UnderlyingType.IsArray)
			{
				return new CollectionWrapper<object>((IList)list);
			}
			if (this._genericCollectionDefinitionType != null)
			{
				this.EnsureGenericWrapperCreator();
				return (IWrappedCollection)this._genericWrapperCreator(null, new object[]
				{
					list
				});
			}
			IList list2 = ((IEnumerable)list).Cast<object>().ToList<object>();
			if (this.CollectionItemType != null)
			{
				Array array = Array.CreateInstance(this.CollectionItemType, list2.Count);
				for (int i = 0; i < list2.Count; i++)
				{
					array.SetValue(list2[i], i);
				}
				list2 = array;
			}
			return new CollectionWrapper<object>(list2);
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00014E24 File Offset: 0x00013024
		private void EnsureGenericWrapperCreator()
		{
			if (this._genericWrapperType == null)
			{
				this._genericWrapperType = ReflectionUtils.MakeGenericType(typeof(CollectionWrapper<>), new Type[]
				{
					this.CollectionItemType
				});
				Type type = ReflectionUtils.InheritsGenericDefinition(this._genericCollectionDefinitionType, typeof(List<>)) ? ReflectionUtils.MakeGenericType(typeof(ICollection<>), new Type[]
				{
					this.CollectionItemType
				}) : this._genericCollectionDefinitionType;
				ConstructorInfo constructor = this._genericWrapperType.GetConstructor(new Type[]
				{
					type
				});
				this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructor);
			}
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00014ED4 File Offset: 0x000130D4
		private bool IsTypeGenericCollectionInterface(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(IEnumerable<>);
		}

		// Token: 0x0400019E RID: 414
		private readonly bool _isCollectionItemTypeNullableType;

		// Token: 0x0400019F RID: 415
		private readonly Type _genericCollectionDefinitionType;

		// Token: 0x040001A0 RID: 416
		private Type _genericWrapperType;

		// Token: 0x040001A1 RID: 417
		private MethodCall<object, object> _genericWrapperCreator;
	}
}

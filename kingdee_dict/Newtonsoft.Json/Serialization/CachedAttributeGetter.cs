using System;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200009A RID: 154
	internal static class CachedAttributeGetter<T> where T : Attribute
	{
		// Token: 0x06000752 RID: 1874 RVA: 0x0001A09A File Offset: 0x0001829A
		public static T GetAttribute(ICustomAttributeProvider type)
		{
			return CachedAttributeGetter<T>.TypeAttributeCache.Get(type);
		}

		// Token: 0x04000242 RID: 578
		private static readonly ThreadSafeStore<ICustomAttributeProvider, T> TypeAttributeCache = new ThreadSafeStore<ICustomAttributeProvider, T>(new Func<ICustomAttributeProvider, T>(JsonTypeReflector.GetAttribute<T>));
	}
}

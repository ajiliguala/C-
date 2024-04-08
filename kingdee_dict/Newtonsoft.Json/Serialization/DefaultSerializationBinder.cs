using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000081 RID: 129
	public class DefaultSerializationBinder : SerializationBinder
	{
		// Token: 0x06000607 RID: 1543 RVA: 0x00014ADC File Offset: 0x00012CDC
		private static Type GetTypeFromTypeNameKey(DefaultSerializationBinder.TypeNameKey typeNameKey)
		{
			string assemblyName = typeNameKey.AssemblyName;
			string typeName = typeNameKey.TypeName;
			if (assemblyName == null)
			{
				return Type.GetType(typeName);
			}
			Assembly assembly = Assembly.LoadWithPartialName(assemblyName);
			if (assembly == null)
			{
				throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					assemblyName
				}));
			}
			Type type = assembly.GetType(typeName);
			if (type == null)
			{
				throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					typeName,
					assembly.FullName
				}));
			}
			return type;
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x00014B77 File Offset: 0x00012D77
		public override Type BindToType(string assemblyName, string typeName)
		{
			return this._typeCache.Get(new DefaultSerializationBinder.TypeNameKey(assemblyName, typeName));
		}

		// Token: 0x04000196 RID: 406
		internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

		// Token: 0x04000197 RID: 407
		private readonly ThreadSafeStore<DefaultSerializationBinder.TypeNameKey, Type> _typeCache = new ThreadSafeStore<DefaultSerializationBinder.TypeNameKey, Type>(new Func<DefaultSerializationBinder.TypeNameKey, Type>(DefaultSerializationBinder.GetTypeFromTypeNameKey));

		// Token: 0x02000082 RID: 130
		internal struct TypeNameKey : IEquatable<DefaultSerializationBinder.TypeNameKey>
		{
			// Token: 0x0600060B RID: 1547 RVA: 0x00014BB6 File Offset: 0x00012DB6
			public TypeNameKey(string assemblyName, string typeName)
			{
				this.AssemblyName = assemblyName;
				this.TypeName = typeName;
			}

			// Token: 0x0600060C RID: 1548 RVA: 0x00014BC6 File Offset: 0x00012DC6
			public override int GetHashCode()
			{
				return ((this.AssemblyName != null) ? this.AssemblyName.GetHashCode() : 0) ^ ((this.TypeName != null) ? this.TypeName.GetHashCode() : 0);
			}

			// Token: 0x0600060D RID: 1549 RVA: 0x00014BF5 File Offset: 0x00012DF5
			public override bool Equals(object obj)
			{
				return obj is DefaultSerializationBinder.TypeNameKey && this.Equals((DefaultSerializationBinder.TypeNameKey)obj);
			}

			// Token: 0x0600060E RID: 1550 RVA: 0x00014C0D File Offset: 0x00012E0D
			public bool Equals(DefaultSerializationBinder.TypeNameKey other)
			{
				return this.AssemblyName == other.AssemblyName && this.TypeName == other.TypeName;
			}

			// Token: 0x04000198 RID: 408
			internal readonly string AssemblyName;

			// Token: 0x04000199 RID: 409
			internal readonly string TypeName;
		}
	}
}

using System;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x02000037 RID: 55
	public interface IReferenceResolver
	{
		// Token: 0x06000220 RID: 544
		object ResolveReference(string reference);

		// Token: 0x06000221 RID: 545
		string GetReference(object value);

		// Token: 0x06000222 RID: 546
		bool IsReferenced(object value);

		// Token: 0x06000223 RID: 547
		void AddReference(string reference, object value);
	}
}

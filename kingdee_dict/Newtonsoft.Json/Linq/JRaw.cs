using System;
using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x0200002B RID: 43
	public class JRaw : JValue
	{
		// Token: 0x060001CE RID: 462 RVA: 0x00007F3E File Offset: 0x0000613E
		public JRaw(JRaw other) : base(other)
		{
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00007F47 File Offset: 0x00006147
		public JRaw(object rawJson) : base(rawJson, JTokenType.Raw)
		{
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00007F54 File Offset: 0x00006154
		public static JRaw Create(JsonReader reader)
		{
			JRaw result;
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
				{
					jsonTextWriter.WriteToken(reader);
					result = new JRaw(stringWriter.ToString());
				}
			}
			return result;
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00007FBC File Offset: 0x000061BC
		internal override JToken CloneToken()
		{
			return new JRaw(this);
		}
	}
}

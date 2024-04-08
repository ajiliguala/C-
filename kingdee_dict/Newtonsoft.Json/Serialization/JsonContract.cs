using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
	// Token: 0x0200002D RID: 45
	public abstract class JsonContract
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060001D2 RID: 466 RVA: 0x00007FC4 File Offset: 0x000061C4
		// (set) Token: 0x060001D3 RID: 467 RVA: 0x00007FCC File Offset: 0x000061CC
		public Type UnderlyingType { get; private set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x00007FD5 File Offset: 0x000061D5
		// (set) Token: 0x060001D5 RID: 469 RVA: 0x00007FDD File Offset: 0x000061DD
		public Type CreatedType { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x00007FE6 File Offset: 0x000061E6
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x00007FEE File Offset: 0x000061EE
		public bool? IsReference { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x00007FF7 File Offset: 0x000061F7
		// (set) Token: 0x060001D9 RID: 473 RVA: 0x00007FFF File Offset: 0x000061FF
		public JsonConverter Converter { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060001DA RID: 474 RVA: 0x00008008 File Offset: 0x00006208
		// (set) Token: 0x060001DB RID: 475 RVA: 0x00008010 File Offset: 0x00006210
		internal JsonConverter InternalConverter { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060001DC RID: 476 RVA: 0x00008019 File Offset: 0x00006219
		// (set) Token: 0x060001DD RID: 477 RVA: 0x00008021 File Offset: 0x00006221
		public MethodInfo OnDeserialized { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060001DE RID: 478 RVA: 0x0000802A File Offset: 0x0000622A
		// (set) Token: 0x060001DF RID: 479 RVA: 0x00008032 File Offset: 0x00006232
		public MethodInfo OnDeserializing { get; set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x0000803B File Offset: 0x0000623B
		// (set) Token: 0x060001E1 RID: 481 RVA: 0x00008043 File Offset: 0x00006243
		public MethodInfo OnSerialized { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060001E2 RID: 482 RVA: 0x0000804C File Offset: 0x0000624C
		// (set) Token: 0x060001E3 RID: 483 RVA: 0x00008054 File Offset: 0x00006254
		public MethodInfo OnSerializing { get; set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x0000805D File Offset: 0x0000625D
		// (set) Token: 0x060001E5 RID: 485 RVA: 0x00008065 File Offset: 0x00006265
		public Func<object> DefaultCreator { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x0000806E File Offset: 0x0000626E
		// (set) Token: 0x060001E7 RID: 487 RVA: 0x00008076 File Offset: 0x00006276
		public bool DefaultCreatorNonPublic { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060001E8 RID: 488 RVA: 0x0000807F File Offset: 0x0000627F
		// (set) Token: 0x060001E9 RID: 489 RVA: 0x00008087 File Offset: 0x00006287
		public MethodInfo OnError { get; set; }

		// Token: 0x060001EA RID: 490 RVA: 0x00008090 File Offset: 0x00006290
		internal void InvokeOnSerializing(object o, StreamingContext context)
		{
			if (this.OnSerializing != null)
			{
				this.OnSerializing.Invoke(o, new object[]
				{
					context
				});
			}
		}

		// Token: 0x060001EB RID: 491 RVA: 0x000080CC File Offset: 0x000062CC
		internal void InvokeOnSerialized(object o, StreamingContext context)
		{
			if (this.OnSerialized != null)
			{
				this.OnSerialized.Invoke(o, new object[]
				{
					context
				});
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x00008108 File Offset: 0x00006308
		internal void InvokeOnDeserializing(object o, StreamingContext context)
		{
			if (this.OnDeserializing != null)
			{
				this.OnDeserializing.Invoke(o, new object[]
				{
					context
				});
			}
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00008144 File Offset: 0x00006344
		internal void InvokeOnDeserialized(object o, StreamingContext context)
		{
			if (this.OnDeserialized != null)
			{
				this.OnDeserialized.Invoke(o, new object[]
				{
					context
				});
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00008180 File Offset: 0x00006380
		internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
		{
			if (this.OnError != null)
			{
				this.OnError.Invoke(o, new object[]
				{
					context,
					errorContext
				});
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000081BD File Offset: 0x000063BD
		internal JsonContract(Type underlyingType)
		{
			ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
			this.UnderlyingType = underlyingType;
			this.CreatedType = underlyingType;
		}
	}
}

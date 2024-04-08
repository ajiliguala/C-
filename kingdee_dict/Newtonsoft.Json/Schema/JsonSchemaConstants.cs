using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x0200008D RID: 141
	internal static class JsonSchemaConstants
	{
		// Token: 0x040001EA RID: 490
		public const string TypePropertyName = "type";

		// Token: 0x040001EB RID: 491
		public const string PropertiesPropertyName = "properties";

		// Token: 0x040001EC RID: 492
		public const string ItemsPropertyName = "items";

		// Token: 0x040001ED RID: 493
		public const string RequiredPropertyName = "required";

		// Token: 0x040001EE RID: 494
		public const string PatternPropertiesPropertyName = "patternProperties";

		// Token: 0x040001EF RID: 495
		public const string AdditionalPropertiesPropertyName = "additionalProperties";

		// Token: 0x040001F0 RID: 496
		public const string RequiresPropertyName = "requires";

		// Token: 0x040001F1 RID: 497
		public const string IdentityPropertyName = "identity";

		// Token: 0x040001F2 RID: 498
		public const string MinimumPropertyName = "minimum";

		// Token: 0x040001F3 RID: 499
		public const string MaximumPropertyName = "maximum";

		// Token: 0x040001F4 RID: 500
		public const string ExclusiveMinimumPropertyName = "exclusiveMinimum";

		// Token: 0x040001F5 RID: 501
		public const string ExclusiveMaximumPropertyName = "exclusiveMaximum";

		// Token: 0x040001F6 RID: 502
		public const string MinimumItemsPropertyName = "minItems";

		// Token: 0x040001F7 RID: 503
		public const string MaximumItemsPropertyName = "maxItems";

		// Token: 0x040001F8 RID: 504
		public const string PatternPropertyName = "pattern";

		// Token: 0x040001F9 RID: 505
		public const string MaximumLengthPropertyName = "maxLength";

		// Token: 0x040001FA RID: 506
		public const string MinimumLengthPropertyName = "minLength";

		// Token: 0x040001FB RID: 507
		public const string EnumPropertyName = "enum";

		// Token: 0x040001FC RID: 508
		public const string OptionsPropertyName = "options";

		// Token: 0x040001FD RID: 509
		public const string ReadOnlyPropertyName = "readonly";

		// Token: 0x040001FE RID: 510
		public const string TitlePropertyName = "title";

		// Token: 0x040001FF RID: 511
		public const string DescriptionPropertyName = "description";

		// Token: 0x04000200 RID: 512
		public const string FormatPropertyName = "format";

		// Token: 0x04000201 RID: 513
		public const string DefaultPropertyName = "default";

		// Token: 0x04000202 RID: 514
		public const string TransientPropertyName = "transient";

		// Token: 0x04000203 RID: 515
		public const string DivisibleByPropertyName = "divisibleBy";

		// Token: 0x04000204 RID: 516
		public const string HiddenPropertyName = "hidden";

		// Token: 0x04000205 RID: 517
		public const string DisallowPropertyName = "disallow";

		// Token: 0x04000206 RID: 518
		public const string ExtendsPropertyName = "extends";

		// Token: 0x04000207 RID: 519
		public const string IdPropertyName = "id";

		// Token: 0x04000208 RID: 520
		public const string OptionValuePropertyName = "value";

		// Token: 0x04000209 RID: 521
		public const string OptionLabelPropertyName = "label";

		// Token: 0x0400020A RID: 522
		public const string ReferencePropertyName = "$ref";

		// Token: 0x0400020B RID: 523
		public static readonly IDictionary<string, JsonSchemaType> JsonSchemaTypeMapping = new Dictionary<string, JsonSchemaType>
		{
			{
				"string",
				JsonSchemaType.String
			},
			{
				"object",
				JsonSchemaType.Object
			},
			{
				"integer",
				JsonSchemaType.Integer
			},
			{
				"number",
				JsonSchemaType.Float
			},
			{
				"null",
				JsonSchemaType.Null
			},
			{
				"boolean",
				JsonSchemaType.Boolean
			},
			{
				"array",
				JsonSchemaType.Array
			},
			{
				"any",
				JsonSchemaType.Any
			}
		};
	}
}

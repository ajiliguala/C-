using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x0200008C RID: 140
	internal class JsonSchemaBuilder
	{
		// Token: 0x06000699 RID: 1689 RVA: 0x00015667 File Offset: 0x00013867
		private void Push(JsonSchema value)
		{
			this._currentSchema = value;
			this._stack.Add(value);
			this._resolver.LoadedSchemas.Add(value);
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00015690 File Offset: 0x00013890
		private JsonSchema Pop()
		{
			JsonSchema currentSchema = this._currentSchema;
			this._stack.RemoveAt(this._stack.Count - 1);
			this._currentSchema = this._stack.LastOrDefault<JsonSchema>();
			return currentSchema;
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x0600069B RID: 1691 RVA: 0x000156CE File Offset: 0x000138CE
		private JsonSchema CurrentSchema
		{
			get
			{
				return this._currentSchema;
			}
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x000156D6 File Offset: 0x000138D6
		public JsonSchemaBuilder(JsonSchemaResolver resolver)
		{
			this._stack = new List<JsonSchema>();
			this._resolver = resolver;
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x000156F0 File Offset: 0x000138F0
		internal JsonSchema Parse(JsonReader reader)
		{
			this._reader = reader;
			if (reader.TokenType == JsonToken.None)
			{
				this._reader.Read();
			}
			return this.BuildSchema();
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x00015714 File Offset: 0x00013914
		private JsonSchema BuildSchema()
		{
			if (this._reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Expected StartObject while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._reader.TokenType
				}));
			}
			this._reader.Read();
			if (this._reader.TokenType == JsonToken.EndObject)
			{
				this.Push(new JsonSchema());
				return this.Pop();
			}
			string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
			this._reader.Read();
			if (!(text == "$ref"))
			{
				this.Push(new JsonSchema());
				this.ProcessSchemaProperty(text);
				while (this._reader.Read() && this._reader.TokenType != JsonToken.EndObject)
				{
					text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
					this._reader.Read();
					this.ProcessSchemaProperty(text);
				}
				return this.Pop();
			}
			string text2 = (string)this._reader.Value;
			this._reader.Read();
			JsonSchema schema = this._resolver.GetSchema(text2);
			if (schema == null)
			{
				throw new Exception("Could not resolve schema reference for Id '{0}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text2
				}));
			}
			return schema;
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x00015870 File Offset: 0x00013A70
		private void ProcessSchemaProperty(string propertyName)
		{
			switch (propertyName)
			{
			case "type":
				this.CurrentSchema.Type = this.ProcessType();
				return;
			case "id":
				this.CurrentSchema.Id = (string)this._reader.Value;
				return;
			case "title":
				this.CurrentSchema.Title = (string)this._reader.Value;
				return;
			case "description":
				this.CurrentSchema.Description = (string)this._reader.Value;
				return;
			case "properties":
				this.ProcessProperties();
				return;
			case "items":
				this.ProcessItems();
				return;
			case "additionalProperties":
				this.ProcessAdditionalProperties();
				return;
			case "patternProperties":
				this.ProcessPatternProperties();
				return;
			case "required":
				this.CurrentSchema.Required = new bool?((bool)this._reader.Value);
				return;
			case "requires":
				this.CurrentSchema.Requires = (string)this._reader.Value;
				return;
			case "identity":
				this.ProcessIdentity();
				return;
			case "minimum":
				this.CurrentSchema.Minimum = new double?(Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "maximum":
				this.CurrentSchema.Maximum = new double?(Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "exclusiveMinimum":
				this.CurrentSchema.ExclusiveMinimum = new bool?((bool)this._reader.Value);
				return;
			case "exclusiveMaximum":
				this.CurrentSchema.ExclusiveMaximum = new bool?((bool)this._reader.Value);
				return;
			case "maxLength":
				this.CurrentSchema.MaximumLength = new int?(Convert.ToInt32(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "minLength":
				this.CurrentSchema.MinimumLength = new int?(Convert.ToInt32(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "maxItems":
				this.CurrentSchema.MaximumItems = new int?(Convert.ToInt32(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "minItems":
				this.CurrentSchema.MinimumItems = new int?(Convert.ToInt32(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "divisibleBy":
				this.CurrentSchema.DivisibleBy = new double?(Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture));
				return;
			case "disallow":
				this.CurrentSchema.Disallow = this.ProcessType();
				return;
			case "default":
				this.ProcessDefault();
				return;
			case "hidden":
				this.CurrentSchema.Hidden = new bool?((bool)this._reader.Value);
				return;
			case "readonly":
				this.CurrentSchema.ReadOnly = new bool?((bool)this._reader.Value);
				return;
			case "format":
				this.CurrentSchema.Format = (string)this._reader.Value;
				return;
			case "pattern":
				this.CurrentSchema.Pattern = (string)this._reader.Value;
				return;
			case "options":
				this.ProcessOptions();
				return;
			case "enum":
				this.ProcessEnum();
				return;
			case "extends":
				this.ProcessExtends();
				return;
			}
			this._reader.Skip();
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x00015D67 File Offset: 0x00013F67
		private void ProcessExtends()
		{
			this.CurrentSchema.Extends = this.BuildSchema();
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x00015D7C File Offset: 0x00013F7C
		private void ProcessEnum()
		{
			if (this._reader.TokenType != JsonToken.StartArray)
			{
				throw new Exception("Expected StartArray token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._reader.TokenType
				}));
			}
			this.CurrentSchema.Enum = new List<JToken>();
			while (this._reader.Read() && this._reader.TokenType != JsonToken.EndArray)
			{
				JToken item = JToken.ReadFrom(this._reader);
				this.CurrentSchema.Enum.Add(item);
			}
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x00015E14 File Offset: 0x00014014
		private void ProcessOptions()
		{
			this.CurrentSchema.Options = new Dictionary<JToken, string>(new JTokenEqualityComparer());
			JsonToken tokenType = this._reader.TokenType;
			if (tokenType != JsonToken.StartArray)
			{
				throw new Exception("Expected array token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._reader.TokenType
				}));
			}
			while (this._reader.Read())
			{
				if (this._reader.TokenType == JsonToken.EndArray)
				{
					return;
				}
				if (this._reader.TokenType != JsonToken.StartObject)
				{
					throw new Exception("Expect object token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						this._reader.TokenType
					}));
				}
				string value = null;
				JToken jtoken = null;
				while (this._reader.Read() && this._reader.TokenType != JsonToken.EndObject)
				{
					string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
					this._reader.Read();
					string a;
					if ((a = text) != null)
					{
						if (a == "value")
						{
							jtoken = JToken.ReadFrom(this._reader);
							continue;
						}
						if (a == "label")
						{
							value = (string)this._reader.Value;
							continue;
						}
					}
					throw new Exception("Unexpected property in JSON schema option: {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						text
					}));
				}
				if (jtoken == null)
				{
					throw new Exception("No value specified for JSON schema option.");
				}
				if (this.CurrentSchema.Options.ContainsKey(jtoken))
				{
					throw new Exception("Duplicate value in JSON schema option collection: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						jtoken
					}));
				}
				this.CurrentSchema.Options.Add(jtoken, value);
			}
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x00015FEC File Offset: 0x000141EC
		private void ProcessDefault()
		{
			this.CurrentSchema.Default = JToken.ReadFrom(this._reader);
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x00016004 File Offset: 0x00014204
		private void ProcessIdentity()
		{
			this.CurrentSchema.Identity = new List<string>();
			JsonToken tokenType = this._reader.TokenType;
			if (tokenType == JsonToken.StartArray)
			{
				while (this._reader.Read())
				{
					if (this._reader.TokenType == JsonToken.EndArray)
					{
						return;
					}
					if (this._reader.TokenType != JsonToken.String)
					{
						throw new Exception("Exception JSON property name string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							this._reader.TokenType
						}));
					}
					this.CurrentSchema.Identity.Add(this._reader.Value.ToString());
				}
				return;
			}
			if (tokenType == JsonToken.String)
			{
				this.CurrentSchema.Identity.Add(this._reader.Value.ToString());
				return;
			}
			throw new Exception("Expected array or JSON property name string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				this._reader.TokenType
			}));
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x00016109 File Offset: 0x00014309
		private void ProcessAdditionalProperties()
		{
			if (this._reader.TokenType == JsonToken.Boolean)
			{
				this.CurrentSchema.AllowAdditionalProperties = (bool)this._reader.Value;
				return;
			}
			this.CurrentSchema.AdditionalProperties = this.BuildSchema();
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00016148 File Offset: 0x00014348
		private void ProcessPatternProperties()
		{
			Dictionary<string, JsonSchema> dictionary = new Dictionary<string, JsonSchema>();
			if (this._reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Expected start object token.");
			}
			while (this._reader.Read() && this._reader.TokenType != JsonToken.EndObject)
			{
				string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
				this._reader.Read();
				if (dictionary.ContainsKey(text))
				{
					throw new Exception("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						text
					}));
				}
				dictionary.Add(text, this.BuildSchema());
			}
			this.CurrentSchema.PatternProperties = dictionary;
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x000161F8 File Offset: 0x000143F8
		private void ProcessItems()
		{
			this.CurrentSchema.Items = new List<JsonSchema>();
			switch (this._reader.TokenType)
			{
			case JsonToken.StartObject:
				this.CurrentSchema.Items.Add(this.BuildSchema());
				return;
			case JsonToken.StartArray:
				while (this._reader.Read())
				{
					if (this._reader.TokenType == JsonToken.EndArray)
					{
						return;
					}
					this.CurrentSchema.Items.Add(this.BuildSchema());
				}
				return;
			default:
				throw new Exception("Expected array or JSON schema object token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._reader.TokenType
				}));
			}
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x000162B0 File Offset: 0x000144B0
		private void ProcessProperties()
		{
			IDictionary<string, JsonSchema> dictionary = new Dictionary<string, JsonSchema>();
			if (this._reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Expected StartObject token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					this._reader.TokenType
				}));
			}
			while (this._reader.Read() && this._reader.TokenType != JsonToken.EndObject)
			{
				string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
				this._reader.Read();
				if (dictionary.ContainsKey(text))
				{
					throw new Exception("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						text
					}));
				}
				dictionary.Add(text, this.BuildSchema());
			}
			this.CurrentSchema.Properties = dictionary;
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00016388 File Offset: 0x00014588
		private JsonSchemaType? ProcessType()
		{
			JsonToken tokenType = this._reader.TokenType;
			if (tokenType == JsonToken.StartArray)
			{
				JsonSchemaType? jsonSchemaType = new JsonSchemaType?(JsonSchemaType.None);
				while (this._reader.Read() && this._reader.TokenType != JsonToken.EndArray)
				{
					if (this._reader.TokenType != JsonToken.String)
					{
						throw new Exception("Exception JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							this._reader.TokenType
						}));
					}
					jsonSchemaType |= JsonSchemaBuilder.MapType(this._reader.Value.ToString());
				}
				return jsonSchemaType;
			}
			if (tokenType == JsonToken.String)
			{
				return new JsonSchemaType?(JsonSchemaBuilder.MapType(this._reader.Value.ToString()));
			}
			throw new Exception("Expected array or JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				this._reader.TokenType
			}));
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x000164A4 File Offset: 0x000146A4
		internal static JsonSchemaType MapType(string type)
		{
			JsonSchemaType result;
			if (!JsonSchemaConstants.JsonSchemaTypeMapping.TryGetValue(type, out result))
			{
				throw new Exception("Invalid JSON schema type: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					type
				}));
			}
			return result;
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x000164FC File Offset: 0x000146FC
		internal static string MapType(JsonSchemaType type)
		{
			return JsonSchemaConstants.JsonSchemaTypeMapping.Single((KeyValuePair<string, JsonSchemaType> kv) => kv.Value == type).Key;
		}

		// Token: 0x040001E6 RID: 486
		private JsonReader _reader;

		// Token: 0x040001E7 RID: 487
		private readonly IList<JsonSchema> _stack;

		// Token: 0x040001E8 RID: 488
		private readonly JsonSchemaResolver _resolver;

		// Token: 0x040001E9 RID: 489
		private JsonSchema _currentSchema;
	}
}

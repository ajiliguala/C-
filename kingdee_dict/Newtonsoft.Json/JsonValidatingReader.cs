using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x02000041 RID: 65
	public class JsonValidatingReader : JsonReader, IJsonLineInfo
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000264 RID: 612 RVA: 0x00008948 File Offset: 0x00006B48
		// (remove) Token: 0x06000265 RID: 613 RVA: 0x00008980 File Offset: 0x00006B80
		public event ValidationEventHandler ValidationEventHandler;

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000266 RID: 614 RVA: 0x000089B5 File Offset: 0x00006BB5
		public override object Value
		{
			get
			{
				return this._reader.Value;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000267 RID: 615 RVA: 0x000089C2 File Offset: 0x00006BC2
		public override int Depth
		{
			get
			{
				return this._reader.Depth;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000268 RID: 616 RVA: 0x000089CF File Offset: 0x00006BCF
		// (set) Token: 0x06000269 RID: 617 RVA: 0x000089DC File Offset: 0x00006BDC
		public override char QuoteChar
		{
			get
			{
				return this._reader.QuoteChar;
			}
			protected internal set
			{
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x0600026A RID: 618 RVA: 0x000089DE File Offset: 0x00006BDE
		public override JsonToken TokenType
		{
			get
			{
				return this._reader.TokenType;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600026B RID: 619 RVA: 0x000089EB File Offset: 0x00006BEB
		public override Type ValueType
		{
			get
			{
				return this._reader.ValueType;
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x000089F8 File Offset: 0x00006BF8
		private void Push(JsonValidatingReader.SchemaScope scope)
		{
			this._stack.Push(scope);
			this._currentScope = scope;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00008A10 File Offset: 0x00006C10
		private JsonValidatingReader.SchemaScope Pop()
		{
			JsonValidatingReader.SchemaScope result = this._stack.Pop();
			this._currentScope = ((this._stack.Count != 0) ? this._stack.Peek() : null);
			return result;
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00008A4B File Offset: 0x00006C4B
		private IEnumerable<JsonSchemaModel> CurrentSchemas
		{
			get
			{
				return this._currentScope.Schemas;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600026F RID: 623 RVA: 0x00008A58 File Offset: 0x00006C58
		private IEnumerable<JsonSchemaModel> CurrentMemberSchemas
		{
			get
			{
				if (this._currentScope == null)
				{
					return new List<JsonSchemaModel>(new JsonSchemaModel[]
					{
						this._model
					});
				}
				if (this._currentScope.Schemas == null || this._currentScope.Schemas.Count == 0)
				{
					return Enumerable.Empty<JsonSchemaModel>();
				}
				switch (this._currentScope.TokenType)
				{
				case JTokenType.None:
					return this._currentScope.Schemas;
				case JTokenType.Object:
				{
					if (this._currentScope.CurrentPropertyName == null)
					{
						throw new Exception("CurrentPropertyName has not been set on scope.");
					}
					IList<JsonSchemaModel> list = new List<JsonSchemaModel>();
					foreach (JsonSchemaModel jsonSchemaModel in this.CurrentSchemas)
					{
						JsonSchemaModel item;
						if (jsonSchemaModel.Properties != null && jsonSchemaModel.Properties.TryGetValue(this._currentScope.CurrentPropertyName, out item))
						{
							list.Add(item);
						}
						if (jsonSchemaModel.PatternProperties != null)
						{
							foreach (KeyValuePair<string, JsonSchemaModel> keyValuePair in jsonSchemaModel.PatternProperties)
							{
								if (Regex.IsMatch(this._currentScope.CurrentPropertyName, keyValuePair.Key))
								{
									list.Add(keyValuePair.Value);
								}
							}
						}
						if (list.Count == 0 && jsonSchemaModel.AllowAdditionalProperties && jsonSchemaModel.AdditionalProperties != null)
						{
							list.Add(jsonSchemaModel.AdditionalProperties);
						}
					}
					return list;
				}
				case JTokenType.Array:
				{
					IList<JsonSchemaModel> list2 = new List<JsonSchemaModel>();
					foreach (JsonSchemaModel jsonSchemaModel2 in this.CurrentSchemas)
					{
						if (!CollectionUtils.IsNullOrEmpty<JsonSchemaModel>(jsonSchemaModel2.Items))
						{
							if (jsonSchemaModel2.Items.Count == 1)
							{
								list2.Add(jsonSchemaModel2.Items[0]);
							}
							if (jsonSchemaModel2.Items.Count > this._currentScope.ArrayItemCount - 1)
							{
								list2.Add(jsonSchemaModel2.Items[this._currentScope.ArrayItemCount - 1]);
							}
						}
						if (jsonSchemaModel2.AllowAdditionalProperties && jsonSchemaModel2.AdditionalProperties != null)
						{
							list2.Add(jsonSchemaModel2.AdditionalProperties);
						}
					}
					return list2;
				}
				case JTokenType.Constructor:
					return Enumerable.Empty<JsonSchemaModel>();
				default:
					throw new ArgumentOutOfRangeException("TokenType", "Unexpected token type: {0}".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						this._currentScope.TokenType
					}));
				}
			}
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00008D10 File Offset: 0x00006F10
		private void RaiseError(string message, JsonSchemaModel schema)
		{
			string message2 = ((IJsonLineInfo)this).HasLineInfo() ? (message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				((IJsonLineInfo)this).LineNumber,
				((IJsonLineInfo)this).LinePosition
			})) : message;
			this.OnValidationEvent(new JsonSchemaException(message2, null, ((IJsonLineInfo)this).LineNumber, ((IJsonLineInfo)this).LinePosition));
		}

		// Token: 0x06000271 RID: 625 RVA: 0x00008D80 File Offset: 0x00006F80
		private void OnValidationEvent(JsonSchemaException exception)
		{
			ValidationEventHandler validationEventHandler = this.ValidationEventHandler;
			if (validationEventHandler != null)
			{
				validationEventHandler(this, new ValidationEventArgs(exception));
				return;
			}
			throw exception;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x00008DA6 File Offset: 0x00006FA6
		public JsonValidatingReader(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			this._reader = reader;
			this._stack = new Stack<JsonValidatingReader.SchemaScope>();
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000273 RID: 627 RVA: 0x00008DCB File Offset: 0x00006FCB
		// (set) Token: 0x06000274 RID: 628 RVA: 0x00008DD3 File Offset: 0x00006FD3
		public JsonSchema Schema
		{
			get
			{
				return this._schema;
			}
			set
			{
				if (this.TokenType != JsonToken.None)
				{
					throw new Exception("Cannot change schema while validating JSON.");
				}
				this._schema = value;
				this._model = null;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000275 RID: 629 RVA: 0x00008DF6 File Offset: 0x00006FF6
		public JsonReader Reader
		{
			get
			{
				return this._reader;
			}
		}

		// Token: 0x06000276 RID: 630 RVA: 0x00008E00 File Offset: 0x00007000
		private void ValidateInEnumAndNotDisallowed(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			JToken jtoken = new JValue(this._reader.Value);
			if (schema.Enum != null)
			{
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				jtoken.WriteTo(new JsonTextWriter(stringWriter), new JsonConverter[0]);
				if (!schema.Enum.ContainsValue(jtoken, new JTokenEqualityComparer()))
				{
					this.RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						stringWriter.ToString()
					}), schema);
				}
			}
			JsonSchemaType? currentNodeSchemaType = this.GetCurrentNodeSchemaType();
			if (currentNodeSchemaType != null && JsonSchemaGenerator.HasFlag(new JsonSchemaType?(schema.Disallow), currentNodeSchemaType.Value))
			{
				this.RaiseError("Type {0} is disallowed.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					currentNodeSchemaType
				}), schema);
			}
		}

		// Token: 0x06000277 RID: 631 RVA: 0x00008ED4 File Offset: 0x000070D4
		private JsonSchemaType? GetCurrentNodeSchemaType()
		{
			switch (this._reader.TokenType)
			{
			case JsonToken.StartObject:
				return new JsonSchemaType?(JsonSchemaType.Object);
			case JsonToken.StartArray:
				return new JsonSchemaType?(JsonSchemaType.Array);
			case JsonToken.Integer:
				return new JsonSchemaType?(JsonSchemaType.Integer);
			case JsonToken.Float:
				return new JsonSchemaType?(JsonSchemaType.Float);
			case JsonToken.String:
				return new JsonSchemaType?(JsonSchemaType.String);
			case JsonToken.Boolean:
				return new JsonSchemaType?(JsonSchemaType.Boolean);
			case JsonToken.Null:
				return new JsonSchemaType?(JsonSchemaType.Null);
			}
			return null;
		}

		// Token: 0x06000278 RID: 632 RVA: 0x00008F60 File Offset: 0x00007160
		public override byte[] ReadAsBytes()
		{
			byte[] result = this._reader.ReadAsBytes();
			this.ValidateCurrentToken();
			return result;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00008F80 File Offset: 0x00007180
		public override decimal? ReadAsDecimal()
		{
			decimal? result = this._reader.ReadAsDecimal();
			this.ValidateCurrentToken();
			return result;
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00008FA0 File Offset: 0x000071A0
		public override DateTimeOffset? ReadAsDateTimeOffset()
		{
			DateTimeOffset? result = this._reader.ReadAsDateTimeOffset();
			this.ValidateCurrentToken();
			return result;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00008FC0 File Offset: 0x000071C0
		public override bool Read()
		{
			if (!this._reader.Read())
			{
				return false;
			}
			if (this._reader.TokenType == JsonToken.Comment)
			{
				return true;
			}
			this.ValidateCurrentToken();
			return true;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00008FE8 File Offset: 0x000071E8
		private void ValidateCurrentToken()
		{
			if (this._model == null)
			{
				JsonSchemaModelBuilder jsonSchemaModelBuilder = new JsonSchemaModelBuilder();
				this._model = jsonSchemaModelBuilder.Build(this._schema);
			}
			switch (this._reader.TokenType)
			{
			case JsonToken.StartObject:
			{
				this.ProcessValue();
				IList<JsonSchemaModel> schemas = this.CurrentMemberSchemas.Where(new Func<JsonSchemaModel, bool>(this.ValidateObject)).ToList<JsonSchemaModel>();
				this.Push(new JsonValidatingReader.SchemaScope(JTokenType.Object, schemas));
				return;
			}
			case JsonToken.StartArray:
			{
				this.ProcessValue();
				IList<JsonSchemaModel> schemas2 = this.CurrentMemberSchemas.Where(new Func<JsonSchemaModel, bool>(this.ValidateArray)).ToList<JsonSchemaModel>();
				this.Push(new JsonValidatingReader.SchemaScope(JTokenType.Array, schemas2));
				return;
			}
			case JsonToken.StartConstructor:
				this.Push(new JsonValidatingReader.SchemaScope(JTokenType.Constructor, null));
				return;
			case JsonToken.PropertyName:
				using (IEnumerator<JsonSchemaModel> enumerator = this.CurrentSchemas.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						JsonSchemaModel schema = enumerator.Current;
						this.ValidatePropertyName(schema);
					}
					return;
				}
				break;
			case JsonToken.Comment:
				goto IL_2E9;
			case JsonToken.Raw:
			case JsonToken.Undefined:
			case JsonToken.Date:
				return;
			case JsonToken.Integer:
				break;
			case JsonToken.Float:
				goto IL_163;
			case JsonToken.String:
				goto IL_1A3;
			case JsonToken.Boolean:
				goto IL_1E3;
			case JsonToken.Null:
				goto IL_223;
			case JsonToken.EndObject:
				goto IL_263;
			case JsonToken.EndArray:
				foreach (JsonSchemaModel schema2 in this.CurrentSchemas)
				{
					this.ValidateEndArray(schema2);
				}
				this.Pop();
				return;
			case JsonToken.EndConstructor:
				this.Pop();
				return;
			default:
				goto IL_2E9;
			}
			this.ProcessValue();
			using (IEnumerator<JsonSchemaModel> enumerator3 = this.CurrentMemberSchemas.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					JsonSchemaModel schema3 = enumerator3.Current;
					this.ValidateInteger(schema3);
				}
				return;
			}
			IL_163:
			this.ProcessValue();
			using (IEnumerator<JsonSchemaModel> enumerator4 = this.CurrentMemberSchemas.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					JsonSchemaModel schema4 = enumerator4.Current;
					this.ValidateFloat(schema4);
				}
				return;
			}
			IL_1A3:
			this.ProcessValue();
			using (IEnumerator<JsonSchemaModel> enumerator5 = this.CurrentMemberSchemas.GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					JsonSchemaModel schema5 = enumerator5.Current;
					this.ValidateString(schema5);
				}
				return;
			}
			IL_1E3:
			this.ProcessValue();
			using (IEnumerator<JsonSchemaModel> enumerator6 = this.CurrentMemberSchemas.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					JsonSchemaModel schema6 = enumerator6.Current;
					this.ValidateBoolean(schema6);
				}
				return;
			}
			IL_223:
			this.ProcessValue();
			using (IEnumerator<JsonSchemaModel> enumerator7 = this.CurrentMemberSchemas.GetEnumerator())
			{
				while (enumerator7.MoveNext())
				{
					JsonSchemaModel schema7 = enumerator7.Current;
					this.ValidateNull(schema7);
				}
				return;
			}
			IL_263:
			foreach (JsonSchemaModel schema8 in this.CurrentSchemas)
			{
				this.ValidateEndObject(schema8);
			}
			this.Pop();
			return;
			IL_2E9:
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00009360 File Offset: 0x00007560
		private void ValidateEndObject(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			Dictionary<string, bool> requiredProperties = this._currentScope.RequiredProperties;
			if (requiredProperties != null)
			{
				List<string> list = (from kv in requiredProperties
				where !kv.Value
				select kv.Key).ToList<string>();
				if (list.Count > 0)
				{
					this.RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						string.Join(", ", list.ToArray())
					}), schema);
				}
			}
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00009408 File Offset: 0x00007608
		private void ValidateEndArray(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			int arrayItemCount = this._currentScope.ArrayItemCount;
			if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
			{
				this.RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					arrayItemCount,
					schema.MaximumItems
				}), schema);
			}
			if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
			{
				this.RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					arrayItemCount,
					schema.MinimumItems
				}), schema);
			}
		}

		// Token: 0x0600027F RID: 639 RVA: 0x000094F9 File Offset: 0x000076F9
		private void ValidateNull(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			if (!this.TestType(schema, JsonSchemaType.Null))
			{
				return;
			}
			this.ValidateInEnumAndNotDisallowed(schema);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00009512 File Offset: 0x00007712
		private void ValidateBoolean(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			if (!this.TestType(schema, JsonSchemaType.Boolean))
			{
				return;
			}
			this.ValidateInEnumAndNotDisallowed(schema);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000952C File Offset: 0x0000772C
		private void ValidateString(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			if (!this.TestType(schema, JsonSchemaType.String))
			{
				return;
			}
			this.ValidateInEnumAndNotDisallowed(schema);
			string text = this._reader.Value.ToString();
			if (schema.MaximumLength != null && text.Length > schema.MaximumLength)
			{
				this.RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text,
					schema.MaximumLength
				}), schema);
			}
			if (schema.MinimumLength != null && text.Length < schema.MinimumLength)
			{
				this.RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text,
					schema.MinimumLength
				}), schema);
			}
			if (schema.Patterns != null)
			{
				foreach (string text2 in schema.Patterns)
				{
					if (!Regex.IsMatch(text, text2))
					{
						this.RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							text,
							text2
						}), schema);
					}
				}
			}
		}

		// Token: 0x06000282 RID: 642 RVA: 0x000096B0 File Offset: 0x000078B0
		private void ValidateInteger(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			if (!this.TestType(schema, JsonSchemaType.Integer))
			{
				return;
			}
			this.ValidateInEnumAndNotDisallowed(schema);
			long num = Convert.ToInt64(this._reader.Value, CultureInfo.InvariantCulture);
			if (schema.Maximum != null)
			{
				double num2 = (double)num;
				double? maximum = schema.Maximum;
				if (num2 > maximum.GetValueOrDefault() && maximum != null)
				{
					this.RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						num,
						schema.Maximum
					}), schema);
				}
				if (schema.ExclusiveMaximum)
				{
					double num3 = (double)num;
					double? maximum2 = schema.Maximum;
					if (num3 == maximum2.GetValueOrDefault() && maximum2 != null)
					{
						this.RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							num,
							schema.Maximum
						}), schema);
					}
				}
			}
			if (schema.Minimum != null)
			{
				double num4 = (double)num;
				double? minimum = schema.Minimum;
				if (num4 < minimum.GetValueOrDefault() && minimum != null)
				{
					this.RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						num,
						schema.Minimum
					}), schema);
				}
				if (schema.ExclusiveMinimum)
				{
					double num5 = (double)num;
					double? minimum2 = schema.Minimum;
					if (num5 == minimum2.GetValueOrDefault() && minimum2 != null)
					{
						this.RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							num,
							schema.Minimum
						}), schema);
					}
				}
			}
			if (schema.DivisibleBy != null && !JsonValidatingReader.IsZero((double)num % schema.DivisibleBy.Value))
			{
				this.RaiseError("Integer {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JsonConvert.ToString(num),
					schema.DivisibleBy
				}), schema);
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x000098F4 File Offset: 0x00007AF4
		private void ProcessValue()
		{
			if (this._currentScope != null && this._currentScope.TokenType == JTokenType.Array)
			{
				this._currentScope.ArrayItemCount++;
				foreach (JsonSchemaModel jsonSchemaModel in this.CurrentSchemas)
				{
					if (jsonSchemaModel != null && jsonSchemaModel.Items != null && jsonSchemaModel.Items.Count > 1 && this._currentScope.ArrayItemCount >= jsonSchemaModel.Items.Count)
					{
						this.RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							this._currentScope.ArrayItemCount
						}), jsonSchemaModel);
					}
				}
			}
		}

		// Token: 0x06000284 RID: 644 RVA: 0x000099CC File Offset: 0x00007BCC
		private void ValidateFloat(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			if (!this.TestType(schema, JsonSchemaType.Float))
			{
				return;
			}
			this.ValidateInEnumAndNotDisallowed(schema);
			double num = Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture);
			if (schema.Maximum != null)
			{
				double num2 = num;
				double? maximum = schema.Maximum;
				if (num2 > maximum.GetValueOrDefault() && maximum != null)
				{
					this.RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						JsonConvert.ToString(num),
						schema.Maximum
					}), schema);
				}
				if (schema.ExclusiveMaximum)
				{
					double num3 = num;
					double? maximum2 = schema.Maximum;
					if (num3 == maximum2.GetValueOrDefault() && maximum2 != null)
					{
						this.RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							JsonConvert.ToString(num),
							schema.Maximum
						}), schema);
					}
				}
			}
			if (schema.Minimum != null)
			{
				double num4 = num;
				double? minimum = schema.Minimum;
				if (num4 < minimum.GetValueOrDefault() && minimum != null)
				{
					this.RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						JsonConvert.ToString(num),
						schema.Minimum
					}), schema);
				}
				if (schema.ExclusiveMinimum)
				{
					double num5 = num;
					double? minimum2 = schema.Minimum;
					if (num5 == minimum2.GetValueOrDefault() && minimum2 != null)
					{
						this.RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, new object[]
						{
							JsonConvert.ToString(num),
							schema.Minimum
						}), schema);
					}
				}
			}
			if (schema.DivisibleBy != null && !JsonValidatingReader.IsZero(num % schema.DivisibleBy.Value))
			{
				this.RaiseError("Float {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					JsonConvert.ToString(num),
					schema.DivisibleBy
				}), schema);
			}
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00009C0C File Offset: 0x00007E0C
		private static bool IsZero(double value)
		{
			double num = 2.220446049250313E-16;
			return Math.Abs(value) < 10.0 * num;
		}

		// Token: 0x06000286 RID: 646 RVA: 0x00009C38 File Offset: 0x00007E38
		private void ValidatePropertyName(JsonSchemaModel schema)
		{
			if (schema == null)
			{
				return;
			}
			string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
			if (this._currentScope.RequiredProperties.ContainsKey(text))
			{
				this._currentScope.RequiredProperties[text] = true;
			}
			if (!schema.AllowAdditionalProperties && !this.IsPropertyDefinied(schema, text))
			{
				this.RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					text
				}), schema);
			}
			this._currentScope.CurrentPropertyName = text;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00009CC8 File Offset: 0x00007EC8
		private bool IsPropertyDefinied(JsonSchemaModel schema, string propertyName)
		{
			if (schema.Properties != null && schema.Properties.ContainsKey(propertyName))
			{
				return true;
			}
			if (schema.PatternProperties != null)
			{
				foreach (string pattern in schema.PatternProperties.Keys)
				{
					if (Regex.IsMatch(propertyName, pattern))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00009D44 File Offset: 0x00007F44
		private bool ValidateArray(JsonSchemaModel schema)
		{
			return schema == null || this.TestType(schema, JsonSchemaType.Array);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00009D54 File Offset: 0x00007F54
		private bool ValidateObject(JsonSchemaModel schema)
		{
			return schema == null || this.TestType(schema, JsonSchemaType.Object);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x00009D64 File Offset: 0x00007F64
		private bool TestType(JsonSchemaModel currentSchema, JsonSchemaType currentType)
		{
			if (!JsonSchemaGenerator.HasFlag(new JsonSchemaType?(currentSchema.Type), currentType))
			{
				this.RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
				{
					currentSchema.Type,
					currentType
				}), currentSchema);
				return false;
			}
			return true;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x00009DBC File Offset: 0x00007FBC
		bool IJsonLineInfo.HasLineInfo()
		{
			IJsonLineInfo jsonLineInfo = this._reader as IJsonLineInfo;
			return jsonLineInfo != null && jsonLineInfo.HasLineInfo();
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600028C RID: 652 RVA: 0x00009DE0 File Offset: 0x00007FE0
		int IJsonLineInfo.LineNumber
		{
			get
			{
				IJsonLineInfo jsonLineInfo = this._reader as IJsonLineInfo;
				if (jsonLineInfo == null)
				{
					return 0;
				}
				return jsonLineInfo.LineNumber;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600028D RID: 653 RVA: 0x00009E04 File Offset: 0x00008004
		int IJsonLineInfo.LinePosition
		{
			get
			{
				IJsonLineInfo jsonLineInfo = this._reader as IJsonLineInfo;
				if (jsonLineInfo == null)
				{
					return 0;
				}
				return jsonLineInfo.LinePosition;
			}
		}

		// Token: 0x040000C9 RID: 201
		private readonly JsonReader _reader;

		// Token: 0x040000CA RID: 202
		private readonly Stack<JsonValidatingReader.SchemaScope> _stack;

		// Token: 0x040000CB RID: 203
		private JsonSchema _schema;

		// Token: 0x040000CC RID: 204
		private JsonSchemaModel _model;

		// Token: 0x040000CD RID: 205
		private JsonValidatingReader.SchemaScope _currentScope;

		// Token: 0x02000042 RID: 66
		private class SchemaScope
		{
			// Token: 0x1700006E RID: 110
			// (get) Token: 0x06000290 RID: 656 RVA: 0x00009E28 File Offset: 0x00008028
			// (set) Token: 0x06000291 RID: 657 RVA: 0x00009E30 File Offset: 0x00008030
			public string CurrentPropertyName { get; set; }

			// Token: 0x1700006F RID: 111
			// (get) Token: 0x06000292 RID: 658 RVA: 0x00009E39 File Offset: 0x00008039
			// (set) Token: 0x06000293 RID: 659 RVA: 0x00009E41 File Offset: 0x00008041
			public int ArrayItemCount { get; set; }

			// Token: 0x17000070 RID: 112
			// (get) Token: 0x06000294 RID: 660 RVA: 0x00009E4A File Offset: 0x0000804A
			public IList<JsonSchemaModel> Schemas
			{
				get
				{
					return this._schemas;
				}
			}

			// Token: 0x17000071 RID: 113
			// (get) Token: 0x06000295 RID: 661 RVA: 0x00009E52 File Offset: 0x00008052
			public Dictionary<string, bool> RequiredProperties
			{
				get
				{
					return this._requiredProperties;
				}
			}

			// Token: 0x17000072 RID: 114
			// (get) Token: 0x06000296 RID: 662 RVA: 0x00009E5A File Offset: 0x0000805A
			public JTokenType TokenType
			{
				get
				{
					return this._tokenType;
				}
			}

			// Token: 0x06000297 RID: 663 RVA: 0x00009E68 File Offset: 0x00008068
			public SchemaScope(JTokenType tokenType, IList<JsonSchemaModel> schemas)
			{
				this._tokenType = tokenType;
				this._schemas = schemas;
				this._requiredProperties = schemas.SelectMany(new Func<JsonSchemaModel, IEnumerable<string>>(this.GetRequiredProperties)).Distinct<string>().ToDictionary((string p) => p, (string p) => false);
			}

			// Token: 0x06000298 RID: 664 RVA: 0x00009EFC File Offset: 0x000080FC
			private IEnumerable<string> GetRequiredProperties(JsonSchemaModel schema)
			{
				if (schema == null || schema.Properties == null)
				{
					return Enumerable.Empty<string>();
				}
				return from p in schema.Properties
				where p.Value.Required
				select p.Key;
			}

			// Token: 0x040000D1 RID: 209
			private readonly JTokenType _tokenType;

			// Token: 0x040000D2 RID: 210
			private readonly IList<JsonSchemaModel> _schemas;

			// Token: 0x040000D3 RID: 211
			private readonly Dictionary<string, bool> _requiredProperties;
		}
	}
}

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000020 RID: 32
	public class RegexConverter : JsonConverter
	{
		// Token: 0x0600010F RID: 271 RVA: 0x000057F8 File Offset: 0x000039F8
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Regex regex = (Regex)value;
			BsonWriter bsonWriter = writer as BsonWriter;
			if (bsonWriter != null)
			{
				this.WriteBson(bsonWriter, regex);
				return;
			}
			this.WriteJson(writer, regex);
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00005827 File Offset: 0x00003A27
		private bool HasFlag(RegexOptions options, RegexOptions flag)
		{
			return (options & flag) == flag;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00005830 File Offset: 0x00003A30
		private void WriteBson(BsonWriter writer, Regex regex)
		{
			string text = null;
			if (this.HasFlag(regex.Options, RegexOptions.IgnoreCase))
			{
				text += "i";
			}
			if (this.HasFlag(regex.Options, RegexOptions.Multiline))
			{
				text += "m";
			}
			if (this.HasFlag(regex.Options, RegexOptions.Singleline))
			{
				text += "s";
			}
			text += "u";
			if (this.HasFlag(regex.Options, RegexOptions.ExplicitCapture))
			{
				text += "x";
			}
			writer.WriteRegex(regex.ToString(), text);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x000058C8 File Offset: 0x00003AC8
		private void WriteJson(JsonWriter writer, Regex regex)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Pattern");
			writer.WriteValue(regex.ToString());
			writer.WritePropertyName("Options");
			writer.WriteValue(regex.Options);
			writer.WriteEndObject();
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00005914 File Offset: 0x00003B14
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			BsonReader bsonReader = reader as BsonReader;
			if (bsonReader != null)
			{
				return this.ReadBson(bsonReader);
			}
			return this.ReadJson(reader);
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000593C File Offset: 0x00003B3C
		private object ReadBson(BsonReader reader)
		{
			string text = (string)reader.Value;
			int num = text.LastIndexOf("/");
			string pattern = text.Substring(1, num - 1);
			string text2 = text.Substring(num + 1);
			RegexOptions regexOptions = RegexOptions.None;
			foreach (char c in text2)
			{
				char c2 = c;
				if (c2 <= 'm')
				{
					if (c2 != 'i')
					{
						if (c2 == 'm')
						{
							regexOptions |= RegexOptions.Multiline;
						}
					}
					else
					{
						regexOptions |= RegexOptions.IgnoreCase;
					}
				}
				else if (c2 != 's')
				{
					if (c2 == 'x')
					{
						regexOptions |= RegexOptions.ExplicitCapture;
					}
				}
				else
				{
					regexOptions |= RegexOptions.Singleline;
				}
			}
			return new Regex(pattern, regexOptions);
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000059EC File Offset: 0x00003BEC
		private Regex ReadJson(JsonReader reader)
		{
			reader.Read();
			reader.Read();
			string pattern = (string)reader.Value;
			reader.Read();
			reader.Read();
			int options = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
			reader.Read();
			return new Regex(pattern, (RegexOptions)options);
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005A40 File Offset: 0x00003C40
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Regex);
		}
	}
}

using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
	// Token: 0x0200005B RID: 91
	public class JsonTextReader : JsonReader, IJsonLineInfo
	{
		// Token: 0x06000355 RID: 853 RVA: 0x0000BFF1 File Offset: 0x0000A1F1
		public JsonTextReader(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			this._reader = reader;
			this._buffer = new StringBuffer(4096);
			this._currentLineNumber = 1;
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000C028 File Offset: 0x0000A228
		private void ParseString(char quote)
		{
			this.ReadStringIntoBuffer(quote);
			if (this._readType == JsonTextReader.ReadType.ReadAsBytes)
			{
				byte[] value;
				if (this._buffer.Position == 0)
				{
					value = new byte[0];
				}
				else
				{
					value = Convert.FromBase64CharArray(this._buffer.GetInternalBuffer(), 0, this._buffer.Position);
					this._buffer.Position = 0;
				}
				this.SetToken(JsonToken.Bytes, value);
				return;
			}
			string text = this._buffer.ToString();
			this._buffer.Position = 0;
			if (text.StartsWith("/Date(", StringComparison.Ordinal) && text.EndsWith(")/", StringComparison.Ordinal))
			{
				this.ParseDate(text);
				return;
			}
			this.SetToken(JsonToken.String, text);
			this.QuoteChar = quote;
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0000C0DC File Offset: 0x0000A2DC
		private void ReadStringIntoBuffer(char quote)
		{
			char c;
			for (;;)
			{
				c = this.MoveNext();
				char c2 = c;
				if (c2 <= '"')
				{
					if (c2 != '\0')
					{
						if (c2 != '"')
						{
							goto IL_2C1;
						}
					}
					else
					{
						if (this._end)
						{
							break;
						}
						this._buffer.Append('\0');
						continue;
					}
				}
				else if (c2 != '\'')
				{
					if (c2 != '\\')
					{
						goto IL_2C1;
					}
					if ((c = this.MoveNext()) == '\0' && this._end)
					{
						goto IL_26D;
					}
					char c3 = c;
					if (c3 <= '\\')
					{
						if (c3 <= '\'')
						{
							if (c3 != '"' && c3 != '\'')
							{
								goto Block_10;
							}
						}
						else if (c3 != '/')
						{
							if (c3 != '\\')
							{
								goto Block_12;
							}
							this._buffer.Append('\\');
							continue;
						}
						this._buffer.Append(c);
						continue;
					}
					if (c3 <= 'f')
					{
						if (c3 == 'b')
						{
							this._buffer.Append('\b');
							continue;
						}
						if (c3 != 'f')
						{
							goto Block_15;
						}
						this._buffer.Append('\f');
						continue;
					}
					else
					{
						if (c3 != 'n')
						{
							switch (c3)
							{
							case 'r':
								this._buffer.Append('\r');
								continue;
							case 't':
								this._buffer.Append('\t');
								continue;
							case 'u':
							{
								char[] array = new char[4];
								for (int i = 0; i < array.Length; i++)
								{
									if ((c = this.MoveNext()) == '\0' && this._end)
									{
										goto IL_1BB;
									}
									array[i] = c;
								}
								char value = Convert.ToChar(int.Parse(new string(array), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
								this._buffer.Append(value);
								continue;
							}
							}
							goto Block_17;
						}
						this._buffer.Append('\n');
						continue;
					}
				}
				if (c == quote)
				{
					return;
				}
				this._buffer.Append(c);
				continue;
				IL_2C1:
				this._buffer.Append(c);
			}
			throw this.CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", new object[]
			{
				quote,
				this._currentLineNumber,
				this._currentLinePosition
			});
			Block_10:
			Block_12:
			Block_15:
			Block_17:
			goto IL_225;
			IL_1BB:
			throw this.CreateJsonReaderException("Unexpected end while parsing unicode character. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
			IL_225:
			throw this.CreateJsonReaderException("Bad JSON escape sequence: {0}. Line {1}, position {2}.", new object[]
			{
				"\\" + c,
				this._currentLineNumber,
				this._currentLinePosition
			});
			IL_26D:
			throw this.CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", new object[]
			{
				quote,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0000C3BC File Offset: 0x0000A5BC
		private JsonReaderException CreateJsonReaderException(string format, params object[] args)
		{
			string message = format.FormatWith(CultureInfo.InvariantCulture, args);
			return new JsonReaderException(message, null, this._currentLineNumber, this._currentLinePosition);
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0000C3EC File Offset: 0x0000A5EC
		private TimeSpan ReadOffset(string offsetText)
		{
			bool flag = offsetText[0] == '-';
			int num = int.Parse(offsetText.Substring(1, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
			int num2 = 0;
			if (offsetText.Length >= 5)
			{
				num2 = int.Parse(offsetText.Substring(3, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
			}
			TimeSpan result = TimeSpan.FromHours((double)num) + TimeSpan.FromMinutes((double)num2);
			if (flag)
			{
				result = result.Negate();
			}
			return result;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x0000C458 File Offset: 0x0000A658
		private void ParseDate(string text)
		{
			string text2 = text.Substring(6, text.Length - 8);
			DateTimeKind dateTimeKind = DateTimeKind.Utc;
			int num = text2.IndexOf('+', 1);
			if (num == -1)
			{
				num = text2.IndexOf('-', 1);
			}
			TimeSpan timeSpan = TimeSpan.Zero;
			if (num != -1)
			{
				dateTimeKind = DateTimeKind.Local;
				timeSpan = this.ReadOffset(text2.Substring(num));
				text2 = text2.Substring(0, num);
			}
			long javaScriptTicks = long.Parse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture);
			DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
			if (this._readType == JsonTextReader.ReadType.ReadAsDateTimeOffset)
			{
				this.SetToken(JsonToken.Date, new DateTimeOffset(dateTime.Add(timeSpan).Ticks, timeSpan));
				return;
			}
			DateTime dateTime2;
			switch (dateTimeKind)
			{
			case DateTimeKind.Unspecified:
				dateTime2 = DateTime.SpecifyKind(dateTime.ToLocalTime(), DateTimeKind.Unspecified);
				goto IL_CA;
			case DateTimeKind.Local:
				dateTime2 = dateTime.ToLocalTime();
				goto IL_CA;
			}
			dateTime2 = dateTime;
			IL_CA:
			this.SetToken(JsonToken.Date, dateTime2);
		}

		// Token: 0x0600035B RID: 859 RVA: 0x0000C540 File Offset: 0x0000A740
		private char MoveNext()
		{
			int num = this._reader.Read();
			int num2 = num;
			if (num2 != -1)
			{
				if (num2 != 10)
				{
					if (num2 != 13)
					{
						this._currentLinePosition++;
					}
					else
					{
						if (this._reader.Peek() == 10)
						{
							this._reader.Read();
						}
						this._currentLineNumber++;
						this._currentLinePosition = 0;
					}
				}
				else
				{
					this._currentLineNumber++;
					this._currentLinePosition = 0;
				}
				return (char)num;
			}
			this._end = true;
			return '\0';
		}

		// Token: 0x0600035C RID: 860 RVA: 0x0000C5CD File Offset: 0x0000A7CD
		private bool HasNext()
		{
			return this._reader.Peek() != -1;
		}

		// Token: 0x0600035D RID: 861 RVA: 0x0000C5E0 File Offset: 0x0000A7E0
		private int PeekNext()
		{
			return this._reader.Peek();
		}

		// Token: 0x0600035E RID: 862 RVA: 0x0000C5ED File Offset: 0x0000A7ED
		public override bool Read()
		{
			this._readType = JsonTextReader.ReadType.Read;
			return this.ReadInternal();
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0000C5FC File Offset: 0x0000A7FC
		public override byte[] ReadAsBytes()
		{
			this._readType = JsonTextReader.ReadType.ReadAsBytes;
			if (!this.ReadInternal())
			{
				throw this.CreateJsonReaderException("Unexpected end when reading bytes: Line {0}, position {1}.", new object[]
				{
					this._currentLineNumber,
					this._currentLinePosition
				});
			}
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Bytes)
			{
				return (byte[])this.Value;
			}
			throw this.CreateJsonReaderException("Unexpected token when reading bytes: {0}. Line {1}, position {2}.", new object[]
			{
				this.TokenType,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000360 RID: 864 RVA: 0x0000C6A8 File Offset: 0x0000A8A8
		public override decimal? ReadAsDecimal()
		{
			this._readType = JsonTextReader.ReadType.ReadAsDecimal;
			if (!this.ReadInternal())
			{
				throw this.CreateJsonReaderException("Unexpected end when reading decimal: Line {0}, position {1}.", new object[]
				{
					this._currentLineNumber,
					this._currentLinePosition
				});
			}
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Float)
			{
				return (decimal?)this.Value;
			}
			decimal num;
			if (this.TokenType == JsonToken.String && decimal.TryParse((string)this.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out num))
			{
				this.SetToken(JsonToken.Float, num);
				return new decimal?(num);
			}
			throw this.CreateJsonReaderException("Unexpected token when reading decimal: {0}. Line {1}, position {2}.", new object[]
			{
				this.TokenType,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0000C794 File Offset: 0x0000A994
		public override DateTimeOffset? ReadAsDateTimeOffset()
		{
			this._readType = JsonTextReader.ReadType.ReadAsDateTimeOffset;
			if (!this.ReadInternal())
			{
				throw this.CreateJsonReaderException("Unexpected end when reading date: Line {0}, position {1}.", new object[]
				{
					this._currentLineNumber,
					this._currentLinePosition
				});
			}
			if (this.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (this.TokenType == JsonToken.Date)
			{
				return new DateTimeOffset?((DateTimeOffset)this.Value);
			}
			throw this.CreateJsonReaderException("Unexpected token when reading date: {0}. Line {1}, position {2}.", new object[]
			{
				this.TokenType,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000362 RID: 866 RVA: 0x0000C84C File Offset: 0x0000AA4C
		private bool ReadInternal()
		{
			char c;
			for (;;)
			{
				char? lastChar = this._lastChar;
				int? num = (lastChar != null) ? new int?((int)lastChar.GetValueOrDefault()) : null;
				if (num != null)
				{
					c = this._lastChar.Value;
					this._lastChar = null;
				}
				else
				{
					c = this.MoveNext();
				}
				if (c == '\0' && this._end)
				{
					break;
				}
				switch (base.CurrentState)
				{
				case JsonReader.State.Start:
				case JsonReader.State.Property:
				case JsonReader.State.ArrayStart:
				case JsonReader.State.Array:
				case JsonReader.State.ConstructorStart:
				case JsonReader.State.Constructor:
					goto IL_A0;
				case JsonReader.State.Complete:
				case JsonReader.State.Closed:
				case JsonReader.State.Error:
					continue;
				case JsonReader.State.ObjectStart:
				case JsonReader.State.Object:
					goto IL_A8;
				case JsonReader.State.PostValue:
					if (this.ParsePostValue(c))
					{
						return true;
					}
					continue;
				}
				goto Block_4;
			}
			return false;
			Block_4:
			throw this.CreateJsonReaderException("Unexpected state: {0}. Line {1}, position {2}.", new object[]
			{
				base.CurrentState,
				this._currentLineNumber,
				this._currentLinePosition
			});
			IL_A0:
			return this.ParseValue(c);
			IL_A8:
			return this.ParseObject(c);
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0000C95C File Offset: 0x0000AB5C
		private bool ParsePostValue(char currentChar)
		{
			for (;;)
			{
				char c = currentChar;
				if (c <= ')')
				{
					switch (c)
					{
					case '\t':
					case '\n':
					case '\r':
						break;
					case '\v':
					case '\f':
						goto IL_7C;
					default:
						if (c != ' ')
						{
							if (c != ')')
							{
								goto IL_7C;
							}
							goto IL_62;
						}
						break;
					}
				}
				else if (c <= '/')
				{
					if (c == ',')
					{
						goto IL_74;
					}
					if (c != '/')
					{
						goto IL_7C;
					}
					goto IL_6C;
				}
				else
				{
					if (c == ']')
					{
						goto IL_58;
					}
					if (c == '}')
					{
						break;
					}
					goto IL_7C;
				}
				IL_BD:
				if ((currentChar = this.MoveNext()) == '\0' && this._end)
				{
					return false;
				}
				continue;
				IL_7C:
				if (!char.IsWhiteSpace(currentChar))
				{
					goto Block_9;
				}
				goto IL_BD;
			}
			base.SetToken(JsonToken.EndObject);
			return true;
			IL_58:
			base.SetToken(JsonToken.EndArray);
			return true;
			IL_62:
			base.SetToken(JsonToken.EndConstructor);
			return true;
			IL_6C:
			this.ParseComment();
			return true;
			IL_74:
			base.SetStateBasedOnCurrent();
			return false;
			Block_9:
			throw this.CreateJsonReaderException("After parsing a value an unexpected character was encountered: {0}. Line {1}, position {2}.", new object[]
			{
				currentChar,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0000CA40 File Offset: 0x0000AC40
		private bool ParseObject(char currentChar)
		{
			for (;;)
			{
				char c = currentChar;
				if (c <= ' ')
				{
					switch (c)
					{
					case '\t':
					case '\n':
					case '\r':
						break;
					case '\v':
					case '\f':
						goto IL_47;
					default:
						if (c != ' ')
						{
							goto IL_47;
						}
						break;
					}
				}
				else
				{
					if (c == '/')
					{
						goto IL_3F;
					}
					if (c == '}')
					{
						break;
					}
					goto IL_47;
				}
				IL_57:
				if ((currentChar = this.MoveNext()) == '\0' && this._end)
				{
					return false;
				}
				continue;
				IL_47:
				if (!char.IsWhiteSpace(currentChar))
				{
					goto Block_5;
				}
				goto IL_57;
			}
			base.SetToken(JsonToken.EndObject);
			return true;
			IL_3F:
			this.ParseComment();
			return true;
			Block_5:
			return this.ParseProperty(currentChar);
		}

		// Token: 0x06000365 RID: 869 RVA: 0x0000CAB8 File Offset: 0x0000ACB8
		private bool ParseProperty(char firstChar)
		{
			char c = firstChar;
			char c2;
			if (this.ValidIdentifierChar(c))
			{
				c2 = '\0';
				c = this.ParseUnquotedProperty(c);
			}
			else
			{
				if (c != '"' && c != '\'')
				{
					throw this.CreateJsonReaderException("Invalid property identifier character: {0}. Line {1}, position {2}.", new object[]
					{
						c,
						this._currentLineNumber,
						this._currentLinePosition
					});
				}
				c2 = c;
				this.ReadStringIntoBuffer(c2);
				c = this.MoveNext();
			}
			if (c != ':')
			{
				c = this.MoveNext();
				this.EatWhitespace(c, false, out c);
				if (c != ':')
				{
					throw this.CreateJsonReaderException("Invalid character after parsing property name. Expected ':' but got: {0}. Line {1}, position {2}.", new object[]
					{
						c,
						this._currentLineNumber,
						this._currentLinePosition
					});
				}
			}
			this.SetToken(JsonToken.PropertyName, this._buffer.ToString());
			this.QuoteChar = c2;
			this._buffer.Position = 0;
			return true;
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0000CBAC File Offset: 0x0000ADAC
		private bool ValidIdentifierChar(char value)
		{
			return char.IsLetterOrDigit(value) || value == '_' || value == '$';
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0000CBC4 File Offset: 0x0000ADC4
		private char ParseUnquotedProperty(char firstChar)
		{
			this._buffer.Append(firstChar);
			char c;
			while ((c = this.MoveNext()) != '\0' || !this._end)
			{
				if (char.IsWhiteSpace(c) || c == ':')
				{
					return c;
				}
				if (!this.ValidIdentifierChar(c))
				{
					throw this.CreateJsonReaderException("Invalid JavaScript property identifier character: {0}. Line {1}, position {2}.", new object[]
					{
						c,
						this._currentLineNumber,
						this._currentLinePosition
					});
				}
				this._buffer.Append(c);
			}
			throw this.CreateJsonReaderException("Unexpected end when parsing unquoted property name. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0000CC80 File Offset: 0x0000AE80
		private bool ParseValue(char currentChar)
		{
			for (;;)
			{
				char c = currentChar;
				if (c <= 'N')
				{
					if (c <= '"')
					{
						switch (c)
						{
						case '\t':
						case '\n':
						case '\r':
							break;
						case '\v':
						case '\f':
							goto IL_1FC;
						default:
							switch (c)
							{
							case ' ':
								break;
							case '!':
								goto IL_1FC;
							case '"':
								goto IL_D9;
							default:
								goto IL_1FC;
							}
							break;
						}
					}
					else
					{
						switch (c)
						{
						case '\'':
							goto IL_D9;
						case '(':
						case '*':
						case '+':
						case '.':
							goto IL_1FC;
						case ')':
							goto IL_1F2;
						case ',':
							goto IL_1E8;
						case '-':
							goto IL_197;
						case '/':
							goto IL_1B2;
						default:
							if (c == 'I')
							{
								goto IL_18F;
							}
							if (c != 'N')
							{
								goto IL_1FC;
							}
							goto IL_187;
						}
					}
				}
				else if (c <= 'f')
				{
					switch (c)
					{
					case '[':
						goto IL_1CB;
					case '\\':
						goto IL_1FC;
					case ']':
						goto IL_1DE;
					default:
						if (c != 'f')
						{
							goto IL_1FC;
						}
						goto IL_EA;
					}
				}
				else
				{
					if (c == 'n')
					{
						goto IL_F2;
					}
					switch (c)
					{
					case 't':
						goto IL_E2;
					case 'u':
						goto IL_1BA;
					default:
						switch (c)
						{
						case '{':
							goto IL_1C2;
						case '|':
							goto IL_1FC;
						case '}':
							goto IL_1D4;
						default:
							goto IL_1FC;
						}
						break;
					}
				}
				IL_25D:
				if ((currentChar = this.MoveNext()) == '\0' && this._end)
				{
					return false;
				}
				continue;
				IL_1FC:
				if (!char.IsWhiteSpace(currentChar))
				{
					goto Block_17;
				}
				goto IL_25D;
			}
			IL_D9:
			this.ParseString(currentChar);
			return true;
			IL_E2:
			this.ParseTrue();
			return true;
			IL_EA:
			this.ParseFalse();
			return true;
			IL_F2:
			if (this.HasNext())
			{
				char c2 = (char)this.PeekNext();
				if (c2 == 'u')
				{
					this.ParseNull();
				}
				else
				{
					if (c2 != 'e')
					{
						throw this.CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", new object[]
						{
							currentChar,
							this._currentLineNumber,
							this._currentLinePosition
						});
					}
					this.ParseConstructor();
				}
				return true;
			}
			throw this.CreateJsonReaderException("Unexpected end. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
			IL_187:
			this.ParseNumberNaN();
			return true;
			IL_18F:
			this.ParseNumberPositiveInfinity();
			return true;
			IL_197:
			if (this.PeekNext() == 73)
			{
				this.ParseNumberNegativeInfinity();
			}
			else
			{
				this.ParseNumber(currentChar);
			}
			return true;
			IL_1B2:
			this.ParseComment();
			return true;
			IL_1BA:
			this.ParseUndefined();
			return true;
			IL_1C2:
			base.SetToken(JsonToken.StartObject);
			return true;
			IL_1CB:
			base.SetToken(JsonToken.StartArray);
			return true;
			IL_1D4:
			base.SetToken(JsonToken.EndObject);
			return true;
			IL_1DE:
			base.SetToken(JsonToken.EndArray);
			return true;
			IL_1E8:
			base.SetToken(JsonToken.Undefined);
			return true;
			IL_1F2:
			base.SetToken(JsonToken.EndConstructor);
			return true;
			Block_17:
			if (char.IsNumber(currentChar) || currentChar == '-' || currentChar == '.')
			{
				this.ParseNumber(currentChar);
				return true;
			}
			throw this.CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", new object[]
			{
				currentChar,
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0000CF04 File Offset: 0x0000B104
		private bool EatWhitespace(char initialChar, bool oneOrMore, out char finalChar)
		{
			bool flag = false;
			char c = initialChar;
			while (c == ' ' || char.IsWhiteSpace(c))
			{
				flag = true;
				c = this.MoveNext();
			}
			finalChar = c;
			return !oneOrMore || flag;
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0000CF38 File Offset: 0x0000B138
		private void ParseConstructor()
		{
			if (this.MatchValue('n', "new", true))
			{
				char c = this.MoveNext();
				if (this.EatWhitespace(c, true, out c))
				{
					while (char.IsLetter(c))
					{
						this._buffer.Append(c);
						c = this.MoveNext();
					}
					this.EatWhitespace(c, false, out c);
					if (c != '(')
					{
						throw this.CreateJsonReaderException("Unexpected character while parsing constructor: {0}. Line {1}, position {2}.", new object[]
						{
							c,
							this._currentLineNumber,
							this._currentLinePosition
						});
					}
					string value = this._buffer.ToString();
					this._buffer.Position = 0;
					this.SetToken(JsonToken.StartConstructor, value);
				}
			}
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0000CFF4 File Offset: 0x0000B1F4
		private void ParseNumber(char firstChar)
		{
			char c = firstChar;
			bool flag = false;
			do
			{
				if (this.IsSeperator(c))
				{
					flag = true;
					this._lastChar = new char?(c);
				}
				else
				{
					this._buffer.Append(c);
				}
			}
			while (!flag && ((c = this.MoveNext()) != '\0' || !this._end));
			string text = this._buffer.ToString();
			bool flag2 = firstChar == '0' && !text.StartsWith("0.", StringComparison.OrdinalIgnoreCase);
			object value;
			JsonToken newToken;
			if (flag2)
			{
				long num = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8);
				value = num;
				newToken = JsonToken.Integer;
			}
			else if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || text.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
			{
				value = decimal.Parse(text, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
				newToken = JsonToken.Float;
			}
			else
			{
				try
				{
					value = Convert.ToInt64(text, CultureInfo.InvariantCulture);
				}
				catch (OverflowException innerException)
				{
					throw new JsonReaderException("JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						text
					}), innerException);
				}
				newToken = JsonToken.Integer;
			}
			this._buffer.Position = 0;
			this.SetToken(newToken, value);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0000D134 File Offset: 0x0000B334
		private void ParseNumberOld(char firstChar)
		{
			char c = firstChar;
			bool flag = false;
			do
			{
				if (this.IsSeperator(c))
				{
					flag = true;
					this._lastChar = new char?(c);
				}
				else
				{
					this._buffer.Append(c);
				}
			}
			while (!flag && ((c = this.MoveNext()) != '\0' || !this._end));
			string text = this._buffer.ToString();
			bool flag2 = firstChar == '0' && !text.StartsWith("0.", StringComparison.OrdinalIgnoreCase);
			object value2;
			JsonToken newToken;
			if (this._readType == JsonTextReader.ReadType.ReadAsDecimal)
			{
				if (flag2)
				{
					long value = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8);
					value2 = Convert.ToDecimal(value);
				}
				else
				{
					value2 = decimal.Parse(text, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
				}
				newToken = JsonToken.Float;
			}
			else if (flag2)
			{
				value2 = (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8));
				newToken = JsonToken.Integer;
			}
			else if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || text.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
			{
				value2 = Convert.ToDouble(text, CultureInfo.InvariantCulture);
				newToken = JsonToken.Float;
			}
			else
			{
				try
				{
					value2 = Convert.ToInt64(text, CultureInfo.InvariantCulture);
				}
				catch (OverflowException innerException)
				{
					throw new JsonReaderException("JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, new object[]
					{
						text
					}), innerException);
				}
				newToken = JsonToken.Integer;
			}
			this._buffer.Position = 0;
			this.SetToken(newToken, value2);
		}

		// Token: 0x0600036D RID: 877 RVA: 0x0000D2C4 File Offset: 0x0000B4C4
		private void ParseComment()
		{
			char c = this.MoveNext();
			if (c == '*')
			{
				while ((c = this.MoveNext()) != '\0' || !this._end)
				{
					if (c == '*')
					{
						if ((c = this.MoveNext()) != '\0' || !this._end)
						{
							if (c == '/')
							{
								IL_95:
								this.SetToken(JsonToken.Comment, this._buffer.ToString());
								this._buffer.Position = 0;
								return;
							}
							this._buffer.Append('*');
							this._buffer.Append(c);
						}
					}
					else
					{
						this._buffer.Append(c);
					}
				}
				goto IL_95;
			}
			throw this.CreateJsonReaderException("Error parsing comment. Expected: *. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0000D384 File Offset: 0x0000B584
		private bool MatchValue(char firstChar, string value)
		{
			char c = firstChar;
			int num = 0;
			while (c == value[num])
			{
				num++;
				if (num >= value.Length || ((c = this.MoveNext()) == '\0' && this._end))
				{
					break;
				}
			}
			return num == value.Length;
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0000D3C8 File Offset: 0x0000B5C8
		private bool MatchValue(char firstChar, string value, bool noTrailingNonSeperatorCharacters)
		{
			bool flag = this.MatchValue(firstChar, value);
			if (!noTrailingNonSeperatorCharacters)
			{
				return flag;
			}
			int num = this.PeekNext();
			char c = (num != -1) ? ((char)num) : '\0';
			return flag && (c == '\0' || this.IsSeperator(c));
		}

		// Token: 0x06000370 RID: 880 RVA: 0x0000D40C File Offset: 0x0000B60C
		private bool IsSeperator(char c)
		{
			if (c <= ')')
			{
				switch (c)
				{
				case '\t':
				case '\n':
				case '\r':
					break;
				case '\v':
				case '\f':
					goto IL_7A;
				default:
					if (c != ' ')
					{
						if (c != ')')
						{
							goto IL_7A;
						}
						if (base.CurrentState == JsonReader.State.Constructor || base.CurrentState == JsonReader.State.ConstructorStart)
						{
							return true;
						}
						return false;
					}
					break;
				}
				return true;
			}
			if (c <= '/')
			{
				if (c != ',')
				{
					if (c != '/')
					{
						goto IL_7A;
					}
					return this.HasNext() && this.PeekNext() == 42;
				}
			}
			else if (c != ']' && c != '}')
			{
				goto IL_7A;
			}
			return true;
			IL_7A:
			if (char.IsWhiteSpace(c))
			{
				return true;
			}
			return false;
		}

		// Token: 0x06000371 RID: 881 RVA: 0x0000D4A0 File Offset: 0x0000B6A0
		private void ParseTrue()
		{
			if (this.MatchValue('t', JsonConvert.True, true))
			{
				this.SetToken(JsonToken.Boolean, true);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing boolean value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000372 RID: 882 RVA: 0x0000D4FC File Offset: 0x0000B6FC
		private void ParseNull()
		{
			if (this.MatchValue('n', JsonConvert.Null, true))
			{
				base.SetToken(JsonToken.Null);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing null value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000373 RID: 883 RVA: 0x0000D554 File Offset: 0x0000B754
		private void ParseUndefined()
		{
			if (this.MatchValue('u', JsonConvert.Undefined, true))
			{
				base.SetToken(JsonToken.Undefined);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing undefined value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000374 RID: 884 RVA: 0x0000D5AC File Offset: 0x0000B7AC
		private void ParseFalse()
		{
			if (this.MatchValue('f', JsonConvert.False, true))
			{
				this.SetToken(JsonToken.Boolean, false);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing boolean value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0000D608 File Offset: 0x0000B808
		private void ParseNumberNegativeInfinity()
		{
			if (this.MatchValue('-', JsonConvert.NegativeInfinity, true))
			{
				this.SetToken(JsonToken.Float, double.NegativeInfinity);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing negative infinity value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0000D66C File Offset: 0x0000B86C
		private void ParseNumberPositiveInfinity()
		{
			if (this.MatchValue('I', JsonConvert.PositiveInfinity, true))
			{
				this.SetToken(JsonToken.Float, double.PositiveInfinity);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing positive infinity value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0000D6D0 File Offset: 0x0000B8D0
		private void ParseNumberNaN()
		{
			if (this.MatchValue('N', JsonConvert.NaN, true))
			{
				this.SetToken(JsonToken.Float, double.NaN);
				return;
			}
			throw this.CreateJsonReaderException("Error parsing NaN value. Line {0}, position {1}.", new object[]
			{
				this._currentLineNumber,
				this._currentLinePosition
			});
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0000D732 File Offset: 0x0000B932
		public override void Close()
		{
			base.Close();
			if (this._reader != null)
			{
				this._reader.Close();
			}
			if (this._buffer != null)
			{
				this._buffer.Clear();
			}
		}

		// Token: 0x06000379 RID: 889 RVA: 0x0000D760 File Offset: 0x0000B960
		public bool HasLineInfo()
		{
			return true;
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x0600037A RID: 890 RVA: 0x0000D763 File Offset: 0x0000B963
		public int LineNumber
		{
			get
			{
				if (base.CurrentState == JsonReader.State.Start)
				{
					return 0;
				}
				return this._currentLineNumber;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x0600037B RID: 891 RVA: 0x0000D775 File Offset: 0x0000B975
		public int LinePosition
		{
			get
			{
				return this._currentLinePosition;
			}
		}

		// Token: 0x040000FE RID: 254
		private const int LineFeedValue = 10;

		// Token: 0x040000FF RID: 255
		private const int CarriageReturnValue = 13;

		// Token: 0x04000100 RID: 256
		private readonly TextReader _reader;

		// Token: 0x04000101 RID: 257
		private readonly StringBuffer _buffer;

		// Token: 0x04000102 RID: 258
		private char? _lastChar;

		// Token: 0x04000103 RID: 259
		private int _currentLinePosition;

		// Token: 0x04000104 RID: 260
		private int _currentLineNumber;

		// Token: 0x04000105 RID: 261
		private bool _end;

		// Token: 0x04000106 RID: 262
		private JsonTextReader.ReadType _readType;

		// Token: 0x0200005C RID: 92
		private enum ReadType
		{
			// Token: 0x04000108 RID: 264
			Read,
			// Token: 0x04000109 RID: 265
			ReadAsBytes,
			// Token: 0x0400010A RID: 266
			ReadAsDecimal,
			// Token: 0x0400010B RID: 267
			ReadAsDateTimeOffset
		}
	}
}

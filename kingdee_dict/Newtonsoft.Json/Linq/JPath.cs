using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
	// Token: 0x02000025 RID: 37
	internal class JPath
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600012D RID: 301 RVA: 0x00005C0E File Offset: 0x00003E0E
		// (set) Token: 0x0600012E RID: 302 RVA: 0x00005C16 File Offset: 0x00003E16
		public List<object> Parts { get; private set; }

		// Token: 0x0600012F RID: 303 RVA: 0x00005C1F File Offset: 0x00003E1F
		public JPath(string expression)
		{
			ValidationUtils.ArgumentNotNull(expression, "expression");
			this._expression = expression;
			this.Parts = new List<object>();
			this.ParseMain();
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00005C4C File Offset: 0x00003E4C
		private void ParseMain()
		{
			int num = this._currentIndex;
			bool flag = false;
			while (this._currentIndex < this._expression.Length)
			{
				char c = this._expression[this._currentIndex];
				char c2 = c;
				switch (c2)
				{
				case '(':
					goto IL_56;
				case ')':
					goto IL_94;
				default:
					if (c2 != '.')
					{
						switch (c2)
						{
						case '[':
							goto IL_56;
						case ']':
							goto IL_94;
						}
						if (flag)
						{
							throw new Exception("Unexpected character following indexer: " + c);
						}
					}
					else
					{
						if (this._currentIndex > num)
						{
							string item = this._expression.Substring(num, this._currentIndex - num);
							this.Parts.Add(item);
						}
						num = this._currentIndex + 1;
						flag = false;
					}
					break;
				}
				IL_FC:
				this._currentIndex++;
				continue;
				IL_56:
				if (this._currentIndex > num)
				{
					string item2 = this._expression.Substring(num, this._currentIndex - num);
					this.Parts.Add(item2);
				}
				this.ParseIndexer(c);
				num = this._currentIndex + 1;
				flag = true;
				goto IL_FC;
				IL_94:
				throw new Exception("Unexpected character while parsing path: " + c);
			}
			if (this._currentIndex > num)
			{
				string item3 = this._expression.Substring(num, this._currentIndex - num);
				this.Parts.Add(item3);
			}
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00005DA8 File Offset: 0x00003FA8
		private void ParseIndexer(char indexerOpenChar)
		{
			this._currentIndex++;
			char c = (indexerOpenChar == '[') ? ']' : ')';
			int currentIndex = this._currentIndex;
			int num = 0;
			bool flag = false;
			while (this._currentIndex < this._expression.Length)
			{
				char c2 = this._expression[this._currentIndex];
				if (char.IsDigit(c2))
				{
					num++;
					this._currentIndex++;
				}
				else
				{
					if (c2 == c)
					{
						flag = true;
						break;
					}
					throw new Exception("Unexpected character while parsing path indexer: " + c2);
				}
			}
			if (!flag)
			{
				throw new Exception("Path ended with open indexer. Expected " + c);
			}
			if (num == 0)
			{
				throw new Exception("Empty path indexer.");
			}
			string value = this._expression.Substring(currentIndex, num);
			this.Parts.Add(Convert.ToInt32(value, CultureInfo.InvariantCulture));
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00005E94 File Offset: 0x00004094
		internal JToken Evaluate(JToken root, bool errorWhenNoMatch)
		{
			JToken jtoken = root;
			foreach (object obj in this.Parts)
			{
				string text = obj as string;
				if (text != null)
				{
					JObject jobject = jtoken as JObject;
					if (jobject != null)
					{
						jtoken = jobject[text];
						if (jtoken == null && errorWhenNoMatch)
						{
							throw new Exception("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, new object[]
							{
								text
							}));
						}
					}
					else
					{
						if (errorWhenNoMatch)
						{
							throw new Exception("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
							{
								text,
								jtoken.GetType().Name
							}));
						}
						return null;
					}
				}
				else
				{
					int num = (int)obj;
					JArray jarray = jtoken as JArray;
					if (jarray != null)
					{
						if (jarray.Count <= num)
						{
							if (errorWhenNoMatch)
							{
								throw new IndexOutOfRangeException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, new object[]
								{
									num
								}));
							}
							return null;
						}
						else
						{
							jtoken = jarray[num];
						}
					}
					else
					{
						if (errorWhenNoMatch)
						{
							throw new Exception("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, new object[]
							{
								num,
								jtoken.GetType().Name
							}));
						}
						return null;
					}
				}
			}
			return jtoken;
		}

		// Token: 0x0400007C RID: 124
		private readonly string _expression;

		// Token: 0x0400007D RID: 125
		private int _currentIndex;
	}
}

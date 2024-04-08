using System;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000C4 RID: 196
	internal class StringBuffer
	{
		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000852 RID: 2130 RVA: 0x0001E468 File Offset: 0x0001C668
		// (set) Token: 0x06000853 RID: 2131 RVA: 0x0001E470 File Offset: 0x0001C670
		public int Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
			}
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x0001E479 File Offset: 0x0001C679
		public StringBuffer()
		{
			this._buffer = StringBuffer._emptyBuffer;
		}

		// Token: 0x06000855 RID: 2133 RVA: 0x0001E48C File Offset: 0x0001C68C
		public StringBuffer(int initalSize)
		{
			this._buffer = new char[initalSize];
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x0001E4A0 File Offset: 0x0001C6A0
		public void Append(char value)
		{
			if (this._position == this._buffer.Length)
			{
				this.EnsureSize(1);
			}
			this._buffer[this._position++] = value;
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0001E4DD File Offset: 0x0001C6DD
		public void Clear()
		{
			this._buffer = StringBuffer._emptyBuffer;
			this._position = 0;
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x0001E4F4 File Offset: 0x0001C6F4
		private void EnsureSize(int appendLength)
		{
			char[] array = new char[(this._position + appendLength) * 2];
			Array.Copy(this._buffer, array, this._position);
			this._buffer = array;
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x0001E52A File Offset: 0x0001C72A
		public override string ToString()
		{
			return this.ToString(0, this._position);
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x0001E539 File Offset: 0x0001C739
		public string ToString(int start, int length)
		{
			return new string(this._buffer, start, length);
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x0001E548 File Offset: 0x0001C748
		public char[] GetInternalBuffer()
		{
			return this._buffer;
		}

		// Token: 0x040002A4 RID: 676
		private char[] _buffer;

		// Token: 0x040002A5 RID: 677
		private int _position;

		// Token: 0x040002A6 RID: 678
		private static readonly char[] _emptyBuffer = new char[0];
	}
}

using System;
using System.IO;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x0200009E RID: 158
	internal class Base64Encoder
	{
		// Token: 0x0600075A RID: 1882 RVA: 0x0001A1F8 File Offset: 0x000183F8
		public Base64Encoder(TextWriter writer)
		{
			ValidationUtils.ArgumentNotNull(writer, "writer");
			this._writer = writer;
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x0001A220 File Offset: 0x00018420
		public void Encode(byte[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (count > buffer.Length - index)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (this._leftOverBytesCount > 0)
			{
				int leftOverBytesCount = this._leftOverBytesCount;
				while (leftOverBytesCount < 3 && count > 0)
				{
					this._leftOverBytes[leftOverBytesCount++] = buffer[index++];
					count--;
				}
				if (count == 0 && leftOverBytesCount < 3)
				{
					this._leftOverBytesCount = leftOverBytesCount;
					return;
				}
				int count2 = Convert.ToBase64CharArray(this._leftOverBytes, 0, 3, this._charsLine, 0);
				this.WriteChars(this._charsLine, 0, count2);
			}
			this._leftOverBytesCount = count % 3;
			if (this._leftOverBytesCount > 0)
			{
				count -= this._leftOverBytesCount;
				if (this._leftOverBytes == null)
				{
					this._leftOverBytes = new byte[3];
				}
				for (int i = 0; i < this._leftOverBytesCount; i++)
				{
					this._leftOverBytes[i] = buffer[index + count + i];
				}
			}
			int num = index + count;
			int num2 = 57;
			while (index < num)
			{
				if (index + num2 > num)
				{
					num2 = num - index;
				}
				int count3 = Convert.ToBase64CharArray(buffer, index, num2, this._charsLine, 0);
				this.WriteChars(this._charsLine, 0, count3);
				index += num2;
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x0001A364 File Offset: 0x00018564
		public void Flush()
		{
			if (this._leftOverBytesCount > 0)
			{
				int count = Convert.ToBase64CharArray(this._leftOverBytes, 0, this._leftOverBytesCount, this._charsLine, 0);
				this.WriteChars(this._charsLine, 0, count);
				this._leftOverBytesCount = 0;
			}
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x0001A3A9 File Offset: 0x000185A9
		private void WriteChars(char[] chars, int index, int count)
		{
			this._writer.Write(chars, index, count);
		}

		// Token: 0x04000246 RID: 582
		private const int Base64LineSize = 76;

		// Token: 0x04000247 RID: 583
		private const int LineSizeInBytes = 57;

		// Token: 0x04000248 RID: 584
		private readonly char[] _charsLine = new char[76];

		// Token: 0x04000249 RID: 585
		private readonly TextWriter _writer;

		// Token: 0x0400024A RID: 586
		private byte[] _leftOverBytes;

		// Token: 0x0400024B RID: 587
		private int _leftOverBytesCount;
	}
}

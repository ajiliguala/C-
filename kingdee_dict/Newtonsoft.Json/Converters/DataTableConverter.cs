using System;
using System.Data;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000019 RID: 25
	public class DataTableConverter : JsonConverter
	{
		// Token: 0x060000F0 RID: 240 RVA: 0x00005160 File Offset: 0x00003360
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			DataTable dataTable = (DataTable)value;
			writer.WriteStartArray();
			foreach (object obj in dataTable.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				writer.WriteStartObject();
				foreach (object obj2 in dataRow.Table.Columns)
				{
					DataColumn dataColumn = (DataColumn)obj2;
					writer.WritePropertyName(dataColumn.ColumnName);
					serializer.Serialize(writer, dataRow[dataColumn]);
				}
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000523C File Offset: 0x0000343C
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			DataTable dataTable;
			if (reader.TokenType == JsonToken.PropertyName)
			{
				dataTable = new DataTable((string)reader.Value);
				reader.Read();
			}
			else
			{
				dataTable = new DataTable();
			}
			reader.Read();
			while (reader.TokenType == JsonToken.StartObject)
			{
				DataRow dataRow = dataTable.NewRow();
				reader.Read();
				while (reader.TokenType == JsonToken.PropertyName)
				{
					string text = (string)reader.Value;
					reader.Read();
					if (!dataTable.Columns.Contains(text))
					{
						Type columnDataType = DataTableConverter.GetColumnDataType(reader.TokenType);
						dataTable.Columns.Add(new DataColumn(text, columnDataType));
					}
					dataRow[text] = reader.Value;
					reader.Read();
				}
				dataRow.EndEdit();
				dataTable.Rows.Add(dataRow);
				reader.Read();
			}
			return dataTable;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00005310 File Offset: 0x00003510
		private static Type GetColumnDataType(JsonToken tokenType)
		{
			switch (tokenType)
			{
			case JsonToken.Integer:
				return typeof(long);
			case JsonToken.Float:
				return typeof(double);
			case JsonToken.String:
			case JsonToken.Null:
			case JsonToken.Undefined:
				return typeof(string);
			case JsonToken.Boolean:
				return typeof(bool);
			case JsonToken.Date:
				return typeof(DateTime);
			}
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000538D File Offset: 0x0000358D
		public override bool CanConvert(Type valueType)
		{
			return valueType == typeof(DataTable);
		}
	}
}

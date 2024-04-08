using System;
using System.Data;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x02000018 RID: 24
	public class DataSetConverter : JsonConverter
	{
		// Token: 0x060000EC RID: 236 RVA: 0x00005068 File Offset: 0x00003268
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			DataSet dataSet = (DataSet)value;
			DataTableConverter dataTableConverter = new DataTableConverter();
			writer.WriteStartObject();
			foreach (object obj in dataSet.Tables)
			{
				DataTable dataTable = (DataTable)obj;
				writer.WritePropertyName(dataTable.TableName);
				dataTableConverter.WriteJson(writer, dataTable, serializer);
			}
			writer.WriteEndObject();
		}

		// Token: 0x060000ED RID: 237 RVA: 0x000050EC File Offset: 0x000032EC
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			DataSet dataSet = new DataSet();
			DataTableConverter dataTableConverter = new DataTableConverter();
			reader.Read();
			while (reader.TokenType == JsonToken.PropertyName)
			{
				DataTable table = (DataTable)dataTableConverter.ReadJson(reader, typeof(DataTable), null, serializer);
				dataSet.Tables.Add(table);
				reader.Read();
			}
			return dataSet;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00005145 File Offset: 0x00003345
		public override bool CanConvert(Type valueType)
		{
			return valueType == typeof(DataSet);
		}
	}
}

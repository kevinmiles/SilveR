﻿using CsvHelper;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SilveR.Helpers
{
    public static class CSVConverter
    {
        public static DataTable CSVDataToDataTable(Stream stream, CultureInfo culture = null)
        {
            DataTable dataTable = new DataTable();

            //use the csvreader to read in the csv data
            TextReader textReader = new StreamReader(stream);
            CsvParser parser = new CsvParser(textReader);
            if (culture != null && culture.NumberFormat.NumberDecimalSeparator == ",")
            {
                parser.Configuration.Delimiter = ";";// if comma then use semicolon
            }

            string[] headerRow = parser.Read();
            for (int i = 0; i < headerRow.Count(); i++)
            {
                DataColumn newCol;

                if (headerRow[i] == "SilveRSelected")
                {
                    newCol = new DataColumn(headerRow[i], System.Type.GetType("System.Boolean"));
                }
                else
                {
                    newCol = new DataColumn(headerRow[i], System.Type.GetType("System.String"));
                }

                dataTable.Columns.Add(newCol);
            }


            stream.Seek(0, SeekOrigin.Begin);
            textReader = new StreamReader(stream);

            CsvReader csv = new CsvReader(textReader);
            if (culture != null && culture.NumberFormat.NumberDecimalSeparator == ",")
            {
                csv.Configuration.Delimiter = ";";// if comma then use semicolon
            }

            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                DataRow row = dataTable.NewRow();
                foreach (DataColumn column in dataTable.Columns)
                {
                    row[column.ColumnName] = csv.GetField(column.DataType, column.ColumnName);
                }
                dataTable.Rows.Add(row);
            }

            parser.Dispose();
            csv.Dispose();
            textReader.Dispose();
            stream.Dispose();

            return dataTable;
        }
    }
}
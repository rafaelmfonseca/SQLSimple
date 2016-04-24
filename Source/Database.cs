using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SQLSimple
{

    public class Row
    {

        private Dictionary<string, object> columnsValues;

        /// <summary>
        /// Return the value of a given row from a given column
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <returns>It will always return a string</returns>
        public object this[string name]
        {
            get
            {
                if (columnsValues.ContainsKey(name))
                    return columnsValues[name];
                return String.Empty;
            }
        }

        /// <summary>
        /// Return the value of a given row from a given column
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="isConvertible">If the value is convertible to int</param>
        /// <returns>It will always return an int.</returns>
        public int this[string name, bool isConvertible]
        {
            get
            {
                try
                {
                    return Convert.ToInt32(this[name]);
                }
                catch (FormatException e)
                {
                    return 0;
                }
            }
        }

        public Row()
        {
            columnsValues = new Dictionary<string, object>();
        }

        public void AddColumnValue(string name, object value)
        {
            columnsValues.Add(name, value);
        }
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public bool Auto_Increment { get; set; } = false;
    }

    public class Database
    {
        /// <summary>
        /// Called when new row is inserted
        /// </summary>
        public event EventHandler OnInsert;

        /// <summary>
        /// Character to split between values
        /// </summary>
        private const char deliminator = '|';

        /// <summary>
        /// Character to split between columns
        /// </summary>
        private const char deliminatorInfo = ':';

        /// <summary>
        /// A convenient SELECT * function
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns>Array with all rows of the table</returns>
        public Row[] Select(string tableName)
        {
            if (!Exists(tableName))
                throw new Exception("Table does not exists.");

            if (!HasColumnsDefinition(tableName))
                throw new Exception("Table does not have columns definitions.");

            if (Count(tableName) == 0)
                throw new Exception("Table does not have rows to show.");

            string[] tableLines = GetLines(tableName);
            ColumnInfo[] columns = GetColumns(tableName);
            List<Row> rows = new List<Row>();
            foreach (string line in tableLines.Skip(1))
            {
                Row tmpRow = new Row();
                string[] rowValues = line.Split(deliminator);
                int count = 0;
                foreach (ColumnInfo column in columns)
                {
                    tmpRow.AddColumnValue(column.ColumnName, rowValues[count]);
                    count++;
                }
                rows.Add(tmpRow);
            }

            return rows.ToArray();
        }

        /// <summary>
        /// Insert method to add new row
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="values">Array containing information for inserting into the DB</param>
        public void Insert(string tableName, params string[] values)
        {
            if (!Exists(tableName))
                throw new Exception("Table does not exists.");

            if (!HasColumnsDefinition(tableName))
                throw new Exception("Table does not have columns definitions.");

            ColumnInfo[] columns = GetColumns(tableName);
            string fileContent = GetContent(tableName);
            List<string> finalValues = new List<string>();
            int count = 0;

            foreach (ColumnInfo column in columns)
            {
                if (column.Auto_Increment)
                {
                    finalValues.Add(GetNextAutoIncrement(tableName, column));
                }
                else
                {
                    finalValues.Add(values[count]);
                    count++;
                }
            }

            using (StreamWriter w = new StreamWriter(File.OpenWrite(TablePath(tableName))))
            {
                w.Write(fileContent + Environment.NewLine + String.Join(deliminator.ToString(), finalValues));
            }

            OnInserted(EventArgs.Empty);
        }

        /// <summary>
        /// Returns true if table exists
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns>true if table exists or false if not</returns>
        public bool Exists(string tableName)
        {
            return File.Exists(TablePath(tableName));
        }

        public ColumnInfo[] GetColumns(string tableName)
        {
            if (!HasColumnsDefinition(tableName))
                throw new Exception("Table does not have columns definitions.");

            string[] tableLines = GetLines(tableName);
            string[] columns = tableLines[0].Split(deliminator);
            List<ColumnInfo> infos = new List<ColumnInfo>();
            foreach (string column in columns)
            {
                ColumnInfo info = new ColumnInfo();
                info.ColumnName = column;
                string[] columnProperties = column.Split(deliminatorInfo);
                if (columnProperties.Length > 1)
                {
                    info.ColumnName = columnProperties[0];
                    switch (columnProperties[1].ToLower())
                    {
                        case "auto_increment":
                            info.Auto_Increment = true;
                            break;
                    }
                }
                infos.Add(info);
            }
            return infos.ToArray<ColumnInfo>();
        }

        /// <summary>
        /// Gets the number of rows contained in the table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns>The number of rows</returns>
        public int Count(string tableName)
        {
            int count = File.ReadAllLines(TablePath(tableName)).Length - 1;
            if (count <= 0)
                return 0;
            else
                return count;
        }

        private string GetNextAutoIncrement(string tableName, ColumnInfo column)
        {
            if (!Exists(tableName))
                throw new Exception("Table does not exists.");

            if (!HasColumnsDefinition(tableName))
                throw new Exception("Table does not have columns definitions.");

            if (Count(tableName) == 0)
                return "1";

            string[] tableLines = GetLines(tableName);
            string[] lastLineValues = tableLines[tableLines.Length - 1].Split(deliminator);
            ColumnInfo[] columns = GetColumns(tableName);
            int columnNumber = Array.FindIndex(columns, c => c.ColumnName == column.ColumnName);

            return (Convert.ToInt32(lastLineValues[columnNumber]) + 1).ToString();
        }

        private bool HasColumnsDefinition(string tableName)
        {
            string[] tableLines = GetLines(tableName);
            try
            {
                return tableLines[0].Contains(deliminator);
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }
        }

        private string GetContent(string tableName)
        {
            return File.ReadAllText(TablePath(tableName));
        }

        private string[] GetLines(string tableName)
        {
            return File.ReadAllLines(TablePath(tableName));
        }

        private string TablePath(string tableName)
        {
            return tableName + ".sdb";
        }

        private void OnInserted(EventArgs e)
        {
            if (OnInsert != null)
                OnInsert(this, e);
        }

    }

}

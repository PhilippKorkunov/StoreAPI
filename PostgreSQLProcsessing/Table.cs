using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class Table : ICloneable
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; private set; }
        public string[] ColumnNames { get; private set; }
        public List<string> Intersections { get; set; }

        public Table(string tableName) 
        {
            TableName = tableName;
            ColumnNames = FindColumns();
            PrimaryKey = FindPKey();
            Intersections = new List<string>();
        }

        public Table(string tableName, string primaryKey, string[] columnNames) 
        {
            TableName = tableName;
            PrimaryKey = primaryKey;
            ColumnNames = columnNames;
            Intersections = new List<string>();
        }

        public object Clone()
        {
            return new Table(TableName, PrimaryKey, ColumnNames);
        }

        public static HashSet<string> FindAllTableNames()
        {
            string findColumnsColumnCommand = "SELECT table_name FROM information_schema.tables\r\n" +
             "WHERE table_schema NOT IN ('information_schema','pg_catalog')";
            HashSet<string> tables = new HashSet<string>();

            var keyTable = new SelectRequest(findColumnsColumnCommand).Execute();
            if (keyTable is not null)
            {
                foreach (DataRow row in keyTable.Rows)
                {
                    if (row["table_name"] is not null)
                    {
                        string? rowStr = row["table_name"].ToString();
                        if (!string.IsNullOrEmpty(rowStr))
                        {
                            tables.Add(rowStr);
                        }
                    }
                }
            }

            return tables;
        }

        public bool isTableConatinsColumn(string columnName) => ColumnNames.Contains(columnName);

        public bool isColumnPKey(string columnName) => PrimaryKey == columnName;

        public static string? IntersectTables(Table table1, Table table2)
        {
            var tableIntesection = (table1.ColumnNames.Intersect(table2.ColumnNames)).ToArray();
            if (tableIntesection.Count() > 1)
            {
                //throw new Exception("There are at least 2 intesrsctions");
                foreach (var intersect in tableIntesection)
                {
                    if (intersect == table1.PrimaryKey || intersect == table2.PrimaryKey)
                    {
                        return intersect;
                    }
                    else { return tableIntesection[0]; }
                }
            }
            else if (tableIntesection.Count() == 1)
            {
                return tableIntesection[0];
            }
            

            return null;
        }
           

        private string[] FindColumns()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["TableNames"] = TableName;
            dict["Limit"] = "1";
            dict["ColumnNames"] = " ";
            dict["isTable"] = "true";

            var table = new SelectRequest(dict).Execute();
            if (table is not null)
            {
                var columnNames = (from DataColumn column in table.Columns
                               select column.ColumnName).ToArray();
                return columnNames;
            }
            else
            {
                throw new Exception($"Table {TableName} is null");
            }
        }

        public string FindPKey()
        {
            string whereCondition = $"where TABLE_NAME = '{TableName}'";
            string pKeyCommand = $"SELECT TABLE_NAME, constraint_name\r\n|| '(' || string_agg(column_name, ',') || ')' as tableName\r\n" +
                $"FROM information_schema.constraint_column_usage \r\n {whereCondition}\r\nGROUP BY TABLE_NAME, \r\nconstraint_name;";
            var keyTable = new SelectRequest(pKeyCommand).Execute();
            if (keyTable is not null)
            {
                foreach (DataRow row in keyTable.Rows)
                {
                    if (row[0] is not null && row[1] is not null)
                    {
                        string? tableName = row[0].ToString();
                        string? constraintAndKey = row[1].ToString();
                        if (tableName is not null && constraintAndKey is not null)
                        {
                            if (constraintAndKey.Contains("pkey"))
                            {
                                var primaryKey = constraintAndKey.Split('(')[1].Split(')')[0];
                                return primaryKey;
                            }
                        }
                    }
                }
            }

            throw new Exception($"Primary key of {TableName} hasen't found");
        }
    }
}

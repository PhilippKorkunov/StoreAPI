using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class SelectRequest : Request, ICloneable
    {
        private bool isCommand;

        public override string Command 
        {
            get
            {
                if (isCommand) return base.Command;
                if (TableNames is not null && RequestColumns is not null)
                {
                    if (TableNames.Length == 1) 
                    { return $"select {String.Join(", ", RequestColumns)} from {TableNames[0]} {WhereCondition} {Limit}"; }
                    
                    return $"select {String.Join(", ", RequestColumns)} from {InnerJoinString} {WhereCondition} {Limit}";
                }

                return base.Command;
            }
            set => base.Command = value; 
        }
        public string WhereCondition { 
            get => whereCondition;
            set 
            {
                if (String.IsNullOrEmpty(value))
                {
                    whereCondition = string.Empty;
                }
                else if (value[0..5] == "where")
                {
                    whereCondition = value;
                }
                else
                {
                    whereCondition = $"where {value}";
                }
            } 
        }
        private string whereCondition;
        public string InnerJoinString { get; set; }
        public string[]? RequestColumns { get; set; }
        public string Limit { get; set; }
        private List<string>? UniqueColumns { get; set; }
        private Dictionary<string, Table>? CurrentTables { get; set; }
        public static new List<string> RequiredKeys { get; set; } = new List<string>() { "ColumnNames" };

        public SelectRequest(Dictionary<string, string> dict) : base(dict, true)
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }

            CheckDictCorrection(dict, RequiredKeys);

            isCommand = false;

            if (dict.ContainsKey("isTable") && dict["isTable"] == "true")
            {
                RequestColumns = String.IsNullOrWhiteSpace(dict["ColumnNames"]) ||
                String.IsNullOrEmpty(dict["ColumnNames"]) ? null : dict["ColumnNames"].Replace(" ", "").Split(',');
                
            }
            else
            {
                UniqueColumns = FindUniqueColumns();
                RequestColumns = String.IsNullOrWhiteSpace(dict["ColumnNames"]) ||
                String.IsNullOrEmpty(dict["ColumnNames"]) ? UniqueColumns.ToArray() : dict["ColumnNames"].Replace(" ", "").Split(',');
                UniqueColumns.Clear();
            }

            FillCurrentTables(dict);

            whereCondition = String.Empty;
            WhereCondition = dict.ContainsKey("WhereCondition") ? $"where {dict["WhereCondition"]}" : String.Empty;
            InnerJoinString = TableNames.Length == 1 ? String.Empty : BuildJoinString();
            Limit = dict.ContainsKey("Limit") ? $"Limit {dict["Limit"]}" : String.Empty;

            BuildSelectString();

            CurrentTables = null;
        }

        public SelectRequest(string[] tableNames, string[]? requestColumns, string innerJoinString, 
            string whereCondition, string limit) : base(true) 
        {
            TableNames = tableNames;
            isCommand = false;
            RequestColumns = requestColumns;
            this.whereCondition = String.Empty;
            WhereCondition = whereCondition;
            InnerJoinString = innerJoinString;
            Limit = limit;
        }

        public object Clone()
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }
            return new SelectRequest(TableNames, RequestColumns, InnerJoinString, WhereCondition, Limit);
        }

        private void FillCurrentTables(Dictionary<string, string> dict)
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }

            if (dict.ContainsKey("isTable") && dict["isTable"] == "true")
            {
                return;
            }
            else
            {
                CurrentTables = new Dictionary<string, Table>();
                foreach (var name in TableNames)
                {
                    Table table = (Table)Tables[name].Clone();
                    CurrentTables[name] = table;
                }
            }
        }

        private string FindTableOfColumn(string columnName)
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }

            foreach (string tableName in TableNames)
            {
                if (Tables[tableName].IsColumnPKey(columnName))
                {
                    return $"{tableName}.{columnName}";
                }
            }

            foreach (string tableName in TableNames)
            {
                if (Tables[tableName].IsTableConatinsColumn(columnName))
                {
                    return $"{tableName}.{columnName}";
                }
            }

            throw new Exception($"Column {columnName} hasn't found");
        }


        private string BuildJoinString()
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }
            if (CurrentTables is null) { throw new ArgumentNullException("CurrentTables must be non null", nameof(CurrentTables)); }

            for (int i = 0; i < TableNames.Length - 1; i++)
            {
                for (int j = i + 1; j < TableNames.Length; j++)
                {
                    var intersection = Table.IntersectTables(Tables[TableNames[i]], Tables[TableNames[j]]);
                    if (intersection is not null)
                    {
                        CurrentTables[TableNames[i]].Intersections.Add($"{TableNames[j]}.{intersection}");
                        CurrentTables[TableNames[j]].Intersections.Add($"{TableNames[i]}.{intersection}");
                    }
                }
            }

            var sotrtedCurrentTables = (from table in TableNames
                                        orderby CurrentTables[table].Intersections.Count descending
                                        select table).ToArray();

            string innerJoinString = $"{sotrtedCurrentTables[0]}";

            innerJoinString += " ";
            for (int i = 0; i < sotrtedCurrentTables.Length - 1; i++)
            {
                var intersections = new List<string>() { };
                intersections.AddRange(CurrentTables[sotrtedCurrentTables[i]].Intersections);
                foreach (var intersection in intersections)
                {
                    var secondTable = intersection.Split('.')[0];
                    var columnName = intersection.Split('.')[1];
                    if ((RequestColumns is null || RequestColumns.Contains(columnName)))
                    {
                        if (sotrtedCurrentTables[i] != secondTable)
                        {
                            innerJoinString += $"join {secondTable} on {sotrtedCurrentTables[i]}.{columnName} = {intersection} ";
                            CurrentTables[secondTable].Intersections.Remove($"{sotrtedCurrentTables[i]}.{columnName}");
                        }
                    }
                }
            }
            return innerJoinString;

        }

        private List<string> FindUniqueColumns()
        {
            if (TableNames is null) { throw new ArgumentNullException("TableNames must be non null", nameof(TableNames)); }

            var uniqueColumns = new HashSet<string>();

            if (TableNames.Length > 1)
            {
                foreach (var name in TableNames)
                {
                    uniqueColumns = uniqueColumns.Union(Tables[name].ColumnNames).ToHashSet();
                }
                return uniqueColumns.ToList();
            }
            
            return Tables[TableNames[0]].ColumnNames.ToList();
        }



        private void BuildSelectString()
        {
            if (TableNames is not null && TableNames.Length == 1)
            {
                isCommand = true;
                var columns = RequestColumns == null ? "*" : String.Join(", ", RequestColumns);
                Command = $"select {columns} from {TableNames[0]} {WhereCondition} {Limit}";
            }
            else
            {
                if (RequestColumns is null)
                {
                    if (UniqueColumns is null) { throw new ArgumentNullException("TableNames must be non null", nameof(UniqueColumns)); }
                    RequestColumns = UniqueColumns.ToArray();
                }

                for (int i = 0; i < RequestColumns.Length; i++)
                {
                    RequestColumns[i] = FindTableOfColumn(RequestColumns[i]);
                }
            }
        }
    }
}


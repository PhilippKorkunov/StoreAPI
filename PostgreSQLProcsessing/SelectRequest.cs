using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using System.Linq;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class SelectRequest : Request
    {
        public string WhereCondition { get; set; }
        public string GroupByColumn { get; set; }
        public string InnerJoinString { get; set; }
        public string[]? RequestColumns { get; set; }
        private List<string> UniqueColumns { get; set; }
        private Dictionary<string, List<string>> FoundColumns { get; set; }
        

        static SelectRequest()
        {
            RequiredKeys.Add("columnNames");
        }
        public SelectRequest(Dictionary<string, string> dict) : base(dict)
        {
            RequestColumns = String.IsNullOrWhiteSpace(dict["columnNames"]) ? null : dict["columnNames"].Replace(" ", "").Split(',');
            WhereCondition = dict.Keys.Contains("where") ? $"where {dict["where"]}" : String.Empty;
            GroupByColumn = dict.Keys.Contains("groupBy") ? $"group by {dict["groupBy"]}" : String.Empty;
            InnerJoinString = RequestColumns is not null && RequestColumns.Length > 1 ? "" : ""; 

            UniqueColumns = new List<string>();
            FoundColumns = FindColumns(TableNames);
            CheckColumnNamesRight();
        }

        private Dictionary<string, List<string>> FindColumns(string[] tableNames)
        {
            Dictionary<string, List<string>> columnsDict = new Dictionary<string, List<string>>();
            Task[] tasks = new Task[tableNames.Length];
            for (int i = 0; i < tableNames.Length; i++)
            {
                tasks[i] = new Task(() =>
                {
                    string findColumnsCommand = $"select * from {tableNames[i]} limit 1)";
                    var table = Execute(findColumnsCommand, true);
                    if (table is not null)
                    {
                        var columns = (from i in table.Columns[i].ColumnName
                                       select table.Columns[i].ColumnName).ToList<string>();
                        UniqueColumns.Intersect(columns);

                        columnsDict[tableNames[i]] = columns;
                    }
                    else
                    {
                        throw new Exception($"Table {tableNames[i]} is null");
                    }
                });
                tasks[i].Start();
            }
            Task.WaitAll(tasks);

            return columnsDict;
        }



        private void CheckColumnNamesRight()
        {
            if (RequestColumns is not null)
            {
                HashSet<string> unfoundColumns = (HashSet<string>)UniqueColumns.Except(RequestColumns);
                if (unfoundColumns.Count > 0)
                {
                    throw new Exception($"Some columns haven't found: [{String.Join(',', unfoundColumns)}]");
                }
                UniqueColumns.Clear();
            }
        }



        private string FindTableOfColumn(string columnName)
        {
            foreach (string tableName in FoundColumns.Keys)
            {
                if (FoundColumns[tableName].Contains(columnName))
                {
                    return $"{tableName}.{columnName}";
                }
            }

            throw new Exception($"Column {columnName} hasn't found");
        }



        private void BuildSelectString()
        {
            if (RequestColumns is null)
            {
                RequestColumns = UniqueColumns.ToArray();
            }

            string[] columns = new string[RequestColumns.Length];

            for (int i = 0; i < RequestColumns.Length; i++)
            {
                columns[i] = FindTableOfColumn(RequestColumns[i]);
            }

            Command = $"select {String.Join(", ", columns)}";
        }



        public DataTable? Execute() => Request.Execute(Command, isReader:true);
    }
}

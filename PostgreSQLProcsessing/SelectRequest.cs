using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

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
                if (TableNames.Length == 1) { return $"select {String.Join(", ", RequestColumns)} from {TableNames[0]} {WhereCondition} {Limit}"; }
                return  $"select {String.Join(", ", RequestColumns)} from {InnerJoinString} {WhereCondition} {Limit}";
            }
            set => base.Command = value; 
        }
        public string WhereCondition { 
            get => whereCondtion;
            set 
            {
                if (String.IsNullOrEmpty(value))
                {
                    whereCondtion = string.Empty;
                }
                else if (value.Substring(0, 5) == "where")
                {
                    whereCondtion = value;
                }
                else
                {
                    whereCondtion = $"where {value}";
                }
            } 
        }

        private string whereCondtion;
        public string InnerJoinString { get; set; }
        public string[]? RequestColumns { get; set; }
        public string Limit { get; set; }
        private List<string> UniqueColumns { get; set; }
        private Dictionary<string, Table> CurrentTables { get; set; }
        public static new List<string> RequiredKeys { get; set; } = new List<string>() { "ColumnNames" };

        public SelectRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckDictCorrection(dict, RequiredKeys);

            isCommand = false;

            if (dict.Keys.Contains("isTable") && dict["isTable"] == "true")
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

            WhereCondition = dict.Keys.Contains("WhereCondition") ? $"where {dict["WhereCondition"]}" : String.Empty;
            InnerJoinString = TableNames.Length == 1 ? "" : BuildJoinString();
            Limit = dict.Keys.Contains("Limit") ? $"Limit {dict["Limit"]}" : String.Empty;

            BuildSelectString(dict);

            if (CurrentTables is not null)
            {
                CurrentTables.Clear();
            }
        }

        public SelectRequest(string command) : base(command)
        {
            isCommand = true;
        }

        public SelectRequest(string[] tableNames, string[]? requestColumns, string innerJoinString, 
            string whereCondition, string limit) : base(tableNames) 
        {
            isCommand = false;
            RequestColumns = requestColumns;
            WhereCondition = whereCondition;
            InnerJoinString = innerJoinString;
            Limit = limit;
            //Command = request.Command;
        }

        public object Clone()
        {
            return new SelectRequest(TableNames, RequestColumns, InnerJoinString, WhereCondition, Limit);
        }

        private void FillCurrentTables(Dictionary<string, string> dict)
        {
            if (dict.Keys.Contains("isTable") && dict["isTable"] == "true")
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

            foreach (string tableName in TableNames)
            {
                if (Tables[tableName].isColumnPKey(columnName))
                {
                    return $"{tableName}.{columnName}";
                }
            }

            foreach (string tableName in TableNames)
            {
                if (Tables[tableName].isTableConatinsColumn(columnName))
                {
                    return $"{tableName}.{columnName}";
                }
            }

            throw new Exception($"Column {columnName} hasn't found");
        }


        private string BuildJoinString()
        {
            for (int i = 0; i < TableNames.Length - 1; i++)
            {
                for (int j = 1; j < TableNames.Length; j++)
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
                                        orderby CurrentTables[table].Intersections.Count
                                        select table).ToArray();

            string innerJoinString = $"{sotrtedCurrentTables[0]}";

            int t = 0;
            for (int j = sotrtedCurrentTables.Length - 1; j >= 0; j--)
            {
                if (CurrentTables[sotrtedCurrentTables[j]].Intersections.Count > 0)
                {
                    t = j;
                    break;
                }
                else
                {
                    innerJoinString += $", {sotrtedCurrentTables[j]}";
                }
            }

            innerJoinString += " ";

            for (int i = 0; i < t; i++)
            {
                var intersections = new List<string>() { };
                intersections.AddRange(CurrentTables[sotrtedCurrentTables[i]].Intersections);
                foreach (var intersection in intersections)
                {
                    var secondTable = intersection.Split('.')[0];
                    var columnName = intersection.Split('.')[1];
                    if (RequestColumns is null || RequestColumns.Contains(columnName))
                    {
                        innerJoinString += $"join {secondTable} on {sotrtedCurrentTables[i]}.{columnName} = {intersection} ";
                        CurrentTables[secondTable].Intersections.Remove($"{sotrtedCurrentTables[i]}.{columnName}");
                    }
                }
            }

            return innerJoinString;

        }

        private List<string> FindUniqueColumns()
        {
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



        private void BuildSelectString(Dictionary<string, string> dict)
        {
            if (TableNames.Length == 1)
            {
                isCommand = true;
                var columns = RequestColumns == null ? "*" : String.Join(", ", RequestColumns);
                Command = $"select {columns} from {TableNames[0]} {WhereCondition} {Limit}";
            }
            else
            {
                if (RequestColumns is null)
                {
                    RequestColumns = UniqueColumns.ToArray();
                }

                for (int i = 0; i < RequestColumns.Length; i++)
                {
                    RequestColumns[i] = FindTableOfColumn(RequestColumns[i]);
                }

                //Command = $"select {String.Join(", ", RequestColumns)} from {InnerJoinString} {WhereCondition} {Limit}";
            }

        }


        public DataTable? Execute() => Request.Execute(Command, isReader: true);
    }
}


﻿using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class UpdateRequest : Request
    {
        public string WhereCondition { get; set; }
        public string[] IdList { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }

        static UpdateRequest()
        {
            RequiredKeys.Add("Id");
            RequiredKeys.Add("columnNames");
            RequiredKeys.Add("Values");
        }
        public UpdateRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            ColumnNames = dict["columnNames"].Replace(" ", "").Split(',').ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);
            Values = dict["Values"].Replace(" ", "").Split(',').ToArray();
            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);
            Command = $"update {TableNames[0]} set ({String.Join(", ", ColumnNames)} values ({String.Join(", ", Values)})) {WhereCondition}";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
        
    }
}

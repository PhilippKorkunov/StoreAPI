﻿using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class InsertRequest : Request
    {
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }
        static InsertRequest()
        {
            RequiredKeys.Add("сolumnNames");
            RequiredKeys.Add("values");
        }

        public InsertRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckTableNamesLength(1);
            ColumnNames = dict["columnNames"].Replace(" ", "").Split(',').ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);
            Values = dict["values"].Replace(" ", "").Split(',').ToArray();
            Command = $"insert into {TableNames[0]} ({String.Join(", ", ColumnNames)} values {String.Join(", ", Values)})";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

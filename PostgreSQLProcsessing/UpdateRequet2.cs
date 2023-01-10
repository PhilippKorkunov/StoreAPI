﻿using Newtonsoft.Json.Linq;
using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class UpdateRequest2 : Request
    {
        public string WhereCondition { get; set; }
        public string[] IdList { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }

        private protected List<string> RequiredKeys { get; set; } = new List<string>() { "Id" };


        public UpdateRequest2(Dictionary<string, string> dict) : base(dict)
        {
            //CheckDictCorrection(dict);

            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            dict.Remove("Id");
            ColumnNames = dict.Keys.ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);
            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);
            Values = (from key in dict.Keys
                      select $"'{dict[key]}'").ToArray();
            Command = $"update {TableNames[0]} set ({String.Join(", ", ColumnNames)} values ({String.Join(", ", Values)})) {WhereCondition}";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);

    }
}
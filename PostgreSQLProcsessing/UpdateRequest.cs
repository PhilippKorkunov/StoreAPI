using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class UpdateRequest : Request
    {
        public string WhereCondition { get; set; }
        public string[] IdList { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }
        private protected static new List<string> RequiredKeys { get; set; } = new List<string>() { "Id", "ColumnNames", "Values" };

        public UpdateRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);

            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            ColumnNames = dict["ColumnNames"].Replace(" ", "").Split(',').ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);

            Values = dict["Values"].Replace(" ", "").Split(',').ToArray();
            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);

            Values = (from key in dict.Keys
                      select $"{key} = '{dict[key]}'").ToArray();
            Command = $"update {TableNames[0]} set {String.Join(",", Values)} {WhereCondition}";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
        
    }
}

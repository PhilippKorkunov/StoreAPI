using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class UpdateRequest2 : Request
    {
        public string WhereCondition { get; set; }
        public string[] IdList { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }

        private protected static new List<string> RequiredKeys { get; set; } = new List<string>() { "Id" };


        public UpdateRequest2(Dictionary<string, string> dict) : base(dict, false)
        {
            if (TableNames is null) { throw new ArgumentNullException(nameof(TableNames), "TableNames must be non null"); }

            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);

            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            dict.Remove("Id");

            ColumnNames = dict.Keys.ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);

            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);
            Values= dict.Values.ToArray();

            var setString  = (from key in dict.Keys
                      select $"{key} = '{dict[key]}'").ToArray();
            Command = $"update {TableNames[0]} set {String.Join(",", setString)} {WhereCondition}";
        }
    }
}

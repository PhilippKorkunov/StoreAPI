

namespace StoreAPI.PostgreSQLProcsessing
{
    public class DeleteRequest : Request
    {
        public string? WhereCondition { get; set; }
        public string[]? IdList { get; set; }
        private protected static new List<string> RequiredKeys { get; set; } = new List<string>() { "Id" };
        public DeleteRequest(Dictionary<string, string> dict) : base(dict, false)
        {
            if (dict is null) { throw new ArgumentNullException(nameof(dict), "Dict must be non null"); }
            if (TableNames is null) { throw new ArgumentNullException(nameof(TableNames), "TableNames must be non null"); }

            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);
            Command = $"delete from {TableNames[0]} {WhereCondition}";
        }
    }
    // 
}

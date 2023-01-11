using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class DeleteRequest : Request
    {
        public string WhereCondition { get; set; }
        public string[] IdList { get; set; }
        private protected static new List<string> RequiredKeys { get; set; } = new List<string>() { "Id" };
        public DeleteRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToArray();
            WhereCondition = BuildWhereConditionWithId(TableNames[0], IdList);
            Command = $"delete from {TableNames[0]} {WhereCondition}";
        }


        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }

    // 
}

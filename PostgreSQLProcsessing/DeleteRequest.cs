using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class DeleteRequest : Request
    {
        public string WhereCondition { get; set; }
        public List<string> IdList { get; set; }

        static DeleteRequest()
        {
            RequiredKeys.Add("Id");
        }
        public DeleteRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToList();
            WhereCondition = BuildWhereConditionWithId(IdList);
            Command = $"delete from {TableNames[0]} {WhereCondition}";
        }


        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

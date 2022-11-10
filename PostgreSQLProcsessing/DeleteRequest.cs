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
            WhereCondition = BuildWhereCondition();
            Command = $"delete from {TableNames[0]} {WhereCondition}";
        }

        

        private string BuildWhereCondition()
        {
            var list = from id in IdList
                       select $"{KeyDict["pk"][TableNames[0]]} = {id}";
            return $"where {String.Join(" or ", list)}";
        }


        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

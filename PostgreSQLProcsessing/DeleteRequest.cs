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
            IdList = dict["Id"].Replace(" ", "").Split(',').ToList();
            WhereCondition = BuildWhereCondition();
            CheckTableNamesLength();
            Command = $"delete from {TableNames[0]} {WhereCondition}";
        }

        private void CheckTableNamesLength()
        {
            if (TableNames.Length != 1)
            {
                throw new Exception("It's required to post only one table name!!!");
            }
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

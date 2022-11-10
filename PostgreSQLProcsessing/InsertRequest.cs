using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class InsertRequest : Request
    {
        public List<string> Columns { get; set; }
        public List<string> Values { get; set; }
        static InsertRequest()
        {
            RequiredKeys.Add("сolumnNames");
            RequiredKeys.Add("values");
        }

        public InsertRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckTableNamesLength(1);
            Columns = dict["Columns"].Replace(" ", "").Split(',').ToList();
            Values = dict["Values"].Replace(" ", "").Split(',').ToList();
            Command = $"insert into {TableNames[0]} ({String.Join(", ", Columns)} values {String.Join(", ", Values)})";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class InsertRequest : Request
    {
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }

        private protected static new List<string> RequiredKeys { get; set; } = new List<string>() { "ColumnNames", "Values" };

        public InsertRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);

            ColumnNames = dict["ColumnNames"].Replace(" ", "").Split(',').ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);

            Values = dict["Values"].Replace(" ", "").Split(',').ToArray();
            Values = (from value in Values
                     select $"'{value}'").ToArray();
            Command = $"insert into {TableNames[0]} ({String.Join(", ", ColumnNames)} values {String.Join(", ", Values)})";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

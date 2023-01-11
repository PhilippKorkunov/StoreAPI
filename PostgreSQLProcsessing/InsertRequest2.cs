using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class InsertRequest2 : Request
    {
        public string[] ColumnNames { get; set; }
        public string[] Values { get; set; }

        private protected static new List<string> RequiredKeys { get; set; } = new List<string>();

        public InsertRequest2(Dictionary<string, string> dict) : base(dict)
        {
            CheckDictCorrection(dict, RequiredKeys);
            CheckTableNamesLength(1);
            ColumnNames = dict.Keys.ToArray();
            CheckColumnNamesRight(TableNames[0], ColumnNames);
            Values = (from key in dict.Keys
                      select $"'{dict[key]}'").ToArray();
            Command = $"insert into {TableNames[0]} ({String.Join(", ", ColumnNames)}) values ({String.Join(", ", Values)})";
        }

        public DataTable? Execute() => Request.Execute(Command, isReader: false);
    }
}

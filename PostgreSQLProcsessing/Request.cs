using Npgsql;
using System.Data;
using System.Security.AccessControl;

namespace StoreAPI.PostgreSQLProcsessing
{
    public abstract class Request
    {
        public virtual string Command { get; set; }
        public string[] TableNames { get; set; }
        private protected static List<string> RequiredKeys { get; set; }
        private protected static string ConnectionString { get; private set; }
        private protected static HashSet<string> AllTableNames { get; set; }
        private protected static Dictionary<string, Table> Tables { get; set; }

        static Request()
        {
            RequiredKeys = new List<string>() { "tableNames"};

            string server = "localhost";
            string port = "5432";
            string username = "postgres";
            string password = "qwe123@";
            string dataBase = "StoreDB";
            ConnectionString = $"Server={server}; Port={port}; User Id={username}; Password={password}; Database={dataBase}";
            AllTableNames = Table.FindAllTableNames();
            Tables = new Dictionary<string, Table>();
            foreach (var name in AllTableNames)
            {
                Tables[name] =  new Table(name);
            }
        }

        public Request(Dictionary<string, string> dict)
        {
            CheckDictCorrection(dict);
            TableNames = dict["tableNames"].Replace(" ", "").Split(',');
            dict.Remove("tableNames");
            CheckTableNamesRight();
            Command = String.Empty;
        }

        public Request(string command)
        {
            Command = command;
        }


        private protected static DataTable? Execute(string cmd, bool isReader = false)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(cmd, connection);
                var dataTable = new DataTable();
                if (isReader)
                {
                    dataTable.Load(command.ExecuteReader());
                }
                else
                {
                    dataTable = null;
                    command.ExecuteNonQuery();
                }
                connection.Close();
                return dataTable;
            }
        }


        private protected string BuildWhereConditionWithId(string tableName, string[] idList)
        {
            var list = from id in idList
                       select $"{Tables[tableName].PrimaryKey} = {id}";
            return $"where {String.Join(" or ", list)}";
        }



        //Check Methods

        private protected void CheckTableNamesLength(int tablesNumber)
        {
            if (TableNames.Length != tablesNumber)
            {
                throw new Exception($"It's required to post only fixed number of tables: {tablesNumber}!!!");
            }
        }


        private protected static void CheckDictCorrection(Dictionary<string, string> dict)
        {
            int requiredColumnsFound = 0;
            foreach (var key in dict.Keys)
            {
                if (RequiredKeys.Contains(key)) { requiredColumnsFound++; }

                string str = dict[key];

                for (int i = 0; i < str.Length; i++)
                {
                    if (!(Char.IsDigit(str[i]) || str[i] == ' ' || str[i] == ',' || Char.IsLetter(str[i]) || str[i] == '_' || str[i] == '='))
                    {
                        throw new Exception($"Symbol '{str[i]}' was incorrect. Use ',' as a delimetr.");
                    }
                }
            }

            if (requiredColumnsFound != RequiredKeys.Count)
            {
                throw new Exception($"Some required keys weren't found.\nList of required keys: [{String.Join(',', RequiredKeys)}]");
            }
        }

        private void CheckTableNamesRight()
        {
            var incorrectTables = TableNames.Except(AllTableNames);
            if (incorrectTables.Count() > 0)
            {
                throw new Exception($"Some table names were incorrect. List of incorrect input: [{String.Join(',', incorrectTables)}]");
            }
        }

        private protected void CheckColumnNamesRight(string tableName, string[] columnNames)
        {
            var incorrectColumns = columnNames.Except(Tables[tableName].ColumnNames);
            if (incorrectColumns.Count() > 0)
            {
                throw new Exception($"Some table names were incorrect. List of incorrect input: [{String.Join(',', incorrectColumns)}]");
            }
        }
    }
}


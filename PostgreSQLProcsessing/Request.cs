using Npgsql;
using System.Data;
using System.Security.AccessControl;

namespace StoreAPI.PostgreSQLProcsessing
{
    public abstract class Request
    {
        public string Command { get; set; }
        public string[] TableNames { get; set; }
        private protected static List<string> RequiredKeys {get; set;}
        public static string ConnectionString { get; private set; }
        private protected static HashSet<string> AllTables {get; set;}
        public static Dictionary<string, Dictionary<string, string>> KeyDict { get; set;}

        static Request()
        {
            RequiredKeys = new List<string>() { "tableNames"};

            string server = "localhost";
            string port = "5432";
            string username = "postgres";
            string password = "qwe123@";
            string dataBase = "ShopDB";
            ConnectionString = $"Server={server}; Port={port}; User Id={username}; Password={password}; Database={dataBase}";
            AllTables = FindAllTables();
            KeyDict = FindKeys();
        }
    

        public Request(Dictionary<string, string> dict)
        {
            CheckDictCorrection(dict);
            TableNames = dict["tableNames"].Replace(" ", "").Split(',');
            CheckTableNamesRight();
            Command = String.Empty;
        }




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
                    if (!(Char.IsDigit(str[i]) || str[i] == ' ' || str[i] == ',' || Char.IsLetter(str[i])))
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
            var incorrectTables = TableNames.Except(AllTables);
            if (incorrectTables.Count() > 0)
            {
                throw new Exception($"Some table names were incorrect. List of incorrect input: [{String.Join(',', incorrectTables)}]");
            }
        }

        private protected string BuildWhereConditionWithId(List<string> idList)
        {
            var list = from id in idList
                       select $"{KeyDict["pk"][TableNames[0]]} = {id}";
            return $"where {String.Join(" or ", list)}";
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

        private static Dictionary<string, Dictionary<string, string>> FindKeys()
        {
            var tables = from table in AllTables
                         select $"TABLE_NAME = '{table}'";
            string whereCondition = $"where {String.Join(" or ", tables)}";
            string command = $"SELECT TABLE_NAME, constraint_name\r\n|| '(' || string_agg(column_name, ',') || ')' as tableName\r\n" +
                $"FROM information_schema.constraint_column_usage \r\n {whereCondition}\r\nGROUP BY TABLE_NAME, \r\nconstraint_name;";

            var keyTable = Execute(command, true);

            Dictionary<string, string>? pKeyDict = new Dictionary<string, string>();
            Dictionary<string, string>? fKeyDict = new Dictionary<string, string>();
            var keys = new Dictionary<string, Dictionary<string, string>>()
            {
                {"fk", fKeyDict},
                {"pk", pKeyDict},
            };


            Task[] tasks = new Task[keyTable.Rows.Count];
            if (keyTable is not null)
            {
                int i = 0;
                foreach (DataRow row in keyTable.Rows)
                {
                    tasks[i] = new Task(() =>
                    {
                        if (row[0] is not null && row[1] is not null)
                        {
                            string tableName = row[0].ToString();
                            string constraintAndKey = row[1].ToString();
                            if (constraintAndKey.Contains("fk"))
                            {
                                keys["fk"].Add(tableName, constraintAndKey.Split('(')[1].Split(')')[0]);
                            }
                            else if (constraintAndKey.Contains("pkey"))
                            {
                                keys["pk"].Add(tableName, constraintAndKey.Split('(')[1].Split(')')[0]);
                            }
                        }
                    });
                    tasks[i].Start();
                    i++;
                }

                Task.WaitAll(tasks);
            }

            return keys;
        }

        private static HashSet<string> FindAllTables()
        {
            string findColumnsColumn = "SELECT table_name FROM information_schema.tables\r\n" +
             "WHERE table_schema NOT IN ('information_schema','pg_catalog')";
            HashSet<string> tables = new HashSet<string>();

            var keyTable = Execute(findColumnsColumn, true);
            if (keyTable is not null)
            {
                foreach (DataRow row in keyTable.Rows)
                {
                    if (row["table_name"] is not null)
                    {
                        tables.Add(row["table_name"].ToString());
                    }
                }
            }

            return tables;
        }

    }
}

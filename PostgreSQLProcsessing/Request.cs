using System.Data;

namespace StoreAPI.PostgreSQLProcsessing
{
    public class Request : AbstractRequest
    {
        private bool IsReader {get;set;}
        public Request(Dictionary<string, string>? dict, bool isReader) : base(dict)
        {
            IsReader = isReader;
        }

        public Request(string command, bool isReader) : base(command)
        {
            IsReader = isReader;
        }

        public Request(bool isReader) : base() 
        {
            IsReader = isReader;
        }

        public async Task<DataTable?> ExecuteAsync() => await AbstractRequest.ExecuteAsync(Command, isReader: IsReader);
        public DataTable? Execute() => AbstractRequest.Execute(Command, isReader: IsReader);
    }
}

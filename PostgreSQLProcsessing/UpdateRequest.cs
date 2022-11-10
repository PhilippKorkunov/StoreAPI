namespace StoreAPI.PostgreSQLProcsessing
{
    public class UpdateRequest : Request
    {
        public string WhereCondition { get; set; }
        public List<string> IdList { get; set; }
        public UpdateRequest(Dictionary<string, string> dict) : base(dict)
        {
            CheckTableNamesLength(1);
            IdList = dict["Id"].Replace(" ", "").Split(',').ToList();
        }
    }
}

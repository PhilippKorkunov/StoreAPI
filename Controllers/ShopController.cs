using Microsoft.AspNetCore.Mvc;
using StoreAPI.PostgreSQLProcsessing;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text;
using System.Data;
using System.Web.Http.Cors;

namespace StoreAPI.Controllers
{
    [EnableCors("*","*","*")]
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : Controller
    {
        private static SelectRequest AdminRequest { get; set; }
        private static SelectRequest CustomerRequest { get; set; }
        private static SelectRequest OrderRequest { get; set; }
        private static SelectRequest ProductRequest { get; set; }
        private static SelectRequest LogRequest { get; set; }


        static StoreController()
        {
             AdminRequest = new SelectRequest(CreateDict("SelectAdmin"));
             CustomerRequest = new SelectRequest(CreateDict("SelectCustomer"));
             OrderRequest = new SelectRequest(CreateDict("SelectOrder"));
             ProductRequest = new SelectRequest(CreateDict("SelectProductList"));
             LogRequest = new SelectRequest(CreateDict("SelectLog"));
        }

        private static Dictionary<string, string> CreateDict(string key)
        {
            Dictionary<string, string> dict = new();
            switch (key)
            {
                case "SelectCustomer":
                    dict["TableNames"] = "customer";
                    dict["ColumnNames"] = "id_customer, email, password, name, surname, patronymic";
                    break;
                case "SelectAdmin":
                    dict["TableNames"] = "administrator";
                    dict["ColumnNames"] = "email, password";
                    break;
                case "SelectOrder":
                    dict["TableNames"] = "store_order, product";
                    dict["ColumnNames"] = "id_product, title, price, id_order, id_customer";
                    break;
                case "SelectProductList":
                    dict["TableNames"] = "product";
                    dict["ColumnNames"] = "";
                    break;
                case "SelectLog":
                    dict["TableNames"] = "main_log, product_log, product, customer";
                    dict["ColumnNames"] = "id_log, datetime, id_product_log, id_product, title, price, id_customer, email";
                    break;
                default:
                    break;
            }

            return dict;
        }


        [HttpGet("Select")]
        public async Task<IActionResult> Select()
        {
            Dictionary<string, string> dict = new();
            return await GetExecuteResult(request : new SelectRequest(dict));
        }

        [HttpGet("SelectUser")]
        public async Task<IActionResult> SelectUser(string login, string password, bool isAdmin)
        {
            var userRequest = isAdmin? (SelectRequest)AdminRequest.Clone() : (SelectRequest)CustomerRequest.Clone();
            userRequest.WhereCondition = $"email = '{login}' and password = '{password}'";
            return await GetExecuteResult(request: userRequest);
        }

        [HttpGet("SelectProductList")]
        public async Task<IActionResult> SelectProductList() => await GetExecuteResult(request: (SelectRequest)ProductRequest.Clone());


        [HttpGet("SelectOrder")]
        public async Task<IActionResult> SelectOrder(string idCustomer, bool isJson = true)
        {
            var orderRequest = (SelectRequest)OrderRequest.Clone();
            orderRequest.WhereCondition = $"id_customer = '{idCustomer}'";
            return await GetExecuteResult(request: orderRequest, isJson: isJson);
        }

        [HttpGet("SelectLog")]
        public async Task<IActionResult> SelectLog() => await GetExecuteResult(request: LogRequest);
    

        [HttpGet("PushOrder")]
        public async Task<IActionResult> PushOrderAsync(string idCustomer)
        {
            var selectResult = (OkObjectResult) await SelectOrder(idCustomer, false);
            if (selectResult.Value is null) { throw new ArgumentNullException(nameof(selectResult.Value), "Data Table must be non null"); }
            DataTable orderTable = (DataTable)selectResult.Value;

            var productIdArray = (from DataRow row in orderTable.Rows
                                  select new List<object>() { row["id_product"], row["id_order"] }).ToList();

            Dictionary<string, string> dict = new()
                {
                    { "TableNames", "main_log" },
                    { "datetime", DateTime.Now.ToString()},
                    { "id_customer", idCustomer}
                };
            await GetExecuteResult(request: new InsertRequest2(dict));

            Dictionary<string, string> selectDict = new()
                {
                    { "TableNames", "main_log" },
                    { "ColumnNames", "" },
                    { "WhereCondition", $"datetime = '{dict["datetime"]}'" }
                };

            var result = (OkObjectResult)(await GetExecuteResult(new SelectRequest(selectDict), true));
            if (result.Value is null) { throw new ArgumentNullException(nameof(result.Value), "Data Table must be non null"); }

            DataTable table = (DataTable)result.Value;
            DataRow tableRow = table.Rows[0];
            string? idLog = tableRow["id_log"].ToString();
            if (idLog is null) { throw new ArgumentNullException(nameof(idLog), "id_log must be non null"); }

            dict.Remove("datetime");
            dict.Remove("id_customer");

            foreach (var product in productIdArray)
            {
                dict["TableNames"] = "product_log";
                dict["id_log"] = idLog;
                var idProduct = product[index: 0].ToString();
                if (idProduct is null) { throw new ArgumentNullException(nameof(idProduct), "id_product must be non null"); }
                dict["id_product"] = idProduct;
                await GetExecuteResult(new InsertRequest2(dict));

                var id = product[index: 1].ToString();
                if (id is null) { throw new ArgumentNullException(nameof(id), "id_product must be non null"); }
                Dictionary<string, string> deleteDict = new()
                    {
                        { "TableNames",  "store_order"},
                        { "Id", id }
                    };
                await GetExecuteResult(new DeleteRequest(dict));
            }
            return Ok();
        }



        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(JsonDocument jsonDocument) => await GetExecuteResult(request: new InsertRequest(ToJsonDict(jsonDocument)));

        [HttpPost("Insert2")]
        public async Task<IActionResult> Insert2(JsonDocument jsonDocument) => await GetExecuteResult(request: new InsertRequest2(ToJsonDict(jsonDocument)));

        [HttpPut("Update")]
        public async Task<IActionResult> Update(JsonDocument jsonDocument) => await GetExecuteResult(request: new UpdateRequest(ToJsonDict(jsonDocument)));

        [HttpPut("Update2")]
        public async Task<IActionResult> Update2(JsonDocument jsonDocument) => await GetExecuteResult(request: new UpdateRequest2(ToJsonDict(jsonDocument)));

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(JsonDocument jsonDocument) => await GetExecuteResult(request: new DeleteRequest(ToJsonDict(jsonDocument)));




        private async Task<IActionResult> GetExecuteResult(Request request, bool isJson = true)
        {
            try
            {
                if (isJson)
                {
                    return Ok(JsonConvert.SerializeObject(await request.ExecuteAsync()));
                }
                else 
                {
                    return Ok(await request.ExecuteAsync());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private protected static string ToJsonString(JsonDocument? json)
        {
            if (json is null) { throw new ArgumentNullException(nameof(json), "Json must be non null"); }
            using var stream = new MemoryStream();
            Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true });
            json.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private protected static Dictionary<string, string> ToJsonDict(JsonDocument? json)
        {
            var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(ToJsonString(json));
            if (jsonDict is null) { throw new ArgumentNullException(nameof(jsonDict), "Json Dict must be non null"); }
            return jsonDict;
        }
    }


}


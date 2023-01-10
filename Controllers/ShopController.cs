using Microsoft.AspNetCore.Mvc;
using StoreAPI.PostgreSQLProcsessing;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;

namespace StoreAPI.Controllers
{
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
           /* AdminRequest = new SelectRequest(CreateDict("SelectAdmin"));
            CustomerRequest = new SelectRequest(CreateDict("SelectCustomer"));
            OrderRequest = new SelectRequest(CreateDict("SelectOrder"));
            ProductRequest = new SelectRequest(CreateDict("SelectProduct"));
            LogRequest = new SelectRequest(CreateDict("SelectLog"));*/
        }

        private static Dictionary<string, string> CreateDict(string key)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            switch (key)
            {
                case "SelectCustomer":
                    dict["TableNames"] = "customer";
                    dict["ColumnNames"] = "email, password";
                    break;
                case "SelectAdmin":
                    dict["TableNames"] = "customer";
                    dict["ColumnNames"] = "login, password";
                    break;
                case "SelectOrder":
                    dict["TableNames"] = "customer";
                    dict["ColumnNames"] = " ";
                    break;
                case "SelectProduct":
                    dict["TableNames"] = "product";
                    dict["ColumnNames"] = " ";
                    break;
                case "SelectLog":
                    //dict["TableNames"] = "main_log, product_log, product, customer";
                    //dict["columnNames"] = " ";
                    break;
                default:
                    break;
            }

            return dict;
        }


        [HttpGet("Select")]
        public IActionResult Select()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            /* dict["TableNames"] = "product, category, store_order";
             dict["columnNames"] = " ";*/
            return SqlRequestWithJson(method: "SelectDict", dict: dict);
        }

        [HttpGet("SelectUser")]
        public IActionResult SelectUser(string login, string password, bool isAdmin)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["TableNames"] = isAdmin ? "administrator" : "customer";
            dict["ColumnNames"] = isAdmin ? "login, password" : "email, password";
            dict["WhereCondition"] = isAdmin ? $"login = '{login}' and password = '{password}'" : $"email = '{login}'and password = '{password}'";
            var request = new SelectRequest(dict);
            return SqlRequestWithJson(method: "SelectCommand", command: request.Command);
        }

        [HttpGet("SelectProductList")]
        public IActionResult SelectProduct()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["TableNames"] = "product";
            dict["ColumnNames"] = " ";
            var request = new SelectRequest(dict);
            return SqlRequestWithJson(method: "SelectCommand", command: request.Command);
        }


        [HttpGet("SelectOrder")]
        public IActionResult SelectOrder(string idCustomer)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["TableNames"] = "store_order, product";
            dict["ColumnNames"] = "id_product, title, price, id_order, id_customer";
            dict["WhereCondition"] = $"id_customer = '{idCustomer}'";
            var request = new SelectRequest(dict);
            return SqlRequestWithJson(method: "SelectCommand", command: request.Command);
        }

        [HttpPost("Insert")]
        public IActionResult Insert(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument: jsonDocument, method: "Insert");

        [HttpPost("Insert2")]
        public IActionResult Insert2(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument: jsonDocument, method: "Insert2");

        [HttpPut("Update")]
        public IActionResult Update(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument: jsonDocument, method: "Update");

        [HttpPut("Update2")]
        public IActionResult Update2(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument: jsonDocument, method: "Update2");

        [HttpDelete("Delete")]
        public IActionResult Delete(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument: jsonDocument, method: "Delete");


        private IActionResult SqlRequestWithJson(JsonDocument? jsonDocument = null, Dictionary<string, string>? dict = null, string? method = null, string? command = null)
        {
            if (method is null) { return BadRequest("Method is null"); }

            try
            {
                if (jsonDocument is not null || dict is not null || command is not null)
                {
                    string json = ToJsonString(jsonDocument);
                    dict = dict is null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(json) : dict;
                    if (dict is not null || command is not null)
                    {
                        switch (method)
                        {
                            case "SelectDict":
                                SelectRequest selectDictRequest = new SelectRequest(dict);
                                return Ok(JsonConvert.SerializeObject(selectDictRequest.Execute()));
                            case "SelectCommand":
                                if (command is not null)
                                {
                                    SelectRequest selectCommandRequest = new SelectRequest(command);
                                    return Ok(JsonConvert.SerializeObject(selectCommandRequest.Execute()));
                                }
                                return BadRequest($"Command is null");
                            case "Delete":
                                DeleteRequest deleteRequest = new DeleteRequest(dict);
                                return Ok(deleteRequest.Execute());
                            case "Insert":
                                InsertRequest insertRequest = new InsertRequest(dict);
                                insertRequest.Execute();
                                break;
                            //return Ok(insertRequest.Execute());
                            case "Insert2":
                                InsertRequest2 insertRequest2 = new InsertRequest2(dict);
                                return Ok(insertRequest2.Execute());
                            //return Ok(insertRequest2.Execute());
                            case "Update":
                                UpdateRequest updateRequest = new UpdateRequest(dict);
                                return Ok(updateRequest.Execute());
                            case "Update2":
                                UpdateRequest2 updateRequest2 = new UpdateRequest2(dict);
                                return Ok(updateRequest2.Execute());
                            default:
                                return BadRequest();
                        }

                    }
                    return BadRequest("Dictionary of inputed json is null");
                }
                return BadRequest("Dictionaty or Json or Command is null");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string ToJsonString(JsonDocument? jdoc)
        {
            using (var stream = new MemoryStream())
            {
                if (jdoc is not null)
                {
                    Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                    jdoc.WriteTo(writer);
                    writer.Flush();
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            return string.Empty;
        }
    }


}


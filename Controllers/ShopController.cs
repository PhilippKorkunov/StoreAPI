using Microsoft.AspNetCore.Mvc;
using StoreAPI.PostgreSQLProcsessing;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;
//using Microsoft.AspNetCore.Cors;
using System.Web.Http.Cors;
using System.Data;
using System.Web.Http.Dispatcher;

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
        private DataTable? OrderTable { get; set; }
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
            Dictionary<string, string> dict = new Dictionary<string, string>();
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
        public IActionResult Select()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            /* dict["TableNames"] = "product, category, store_order";
             dict["columnNames"] = " ";*/
            return GetExecuteResult(method: "SelectDict", dict: dict);
        }

        [HttpGet("SelectUser")]
        public IActionResult SelectUser(string login, string password, bool isAdmin)
        {
            var requst = isAdmin? (SelectRequest)AdminRequest.Clone() : (SelectRequest)CustomerRequest.Clone();
            requst.WhereCondition = $"email = '{login}' and password = '{password}'";
            return GetExecuteResult(method: "SelectStaticCommand", selectRequest: requst);
        }

        [HttpGet("SelectProductList")]
        public IActionResult SelectProductList() => GetExecuteResult(method: "SelectStaticCommand", selectRequest: (SelectRequest)ProductRequest.Clone());


        [HttpGet("SelectOrder")]
        public IActionResult SelectOrder(string idCustomer)
        {
            var requst = (SelectRequest)OrderRequest.Clone();
            requst.WhereCondition = $"id_customer = '{idCustomer}'";
            return GetExecuteResult(method: "SelectStaticOrderCommand", selectRequest: requst);
        }

        [HttpGet("SelectLog")]
        public IActionResult SelectLog()
        {
            return GetExecuteResult(method: "SelectStaticCommand", selectRequest: (SelectRequest)LogRequest.Clone());
        }
    

        [HttpPost("PushOrder")]
        public IActionResult PushOrder(string idCustomer)
        {
            SelectOrder(idCustomer);
            if (OrderTable is not null) 
            {
                var productIdArray = (from DataRow row in OrderTable.Rows
                                     select new List<object>() { row["id_product"], row["id_order"] }).ToList();

                //return Ok(productIdArray);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["TableNames"] = "main_log";
                dict["datetime"] = DateTime.Now.ToString();
                dict["id_customer"] = idCustomer;
                new InsertRequest2(dict).Execute();

                Dictionary<string, string> selectDict = new Dictionary<string, string>();
                selectDict["TableNames"] = "main_log";
                selectDict["ColumnNames"] = "";
                selectDict["WhereCondition"] = $"datetime = '{dict["datetime"]}'";

                DataRow tableRow = new SelectRequest(selectDict).Execute().Rows[0];
                string idLog = tableRow["id_log"].ToString();

                dict.Remove("datetime");
                dict.Remove("id_customer");

                foreach (var product in productIdArray) 
                {
                    dict["TableNames"] = "product_log";
                    dict["id_log"] = idLog;
                    dict["id_product"] = product[0].ToString();
                    dict["id_order"] = product[1].ToString();

                    new InsertRequest2(dict).Execute();

                }
                return Ok(); 
            }

            return BadRequest();
        }



        [HttpPost("Insert")]
        public IActionResult Insert(JsonDocument jsonDocument) => GetExecuteResult(jsonDocument: jsonDocument, method: "Insert");

        [HttpPost("Insert2")]
        public IActionResult Insert2(JsonDocument jsonDocument) => GetExecuteResult(jsonDocument: jsonDocument, method: "Insert2");

        [HttpPut("Update")]
        public IActionResult Update(JsonDocument jsonDocument) => GetExecuteResult(jsonDocument: jsonDocument, method: "Update");

        [HttpPut("Update2")]
        public IActionResult Update2(JsonDocument jsonDocument) => GetExecuteResult(jsonDocument: jsonDocument, method: "Update2");

        [HttpDelete("Delete")]
        public IActionResult Delete(JsonDocument jsonDocument) => GetExecuteResult(jsonDocument: jsonDocument, method: "Delete");


        private IActionResult GetExecuteResult(JsonDocument? jsonDocument = null, Dictionary<string, string>? dict = null, 
            string? method = null, string? command = null, SelectRequest? selectRequest = null)
        {
            if (method is null) { return BadRequest("Method is null"); }

            try
            {
                if (jsonDocument is not null || dict is not null || command is not null || selectRequest is not null)
                {
                    string json = ToJsonString(jsonDocument);
                    dict = dict is null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(json) : dict;
                    if (dict is not null || command is not null || selectRequest is not null)
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
                            case "SelectStaticCommand":
                                if (selectRequest is not null)
                                {
                                    return Ok(JsonConvert.SerializeObject(selectRequest.Execute()));
                                }
                                return BadRequest("Select Request is null");
                            case "SelectStaticOrderCommand":
                                if (selectRequest is not null)
                                {
                                    OrderTable = selectRequest.Execute();
                                    return Ok(JsonConvert.SerializeObject(OrderTable));
                                }
                                return BadRequest("Select Request is null");
                            case "Delete":
                                DeleteRequest deleteRequest = new DeleteRequest(dict);
                                return Ok(deleteRequest.Execute());
                            case "Insert":
                                InsertRequest insertRequest = new InsertRequest(dict);
                                insertRequest.Execute();
                                return Ok(insertRequest.Execute());
                            case "Insert2":
                                InsertRequest2 insertRequest2 = new InsertRequest2(dict);
                                return Ok(insertRequest2.Execute());
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
                return BadRequest("Json is null");
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


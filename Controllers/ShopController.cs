using Microsoft.AspNetCore.Mvc;
using StoreAPI.PostgreSQLProcsessing;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : Controller
    {

        [HttpGet("Select")]
        public IActionResult Select()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
           /* dict["tableNames"] = "product, category, store_order";
            dict["columnNames"] = " ";*/
            return SqlRequestWithJson(method: "SelectDict", dict: dict);
        }

        [HttpGet("SelectUser")]
        public IActionResult SelectUser(string login, string password, bool isAdmin)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["tableNames"] = isAdmin ? "administrator" : "customer";
            dict["columnNames"] = isAdmin ? "login, password": "email, password";
            dict["whereCondition"] = isAdmin ? $"login = {login}, password = {password}" : $"email = {login}, password = {password}";
            var request = new SelectRequest(dict);
            return SqlRequestWithJson(method: "SelectCommand", command:request.Command);
        }


        [HttpGet("SelectOrder")]
        public IActionResult SelectOrder(string login, string password, bool isAdmin)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["tableNames"] = "store_order";
            dict["columnNames"] = " ";
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
        public IActionResult Delete(JsonDocument jsonDocument) => SqlRequestWithJson(jsonDocument:jsonDocument, method: "Delete");


        private IActionResult SqlRequestWithJson(JsonDocument? jsonDocument = null, Dictionary<string,string>? dict = null, string? method = null, string? command = null)
        {
            if (method is null) { return BadRequest("Method is null"); }

            try
            {
                if (jsonDocument is not null || dict is not null || command is not null)
                {
                    string json = ToJsonString(jsonDocument);
                    dict = dict is null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(json) : dict;
                    if (dict is not null)
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
                                    return Ok(selectCommandRequest.Execute());
                                }
                                return BadRequest($"command is null");
                            case "Delete":
                                DeleteRequest deleteRequest = new DeleteRequest(dict);
                                return Ok(deleteRequest.Execute());
                            case "Insert":
                                InsertRequest insertRequest = new InsertRequest(dict);
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
                return BadRequest("Dictionaty or Json is null");
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

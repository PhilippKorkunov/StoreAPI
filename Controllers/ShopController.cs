using Microsoft.AspNetCore.Mvc;
using StoreAPI.PostgreSQLProcsessing;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : Controller
    {

        [HttpGet("Select")]
        public IActionResult Select(JsonDocument jsonDocument) => SqlRequest(jsonDocument, MethodBase.GetCurrentMethod().Name);
 
        [HttpPost("Insert")]
        public IActionResult Insert(JsonDocument jsonDocument) => SqlRequest(jsonDocument, MethodBase.GetCurrentMethod().Name);

        [HttpDelete("Delete")]
        public IActionResult Delete(JsonDocument jsonDocument) => SqlRequest(jsonDocument, MethodBase.GetCurrentMethod().Name);


        public IActionResult SqlRequest(JsonDocument jsonDocument, string? method)
        {
            if (method is null) { return BadRequest("Method is null"); }

            try
            {
                if (jsonDocument is not null)
                {
                    string json = ToJsonString(jsonDocument);
                    Dictionary<string, string>? dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (dict is not null)
                    {
                        switch (method)
                        {
                            case "Delete":
                                DeleteRequest deleteRequest = new DeleteRequest(dict);
                                return Ok(deleteRequest.Execute());
                            case "Select":
                                SelectRequest selectRequest = new SelectRequest(dict);
                                return Ok();
                            default:
                                return BadRequest();
                        }

                    }
                    return BadRequest("Dictionary of inputed json is null");
                }
                return BadRequest("Json Document is null");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static string ToJsonString(JsonDocument jdoc)
        {
            using (var stream = new MemoryStream())
            {
                Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                jdoc.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }


}

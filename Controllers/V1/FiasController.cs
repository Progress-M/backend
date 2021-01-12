using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FiasController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<AuthController> _logger;
        private const string FiasUrl = "https://fias.nalog.ru";

        public FiasController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{text}")]
        [Produces("application/json")]
        public ActionResult Searsh(string text)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.QueryString.Add("text", text);
            string result = webClient.DownloadString($"{FiasUrl}/Search/Searching");
            var data = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(result);
            _logger.LogInformation("FIAS", data);

            return Ok(data.Select(d => d["PresentRow"]).ToArray());
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dadata;

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

        [HttpGet("address/{text}")]
        [Produces("application/json")]
        public ActionResult<List<string>> Searsh(string text)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.QueryString.Add("text", text);
            string result = webClient.DownloadString($"{FiasUrl}/Search/Searching");
            var data = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(result);
            _logger.LogInformation("FIAS", data);

            return Ok(data.Select(d => d["PresentRow"]).ToArray());
        }

        [HttpGet("inn/{inn}")]
        [Produces("application/json")]
        public async Task<ActionResult<List<Dadata.Model.Party>>> SearshINN(string inn)
        {
            var token = "32a50c1c500a4196a0d9ebfe04ba44b2724c83e3";
            var api = new SuggestClientAsync(token);
            var result = await api.FindParty(inn);
            _logger.LogInformation("INN", result.suggestions);

            return Ok(result.suggestions);
        }
    }
}

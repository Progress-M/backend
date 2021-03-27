using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using System.Linq;
using Main.Models;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FileController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<AuthController> _logger;

        public FileController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public ActionResult GetImage(int id)
        {
            var file = Context.Files.SingleOrDefault(f => f.Id == id);
            if (file == null)
            {
                return NotFound(
                    new ErrorResponse {
                        status = ErrorStatus.FileError,
                        message = $"Файл с id = '{id}' не найден."
                    }
                );
            }

            return File(file.bytes, "image/png");
        }
    }
}

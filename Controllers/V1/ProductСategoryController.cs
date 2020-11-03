using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductСategoryController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<ProductСategory> _logger;

        public ProductСategoryController(KindContext KindContext, ILogger<ProductСategory> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetProductСategory(int id)
        {
            var item = await Context.ProductСategory
               .AsNoTracking()
               .SingleOrDefaultAsync(category => category.Id == id);

            if (item == null)
            {
                return NotFound($"Not found product category with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetProductСategory()
        {
            return Ok(await Context.ProductСategory.ToListAsync());
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateProductСategory(ProductСategory productСategory)
        {
            Context.ProductСategory.Add(productСategory);
            await Context.SaveChangesAsync();
            return Ok(productСategory);
        }

    }
}

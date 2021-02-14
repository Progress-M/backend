using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductCategoryController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<ProductCategory> _logger;

        public ProductCategoryController(KindContext KindContext, ILogger<ProductCategory> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetProductCategory(int id)
        {
            var item = await Context.ProductCategory
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
        public async Task<ActionResult> GetProductCategory()
        {
            return Ok(await Context.ProductCategory.ToListAsync());
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateProductCategory(ProductCategory ProductCategory)
        {
            Context.ProductCategory.Add(ProductCategory);
            await Context.SaveChangesAsync();
            return Ok(ProductCategory);
        }

    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using Main.Function;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Reflection;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductCategoryController : Controller
    {
        readonly string subfolder = @"/image/product-category/";
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
        public async Task<ActionResult> CreateProductCategory([FromForm] ProductCategoryRequest request)
        {
            var category = new ProductCategory(request);
            Context.ProductCategory.Add(category);
            await Context.SaveChangesAsync();

            category.ImageName = await Utils.saveFile(request.image, $"{subfolder}", category.Id);

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> UpdateProductCategory(int id, [FromForm] ProductCategoryRequest request)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound($"Not found category with id = {id}");
            }

            category.Name = request.name;

            if (request.image != null)
            {
                Utils.deleteFile($"{subfolder}", category.ImageName);
                category.ImageName = await Utils.saveFile(request.image, $"{subfolder}", category.Id);
            }

            await Context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GetImage(int id)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound($"Not found category with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .Replace(Utils.subfolder, "");

            if (!System.IO.File.Exists($"{filePath}{subfolder}{category.ImageName}"))
            {
                return NotFound($"Not found file with name = '{category.ImageName}'");
            }

            var stream = System.IO.File.OpenRead($"{filePath}{subfolder}{category.ImageName}");
            return new FileStreamResult(stream, "image/jpeg");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdatImage(int id, [FromForm] ImageRequest imageRequest)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound($"Not found category with id = {id}");
            }

            Utils.deleteFile($"{subfolder}", category.ImageName);
            category.ImageName = await Utils.saveFile(imageRequest.image, $"{subfolder}", category.Id);
            await Context.SaveChangesAsync();

            return Ok();
        }

    }
}

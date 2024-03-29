using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Main.Models;
using System.Linq;

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
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult> GetProductCategory(int id)
        {
            var item = await Context.ProductCategory
               .AsNoTracking()
               .SingleOrDefaultAsync(category => category.Id == id);

            if (item == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.ProductCategoryError,
                        message = $"Не найдена категория с id = '{id}'"
                    }
                );
            }

            return Ok(item);
        }

        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        public ActionResult GetProductCategory()
        {
            return Ok(Context.ProductCategory.ToList().OrderBy(pc => pc.Priority));
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateProductCategory([FromForm] ProductCategoryRequest request)
        {
            var category = new ProductCategory(request);

            if (request.image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    request.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    category.Image = file;
                    await Context.SaveChangesAsync();
                }
            }
            Context.ProductCategory.Add(category);
            await Context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> UpdateProductCategory(int id, [FromForm] ProductCategoryRequest request)
        {
            var category = await Context.ProductCategory
                .Include(c => c.Image)
                .SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.ProductCategoryError,
                        message = $"Не найдена категория с id = '{id}'"
                    }
                );
            }

            category.Name = request.name;
            category.AgeLimit = request.ageLimit;
            category.Priority = request.priority;

            if (request.image != null)
            {
                if (category.Image != null)
                {
                    Context.Files.Remove(category.Image);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    request.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    category.Image = file;
                    await Context.SaveChangesAsync();
                }
            }

            await Context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GetImage(int id)
        {
            var category = await Context.ProductCategory
                .Include(c => c.Image)
                .SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.ProductCategoryError,
                        message = $"Не найдена категория с id = '{id}'"
                    }
                );
            }

            return File(category.Image.bytes, "image/png");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateImage(int id, [FromForm] ImageRequest imageRequest)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == id);
            if (category == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.ProductCategoryError,
                        message = $"Не найдена категория с id = '{id}'"
                    }
                );
            }

            if (imageRequest.image != null)
            {
                if (category.Image != null)
                {
                    Context.Files.Remove(category.Image);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    imageRequest.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    category.Image = file;
                    await Context.SaveChangesAsync();
                }
            }

            return Ok();
        }

    }
}

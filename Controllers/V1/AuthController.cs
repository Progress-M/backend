using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Main.Models;
using Microsoft.AspNetCore.Cors;
using System;
using Microsoft.Extensions.Configuration;

using Main.Function;
using Microsoft.AspNetCore.Authorization;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<User> _logger;

        public AuthController(KindContext KindContext, ILogger<User> logger, IConfiguration configuration)
        {
            Context = KindContext;
            _logger = logger;
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        [HttpPost("company")]
        public async Task<IActionResult> AccessToken(AuthRequest auth)
        {
            var item = await Context.Company
                .Include(c => c.ProductСategory)
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    company =>
                        company.Email == auth.username &&
                        company.Password == auth.password
            );

            if (item == null)
            {
                return Unauthorized(
                    new
                    {
                        status = AuthStatus.Fail,
                        message = "Authentication failed"
                    }
                );
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    company = item,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                });
        }

        [HttpGet("refresh-token")]
        [Authorize(Policy = "ValidAccessToken")]
        public IActionResult RefreshToken()
        {
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                });
        }

        [HttpPost("token/user/{playerId}")]
        public async Task<IActionResult> AccessTokenUser(string playerId)
        {
            var item = await Context.User
                .Include(u => u.Favorites)
                    .ThenInclude(c => c.ProductСategory)
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.PlayerId == playerId);

            if (item == null)
            {
                return Unauthorized(
                    new
                    {
                        status = AuthStatus.Fail,
                        message = "Authentication failed"
                    }
                );
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    user = item,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                });
        }
    }
}

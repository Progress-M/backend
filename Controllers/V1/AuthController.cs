using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Main.Models;
using Microsoft.AspNetCore.Cors;
using System;

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

        public AuthController(KindContext KindContext, ILogger<User> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpPost("company")]
        [Produces("application/json")]
        public async Task<ActionResult> SignInCompany(AuthRequest auth)
        {
            try
            {
                var item = await Context.Company
                    .AsNoTracking()
                    .SingleOrDefaultAsync(company => company.Email == auth.username);

                if (item == null)
                {
                    return Ok(
                        new AuthResponse
                        {
                            status = AuthStatus.Fail,
                            message = "Authentication failed"
                        }
                    );
                }

                return Ok(
                    new AuthResponse
                    {
                        status = AuthStatus.Success,
                        message = ""
                    }
                );
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError($"SignInUser: {e}");
                return Ok(
                    new AuthResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Authentication failed: 'InvalidOperationException'"
                    }
                );
            }
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<AuthResponse>> SignInUser(AuthRequest auth)
        {
            try
            {
                var item = await Context.User
                    .AsNoTracking()
                    .SingleOrDefaultAsync(user => user.Email == auth.username && user.Password == auth.password);

                if (item == null)
                {
                    return Ok(
                        new AuthResponse
                        {
                            status = AuthStatus.Fail,
                            message = "Authentication failed"
                        }
                    );
                }

                return Ok(
                    new AuthResponse
                    {
                        status = AuthStatus.Success,
                        message = ""
                    }
                );
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError($"Error: {e}");
                return Ok(
                    new AuthResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Authentication failed: 'InvalidOperationException'"
                    }
                );
            }
        }

    }
}

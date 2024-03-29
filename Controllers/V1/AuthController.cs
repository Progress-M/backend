using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Main.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;

using Main.Function;
using Microsoft.AspNetCore.Authorization;
using MimeKit;
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
        public IConfiguration _configuration { get; }

        public AuthController(KindContext KindContext, ILogger<User> logger, IConfiguration Configuration)
        {
            Context = KindContext;
            _logger = logger;
            _configuration = Configuration;
        }

        [HttpPost("company")]
        public async Task<IActionResult> AccessToken(AuthRequest auth)
        {
            var item = await Context.Company
                .Include(c => c.ProductCategory)
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    company =>
                        company.Email == auth.username &&
                        company.Password == auth.password
            );

            if (item == null || !item.EmailConfirmed)
            {
                return Unauthorized(
                    new BdobrResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Такой компании не существует или аккаунт не подтверждён."
                    }
                );
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    company = item,
                    access_token = Auth.generateToken(_configuration),
                    token_type = "bearer"
                });
        }

        [HttpPost("company/lost-password")]
        public async Task<IActionResult> LostPassword(EmailRequest request)
        {
            var item = await Context.Company.SingleOrDefaultAsync(company => company.Email == request.email);

            if (item == null)
            {
                return Unauthorized(
                    new BdobrResponse
                    {
                        status = ResponseStatus.SignInError,
                        message = $"Компания с email = '{request.email}' не найдена."
                    }
                );
            }
            item.EmailConfirmed = false;
            var code = Utils.RandomCode();

            Context.EmailCode.Add(new EmailCode
            {
                code = code,
                email = request.email
            });
            await Context.SaveChangesAsync();

            MimeMessage message = Utils.BuildMessageСonfirmation(request.email, code);
            Utils.SendEmail(message);

            return Ok();
        }

        [HttpPost("company/restore-password")]
        public async Task<IActionResult> RestorePassword(PasswordRestoreRequest request)
        {
            var ue = await Context.EmailCode
                  .SingleOrDefaultAsync(ue => ue.email == request.email);

            if (ue == null || ue.code != request.code)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.SignInError,
                        message = $"Не найден email = {request.email} или некорректный код."
                    }
                );
            }

            double durationSeconds = DateTime.UtcNow.Subtract(ue.createdDateTime).TotalSeconds;
            TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
            var EmailCodeTimeLife = Int32.Parse(_configuration["EmailCodeTimeLife"]);
            if (seconds.TotalMinutes > EmailCodeTimeLife)
            {
                return BadRequest(
                    new BdobrResponse
                    {
                        status = ResponseStatus.SignInError,
                        message = $"Время жизни кода истекло."
                    }
                );
            }

            var company = await Context.Company.SingleOrDefaultAsync(c => c.Email == request.email);

            if (company == null)
            {
                return BadRequest(new BdobrResponse
                {
                    status = ResponseStatus.SignUpError,
                    message = $"Компания с email = '{request.email}' не найдена."
                });
            }

            company.EmailConfirmed = true;
            company.Password = request.newPassword;
            Context.EmailCode.Remove(ue);
            await Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("refresh-token")]
        [Authorize(Policy = "ValidAccessToken")]
        public IActionResult RefreshToken()
        {
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    access_token = Auth.generateToken(_configuration),
                    token_type = "bearer"
                });
        }

        [HttpPost("token/company/{playerId}")]
        public async Task<IActionResult> AccessTokenCompany(string playerId)
        {
            var item = await Context.Company
               .Include(c => c.ProductCategory)
               .AsNoTracking()
               .SingleOrDefaultAsync(
                   company =>
                       company.PlayerId == playerId
           );

            if (item == null || !item.EmailConfirmed)
            {
                return Unauthorized(
                    new BdobrResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Компания с playerId = '{playerId}' не существует, либо у неё не подтвежден email."
                    }
                );
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    company = item,
                    access_token = Auth.generateToken(_configuration),
                    token_type = "bearer"
                });
        }

        [HttpPost("pincode/company")]
        public async Task<IActionResult> AuthCompanyByPinCode(AuthPinCodeRequest request)
        {
            var item = await Context.Company
               .Include(c => c.ProductCategory)
               .AsNoTracking()
               .SingleOrDefaultAsync(company => company.PlayerId == request.playerId && company.PinCode == request.pincode);

            if (item == null || !item.EmailConfirmed)
            {
                return Unauthorized(
                    new BdobrResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Компания с playerId = '{request.playerId}' не существует, либо у неё не подтвежден email."
                    }
                );
            }

            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    company = item,
                    access_token = Auth.generateToken(_configuration),
                    token_type = "bearer"
                });
        }

        [HttpPost("token/user/{playerId}")]
        public async Task<IActionResult> AccessTokenUser(string playerId)
        {
            var item = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.PlayerId == playerId);

            if (item == null)
            {
                return Unauthorized(
                    new BdobrResponse
                    {
                        status = AuthStatus.Fail,
                        message = $"Пользователя с playerId = '{playerId}' не существует."
                    }
                );
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    user = item,
                    access_token = Auth.generateToken(_configuration),
                    token_type = "bearer"
                });
        }
    }
}

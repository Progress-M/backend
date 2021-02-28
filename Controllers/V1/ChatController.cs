using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Main.Function;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    // [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ChatController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<AuthController> _logger;

        public ChatController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetChats(int userId, int lastId = -1)
        {
            var user = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return NotFound($"Not found user with id = {userId}");
            }

            var messages = await Context.Message
                    .AsNoTracking()
                    .Include(m => m.company)
                    .Where(message => message.Id > lastId && message.user.Id == userId)
                    .ToListAsync();

            var comparer = new CompanyComparer();

            var groups = messages
                    .GroupBy(message => message.company, comparer)
                    .Select(messages =>
                    {
                        var last = messages.ToList().Last();
                        return new
                        {
                            Company = messages.Key,
                            LastMessage = last.text,
                            dateTime = last.sendingTime,
                        };
                    })
                    .OrderByDescending(messages => messages.dateTime);

            return Ok(new
            {
                chats = groups,
                lastId = messages?.Last()?.Id
            });
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetCompanyChats(int companyId, int lastId = -1)
        {
            var company = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound($"Not found user with id = {companyId}");
            }

            var messages = await Context.Message
                    .AsNoTracking()
                    .Include(m => m.user)
                    .Where(message => message.Id > lastId && message.company.Id == companyId)
                    .ToListAsync();

            var comparer = new UserComparer();

            var groups = messages
                    .GroupBy(message => message.user, comparer)
                    .Select(messages =>
                    {
                        var last = messages.ToList().Last();
                        return new
                        {
                            User = messages.Key,
                            LastMessage = last.text,
                            dateTime = last.sendingTime,
                        };
                    })
                    .OrderByDescending(messages => messages.dateTime);

            return Ok(new
            {
                chats = groups,
                lastId = messages?.Last()?.Id
            });
        }

        [HttpGet("message")]
        public async Task<IActionResult> GetMessages(int userId, int companyId, int lastMessageId = -1)
        {
            var user = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return NotFound($"Not found user with id = {userId}");
            }

            return Ok(
                await Context.Message
                    .AsNoTracking()
                    .Include(m => m.company)
                    .Include(m => m.user)
                    .ToListAsync()
            );

            var messages = (await Context.Message
                    .AsNoTracking()
                    .Include(m => m.company)
                    .Include(m => m.user)
                    .Where(message => message.user.Id == userId && message.Id > lastMessageId && message.company.Id == companyId)
                    .ToListAsync())
                    .OrderBy(message => message.sendingTime)
                    .GroupBy(message => new { message.sendingTime.Year, message.sendingTime.Month, message.sendingTime.Day })
                    .Select(messages =>
                    {
                        Console.WriteLine(messages);
                        var last = messages?.ToList()?.Last();
                        return new
                        {
                            Messages = messages,
                            Day = last?.sendingTime,
                        };
                    });

            return Ok(messages);
        }

        [HttpPost("message")]
        public async Task<IActionResult> AddMessage(ChatMessage messageRequest)
        {
            var company = await Context.Company
               .SingleOrDefaultAsync(company => company.Id == messageRequest.companyId);

            if (company == null)
            {
                return NotFound($"Not found comapny with id = {messageRequest.companyId}");
            }

            var user = await Context.User
               .SingleOrDefaultAsync(user => user.Id == messageRequest.userId);

            if (user == null)
            {
                return NotFound($"Not found user with id = {messageRequest.userId}");
            }

            var message = new Message(user, company, messageRequest.isUserMessage, messageRequest.text);
            await Context.Message.AddAsync(message);
            await Context.SaveChangesAsync();

            return Ok(message);
        }


    }
}

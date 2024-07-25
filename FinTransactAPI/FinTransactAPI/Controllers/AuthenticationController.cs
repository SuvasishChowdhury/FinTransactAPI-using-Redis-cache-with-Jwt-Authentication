using FinTransactAPI.Cache;
using FinTransactAPI.Data;
using FinTransactAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinTransactAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DbContextClass _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        public AuthenticationController(DbContextClass context, ICacheService cacheService, ILogger<ProductController> logger) 
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] Login user)
        {
            var loginCache = new List<Login>();

            if (user is null)
            {
                return BadRequest("Invalid user request!!!");
            }
            loginCache = _cacheService.GetData<List<Login>>("Login");
            if (loginCache == null)
            {
                var login = _context.Logins.ToList();
                if(login != null)
                {
                    loginCache = login;
                    var expiryTime = DateTimeOffset.Now.AddMinutes(10);

                    _cacheService.SetData("Login", loginCache, expiryTime);
                }
            }
            bool hasUser = loginCache.Where(s=>s.UserName == user.UserName && s.Password == user.Password).Any();
            if (hasUser)
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddMinutes(6),
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new JWTTokenResponse { Token = tokenString });
            }
            return Unauthorized();
        }

        // for testing purpose to create a user...
        //[HttpGet]
        //[Route("UserList")]
        //public async Task<ActionResult<IEnumerable<Login>>> Get()
        //{
        //    var userCache = new List<Login>();

        //    _logger.LogInformation("Someone try to fetch all Users!");

        //    userCache = _cacheService.GetData<List<Login>>("Login");
        //    if (userCache == null)
        //    {
        //        var users = await _context.Logins.ToListAsync();
        //        if (users.Count > 0)
        //        {
        //            userCache = users;
        //            var expirationTime = DateTimeOffset.Now.AddMinutes(3.0);
        //            _cacheService.SetData("Login", userCache, expirationTime);
        //        }
        //    }
        //    return userCache;
        //}

        //[HttpPost]
        //[Route("CreateUser")]
        //public async Task<ActionResult<Login>> POST(Login login)
        //{
        //    var userCache = new List<Login>();
        //    userCache = _cacheService.GetData<List<Login>>("Login");
        //    bool hasUser = userCache.Where(s => s.UserName == login.UserName).Any();
        //    if (!hasUser) 
        //    {
        //        return BadRequest();
        //    }
        //    else
        //    {
        //        _context.Logins.Add(login);
        //    }
        //    if (await _context.SaveChangesAsync() > 0)
        //    {
        //        _cacheService.RemoveData("Login");

        //        return CreatedAtAction(nameof(Get), new { id = login.ID }, login);
        //    }
        //    else 
        //    {
        //        return BadRequest();
        //    };
        //}
    }
}

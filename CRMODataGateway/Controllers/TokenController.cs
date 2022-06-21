using CRMODataGateway.Interfaces;
using CRMODataGateway.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ODataGateway.Shared.Dtos;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRMODataGateway.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public TokenController(IConfiguration config, ITokenService tokenService)
        {
            _configuration = config;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(UserInfo _user)
        {   
            User user = await _tokenService.GetUserInfo(_user);
            user.AccessCode = _tokenService.EncryptString(_user.Password);

            if (await _tokenService.ValidateUser(user)) 
            {
                var claims =await _tokenService.CreateTokenClaims(user); 
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                var tokenDescriptor = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Issuer"],
                    claims,
                    expires: DateTime.Now.AddDays(2),
                    signingCredentials: credentials
                    );
                return Ok(new JwtSecurityTokenHandler().WriteToken(tokenDescriptor));
            }
            return BadRequest();

        }

    }
}

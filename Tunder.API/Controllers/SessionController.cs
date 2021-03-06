﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CommonCode.Code.Builders;
using CommonCode.Helpers;
using CommonCode.Messages.ViewModels;
using Data.BusinessObject.Requests;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data.Model.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tunder.API.Services;

namespace Tunder.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configs;
        private readonly INotificationService _notificationService;


        public SessionController(IAuthService authService, IUserRepository userRepository, IConfiguration configs,
            INotificationService notificationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configs = configs ?? throw new ArgumentNullException(nameof(configs));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        [HttpPost("register")]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            userDto.Email = userDto.Email.ToLower();

            if (await _userRepository.UserEmailExistsAsync(userDto.Email))
            {
                return BadRequest();
            }

            User user = await _authService.RegisterAsync(userDto);

            await _notificationService.SendWelcomeMessageAsync(user);
            
            return Created("user/me", user);
        }

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            User user = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            Claim[] claims = {
                new Claim(ClaimTypes.NameIdentifier, user.Guid.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = CryptoHelpers.GetSymmetricSecurityKey(_configs);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }

        /*
        [HttpDelete("logout")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Logout()
        {
            //HttpContext.Request.Headers.TryGetValue();

            return NoContent();
        }
        */
    }
}
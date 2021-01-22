using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authenticate.Data;
using AuthenticationApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Controller
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUser _userService;
        private readonly IOptions<appSetting> _appSettings;
        private readonly IConfiguration _config;

        public UserController(IUser user, IOptions<appSetting> appSettings, IConfiguration config)
        {
            _userService = user;
            _appSettings = appSettings;
            _config = config;

        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
            var key = Encoding.UTF8.GetBytes(_config["AppSettings:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.FirstName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.LastName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
               
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            
            var tokenString = tokenHandler.WriteToken(token);
            var expiresWhen = tokenDescriptor.Expires;

            //to validate token 
            //var validate = tokenHandler.ValidateToken(tokenString);

            // return basic user info and authentication token
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString,
                expires = expiresWhen

            });
        }

        // post: User/Register
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            var NewUser = new User
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.UserName
            };

            try
            {
                // create user
                _userService.Create(NewUser, model.Password);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        //[AllowAnonymous]
        // Get: User/GetAll
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
           // var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(users);
        }

       
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _userService.GetById(id);
                // var model = _mapper.Map<UserModel>(user);
                return Ok(user);
            }

            catch (ArgumentException ex)
            {
                return NotFound(new { messg = ex.Message });
            }
           
        }

       
        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody]UpdateModel model)
        {
            //// map model to entity and set id
            //var user = _mapper.Map<User>(model);
            //user.Id = id;
          
            var NewUser = new User
            {
                Id = id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username
            };

            try
            {
                // update user 
                _userService.Update(NewUser, model.Password);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _userService.Delete(id);
                return Ok();
            }

            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
           
        }
    }
}

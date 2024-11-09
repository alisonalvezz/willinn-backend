using Core.Models;
using Core.Resources;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await userService.Login(request.Email, request.Password);
            if (token == null) throw new UnauthorizedAccessException("Invalid credentials");
            return Ok(token);
        }


        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            var newUser = await userService.CreateUser(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetUserById(id);
            return Ok(user);
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
        {
            await userService.UpdateUser(id, userDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await userService.DeleteUser(id);
            return NoContent();
        }

        [HttpPut("{id}/activate")]
        [Authorize]
        public async Task<IActionResult> ActivateUser(int id)
        {
            await userService.ActivateUser(id);
            return NoContent();
        }
    }
}

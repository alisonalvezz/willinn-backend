using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Models;
using Core.Resources;
using Core.Services;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Services;

public class UserService(ApplicationDbContext context, IConfiguration configuration) : IUserService
{
    private readonly string _jwtSecretKey = configuration["Jwt:Key"];

    public async Task<dynamic> Login(string email, string password)
    {
        try
        {
            var user = await context.User.SingleOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

            if (key.Length < 32)
            {
                throw new Exception("JWT Secret Key must be at least 32 characters long.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var name = user.Name;
            var useremail = user.Email;
            return new { token = tokenHandler.WriteToken(token), name = name, email = useremail };
                
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while logging in.", ex);
        }
    }

    public async Task<User> CreateUser(UserDto userDto)
    {
        try
        {
            var existingUser = await context.User.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            context.User.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating the user.", ex);
        }
    }


    public async Task<IEnumerable<User>> GetAllUsers()
    {
        try
        {
            return await context.User.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving users.", ex);
        }
    }

    public async Task<User> GetUserById(int id)
    {
        try
        {
            var user = await context.User.SingleOrDefaultAsync(u => u.Id == id && u.IsActive);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            return user;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving the user.", ex);
        }
    }


    public async Task<bool> UpdateUser(int id, UpdateUserDto userDto)
    {
        try
        {
            var existingUser = await context.User.FindAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
                
            if (!string.IsNullOrEmpty(userDto.Name))
            {
                existingUser.Name = userDto.Name;
            }

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                existingUser.Email = userDto.Email;
            }

            if (!string.IsNullOrEmpty(userDto.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            context.User.Update(existingUser);
            await context.SaveChangesAsync();
            return true;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the user.", ex);
        }
    }

    public async Task<bool> DeleteUser(int id)
    {
        try
        {
            var user = await context.User.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.IsActive = false;
            context.User.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> ActivateUser(int id)
    {
        try
        {
            var user = await context.User.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.IsActive = true;
            context.User.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
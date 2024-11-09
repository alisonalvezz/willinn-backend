using Core.Models;
using Core.Resources;

namespace Core.Services;

public interface IUserService
{
    Task<dynamic> Login(string email, string password);
    Task<User> CreateUser(UserDto userDto);
    Task<IEnumerable<User>> GetAllUsers();
    Task<User> GetUserById(int id);
    Task<bool> UpdateUser(int id, UpdateUserDto userDto);
    Task<bool> DeleteUser(int id);
    Task<bool> ActivateUser(int id);
}
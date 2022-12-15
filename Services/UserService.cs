using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ReservationSystem.Models;
using ReservationSystem.Repositories;
using System.Security.Cryptography;

namespace ReservationSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }
        public async Task<UserDTO> CreateUserAsync(User user)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256/8));

            User newUser = new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Salt = salt,
                Password = hashedPassword,
                JoinDate = DateTime.Now
            };

            newUser = await _repository.AddUserAsync(newUser);
            if(newUser == null)
            {
                return null;
            }
            return UserToDTO(newUser);
        }

        private UserDTO UserToDTO(User user)
        {
            UserDTO dto = new UserDTO();
            dto.UserName = user.UserName;
            dto.FirstName = user.FirstName;
            dto.LastName = user.LastName;
            dto.JoinDate = user.JoinDate;
            dto.LoginDate = user.LoginDate;
            return dto;
        }
    }
}

using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    public interface IUserService
    {
        public Task<UserDTO> CreateUserAsync(User user);

    }
}

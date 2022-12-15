using ReservationSystem.Models;

namespace ReservationSystem.Repositories
{
    public interface IUserRepository
    {
        public Task<User> GetUserAsync(String userName);
        public Task<User> AddUserAsync(User user);

    }
}

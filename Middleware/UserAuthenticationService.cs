using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using ReservationSystem.Models;

namespace ReservationSystem.Middleware
{
    public interface IUserAuthenticationService
    {
        Task<User> Authenticate(string username, string password);
        Task<bool> IsAllowed(String username, ItemDTO item);
        Task<bool> IsAllowed(String username, User user);
        Task<bool> IsAllowed(String username, ReservationDTO reservation);
    }
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly ReservationContext _context;

        public UserAuthenticationService(ReservationContext context)
        {
            _context = context;
        }
        public async Task<User> Authenticate(string username, string password)
        {
            User? user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync();

            if(user == null)
            {
                return null;
            }
            byte[] salt = user.Salt;

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if(hashedPassword != user.Password)
            {
                return null;
            }

            return user;
        }

        public async Task<bool> IsAllowed(string username, ItemDTO item)
        {
            User? user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync();
            Item? dbItem = await _context.Items.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == item.Id);

            if(user == null)
            {
                return false;
            }
            if(dbItem == null && item.Owner == user.UserName) //Mikäli lisätään uutta itemiä
            {
                return true;
            }
            if(dbItem == null)
            {
                return false;
            }
            if(user.Id == dbItem.Owner.Id)
            {
                return true;
            }
            return false;
          
        }

        public async Task<bool> IsAllowed(string username, User user)
        {
            User? dbUser = await _context.Users.Where(x => x.UserName == user.UserName).FirstOrDefaultAsync();

            if(dbUser != null && dbUser.UserName == username)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsAllowed(string username, ReservationDTO reservation)
        {
            User user = await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync();
            Reservation dbReservation = await _context.Reservations.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == reservation.Id);

            if (user == null)
            {
                return false;
            }
            if (dbReservation == null && reservation.Owner == user.UserName)
            {
                return true;
            }
            if (dbReservation == null)
            {
                return false;
            }

        
            if(user.Id == dbReservation.Owner.Id)
            {
                return true;
            }
            return false;
        }
    }
}

using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    public interface IReservationService
    {
        public Task<ReservationDTO> CreateReservationAsync(ReservationDTO dto);
        public Task<ReservationDTO> GetReservationAsync(long id);
        public Task<IEnumerable<ReservationDTO>> GetReservationsAsync();
        public Task<ReservationDTO> UpdateReservationAsync(ReservationDTO reservation);
        public Task<Boolean> DeleteReservationAsync(long id);
    }
}

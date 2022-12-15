namespace ReservationSystem.Models
{
    public class User
    {
        public long Id { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public byte[]? Salt { get; set; }
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LoginDate { get; set; }
    }

    public class UserDTO
    {
        public String UserName { get; set; }
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LoginDate { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UhlnocsServer.Calculations;

namespace UhlnocsServer.Users
{
    // this class describes users table in database
    [Table("users")]
    public class User
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("surname")]
        public string Surname { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public List<Launch> Launches { get; } = new List<Launch>();

        public User(string id, string email, string password, string name, string surname, string description)
        {
            Id = id;
            Email = email;
            Password = password;
            Name = name;
            Surname = surname;
            Description = description;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace FinTransactAPI.Model
{
    public class Login
    {
        [Key]
        public int ID { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}

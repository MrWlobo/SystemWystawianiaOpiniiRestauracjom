using System.ComponentModel.DataAnnotations;
namespace LoginR
{
    public class LoginRequest
{
    [Required]
    public string Login1 { get; set; }

    [Required]
    public string Password { get; set; }
}
}

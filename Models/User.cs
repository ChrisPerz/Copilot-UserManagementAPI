namespace Users.Models;
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string JobTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Additional properties can be added as needed
}
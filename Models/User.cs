namespace Users.Models;
public class User
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string JobTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Additional properties can be added as needed
}
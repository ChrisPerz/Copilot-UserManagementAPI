using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Users.Models;
using Microsoft.Extensions.Logging;
using UserManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;

[Route("api/Users")]
[ApiController]
public class UserController : ControllerBase 
{
    // I asked for a logger to log errors and other information, so I can track issues in production
    private readonly ILogger<UserController> _logger;
    private readonly IRouteCounterService _routeCounter;

    public UserController(ILogger<UserController> logger, IRouteCounterService routeCounter)
    {
        _logger = logger;
        _routeCounter = routeCounter;
    }


    //These methods should be in a service layer, but for simplicity I will leave them here

    // COPILOT suggested me to use a ConcurrentDictionary instead of the normal one, for thread safety
    private static ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>(
        new Dictionary<int, User>
        {
            { 1, new User { Name = "Alice", Email = "alice@example.com", JobTitle = ".Net Developer" } },
            { 2, new User { Name = "Bob", Email = "bob@example.com", JobTitle = "Cybersecurity" } },
            { 3, new User { Name = "Charlie", Email = "charlie@example.com", JobTitle = "HR" } }
        }
    );

    // I asked for possible bugs and improvements and COPILOT suggested me to use a method to validate the email format - 
    //I think is better to not add it here, since its out of controller responsabilities, but I will leave it here to easier understanding in review.
    public bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // COPILOT suggested me to use a method to check for duplicate emails, I think is a good idea to avoid duplicates in the user list, since 
    // the email is like a unique identifier for the user in this case that I use only name and email - a personal ID could be better in a real application
    private bool IsEmailDuplicate(string email)
    {
        return users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] User loginUser)
    {
        if (loginUser == null || string.IsNullOrWhiteSpace(loginUser.Email))
        {
            return BadRequest("Invalid login data.");
        }

        var user = users.Values.FirstOrDefault(u => u.Email.Equals(loginUser.Email, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            return Ok("Login successful.");
        }

        return Unauthorized("Invalid email or user does not exist.");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAllUsers()
    {
        return Ok(users.Values);
    }

    [HttpGet("{id:int:min(1)}")]
    public IActionResult GetUser(int id)
    {
        if (users.TryGetValue(id, out User user))
            return Ok(user);

        return NotFound($"User with ID {id} not found.");
    }

    // COPILOT suggested me to use FromBody attribute here AND the use of IsNullOrWhiteSpace method to validate the user data
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        try
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email) || 
                string.IsNullOrWhiteSpace(user.JobTitle) || !IsValidEmail(user.Email) || IsEmailDuplicate(user.Email))
            {
                return BadRequest("Invalid user data or email already exist.");
            }

             // int newId = users.Keys.Max() + 1; 
            // COPILOT suggested me to use a safer way to generate the new ID, since the Max() method could throw an exception if the dictionary is empty
            int newId = users.Any() ? users.Keys.Max() + 1 : 1;
            users[newId] = user;

            return CreatedAtAction(nameof(GetUser), new { id = newId }, user);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "Error occurred while creating a user.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("{id:int:min(1)}")]
    public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
    {
        // COPILOT suggested me this, but I used the min(1) attribute in the route instead

        // if (id <= 0)
        // {
        //     return BadRequest("ID must be a positive integer.");
        // }

        if (updatedUser == null || string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Email))
        {
            return BadRequest("Invalid user data.");
        }

        if (users.TryGetValue(id, out User existingUser))
        {
            // Update the user details
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.JobTitle = updatedUser.JobTitle;
            existingUser.UpdatedAt = DateTime.UtcNow;

            return Ok(existingUser);
        }

        return NotFound($"User with ID {id} not found.");
    }

    // COPILOT helped me here with the NoContent response that I did not know about
    [HttpDelete("{id:int:min(1)}")]
    [Authorize(Policy = "CanAccessAdminPanel")]
    public IActionResult DeleteUser(int id)
    {
        if (users.TryRemove(id, out _))
        {
            return NoContent(); // Successfully deleted - return 204 code
        }

        return NotFound($"User with ID {id} not found.");
    }


    [HttpGet]
    [Route("routeCounter")]
    public IActionResult GetAllRoutesCount()
    {
        return Ok(_routeCounter.GetCounts());
    }
}
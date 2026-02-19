using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using CredibilityIndex.Infrastructure.Auth;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
     
        [HttpGet("stats")]
        public IActionResult GetSystemStats()
        {
            return Ok(new
            {
                TotalUsers = _userManager.Users.Count(),
                ServerStatus = "Healthy",
                LastBackup = DateTime.UtcNow.AddDays(-1)
            });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.DisplayName,
                    u.EmailConfirmed
                })
                .ToList();

            return Ok(new
            {
                TotalCount = users.Count,
                Users = users
            });
        }
    }
}

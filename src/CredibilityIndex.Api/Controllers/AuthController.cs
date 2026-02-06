// using Microsoft.AspNetCore.Mvc;
// using CredibilityIndex.Api.Contracts;

// namespace
// CredibilityIndex.Api.Contracts;

// [ApiController]
// [Route("api/[controller]")]

// public class AuthController :
// ControllerBase
// {
//     [httpPost("register")]
//     public IActionResult
// Register([FromBody] RegisterRequest request)
//   {
//     if(string.IsNullorEmpty(request.Email)){
//         return BadRequest("Email is required. ");
//     }
//     var result = $"User {request.Username} received successfully ";
//     return oK(new { Message = result});
//   }
// }
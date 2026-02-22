using System.ComponentModel.DataAnnotations;
namespace CredibilityIndex.Api.Contracts.Auth;

public record RegisterRequest(

    [Required]
    [EmailAddress]
    [StringLength(100, MinimumLength = 3)]
    string Email,

    [Required]
    [StringLength(16, MinimumLength = 8)]
    string Password,

    [Required]
    [StringLength(20, MinimumLength = 2)]
    string DisplayName

    // conflict A Parameter (used to create the object).
    //A Property (used to store the value).
    // [property: System.ComponentModel.DataAnnotations.Required]
    // [property: System.ComponentModel.DataAnnotations.EmailAddress]
    // [property: System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 3)]
    // string Email,

    // [property: System.ComponentModel.DataAnnotations.Required]
    // [property: System.ComponentModel.DataAnnotations.StringLength(16, MinimumLength = 8)]
    // string Password,

    // [property: System.ComponentModel.DataAnnotations.Required]
    // [property: System.ComponentModel.DataAnnotations.StringLength(20, MinimumLength = 2)]
    // string DisplayName 
);
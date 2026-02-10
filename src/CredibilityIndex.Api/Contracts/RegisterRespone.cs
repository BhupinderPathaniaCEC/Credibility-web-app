namespace CredibilityIndex.Api.Contracts;

public record RegisterResponse(
    Guid Id,
    string Email,
    string DisplayName
);
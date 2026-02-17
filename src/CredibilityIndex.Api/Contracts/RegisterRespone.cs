using System;
namespace CredibilityIndex.Api.Contracts;

public record RegisterResponse(
    string Id,
    string Email,
    string DisplayName
);
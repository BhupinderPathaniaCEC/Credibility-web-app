using System;

namespace CredibilityIndex.Api.Contracts.Auth;

public record RegisterResponse(
    string Id,
    string Email,
    string DisplayName
);
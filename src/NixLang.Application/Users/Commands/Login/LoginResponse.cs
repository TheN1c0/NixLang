namespace NixLang.Application.Users.Commands.Login;

public record LoginResponse(string AccessToken, DateTime ExpiresAt);

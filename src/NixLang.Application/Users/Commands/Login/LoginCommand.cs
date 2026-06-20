using MediatR;

namespace NixLang.Application.Users.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

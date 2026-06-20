using MediatR;

namespace NixLang.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string FullName, string Email, string Password) : IRequest<Guid>;

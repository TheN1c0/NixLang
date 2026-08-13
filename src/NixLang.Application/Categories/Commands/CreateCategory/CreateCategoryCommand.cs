using MediatR;

namespace NixLang.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : IRequest<Guid>;

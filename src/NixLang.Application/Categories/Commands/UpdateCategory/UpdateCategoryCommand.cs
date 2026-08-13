using MediatR;

namespace NixLang.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name, string Description) : IRequest<bool>;

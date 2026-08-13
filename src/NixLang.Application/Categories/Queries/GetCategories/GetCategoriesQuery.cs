using MediatR;
using NixLang.Domain.Entities;

namespace NixLang.Application.Categories.Queries.GetCategories;

public record CategoryDto(Guid Id, string Name, string Description);

public record GetCategoriesQuery() : IRequest<List<CategoryDto>>;

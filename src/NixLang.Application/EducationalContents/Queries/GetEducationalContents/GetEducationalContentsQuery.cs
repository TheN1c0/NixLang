using System;
using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.EducationalContents.Queries.GetEducationalContents;

public record EducationalContentItemDto(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel,
    DateTime CreatedAt);

public record GetEducationalContentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Type = null,
    string? Level = null) : IRequest<PagedResult<EducationalContentItemDto>>;

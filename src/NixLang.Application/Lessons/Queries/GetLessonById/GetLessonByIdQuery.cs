using System;
using MediatR;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

/// <summary>
/// Query to retrieve details of a single published lesson by its ID.
/// </summary>
public record GetLessonByIdQuery(Guid Id) : IRequest<LessonDetailDto>;

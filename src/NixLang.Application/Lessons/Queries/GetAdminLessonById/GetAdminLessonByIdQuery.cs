using MediatR;
using NixLang.Application.Lessons.Queries.GetLessonById;

namespace NixLang.Application.Lessons.Queries.GetAdminLessonById;

public record AdminLessonDetailDto(
    Guid Id,
    string Title,
    string Description,
    string ReferenceLevel,
    string Status,
    int ExerciseCount,
    IReadOnlyCollection<LessonBlockDto> LessonBlocks);

public record GetAdminLessonByIdQuery(Guid Id) : IRequest<AdminLessonDetailDto>;

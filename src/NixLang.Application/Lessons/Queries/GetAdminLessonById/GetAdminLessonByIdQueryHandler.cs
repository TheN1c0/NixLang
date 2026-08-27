using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Lessons.Queries.GetLessonById;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Lessons.Queries.GetAdminLessonById;

public class GetAdminLessonByIdQueryHandler : IRequestHandler<GetAdminLessonByIdQuery, AdminLessonDetailDto>
{
    private readonly ILessonRepository _lessonRepository;

    public GetAdminLessonByIdQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<AdminLessonDetailDto> Handle(GetAdminLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lesson == null)
        {
            throw new LessonNotFoundException(request.Id);
        }

        var exerciseCount = lesson.LessonBlocks.Count(b => b.Type == LessonBlockType.Exercise);

        var blockDtos = lesson.LessonBlocks
            .Select(b => new LessonBlockDto(
                b.Id,
                b.Type.ToString(),
                b.Sequence,
                b.Configuration.Value,
                b.ReferencedExerciseId,
                b.Exercise != null ? new ExerciseDto(
                    b.Exercise.Id,
                    b.Exercise.Type.ToString(),
                    b.Exercise.Statement,
                    b.Exercise.CorrectAnswer,
                    b.Exercise.AudioResourceUrl,
                    b.Exercise.Options
                        .Select(o => new ExerciseOptionDto(o.Id, o.Text, o.DisplayOrder))
                        .OrderBy(o => o.DisplayOrder)
                        .ToList()
                        .AsReadOnly()) : null,
                b.ReferencedEducationalContentId,
                b.EducationalContent != null ? new EducationalContentSummaryDto(
                    b.EducationalContent.Id,
                    b.EducationalContent.Title,
                    b.EducationalContent.Summary,
                    b.EducationalContent.Body,
                    b.EducationalContent.Type.ToString(),
                    b.EducationalContent.ReferenceLevel?.ToString()) : null))
            .ToList()
            .AsReadOnly();

        return new AdminLessonDetailDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.ReferenceLevel.ToString(),
            lesson.Status.ToString(),
            exerciseCount,
            blockDtos);
    }
}

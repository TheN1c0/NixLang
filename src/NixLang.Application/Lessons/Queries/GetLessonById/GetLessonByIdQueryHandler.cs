using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Domain.Repositories;
using NixLang.Domain.Enums;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDetailDto>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonByIdQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<LessonDetailDto> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetPublishedByIdAsync(request.Id, cancellationToken);

        if (lesson is null)
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
                    b.Exercise.AudioResourceUrl) : null))
            .ToList()
            .AsReadOnly();

        return new LessonDetailDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.ReferenceLevel.ToString(),
            exerciseCount,
            blockDtos);
    }
}

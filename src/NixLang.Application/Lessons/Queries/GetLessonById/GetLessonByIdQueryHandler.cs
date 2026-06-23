using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Domain.Repositories;

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

        return new LessonDetailDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.ReferenceLevel.ToString(),
            lesson.Exercises.Count);
    }
}

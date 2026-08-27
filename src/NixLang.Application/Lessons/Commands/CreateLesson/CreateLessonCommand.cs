using MediatR;

namespace NixLang.Application.Lessons.Commands.CreateLesson;

public record CreateLessonBlockDto(
    string Type, 
    string ConfigurationValue, 
    Guid? ReferencedExerciseId, 
    Guid? ReferencedEducationalContentId = null);

public record CreateLessonCommand(
    string Title,
    string Description,
    string ReferenceLevel,
    List<Guid>? CategoryIds,
    List<CreateLessonBlockDto>? LessonBlocks) : IRequest<Guid>;

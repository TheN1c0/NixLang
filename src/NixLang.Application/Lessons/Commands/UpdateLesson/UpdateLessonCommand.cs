using MediatR;
using NixLang.Application.Lessons.Commands.CreateLesson;

namespace NixLang.Application.Lessons.Commands.UpdateLesson;

public record UpdateLessonCommand(
    Guid Id,
    string Title,
    string Description,
    string ReferenceLevel,
    string Status, // PublicationStatus (Draft, Published, Disabled)
    List<Guid>? CategoryIds,
    List<CreateLessonBlockDto>? LessonBlocks) : IRequest<bool>;

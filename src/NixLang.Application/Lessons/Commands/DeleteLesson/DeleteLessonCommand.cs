using MediatR;

namespace NixLang.Application.Lessons.Commands.DeleteLesson;

public record DeleteLessonCommand(Guid Id) : IRequest<bool>;

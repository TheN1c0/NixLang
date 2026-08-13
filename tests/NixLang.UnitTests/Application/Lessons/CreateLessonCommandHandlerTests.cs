using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Lessons.Commands.CreateLesson;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Lessons;

public class CreateLessonCommandHandlerTests
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateLessonCommandHandler _handler;

    public CreateLessonCommandHandlerTests()
    {
        _lessonRepository = Substitute.For<ILessonRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateLessonCommandHandler(_lessonRepository, _categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateLessonAndSave()
    {
        // Arrange
        var command = new CreateLessonCommand(
            "Verb To Be",
            "Learn verb to be grammar rules",
            "A1",
            null,
            new List<CreateLessonBlockDto>
            {
                new CreateLessonBlockDto("Heading", "Introduction", null),
                new CreateLessonBlockDto("Paragraph", "Explanation text", null)
            });

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        await _lessonRepository.Received(1).AddAsync(Arg.Is<Lesson>(l => l.Title == "Verb To Be" && l.Description == "Learn verb to be grammar rules" && l.ReferenceLevel == ReferenceLevel.A1), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NixLang.Application.Categories.Commands.CreateCategory;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateCategoryCommandHandler(_categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithUniqueName_ShouldCreateCategoryAndSave()
    {
        // Arrange
        var command = new CreateCategoryCommand("Vocabulary", "Words and expressions");
        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        await _categoryRepository.Received(1).AddAsync(Arg.Is<Category>(c => c.Name == "Vocabulary" && c.Description == "Words and expressions"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowCategoryAlreadyExistsException()
    {
        // Arrange
        var command = new CreateCategoryCommand("Grammar", "Grammar rules");
        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<CategoryAlreadyExistsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new CategoryNotFoundException(request.Id);
        }

        // Check name uniqueness if it has changed
        if (!string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                throw new CategoryAlreadyExistsException(request.Name);
            }
        }

        category.Update(request.Name, request.Description);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

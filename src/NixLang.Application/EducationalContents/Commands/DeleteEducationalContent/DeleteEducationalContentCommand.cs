using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Commands.DeleteEducationalContent;

public record DeleteEducationalContentCommand(Guid Id) : IRequest<bool>;

public class DeleteEducationalContentCommandHandler : IRequestHandler<DeleteEducationalContentCommand, bool>
{
    private readonly IEducationalContentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEducationalContentCommandHandler(
        IEducationalContentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteEducationalContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            throw new EducationalContentNotFoundException(request.Id);
        }

        var inUse = await _repository.IsContentInUseAsync(request.Id, cancellationToken);
        if (inUse)
        {
            throw new EducationalContentInUseException(request.Id);
        }

        await _repository.DeleteAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

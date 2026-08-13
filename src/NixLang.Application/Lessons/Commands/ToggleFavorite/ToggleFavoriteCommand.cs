using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Application.Lessons.Commands.ToggleFavorite;

public record ToggleFavoriteCommand(Guid LessonId) : IRequest<bool>;

public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, bool>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFavoriteCommandHandler(
        IFavoriteRepository favoriteRepository,
        ILessonRepository lessonRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _lessonRepository = lessonRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Verify lesson exists
        var lesson = await _lessonRepository.GetPublishedByIdAsync(request.LessonId, cancellationToken);

        if (lesson == null)
        {
            throw new LessonNotFoundException(request.LessonId);
        }

        // 2. Check if already favorited
        var favorite = await _favoriteRepository.GetAsync(userId, request.LessonId, cancellationToken);

        if (favorite != null)
        {
            // Remove
            _favoriteRepository.Remove(favorite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            // Add
            var newFavorite = new Favorite(userId, request.LessonId);
            await _favoriteRepository.AddAsync(newFavorite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

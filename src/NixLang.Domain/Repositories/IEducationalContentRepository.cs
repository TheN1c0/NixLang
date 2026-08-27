using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Repositories;

public interface IEducationalContentRepository
{
    Task<IReadOnlyList<EducationalContent>> GetPublishedAsync(
        int page,
        int pageSize,
        string? search = null,
        EducationalContentType? type = null,
        ReferenceLevel? level = null,
        CancellationToken cancellationToken = default);

    Task<int> CountPublishedAsync(
        string? search = null,
        EducationalContentType? type = null,
        ReferenceLevel? level = null,
        CancellationToken cancellationToken = default);

    Task<EducationalContent?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationalContent>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        EducationalContentType? type = null,
        PublicationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(
        string? search = null,
        EducationalContentType? type = null,
        PublicationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<EducationalContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(EducationalContent content, CancellationToken cancellationToken = default);

    Task DeleteAsync(EducationalContent content, CancellationToken cancellationToken = default);

    Task<bool> IsContentInUseAsync(Guid contentId, CancellationToken cancellationToken = default);
}

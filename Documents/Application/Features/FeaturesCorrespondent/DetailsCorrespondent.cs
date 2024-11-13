
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.CustomFields;
using MediatR;

namespace Application.Features.FeaturesCorrespondent
{
    public class DetailsCorrespondent
    {
        public sealed record Query : IRequest<Correspondent>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, Correspondent>
        {
            private readonly ICorrespondentRepository _repository;

            public Handler(ICorrespondentRepository repository)
            {
                _repository = repository;
            }

            public async Task<Correspondent> Handle(Query request, CancellationToken cancellationToken)
            {
                Correspondent CorrespondentDetails = await _repository.FindByIdAsync(request.Id, cancellationToken);
                if (CorrespondentDetails == null)
                {
                    throw new CorrespondentException($"Correspondent with ID {request.Id} not found.");
                }
                return CorrespondentDetails;
            }
        }
    }
}

using Application.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureGroup
{
    public class DeleteGroup
    {
        public sealed record Command : IRequest
        {
            public readonly Guid Id;
            public Command(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IGroupRepository _repository;

            public Handler(IGroupRepository repository)
            {
                _repository = repository;

            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {

                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}

using Application.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeaturesMailRule
{
    public class DeleteMailRule
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
            private readonly IMailRuleRepository _repository;

            public Handler(IMailRuleRepository repository)
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

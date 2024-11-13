using Application.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureUser
{
    public class GetListGroupsByID
    {
        public sealed record Query : IRequest<List<string>>
        {
            public readonly string Id;
            public Query(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, List<string>>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
            {

                List<string> userdetails = await _repository.GetListGroupsByID(request.Id, cancellationToken);

                return userdetails;
            }
        }
    }
}

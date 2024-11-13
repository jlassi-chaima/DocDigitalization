using Application.Repository;
using Domain.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureUser
{
    public class GetNameUser
    {
        public sealed record Query : IRequest<string>
        {
            public readonly string Id;
            public Query(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, string>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                ApplicationUser userdetails = await _repository.FindByIdAsyncString(request.Id, cancellationToken);

                return userdetails.FirstName+ " "+userdetails.LastName;
            }
        }
    }
}

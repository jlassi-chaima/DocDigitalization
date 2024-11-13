using Application.Repository;
using Domain.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureGroup
{
    public class ListGroup
    {
        public sealed record Query : IRequest<List<Groups>>
        {


        }
        public sealed class Handler : IRequestHandler<Query, List<Groups>>
        {
            private readonly IGroupRepository _groupRepository;

            public Handler(IGroupRepository groupRepository)
            {
                _groupRepository = groupRepository;
            }

            public async Task<List<Groups>> Handle(Query request, CancellationToken cancellationToken)
            {
                return (List<Groups>)await _groupRepository.GetAllAsync();
            }
        }
    }
}

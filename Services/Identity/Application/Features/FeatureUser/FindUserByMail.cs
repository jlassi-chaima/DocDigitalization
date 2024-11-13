using Application.Repository;
using Domain.User;
using MediatR;


namespace Application.Features.FeatureUser
{
    public class FindUserByMail
    {
        public sealed record Query : IRequest<string>
        {
            public readonly string Mail;
            public Query(string mail)
            {
                Mail = mail;
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
                ApplicationUser userdetails = await _repository.FindUserByMailAsyncString(request.Mail, cancellationToken);
                if(userdetails != null)
                {
                    return userdetails.Id;
                }
                return null;
            }
        }
    }
}

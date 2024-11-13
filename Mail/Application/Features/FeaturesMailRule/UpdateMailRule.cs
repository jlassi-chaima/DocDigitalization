
//using Application.Dtos.MailRule;
//using Application.Repository;
//using Core.Database;
//using Domain.MailAccounts;
//using Domain.MailRules;
//using FluentValidation;
//using MapsterMapper;
//using MediatR;

//namespace Application.Features.FeaturesMailRule
//{
//    public class UpdateMailRule
//    {
//        public sealed record Command : IRequest<MailRule>
//        {
//            public readonly Guid MailRuleId;
//            public readonly MailRuleDto MailRuleupdate;

//            public Command(MailRuleDto mailRuleupdate, Guid id)
//            {
//                MailRuleupdate = mailRuleupdate;
//                MailRuleId = id;
//            }
//        }
//        public sealed class AddValidator : AbstractValidator<Command>
//        {
//            public AddValidator(IMailRuleRepository _repository)
//            {
//                RuleFor(mr => mr.MailRuleupdate.Name).NotEmpty().MustAsync(async (name, ct) => !await _repository.ExistsAsync(mr => mr.Name == name , ct))
//                                 .WithMessage("Name must be unique.");

//                RuleFor(mr => mr.MailRuleupdate.Order).NotEmpty().MustAsync(async (order, ct) => !await _repository.ExistsAsync(mr => mr.Order == order, ct))
//                                 .WithMessage("Order must be unique.");

//            }
//        }
//        public sealed class Handler : IRequestHandler<Command, MailRule>
//        {
//            private readonly IMailRuleRepository _repository;
//            private readonly IMapper _mapper;
//            public Handler(IMailRuleRepository repository, IMapper mapper)
//            {
//                _repository = repository;
//                _mapper = mapper;

//            }

//            public async Task<MailRule> Handle(Command request, CancellationToken cancellationToken)
//            {
//                MailRule mailruletoupdate = _repository.FindByIdAsync(request.MailRuleId, cancellationToken).GetAwaiter().GetResult();

//                _mapper.Map(request.MailRuleupdate, mailruletoupdate);
//                await _repository.UpdateAsync(mailruletoupdate);
//                return mailruletoupdate;

//            }
//        }
//    }
//}
using Application.Dtos.MailRule;
using Application.Repository;
using Core.Database;
using Domain.MailAccounts;
using Domain.MailRules;
using FluentValidation;
using MapsterMapper;
using MediatR;

namespace Application.Features.FeaturesMailRule
{
    public class UpdateMailRule
    {
        public sealed record Command : IRequest<MailRule>
        {
            public readonly Guid MailRuleId;
            public readonly MailRuleDto MailRuleUpdate;

            public Command(MailRuleDto mailRuleUpdate, Guid id)
            {
                MailRuleUpdate = mailRuleUpdate;
                MailRuleId = id;
            }
        }

        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(IMailRuleRepository repository)
            {
                RuleFor(mr => mr.MailRuleUpdate.Name)
                    .NotEmpty()
                    .MustAsync(async (command, name, ct) =>
                        !await repository.ExistsAsync(
                            mr => mr.Name == name && mr.Id != command.MailRuleId, ct))
                    .WithMessage("Name must be unique.")
                    .WithState(cmd => cmd);

                RuleFor(mr => mr.MailRuleUpdate.Order)
                    .NotEmpty()
                    .MustAsync(async (command, order, ct) =>
                        !await repository.ExistsAsync(
                            mr => mr.Order == order && mr.Id != command.MailRuleId, ct))
                    .WithMessage("Order must be unique.")
                    .WithState(cmd => cmd);
            }
        }

        public sealed class Handler : IRequestHandler<Command, MailRule>
        {
            private readonly IMailRuleRepository _repository;
            private readonly IMapper _mapper;

            public Handler(IMailRuleRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<MailRule> Handle(Command request, CancellationToken cancellationToken)
            {
                MailRule mailRuleToUpdate = await _repository.FindByIdAsync(request.MailRuleId, cancellationToken);

                _mapper.Map(request.MailRuleUpdate, mailRuleToUpdate);
                await _repository.UpdateAsync(mailRuleToUpdate);
                return mailRuleToUpdate;
            }
        }
    }
}
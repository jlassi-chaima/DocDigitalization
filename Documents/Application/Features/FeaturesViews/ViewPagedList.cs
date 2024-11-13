

using Application.Dtos.View;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.Views;
using MediatR;
using Serilog;

namespace Application.Features.FeaturesViews
{
    public class ViewPagedList
    {
        public sealed record Query : IRequest<PagedList<View>>
        {
            public ViewParameters ViewParameters { get; set; }
            public string Owner { get; set; }
            public string? NameIcontains { get; set; }

            public Query(ViewParameters viewParameters, string owner, string name_icontains)
            {
                ViewParameters = viewParameters;
                Owner = owner;
                NameIcontains = name_icontains;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<View>>
        {
            private readonly IViewRepository _viewRepository;

            public Handler(IViewRepository viewRepository)
            {
                _viewRepository = viewRepository;
            }

            public async Task<PagedList<View>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    return await _viewRepository.GetViewsAsync<ViewDto>(request.ViewParameters, request.Owner, request.NameIcontains);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }

            }
        }
    }
}

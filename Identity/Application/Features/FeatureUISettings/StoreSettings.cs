//using Application.Dtos.UISettingsDTO;
//using Application.Repository;
//using Domain.Settings;
//using MapsterMapper;
//using MediatR;
//using Newtonsoft.Json;


//namespace Application.Features.FeatureUISettings
//{
//    public class StoreSettings
//    {
//        public sealed record Command : IRequest<UISettings>
//        {
//            public readonly Guid Id;
//            public readonly StoreSettingsDTO StoreSettings;

//            public Command(StoreSettingsDTO storesettings, Guid id)
//            {
//                StoreSettings = storesettings;
//                Id = id;
//            }
//        }
//        public sealed class Handler : IRequestHandler<Command, UISettings>
//        {
//            private readonly IUISettingsRepository _repository;
//            private readonly IMapper _mapper;
//            public Handler(IUISettingsRepository repository, IMapper mapper)
//            {
//                _repository = repository;
//                _mapper = mapper;
//            }
//            public async Task<UISettings> Handle(Command request, CancellationToken cancellationToken)
//            {
//                UISettings uisettings =await _repository.FindByIdAsync(request.Id);
//                if (request.StoreSettings.Settings.Dark_mode != null)
//                {
//                    uisettings.DarkMode_enabled = (bool)request.StoreSettings.Settings.Dark_mode.Enabled;
//                }

//                uisettings.Language = request.StoreSettings.Settings.Language;
//                uisettings.DocumentListSize = request.StoreSettings.Settings.DocumentListSize;
//                uisettings.Settings = JsonConvert.SerializeObject(request.StoreSettings.Settings);
//                await _repository.UpdateAsync(uisettings);


//                return uisettings;



//            }

//        }
//    }
//}
using Application.Dtos.UISettingsDTO;
using Application.Repository;
using Domain.Settings;
using MapsterMapper;
using MediatR;
using Newtonsoft.Json;

namespace Application.Features.FeatureUISettings
{
    public class StoreSettings
    {
        public sealed record Command : IRequest<UISettings>
        {
            public readonly Guid Id;
            public readonly StoreSettingsDTO StoreSettings;

            public Command(StoreSettingsDTO storesettings, Guid id)
            {
                StoreSettings = storesettings;
                Id = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, UISettings>
        {
            private readonly IUISettingsRepository _repository;
            private readonly IMapper _mapper;

            public Handler(IUISettingsRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<UISettings> Handle(Command request, CancellationToken cancellationToken)
            {
                UISettings uisettings = await _repository.FindByIdAsync(request.Id);

                if (request.StoreSettings.Settings.Dark_mode != null)
                {
                    if (request.StoreSettings.Settings.Dark_mode.Enabled.HasValue)
                    {
                        uisettings.DarkMode_enabled = request.StoreSettings.Settings.Dark_mode.Enabled.Value;
                    }
                    else
                    {
                        uisettings.DarkMode_enabled = false; // Or handle null case appropriately
                    }
                }

                uisettings.Language = request.StoreSettings.Settings.Language;
                uisettings.DocumentListSize = request.StoreSettings.Settings.DocumentListSize;
                uisettings.Settings = JsonConvert.SerializeObject(request.StoreSettings.Settings);

                await _repository.UpdateAsync(uisettings);

                return uisettings;
            }
        }
    }
}

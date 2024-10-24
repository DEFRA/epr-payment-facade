using AutoMapper;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Common.Mapping
{
    [ExcludeFromCodeCoverage]
    public class OfflinePaymentRequestMappingProfile : Profile
    {
        public OfflinePaymentRequestMappingProfile()
        {
            CreateMap<OfflinePaymentRequestDto, InsertOfflinePaymentRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount!.Value));
        }
    }
}

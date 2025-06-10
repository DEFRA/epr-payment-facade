using AutoMapper;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.V2Payments;
using EPR.Payment.Facade.Common.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Common.Mapping
{
    [ExcludeFromCodeCoverage]
    public class PaymentRequestMappingProfileV2 : Profile
    {
        public PaymentRequestMappingProfileV2()
        {
            CreateMap<OnlinePaymentRequestV2Dto, GovPayRequestV2Dto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount!.Value))
                .ForMember(dest => dest.OrganisationId, opt => opt.MapFrom(src => src.OrganisationId!.Value))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.return_url, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<OnlinePaymentRequestV2Dto, InsertOnlinePaymentRequestV2Dto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.OrganisationId, opt => opt.MapFrom(src => src.OrganisationId!.Value))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount!.Value))
                .ForMember(dest => dest.ReasonForPayment, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
using AutoMapper;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Common.Mapping
{
    [ExcludeFromCodeCoverage]
    public class PaymentRequestMappingProfile : Profile
    {
        public PaymentRequestMappingProfile()
        {
            CreateMap<PaymentRequestDto, GovPayRequestDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount!.Value))
                .ForMember(dest => dest.OrganisationId, opt => opt.MapFrom(src => src.OrganisationId!.Value))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.return_url, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore());

            CreateMap<PaymentRequestDto, InsertPaymentRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.OrganisationId, opt => opt.MapFrom(src => src.OrganisationId!.Value))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount!.Value))
                .ForMember(dest => dest.ReasonForPayment, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<PaymentRequestDto, UpdatePaymentRequestDto>()
                .ForMember(dest => dest.ExternalPaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.GovPayPaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatus.InProgress))
                .ForMember(dest => dest.UpdatedByOrganisationId, opt => opt.MapFrom(src => src.OrganisationId!.Value))
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.MapFrom(src => src.UserId!.Value))
                .ForMember(dest => dest.ErrorCode, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore());

            CreateMap<PaymentDetailsDto, UpdatePaymentRequestDto>()
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.MapFrom(src => src.UpdatedByUserId))
                .ForMember(dest => dest.UpdatedByOrganisationId, opt => opt.MapFrom(src => src.UpdatedByOrganisationId))
                .ForMember(dest => dest.ExternalPaymentId, opt => opt.MapFrom(src => src.ExternalPaymentId))
                .ForMember(dest => dest.Reference, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorCode, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
                .ForMember(dest => dest.GovPayPaymentId, opt => opt.Ignore());
        }
    }
}

﻿namespace EPR.Payment.Facade.Common.Constants
{
    public static class ValidationMessages
    {
        // Producer Registration Fee Validation Messages
        public const string ProducerTypeInvalid = "Producer Type must be one of the following: ";
        public const string NumberOfSubsidiariesRange = "Number of subsidiaries must be greater than or equal to 0.";
        public const string NumberOfOMPSubsidiariesLessThanOrEqualToNumberOfSubsidiaries = "Number of online marketplace subsidiaries must be less than or equal to number of subsidiaries.";
        public const string ApplicationReferenceNumberRequired = "Application Reference Number is required.";
        public const string NoOfSubsidiariesOnlineMarketplaceRange = "Number of Subsidiaries with Online Marketplace must be greater than or equal to 0.";

        // PaymentRequestDto Validation Messages
        public const string UserIdRequired = "User ID is required.";
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string ReferenceRequired = "Reference is required.";
        public const string AmountRequired = "Amount is required";
        public const string AmountGreaterThanZero = "Amount must be greater than zero.";
        public const string RegulatorInvalid = "Invalid Regulator.";
        public const string RequestorTypeNotValid= "Invalid Requestor Type.";
        public const string RegulatorNotENG = "Online payment is not supported for this regulator, acceptable value is 'GB-ENG'.";
        public const string DescriptionRequired = "The Description field is required.";
        public const string InvalidDescription = "Description is invalid; acceptable values are 'Registration fee' or 'Packaging data resubmission fee'.";
        public const string InvalidDescriptionV2 = "Description is invalid; acceptable values are 'Registration fee' or 'Packaging data resubmission fee' or 'Accreditation fee'.";
        public const string InvalidRegulatorOffline = "Regulator is invalid; acceptable values are 'GB-ENG', 'GB-SCT', 'GB-WLS' and 'GB-NIR'.";                
        public const string OfflinePaymentMethodRequired = "The PaymentMethod field is required.";
        public const string OfflinePaymentOrgIdRequired = "Organization Id must not be null";
        public const string OfflinePaymentOrgIdNotDefaultGuid = "Organization Id must not be default Guid value";

        // Common Validation Messages
        public const string RegulatorRequired = "Regulator is required.";
        public const string InvalidComplianceSchemeMember = "Invalid ComplianceSchemeMember entry.";
        public const string InvalidMemberId = "MemberId is required.";
        public const string MemberTypeRequired = "MemberType is required.";
        public const string InvalidMemberType = "Member Type must be one of the following: "; 
        public const string InvalidSubmissionDate = "Submission date is mandatory and must be a valid date.";
        public const string FutureSubmissionDate = "Submission date cannot be a date in the future.";
        public const string SubmissionDateMustBeUtc = "Submission date must be in the UTC format which is YYYY-MM-DDTHH:MM:SSZ.";

        // Compliance Scheme Resubmission Fee Validation Messages
        public const string ResubmissionDateRequired = "Resubmission date is mandatory and must be a valid date.";
        public const string FutureResubmissionDate = "Resubmission date cannot be a date in the future.";
        public const string ResubmissionDateMustBeUtc = "Resubmission date must be in the UTC format which is YYYY-MM-DDTHH:MM:SSZ.";
        public const string ReferenceNumberRequired = "Reference Number is required.";
        public const string MemberCountGreaterThanZero = "Member Count must be greater than zero.";

        // Reprocessor-Exporter Registration Fees Validation Messages
        public const string ReprocessorExporterDateRequired = "Submission date is mandatory and must be a valid date.";
        public const string RexExFutureResubmissionDate = "Submission date cannot be a date in the future.";
        public const string RexExsubmissionDateMustBeUtc = "Submission date must be in the UTC format which is YYYY-MM-DDTHH:MM:SSZ.";
        public const string MaterialTypeInvalid = "Invalid MaterialType.";
        public const string RequestorTypeInvalid = "Invalid RequestorType.";
        public const string RegulatorTypeInvalid = "Invalid Regulator.";

        //  AccreditationFeesRequestDto Validation Messages
        public const string EmptyRequestorType = "Requestor type is required";
        public const string InvalidRequestorType = "Requestor type must be one of the following: ";
        public const string EmptyTonnageBand = "Tonnage band is required";
        public const string InvalidTonnageBand = "Tonnage band must be one of the following: ";
        public const string EmptyMaterialType = "Material type is required";
        public const string InvalidMaterialType = "Material type must be one of the following: ";
        public static string InvalidNumberOfOverseasSiteForExporter = $"Number of Overseas site must be greater than 0 and less than equal to {ReprocessorExporterConstants.MaxNumberOfOverseasSitesAllowed} for requestor type of exporter.";
        public const string InvalidNumberOfOverseasSiteForReprocessor = "Number of Overseas site must be 0 for requestor type of reprocessor.";
    }
}
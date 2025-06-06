# epr-payment-facade


## Description
Service Facade to calculate fees and manage payment records for EPR

## Getting Started

### Prerequisites
- EPR Payment Service - https://github.com/DEFRA/epr-payment-service
- .NET 8 SDK
- Visual Studio or Visual Studio Code
- GovPayService API Key

### Installation
1. Clone the repository:
    ```bash
    git clone https://github.com/DEFRA/epr-payment-facade.git
    ```
2. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Facade
    ```
3. Restore the dependencies:
    ```bash
    dotnet restore
    ```

### Configuration
The application uses appsettings.json for configuration. For development, use appsettings.Development.json.

Replace the BearerToken below with your GovPayService API Key

#### Sample 
appsettings.Development.json

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.FeatureManagement": "Information"
    }
  },
  "AllowedHosts": "*",
  "FeatureManagement": {
    "EnableOnlinePaymentsFeature": true,
    "EnablePaymentInitiation": true,
    "EnablePaymentCompletion": true,
    "EnableProducersFeesFeature": true,
    "EnableProducersFeesCalculation": true,
    "EnableProducerResubmissionFee": true,
    "EnableComplianceSchemeFeature": true,
    "EnableComplianceSchemeFees": true,
    "EnableHomePage": true,
    "EnableProducersResubmissionFeesFeature": true,
    "EnableResubmissionComplianceSchemeFeature": true,
    "EnableResubmissionFeesCalculation": true,
    "EnableOfflinePaymentsFeature": true,
    "EnableOfflinePayment": true,
    "EnableOfflinePaymentV2": true,
    "EnableAuthenticationFeature": false,
    "EnableReprocessorOrExporterRegistrationFeesFeature": true,
    "EnableReprocessorOrExporterRegistrationFeesCalculation": true
  },
   "AzureAdB2C": {
    "Instance": "*****",
    "Domain": "*****",
    "ClientId": "*****",
    "SignUpSignInPolicyId": "*****",
    "Scopes": "*****",
  },
  "Services": {
    "PaymentServiceHealthCheck": {
      "Url": "https://localhost:7107/",
      "EndPointName": "health"
    },
    "PaymentService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "payment_service_client"
    },
    "GovPayService": {
      "Url": "https://publicapi.payments.service.gov.uk",
      "EndPointName": "v1",
      "BearerToken": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
      "Retries": 3
    },
    "ProducerFeesService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "producer_fees_service_client",
      "ServiceClientId": "********-****-****-****-************"
    },
    "ComplianceSchemeFeesService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "compliance_scheme_fees_service_client",
      "ServiceClientId": "********-****-****-****-************"
    },
    "OfflinePaymentService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "offline_payment_service_client",
       "ServiceClientId": "********-****-****-****-************"
    },
    "OfflinePaymentServiceV2": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v2",
      "HttpClientName": "offline_payment_service_client",
       "ServiceClientId": "********-****-****-****-************"
    },
    "RegistrationFeesService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "registration_fees_service_client"
    },
     "ProducerResubmissionFeesService": {
      "Url": "https://localhost:7107/",
      "EndPointName": "api/v1",
      "HttpClientName": "producer_resubmission_fees_service_client",
       "ServiceClientId": "********-****-****-****-************"
    }
  },
  "PaymentServiceOptions": {
    "ReturnUrl": "https://localhost:7274/GovPayCallback",
    "Description": "Registration fee",
    "ErrorUrl": "https://localhost:7274/"
  },
  "AllowedOrigins": [
    "https://localhost:7166",
    "https://card.payments.service.gov.uk"
  ]
}
```

### Building the Application
1. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Facade
    ```

2. To build the application:
    ```bash
    dotnet build
    ```

### Running the Application
1. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Facade
    ```
 
2. To run the service locally:
    ```bash
    dotnet run
    ```

3. Launch Browser:

    Service Health Check:
    [https://localhost:7166/health](https://localhost:7166/health)

    Swagger:
    [https://localhost:7166/swagger/index.html](https://localhost:7166/swagger/index.html)
    
    Sample Test UI:
    [https://localhost:7166/index.html](https://localhost:7166/index.html)
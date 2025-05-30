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
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AzureAdB2C": {
        "Instance": "https://xxxxxxx.b2clogin.com",
        "Domain": "xxxxxxx.onmicrosoft.com",
        "ClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx",
        "SignUpSignInPolicyId": "B2C_1A_EPR_SignUpSignIn",
        "Scopes": "https://xxxxxxx.onmicrosoft.com/epr-dev-payments-facade/payment-service"
    },
    "Services": {
        "PaymentServiceHealthCheck": {
            "Url": "https://localhost:7107/",
            "EndPointName": "health"
        },
        "PaymentService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "v1",
            "HttpClientName": "payment_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "GovPayService": {
            "Url": "https://publicapi.payments.service.gov.uk",
            "EndPointName": "v1",
            "BearerToken": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            "ServiceClientId": "TEST"
        },
        "ProducerFeesService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "api/v1",
            "HttpClientName": "producer_fees_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "ComplianceSchemeFeesService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "api/v1",
            "HttpClientName": "compliance_scheme_fees_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "OfflinePaymentService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "api/v1",
            "HttpClientName": "offline_payment_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "RegistrationFeesService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "api/v1",
            "HttpClientName": "registration_fees_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "ProducerResubmissionFeesService": {
            "Url": "https://devrwdwebwab425.azurewebsites.net/",
            "EndPointName": "api/v1",
            "HttpClientName": "producer_resubmission_fees_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        },
        "RexExpoAccreditationFeesService": {
            "Url": "https://localhost:7107/",
            "EndPointName": "api/v1",
            "HttpClientName": "rexexpo_accreditation_fees_service_client",
            "ServiceClientId": "xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx"
        }
    },
    "PaymentServiceOptions": {
        "ReturnUrl": "https://localhost:7274/GovPayCallback",
        "Description": "Static payment description",
        "ErrorUrl": "https://localhost:7274/"
    },
    "AllowedOrigins": [
        "https://localhost:7166",
        "https://card.payments.service.gov.uk"
    ],
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
        "EnableAuthenticationFeature": false,
        "EnableReprocessorOrExporterRegistrationFeesFeature": true,
        "EnableReprocessorOrExporterRegistrationFeesCalculation": true,
        "EnableReprocessorOrExporterAccreditationFeesFeature": true,
        "EnableReprocessorOrExporterAccreditationFeesCalculation": true
    }
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
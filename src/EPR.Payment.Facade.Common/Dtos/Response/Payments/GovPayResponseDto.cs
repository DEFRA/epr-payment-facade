using Newtonsoft.Json;

public class GovPayResponseDto
{
    [JsonProperty("amount")]
    public int Amount { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("reference")]
    public string? Reference { get; set; }

    [JsonProperty("language")]
    public string? Language { get; set; }

    [JsonProperty("state")]
    public StateDto? State { get; set; }

    [JsonProperty("payment_id")]
    public string? PaymentId { get; set; }

    [JsonProperty("payment_provider")]
    public string? PaymentProvider { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("refund_summary")]
    public RefundSummaryDto? RefundSummary { get; set; }

    [JsonProperty("settlement_summary")]
    public SettlementSummaryDto? SettlementSummary { get; set; }

    [JsonProperty("delayed_capture")]
    public bool DelayedCapture { get; set; }

    [JsonProperty("moto")]
    public bool Moto { get; set; }

    [JsonProperty("return_url")]
    public string? ReturnUrl { get; set; }

    [JsonProperty("authorisation_mode")]
    public string? AuthorisationMode { get; set; }

    [JsonProperty("_links")]
    public LinksDto? Links { get; set; }
}

public class StateDto
{
    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("finished")]
    public bool Finished { get; set; }
}

public class RefundSummaryDto
{
    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("amount_available")]
    public int AmountAvailable { get; set; }

    [JsonProperty("amount_submitted")]
    public int AmountSubmitted { get; set; }
}

public class SettlementSummaryDto
{
    // Define properties as needed, if any
}

public class LinksDto
{
    [JsonProperty("self")]
    public LinkDto? Self { get; set; }

    [JsonProperty("next_url")]
    public LinkDto? NextUrl { get; set; }

    [JsonProperty("next_url_post")]
    public NextUrlPostDto? NextUrlPost { get; set; }

    [JsonProperty("events")]
    public LinkDto? Events { get; set; }

    [JsonProperty("refunds")]
    public LinkDto? Refunds { get; set; }

    [JsonProperty("cancel")]
    public LinkDto? Cancel { get; set; }
}

public class LinkDto
{
    [JsonProperty("href")]
    public string? Href { get; set; }

    [JsonProperty("method")]
    public string? Method { get; set; }
}

public class NextUrlPostDto : LinkDto
{
    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("params")]
    public ParamsDto? Params { get; set; }
}

public class ParamsDto
{
    [JsonProperty("chargeTokenId")]
    public string? ChargeTokenId { get; set; }
}

using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    public class SsiQueryResponse<T>
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public bool IsSuccess => Code == "SUCCESS";
    }

    public class SsiApiResponseV1<T> : SsiQueryResponse<T>
    {
        [JsonPropertyName("paging")]
        public int? Paging { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    #region Symbol + details
    public class SsiSymbolDto
    {
        [JsonPropertyName("isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("companyNameEn")]
        public string? CompanyNameEn { get; set; }

        [JsonPropertyName("companyNameVi")]
        public string? CompanyNameVi { get; set; }

        [JsonPropertyName("exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("stockSymbol")]
        public string? StockSymbol { get; set; }

        [JsonPropertyName("stockType")]
        public string? StockType { get; set; }

        [JsonPropertyName("tradingStatus")]
        public string? TradingStatus { get; set; }
    }

    public class SsiSymbolDetailsDto
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("subSectorCode")]
        public string? SubSectorCode { get; set; }

        [JsonPropertyName("industryName")]
        public string? IndustryName { get; set; }

        [JsonPropertyName("superSector")]
        public string? SuperSector { get; set; }

        [JsonPropertyName("sector")]
        public string? Sector { get; set; }

        [JsonPropertyName("subSector")]
        public string? SubSector { get; set; }

        [JsonPropertyName("foundingDate")]
        public string? FoundingDate { get; set; }

        [JsonPropertyName("charterCapital")]
        public string? CharterCapital { get; set; }

        [JsonPropertyName("numberOfEmployee")]
        public int? NumberOfEmployee { get; set; }

        [JsonPropertyName("bankNumberOfBranch")]
        public int? BankNumberOfBranch { get; set; }

        [JsonPropertyName("companyProfile")]
        public string? CompanyProfile { get; set; }

        [JsonPropertyName("listingDate")]
        public string? ListingDate { get; set; }

        [JsonPropertyName("exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("firstPrice")]
        public string? FirstPrice { get; set; }

        [JsonPropertyName("issueShare")]
        public string? IssueShare { get; set; }

        [JsonPropertyName("listedValue")]
        public string? ListedValue { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("quantity")]
        public long? Quantity { get; set; }

        [JsonPropertyName("stockType")]
        public string? StockType { get; set; }

        [JsonPropertyName("freeFloatRate")]
        public string? FreeFloatRate { get; set; }

        [JsonPropertyName("updateDate")]
        public string? UpdateDate { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("telephone")]
        public string? Telephone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("fax")]
        public string? Fax { get; set; }
    }
    #endregion Symbol + details

    #region Industry
    public class SsiIndustryDto
    {
        [JsonPropertyName("industryName")]
        public SsiIndustryName? IndustryName { get; set; }

        [JsonPropertyName("industryCode")]
        public string? IndustryCode { get; set; }

        [JsonPropertyName("listCompany")]
        public List<SsiCompanyOfIndustry>? CompanyList { get; set; }

        [JsonPropertyName("industryLevel")]
        public string? IndustryLevel { get; set; }

        [JsonPropertyName("industryCloseIndex")]
        public string? IndustryCloseIndex { get; set; }
    }

    public class SsiIndustryName
    {
        [JsonPropertyName("vi")]
        public string? Vi { get; set; }

        [JsonPropertyName("vn")]
        public string? Vn { get; set; }

        [JsonPropertyName("en")]
        public string? En { get; set; }
    }

    public class SsiCompanyOfIndustry
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set;}
    }
    #endregion Industry
}

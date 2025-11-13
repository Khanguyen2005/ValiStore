using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ValiModern.Helpers
{
    public class PayPalClient
    {
        public string Mode { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        public string BaseUrl => Mode == "Live"
            ? "https://api-m.paypal.com"
            : "https://api-m.sandbox.paypal.com";

        public PayPalClient(string clientId, string clientSecret, string mode)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Mode = mode;
        }

        private async Task<AuthResponse> Authenticate()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{BaseUrl}/v1/oauth2/token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Authorization", $"Basic {auth}" }
                },
                Content = new FormUrlEncodedContent(content)
            };

            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(request);
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[PayPal Auth] Status: {httpResponse.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[PayPal Auth] Response: {jsonResponse}");

            var response = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

            return response;
        }

        public async Task<CreateOrderResponse> CreateOrder(string value, string currency, string reference)
        {
            var auth = await Authenticate();

            if (auth == null || string.IsNullOrEmpty(auth.access_token))
            {
                throw new Exception("Failed to authenticate with PayPal");
            }

            var request = new CreateOrderRequest
            {
                intent = "CAPTURE",
                purchase_units = new List<PurchaseUnit>
                {
                    new PurchaseUnit
                    {
                        reference_id = reference,
                        amount = new Amount
                        {
                            currency_code = currency,
                            value = value
                        }
                    }
                }
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var json = JsonConvert.SerializeObject(request);
            System.Diagnostics.Debug.WriteLine($"[PayPal CreateOrder] Request: {json}");

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync($"{BaseUrl}/v2/checkout/orders", httpContent);
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[PayPal CreateOrder] Status: {httpResponse.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[PayPal CreateOrder] Response: {jsonResponse}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"PayPal API error: {httpResponse.StatusCode} - {jsonResponse}");
            }

            var response = JsonConvert.DeserializeObject<CreateOrderResponse>(jsonResponse);

            return response;
        }

        public async Task<CaptureOrderResponse> CaptureOrder(string orderId)
        {
            var auth = await Authenticate();

            if (auth == null || string.IsNullOrEmpty(auth.access_token))
            {
                throw new Exception("Failed to authenticate with PayPal");
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var httpContent = new StringContent("", Encoding.Default, "application/json");
            var httpResponse = await httpClient.PostAsync($"{BaseUrl}/v2/checkout/orders/{orderId}/capture", httpContent);

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[PayPal Capture] Status: {httpResponse.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[PayPal Capture] Response: {jsonResponse}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"PayPal capture error: {httpResponse.StatusCode} - {jsonResponse}");
            }

            var response = JsonConvert.DeserializeObject<CaptureOrderResponse>(jsonResponse);

            return response;
        }
    }

    // Response models
    public class AuthResponse
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }

    public class CreateOrderRequest
    {
        public string intent { get; set; }
        public List<PurchaseUnit> purchase_units { get; set; } = new List<PurchaseUnit>();
    }

    public class CreateOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public List<Link> links { get; set; }
    }

    public class CaptureOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public PaymentSource payment_source { get; set; }
        public List<PurchaseUnit> purchase_units { get; set; }
        public Payer payer { get; set; }
        public List<Link> links { get; set; }
    }

    public class PurchaseUnit
    {
        public Amount amount { get; set; }
        public string reference_id { get; set; }
        public Shipping shipping { get; set; }
        public Payments payments { get; set; }
    }

    public class Payments
    {
        public List<Capture> captures { get; set; }
    }

    public class Shipping
    {
        public Address address { get; set; }
    }

    public class Capture
    {
        public string id { get; set; }
        public string status { get; set; }
        public Amount amount { get; set; }
        public SellerProtection seller_protection { get; set; }
        public bool final_capture { get; set; }
        public string disbursement_mode { get; set; }
        public SellerReceivableBreakdown seller_receivable_breakdown { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public List<Link> links { get; set; }
    }

    public class Amount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }

    public class Name
    {
        public string given_name { get; set; }
        public string surname { get; set; }
    }

    public class SellerProtection
    {
        public string status { get; set; }
        public List<string> dispute_categories { get; set; }
    }

    public class SellerReceivableBreakdown
    {
        public Amount gross_amount { get; set; }
        public PayPalFee paypal_fee { get; set; }
        public Amount net_amount { get; set; }
    }

    public class PayPal
    {
        public Name name { get; set; }
        public string email_address { get; set; }
        public string account_id { get; set; }
    }

    public class PayPalFee
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }

    public class Address
    {
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string admin_area_2 { get; set; }
        public string admin_area_1 { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
    }

    public class Payer
    {
        public Name name { get; set; }
        public string email_address { get; set; }
        public string payer_id { get; set; }
    }

    public class PaymentSource
    {
        public PayPal paypal { get; set; }
    }
}

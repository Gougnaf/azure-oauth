using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using ARMManager.Model;
using Microsoft.Extensions.Options;

namespace ARMManager.Controllers
{
    public class HomeController : Controller
    {
        private const string AZURE_LOGIN_BASE_URL = "https://login.microsoftonline.com/";
        private const string AZURE_OAUTH_RESOURCE = "https%3a%2f%2fmanagement.core.windows.net%2f";
        private const string AZURE_OAUTH_BASE_ADRESS = "https://login.windows.net/";
        private const string AZURE_OAUTH_TOKEN_PATH = "oauth2/token";
        private const string AZURE_OAUTH_AUTHORIZE_PATH = "oauth2/authorize";
        private const string AZURE_API_BASE_ADRESS = "https://management.azure.com/";
        private const string AZURE_SUBSCRIPTIONS_PATH = "subscriptions?api-version=2014-04-01-preview";
        private readonly AzureOptions _azureOptions;

        public HomeController(IOptions<AzureOptions> azureOptionsAccessor)
        {
            _azureOptions = azureOptionsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ChooseDomain()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChooseDomain(ViewModels.DomainChoosingViewModel domainVM)
        {

            return this.Redirect($"{AZURE_LOGIN_BASE_URL}{domainVM.Domain}/{AZURE_OAUTH_AUTHORIZE_PATH}?client_id={_azureOptions.AzureOAuthClientId}&response_mode=query&response_type=code&redirect_uri={_azureOptions.AzureOAuthRedirectUri}&resource={AZURE_OAUTH_RESOURCE}&domain_hint=live.com&state={domainVM.Domain}");
        }

        public async Task<IActionResult> SignIn(string error, string error_description, string code, string state, string session_state)
        {
            if (error != null)
                ViewData["Message"] = "/ error : " + error + Environment.NewLine + " / error_description : " + error_description;
            else
            {
                if (code != null && state != null)
                {
                    //ViewData["Message"] = "/ code : " + code + " / state : " + state + "session_state : " + session_state;

                    var token = await GetTokenAsync(code, state);

                    ViewData["Message"] += "/token : " + token.access_token;

                    var subscriptions = await GetUserAzureSubscritptionsAsync(token.access_token);
                    return View("Subscriptions", subscriptions);
                }
            }
            return View();
        }

        private async Task<IEnumerable<Model.Subscription>> GetUserAzureSubscritptionsAsync(string access_token)
        {
            var client = new HttpClient() { BaseAddress = new Uri(AZURE_API_BASE_ADRESS) };
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);
            var response = await client.GetStringAsync(AZURE_SUBSCRIPTIONS_PATH);

            var azureSubscriptionsResponse = JsonConvert.DeserializeObject<Model.AzureSubscriptionsResponse>(response);
            return azureSubscriptionsResponse?.value;
        }

        private async Task<Model.OAuthToken> GetTokenAsync(string code, string state)
        {
            var client = new HttpClient() { BaseAddress = new Uri(AZURE_OAUTH_BASE_ADRESS + state + "/") };
            var content = new FormUrlEncodedContent(new[]
            {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", _azureOptions.AzureOAuthRedirectUri),
                        new KeyValuePair<string, string>("client_id", _azureOptions.AzureOAuthClientId),
                        new KeyValuePair<string, string>("client_secret", _azureOptions.AzureOAuthClientSecret)
                    });
            var response = await client.PostAsync(AZURE_OAUTH_TOKEN_PATH, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Model.OAuthToken>(responseContent);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

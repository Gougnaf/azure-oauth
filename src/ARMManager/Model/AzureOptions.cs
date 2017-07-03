namespace ARMManager.Model
{
    public class AzureOptions
    {
        public AzureOptions() { }

        public string AzureOAuthClientId { get; set; }
        public string AzureOAuthRedirectUri { get; set; }
        public string AzureOAuthClientSecret { get; set; }
    }
}

namespace TaskManager.Services.Implementations
{
    public class TwoFAConfiguration : I2FAConfiguration
    {
        public string TwoFAExplainLink { get; set; }

        public string TwoFactorSecretCode { get; set; }

        public bool TwoFAMandatory { get; set; }
    }
}

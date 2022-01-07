using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Implementations
{
    public class TwoFAConfiguration : I2FAConfiguration
    {
        public bool TwoFAEnabled { get; set; }

        public string TwoFactorSecretCode { get; set; }
    }
}

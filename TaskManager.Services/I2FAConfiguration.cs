using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface I2FAConfiguration
    {
        string TwoFAExplainLink { get; }

        string TwoFactorSecretCode { get; }

        bool TwoFAMandatory { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface I2FAConfiguration
    {
        bool TwoFAEnabled { get; }

        string TwoFactorSecretCode { get; }
    }
}

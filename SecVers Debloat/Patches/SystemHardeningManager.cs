using System;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using SecVers_Debloat.Patches.Hardening;

namespace SecVers_Debloat.Patches
{
    public class SystemHardeningManager
    {
        private readonly NetworkHardening _networkHardening;
        private readonly ExploitProtection _exploitProtection;
        private readonly ServiceHardening _serviceHardening;
        private readonly AccountSecurity _accountSecurity;
        private readonly SystemIntegrity _systemIntegrity;
        private readonly PrivacyHardening _privacyHardening;
        private readonly AttackSurfaceReduction _asr;

        public SystemHardeningManager()
        {
            _networkHardening = new NetworkHardening();
            _exploitProtection = new ExploitProtection();
            _serviceHardening = new ServiceHardening();
            _accountSecurity = new AccountSecurity();
            _systemIntegrity = new SystemIntegrity();
            _privacyHardening = new PrivacyHardening();
            _asr = new AttackSurfaceReduction();
        }

       
        public NetworkHardening Network => _networkHardening;
        public ExploitProtection Exploit => _exploitProtection;
        public ServiceHardening Services => _serviceHardening;
        public AccountSecurity Accounts => _accountSecurity;
        public SystemIntegrity System => _systemIntegrity;
        public PrivacyHardening Privacy => _privacyHardening;
        public AttackSurfaceReduction ASR => _asr;
    }
}

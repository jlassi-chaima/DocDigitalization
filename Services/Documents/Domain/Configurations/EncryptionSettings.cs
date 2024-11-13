

using System.Security;

namespace Domain.Configurations
{
    public class EncryptionSettings : IDisposable
    {
   
        public SecureString? _secureKey;
        public string? SecureKey
        {
            get => null;
            set
            {
                _secureKey = ConvertToSecureString(value);
            }
        }
        private SecureString ConvertToSecureString(string? str)
        {
            var secureString = new SecureString();
            if (!string.IsNullOrEmpty(str))
            {
                foreach (char c in str)
                {
                    secureString.AppendChar(c);
                }
                secureString.MakeReadOnly();
            }
            return secureString;
        }
        public void Dispose()
        {
            _secureKey?.Dispose();
        }
     
    }
}

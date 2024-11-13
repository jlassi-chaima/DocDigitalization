using Domain.Configurations;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Application.Helper
{
    public  class EncryptionHelper(IConfiguration configuration)
    {

        private readonly EncryptionSettings _encryptionSettings = configuration.GetSection("EncryptionSettings").Get<EncryptionSettings>()!;

        // Static constructor to initialize configuration and settings
       

      
        private static byte[] GenerateStaticKey() => new byte[32]
        {
        0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
        0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
        0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
        0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00
        };

        public  byte[] EncryptKey(byte[] key)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Mode = CipherMode.CBC;

                    aesAlg.GenerateIV(); // Generate a new IV for each encryption
                    byte[] iv = aesAlg.IV;

                    using (var encryptor = aesAlg.CreateEncryptor(ConvertSecureStringToByteArray(_encryptionSettings._secureKey), iv))
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Store IV at the beginning of the encrypted data
                        msEncrypt.Write(iv, 0, iv.Length);
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(key, 0, key.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        return msEncrypt.ToArray(); // Return the IV + encrypted key
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
           
        }

        public  byte[] DecryptKey(byte[] encryptedKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                if (encryptedKey.Length < 16)
                {
                    throw new ArgumentException("Invalid encrypted key length.");
                }

                byte[] iv = new byte[16];
                Array.Copy(encryptedKey, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                using (var msDecrypt = new MemoryStream(encryptedKey, iv.Length, encryptedKey.Length - iv.Length))
                {
                    using (var decryptor = aesAlg.CreateDecryptor(ConvertSecureStringToByteArray(_encryptionSettings._secureKey), aesAlg.IV))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msPlainText = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlainText);
                        return msPlainText.ToArray(); // Return the decrypted key
                    }
                }
            }
        }

        public static byte[] GenerateEncryptionKey(int size)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] key = new byte[size];
                rng.GetBytes(key); 
                return key;
            }
        }

        public static byte[] GenerateEncryptionIV(int size)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] iv = new byte[size];
                rng.GetBytes(iv); // Generate a random IV of the specified size
                return iv;
            }
        }
        public static byte[] DecryptDocument(byte[] encryptedFileBytes, byte[] key, byte[] iv)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;  
                    aesAlg.BlockSize = 128; 
                    aesAlg.Padding = PaddingMode.PKCS7; 
                    aesAlg.Mode = CipherMode.CBC; 

                    aesAlg.Key = key; 
                    aesAlg.IV = iv;  

                    // Create a decryptor to perform the stream transform
                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (var msDecrypt = new MemoryStream(encryptedFileBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msResult = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msResult); // Decrypt and copy the result to the MemoryStream
                        return msResult.ToArray();   // Return the decrypted byte array
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Cryptographic error occurred during decryption: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error decrypting the document: " + ex.Message, ex);
            }
        }
        public static byte[] EncryptDocument(string filePath, byte[] key, byte[] iv)
        {
            try
            {
          
                byte[] fileBytes = File.ReadAllBytes(filePath);              
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;  
                    aesAlg.BlockSize = 128; 
                    aesAlg.Padding = PaddingMode.PKCS7;  
                    aesAlg.Mode = CipherMode.CBC; 

               
                    aesAlg.Key = key;  
                    aesAlg.IV = iv;   

                    // Encrypt the document
                    using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        private static string? ConvertToInsecureString(SecureString? secureString)
        {
            if (secureString == null) return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private static byte[] ConvertSecureStringToByteArray(SecureString secureString)
        {
            if (secureString == null || secureString.Length == 0)
            {
                throw new ArgumentNullException(nameof(secureString), "SecureString is null or empty.");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                string keyString = Marshal.PtrToStringUni(unmanagedString);
                try
                {
                    return Convert.FromBase64String(keyString);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Key string is not a valid Base64 string.", ex);
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

       
    }
}

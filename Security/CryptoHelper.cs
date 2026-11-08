using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AVRoad.Security
{
    /// <summary>
    /// Xử lý chữ ký số RSA-2048 xác thực bản quyền, tương thích 100% với .NET 2.0 / 3.5 / 4.0+.
    /// Sử dụng provider PROV_RSA_AES (24) để hỗ trợ SHA-256 trên mọi phiên bản Windows.
    /// </summary>
    public static class CryptoHelper
    {
        public const string PublicKeyPem = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAi9xQ1EpUK+sQCd+zVTAb
Ct2HdGFWcPgjPRCvREdZ7204aTLJ2lOKYi+y7/fQku2AdYSt/ueEiYzfMQn0GOw5
T3b8dGzKjCrdG2hHSK8tFlCYn/b9Jj28rU3Qjun4u5GXG1USTAT7J1CW/34Rtq56
OFC3zeCiCBfGfMJz9MUf7m+lZd6UQ1XEBMEcMwc3l5tV9rqT2EUwgAbgtDLtV9yJ
knaz6bHaTtqM4LUKV1TT2jXogXdw+N/2JGPtiG8kl2dHa1ABeG0ioBuIMUWvAe7i
bZcAYSxEycL2LtQvHf4FxkIyqGsmjegTaMiJaC+OzibvqQJj0G8jzQdio23K+sKa
oQIDAQAB
-----END PUBLIC KEY-----";

        private const string ModulusBase64 = "i9xQ1EpUK+sQCd+zVTAbCt2HdGFWcPgjPRCvREdZ7204aTLJ2lOKYi+y7/fQku2AdYSt/ueEiYzfMQn0GOw5T3b8dGzKjCrdG2hHSK8tFlCYn/b9Jj28rU3Qjun4u5GXG1USTAT7J1CW/34Rtq56OFC3zeCiCBfGfMJz9MUf7m+lZd6UQ1XEBMEcMwc3l5tV9rqT2EUwgAbgtDLtV9yJknaz6bHaTtqM4LUKV1TT2jXogXdw+N/2JGPtiG8kl2dHa1ABeG0ioBuIMUWvAe7ibZcAYSxEycL2LtQvHf4FxkIyqGsmjegTaMiJaC+OzibvqQJj0G8jzQdio23K+sKaoQ==";
        private const string ExponentBase64 = "AQAB";

        /// <summary>
        /// Xác thực tính toàn vẹn và hợp lệ của Payload bằng chữ ký số RSA-2048 (SHA256 PKCS#1 v1.5).
        /// </summary>
        public static bool VerifySignature(string payloadJson, string signatureBase64)
        {
            if (string.IsNullOrEmpty(payloadJson) || string.IsNullOrEmpty(signatureBase64))
                return false;

            try
            {
                // PROV_RSA_AES (24) hỗ trợ SHA-256 trên Windows XP SP3, Win 7, 8, 10, 11
                CspParameters csp = new CspParameters(24);
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
                {
                    RSAParameters p = GetPublicKeyParameters();
                    rsa.ImportParameters(p);

                    byte[] dataBytes = Encoding.UTF8.GetBytes(payloadJson);
                    byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] hash = sha.ComputeHash(dataBytes);
                        string oid = CryptoConfig.MapNameToOID("SHA256");
                        return rsa.VerifyHash(hash, oid, signatureBytes);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static RSAParameters GetPublicKeyParameters()
        {
            RSAParameters p = new RSAParameters();
            p.Modulus = Convert.FromBase64String(ModulusBase64);
            p.Exponent = Convert.FromBase64String(ExponentBase64);
            return p;
        }

        /// <summary>
        /// Trích xuất RSAParameters từ chuỗi PEM SubjectPublicKeyInfo nếu truyền khóa ngoài.
        /// </summary>
        public static RSAParameters ParsePemPublicKey(string pemString)
        {
            string clean = pemString
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();

            byte[] der = Convert.FromBase64String(clean);
            // Chuẩn PKCS#8 SubjectPublicKeyInfo cho RSA 2048:
            // 33 bytes header, sau đó là 256 bytes Modulus, 2 bytes phân cách, 3 bytes Exponent
            if (der.Length >= 294)
            {
                byte[] mod = new byte[256];
                Array.Copy(der, 33, mod, 0, 256);
                byte[] exp = new byte[3];
                Array.Copy(der, 291, exp, 0, 3);

                RSAParameters p = new RSAParameters();
                p.Modulus = mod;
                p.Exponent = exp;
                return p;
            }

            return GetPublicKeyParameters();
        }
    }
}

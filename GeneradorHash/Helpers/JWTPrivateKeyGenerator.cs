using System.Security.Cryptography;
using System.Text;

namespace JWTAuth.Helpers
{
    public class JWTPrivateKeyGenerator
    {
        //Recibimos un texto que convertiremos en clave para generar un Hash privado para JWT.
        public string GenerarClavePrivada512(string claveMaestra)
        {
            using var sha512 = SHA512.Create();
            byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(claveMaestra));

            // Devolvemos una clave base64 que es >= 512 bits (64 bytes en binario => 88 caracteres en Base64)
            return Convert.ToBase64String(hash);
        }
    }
}

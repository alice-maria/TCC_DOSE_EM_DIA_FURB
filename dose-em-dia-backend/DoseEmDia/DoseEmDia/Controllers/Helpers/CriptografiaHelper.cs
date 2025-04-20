using System.Security.Cryptography;
using System.Text;

namespace DoseEmDia.Controllers.Helpers
{
    public static class CriptografiaHelper
    {
        public static string GerarSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public static string GerarHash(string senha, string salt)
        {
            var senhaComSalt = senha + salt;
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senhaComSalt));
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerificarSenha(string senhaDigitada, string hashSalvo, string saltSalvo)
        {
            var hashDigitado = GerarHash(senhaDigitada, saltSalvo);
            return hashDigitado == hashSalvo;
        }
    }
}

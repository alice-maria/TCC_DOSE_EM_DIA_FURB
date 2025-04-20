namespace DoseEmDia.Models.Exceptions
{
    public class UsuarioException :Exception
    {
        public class UsuarioNaoEncontradoException : Exception
        {
            public UsuarioNaoEncontradoException(string email)
                : base($"Usuário com o e-mail '{email}' não foi encontrado.") { }
            public UsuarioNaoEncontradoException(int id)
                : base($"Usuário com o ID '{id}' não foi encontrado.") { }
        }

        public class EmailJaCadastradoException : Exception
        {
            public EmailJaCadastradoException(string email)
                : base($"O e-mail '{email}' já está cadastrado.") { }
        }

        public class TokenInvalidoOuExpiradoException : Exception
        {
            public TokenInvalidoOuExpiradoException()
                : base("O token de redefinição de senha é inválido ou expirou.") { }
        }
    }
}

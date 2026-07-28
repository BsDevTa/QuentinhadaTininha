namespace QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;

public class LoginResposta
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiraEm { get; set; }
    public UsuarioAutenticadoDto Usuario { get; set; } = null!;
}

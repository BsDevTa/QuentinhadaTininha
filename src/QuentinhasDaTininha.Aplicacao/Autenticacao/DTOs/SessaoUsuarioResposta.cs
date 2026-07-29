namespace QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;

public class SessaoUsuarioResposta
{
    public bool Autenticado { get; set; }
    public UsuarioAutenticadoDto Usuario { get; set; } = null!;
}

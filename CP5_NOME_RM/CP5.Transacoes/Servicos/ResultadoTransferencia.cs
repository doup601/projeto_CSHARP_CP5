namespace CP5.Transacoes.Servicos;

/// <summary>
/// Resultado da operação de transferência, usado pelo Program
/// para exibir o relatório e compor as evidências.
/// </summary>
public sealed class ResultadoTransferencia
{
    public bool Sucesso { get; }

    public bool CommitRealizado { get; }

    public bool RollbackRealizado { get; }

    public string Mensagem { get; }

    public string? DetalheErro { get; }

    private ResultadoTransferencia(
        bool sucesso, bool commitRealizado, bool rollbackRealizado, string mensagem, string? detalheErro)
    {
        Sucesso = sucesso;
        CommitRealizado = commitRealizado;
        RollbackRealizado = rollbackRealizado;
        Mensagem = mensagem;
        DetalheErro = detalheErro;
    }

    public static ResultadoTransferencia ComSucesso(string mensagem) =>
        new(sucesso: true, commitRealizado: true, rollbackRealizado: false, mensagem: mensagem, detalheErro: null);

    /// <summary>Validação impediu a operação antes da transação.</summary>
    public static ResultadoTransferencia Falha(string mensagem) =>
        new(sucesso: false, commitRealizado: false, rollbackRealizado: false, mensagem: mensagem, detalheErro: null);

    /// <summary>Falha dentro da transação; ROLLBACK executado com sucesso.</summary>
    public static ResultadoTransferencia ComRollback(string detalheErro) =>
        new(
            sucesso: false,
            commitRealizado: false,
            rollbackRealizado: true,
            mensagem: "Transferencia desfeita (ROLLBACK executado). Nenhuma alteracao parcial permaneceu no banco.",
            detalheErro: detalheErro);
}

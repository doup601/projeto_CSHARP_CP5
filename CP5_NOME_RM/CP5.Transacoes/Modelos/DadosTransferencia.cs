namespace CP5.Transacoes.Modelos;

/// <summary>Dados de entrada da operação de transferência.</summary>
public sealed class DadosTransferencia
{
    public string ContaOrigemNumero { get; init; } = string.Empty;

    public string ContaDestinoNumero { get; init; } = string.Empty;

    public decimal Valor { get; init; }

    /// <summary>
    /// Quando true, um erro deliberado é lançado depois do débito e
    /// antes do crédito, para demonstrar o ROLLBACK (D01, D03).
    /// </summary>
    public bool SimularErro { get; init; }
}

namespace CP5.Transacoes.Modelos;

/// <summary>Leitura de uma conta (tabela CONTA / Conta).</summary>
public sealed class ContaInfo
{
    public int Id { get; init; }

    public string Numero { get; init; } = string.Empty;

    public decimal Saldo { get; init; }

    public bool Ativa { get; init; }
}

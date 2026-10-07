namespace CP5.Transacoes.Modelos;

/// <summary>Leitura de um registro da tabela TRANSFERENCIA / Transferencia.</summary>
public sealed class TransferenciaInfo
{
    public int Id { get; init; }

    public int ContaOrigemId { get; init; }

    public int ContaDestinoId { get; init; }

    public decimal Valor { get; init; }

    public DateTime DataTransferencia { get; init; }

    public string? Observacao { get; init; }
}

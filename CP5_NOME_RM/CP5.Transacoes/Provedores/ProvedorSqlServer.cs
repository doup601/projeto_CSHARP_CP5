using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace CP5.Transacoes.Provedores;

/// <summary>
/// Provider de SQL Server (Microsoft.Data.SqlClient).
/// </summary>
public sealed class ProvedorSqlServer : IProvedorBanco
{
    private readonly string _connectionString;

    public ProvedorSqlServer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string Nome => "SQL Server";

    public string PrefixoParametro => "@";

    public DbConnection CriarConexao() => new SqlConnection(_connectionString);

    public DbCommand CriarComando(string sql, DbConnection conexao, DbTransaction? transacao)
    {
        var comando = new SqlCommand(sql, (SqlConnection)conexao);

        if (transacao is not null)
        {
            comando.Transaction = (SqlTransaction)transacao;
        }

        return comando;
    }

    public string SqlConsultarContaPorNumero => """
        SELECT Id, Numero, Saldo, Ativa
        FROM dbo.Conta
        WHERE Numero = @pNumero;
        """;

    public string SqlConsultarSaldos => """
        SELECT Id, Numero, Saldo, Ativa
        FROM dbo.Conta
        ORDER BY Numero;
        """;

    public string SqlConsultarTransferencias => """
        SELECT Id, ContaOrigemId, ContaDestinoId, Valor, DataTransferencia, Observacao
        FROM dbo.Transferencia
        ORDER BY Id;
        """;

    // Débito condicional e atômico: a checagem de saldo e o UPDATE
    // ocorrem na mesma instrução, dentro da transação.
    public string SqlDebitar => """
        UPDATE dbo.Conta
        SET Saldo = Saldo - @pValor
        WHERE Id = @pId
          AND Saldo >= @pValor;
        """;

    public string SqlCreditar => """
        UPDATE dbo.Conta
        SET Saldo = Saldo + @pValor
        WHERE Id = @pId
          AND Ativa = 1;
        """;

    // Id (IDENTITY) e DataTransferencia (DEFAULT SYSDATETIME)
    // são preenchidos pelo próprio banco, conforme script S01.
    public string SqlInserirTransferencia => """
        INSERT INTO dbo.Transferencia (ContaOrigemId, ContaDestinoId, Valor, Observacao)
        VALUES (@pContaOrigemId, @pContaDestinoId, @pValor, @pObservacao);
        """;
}

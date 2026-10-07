using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace CP5.Transacoes.Provedores;

/// <summary>
/// Provider de Oracle (Oracle.ManagedDataAccess.Core).
/// </summary>
public sealed class ProvedorOracle : IProvedorBanco
{
    private readonly string _connectionString;

    public ProvedorOracle(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string Nome => "Oracle";

    public string PrefixoParametro => ":";

    public DbConnection CriarConexao() => new OracleConnection(_connectionString);

    public DbCommand CriarComando(string sql, DbConnection conexao, DbTransaction? transacao)
    {
        // BindByName é essencial para os parâmetros nomeados (:pValor).
        var comando = new OracleCommand(sql, (OracleConnection)conexao)
        {
            BindByName = true
        };

        if (transacao is not null)
        {
            comando.Transaction = (OracleTransaction)transacao;
        }

        return comando;
    }

    public string SqlConsultarContaPorNumero => """
        SELECT ID, NUMERO, SALDO, ATIVA
        FROM CONTA
        WHERE NUMERO = :pNumero;
        """;

    public string SqlConsultarSaldos => """
        SELECT ID, NUMERO, SALDO, ATIVA
        FROM CONTA
        ORDER BY NUMERO;
        """;

    public string SqlConsultarTransferencias => """
        SELECT ID, CONTA_ORIGEM_ID, CONTA_DESTINO_ID, VALOR, DATA_TRANSFERENCIA, OBSERVACAO
        FROM TRANSFERENCIA
        ORDER BY ID;
        """;

    // Débito condicional e atômico: a checagem de saldo e o UPDATE
    // ocorrem na mesma instrução, dentro da transação.
    public string SqlDebitar => """
        UPDATE CONTA
        SET SALDO = SALDO - :pValor
        WHERE ID = :pId
          AND SALDO >= :pValor;
        """;

    public string SqlCreditar => """
        UPDATE CONTA
        SET SALDO = SALDO + :pValor
        WHERE ID = :pId
          AND ATIVA = 1;
        """;

    // ID (IDENTITY) e DATA_TRANSFERENCIA (DEFAULT SYSTIMESTAMP)
    // são preenchidos pelo próprio banco, conforme script S02.
    public string SqlInserirTransferencia => """
        INSERT INTO TRANSFERENCIA (CONTA_ORIGEM_ID, CONTA_DESTINO_ID, VALOR, OBSERVACAO)
        VALUES (:pContaOrigemId, :pContaDestinoId, :pValor, :pObservacao);
        """;
}

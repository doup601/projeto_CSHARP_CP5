using System.Data.Common;

namespace CP5.Transacoes.Provedores;

/// <summary>
/// Abstração do provider de banco: permite que a mesma lógica de
/// transferência execute, sem alterações, em SQL Server e em Oracle.
/// </summary>
public interface IProvedorBanco
{
    /// <summary>Nome de exibição do banco (relatórios e evidências).</summary>
    string Nome { get; }

    /// <summary>Prefixo de parâmetros do provider (@ no SQL Server, : no Oracle).</summary>
    string PrefixoParametro { get; }

    /// <summary>Cria uma conexão com o banco (ainda não aberta).</summary>
    DbConnection CriarConexao();

    /// <summary>
    /// Cria um comando associado à conexão e, quando informado,
    /// à mesma transação (requisito central do CP5).
    /// </summary>
    DbCommand CriarComando(string sql, DbConnection conexao, DbTransaction? transacao);

    /// <summary>Consulta uma conta pelo número (Id, Numero, Saldo, Ativa).</summary>
    string SqlConsultarContaPorNumero { get; }

    /// <summary>Lista todas as contas (Id, Numero, Saldo, Ativa).</summary>
    string SqlConsultarSaldos { get; }

    /// <summary>Lista o histórico de transferências.</summary>
    string SqlConsultarTransferencias { get; }

    /// <summary>
    /// Débito condicional: só atualiza se SALDO &gt;= valor.
    /// Retorna a quantidade de linhas afetadas.
    /// </summary>
    string SqlDebitar { get; }

    /// <summary>Crédito na conta de destino. Retorna a quantidade de linhas afetadas.</summary>
    string SqlCreditar { get; }

    /// <summary>Registra a transferência na tabela TRANSFERENCIA.</summary>
    string SqlInserirTransferencia { get; }
}

using System.Data.Common;
using CP5.Transacoes.Modelos;
using CP5.Transacoes.Provedores;

namespace CP5.Transacoes.Consultas;

/// <summary>
/// Consultas SOMENTE LEITURA, executadas em conexão própria (fora de
/// qualquer transação). Servem para exibir o estado REAL do banco antes
/// e depois da operação, alimentando as evidências de COMMIT e ROLLBACK.
/// </summary>
public static class ConsultasBanco
{
    /// <summary>
    /// Consulta uma conta pelo número. Retorna null quando a conta não existe.
    /// </summary>
    public static async Task<ContaInfo?> ConsultarContaAsync(
        DbConnection conexao, IProvedorBanco provedor, string numero, DbTransaction? transacao = null)
    {
        using var comando = provedor.CriarComando(provedor.SqlConsultarContaPorNumero, conexao, transacao);
        AdicionarParametro(comando, provedor, "pNumero", numero);

        await using var leitor = await comando.ExecuteReaderAsync();

        if (await leitor.ReadAsync())
        {
            // Ordinais fixos: 0=Id, 1=Numero, 2=Saldo, 3=Ativa
            return new ContaInfo
            {
                Id = Convert.ToInt32(leitor[0]),
                Numero = leitor[1].ToString() ?? string.Empty,
                Saldo = Convert.ToDecimal(leitor[2]),
                Ativa = Convert.ToBoolean(leitor[3])
            };
        }

        return null;
    }

    /// <summary>
    /// Lista todas as contas com seus saldos.
    /// </summary>
    public static async Task<IReadOnlyList<ContaInfo>> ConsultarSaldosAsync(
        DbConnection conexao, IProvedorBanco provedor)
    {
        using var comando = provedor.CriarComando(provedor.SqlConsultarSaldos, conexao, null);
        await using var leitor = await comando.ExecuteReaderAsync();

        var contas = new List<ContaInfo>();
        while (await leitor.ReadAsync())
        {
            contas.Add(new ContaInfo
            {
                Id = Convert.ToInt32(leitor[0]),
                Numero = leitor[1].ToString() ?? string.Empty,
                Saldo = Convert.ToDecimal(leitor[2]),
                Ativa = Convert.ToBoolean(leitor[3])
            });
        }

        return contas;
    }

    /// <summary>
    /// Lista o histórico de transferências registrado em TRANSFERENCIA.
    /// </summary>
    public static async Task<IReadOnlyList<TransferenciaInfo>> ConsultarTransferenciasAsync(
        DbConnection conexao, IProvedorBanco provedor)
    {
        using var comando = provedor.CriarComando(provedor.SqlConsultarTransferencias, conexao, null);
        await using var leitor = await comando.ExecuteReaderAsync();

        var transferencias = new List<TransferenciaInfo>();
        while (await leitor.ReadAsync())
        {
            // Ordinais fixos: 0=Id, 1=ContaOrigemId, 2=ContaDestinoId,
            // 3=Valor, 4=DataTransferencia, 5=Observacao
            transferencias.Add(new TransferenciaInfo
            {
                Id = Convert.ToInt32(leitor[0]),
                ContaOrigemId = Convert.ToInt32(leitor[1]),
                ContaDestinoId = Convert.ToInt32(leitor[2]),
                Valor = Convert.ToDecimal(leitor[3]),
                DataTransferencia = Convert.ToDateTime(leitor[4]),
                Observacao = leitor[5] as string
            });
        }

        return transferencias;
    }

    private static void AdicionarParametro(
        DbCommand comando, IProvedorBanco provedor, string nome, object valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = provedor.PrefixoParametro + nome;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }
}

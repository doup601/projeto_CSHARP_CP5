using System.Data.Common;
using System.Globalization;
using CP5.Transacoes.Consultas;
using CP5.Transacoes.Modelos;
using CP5.Transacoes.Provedores;

namespace CP5.Transacoes.Servicos;

/// <summary>
/// Serviço central do CP5: executa a transferência entre contas como uma
/// UNICA transação local. Débito, crédito e o registro em TRANSFERENCIA
/// participam da mesma conexão e da mesma transação (D01, D02, D03).
/// </summary>
public sealed class TransferenciaService
{
    private readonly IProvedorBanco _provedor;

    public TransferenciaService(IProvedorBanco provedor)
    {
        _provedor = provedor;
    }

    public async Task<ResultadoTransferencia> ExecutarTransferenciaAsync(DadosTransferencia dados)
    {
        // ==================================================================
        // VALIDAÇÕES PRÉ-TRANSAÇÃO (somente leitura; não precisam estar
        // dentro da transação - D02, critério 6)
        // ==================================================================

        if (dados.Valor <= 0)
        {
            return ResultadoTransferencia.Falha("O valor da transferencia deve ser maior que zero.");
        }

        if (string.Equals(dados.ContaOrigemNumero, dados.ContaDestinoNumero, StringComparison.OrdinalIgnoreCase))
        {
            return ResultadoTransferencia.Falha("A conta de origem e a conta de destino devem ser diferentes.");
        }

        ContaInfo? contaOrigem;
        ContaInfo? contaDestino;

        // Leitura em conexão separada, antes da transação.
        await using (var conexaoLeitura = _provedor.CriarConexao())
        {
            await conexaoLeitura.OpenAsync();

            contaOrigem = await ConsultasBanco.ConsultarContaAsync(
                conexaoLeitura, _provedor, dados.ContaOrigemNumero);

            contaDestino = await ConsultasBanco.ConsultarContaAsync(
                conexaoLeitura, _provedor, dados.ContaDestinoNumero);
        }

        if (contaOrigem is null)
        {
            return ResultadoTransferencia.Falha($"Conta de origem '{dados.ContaOrigemNumero}' nao encontrada.");
        }

        if (contaDestino is null)
        {
            return ResultadoTransferencia.Falha($"Conta de destino '{dados.ContaDestinoNumero}' nao encontrada.");
        }

        if (!contaOrigem.Ativa || !contaDestino.Ativa)
        {
            return ResultadoTransferencia.Falha("A transferencia exige que ambas as contas estejam ativas.");
        }

        // Pré-verificação de saldo: apenas para devolver uma mensagem
        // amigável ANTES de abrir a transação. A garantia real é o
        // UPDATE condicional do passo [2], que avalia o saldo no
        // próprio UPDATE, dentro da transação.
        if (contaOrigem.Saldo < dados.Valor)
        {
            return ResultadoTransferencia.Falha(
                $"Saldo insuficiente na conta de origem {contaOrigem.Numero}: " +
                $"disponivel {Moeda(contaOrigem.Saldo)}, necessario {Moeda(dados.Valor)}.");
        }

        // ==================================================================
        // TRANSAÇÃO LOCAL: todas as etapas abaixo usam a MESMA conexão
        // e a MESMA transação. Sucesso -> COMMIT. Qualquer falha -> ROLLBACK.
        // ==================================================================

        await using var conexao = _provedor.CriarConexao();
        await conexao.OpenAsync();

        // [1] INÍCIO DA TRANSAÇÃO
        using var transacao = conexao.BeginTransaction();

        try
        {
            // [2] DÉBITO NA CONTA DE ORIGEM
            // UPDATE condicional e atômico: só debita se SALDO >= VALOR.
            // A verificação e a alteração ocorrem na mesma instrução, sob a
            // transação - não há janela de corrida (evita SELECT-then-UPDATE).
            var linhasDebitadas = await ExecutarDebitoAsync(conexao, transacao, contaOrigem.Id, dados.Valor);

            if (linhasDebitadas != 1)
            {
                throw new InvalidOperationException(
                    "Saldo insuficiente no momento do debito (verificacao condicional dentro da transacao).");
            }

            // [3] ERRO DELIBERADO (somente quando solicitado)
            // Provocado DEPOIS do débito e ANTES do crédito, para demonstrar
            // que o ROLLBACK desfaz a alteração parcial (D01, D03).
            if (dados.SimularErro)
            {
                throw new InvalidOperationException(
                    "Erro deliberado para testar ROLLBACK (simulacao prevista pelo CP5).");
            }

            // [4] CRÉDITO NA CONTA DE DESTINO
            var linhasCreditadas = await ExecutarCreditoAsync(conexao, transacao, contaDestino.Id, dados.Valor);

            if (linhasCreditadas != 1)
            {
                throw new InvalidOperationException(
                    "Nao foi possivel creditar a conta de destino (conta inexistente ou inativa).");
            }

            // [5] REGISTRO DA TRANSFERÊNCIA (na mesma transação)
            await RegistrarTransferenciaAsync(conexao, transacao, contaOrigem.Id, contaDestino.Id, dados.Valor);

            // [6] COMMIT - somente após débito + crédito + registro
            transacao.Commit();

            return ResultadoTransferencia.ComSucesso(
                $"Transferencia de {Moeda(dados.Valor)} de {contaOrigem.Numero} para {contaDestino.Numero} " +
                "confirmada com COMMIT.");
        }
        catch (Exception excecao)
        {
            // [7] ROLLBACK - desfaz TODAS as alterações da transação,
            // restaurando saldos e histórico ao estado anterior.
            try
            {
                transacao.Rollback();
            }
            catch (Exception erroNoRollback)
            {
                return ResultadoTransferencia.Falha(
                    $"Falha durante o ROLLBACK: {erroNoRollback.Message} " +
                    $"(erro original: {excecao.Message})");
            }

            return ResultadoTransferencia.ComRollback(excecao.Message);
        }
    }

    private async Task<int> ExecutarDebitoAsync(
        DbConnection conexao, DbTransaction transacao, int contaId, decimal valor)
    {
        using var comando = _provedor.CriarComando(_provedor.SqlDebitar, conexao, transacao);
        AdicionarParametro(comando, "pId", contaId);
        AdicionarParametro(comando, "pValor", valor);
        return await comando.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecutarCreditoAsync(
        DbConnection conexao, DbTransaction transacao, int contaId, decimal valor)
    {
        using var comando = _provedor.CriarComando(_provedor.SqlCreditar, conexao, transacao);
        AdicionarParametro(comando, "pId", contaId);
        AdicionarParametro(comando, "pValor", valor);
        return await comando.ExecuteNonQueryAsync();
    }

    private async Task RegistrarTransferenciaAsync(
        DbConnection conexao, DbTransaction transacao, int contaOrigemId, int contaDestinoId, decimal valor)
    {
        using var comando = _provedor.CriarComando(_provedor.SqlInserirTransferencia, conexao, transacao);
        AdicionarParametro(comando, "pContaOrigemId", contaOrigemId);
        AdicionarParametro(comando, "pContaDestinoId", contaDestinoId);
        AdicionarParametro(comando, "pValor", valor);
        AdicionarParametro(comando, "pObservacao", "CP5 - transferencia registrada pela aplicacao");
        await comando.ExecuteNonQueryAsync();
    }

    private void AdicionarParametro(DbCommand comando, string nome, object valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = _provedor.PrefixoParametro + nome;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }

    private static string Moeda(decimal valor) => $"R$ {valor.ToString("N2", CultureInfo.InvariantCulture)}";
}

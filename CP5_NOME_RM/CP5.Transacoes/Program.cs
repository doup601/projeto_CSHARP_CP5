using System.Globalization;
using CP5.Transacoes.Config;
using CP5.Transacoes.Consultas;
using CP5.Transacoes.Modelos;
using CP5.Transacoes.Provedores;
using CP5.Transacoes.Servicos;
using Microsoft.Extensions.Configuration;

namespace CP5.Transacoes;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("==========================================================");
        Console.WriteLine("  CP5 - Transacoes Locais em C# (ADO.NET)");
        Console.WriteLine("  SQL Server + Oracle | COMMIT / ROLLBACK | 3ESPH-2026");
        Console.WriteLine("==========================================================");

        try
        {
            var configuracao = Configuracoes.Carregar();
            var provedor = EscolherProvedor(configuracao);
            var servico = new TransferenciaService(provedor);

            Console.WriteLine();
            Console.WriteLine($"Banco selecionado: {provedor.Nome}");

            bool continuar = true;

            while (continuar)
            {
                var dados = LerDadosTransferencia();

                // Estado do banco ANTES - leitura somente, conexao separada.
                await ExibirEstadoDoBancoAsync(provedor, "ANTES");

                var resultado = await servico.ExecutarTransferenciaAsync(dados);

                ExibirResultado(resultado);

                // Estado do banco DEPOIS - comprova o resultado no banco real.
                await ExibirEstadoDoBancoAsync(provedor, "DEPOIS");

                if (resultado.RollbackRealizado)
                {
                    Console.WriteLine();
                    Console.WriteLine("Comprovacao exigida pelo CP5: confirme no SGBD (SSMS / DBeaver /");
                    Console.WriteLine("Valentina) que os saldos voltaram ao estado anterior e que");
                    Console.WriteLine("NENHUM registro parcial permaneceu em TRANSFERENCIA.");
                }

                continuar = PerguntarSimNao("Executar outra operacao? (S/N): ");
            }

            Console.WriteLine();
            Console.WriteLine("Encerrando o CP5. Ate logo!");
        }
        catch (Exception excecao)
        {
            Console.WriteLine();
            Console.WriteLine("A operacao nao pode ser concluida:");
            Console.WriteLine(excecao.Message);
            Console.WriteLine();
            Console.WriteLine("Verifique: servidor do banco no ar; connection string correta;");
            Console.WriteLine("database/schema do CP5 criado (scripts S01/S02); permissoes do usuario.");
        }
    }

    private static IProvedorBanco EscolherProvedor(IConfigurationRoot configuracao)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Selecione o banco de dados para esta operacao:");
            Console.WriteLine("  1 - SQL Server");
            Console.WriteLine("  2 - Oracle");
            Console.Write("Opcao: ");
            var opcao = (Console.ReadLine() ?? string.Empty).Trim();

            if (opcao == "1")
            {
                return new ProvedorSqlServer(ObterConnectionString(configuracao, "SqlServer"));
            }

            if (opcao == "2")
            {
                return new ProvedorOracle(ObterConnectionString(configuracao, "Oracle"));
            }

            Console.WriteLine("Opcao invalida. Digite 1 ou 2.");
        }
    }

    private static string ObterConnectionString(IConfigurationRoot configuracao, string chave)
    {
        var valor = configuracao.GetConnectionString(chave);

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException(
                $"Connection string '{chave}' nao encontrada. Configure em appsettings.json " +
                $"ou na variavel de ambiente ConnectionStrings__{chave}.");
        }

        return valor;
    }

    private static DadosTransferencia LerDadosTransferencia()
    {
        Console.WriteLine();
        Console.WriteLine("--- Dados da transferencia ---");

        Console.Write("Conta de origem (ex.: CC-1001): ");
        var origem = (Console.ReadLine() ?? string.Empty).Trim();

        Console.Write("Conta de destino (ex.: CC-1002): ");
        var destino = (Console.ReadLine() ?? string.Empty).Trim();

        var valor = LerValor();

        Console.Write("Simular erro deliberado para testar o ROLLBACK? (S/N): ");
        var resposta = (Console.ReadLine() ?? string.Empty).Trim();
        var simularErro = resposta.Equals("S", StringComparison.OrdinalIgnoreCase)
                          || resposta.Equals("SIM", StringComparison.OrdinalIgnoreCase);

        return new DadosTransferencia
        {
            ContaOrigemNumero = origem,
            ContaDestinoNumero = destino,
            Valor = valor,
            SimularErro = simularErro
        };
    }

    private static decimal LerValor()
    {
        while (true)
        {
            Console.Write("Valor da transferencia (R$): ");
            var texto = (Console.ReadLine() ?? string.Empty).Trim().Replace(',', '.');

            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
                && valor > 0)
            {
                return valor;
            }

            Console.WriteLine("  Valor invalido. Use um numero maior que zero (ex.: 200 ou 200.50).");
        }
    }

    private static async Task ExibirEstadoDoBancoAsync(IProvedorBanco provedor, string momento)
    {
        Console.WriteLine();
        Console.WriteLine($"=== Estado do banco ({momento}) - {provedor.Nome} ===");

        await using var conexao = provedor.CriarConexao();
        await conexao.OpenAsync();

        var contas = await ConsultasBanco.ConsultarSaldosAsync(conexao, provedor);
        var total = contas.Sum(conta => conta.Saldo);

        Console.WriteLine("Contas:");
        foreach (var conta in contas)
        {
            Console.WriteLine($"  {conta.Numero,-10} Saldo: {Moeda(conta.Saldo)}");
        }

        Console.WriteLine($"  Total dos saldos: {Moeda(total)}");

        var transferencias = await ConsultasBanco.ConsultarTransferenciasAsync(conexao, provedor);

        Console.WriteLine($"Registros em TRANSFERENCIA: {transferencias.Count}");

        foreach (var transferencia in transferencias)
        {
            Console.WriteLine(
                $"  #{transferencia.Id} | origemId={transferencia.ContaOrigemId} " +
                $"destinoId={transferencia.ContaDestinoId} | valor={Moeda(transferencia.Valor)} | " +
                $"data={transferencia.DataTransferencia:dd/MM/yyyy HH:mm:ss} | " +
                $"obs={transferencia.Observacao ?? "-"}");
        }
    }

    private static void ExibirResultado(ResultadoTransferencia resultado)
    {
        Console.WriteLine();
        Console.WriteLine("=== Resultado ===");

        if (resultado.CommitRealizado)
        {
            Console.WriteLine("COMMIT realizado com sucesso.");
            Console.WriteLine(resultado.Mensagem);
        }
        else if (resultado.RollbackRealizado)
        {
            Console.WriteLine("ROLLBACK realizado com sucesso.");
            Console.WriteLine(resultado.Mensagem);
            Console.WriteLine($"Detalhe do erro controlado: {resultado.DetalheErro}");
        }
        else
        {
            Console.WriteLine("A transferencia NAO foi executada (validacao impediu a operacao).");
            Console.WriteLine(resultado.Mensagem);
        }
    }

    private static bool PerguntarSimNao(string pergunta)
    {
        while (true)
        {
            Console.Write(pergunta);
            var resposta = (Console.ReadLine() ?? string.Empty).Trim();

            if (resposta.Equals("S", StringComparison.OrdinalIgnoreCase)
                || resposta.Equals("SIM", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (resposta.Equals("N", StringComparison.OrdinalIgnoreCase)
                || resposta.Equals("NAO", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("  Responda S ou N.");
        }
    }

    private static string Moeda(decimal valor) => $"R$ {valor.ToString("N2", CultureInfo.InvariantCulture)}";
}

using Microsoft.Extensions.Configuration;

namespace CP5.Transacoes.Config;

/// <summary>
/// Carrega a configuracao da aplicacao. Prioridade:
/// 1) variaveis de ambiente (ConnectionStrings__SqlServer /
///    ConnectionStrings__Oracle) - nao versionadas, seguras;
/// 2) appsettings.json (contem apenas marcadores de placeholder).
/// </summary>
public static class Configuracoes
{
    public static IConfigurationRoot Carregar()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }
}

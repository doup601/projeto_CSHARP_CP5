# CP5 — C# Software Development — 3ESPH-2026

## 1. Integrantes

| Nome | RM |
|---|---|
| Pedro Gaspar Fernandes Ferrari | RM 554887 |
| Enrico Ricarte Rodrigues | RM 558571 |
| Victor Freire | RM 556191 |

**Turma:** 3ESPH  
**Checkpoint:** CP5  
**Arquivo de entrega:** `CP5_<NOME>_RM<RM>.zip` — use o nome e o RM do integrante que fará o upload. Ex.: se Pedro entregar: `CP5_Pedro_Gaspar_Fernandes_Ferrari_RM554887.zip`

> Um integrante realiza o upload; todos os integrantes acima estão identificados neste README.

## 2. Objetivo

Aplicação console em C# que implementa uma **transferência entre contas** utilizando **transação local** em ADO.NET: débito da conta de origem, crédito da conta de destino e registro na tabela `TRANSFERENCIA` participam da **mesma unidade transacional**. Em caso de sucesso ocorre `COMMIT`; em caso de falha deliberada, `ROLLBACK`, sem saldo parcial e sem histórico parcial. A mesma operação é executada, em momentos independentes, em **SQL Server** e em **Oracle** (cada execução usa apenas um banco; não há transação distribuída).

## 3. Tecnologia utilizada

- Linguagem: C#
- Framework/versão .NET: **net10.0** (SDK 10.0.301 disponível no ambiente; o enunciado recomenda .NET 9, mas o trata como recomendação, não exigência — .NET 10 é compatível e atende ao requisito)
- Tecnologia de acesso a dados: **ADO.NET** — `Microsoft.Data.SqlClient` (SQL Server) e `Oracle.ManagedDataAccess.Core` (Oracle)
- SQL Server: <informar versão/ambiente utilizado>
- Oracle: <informar versão/ambiente utilizado, ex.: Oracle XE 21c local — PDB XEPDB1>

> A tecnologia de acesso a dados é livre no CP5. O requisito é demonstrar corretamente a transação.

## 4. Estrutura do banco

Base/schema sugerido para demonstração: `CP5_RM554887` (identificador simples baseado no RM do primeiro integrante; cada grupo pode adotar o RM de quem configura o banco).

Modelo de referência (scripts oficiais do D04):

- `CLIENTE` (Id, Nome, Documento, Ativo)
- `TIPO_CONTA` (Id, Descricao)
- `CONTA` (Id, ClienteId, TipoContaId, Numero, Saldo, Ativa)
- `TRANSFERENCIA` (Id, ContaOrigemId, ContaDestinoId, Valor, DataTransferencia, Observacao)

No SQL Server, informar o nome da database utilizada.  
No Oracle, informar o service/PDB e o schema utilizado. **Não informar senha real.**

Dados iniciais de referência: `CC-1001 = R$ 1.000,00`, `CC-1002 = R$ 500,00`, `CP-2001 = R$ 1.500,00`.

## 5. Como preparar o SQL Server

1. Criar/selecionar uma database própria para o CP5 (ex.: `CP5_RM554887`).
2. Confirmar que **não** está conectado ao `master`.
3. Executar `scripts/S01_SQLServer_Estrutura_Dados.sql`.
4. Usar `scripts/S03_SQLServer_Testes_Commit_Rollback.sql` quando necessário para validação manual (contém COMMIT, ROLLBACK e bloco de reset).

## 6. Como preparar o Oracle

No Oracle XE local:

1. Utilizar o `XEPDB1`.
2. Criar/usar um schema próprio para o CP5 (ex.: `CP5_RM554887`).
3. **Somente em Oracle XE local e com permissão administrativa**, o script opcional `scripts/S00_Oracle_XE_Local_Preparar_Usuario_CP5.sql` mostra a preparação usada no laboratório (CREATE SESSION, CREATE TABLE, CREATE SEQUENCE e quota no USERS).
4. Executar `scripts/S02_Oracle_Estrutura_Dados.sql` conectado ao schema do CP5.
5. Usar `scripts/S04_Oracle_Testes_Commit_Rollback.sql` para validação manual, mantendo o **Auto-commit desativado** durante o teste manual de `ROLLBACK`.

> Em servidor institucional, não presumir acesso `SYSTEM`; utilizar as credenciais e permissões fornecidas pela instituição e **não executar o S00 sem autorização**.

## 7. Como executar a aplicação

```text
1. Abrir o terminal na pasta CP5_NOME_RM/CP5.Transacoes.
2. Restaurar pacotes:      dotnet restore
3. Ajustar as connection strings localmente (veja a seção 11).
4. Executar:               dotnet run
5. Selecionar 1 (SQL Server) ou 2 (Oracle).
6. Informar conta de origem, conta de destino, valor
   e se deseja simular erro deliberado (S/N).
```

A aplicação exibe o estado do banco **ANTES** e **DEPOIS** da operação (saldos, total e histórico de `TRANSFERENCIA`), consultando o banco em uma conexão separada e somente leitura.

## 8. Teste de COMMIT

```text
Conta origem: CC-1001
Conta destino: CC-1002
Valor: 200
Simular erro: N
```

Resultado esperado: `CC-1001 = R$ 800,00`, `CC-1002 = R$ 700,00`, total dos saldos preservado (`R$ 1.500,00` no par; `R$ 3.000,00` na base completa) e **1 novo registro** em `TRANSFERENCIA`, confirmados pelo `COMMIT`.

## 9. Teste de ROLLBACK

Explique como provocar deliberadamente uma falha **depois do início da transação e depois de pelo menos uma alteração**: na aplicação, responda **S** à pergunta "Simular erro deliberado para testar o ROLLBACK?". O erro é lançado **após o débito e antes do crédito**, o que é a exigência do enunciado (erro proposital e controlado — erros acidentais de conexão/sintaxe não servem como demonstração).

Antes do teste, reinicialize os dados (reexecute `S01`/`S02`, ou o bloco de reset de `S03`/`S04`).

Resultado esperado: `CC-1001 = R$ 1.000,00`, `CC-1002 = R$ 500,00` e **nenhum registro parcial** em `TRANSFERENCIA`.

> Comprovação: além da saída da aplicação, confirme o estado real do banco com consultas diretas no SSMS / DBeaver / Valentina (no Oracle, mantenha o **Auto-commit desativado**).

## 10. Evidências

Relacionar as seis evidências entregues (pastas `evidencias/`):

1. **E01** — Estrutura da solução (projeto C#, pasta `scripts/`, organização da entrega).
2. **E02** — SQL Server / COMMIT (saldos finais corretos e registro em `TRANSFERENCIA`).
3. **E03** — SQL Server / ROLLBACK (falha simulada e comprovação de que nada parcial permaneceu).
4. **E04** — Oracle / COMMIT.
5. **E05** — Oracle / ROLLBACK.
6. **E06** — Código da transação (`CP5.Transacoes/Servicos/TransferenciaService.cs`).

Imagens devem ser legíveis. Nomes sugeridos (não obrigatórios): `01_estrutura.png`, `02_sqlserver_commit.png`, `03_sqlserver_rollback.png`, `04_oracle_commit.png`, `05_oracle_rollback.png`, `06_codigo_transacao.png`.

## 11. Segurança

- **Não** incluir senha institucional real nem credenciais pessoais.
- O `appsettings.json` entregue contém apenas marcadores (`SEU_USUARIO` / `SUA_SENHA_AQUI`).
- Para executar localmente, use variáveis de ambiente (prioridade sobre o arquivo):
  - `ConnectionStrings__SqlServer`
  - `ConnectionStrings__Oracle`
- Alternativas: User Secrets (`dotnet user-secrets`) ou configuração local não versionada.
- Credenciais artificiais criadas especificamente para um Oracle XE **local de laboratório** podem aparecer no material, desde que claramente identificadas como credenciais do ambiente local.

## 12. Observações finais

- A transação é **local**: cada execução trabalha com **apenas um** banco de cada vez; SQL Server e Oracle nunca participam da mesma transação.
- **Validação de saldo em duas camadas:** (a) pré-transação, leitura em conexão separada, para mensagem amigável antecipada; (b) dentro da transação, o débito é um **UPDATE condicional atômico** (`SET Saldo = Saldo - :valor WHERE Id = :id AND Saldo >= :valor`) — a verificação e a alteração ocorrem na mesma instrução, eliminando a janela de corrida de um padrão inseguro `SELECT saldo → UPDATE`. As constraints do D04 (`CK_Conta_Saldo`, `CK_Transferencia_Valor`) são a rede de segurança final.
- O débito, o crédito e o `INSERT` em `TRANSFERENCIA` usam a **mesma conexão e a mesma transação**; todos os comandos são parametrizados (evita SQL injection e problemas de cultura/formato).
- O `Id` (IDENTITY) e a data de transferência (DEFAULT) são preenchidos pelo próprio banco, conforme os scripts oficiais.
- Limitação: os testes reais em SQL Server e Oracle dependem dos ambientes de cada grupo; as evidências E02–E05 só devem ser produzidas após execuções reais.

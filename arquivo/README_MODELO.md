# CP5 — C# Software Development — 3ESPH-2026

## 1. Integrantes

| Nome | RM |
|---|---|
| NOME DO INTEGRANTE 1 | RM000000 |
| NOME DO INTEGRANTE 2 | RM000000 |

**Turma:** 3ESPH  
**Checkpoint:** CP5  
**Arquivo de entrega:** `CP5_NOME_RM.zip`

## 2. Objetivo

Descreva brevemente a aplicação e explique que ela implementa uma transferência entre contas utilizando **transação local**, garantindo `COMMIT` em caso de sucesso e `ROLLBACK` em caso de falha.

## 3. Tecnologia utilizada

- Linguagem: C#
- Framework/versão .NET: INFORMAR (ambiente de referência recomendado: .NET 9)
- Tecnologia de acesso a dados: ADO.NET / Entity Framework Core / outra equivalente
- SQL Server: versão/ambiente utilizado
- Oracle: versão/ambiente utilizado

> A tecnologia de acesso a dados é livre. O requisito é demonstrar corretamente a transação.

## 4. Estrutura do banco

Base/schema sugerido para demonstração: `CP5_RM999999`.

Modelo de referência:

- `CLIENTE`
- `TIPO_CONTA`
- `CONTA`
- `TRANSFERENCIA`

No SQL Server, informar o nome da database utilizada.  
No Oracle, informar o service/PDB e o schema utilizado. **Não informar senha real.**

## 5. Como preparar o SQL Server

1. Criar/selecionar uma database própria para o CP5.
2. Confirmar que não está executando no `master`.
3. Executar `S01_SQLServer_Estrutura_Dados.sql`.
4. Usar `S03_SQLServer_Testes_Commit_Rollback.sql` quando necessário para validação manual.

## 6. Como preparar o Oracle

No Oracle XE local:

1. Utilizar o `XEPDB1`.
2. Criar/usar um schema próprio para o CP5.
3. **Somente em Oracle XE local e com permissão administrativa**, o script opcional `S00_Oracle_XE_Local_Preparar_Usuario_CP5.sql` mostra a preparação utilizada no laboratório.
4. Executar `S02_Oracle_Estrutura_Dados.sql` conectado ao schema do CP5.
5. Usar `S04_Oracle_Testes_Commit_Rollback.sql` para validação manual, mantendo o **Auto-commit desativado** durante o teste manual de `ROLLBACK`.

> Em servidor institucional, não presumir acesso `SYSTEM`; utilizar as credenciais e permissões fornecidas pela instituição e **não executar o S00 sem autorização**.

## 7. Como executar a aplicação

Descreva os passos necessários para compilar e executar o projeto.

Exemplo:

```text
1. Abrir a solução no Visual Studio / VS Code.
2. Ajustar as connection strings localmente.
3. Selecionar SQL Server ou Oracle.
4. Executar a aplicação.
```

## 8. Teste de COMMIT

Informe os dados usados no teste e o resultado esperado.

Exemplo:

```text
Conta origem: CC-1001
Conta destino: CC-1002
Valor: 200,00
Simular erro: NÃO
```

Resultado esperado: débito, crédito e registro obrigatório em `TRANSFERENCIA` confirmados pelo `COMMIT`.

## 9. Teste de ROLLBACK

Explique como provocar deliberadamente uma falha **depois do início da transação e depois de pelo menos uma alteração**.

Resultado esperado: nenhum saldo parcial e nenhum histórico parcial permanecem após o `ROLLBACK`.

## 10. Evidências

Relacionar as seis evidências entregues:

1. Estrutura da solução.
2. SQL Server — COMMIT.
3. SQL Server — ROLLBACK.
4. Oracle — COMMIT.
5. Oracle — ROLLBACK.
6. Código da transação.

## 11. Segurança

- Não incluir senha institucional real.
- Não publicar credenciais pessoais.
- Utilizar marcadores como `SUA_SENHA_AQUI`, User Secrets, variáveis de ambiente ou configuração local não versionada.

## 12. Observações finais

Registre aqui alguma limitação ou decisão técnica importante da implementação.

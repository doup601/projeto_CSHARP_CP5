D04 — SCRIPTS OFICIAIS DO CP5
================================

Convenção:
S00, S01, S02... = ordem didática dos scripts.

S00_Oracle_XE_Local_Preparar_Usuario_CP5.sql
    OPCIONAL. Somente para Oracle XE local quando houver acesso administrativo.
    Não executar em ambiente institucional sem autorização.

S01_SQLServer_Estrutura_Dados.sql
    Executar conectado à database criada para o CP5.

S02_Oracle_Estrutura_Dados.sql
    Executar conectado ao schema criado/fornecido para o CP5.

S03_SQLServer_Testes_Commit_Rollback.sql
    Demonstra COMMIT e ROLLBACK no SQL Server.

S04_Oracle_Testes_Commit_Rollback.sql
    Demonstra COMMIT e ROLLBACK no Oracle.
    Para teste manual de ROLLBACK no DBeaver/Valentina, desative o Auto-commit.

ORACLE XE 21c LOCAL VALIDADO
-----------------------------
No ambiente local utilizado na validação deste material foram necessários:
CREATE SESSION
CREATE TABLE
CREATE SEQUENCE
QUOTA 100M ON USERS

O CREATE SEQUENCE é documentado aqui como requisito observado no ambiente
Oracle XE 21c validado para a estratégia IDENTITY usada pelo material; não
deve ser interpretado como regra universal para todo ambiente Oracle.

Em ambiente institucional, use as credenciais e permissões fornecidas pela
instituição e não tente executar o S00 como SYSTEM sem autorização.

MODELO
------
CLIENTE 1 ----- N CONTA N ----- 1 TIPO_CONTA
                     |
                     +----- N TRANSFERENCIA
                         (origem / destino)

OPERAÇÃO TRANSACIONAL OBRIGATÓRIA
----------------------------------
1. validar contas/saldo;
2. debitar origem;
3. creditar destino;
4. registrar em TRANSFERENCIA;
5. COMMIT no sucesso ou ROLLBACK na falha.

Sugestão de database/schema:
CP5_RM999999

Arquivo de entrega:
CP5_NOME_RM.zip

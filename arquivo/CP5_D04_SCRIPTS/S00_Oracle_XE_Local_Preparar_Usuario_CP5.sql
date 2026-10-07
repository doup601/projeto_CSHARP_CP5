-- ============================================================================
-- CP5 - ORACLE XE LOCAL - PREPARAÇÃO DO USUÁRIO/SCHEMA
-- SCRIPT OPCIONAL - SOMENTE PARA AMBIENTE ORACLE XE LOCAL
-- ============================================================================
-- NÃO EXECUTE ESTE SCRIPT EM AMBIENTE INSTITUCIONAL SEM AUTORIZAÇÃO.
-- Em ambiente institucional, utilize o usuário/schema e as permissões
-- fornecidos pela instituição.
--
-- Para o laboratório local validado neste CP5:
--   PDB/Service: XEPDB1
--   exemplo de usuário/schema: CP5_RM999999
--
-- Execute como SYSTEM conectado ao XEPDB1.
-- ============================================================================

SELECT
    USER AS USUARIO_ATUAL,
    SYS_CONTEXT('USERENV', 'CON_NAME') AS CONTAINER
FROM DUAL;

-- PASSO 1 - VERIFIQUE SE O USUÁRIO JÁ EXISTE
SELECT USERNAME, ACCOUNT_STATUS, DEFAULT_TABLESPACE
FROM DBA_USERS
WHERE USERNAME = 'CP5_RM999999';

-- PASSO 2 - SOMENTE SE O USUÁRIO AINDA NÃO EXISTIR
-- Ajuste o nome e escolha uma senha LOCAL própria antes de executar.
-- NÃO use senha institucional real em arquivos compartilhados.
--
-- CREATE USER CP5_RM999999
-- IDENTIFIED BY "SUA_SENHA_LOCAL_AQUI"
-- DEFAULT TABLESPACE USERS
-- TEMPORARY TABLESPACE TEMP
-- QUOTA 100M ON USERS;

-- PASSO 3 - APÓS CONFIRMAR QUE O USUÁRIO EXISTE
-- No Oracle XE 21c local validado para este material, estes privilégios
-- foram necessários para o modelo utilizado, inclusive as colunas IDENTITY.
GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE
TO CP5_RM999999;

ALTER USER CP5_RM999999
QUOTA 100M ON USERS;

-- PASSO 4 - CONFERÊNCIA
SELECT
    GRANTEE,
    PRIVILEGE
FROM DBA_SYS_PRIVS
WHERE GRANTEE = 'CP5_RM999999'
ORDER BY PRIVILEGE;

SELECT
    USERNAME,
    TABLESPACE_NAME,
    BYTES,
    MAX_BYTES
FROM DBA_TS_QUOTAS
WHERE USERNAME = 'CP5_RM999999';

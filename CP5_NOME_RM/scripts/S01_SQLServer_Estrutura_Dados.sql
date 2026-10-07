-- ============================================================================
-- D04 - CP5 - C# SOFTWARE DEVELOPMENT - FIAP 2026.2 - 3ESPH
-- SCRIPT OFICIAL DE REFERÊNCIA - SQL SERVER
--
-- MODELO:
--   CLIENTE
--   TIPO_CONTA
--   CONTA
--   TRANSFERENCIA
--
-- IMPORTANTE
-- 1) Crie previamente uma base própria para o CP5.
--    Exemplo de nome: CP5_RM999999
-- 2) Selecione essa base ANTES de executar este script.
-- 3) NÃO execute este script no banco master.
-- 4) Este script NÃO cria a database.
-- 5) Stored Procedures, Triggers e transações distribuídas estão fora do CP5.
-- ============================================================================

-- Exemplo (ajuste para a SUA base):
-- USE CP5_RM999999;
-- GO

SELECT DB_NAME() AS BancoAtual;
GO

-- Proteção simples: interrompe se o usuário esquecer no master.
IF DB_NAME() = 'master'
BEGIN
    THROW 51000, 'ERRO: selecione a base do CP5 antes de executar este script. Nao execute no master.', 1;
END;
GO

-- ============================================================================
-- 1. LIMPEZA CONTROLADA
--    Remove apenas os objetos deste exercício, respeitando as FKs.
-- ============================================================================
IF OBJECT_ID('dbo.Transferencia', 'U') IS NOT NULL
    DROP TABLE dbo.Transferencia;

IF OBJECT_ID('dbo.Conta', 'U') IS NOT NULL
    DROP TABLE dbo.Conta;

IF OBJECT_ID('dbo.TipoConta', 'U') IS NOT NULL
    DROP TABLE dbo.TipoConta;

IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL
    DROP TABLE dbo.Cliente;
GO

-- ============================================================================
-- 2. CLIENTE
-- ============================================================================
CREATE TABLE dbo.Cliente
(
    Id          INT IDENTITY(1,1) NOT NULL,
    Nome        VARCHAR(100) NOT NULL,
    Documento   VARCHAR(20) NOT NULL,
    Ativo       BIT NOT NULL
        CONSTRAINT DF_Cliente_Ativo DEFAULT 1,

    CONSTRAINT PK_Cliente
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Cliente_Documento
        UNIQUE (Documento)
);
GO

-- ============================================================================
-- 3. TIPO_CONTA
-- ============================================================================
CREATE TABLE dbo.TipoConta
(
    Id          INT IDENTITY(1,1) NOT NULL,
    Descricao   VARCHAR(50) NOT NULL,

    CONSTRAINT PK_TipoConta
        PRIMARY KEY (Id),

    CONSTRAINT UQ_TipoConta_Descricao
        UNIQUE (Descricao)
);
GO

-- ============================================================================
-- 4. CONTA
-- ============================================================================
CREATE TABLE dbo.Conta
(
    Id              INT IDENTITY(1,1) NOT NULL,
    ClienteId       INT NOT NULL,
    TipoContaId     INT NOT NULL,
    Numero          VARCHAR(20) NOT NULL,
    Saldo           DECIMAL(15,2) NOT NULL,
    Ativa           BIT NOT NULL
        CONSTRAINT DF_Conta_Ativa DEFAULT 1,

    CONSTRAINT PK_Conta
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Conta_Numero
        UNIQUE (Numero),

    CONSTRAINT FK_Conta_Cliente
        FOREIGN KEY (ClienteId)
        REFERENCES dbo.Cliente(Id),

    CONSTRAINT FK_Conta_TipoConta
        FOREIGN KEY (TipoContaId)
        REFERENCES dbo.TipoConta(Id),

    CONSTRAINT CK_Conta_Saldo
        CHECK (Saldo >= 0)
);
GO

-- ============================================================================
-- 5. TRANSFERENCIA
-- ============================================================================
CREATE TABLE dbo.Transferencia
(
    Id                  INT IDENTITY(1,1) NOT NULL,
    ContaOrigemId       INT NOT NULL,
    ContaDestinoId      INT NOT NULL,
    Valor               DECIMAL(15,2) NOT NULL,
    DataTransferencia   DATETIME2 NOT NULL
        CONSTRAINT DF_Transferencia_Data DEFAULT SYSDATETIME(),
    Observacao          VARCHAR(200) NULL,

    CONSTRAINT PK_Transferencia
        PRIMARY KEY (Id),

    CONSTRAINT FK_Transferencia_ContaOrigem
        FOREIGN KEY (ContaOrigemId)
        REFERENCES dbo.Conta(Id),

    CONSTRAINT FK_Transferencia_ContaDestino
        FOREIGN KEY (ContaDestinoId)
        REFERENCES dbo.Conta(Id),

    CONSTRAINT CK_Transferencia_Valor
        CHECK (Valor > 0),

    CONSTRAINT CK_Transferencia_ContasDiferentes
        CHECK (ContaOrigemId <> ContaDestinoId)
);
GO

-- ============================================================================
-- 6. DADOS INICIAIS
-- ============================================================================
INSERT INTO dbo.TipoConta (Descricao)
VALUES
    ('Conta Corrente'),
    ('Conta Poupanca');
GO

INSERT INTO dbo.Cliente (Nome, Documento)
VALUES
    ('Cliente Exemplo A', 'DOC-0001'),
    ('Cliente Exemplo B', 'DOC-0002'),
    ('Cliente Exemplo C', 'DOC-0003');
GO

INSERT INTO dbo.Conta
(
    ClienteId,
    TipoContaId,
    Numero,
    Saldo
)
VALUES
    (1, 1, 'CC-1001', 1000.00),
    (2, 1, 'CC-1002',  500.00),
    (3, 2, 'CP-2001', 1500.00);
GO

-- ============================================================================
-- 7. CONFERÊNCIA
-- ============================================================================
SELECT
    C.Id AS ClienteId,
    C.Nome,
    C.Documento,
    CO.Id AS ContaId,
    CO.Numero,
    TC.Descricao AS TipoConta,
    CO.Saldo,
    CO.Ativa
FROM dbo.Cliente C
INNER JOIN dbo.Conta CO
    ON CO.ClienteId = C.Id
INNER JOIN dbo.TipoConta TC
    ON TC.Id = CO.TipoContaId
ORDER BY C.Id, CO.Id;
GO

SELECT
    SUM(Saldo) AS TotalDosSaldos
FROM dbo.Conta;
GO

SELECT
    Id,
    ContaOrigemId,
    ContaDestinoId,
    Valor,
    DataTransferencia,
    Observacao
FROM dbo.Transferencia
ORDER BY Id;
GO

-- ============================================================================
-- FIM DO SCRIPT DE ESTRUTURA E DADOS
-- ============================================================================

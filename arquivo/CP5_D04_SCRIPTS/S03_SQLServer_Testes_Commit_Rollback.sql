-- CP5 - TESTES DE COMMIT / ROLLBACK - SQL SERVER
-- Execute conectado à database do CP5. NÃO execute no master.

SELECT DB_NAME() AS BancoAtual;
GO

IF DB_NAME() = 'master'
BEGIN
    THROW 51000, 'ERRO: selecione a base do CP5 antes de executar os testes.', 1;
END;
GO

-- ============================================================================
-- ESTADO INICIAL
-- ============================================================================
SELECT Id, Numero, Saldo
FROM dbo.Conta
ORDER BY Numero;

SELECT *
FROM dbo.Transferencia
ORDER BY Id;
GO

-- ============================================================================
-- TESTE 1 - SUCESSO / COMMIT
-- Contas de referência: CC-1001 -> CC-1002
-- ============================================================================
BEGIN TRANSACTION;

UPDATE dbo.Conta
SET Saldo = Saldo - 200.00
WHERE Numero = 'CC-1001';

UPDATE dbo.Conta
SET Saldo = Saldo + 200.00
WHERE Numero = 'CC-1002';

INSERT INTO dbo.Transferencia
(
    ContaOrigemId,
    ContaDestinoId,
    Valor,
    Observacao
)
SELECT
    O.Id,
    D.Id,
    200.00,
    'Teste COMMIT CP5'
FROM dbo.Conta O
CROSS JOIN dbo.Conta D
WHERE O.Numero = 'CC-1001'
  AND D.Numero = 'CC-1002';

COMMIT TRANSACTION;
GO

SELECT Id, Numero, Saldo
FROM dbo.Conta
ORDER BY Numero;

SELECT *
FROM dbo.Transferencia
ORDER BY Id;
GO

-- ============================================================================
-- RESET PARA O TESTE DE ROLLBACK
-- ============================================================================
DELETE FROM dbo.Transferencia;

UPDATE dbo.Conta SET Saldo = 1000.00 WHERE Numero = 'CC-1001';
UPDATE dbo.Conta SET Saldo =  500.00 WHERE Numero = 'CC-1002';
UPDATE dbo.Conta SET Saldo = 1500.00 WHERE Numero = 'CP-2001';
GO

-- ============================================================================
-- TESTE 2 - FALHA / ROLLBACK
-- A falha é simulada DEPOIS do débito.
-- ============================================================================
BEGIN TRANSACTION;

UPDATE dbo.Conta
SET Saldo = Saldo - 200.00
WHERE Numero = 'CC-1001';

-- Neste ponto o débito existe somente dentro da transação.
SELECT Id, Numero, Saldo
FROM dbo.Conta
WHERE Numero IN ('CC-1001', 'CC-1002')
ORDER BY Numero;

-- Simulação didática: imagine que a aplicação C# lançou uma exceção aqui.
ROLLBACK TRANSACTION;
GO

-- Resultado esperado:
-- CC-1001 = 1000,00
-- CC-1002 =  500,00
-- nenhum registro parcial em TRANSFERENCIA
SELECT Id, Numero, Saldo
FROM dbo.Conta
ORDER BY Numero;

SELECT *
FROM dbo.Transferencia
ORDER BY Id;
GO

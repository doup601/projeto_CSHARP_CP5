-- CP5 - TESTES DE COMMIT / ROLLBACK - ORACLE
-- Execute conectado como o schema do CP5, por exemplo CP5_RM999999.
--
-- ============================================================================
-- ATENÇÃO - AUTO-COMMIT
-- Para demonstrar ROLLBACK MANUALMENTE no DBeaver ou Valentina Studio,
-- o Auto-commit deve estar DESATIVADO.
-- Com Auto-commit ativo, um UPDATE pode ser confirmado antes do ROLLBACK,
-- invalidando a demonstração.
-- ============================================================================

SELECT
    USER AS USUARIO_ATUAL,
    SYS_CONTEXT('USERENV', 'CON_NAME') AS CONTAINER
FROM DUAL;

-- ============================================================================
-- ESTADO INICIAL
-- ============================================================================
SELECT ID, NUMERO, SALDO
FROM CONTA
ORDER BY NUMERO;

SELECT *
FROM TRANSFERENCIA
ORDER BY ID;

-- ============================================================================
-- TESTE 1 - SUCESSO / COMMIT
-- Contas de referência: CC-1001 -> CC-1002
-- ============================================================================
UPDATE CONTA
SET SALDO = SALDO - 200.00
WHERE NUMERO = 'CC-1001';

UPDATE CONTA
SET SALDO = SALDO + 200.00
WHERE NUMERO = 'CC-1002';

INSERT INTO TRANSFERENCIA
(
    CONTA_ORIGEM_ID,
    CONTA_DESTINO_ID,
    VALOR,
    OBSERVACAO
)
SELECT
    O.ID,
    D.ID,
    200.00,
    'Teste COMMIT CP5'
FROM CONTA O
CROSS JOIN CONTA D
WHERE O.NUMERO = 'CC-1001'
  AND D.NUMERO = 'CC-1002';

COMMIT;

SELECT ID, NUMERO, SALDO
FROM CONTA
ORDER BY NUMERO;

SELECT *
FROM TRANSFERENCIA
ORDER BY ID;

-- ============================================================================
-- RESET PARA O TESTE DE ROLLBACK
-- ============================================================================
DELETE FROM TRANSFERENCIA;

UPDATE CONTA SET SALDO = 1000.00 WHERE NUMERO = 'CC-1001';
UPDATE CONTA SET SALDO =  500.00 WHERE NUMERO = 'CC-1002';
UPDATE CONTA SET SALDO = 1500.00 WHERE NUMERO = 'CP-2001';

COMMIT;

-- ============================================================================
-- TESTE 2 - FALHA / ROLLBACK
-- A falha é simulada DEPOIS do débito.
-- ============================================================================
UPDATE CONTA
SET SALDO = SALDO - 200.00
WHERE NUMERO = 'CC-1001';

-- Neste ponto o débito existe somente dentro da transação da sessão.
SELECT ID, NUMERO, SALDO
FROM CONTA
WHERE NUMERO IN ('CC-1001', 'CC-1002')
ORDER BY NUMERO;

-- Simulação didática: imagine que a aplicação C# lançou uma exceção aqui.
ROLLBACK;

-- Resultado esperado:
-- CC-1001 = 1000,00
-- CC-1002 =  500,00
-- nenhum registro parcial em TRANSFERENCIA
SELECT ID, NUMERO, SALDO
FROM CONTA
ORDER BY NUMERO;

SELECT *
FROM TRANSFERENCIA
ORDER BY ID;

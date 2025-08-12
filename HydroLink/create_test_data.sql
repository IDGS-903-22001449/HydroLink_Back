-- Script para crear datos de prueba para ProductoComprado

-- Buscar el UserId del usuario vicente@gmail.com
DECLARE @UserId NVARCHAR(450);
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'vicente@gmail.com';

-- Buscar algunos ProductoIds existentes
DECLARE @ProductoId1 INT, @ProductoId2 INT;
SELECT TOP 1 @ProductoId1 = Id FROM ProductoHydroLink WHERE Activo = 1;
SELECT @ProductoId2 = Id FROM ProductoHydroLink WHERE Activo = 1 AND Id <> @ProductoId1;

-- Si tenemos el usuario y al menos un producto, crear registros de prueba
IF @UserId IS NOT NULL AND @ProductoId1 IS NOT NULL
BEGIN
    -- Insertar primera compra
    IF NOT EXISTS (SELECT 1 FROM ProductoComprado WHERE UserId = @UserId AND ProductoId = @ProductoId1)
    BEGIN
        INSERT INTO ProductoComprado (UserId, ProductoId, FechaCompra, VentaId)
        VALUES (@UserId, @ProductoId1, GETDATE() - 30, NULL);
        PRINT 'Compra 1 creada para Usuario: ' + @UserId + ', Producto: ' + CAST(@ProductoId1 AS VARCHAR);
    END
    ELSE
        PRINT 'La compra 1 ya existe';

    -- Insertar segunda compra si hay un segundo producto
    IF @ProductoId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ProductoComprado WHERE UserId = @UserId AND ProductoId = @ProductoId2)
    BEGIN
        INSERT INTO ProductoComprado (UserId, ProductoId, FechaCompra, VentaId)
        VALUES (@UserId, @ProductoId2, GETDATE() - 15, NULL);
        PRINT 'Compra 2 creada para Usuario: ' + @UserId + ', Producto: ' + CAST(@ProductoId2 AS VARCHAR);
    END

    -- Añadir un manual PDF de prueba a un producto (base64 dummy)
    UPDATE ProductoHydroLink 
    SET ManualUsuarioPdf = 'JVBERi0xLjQKJeLjz9MKNCAwIG9iago8PAovVHlwZSAvQ2F0YWxvZwovUGFnZXMgMiAwIFIKPj4KZW5kb2JqCjIgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFsgMSAwIFIgXQovQ291bnQgMQo+PgplbmRvYmoKMSAwIG9iago8PAovVHlwZSAvUGFnZQovUGFyZW50IDIgMCBSCi9NZWRpYUJveCBbIDAgMCA2MTIgNzkyIF0KL0NvbnRlbnRzIDMgMCBSCj4+CmVuZG9iagozIDAgb2JqCjw8Ci9MZW5ndGggMTA4Cj4+CnN0cmVhbQpCVApxCjIgMCAwIDIgMzAwIDY1MCBjbQpCVAovRjEgMTIgVGYKKEhlbGxvIFdvcmxkISBUaGlzIGlzIGEgdGVzdCBQREYgbWFudWFsKSBUagpFVApRCmVuZHN0cmVhbQplbmRvYmoKeHJlZgowIDQKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMDM5IDAwMDAwIG4gCjAwMDAwMDAwOTMgMDAwMDAgbiAKMDAwMDAwMDE0NyAwMDAwMCBuIAp0cmFpbGVyCjw8Ci9TaXplIDQKL1Jvb3QgNCAwIFIKPj4Kc3RhcnR4cmVmCjMxMAolJUVPRgo='
    WHERE Id = @ProductoId1;
    PRINT 'Manual PDF añadido al producto: ' + CAST(@ProductoId1 AS VARCHAR);

    -- Mostrar los datos creados
    SELECT 
        pc.Id,
        pc.FechaCompra,
        p.Nombre as ProductoNombre,
        CASE 
            WHEN p.ManualUsuarioPdf IS NOT NULL THEN 'Sí'
            ELSE 'No'
        END as TieneManual
    FROM ProductoComprado pc
    INNER JOIN ProductoHydroLink p ON pc.ProductoId = p.Id
    WHERE pc.UserId = @UserId;
END
ELSE
BEGIN
    PRINT 'No se pudo encontrar el usuario o productos para crear datos de prueba';
    PRINT 'UserId: ' + ISNULL(@UserId, 'NULL');
    PRINT 'ProductoId1: ' + ISNULL(CAST(@ProductoId1 AS VARCHAR), 'NULL');
END

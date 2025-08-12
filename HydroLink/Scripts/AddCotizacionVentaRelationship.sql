-- Migración para agregar relación entre Cotización y Venta
-- Ejecutar este script en SQL Server Management Studio

-- Agregar columna VentaId a la tabla Cotizacion (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cotizacion]') AND name = 'VentaId')
BEGIN
    ALTER TABLE [dbo].[Cotizacion] 
    ADD [VentaId] int NULL;
END

-- Agregar columna CotizacionId a la tabla Venta (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Venta]') AND name = 'CotizacionId')
BEGIN
    ALTER TABLE [dbo].[Venta] 
    ADD [CotizacionId] int NULL;
END

-- Crear foreign key constraint de Cotizacion a Venta (relación uno-a-uno)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Cotizacion_Venta_VentaId')
BEGIN
    ALTER TABLE [dbo].[Cotizacion]
    ADD CONSTRAINT [FK_Cotizacion_Venta_VentaId] 
        FOREIGN KEY ([VentaId]) 
        REFERENCES [dbo].[Venta] ([Id])
        ON DELETE SET NULL;
END

-- Crear foreign key constraint de Venta a Cotizacion (relación uno-a-uno)
-- Esta relación se configura desde Cotizacion como el lado principal
-- Solo agregamos el índice para la consulta inversa
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Venta_Cotizacion_CotizacionId')
BEGIN
    ALTER TABLE [dbo].[Venta]
    ADD CONSTRAINT [FK_Venta_Cotizacion_CotizacionId] 
        FOREIGN KEY ([CotizacionId]) 
        REFERENCES [dbo].[Cotizacion] ([Id])
        ON DELETE SET NULL;
END

-- Crear índices para mejorar el rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cotizacion_VentaId')
BEGIN
    CREATE INDEX [IX_Cotizacion_VentaId] ON [dbo].[Cotizacion] ([VentaId]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Venta_CotizacionId')
BEGIN
    CREATE INDEX [IX_Venta_CotizacionId] ON [dbo].[Venta] ([CotizacionId]);
END

PRINT 'Relación entre Cotización y Venta agregada exitosamente.'

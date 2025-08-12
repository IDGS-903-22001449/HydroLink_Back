-- Script para insertar datos de ejemplo para productos y componentes para pruebas de cotización

-- Insertar componentes básicos si no existen
IF NOT EXISTS (SELECT 1 FROM Componente WHERE Nombre = 'Bomba de Agua 12V')
BEGIN
    INSERT INTO Componente (Nombre, Descripcion, Categoria, Precio, UnidadMedida, Especificaciones, Activo, FechaCreacion)
    VALUES ('Bomba de Agua 12V', 'Bomba de agua sumergible de 12V para sistemas de riego', 'Bomba', 250.00, 'Pieza', '12V, 5L/min, Sumergible', 1, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Componente WHERE Nombre = 'Sensor de Humedad')
BEGIN
    INSERT INTO Componente (Nombre, Descripcion, Categoria, Precio, UnidadMedida, Especificaciones, Activo, FechaCreacion)
    VALUES ('Sensor de Humedad', 'Sensor capacitivo de humedad del suelo', 'Sensor', 35.00, 'Pieza', 'Capacitivo, Resistente al agua', 1, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Componente WHERE Nombre = 'Microcontrolador ESP32')
BEGIN
    INSERT INTO Componente (Nombre, Descripcion, Categoria, Precio, UnidadMedida, Especificaciones, Activo, FechaCreacion)
    VALUES ('Microcontrolador ESP32', 'Microcontrolador con WiFi y Bluetooth integrado', 'Control', 180.00, 'Pieza', 'WiFi, Bluetooth, 32 GPIO', 1, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Componente WHERE Nombre = 'Manguera Flexible')
BEGIN
    INSERT INTO Componente (Nombre, Descripcion, Categoria, Precio, UnidadMedida, Especificaciones, Activo, FechaCreacion)
    VALUES ('Manguera Flexible', 'Manguera flexible para sistemas de riego', 'Tubería', 15.00, 'Metro', 'Diámetro 6mm, Resistente UV', 1, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Componente WHERE Nombre = 'Válvula Solenoide')
BEGIN
    INSERT INTO Componente (Nombre, Descripcion, Categoria, Precio, UnidadMedida, Especificaciones, Activo, FechaCreacion)
    VALUES ('Válvula Solenoide', 'Válvula electromagnética para control de flujo', 'Válvula', 120.00, 'Pieza', '12V, 1/2 pulgada, Normalmente cerrada', 1, GETUTCDATE())
END

-- Insertar algunos clientes de ejemplo
IF NOT EXISTS (SELECT 1 FROM Persona WHERE Email = 'maria.lopez@example.com')
BEGIN
    INSERT INTO Persona (Nombre, Apellido, Email, Telefono, Direccion, TipoPersona, Empresa, FechaRegistro, Activo)
    VALUES ('María', 'López', 'maria.lopez@example.com', '555-0123', 'Av. Principal 123, Ciudad', 'Cliente', 'López Jardines', GETUTCDATE(), 1)
END

IF NOT EXISTS (SELECT 1 FROM Persona WHERE Email = 'carlos.mendoza@example.com')
BEGIN
    INSERT INTO Persona (Nombre, Apellido, Email, Telefono, Direccion, TipoPersona, Empresa, FechaRegistro, Activo)
    VALUES ('Carlos', 'Mendoza', 'carlos.mendoza@example.com', '555-0456', 'Calle Comercial 456, Ciudad', 'Cliente', 'Restaurant Verde', GETUTCDATE(), 1)
END

-- Insertar productos HydroLink de ejemplo
IF NOT EXISTS (SELECT 1 FROM ProductoHydroLink WHERE Nombre = 'Sistema Hidropónico Residencial')
BEGIN
    INSERT INTO ProductoHydroLink (Nombre, Descripcion, Categoria, Precio, Activo, FechaCreacion, Especificaciones, TipoInstalacion, TiempoInstalacion, Garantia, ImagenBase64)
    VALUES ('Sistema Hidropónico Residencial', 'Sistema completo de hidroponía para uso doméstico', 'Residencial', 12000.00, 1, GETUTCDATE(), 
           'Sistema automatizado con sensores de humedad y control WiFi', 'Instalación interior/exterior', '2-3 horas', '1 año garantía completa', NULL)
END

IF NOT EXISTS (SELECT 1 FROM ProductoHydroLink WHERE Nombre = 'Sistema Hidropónico Comercial')
BEGIN
    INSERT INTO ProductoHydroLink (Nombre, Descripcion, Categoria, Precio, Activo, FechaCreacion, Especificaciones, TipoInstalacion, TiempoInstalacion, Garantia, ImagenBase64)
    VALUES ('Sistema Hidropónico Comercial', 'Sistema hidropónico para uso comercial e industrial', 'Comercial', 25000.00, 1, GETUTCDATE(), 
           'Sistema robusto con múltiples sensores y control remoto', 'Instalación profesional', '4-6 horas', '2 años garantía completa', NULL)
END

-- Relacionar componentes con productos (ComponenteRequerido)
DECLARE @ResidencialId INT = (SELECT Id FROM ProductoHydroLink WHERE Nombre = 'Sistema Hidropónico Residencial')
DECLARE @ComercialId INT = (SELECT Id FROM ProductoHydroLink WHERE Nombre = 'Sistema Hidropónico Comercial')
DECLARE @BombaId INT = (SELECT Id FROM Componente WHERE Nombre = 'Bomba de Agua 12V')
DECLARE @SensorId INT = (SELECT Id FROM Componente WHERE Nombre = 'Sensor de Humedad')
DECLARE @ESP32Id INT = (SELECT Id FROM Componente WHERE Nombre = 'Microcontrolador ESP32')
DECLARE @MangueraId INT = (SELECT Id FROM Componente WHERE Nombre = 'Manguera Flexible')
DECLARE @ValvulaId INT = (SELECT Id FROM Componente WHERE Nombre = 'Válvula Solenoide')

-- Componentes para sistema residencial
IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ResidencialId AND ComponenteId = @BombaId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ResidencialId, @BombaId, 1, 'Bomba principal del sistema')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ResidencialId AND ComponenteId = @SensorId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ResidencialId, @SensorId, 2, 'Sensores para monitoreo de humedad')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ResidencialId AND ComponenteId = @ESP32Id)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ResidencialId, @ESP32Id, 1, 'Controlador principal del sistema')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ResidencialId AND ComponenteId = @MangueraId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ResidencialId, @MangueraId, 10, 'Manguera para distribución de agua')
END

-- Componentes para sistema comercial (cantidades mayores)
IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ComercialId AND ComponenteId = @BombaId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ComercialId, @BombaId, 2, 'Bombas principales con redundancia')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ComercialId AND ComponenteId = @SensorId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ComercialId, @SensorId, 5, 'Múltiples sensores para monitoreo')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ComercialId AND ComponenteId = @ESP32Id)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ComercialId, @ESP32Id, 2, 'Controladores principales con redundancia')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ComercialId AND ComponenteId = @MangueraId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ComercialId, @MangueraId, 25, 'Manguera para sistema comercial')
END

IF NOT EXISTS (SELECT 1 FROM ComponenteRequerido WHERE ProductoHydroLinkId = @ComercialId AND ComponenteId = @ValvulaId)
BEGIN
    INSERT INTO ComponenteRequerido (ProductoHydroLinkId, ComponenteId, Cantidad, Especificaciones)
    VALUES (@ComercialId, @ValvulaId, 3, 'Válvulas para control de sectores')
END

PRINT 'Datos de ejemplo insertados correctamente.'

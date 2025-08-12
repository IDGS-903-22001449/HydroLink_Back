-- ================================================
-- DATOS DE EJEMPLO - PRODUCTOS MODULARES HYDROLINK
-- ================================================

-- 1. INSERTAR COMPONENTES BASE
INSERT INTO Componente (Nombre, Descripcion, Categoria, PrecioUnitario, UnidadMedida, Especificaciones, EsPersonalizable, FechaCreacion, Activo)
VALUES 
-- Sensores
('Sensor pH Digital', 'Sensor digital de pH con pantalla LCD, rango 0-14 pH', 'Sensores', 45.00, 'Unidad', 'Precisión ±0.1 pH, resistente al agua IP65', 0, GETDATE(), 1),
('Sensor Temperatura/Humedad', 'Sensor DHT22 para temperatura y humedad relativa', 'Sensores', 25.00, 'Unidad', 'Rango temperatura: -40°C a 80°C, Humedad: 0-100% RH', 0, GETDATE(), 1),
('Sensor Conductividad EC', 'Medidor de conductividad eléctrica para nutrientes', 'Sensores', 35.00, 'Unidad', 'Rango: 0-5000 μS/cm, compensación automática temperatura', 0, GETDATE(), 1),
('Sensor Nivel de Agua', 'Sensor ultrasónico para monitoreo de nivel de agua', 'Sensores', 30.00, 'Unidad', 'Rango: 2cm - 400cm, precisión 3mm', 0, GETDATE(), 1),

-- Sistema de Riego
('Bomba de Agua 12V', 'Bomba sumergible de 12V para circulación de nutrientes', 'Riego', 55.00, 'Unidad', 'Caudal: 300L/h, Altura: 3m, consumo 24W', 0, GETDATE(), 1),
('Tubería PVC 1/2"', 'Tubería de PVC para distribución de agua', 'Riego', 3.50, 'Metro', 'Diámetro 1/2", presión máxima 10 bar', 0, GETDATE(), 1),
('Conexiones T 1/2"', 'Conectores en T para derivaciones de tubería', 'Riego', 1.20, 'Unidad', 'PVC, rosca 1/2", para sistemas de baja presión', 0, GETDATE(), 1),
('Válvula Solenoide', 'Válvula electromagnética para control automatizado', 'Riego', 25.00, 'Unidad', '12V DC, caudal 0-120 L/min, normalmente cerrada', 0, GETDATE(), 1),
('Goteros Autocompensados', 'Goteros de caudal constante para riego localizado', 'Riego', 0.35, 'Unidad', 'Caudal: 2 L/h, compensación de presión 0.5-4 bar', 0, GETDATE(), 1),

-- Estructura Física
('Perfil Aluminio 40x40', 'Perfil estructural de aluminio para soporte', 'Estructura', 12.00, 'Metro', 'Sección 40x40mm, aleación 6063-T5, anodizado', 0, GETDATE(), 1),
('Bandeja de Cultivo', 'Bandeja perforada para colocación de plantas', 'Estructura', 18.00, 'Unidad', 'Polipropileno, 60x40x8cm, 20 orificios de 5cm', 0, GETDATE(), 1),
('Soporte para Plantas', 'Red o estructura para soporte de plantas trepadoras', 'Estructura', 8.50, 'Unidad', 'Malla plástica 1x2m, resistente UV', 0, GETDATE(), 1),
('Depósito de Nutrientes', 'Tanque para almacenamiento de solución nutritiva', 'Estructura', 85.00, 'Unidad', 'Polietileno 200L, con tapa hermética y válvula', 0, GETDATE(), 1),

-- Sistema de Control
('Controlador Arduino', 'Microcontrolador para automatización del sistema', 'Control', 25.00, 'Unidad', 'Arduino Mega 2560, 54 pines I/O, memoria 256KB', 0, GETDATE(), 1),
('Módulo Relé 8 Canales', 'Módulo de relés para control de dispositivos', 'Control', 15.00, 'Unidad', '8 relés 10A, opto-aislados, 5V/12V', 0, GETDATE(), 1),
('Pantalla LCD 20x4', 'Display para visualización de datos del sistema', 'Control', 12.00, 'Unidad', 'LCD 20x4 caracteres, backlight azul, I2C', 0, GETDATE(), 1),
('Fuente de Poder 12V', 'Fuente switching para alimentación del sistema', 'Control', 20.00, 'Unidad', '12V 5A, protección cortocircuito', 0, GETDATE(), 1),

-- Iluminación
('Panel LED Full Spectrum', 'Iluminación LED para crecimiento de plantas', 'Iluminacion', 120.00, 'Unidad', '100W, espectro completo 380-780nm, IP65', 0, GETDATE(), 1),
('Timer Digital', 'Temporizador digital para control de iluminación', 'Iluminacion', 15.00, 'Unidad', 'Programable 24h, resolución 1 minuto', 0, GETDATE(), 1),

-- Materiales Diversos
('Sustrato Perlita', 'Sustrato inerte para cultivo hidropónico', 'Materiales', 8.00, 'Litro', 'Perlita expandida, pH neutro, grano 2-6mm', 0, GETDATE(), 1),
('Nutrientes Hidropónicos', 'Solución nutritiva concentrada A+B', 'Materiales', 35.00, 'Litro', 'NPK balanceado + micronutrientes, rinde 500L', 0, GETDATE(), 1);

-- 2. CREAR PRODUCTO MODULAR BASE
INSERT INTO ProductoModular (Nombre, Version, Descripcion, CapacidadPorModulo, PrecioBaseModulo, PrecioModuloAdicional, TipoPlanta, Dimensiones, Especificaciones, Activo, FechaCreacion)
VALUES (
    'Sistema Hidropónico HydroLink', 
    'v1.0', 
    'Sistema hidropónico modular automatizado con monitoreo de sensores y control de riego inteligente. Ideal para producción comercial y doméstica de vegetales de hoja verde.',
    20, 
    1250.00, 
    950.00, 
    'Lechugas, espinacas, rúcula, aromáticas, microgreens', 
    '120cm x 80cm x 180cm por módulo', 
    'Sistema NFT (Nutrient Film Technique) con recirculación automática, monitoreo 24/7 de pH, EC y temperatura, control remoto vía WiFi, estructura en aluminio resistente a la corrosión.',
    1, 
    GETDATE()
);

-- 3. AGREGAR COMPONENTES AL PRODUCTO MODULAR
DECLARE @ProductoId INT = (SELECT TOP 1 Id FROM ProductoModular WHERE Nombre = 'Sistema Hidropónico HydroLink');

-- Sensores (BASE - se comparten entre módulos)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Instalar en zona central para monitoreo general del sistema', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor pH Digital';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Colocar en área ventilada, protegido de salpicaduras', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor Temperatura/Humedad';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Sumergir en tanque de nutrientes para monitoreo continuo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor Conductividad EC';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Instalar en depósito principal de nutrientes', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor Nivel de Agua';

-- Sistema de Riego (BASE + ADICIONAL)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Bomba principal para todo el sistema. Una bomba maneja hasta 4 módulos', 1, GETDATE()
FROM Componente WHERE Nombre = 'Bomba de Agua 12V';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 8, 6, 'ADICIONAL', 'Distribución principal + conexiones por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Tubería PVC 1/2"';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 4, 3, 'ADICIONAL', 'Conexiones para distribución por bandeja', 1, GETDATE()
FROM Componente WHERE Nombre = 'Conexiones T 1/2"';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Una válvula por módulo para control independiente', 1, GETDATE()
FROM Componente WHERE Nombre = 'Válvula Solenoide';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 20, 20, 'ADICIONAL', 'Un gotero por planta (20 plantas por módulo)', 1, GETDATE()
FROM Componente WHERE Nombre = 'Goteros Autocompensados';

-- Estructura (ADICIONAL - crece con módulos)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 6, 4, 'ADICIONAL', 'Estructura base + perfiles adicionales por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Perfil Aluminio 40x40';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Una bandeja por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Bandeja de Cultivo';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Soporte por módulo para plantas trepadoras', 0, GETDATE()
FROM Componente WHERE Nombre = 'Soporte para Plantas';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Depósito principal dimensionado para hasta 6 módulos', 1, GETDATE()
FROM Componente WHERE Nombre = 'Depósito de Nutrientes';

-- Sistema de Control (BASE - compartido)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Controlador central para todo el sistema', 1, GETDATE()
FROM Componente WHERE Nombre = 'Controlador Arduino';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Módulo de relés central, expandible según necesidad', 1, GETDATE()
FROM Componente WHERE Nombre = 'Módulo Relé 8 Canales';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Pantalla central para monitoreo del sistema', 1, GETDATE()
FROM Componente WHERE Nombre = 'Pantalla LCD 20x4';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Fuente principal, dimensionada para el sistema completo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Fuente de Poder 12V';

-- Iluminación (ADICIONAL - opcional pero recomendada)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Panel LED por módulo para cultivo interior', 0, GETDATE()
FROM Componente WHERE Nombre = 'Panel LED Full Spectrum';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Timer central para control de fotoperiodo', 0, GETDATE()
FROM Componente WHERE Nombre = 'Timer Digital';

-- Materiales (ADICIONAL)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 20, 20, 'ADICIONAL', 'Sustrato para 20 plantas por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sustrato Perlita';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 2, 1, 'ADICIONAL', 'Nutrientes base + adicional por módulo extra', 1, GETDATE()
FROM Componente WHERE Nombre = 'Nutrientes Hidropónicos';

-- ================================================
-- MENSAJES DE CONFIRMACIÓN
-- ================================================
PRINT '✅ Se han insertado ' + CAST((SELECT COUNT(*) FROM Componente) AS VARCHAR) + ' componentes';
PRINT '✅ Se ha creado ' + CAST((SELECT COUNT(*) FROM ProductoModular) AS VARCHAR) + ' producto modular';
PRINT '✅ Se han configurado ' + CAST((SELECT COUNT(*) FROM ComponenteModulo) AS VARCHAR) + ' relaciones componente-módulo';
PRINT '';
PRINT '🎯 SISTEMA LISTO PARA USAR:';
PRINT '   - Producto: Sistema Hidropónico HydroLink v1.0';
PRINT '   - Capacidad: 20 plantas por módulo';
PRINT '   - Componentes configurados: ' + CAST((SELECT COUNT(*) FROM ComponenteModulo WHERE ProductoModularId = @ProductoId) AS VARCHAR);
PRINT '   - Componentes BASE: ' + CAST((SELECT COUNT(*) FROM ComponenteModulo WHERE ProductoModularId = @ProductoId AND TipoComponente = 'BASE') AS VARCHAR);
PRINT '   - Componentes ADICIONALES: ' + CAST((SELECT COUNT(*) FROM ComponenteModulo WHERE ProductoModularId = @ProductoId AND TipoComponente = 'ADICIONAL') AS VARCHAR);
PRINT '';
PRINT '🚀 ¡El sistema modular HydroLink está listo para generar cotizaciones!';

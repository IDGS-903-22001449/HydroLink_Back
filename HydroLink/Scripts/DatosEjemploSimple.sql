-- Insertar componentes básicos
INSERT INTO Componente (Nombre, Descripcion, Categoria, PrecioUnitario, UnidadMedida, Especificaciones, EsPersonalizable, FechaCreacion, Activo)
VALUES 
('Sensor pH Digital', 'Sensor digital de pH con pantalla LCD', 'Sensores', 45.00, 'Unidad', 'Precisión ±0.1 pH, IP65', 0, GETDATE(), 1),
('Sensor Temperatura', 'Sensor DHT22 temperatura y humedad', 'Sensores', 25.00, 'Unidad', 'Rango -40°C a 80°C', 0, GETDATE(), 1),
('Bomba de Agua 12V', 'Bomba sumergible 12V', 'Riego', 55.00, 'Unidad', 'Caudal 300L/h, 24W', 0, GETDATE(), 1),
('Tubería PVC 1/2"', 'Tubería PVC distribución', 'Riego', 3.50, 'Metro', 'Diámetro 1/2", 10 bar', 0, GETDATE(), 1),
('Bandeja de Cultivo', 'Bandeja perforada plantas', 'Estructura', 18.00, 'Unidad', '60x40x8cm, 20 orificios', 0, GETDATE(), 1),
('Controlador Arduino', 'Microcontrolador sistema', 'Control', 25.00, 'Unidad', 'Arduino Mega 2560', 0, GETDATE(), 1),
('Panel LED', 'Iluminación LED plantas', 'Iluminacion', 120.00, 'Unidad', '100W espectro completo', 0, GETDATE(), 1),
('Nutrientes', 'Solución nutritiva A+B', 'Materiales', 35.00, 'Litro', 'NPK + micronutrientes', 0, GETDATE(), 1);

-- Crear producto modular
INSERT INTO ProductoModular (Nombre, Version, Descripcion, CapacidadPorModulo, PrecioBaseModulo, PrecioModuloAdicional, TipoPlanta, Dimensiones, Especificaciones, Activo, FechaCreacion)
VALUES (
    'Sistema Hidropónico HydroLink', 
    'v1.0', 
    'Sistema hidropónico modular automatizado',
    20, 
    1250.00, 
    950.00, 
    'Lechugas, espinacas, aromáticas', 
    '120cm x 80cm x 180cm por módulo', 
    'Sistema NFT con monitoreo automático',
    1, 
    GETDATE()
);

PRINT 'Datos insertados correctamente';
PRINT 'Sistema HydroLink configurado y listo para usar';

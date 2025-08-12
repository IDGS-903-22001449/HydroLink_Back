-- Vincular componentes al producto modular
DECLARE @ProductoId INT = (SELECT Id FROM ProductoModular WHERE Nombre = 'Sistema Hidropónico HydroLink');

-- Sensores (BASE - compartidos)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Monitoreo central del sistema', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor pH Digital';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Control ambiental', 1, GETDATE()
FROM Componente WHERE Nombre = 'Sensor Temperatura';

-- Riego (BASE + ADICIONAL)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Bomba principal del sistema', 1, GETDATE()
FROM Componente WHERE Nombre = 'Bomba de Agua 12V';

INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 5, 3, 'ADICIONAL', 'Distribución por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Tubería PVC 1/2"';

-- Estructura (ADICIONAL)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Una bandeja por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Bandeja de Cultivo';

-- Control (BASE)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 0, 'BASE', 'Controlador central', 1, GETDATE()
FROM Componente WHERE Nombre = 'Controlador Arduino';

-- Iluminación (ADICIONAL - opcional)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 1, 1, 'ADICIONAL', 'Panel LED por módulo', 0, GETDATE()
FROM Componente WHERE Nombre = 'Panel LED';

-- Materiales (ADICIONAL)
INSERT INTO ComponenteModulo (ProductoModularId, ComponenteId, CantidadBase, CantidadPorModuloAdicional, TipoComponente, NotasInstalacion, EsObligatorio, FechaCreacion)
SELECT @ProductoId, Id, 2, 1, 'ADICIONAL', 'Nutrientes por módulo', 1, GETDATE()
FROM Componente WHERE Nombre = 'Nutrientes';

PRINT 'Componentes vinculados al producto modular correctamente';
PRINT 'Sistema listo para generar cotizaciones modulares';

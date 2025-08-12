-- Script para insertar productos de ejemplo con imágenes base64
-- Imagen base64 de ejemplo (imagen pequeña de sistema hidropónico)

-- Producto 1: Sistema Hidropónico NFT Básico
INSERT INTO ProductoHydroLink (Nombre, Descripcion, Categoria, Precio, Activo, FechaCreacion, Especificaciones, TipoInstalacion, TiempoInstalacion, Garantia, ImagenBase64)
VALUES (
    'Sistema Hidropónico NFT Básico',
    'Sistema de cultivo hidropónico NFT (Nutrient Film Technique) ideal para principiantes. Incluye bomba de agua, tubería y estructura básica.',
    'Sistemas NFT',
    2500.00,
    1,
    GETDATE(),
    'Capacidad: 20 plantas, Dimensiones: 2m x 1m x 0.5m, Material: PVC y acero inoxidable',
    'Instalación en interiores o exteriores techados',
    '2-3 horas',
    '12 meses en estructura, 6 meses en componentes eléctricos',
    'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCdABmX/9k='
);

-- Producto 2: Sistema Hidropónico DWC Avanzado
INSERT INTO ProductoHydroLink (Nombre, Descripcion, Categoria, Precio, Activo, FechaCreacion, Especificaciones, TipoInstalacion, TiempoInstalacion, Garantia, ImagenBase64)
VALUES (
    'Sistema Hidropónico DWC Avanzado',
    'Sistema de cultivo hidropónico DWC (Deep Water Culture) con control automático de pH y nutrientes. Perfecto para cultivos comerciales.',
    'Sistemas DWC',
    4800.00,
    1,
    GETDATE(),
    'Capacidad: 50 plantas, Dimensiones: 3m x 2m x 1m, Control automático, Sensores incluidos',
    'Instalación profesional requerida',
    '4-6 horas',
    '24 meses en estructura, 12 meses en componentes electrónicos',
    'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCdABmX/9k='
);

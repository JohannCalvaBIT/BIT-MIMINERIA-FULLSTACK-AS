## Purpose

Proporcionar al Administrador Global una interfaz centralizada y gobernada para la administración de catálogos globales del sistema (Empresa, Formato, Disciplina), asegurando la integridad referencial, la prevención de duplicidad y el control de acceso.

## Requirements

## ADDED Requirements

### Requirement: Crear registro en catálogo
El sistema SHALL permitir al Administrador Global crear nuevos registros en los catálogos globales (Empresa, Formato o Disciplina) mediante un formulario validado que verifique la ausencia de duplicidad (case-insensitive) y que respete el límite de caracteres. La operación debe ser auditable (G-GLOBAL-06).

#### Scenario: Crear empresa con código y descripción válidos
- **WHEN** el Administrador Global completa el formulario "Crear Empresa" con Código="EMP001" y Descripción="Mi Minería S.A." sin duplicados
- **THEN** el sistema crea el registro, muestra un mensaje de éxito y actualiza el listado

#### Scenario: Intentar crear empresa con código duplicado
- **WHEN** el Administrador Global intenta crear una empresa con código ya existente (sin distinguir mayúsculas/minúsculas)
- **THEN** el sistema muestra un error de validación "Código ya existe" y no crea el registro

#### Scenario: Intentar crear empresa con descripción duplicada
- **WHEN** el Administrador Global intenta crear una empresa con descripción ya existente
- **THEN** el sistema muestra un error de validación y no crea el registro

#### Scenario: Crear formato con límite de caracteres
- **WHEN** el Administrador Global completa el formulario "Crear Formato" con Código (máx 150 caracteres) y Descripción (máx 250 caracteres)
- **THEN** el sistema crea el registro si ambos campos cumplen los límites y no hay duplicidad

#### Scenario: Crear disciplina
- **WHEN** el Administrador Global completa el formulario "Crear Disciplina" con Código y Descripción válidos
- **THEN** el sistema crea el registro con el mismo rigor de validación que Empresa y Formato

### Requirement: Editar registro en catálogo
El sistema SHALL permitir al Administrador Global editar un registro existente, permitiendo la modificación de ambos campos (Código y Descripción) sujeto a validaciones anti-duplicado. Si el registro está en uso por un proyecto, el Código no podrá ser editado (solo la Descripción). Cualquier cambio debe ser auditable (G-GLOBAL-06).

#### Scenario: Editar descripción de empresa en uso
- **WHEN** el Administrador Global abre un registro de Empresa que está en uso en proyectos, intenta cambiar la Descripción a "Nueva Descripción"
- **THEN** el sistema permite la edición, valida que la nueva descripción no sea duplicada, guarda el cambio y lo audita

#### Scenario: Intentar editar código de empresa en uso
- **WHEN** el Administrador Global intenta cambiar el Código de una Empresa que está en uso en proyectos
- **THEN** el sistema deshabilita el campo Código, muestra un mensaje informativo y solo permite editar la Descripción

#### Scenario: Editar empresa no en uso
- **WHEN** el Administrador Global abre un registro de Empresa que no está en uso, edita Código y Descripción
- **THEN** el sistema permite ambas ediciones, valida anti-duplicado para ambos campos y guarda los cambios

#### Scenario: Validar duplicidad durante edición
- **WHEN** el Administrador Global intenta cambiar el Código a uno que ya existe (incluso case-insensitive)
- **THEN** el sistema rechaza la edición con error de validación

### Requirement: Eliminar registro en catálogo
El sistema SHALL permitir la eliminación de registros únicamente cuando no están en uso en proyectos. Debe solicitarse confirmación antes de eliminar. La operación debe ser auditable e irreversible (G-GLOBAL-06).

#### Scenario: Eliminar empresa no en uso
- **WHEN** el Administrador Global selecciona la acción "Eliminar" en una Empresa no utilizada en proyectos
- **THEN** el sistema muestra un diálogo de confirmación; al confirmar, elimina el registro y lo audita

#### Scenario: Intentar eliminar empresa en uso
- **WHEN** el Administrador Global intenta eliminar una Empresa que está en uso en proyectos
- **THEN** el sistema deshabilita la acción "Eliminar", muestra un mensaje de bloqueo ("En uso por X proyectos") y no permite la eliminación

#### Scenario: Cancelar eliminación
- **WHEN** el diálogo de confirmación es mostrado y el Administrador Global selecciona "Cancelar"
- **THEN** el sistema cierra el diálogo sin realizar cambios

### Requirement: Seleccionar y listar catálogos
El sistema SHALL proporcionar una interfaz de selección para escoger entre los tres catálogos (Empresa, Formato, Disciplina) y mostrar el listado de registros del catálogo seleccionado con capacidades de búsqueda y filtrado.

#### Scenario: Seleccionar catálogo Empresa
- **WHEN** el Administrador Global accede al submódulo "Catálogos" dentro de Seguridad
- **THEN** el sistema muestra un selector de catálogos; al elegir "Empresa", carga y muestra el listado de empresas

#### Scenario: Listar formato con búsqueda
- **WHEN** el Administrador Global selecciona "Formato" y la interfaz muestra el listado vacío o existente
- **THEN** el sistema permite buscar por Código de formato o Descripción en tiempo real

#### Scenario: Listar disciplina con filtros
- **WHEN** el Administrador Global selecciona "Disciplina"
- **THEN** el sistema muestra todos los registros de disciplinas registradas, permitiendo búsqueda por Código o Descripción

### Requirement: Buscar y filtrar registros
El sistema SHALL permitir búsqueda por Código y Descripción (case-insensitive) en todos los catálogos, con resultados actualizados de forma interactiva.

#### Scenario: Buscar empresa por código
- **WHEN** el Administrador Global escribe "EMP" en el filtro de Código del catálogo Empresa
- **THEN** el sistema filtra el listado mostrando solo empresas cuyo Código contenga "EMP" (insensible a mayúsculas)

#### Scenario: Buscar formato por descripción
- **WHEN** el Administrador Global escribe "inspección" en el filtro de Descripción del catálogo Formato
- **THEN** el sistema filtra mostrando solo formatos con "inspección" en la descripción

#### Scenario: Búsqueda sin resultados
- **WHEN** el Administrador Global realiza una búsqueda que no retorna resultados
- **THEN** el sistema muestra un mensaje informativo ("No hay registros que coincidan con la búsqueda")

### Requirement: Control de acceso al módulo de catálogos
El sistema SHALL garantizar que solo el Administrador Global puede acceder al submódulo "Catálogos" dentro de Seguridad, validando permisos en el backend (G-GLOBAL-03, G-WEB-BE-04).

#### Scenario: Administrador Global accede al módulo
- **WHEN** un usuario autenticado con rol "Administrador Global" navega a Seguridad > Catálogos
- **THEN** el sistema carga la interfaz de catálogos sin restricción

#### Scenario: Usuario sin permiso intenta acceder
- **WHEN** un usuario autenticado sin rol "Administrador Global" intenta navegar a Seguridad > Catálogos
- **THEN** el sistema rechaza el acceso, muestra un mensaje de "Acceso denegado" y lo audita (G-GLOBAL-06)

#### Scenario: Validación de autorización en backend
- **WHEN** un cliente envía una petición POST/PUT/DELETE a `/api/catalogs/empresas` sin ser Administrador Global
- **THEN** el backend retorna 403 Forbidden y registra el intento fallido en auditoría

### Requirement: Persistencia y auditoría
El sistema SHALL persistir todos los cambios en la base de datos y registrar eventos de auditoría (creación, edición, eliminación) con usuario, timestamp y nuevo valor (G-GLOBAL-06).

#### Scenario: Auditar creación de empresa
- **WHEN** se crea exitosamente una empresa
- **THEN** se inserta un registro en la tabla de auditoría con: usuario, acción (CREATE), entidad (Empresa), ID de registro, timestamp y valores nuevos

#### Scenario: Auditar edición de formato
- **WHEN** se edita una descripción de Formato
- **THEN** se inserta un registro de auditoría con usuario, acción (UPDATE), campo modificado, valor anterior y nuevo valor, timestamp

#### Scenario: Auditar eliminación de disciplina
- **WHEN** se elimina una Disciplina
- **THEN** se inserta un registro de auditoría con usuario, acción (DELETE), entidad y timestamp (el registro de la tabla de disciplinas se elimina permanentemente)

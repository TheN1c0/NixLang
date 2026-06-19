# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 1.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

## 4. Relaciones entre Entidades

### 4.1 Mapa de Relaciones

| #  | Entidad Origen       | Relación                                | Entidad Destino  | Cardinalidad |
|----|----------------------|-----------------------------------------|------------------|:------------:|
| R1 | Lección              | *contiene*                              | Ejercicio        | 1 : N        |
| R2 | Ejercicio            | *tiene alternativas*                    | AlternativaEjercicio | 1 : N        |
| R3 | Lección              | *clasificada en (vía LecciónCategoría)* | Categoría        | N : M        |
| R4 | Usuario              | *marca como favorita (vía Favorito)*    | Lección          | N : M        |
| R5 | Usuario              | *registra progreso en*                  | Lección (vía ProgresoLección) | N : M (con atributos) |
| R6 | ProgresoLección      | *contiene resultados de*                | ResultadoEjercicio | 1 : N        |
| R7 | ResultadoEjercicio   | *corresponde a*                         | Ejercicio        | N : 1        |
| R8 | Usuario              | *genera*                                | GrabaciónAudio   | 1 : N        |
| R9 | Ejercicio            | *recibe grabaciones de*                 | GrabaciónAudio   | 1 : N        |
| R10| Usuario              | *genera*                                | RegistroActividad | 1 : N        |

### 4.2 Descripción Detallada de Relaciones

#### R1 — Lección ↔ Ejercicio `[1 : N]`
Una lección **contiene** uno o más ejercicios, los cuales se presentan de manera secuencial dentro de ella. Un ejercicio **pertenece** de forma exclusiva a una única lección.

#### R2 — Ejercicio ↔ AlternativaEjercicio `[1 : N]`
Un ejercicio de opción múltiple **tiene** dos o más alternativas. Al menos una de estas alternativas debe ser la correcta. Las alternativas existen únicamente ligadas al ejercicio correspondiente.

#### R3 — Lección ↔ Categoría (vía LecciónCategoría) `[N : M]`
Una lección **puede pertenecer** a una o más categorías temáticas, y una categoría **puede agrupar** múltiples lecciones, relacionándose a través de la entidad asociativa **LecciónCategoría**. Una lección puede no tener categorías asociadas, y una categoría puede no contener lecciones.

#### R4 — Usuario ↔ Lección (vía Favorito) `[N : M]`
Un usuario **puede marcar** múltiples lecciones como favoritas, y una lección **puede ser marcada** como favorita por múltiples usuarios, relacionándose a través de la entidad asociativa **Favorito** que documenta la fecha de dicha marcación.

#### R5 — Usuario ↔ Lección (Progreso) `[N : M con atributos]`
Un usuario **registra su progreso** al realizar una lección, pudiendo completarla en múltiples intentos independientes. Cada intento de lección genera un registro de **ProgresoLección** único que vincula al usuario con la lección correspondiente.

#### R6 — ProgresoLección ↔ ResultadoEjercicio `[1 : N]`
Un registro de **ProgresoLección** **contiene** los resultados individuales de cada ejercicio respondido durante ese intento. Cada **ResultadoEjercicio** pertenece de forma exclusiva a un único progreso de lección.

#### R7 — ResultadoEjercicio ↔ Ejercicio `[N : 1]`
Cada **ResultadoEjercicio** **corresponde** a un ejercicio específico de la lección. Un mismo ejercicio puede ser evaluado en múltiples resultados a través de diferentes intentos de progreso.

#### R8 y R9 — Usuario / Ejercicio ↔ GrabaciónAudio `[1 : N]`
Un usuario **genera** una **GrabaciónAudio** al realizar un ejercicio de pronunciación. Cada grabación pertenece a un único usuario y está asociada a un ejercicio de pronunciación específico.

#### R10 — Usuario ↔ RegistroActividad `[1 : N]`
Un usuario **genera** registros de actividad que representan sus interacciones y eventos relevantes dentro de la plataforma. Cada **RegistroActividad** está vinculado a un único usuario.

---


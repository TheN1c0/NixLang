# Análisis de Impacto y Ajustes al Backlog — Evolución LessonBlock

Este documento detalla la revisión de impacto sobre los **Requerimientos Funcionales (RF)** y las **Historias de Usuario (HU)** tras la incorporación de la entidad `LessonBlock` al Agregado Lección.

## Principio General de Separación de Responsabilidades

Para mantener la consistencia de la documentación y evitar contaminar los requerimientos funcionales y las historias de usuario con detalles de diseño técnico/dominio, se aplican los siguientes límites:
1. **Historias de Usuario (HU):** Describen las necesidades del usuario o del administrador (los actores). No conocen conceptos de diseño como `LessonBlock`.
2. **Requerimientos Funcionales (RF):** Describen el comportamiento esperado del sistema ante las acciones del usuario.
3. **Modelo de Dominio:** Describe la representación interna y lógica de negocio (`LessonBlock`, agregados, invariantes).
4. **Diseño Técnico:** Define la implementación de persistencia, DTOs y controladores.

---

## 1. Requerimientos Funcionales (RF) Afectados

### RF-007 — Consulta de Detalle de Lección
* **Comportamiento Esperado**: El sistema debe permitir visualizar la información detallada de una lección, incluyendo título, descripción, nivel referencial y cantidad de ejercicios.
* **Impacto del Dominio**: Internamente, el sistema recupera la secuencia de `LessonBlocks` asociada para construir la respuesta. La "cantidad de ejercicios" se calcula dinámicamente en el backend contando los bloques de tipo `EXERCISE`. No se expone la "cantidad de bloques" como requerimiento funcional.

### RF-012 — Visualización de Ejercicios
* **Comportamiento Esperado**: El sistema debe mostrar el contenido correspondiente al paso activo dentro del recorrido de la lección (que internamente se traduce en renderizar el `LessonBlock` correspondiente y, si es de tipo `EXERCISE`, el ejercicio asociado).

### RF-013 — Navegación
* **Comportamiento Esperado**: El sistema debe permitir navegar de forma secuencial (avanzar y retroceder) a través de los distintos pasos/contenidos definidos para la lección.
* **Impacto del Dominio**: La navegación se realiza iterando sobre la colección ordenada de `LessonBlocks`.

### RF-014 — Finalización de Lección
* **Comportamiento Esperado**: La lección se considera finalizada cuando el usuario completa el recorrido definido para la lección y realiza las interacciones requeridas por los bloques correspondientes.
* **Impacto del Dominio**: El backend valida que se hayan recorrido todos los `LessonBlocks` y registrado las respuestas correspondientes a los bloques evaluables.

### RF-027 / RF-028 — Creación y Edición de Lecciones
* **Comportamiento Esperado**: El administrador debe poder crear y modificar lecciones.
* **Impacto del Dominio**: La creación y edición a nivel de base de datos considera la composición y ordenación de la secuencia de `LessonBlocks`.

### RF-029 — Eliminación de Lecciones
* **Comportamiento Esperado**: Al eliminar una lección, se eliminan todos sus elementos contenidos.
* **Impacto del Dominio**: Se realiza la eliminación en cascada de los `LessonBlocks` asociados.

### RF-030 — Creación de Ejercicios
* **Comportamiento Esperado**: El administrador debe poder crear ejercicios y agregarlos a una lección.
* **Impacto del Dominio**: Funcionalmente, el administrador asocia el ejercicio a la lección. Internamente, el dominio genera un `LessonBlock` de tipo `EXERCISE` que encapsula la referencia al ejercicio creado.

### RF-032 — Eliminación de Ejercicios
* **Comportamiento Esperado**: Al eliminar un ejercicio, el sistema debe verificar que no esté siendo utilizado en ningún contenido de lección activo.
* **Impacto del Dominio**: Se valida que no existan `LessonBlocks` de tipo `EXERCISE` referenciando el identificador del ejercicio.

### RF-034 / RF-035 — Audio y Pronunciación
* **Comportamiento Esperado**: El sistema reproduce audios y graba pronunciación de los usuarios cuando el paso de la lección lo requiera.
* **Impacto del Dominio**: Estas acciones se mapean a bloques específicos de tipo `AUDIO` y `EXERCISE` (modalidad pronunciación).

---

## 2. Historias de Usuario (HU) Afectadas

Para cumplir con el principio general, las historias de usuario no mencionan el concepto técnico de `LessonBlock` y se centran en la experiencia del actor.

### HU-008 (Detalle de Lección)
* **Redacción Ajustada**: *Como usuario, quiero consultar la información de una lección para conocer su contenido y propósito pedagógico antes de comenzar a estudiarla.*

### HU-009 (Métricas de la Lección)
* **Redacción Ajustada**: *Como usuario, quiero visualizar el título, descripción, nivel referencial y cantidad de ejercicios de una lección para decidir si se ajusta a mis necesidades de aprendizaje.*
* **Nota técnica**: El sistema calcula la cantidad de ejercicios en segundo plano sumando los bloques de tipo `EXERCISE`.

### HU-019 (Carga de Contenido)
* **Redacción Ajustada**: *Como usuario, quiero que el sistema cargue el contenido de la lección seleccionada en el orden definido para poder realizarla.*

### HU-020 (Visualización de Contenido)
* **Redacción Ajustada**: *Como usuario, quiero visualizar el contenido de la lección para avanzar en mi aprendizaje.*

### HU-021 (Visualización de Ejercicio Activo)
* **Redacción Ajustada**: *Como usuario, cuando el contenido actual corresponde a un ejercicio, quiero visualizar su enunciado para comprender qué debo resolver.*

### HU-022 (Tipo de Ejercicio)
* **Redacción Ajustada**: *Como usuario, cuando el paso actual de la lección corresponde a una actividad evaluable, quiero ver de qué tipo es para saber cómo responder.*

### HU-023 (Posición en la Lección)
* **Redacción Ajustada**: *Como usuario, quiero visualizar mi posición dentro de la lección (ej. paso actual de un total) para saber cuánto me falta para completarla.*

### HU-024 / HU-025 / HU-026 (Navegación)
* **Redacción Ajustada**:
  * **HU-024**: *Como usuario, quiero avanzar por el contenido de la lección para continuar mi aprendizaje.*
  * **HU-025**: *Como usuario, quiero retroceder dentro de la lección para revisar los pasos o explicaciones anteriores.*
  * **HU-026**: *Como usuario, quiero navegar secuencialmente entre los distintos pasos de la lección para mantener el flujo pedagógico.*

### HU-027 / HU-053 (Finalización)
* **Redacción Ajustada**:
  * **HU-027**: *Como usuario, quiero que la lección finalice cuando complete el recorrido definido y realice las interacciones requeridas para poder registrar mi avance.*
  * **HU-053**: *Como usuario, quiero que el sistema registre la lección como completada cuando termine todos sus pasos e interacciones.*

### HU-080 / HU-084 (Creación y Asociación por Admin)
* **Redacción Ajustada**:
  * **HU-080**: *Como administrador, quiero crear nuevos ejercicios para complementar la oferta pedagógica de una lección.*
  * **HU-084**: *Como administrador, quiero agregar ejercicios a una lección para estructurar su contenido.*

### HU-089 / HU-090 (Eliminación por Admin)
* **Redacción Ajustada**:
  * **HU-089**: *Como administrador, quiero eliminar ejercicios obsoletos que ya no se utilicen en ninguna lección.*
  * **HU-090**: *Como administrador, quiero recibir una advertencia si intento eliminar un ejercicio que está siendo utilizado en algún contenido de lección activo.*

---

## 3. Nuevos Requerimientos Funcionales (RF) — Colecciones

### RF-048 — Visualización de Colecciones en Catálogo
* **Comportamiento Esperado**: El sistema debe permitir al usuario explorar las colecciones de aprendizaje disponibles, visualizando su título, descripción, icono/imagen representativo, nivel referencial sugerido y porcentaje de progreso derivado.
* **Impacto del Dominio**: Se consultan las entidades `Colección` con estado `Published`. El progreso se calcula dinámicamente según las lecciones completadas por el usuario autenticado.

### RF-049 — Detalle de Colección y Acceso Libre a Lecciones
* **Comportamiento Esperado**: El sistema debe permitir visualizar el detalle de una colección, listando todas sus lecciones asociadas en el orden sugerido, con su estado de avance y permitiendo acceder directamente a cualquiera de ellas sin bloqueos.
* **Impacto del Dominio**: Se recupera la colección con sus `ColecciónLección` ordenadas y las lecciones publicadas vinculadas, junto con el estado del usuario (`LessonProgress`).

### RF-050 — Cálculo de Progreso Agregado de Colección
* **Comportamiento Esperado**: El sistema debe calcular y mostrar el avance global del usuario en una colección (porcentaje y conteo de lecciones completadas respecto al total).
* **Impacto del Dominio**: Cálculo derivado en el Application layer ($N_{completadas} / N_{total} \times 100$), sin persistencia de estado redundante.

### RF-051 — Gestión de Colecciones por Administrador (CRUD)
* **Comportamiento Esperado**: El administrador debe poder crear, listar, consultar, editar y eliminar colecciones, así como alternar su estado de publicación (`Borrador`, `Publicada`, `Desactivada`).
* **Impacto del Dominio**: Operaciones sobre el Agregado `Colección`.

### RF-052 — Asignación y Reordenación de Lecciones en Colección
* **Comportamiento Esperado**: El administrador debe poder vincular lecciones del catálogo a una colección, definir su secuencia pedagógica sugerida y desvincular lecciones cuando sea necesario.
* **Impacto del Dominio**: Gestión de la colección interna `ColecciónLección` dentro del Agregado `Colección`.

### RF-053 — Filtrado y Búsqueda de Contenido por Colección
* **Comportamiento Esperado**: El sistema debe permitir filtrar lecciones del catálogo por colección o buscar colecciones por texto y nivel sugerido.

---

## 4. Nuevas Historias de Usuario (HU) — Colecciones

### HU-140 — Exploración de Colecciones
* **Redacción**: *Como estudiante, quiero ver una lista de colecciones temáticas y experienciales en el catálogo para descubrir contenidos organizados por situaciones de la vida real.*

### HU-141 — Consulta de Detalle de Colección
* **Redacción**: *Como estudiante, quiero ver el detalle de una colección con sus lecciones asociadas para entender los temas que cubre y su secuencia pedagógica recomendada.*

### HU-142 — Acceso Libre a Lecciones de una Colección
* **Redacción**: *Como estudiante, quiero poder iniciar cualquier lección dentro de una colección directamente y sin prerrequisitos para mantener mi libertad de aprendizaje.*

### HU-143 — Visualización de Progreso en Colección
* **Redacción**: *Como estudiante, quiero ver mi porcentaje de avance y cuántas lecciones he completado dentro de una colección para seguir mi ritmo de estudio.*

### HU-144 — Creación de Colección por Admin
* **Redacción**: *Como administrador, quiero crear nuevas colecciones con título, descripción y nivel para empaquetar lecciones bajo contextos de aprendizaje atractivos.*

### HU-145 — Gestión de Estado de Colección
* **Redacción**: *Como administrador, quiero publicar, guardar en borrador o desactivar colecciones para controlar cuándo son visibles para los estudiantes.*

### HU-146 — Asociación de Lecciones a Colección
* **Redacción**: *Como administrador, quiero asociar lecciones existentes a una colección para estructurar su contenido didáctico.*

### HU-147 — Reordenamiento de Lecciones en Colección
* **Redacción**: *Como administrador, quiero reordenar la secuencia sugerida de lecciones dentro de una colección para ofrecer un recorrido pedagógico coherente.*

### HU-148 — Eliminación de Colección
* **Redacción**: *Como administrador, quiero eliminar una colección obsoleta asegurando que sus lecciones permanezcan intactas en el catálogo general.*


# Diseño Funcional — Colecciones

## 1. ¿Qué es una Colección?

### Objetivo
Una **Colección** es una agrupación temática, experiencial o intencional de lecciones diseñada para organizar, contextualizar y facilitar el descubrimiento de contenidos en torno a un propósito de la vida real o una necesidad concreta del estudiante.

Ejemplos típicos:
* **Inglés para viajar:** Lecciones sobre aeropuertos, hoteles, pedir comida, emergencias.
* **Inglés para entrevistas laborales:** Presentación personal, fortalezas/debilidades, responder preguntas técnicas.
* **Inglés para conversar:** Saludos naturales, modismos, romper el hielo, hablar del clima.
* **Inglés con series:** Expresiones informales, humor, dobles sentidos, entonación.
* **Inglés para situaciones cotidianas:** Compras en el supermercado, hablar con vecinos, llamadas telefónicas.

### Principio Fundamental: Libertad de Aprendizaje
Fiel a la filosofía fundacional de NixLang, **las colecciones no constituyen rutas obligatorias, lineales ni bloqueantes**:
* No existen prerrequisitos forzados entre lecciones de una colección.
* El usuario puede entrar a una colección y realizar **cualquier lección directamente** en el momento que desee.
* Una lección completada dentro de una colección refleja su progreso en toda la plataforma (y viceversa).
* Una misma lección puede formar parte de más de una colección simultáneamente (relación N:M).

---

## 2. Diferencia entre Categorías y Colecciones

| Dimensión | Categoría | Colección |
| :--- | :--- | :--- |
| **Enfoque** | Taxonómico / Temático abstracto (¿De qué trata el contenido lingüístico?). | Experiencial / Intencional / Orientado a metas (¿Para qué situación real me sirve?). |
| **Ejemplos** | *Gramática*, *Vocabulario*, *Pronunciación*, *Negocios*, *Viajes*. | *Supervivencia en el Aeropuerto*, *Inglés para Developers*, *Series & Sitcoms*. |
| **Estructura** | Conjunto plano de lecciones sin orden relativo intrínseco. | Conjunto de lecciones con una **secuencia pedagógica sugerida** (Paso 1, Paso 2...). |
| **Experiencia UI** | Filtro secundario o etiqueta clasificatoria. | Tarjeta de experiencia con portada, descripción, progreso derivado y visor de lecciones. |
| **Progreso visible** | No mide avance conjunto directo. | Muestra porcentaje de avance acumulado del usuario dentro de la colección. |

---

## 3. Características Funcionales de una Colección

Una colección:
1. **Posee identidad propia:** Título claro, descripción contextualizada, icono o imagen alusiva y nivel sugerido orientativo (opcional).
2. **Contiene lecciones ordenadas:** Define un orden de presentación recomendado por el autor/pedagogo.
3. **No impone restricciones de acceso:** Todas sus lecciones están abiertas desde el inicio.
4. **Muestra progreso derivado:** Calcula dinámicamente cuántas lecciones han sido completadas por el usuario autenticado (ej. `3 de 5 lecciones completadas — 60%`).
5. **Tiene ciclo de publicación:** Estados `Borrador` (`Draft`), `Publicada` (`Published`) y `Desactivada` (`Disabled`). Solo las colecciones publicadas y sus lecciones publicadas se exponen al estudiante.
6. **Mantiene independencia de contenido:** Eliminar una colección no elimina las lecciones asociadas del catálogo.

---

## 4. Experiencia del Usuario (Frontend)

### 4.1 Descubrimiento y Catálogo
* **Vista de Colecciones:** Sección destacada en el Catálogo de NixLang donde el estudiante puede explorar tarjetas de colecciones con su título, descripción, icono, nivel sugerido y barra de progreso personal.
* **Filtrado:** El usuario puede filtrar o buscar colecciones por texto y por nivel orientativo (A1, A2, B1, B2).

### 4.2 Detalle de Colección
Al hacer clic en una tarjeta de colección, el usuario accede a la vista detallada:
* **Cabecera:** Título, descripción del objetivo de aprendizaje, nivel referencial y progreso consolidado.
* **Lista de Lecciones:** Tarjetas de lecciones organizadas en la secuencia sugerida. Cada tarjeta muestra:
  * Número de orden sugerido (opcional / orientativo).
  * Título y descripción de la lección.
  * Nivel CEFR de la lección.
  * Estado de progreso individual del usuario (`No iniciada`, `En progreso`, `Completada`) y porcentaje.
  * Botón de acceso directo para iniciar o continuar la lección de inmediato.
  * Indicador y botón de favorito.

### 4.3 Flujo de Navegación
```text
Catálogo de Colecciones
       ↓ (Selecciona colección)
Detalle de Colección (Muestra progreso derivado y lista de lecciones)
       ↓ (Selecciona CUALQUIER lección libremente)
Visor interactivo de Lección (/lessons/play/:id)
       ↓ (Al completar lección)
Retorno a Detalle de Colección (Progreso derivado actualizado automáticamente)
```

---

## 5. Gestión Administrativa (Panel de Admin)

El usuario con rol `ADMINISTRADOR` o `ADMIN` dispone de herramientas completas para:
1. **Crear Colección:** Ingresar título, descripción, icono/imagen, nivel sugerido opcional y estado inicial.
2. **Editar Colección:** Modificar datos generales, cambiar estado de publicación (`Draft` ↔ `Published` ↔ `Disabled`).
3. **Asignar y Ordenar Lecciones:** 
   * Seleccionar lecciones del catálogo global para vincularlas a la colección.
   * Modificar el orden secuencial sugerido de las lecciones (arriba/abajo o drag-and-drop).
   * Desvincular lecciones sin eliminarlas de la base de datos.
4. **Eliminar Colección:** Elimina la agrupación y sus enlaces de orden (`CollectionLessons`), respetando la integridad de las lecciones.

---

## 6. Reglas de Negocio Asociadas

* **RN-35:** Una Colección agrupa lecciones bajo un tema, propósito, contexto o experiencia de aprendizaje común.
* **RN-36:** Las colecciones no constituyen una ruta obligatoria ni bloqueante. El usuario puede iniciar cualquier lección perteneciente a una colección directamente y en cualquier orden.
* **RN-37:** Una lección puede pertenecer simultáneamente a múltiples colecciones y a múltiples categorías.
* **RN-38:** Una colección define un orden sugerido/secuencial para sus lecciones con fines pedagógicos y de presentación, pero el acceso a ellas es completamente libre.
* **RN-39:** El progreso de una colección se calcula de forma derivada en función del progreso individual de las lecciones que la componen.
* **RN-40:** Una colección puede tener estados de publicación: Borrador, Publicada o Desactivada. Solo colecciones publicadas y sus lecciones publicadas son visibles para el estudiante.
* **RN-41:** Al eliminar una colección, se desasocian sus lecciones vinculadas sin eliminarlas del catálogo general.

# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 2.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

---

# 4. Relaciones entre Entidades

## 4.1 Mapa de Relaciones

| #   | Entidad Origen     | Relación                                | Entidad Destino               |      Cardinalidad     |
| --- | ------------------ | --------------------------------------- | ----------------------------- | :-------------------: |
| R1  | Lección            | *contiene*                              | LessonBlock                   |         1 : N         |
| R2  | LessonBlock        | *utiliza*                               | Ejercicio                     |         0 : 1         |
| R3  | Ejercicio          | *tiene alternativas*                    | AlternativaEjercicio          |         1 : N         |
| R4  | Lección            | *clasificada en (vía LecciónCategoría)* | Categoría                     |         N : M         |
| R5  | Usuario            | *marca como favorita (vía Favorito)*    | Lección                       |         N : M         |
| R6  | Usuario            | *registra progreso en*                  | Lección (vía ProgresoLección) | N : M (con atributos) |
| R7  | ProgresoLección    | *contiene resultados de*                | ResultadoEjercicio            |         1 : N         |
| R8  | ResultadoEjercicio | *corresponde a*                         | Ejercicio                     |         N : 1         |
| R9  | Usuario            | *genera*                                | GrabaciónAudio                |         1 : N         |
| R10 | Ejercicio          | *recibe grabaciones de*                 | GrabaciónAudio                |         1 : N         |
| R11 | Usuario            | *genera*                                | RegistroActividad             |         1 : N         |
| R12 | Colección          | *agrupa (vía ColecciónLección)*         | Lección                       |     N : M (ordenado)  |

---

## 4.2 Descripción Detallada de Relaciones

### R1 — Lección ↔ LessonBlock `[1 : N]`

Una lección **contiene** uno o más `LessonBlocks`, organizados en un orden determinado. Cada `LessonBlock` pertenece exclusivamente a una única lección.

---

### R2 — LessonBlock ↔ Ejercicio `[0 : 1]`

Un `LessonBlock` puede **utilizar** un ejercicio cuando representa una actividad evaluable. Los bloques que muestran contenido informativo no requieren asociarse a un ejercicio.

Cada ejercicio es utilizado por un único `LessonBlock`.

---

### R3 — Ejercicio ↔ AlternativaEjercicio `[1 : N]`

Un ejercicio de opción múltiple **tiene** dos o más alternativas. Al menos una de ellas debe ser correcta. Las alternativas existen únicamente ligadas al ejercicio correspondiente.

---

### R4 — Lección ↔ Categoría (vía LecciónCategoría) `[N : M]`

Una lección puede pertenecer a una o más categorías temáticas y una categoría puede agrupar múltiples lecciones. La relación se implementa mediante la entidad asociativa `LecciónCategoría`.

---

### R5 — Usuario ↔ Lección (vía Favorito) `[N : M]`

Un usuario puede marcar múltiples lecciones como favoritas y una lección puede ser marcada como favorita por múltiples usuarios. La entidad `Favorito` registra dicha asociación.

---

### R6 — Usuario ↔ Lección (Progreso) `[N : M con atributos]`

Un usuario registra su progreso al realizar una lección. Cada intento genera un registro independiente de `ProgresoLección`.

---

### R7 — ProgresoLección ↔ ResultadoEjercicio `[1 : N]`

Un registro de progreso contiene los resultados obtenidos durante la resolución de los ejercicios realizados en ese intento.

---

### R8 — ResultadoEjercicio ↔ Ejercicio `[N : 1]`

Cada resultado corresponde a un ejercicio específico. Un mismo ejercicio puede generar múltiples resultados a lo largo de distintos intentos realizados por diferentes usuarios.

---

### R9 y R10 — Usuario / Ejercicio ↔ GrabaciónAudio `[1 : N]`

Un usuario puede generar grabaciones de audio durante la resolución de ejercicios de pronunciación. Cada grabación queda asociada al ejercicio correspondiente.

---

### R11 — Usuario ↔ RegistroActividad `[1 : N]`

Un usuario genera registros de actividad que representan los eventos relevantes ocurridos durante el uso de la plataforma.

---

### R12 — Colección ↔ Lección (vía ColecciónLección) `[N : M ordenado]`

Una colección **agrupa** un conjunto ordenado de lecciones orientadas a un propósito o contexto formativo. Una lección puede pertenecer simultáneamente a múltiples colecciones. La entidad `ColecciónLección` preserva el orden de presentación sugerido dentro de la colección. El acceso a cada lección es independiente y directo.

---

## 4.3 Resumen Visual

```text
Usuario ──── 1 : N ──── ProgresoLección
Usuario ──── N : M ──── Lección              (vía Favorito)
Usuario ──── 1 : N ──── GrabaciónAudio
Usuario ──── 1 : N ──── RegistroActividad

Colección ─── N : M ──── Lección              (vía ColecciónLección, ordenado)

Lección ──── 1 : N ──── LessonBlock
LessonBlock ──── 0 : 1 ──── Ejercicio
Ejercicio ──── 1 : N ──── AlternativaEjercicio
Lección ──── N : M ──── Categoría            (vía LecciónCategoría)

ProgresoLección ──── 1 : N ──── ResultadoEjercicio
ResultadoEjercicio ──── N : 1 ──── Ejercicio
Ejercicio ──── 1 : N ──── GrabaciónAudio
```

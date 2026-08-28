# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 2.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

---

## 1. Lista de Entidades del Dominio

| #  | Entidad                  | Tipo               | Agregado al que pertenece  |
| -- | ------------------------ | ------------------ | -------------------------- |
| 1  | **Usuario**              | Entidad Raíz       | Agregado Usuario           |
| 2  | **Lección**              | Entidad Raíz       | Agregado Lección           |
| 3  | **LessonBlock**          | Entidad            | Agregado Lección           |
| 4  | **Ejercicio**            | Entidad            | Agregado Lección           |
| 5  | **AlternativaEjercicio** | Entidad            | Agregado Lección           |
| 6  | **Categoría**            | Entidad Raíz       | Agregado Categoría         |
| 7  | **Colección**            | Entidad Raíz       | Agregado Colección         |
| 8  | **ColecciónLección**     | Entidad            | Agregado Colección         |
| 9  | **ProgresoLección**      | Entidad Raíz       | Agregado Progreso          |
| 10 | **ResultadoEjercicio**   | Entidad            | Agregado Progreso          |
| 11 | **GrabaciónAudio**       | Entidad            | Agregado Progreso          |
| 12 | **RegistroActividad**    | Entidad            | Agregado Progreso          |
| 13 | **Favorito**             | Entidad Asociativa | Asociación entre agregados |
| 14 | **LecciónCategoría**     | Entidad Asociativa | Asociación entre agregados |

---

## Value Objects Identificados

| Value Object          | Descripción                                                                                                                                                                                                                                                                |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **NivelReferencial**  | Enumeración: `A1`, `A2`, `B1`, `B2`                                                                                                                                                                                                                                        |
| **TipoLessonBlock**   | Enumeración que identifica el tipo de bloque que compone una lección. Inicialmente contempla: `HEADING`, `PARAGRAPH`, `IMAGE`, `AUDIO`, `EXERCISE`, `FEEDBACK`, `REVIEW`, `SUMMARY` y `EDUCATIONAL_CONTENT`. La plataforma podrá incorporar nuevos tipos sin modificar la estructura del dominio. |
| **TipoEjercicio**     | Enumeración: `TRADUCCION`, `COMPLETAR_ESPACIOS`, `OPCION_MULTIPLE`, `PRONUNCIACION`                                                                                                                                                                                        |
| **EstadoPublicación** | Enumeración: `DRAFT`, `PUBLISHED`, `DISABLED` (aplica a Lección y Colección)                                                                                                                                                                                               |
| **EstadoProgreso**    | Enumeración: `NO_INICIADA`, `EN_PROGRESO`, `COMPLETADA`                                                                                                                                                                                                                    |
| **TipoActividad**     | Enumeración: `INICIO_SESION`, `INICIO_LECCION`, `RESPUESTA_EJERCICIO`, `COMPLETAR_LECCION`, `AGREGAR_FAVORITO`, `QUITAR_FAVORITO`                                                                                                                                          |
| **Rol**               | Enumeración: `USUARIO`, `ADMINISTRADOR`                                                                                                                                                                                                                                    |
| **CorreoElectrónico** | Cadena validada con formato de email, única en el sistema                                                                                                                                                                                                                  |
| **Contraseña**        | Cadena almacenada como hash seguro                                                                                                                                                                                                                                         |

---

## Consideraciones del Modelo

El núcleo del dominio evoluciona desde un modelo centrado en ejercicios hacia un modelo centrado en bloques de aprendizaje y colecciones de experiencias:

1. Una **Lección** deja de estar compuesta directamente por ejercicios y pasa a estar formada por una secuencia ordenada de **LessonBlocks**.
2. Cada **LessonBlock** representa una unidad de contenido o interacción dentro de la experiencia de aprendizaje (información, multimedia, ejercicios evaluables o contenido educativo independiente).
3. Una **Colección** agrupa lecciones bajo un **tema, propósito o contexto experiencial de la vida real** (ej. "Inglés para viajar", "Inglés para entrevistas laborales").
4. **Libertad de Aprendizaje**: Las colecciones organizan y guían el descubrimiento, pero **no imponen rutas obligatorias ni bloquean lecciones**. El estudiante puede ingresar a cualquier colección y resolver cualquier lección en cualquier orden.
5. El progreso de la colección se deriva dinámicamente del progreso de sus lecciones componentes, sin requerir entidades redundantes de seguimiento.


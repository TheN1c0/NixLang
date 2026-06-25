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
| 7  | **ProgresoLección**      | Entidad Raíz       | Agregado Progreso          |
| 8  | **ResultadoEjercicio**   | Entidad            | Agregado Progreso          |
| 9  | **GrabaciónAudio**       | Entidad            | Agregado Progreso          |
| 10 | **RegistroActividad**    | Entidad            | Agregado Progreso          |
| 11 | **Favorito**             | Entidad Asociativa | Asociación entre agregados |
| 12 | **LecciónCategoría**     | Entidad Asociativa | Asociación entre agregados |

---

## Value Objects Identificados

| Value Object          | Descripción                                                                                                                                                                                                                                                                |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **NivelReferencial**  | Enumeración: `A1`, `A2`, `B1`, `B2`                                                                                                                                                                                                                                        |
| **TipoLessonBlock**   | Enumeración que identifica el tipo de bloque que compone una lección. Inicialmente contempla: `HEADING`, `PARAGRAPH`, `IMAGE`, `AUDIO`, `EXERCISE`, `FEEDBACK`, `REVIEW` y `SUMMARY`. La plataforma podrá incorporar nuevos tipos sin modificar la estructura del dominio. |
| **TipoEjercicio**     | Enumeración: `TRADUCCION`, `COMPLETAR_ESPACIOS`, `OPCION_MULTIPLE`, `PRONUNCIACION`                                                                                                                                                                                        |
| **EstadoProgreso**    | Enumeración: `NO_INICIADA`, `EN_PROGRESO`, `COMPLETADA`                                                                                                                                                                                                                    |
| **TipoActividad**     | Enumeración: `INICIO_SESION`, `INICIO_LECCION`, `RESPUESTA_EJERCICIO`, `COMPLETAR_LECCION`, `AGREGAR_FAVORITO`, `QUITAR_FAVORITO`                                                                                                                                          |
| **Rol**               | Enumeración: `USUARIO`, `ADMINISTRADOR`                                                                                                                                                                                                                                    |
| **CorreoElectrónico** | Cadena validada con formato de email, única en el sistema                                                                                                                                                                                                                  |
| **Contraseña**        | Cadena almacenada como hash seguro                                                                                                                                                                                                                                         |

---

## Consideraciones del Modelo

El núcleo del dominio evoluciona desde un modelo centrado en ejercicios hacia un modelo centrado en bloques de aprendizaje.

Una **Lección** deja de estar compuesta directamente por ejercicios y pasa a estar formada por una secuencia ordenada de **LessonBlocks**.

Cada **LessonBlock** representa una unidad de contenido o interacción dentro de la experiencia de aprendizaje.

Dependiendo de su tipo, un bloque puede:

* mostrar información;
* presentar imágenes;
* reproducir audio;
* solicitar una interacción al usuario;
* presentar un ejercicio;
* entregar retroalimentación;
* realizar un repaso;
* mostrar un resumen.

Los **Ejercicios** continúan existiendo como entidad del dominio, pero dejan de ser la unidad estructural de una lección. En adelante representan únicamente la lógica de una actividad evaluable, la cual puede ser utilizada por un `LessonBlock` de tipo `EXERCISE`.

Este enfoque permite incorporar nuevos tipos de bloques sin modificar la estructura fundamental de las lecciones ni del agregado correspondiente.

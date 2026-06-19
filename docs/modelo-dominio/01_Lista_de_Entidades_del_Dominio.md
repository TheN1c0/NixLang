# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 1.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

---

## 1. Lista de Entidades del Dominio

| #  | Entidad                    | Tipo                | Agregado al que pertenece  |
|----|----------------------------|---------------------|----------------------------|
| 1  | **Usuario**                | Entidad Raíz        | Agregado Usuario           |
| 2  | **Lección**                | Entidad Raíz        | Agregado Lección           |
| 3  | **Ejercicio**              | Entidad             | Agregado Lección           |
| 4  | **AlternativaEjercicio**   | Entidad             | Agregado Lección           |
| 5  | **Categoría**              | Entidad Raíz        | Agregado Categoría         |
| 6  | **ProgresoLección**        | Entidad Raíz        | Agregado Progreso          |
| 7  | **ResultadoEjercicio**     | Entidad             | Agregado Progreso          |
| 8  | **GrabaciónAudio**         | Entidad             | Agregado Progreso          |
| 9  | **RegistroActividad**      | Entidad             | Agregado Progreso          |
| 10 | **Favorito**               | Entidad Asociativa  | Asociación entre agregados |
| 11 | **LecciónCategoría**       | Entidad Asociativa  | Asociación entre agregados |

### Value Objects Identificados

| Value Object           | Descripción                                                   |
|------------------------|---------------------------------------------------------------|
| **NivelReferencial**   | Enumeración: `A1`, `A2`, `B1`, `B2`                          |
| **TipoEjercicio**      | Enumeración: `TRADUCCION`, `COMPLETAR_ESPACIOS`, `OPCION_MULTIPLE`, `PRONUNCIACION` |
| **EstadoProgreso**     | Enumeración: `NO_INICIADA`, `EN_PROGRESO`, `COMPLETADA`      |
| **TipoActividad**      | Enumeración: `INICIO_SESION`, `INICIO_LECCION`, `RESPUESTA_EJERCICIO`, `COMPLETAR_LECCION`, `AGREGAR_FAVORITO`, `QUITAR_FAVORITO` |
| **Rol**                | Enumeración: `USUARIO`, `ADMINISTRADOR`                       |
| **CorreoElectrónico**  | Cadena validada con formato de email, única en el sistema     |
| **Contraseña**         | Cadena almacenada como hash seguro                            |

---

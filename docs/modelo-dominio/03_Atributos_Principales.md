# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 2.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

---

# 3. Atributos Principales

## 3.1 Usuario

| Atributo            | Tipo              |
| ------------------- | ----------------- |
| `id`                | Identificador     |
| `nombre`            | Texto             |
| `correoElectrónico` | CorreoElectrónico |
| `contraseña`        | Contraseña        |
| `rol`               | Rol               |
| `fechaRegistro`     | FechaHora         |

---

## 3.2 Lección

| Atributo             | Tipo              |
| -------------------- | ----------------- |
| `id`                 | Identificador     |
| `título`             | Texto             |
| `descripción`        | Texto             |
| `nivelReferencial`   | NivelReferencial  |
| `estado`             | EstadoPublicación |
| `fechaCreación`      | FechaHora         |
| `fechaActualización` | FechaHora         |

---

## 3.3 LessonBlock

| Atributo        | Tipo                     |
| --------------- | ------------------------ |
| `id`            | Identificador            |
| `tipo`          | TipoLessonBlock          |
| `orden`         | Entero                   |
| `configuración` | ConfiguraciónBloque      |
| `ejercicioId`   | Identificador (Opcional) |

> **📝 NOTA:** La configuración del bloque contiene la información necesaria para representar el contenido y el comportamiento correspondiente a su tipo. Solo los bloques de tipo `EXERCISE` requieren una referencia a un `Ejercicio`.

---

## 3.4 Ejercicio

| Atributo            | Tipo          |
| ------------------- | ------------- |
| `id`                | Identificador |
| `tipo`              | TipoEjercicio |
| `enunciado`         | Texto         |
| `respuestaCorrecta` | Texto         |
| `recursoAudioUrl`   | URL           |

> **📝 NOTA:** La `respuestaCorrecta` aplica para ejercicios de tipo `TRADUCCION` y `COMPLETAR_ESPACIOS`. Para `OPCION_MULTIPLE`, la corrección se define a través de sus alternativas. Para `PRONUNCIACION`, la evaluación puede seguir criterios distintos.

---

## 3.5 AlternativaEjercicio

| Atributo     | Tipo          |
| ------------ | ------------- |
| `id`         | Identificador |
| `texto`      | Texto         |
| `esCorrecta` | Booleano      |
| `orden`      | Entero        |

---

## 3.6 Categoría

| Atributo      | Tipo          |
| ------------- | ------------- |
| `id`          | Identificador |
| `nombre`      | Texto         |
| `descripción` | Texto         |

---

## 3.7 ProgresoLección

| Atributo            | Tipo           |
| ------------------- | -------------- |
| `id`                | Identificador  |
| `estado`            | EstadoProgreso |
| `porcentajeAvance`  | Decimal        |
| `fechaInicio`       | FechaHora      |
| `fechaFinalización` | FechaHora      |

---

## 3.8 ResultadoEjercicio

| Atributo         | Tipo          |
| ---------------- | ------------- |
| `id`             | Identificador |
| `respuestaDada`  | Texto         |
| `esCorrecta`     | Booleano      |
| `fechaRespuesta` | FechaHora     |

---

## 3.9 GrabaciónAudio

| Atributo         | Tipo          |
| ---------------- | ------------- |
| `id`             | Identificador |
| `archivoUrl`     | URL           |
| `fechaGrabación` | FechaHora     |

---

## 3.10 RegistroActividad

| Atributo        | Tipo          |
| --------------- | ------------- |
| `id`            | Identificador |
| `tipoActividad` | TipoActividad |
| `referenciaId`  | Identificador |
| `fechaHora`     | FechaHora     |

---

## 3.11 Favorito

| Atributo         | Tipo          |
| ---------------- | ------------- |
| `id`             | Identificador |
| `fechaMarcación` | FechaHora     |

---

## 3.12 LecciónCategoría

| Atributo | Tipo          |
| -------- | ------------- |
| `id`     | Identificador |

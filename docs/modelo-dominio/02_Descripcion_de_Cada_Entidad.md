# Modelo de Dominio Conceptual — NixLang

> **Plataforma de Aprendizaje de Inglés para Hispanohablantes**
> Versión del Modelo: 2.0 · Junio 2025
> Basado en: Documentación de Producto V1, Requerimientos Funcionales (RF-001 a RF-047), Decisiones de Negocio (DN-001 a DN-007), Historias de Usuario (HU-001 a HU-139)

---

# 2. Descripción de Cada Entidad

## 2.1 Usuario

Persona registrada en la plataforma NixLang.

Representa al actor principal del sistema y puede desempeñar el rol de usuario regular, quien consume el contenido educativo y registra su progreso, o de administrador, responsable de gestionar el contenido disponible en la plataforma.

---

## 2.2 Lección

Unidad pedagógica principal de la plataforma.

Representa una experiencia de aprendizaje diseñada para alcanzar un objetivo específico, como aprender un concepto gramatical, adquirir vocabulario o desarrollar una habilidad determinada.

Cada lección constituye una unidad independiente dentro del catálogo y puede ser realizada por el usuario cuando lo estime conveniente.

---

## 2.3 LessonBlock

Unidad mínima de contenido que compone una lección.

Representa un elemento de la experiencia de aprendizaje y encapsula el contenido o la interacción que será presentada al usuario.

Su comportamiento depende del tipo de bloque que represente, permitiendo construir experiencias de aprendizaje flexibles y extensibles.

---

## 2.4 Ejercicio

Actividad evaluable utilizada para comprobar el aprendizaje del usuario.

Define la lógica necesaria para validar una respuesta y determinar el resultado obtenido, independientemente de la forma en que sea presentada dentro de una lección.

Inicialmente la plataforma contempla ejercicios de traducción, completar espacios, opción múltiple y pronunciación.

---

## 2.5 AlternativaEjercicio

Posible respuesta disponible para un ejercicio de opción múltiple.

Permite representar las distintas opciones que el usuario puede seleccionar durante la resolución de una actividad evaluable.

---

## 2.6 Categoría

Agrupación temática utilizada para clasificar y organizar las lecciones disponibles en el catálogo.

Su propósito es facilitar la organización del contenido y mejorar la experiencia de búsqueda y exploración por parte del usuario.

---

## 2.7 ProgresoLección

Representa el avance de un usuario durante la realización de una lección.

Permite conocer el estado del aprendizaje, el porcentaje de avance alcanzado y la información necesaria para realizar el seguimiento de cada intento.

---

## 2.8 ResultadoEjercicio

Representa el resultado obtenido al responder una actividad evaluable.

Almacena la información necesaria para determinar el desempeño alcanzado por el usuario durante el proceso de aprendizaje.

---

## 2.9 GrabaciónAudio

Representa un recurso de audio generado por el usuario durante una actividad que requiere pronunciación.

Permite conservar la evidencia correspondiente a la interacción realizada.

---

## 2.10 RegistroActividad

Representa un evento relevante generado durante el uso de la plataforma.

Su finalidad es proporcionar información para auditoría, métricas de uso y análisis del comportamiento de la plataforma.

---

## 2.11 Favorito

Representa la preferencia de un usuario por una lección determinada.

Permite construir una colección personalizada de contenido de interés para facilitar su acceso posterior.

---

## 2.12 LecciónCategoría

Representa la asociación utilizada para organizar las lecciones dentro de las categorías temáticas del catálogo.

Su finalidad es mantener una clasificación flexible del contenido educativo.

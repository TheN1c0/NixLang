# NixLang – Requerimientos No Funcionales (RNF)

Versión: 1.0
Estado: Aprobado para MVP
Fecha: Junio 2026

## 1. Introducción

Este documento define los Requerimientos No Funcionales de NixLang, una plataforma web para el aprendizaje de inglés orientada a hispanohablantes. Los RNF establecen los criterios de calidad, rendimiento, seguridad, disponibilidad, mantenibilidad y operación que deberá cumplir el sistema durante su desarrollo y explotación.

---

# 2. Rendimiento

### RNF-001 – Tiempo de Respuesta General

**Prioridad:** MVP

El sistema deberá responder las solicitudes de navegación, consulta de perfil, progreso y favoritos en un tiempo inferior a 3 segundos bajo condiciones normales de operación.

### RNF-002 – Búsquedas y Filtros

**Prioridad:** MVP

Las búsquedas de lecciones y la aplicación de filtros deberán completarse en menos de 2 segundos.

### RNF-003 – Apertura de Lecciones

**Prioridad:** MVP

La carga de una lección deberá completarse en menos de 2 segundos.

### RNF-004 – Inicio de Sesión

**Prioridad:** MVP

La autenticación deberá percibirse como inmediata por parte del usuario, con una respuesta objetivo inferior a 1 segundo.

---

# 3. Disponibilidad

### RNF-005 – Operación Continua

**Prioridad:** MVP

La plataforma deberá estar diseñada para operar 24 horas al día, 7 días a la semana.

### RNF-006 – Disponibilidad del Servicio

**Prioridad:** MVP

La indisponibilidad acumulada no deberá superar 8 horas por mes calendario.

### RNF-007 – Recuperación Operativa

**Prioridad:** MVP

La plataforma deberá contar con mecanismos que permitan restaurar el servicio mediante respaldos en caso de fallo crítico.

---

# 4. Escalabilidad

### RNF-008 – Escalabilidad Inicial

**Prioridad:** MVP

La arquitectura deberá soportar al menos 50 usuarios concurrentes sin degradación significativa del rendimiento.

### RNF-009 – Crecimiento Futuro

**Prioridad:** Futuro

La arquitectura deberá permitir la incorporación de nuevos módulos funcionales sin requerir rediseños completos del sistema.

### RNF-010 – Evolución Arquitectónica

**Prioridad:** Futuro

La solución deberá estar organizada en módulos desacoplados que permitan una futura migración parcial hacia arquitecturas distribuidas o microservicios.

---

# 5. Seguridad

### RNF-011 – Comunicación Segura

**Prioridad:** MVP

Toda comunicación entre cliente y servidor deberá realizarse mediante HTTPS.

### RNF-012 – Almacenamiento de Contraseñas

**Prioridad:** MVP

Las contraseñas nunca deberán almacenarse en texto plano y deberán protegerse mediante algoritmos de hash seguros.

### RNF-013 – Verificación de Correo Electrónico

**Prioridad:** MVP

Los usuarios deberán verificar su dirección de correo electrónico antes de activar su cuenta.

### RNF-014 – Control de Acceso

**Prioridad:** MVP

El sistema deberá restringir el acceso a recursos protegidos según el rol del usuario.

### RNF-015 – Gestión de Roles

**Prioridad:** MVP

La plataforma deberá soportar al menos los roles Usuario y Administrador.

### RNF-016 – Protección contra Amenazas Comunes

**Prioridad:** MVP

La aplicación deberá implementar medidas de protección contra ataques comunes, incluyendo:

- Inyección SQL.
- Cross-Site Scripting (XSS).
- Cross-Site Request Forgery (CSRF).
- Acceso no autorizado.

---

# 6. Privacidad y Protección de Datos

### RNF-017 – Datos Personales

**Prioridad:** MVP

El sistema podrá almacenar:

- Nombre.
- Correo electrónico.
- País.
- Idioma nativo.
- Fecha de nacimiento.

### RNF-018 – Eliminación de Cuenta

**Prioridad:** MVP

El usuario podrá solicitar la eliminación permanente de su cuenta y sus datos asociados.

### RNF-019 – Registro de Último Acceso

**Prioridad:** MVP

El sistema deberá registrar la fecha y hora del último inicio de sesión de cada usuario.

### RNF-020 – Conservación de Datos

**Prioridad:** MVP

Los datos de usuario se conservarán mientras la cuenta permanezca activa.

### RNF-021 – Gestión de Cuentas Inactivas

**Prioridad:** Futuro

La plataforma podrá implementar políticas automáticas de eliminación o depuración de cuentas inactivas.

---

# 7. Compatibilidad

### RNF-022 – Diseño Responsive

**Prioridad:** MVP

La interfaz deberá adaptarse correctamente a dispositivos móviles y de escritorio.

### RNF-023 – Navegadores Soportados

**Prioridad:** MVP

La plataforma deberá ser compatible con las versiones modernas de:

- Google Chrome.
- Microsoft Edge.
- Mozilla Firefox.
- Safari.

### RNF-024 – Navegadores Obsoletos

**Prioridad:** MVP

No se garantiza compatibilidad con navegadores obsoletos o fuera de soporte.

---

# 8. Usabilidad

### RNF-025 – Facilidad de Uso

**Prioridad:** MVP

La interfaz deberá permitir que un usuario nuevo complete una lección sin necesidad de capacitación previa.

### RNF-026 – Consistencia Visual

**Prioridad:** MVP

Los componentes visuales deberán mantener comportamiento y apariencia consistentes en toda la aplicación.

### RNF-027 – Idiomas

**Prioridad:** MVP

La interfaz estará disponible inicialmente en español.

### RNF-028 – Accesibilidad

**Prioridad:** Futuro

La plataforma deberá evolucionar progresivamente hacia el cumplimiento de estándares básicos de accesibilidad web.

---

# 9. Mantenibilidad

### RNF-029 – Arquitectura

**Prioridad:** MVP

La solución deberá implementarse utilizando principios de Clean Architecture.

### RNF-030 – Principios de Diseño

**Prioridad:** MVP

El código deberá respetar los principios SOLID.

### RNF-031 – Calidad de Código

**Prioridad:** MVP

El proyecto deberá incorporar análisis estático automatizado durante el proceso de desarrollo.

### RNF-032 – Pruebas Unitarias

**Prioridad:** MVP

Los componentes críticos del sistema deberán contar con pruebas unitarias.

### RNF-033 – Pruebas de Integración

**Prioridad:** MVP

Los flujos principales del sistema deberán contar con pruebas de integración.

### RNF-034 – Integración Continua

**Prioridad:** MVP

El sistema deberá disponer de un pipeline de CI/CD para automatizar validaciones y despliegues.

---

# 10. Infraestructura

### RNF-035 – Tecnologías Base

**Prioridad:** MVP

La plataforma utilizará:

- Angular.
- ASP.NET Core Web API.
- PostgreSQL.

### RNF-036 – Contenedorización

**Prioridad:** MVP

La aplicación deberá poder desplegarse mediante contenedores Docker.

### RNF-037 – Portabilidad

**Prioridad:** MVP

La solución deberá ser portable entre distintos proveedores de infraestructura.

### RNF-038 – Evolución Cloud

**Prioridad:** Futuro

La arquitectura deberá permitir despliegues sobre Microsoft Azure.

---

# 11. Observabilidad

### RNF-039 – Registro de Eventos

**Prioridad:** MVP

La aplicación deberá registrar eventos de información, advertencia y error.

### RNF-040 – Monitoreo

**Prioridad:** MVP

La plataforma deberá monitorear:

- Disponibilidad.
- Tiempo de respuesta.
- Uso de CPU.
- Uso de memoria.
- Errores del sistema.

### RNF-041 – Alertas

**Prioridad:** MVP

La plataforma deberá generar alertas por correo electrónico ante:

- Caída de la API.
- Errores críticos.
- Indisponibilidad de la base de datos.

---

# 12. Respaldo y Recuperación

### RNF-042 – Frecuencia de Respaldo

**Prioridad:** MVP

La base de datos deberá respaldarse automáticamente cada 12 horas.

### RNF-043 – Retención de Respaldo

**Prioridad:** MVP

Los respaldos deberán conservarse durante un mínimo de 7 días.

### RNF-044 – Restauración

**Prioridad:** MVP

La plataforma deberá permitir la restauración de respaldos válidos en caso de fallo operativo.

---

# 13. Funcionalidades Futuras

### RNF-045 – Audio y Pronunciación

**Prioridad:** Futuro

Los requerimientos asociados a grabación, reproducción y almacenamiento de audio serán definidos en una versión posterior del sistema.

### RNF-046 – Escalamiento Empresarial

**Prioridad:** Futuro

La arquitectura deberá permitir evolucionar hacia modelos freemium, suscripción y crecimiento de usuarios sin rediseño completo de la plataforma.

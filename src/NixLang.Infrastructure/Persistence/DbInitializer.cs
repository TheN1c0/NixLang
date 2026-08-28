using Microsoft.EntityFrameworkCore;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.ValueObjects;

namespace NixLang.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(NixLangDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Apply any pending EF Core migrations automatically
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        // 2. Ensure Default Admin and Demo Users exist with password "Password123!"
        var adminEmail = Email.Create("admin@nixlang.com");
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var passwordHash = passwordHasher.HashPassword("Password123!");

        if (existingAdmin == null)
        {
            var adminUser = new User("Administrador NixLang", adminEmail, passwordHash);
            adminUser.UpdateRole(UserRole.Administrator);
            context.Users.Add(adminUser);
        }
        else
        {
            // Ensure proper role and password
            adminUserPasswordSync(existingAdmin, passwordHash);
        }

        var studentEmail = Email.Create("demo@nixlang.com");
        var existingStudent = await context.Users.FirstOrDefaultAsync(u => u.Email == studentEmail);
        if (existingStudent == null)
        {
            var studentUser = new User("Nico Demo", studentEmail, passwordHash);
            context.Users.Add(studentUser);
        }

        // 3. Seed collections and lessons if not already present
        if (await context.Collections.AnyAsync())
        {
            return;
        }

        // 4. Create Categories
        var catFundamentals = new Category("Fundamentos y Vocabulario", "Vocabulario básico, saludos y estructuras iniciales para la vida diaria.");
        var catBusiness = new Category("Inglés Profesional", "Comunicación para el entorno laboral, gestión de proyectos y colaboración.");
        context.Categories.AddRange(catFundamentals, catBusiness);
        await context.SaveChangesAsync();

        // -------------------------------------------------------------
        // COLECCIÓN 1: Supervivencia en Inglés: Fundamentos (Nivel A1)
        // -------------------------------------------------------------
        var col1 = new Collection(
            "Supervivencia en Inglés: Fundamentos y Primeros Pasos",
            "Aprende las frases y estructuras esenciales desde cero para desenvolverte en situaciones reales: saludos, pedir con cortesía y ubicarte.",
            null,
            ReferenceLevel.A1,
            1);

        // --- Lección 1.1: Saludos y Presentaciones ---
        var l1_1 = new Lesson(
            "Saludos y Presentaciones (Hello, I am...)",
            "Aprende a romper el hielo, saludar cordialmente y presentarte en cualquier situación sin nervios.",
            ReferenceLevel.A1);
        l1_1.AddCategory(catFundamentals);

        var c1_1_vocab = new EducationalContent(
            "Palabras Clave: Saludos y Cortesía",
            "Los saludos más comunes para iniciar cualquier interacción social o casual.",
            "• Hello / Hi = Hola (Hi es más casual y muy usado)\n• Good morning = Buenos días\n• Please = Por favor\n• Thank you / Thanks = Gracias\n• Nice to meet you = Mucho gusto / Encantado de conocerte",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A1);
        c1_1_vocab.Publish();

        var c1_1_rule = new EducationalContent(
            "Estructura: Cómo decir tu nombre",
            "Dos formas sencillas y naturales de presentarte.",
            "Para presentarte puedes usar cualquiera de estas dos fórmulas:\n1. 'I am [Tu Nombre]' (Yo soy...)\n2. 'My name is [Tu Nombre]' (Mi nombre es...)\n\nEjemplo en diálogo:\n— 'Hello! I am Alex. Nice to meet you.'\n— 'Hi Alex! My name is Maria.'",
            EducationalContentType.Rule,
            ReferenceLevel.A1);
        c1_1_rule.Publish();

        var ex1_1_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "¿Cuál es la forma más natural y común de decir 'Mucho gusto en conocerte' en inglés?",
            "Nice to meet you");
        ex1_1_mc.AddOption("Nice to meet you", true, 1);
        ex1_1_mc.AddOption("Good morning to you", false, 2);
        ex1_1_mc.AddOption("Thank you very much", false, 3);
        ex1_1_mc.AddOption("Please see you", false, 4);

        var ex1_1_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la presentación: Hello! My ___ is Alex.",
            "name");

        var ex1_1_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: 'Hola, yo soy David'",
            "Hello, I am David");

        context.EducationalContents.AddRange(c1_1_vocab, c1_1_rule);
        context.Exercises.AddRange(ex1_1_mc, ex1_1_fb, ex1_1_tr);
        await context.SaveChangesAsync();

        l1_1.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_1.Id, LessonBlockType.Heading, 1, new BlockConfiguration("¡Bienvenido a tu primer paso en inglés!")));
        l1_1.AddLessonBlock(LessonBlock.CreateContentBlock(l1_1.Id, 2, c1_1_vocab.Id));
        l1_1.AddLessonBlock(LessonBlock.CreateContentBlock(l1_1.Id, 3, c1_1_rule.Id));
        l1_1.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_1.Id, LessonBlockType.Paragraph, 4, new BlockConfiguration("Ahora pondremos a prueba tu comprensión con unos ejercicios interactivos breves y directos.")));
        l1_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_1.Id, 5, ex1_1_mc.Id));
        l1_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_1.Id, 6, ex1_1_fb.Id));
        l1_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_1.Id, 7, ex1_1_tr.Id));
        l1_1.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_1.Id, LessonBlockType.Summary, 8, new BlockConfiguration("¡Excelente trabajo! Ya sabes saludar y presentarte con naturalidad usando 'I am' o 'My name is'.")));
        l1_1.Publish();

        // --- Lección 1.2: Pedir con Cortesía en Cafeterías y Restaurantes ---
        var l1_2 = new Lesson(
            "Pedir con Cortesía en Cafeterías y Restaurantes",
            "Domina la fórmula indispensable para ordenar bebidas, comida y pedir la cuenta como un nativo.",
            ReferenceLevel.A1);
        l1_2.AddCategory(catFundamentals);

        var c1_2_vocab = new EducationalContent(
            "Palabras Clave: En la Cafetería",
            "Vocabulario básico para ordenar lo que necesitas.",
            "• Coffee = Café\n• Water = Agua\n• Tea = Té\n• The check / The bill = La cuenta\n• A cup of... = Una taza de...",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A1);
        c1_2_vocab.Publish();

        var c1_2_tip = new EducationalContent(
            "La Fórmula Mágica de Cortesía: 'Can I have...'",
            "En inglés nunca se traduce literalmente 'dame' (give me), se pide con 'Can I have...'",
            "En español solemos decir 'Me da un café' o 'Quiero un agua'. En inglés, decir 'I want' o 'Give me' suena muy brusco o demandante.\n\nEn su lugar, usa siempre:\n👉 'Can I have a coffee, please?' (¿Me da un café, por favor?)\n👉 'Can I have the check, please?' (¿Me trae la cuenta, por favor?)",
            EducationalContentType.Tip,
            ReferenceLevel.A1);
        c1_2_tip.Publish();

        var ex1_2_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "¿Cómo pides un agua de forma educada en una cafetería o restaurante?",
            "Can I have a water, please?");
        ex1_2_mc.AddOption("Can I have a water, please?", true, 1);
        ex1_2_mc.AddOption("Give me water right now", false, 2);
        ex1_2_mc.AddOption("I want water yes", false, 3);
        ex1_2_mc.AddOption("Where water please", false, 4);

        var ex1_2_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la orden: Can I ___ a coffee, please?",
            "have");

        var ex1_2_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: 'La cuenta, por favor'",
            "The check, please");

        context.EducationalContents.AddRange(c1_2_vocab, c1_2_tip);
        context.Exercises.AddRange(ex1_2_mc, ex1_2_fb, ex1_2_tr);
        await context.SaveChangesAsync();

        l1_2.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_2.Id, LessonBlockType.Heading, 1, new BlockConfiguration("Ordenar bebidas y comida con total cortesía")));
        l1_2.AddLessonBlock(LessonBlock.CreateContentBlock(l1_2.Id, 2, c1_2_vocab.Id));
        l1_2.AddLessonBlock(LessonBlock.CreateContentBlock(l1_2.Id, 3, c1_2_tip.Id));
        l1_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_2.Id, 4, ex1_2_mc.Id));
        l1_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_2.Id, 5, ex1_2_fb.Id));
        l1_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_2.Id, 6, ex1_2_tr.Id));
        l1_2.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_2.Id, LessonBlockType.Summary, 7, new BlockConfiguration("¡Genial! Dominas el patrón 'Can I have [objeto], please?', la estructura más usada en cualquier país de habla inglesa.")));
        l1_2.Publish();

        // --- Lección 1.3: Preguntar por Direcciones y Lugares ---
        var l1_3 = new Lesson(
            "Preguntar por Direcciones y Lugares (Where is...?)",
            "Aprende a ubicarte en una ciudad, pedir indicaciones y encontrar el baño, la estación o tu hotel.",
            ReferenceLevel.A1);
        l1_3.AddCategory(catFundamentals);

        var c1_3_vocab = new EducationalContent(
            "Palabras Clave: Lugares Comunes y Cortesía",
            "Espacios indispensables cuando viajas o te desplazas.",
            "• Restroom / Bathroom = Baño / Servicios\n• Station = Estación (de tren o metro)\n• Hotel = Hotel\n• Airport = Aeropuerto\n• Excuse me = Disculpe / Con permiso",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A1);
        c1_3_vocab.Publish();

        var c1_3_concept = new EducationalContent(
            "El Patrón: 'Excuse me, where is the...?'",
            "Cómo llamar la atención educadamente y preguntar una ubicación.",
            "Estructura universal:\n'Excuse me, where is the [lugar]?'\n\nEjemplos:\n• 'Excuse me, where is the bathroom?' (Disculpe, ¿dónde está el baño?)\n• 'Excuse me, where is the station?' (Disculpe, ¿dónde está la estación?)",
            EducationalContentType.Concept,
            ReferenceLevel.A1);
        c1_3_concept.Publish();

        var ex1_3_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "Si necesitas llamar la atención de un transeúnte o empleado con educación para preguntar algo, dices:",
            "Excuse me");
        ex1_3_mc.AddOption("Excuse me", true, 1);
        ex1_3_mc.AddOption("Listen to me now", false, 2);
        ex1_3_mc.AddOption("Hello goodbye", false, 3);
        ex1_3_mc.AddOption("Where are you", false, 4);

        var ex1_3_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la pregunta: Excuse me, ___ is the station?",
            "where");

        var ex1_3_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: '¿Dónde está el baño?'",
            "Where is the bathroom?");

        context.EducationalContents.AddRange(c1_3_vocab, c1_3_concept);
        context.Exercises.AddRange(ex1_3_mc, ex1_3_fb, ex1_3_tr);
        await context.SaveChangesAsync();

        l1_3.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_3.Id, LessonBlockType.Heading, 1, new BlockConfiguration("Ubicarte y pedir indicaciones en la ciudad")));
        l1_3.AddLessonBlock(LessonBlock.CreateContentBlock(l1_3.Id, 2, c1_3_vocab.Id));
        l1_3.AddLessonBlock(LessonBlock.CreateContentBlock(l1_3.Id, 3, c1_3_concept.Id));
        l1_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_3.Id, 4, ex1_3_mc.Id));
        l1_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_3.Id, 5, ex1_3_fb.Id));
        l1_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l1_3.Id, 6, ex1_3_tr.Id));
        l1_3.AddLessonBlock(LessonBlock.CreateInformationalBlock(l1_3.Id, LessonBlockType.Summary, 7, new BlockConfiguration("¡Has completado la colección de Fundamentos! Ya tienes las herramientas básicas para saludar, pedir alimentos y ubicarte en cualquier lugar.")));
        l1_3.Publish();

        // -------------------------------------------------------------
        // COLECCIÓN 2: Inglés Profesional y Digital: Trabajo (Nivel A2)
        // -------------------------------------------------------------
        var col2 = new Collection(
            "Inglés Profesional y Digital: Comunicación en el Trabajo",
            "Avanza hacia situaciones reales de trabajo: términos clave indispensables, reportar estado, pedir ayuda y redactar mensajes claros sin tropiezos.",
            null,
            ReferenceLevel.A2,
            2);

        // --- Lección 2.1: Palabras Clave de Oficina y Gestión de Tareas ---
        var l2_1 = new Lesson(
            "Palabras Clave de Oficina y Gestión de Tareas",
            "Comprende el vocabulario esencial de proyectos, reuniones y cómo reportar el estado de tus tareas.",
            ReferenceLevel.A2);
        l2_1.AddCategory(catBusiness);

        var c2_1_vocab = new EducationalContent(
            "Palabras Clave: Conceptos de Trabajo Diario",
            "Glosario de términos que escucharás y leerás todos los días en el entorno laboral.",
            "• Deadline = Fecha límite de entrega\n• Meeting = Reunión\n• Task = Tarea\n• Update = Actualización / Poner al día\n• Schedule = Agendar (como verbo) o Cronograma (como sustantivo)\n• Urgent = Urgente",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A2);
        c2_1_vocab.Publish();

        var c2_1_rule = new EducationalContent(
            "Estructura de Estado: 'I am working on...'",
            "Cómo informar en qué estás avanzando en tus tareas diarias.",
            "Usa el presente continuo para indicar tu foco actual:\n👉 'I am working on the [tarea]'\n👉 'I finished the [tarea]'\n\nEjemplo:\n'Today I am working on the project update.' (Hoy estoy trabajando en la actualización del proyecto).",
            EducationalContentType.Rule,
            ReferenceLevel.A2);
        c2_1_rule.Publish();

        var c2_1_mistake = new EducationalContent(
            "Error Común: 'Work' vs 'Job'",
            "Dos palabras que parecen iguales pero se usan de forma muy distinta.",
            "⚠️ 'Job' es un sustantivo contable (tu puesto o empleo: 'I have a new job').\n⚠️ 'Work' es incontable o verbo (la actividad o volumen de trabajo: 'I have a lot of work today', nunca digas 'a lot of jobs').",
            EducationalContentType.CommonMistake,
            ReferenceLevel.A2);
        c2_1_mistake.Publish();

        var ex2_1_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "¿Qué término en inglés se utiliza para referirse a la 'fecha límite' de entrega de un proyecto o tarea?",
            "Deadline");
        ex2_1_mc.AddOption("Deadline", true, 1);
        ex2_1_mc.AddOption("Schedule", false, 2);
        ex2_1_mc.AddOption("Meeting", false, 3);
        ex2_1_mc.AddOption("Update", false, 4);

        var ex2_1_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la frase: I need to ___ a meeting for tomorrow.",
            "schedule");

        var ex2_1_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: 'Estoy trabajando en la tarea'",
            "I am working on the task");

        context.EducationalContents.AddRange(c2_1_vocab, c2_1_rule, c2_1_mistake);
        context.Exercises.AddRange(ex2_1_mc, ex2_1_fb, ex2_1_tr);
        await context.SaveChangesAsync();

        l2_1.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_1.Id, LessonBlockType.Heading, 1, new BlockConfiguration("El vocabulario indispensable del entorno laboral moderno")));
        l2_1.AddLessonBlock(LessonBlock.CreateContentBlock(l2_1.Id, 2, c2_1_vocab.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateContentBlock(l2_1.Id, 3, c2_1_rule.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateContentBlock(l2_1.Id, 4, c2_1_mistake.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_1.Id, 5, ex2_1_mc.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_1.Id, 6, ex2_1_fb.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_1.Id, 7, ex2_1_tr.Id));
        l2_1.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_1.Id, LessonBlockType.Summary, 8, new BlockConfiguration("¡Excelente! Ya manejas los términos nucleares de gestión de tareas y la forma correcta de reportar tu actividad.")));
        l2_1.Publish();

        // --- Lección 2.2: Comunicar Bloqueos y Pedir Apoyo en Equipo ---
        var l2_2 = new Lesson(
            "Comunicar Bloqueos y Pedir Apoyo en Equipo",
            "Aprende a reportar impedimentos con claridad y pedir ayuda o compartir pantalla de forma profesional.",
            ReferenceLevel.A2);
        l2_2.AddCategory(catBusiness);

        var c2_2_vocab = new EducationalContent(
            "Palabras Clave: Soporte y Colaboración",
            "Términos indispensables para resolver inconvenientes en equipo.",
            "• Blocked = Bloqueado / Trabado (no puedes avanzar)\n• Issue / Bug = Problema / Error\n• Help / Support = Ayuda / Soporte\n• Share screen = Compartir pantalla\n• Quick call = Llamada rápida de 5 minutos",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A2);
        c2_2_vocab.Publish();

        var c2_2_concept = new EducationalContent(
            "Patrones para Explicar un Bloqueo con Claridad",
            "Estructuras directas y profesionales para pedir ayuda en chats o videollamadas.",
            "En reuniones ágiles (Daily Standup) o canales de chat, usa estas fórmulas clave:\n1. 'I am blocked on [tema] because [motivo].'\n2. 'Could you help me with this issue?'\n3. 'Do you have 5 minutes for a quick call?'",
            EducationalContentType.Concept,
            ReferenceLevel.A2);
        c2_2_concept.Publish();

        var c2_2_tip = new EducationalContent(
            "Tip de Cortesía: 'Could you...' en lugar de imperativos",
            "Hacer solicitudes en modo condicional suena cooperativo y respetuoso.",
            "En vez de decir 'Help me with this' (puede sonar cortante o autoritario), utiliza siempre:\n👉 'Could you take a look at this when you have a moment?' (¿Podrías echarle un vistazo cuando tengas un momento?)",
            EducationalContentType.Tip,
            ReferenceLevel.A2);
        c2_2_tip.Publish();

        var ex2_2_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "¿Cuál es la expresión estándar para avisar a tu equipo que no puedes avanzar debido a un problema?",
            "I am blocked on this task");
        ex2_2_mc.AddOption("I am blocked on this task", true, 1);
        ex2_2_mc.AddOption("I am closed on the work", false, 2);
        ex2_2_mc.AddOption("I stop my computer now", false, 3);
        ex2_2_mc.AddOption("No more job for me", false, 4);

        var ex2_2_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la solicitud: Could you ___ your screen, please?",
            "share");

        var ex2_2_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: '¿Tienes tiempo para una llamada rápida?'",
            "Do you have time for a quick call?");

        context.EducationalContents.AddRange(c2_2_vocab, c2_2_concept, c2_2_tip);
        context.Exercises.AddRange(ex2_2_mc, ex2_2_fb, ex2_2_tr);
        await context.SaveChangesAsync();

        l2_2.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_2.Id, LessonBlockType.Heading, 1, new BlockConfiguration("Cómo destrabar problemas y colaborar en equipo")));
        l2_2.AddLessonBlock(LessonBlock.CreateContentBlock(l2_2.Id, 2, c2_2_vocab.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateContentBlock(l2_2.Id, 3, c2_2_concept.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateContentBlock(l2_2.Id, 4, c2_2_tip.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_2.Id, 5, ex2_2_mc.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_2.Id, 6, ex2_2_fb.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_2.Id, 7, ex2_2_tr.Id));
        l2_2.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_2.Id, LessonBlockType.Summary, 8, new BlockConfiguration("¡Muy bien! Comunicar impedimentos y pedir ayuda con respeto es una de las habilidades más valoradas en equipos profesionales.")));
        l2_2.Publish();

        // --- Lección 2.3: Mensajes Claros, Follow-ups y Falsos Amigos ---
        var l2_3 = new Lesson(
            "Mensajes Claros, Follow-ups y Falsos Amigos",
            "Redacta correos y mensajes efectivos, domina los cierres profesionales y evita confusiones con falsos amigos como 'Actually'.",
            ReferenceLevel.A2);
        l2_3.AddCategory(catBusiness);

        var c2_3_vocab = new EducationalContent(
            "Palabras Clave: Comunicación Escrita",
            "Fórmulas comunes en correos y mensajes de trabajo.",
            "• Follow-up = Seguimiento de un tema previo\n• Attached = Adjunto (documento, archivo)\n• As discussed = Como conversamos / según lo acordado\n• Let me know = Avísame / Hazme saber\n• Best regards = Saludos cordiales",
            EducationalContentType.Vocabulary,
            ReferenceLevel.A2);
        c2_3_vocab.Publish();

        var c2_3_mistake = new EducationalContent(
            "⚠️ Falso Amigo Crítico: 'Actually' vs 'Currently'",
            "Uno de los errores más frecuentes de los hispanohablantes al escribir en inglés.",
            "❌ 'Actually' NO significa 'actualmente'. Significa 'En realidad' o 'De hecho'.\n✅ Si quieres decir 'actualmente' o 'en este momento', debes usar: 'Currently' o 'Right now'.\n\nEjemplos:\n• 'Currently, I am working on the report' = Actualmente estoy trabajando en el reporte.\n• 'Actually, that is not correct' = En realidad, eso no es correcto.",
            EducationalContentType.CommonMistake,
            ReferenceLevel.A2);
        c2_3_mistake.Publish();

        var c2_3_rule = new EducationalContent(
            "Estructura de Cierre: 'Please let me know if...'",
            "La frase estándar de oro para cerrar un mensaje profesional.",
            "Al finalizar un correo o actualización, usa:\n👉 'Please let me know if you have any questions.' (Por favor avísame si tienes alguna duda).\n\nFirma formal:\n'Best regards,\n[Tu Nombre]'",
            EducationalContentType.Rule,
            ReferenceLevel.A2);
        c2_3_rule.Publish();

        var ex2_3_mc = new Exercise(
            ExerciseType.MultipleChoice,
            "Si quieres decir 'Actualmente estamos revisando el documento', ¿cuál es la palabra correcta?",
            "Currently");
        ex2_3_mc.AddOption("Currently", true, 1);
        ex2_3_mc.AddOption("Actually", false, 2);
        ex2_3_mc.AddOption("Eventual", false, 3);
        ex2_3_mc.AddOption("Real", false, 4);

        var ex2_3_fb = new Exercise(
            ExerciseType.FillInTheBlank,
            "Completa la frase: Please let me ___ if you have any questions.",
            "know");

        var ex2_3_tr = new Exercise(
            ExerciseType.Translation,
            "Traduce al inglés: 'Adjunto el documento. Saludos cordiales.'",
            "I attached the document. Best regards.");

        context.EducationalContents.AddRange(c2_3_vocab, c2_3_mistake, c2_3_rule);
        context.Exercises.AddRange(ex2_3_mc, ex2_3_fb, ex2_3_tr);
        await context.SaveChangesAsync();

        l2_3.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_3.Id, LessonBlockType.Heading, 1, new BlockConfiguration("Redactar mensajes profesionales sin confusiones")));
        l2_3.AddLessonBlock(LessonBlock.CreateContentBlock(l2_3.Id, 2, c2_3_vocab.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateContentBlock(l2_3.Id, 3, c2_3_mistake.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateContentBlock(l2_3.Id, 4, c2_3_rule.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_3.Id, 5, ex2_3_mc.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_3.Id, 6, ex2_3_fb.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateExerciseBlock(l2_3.Id, 7, ex2_3_tr.Id));
        l2_3.AddLessonBlock(LessonBlock.CreateInformationalBlock(l2_3.Id, LessonBlockType.Summary, 8, new BlockConfiguration("¡Felicidades! Has completado la colección de Inglés Profesional. Ahora cuentas con estructuras sólidas, vocabulario clave y previenes falsos amigos.")));
        l2_3.Publish();

        // -------------------------------------------------------------
        // Guardar Lecciones y Asociar a Colecciones
        // -------------------------------------------------------------
        context.Lessons.AddRange(l1_1, l1_2, l1_3, l2_1, l2_2, l2_3);
        await context.SaveChangesAsync();

        col1.AddLesson(l1_1.Id);
        col1.AddLesson(l1_2.Id);
        col1.AddLesson(l1_3.Id);
        col1.Publish();

        col2.AddLesson(l2_1.Id);
        col2.AddLesson(l2_2.Id);
        col2.AddLesson(l2_3.Id);
        col2.Publish();

        context.Collections.AddRange(col1, col2);
        await context.SaveChangesAsync();
    }

    private static void adminUserPasswordSync(User user, string passwordHash)
    {
        // Keep user active and role updated if needed
        if (user.Role != UserRole.Administrator)
        {
            user.UpdateRole(UserRole.Administrator);
        }
    }
}

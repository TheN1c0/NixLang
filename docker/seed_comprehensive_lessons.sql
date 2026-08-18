-- =========================================================
-- NixLang - Comprehensive Lesson Catalog Seed (UTF-8)
-- =========================================================

-- 1. CATEGORIES
INSERT INTO categories (id, name, description)
VALUES 
  ('11111111-0000-0000-0000-000000000001', 'Grammar', 'Conceptos fundamentales y estructuras gramaticales en inglés.')
ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description;

INSERT INTO categories (id, name, description)
VALUES 
  ('11111111-0000-0000-0000-000000000002', 'Work & Tech', 'Inglés profesional para reuniones, comunicación remota y tecnología.')
ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description;

INSERT INTO categories (id, name, description)
VALUES 
  ('11111111-0000-0000-0000-000000000003', 'Common Mistakes', 'Errores comunes de hispanohablantes, falsos amigos y preposiciones.')
ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description;

INSERT INTO categories (id, name, description)
VALUES 
  ('11111111-0000-0000-0000-000000000004', 'Travel & Daily Life', 'Situaciones de la vida diaria, restaurantes, aeropuertos y compras.')
ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description;

INSERT INTO categories (id, name, description)
VALUES 
  ('11111111-0000-0000-0000-000000000005', 'Phrasal Verbs', 'Verbos compuestos clave para hablar de forma fluida y natural.')
ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description;

-- Helper variables for category IDs
DO $$
DECLARE
  v_cat_grammar UUID;
  v_cat_work UUID;
  v_cat_mistakes UUID;
  v_cat_travel UUID;
  v_cat_phrasal UUID;
BEGIN
  SELECT id INTO v_cat_grammar FROM categories WHERE name = 'Grammar';
  SELECT id INTO v_cat_work FROM categories WHERE name = 'Work & Tech';
  SELECT id INTO v_cat_mistakes FROM categories WHERE name = 'Common Mistakes';
  SELECT id INTO v_cat_travel FROM categories WHERE name = 'Travel & Daily Life';
  SELECT id INTO v_cat_phrasal FROM categories WHERE name = 'Phrasal Verbs';

  -- =========================================================
  -- LECCIÓN 1 (A1 - Work & Tech): Daily Standup: What are you working on?
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000001', 'Daily Standup: What are you working on?', 'Aprende las frases esenciales para explicar tus avances diarios y bloqueos en una reunión ágil.', 'A1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000001', v_cat_work)
  ON CONFLICT DO NOTHING;

  -- Exercises for Lesson 1
  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000001', 'MultipleChoice', '¿Cómo dirías en tu reunión: ''Ayer trabajé en la pantalla de inicio''?', 'Yesterday, I worked on the home screen.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'Yesterday, I worked on the home screen.', true, 1),
    ('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'Yesterday, I am work in the home screen.', false, 2),
    ('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000001', 'Yesterday, I work the home screen.', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000002', 'FillInTheBlank', 'Today, I am ___ on fixing the login bug.', 'working')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000003', 'Translation', 'Traduce: ''No tengo bloqueos hoy.''', 'I have no blockers today.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  -- Blocks for Lesson 1
  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000001';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 1, 'Heading', 'Dominando el Daily Standup en Inglés', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 2, 'Paragraph', 'En un Daily Standup, solemos responder tres preguntas clave: qué hicimos ayer, qué haremos hoy y si tenemos algún impedimento (blocker).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000001'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 4, 'Paragraph', 'Para hablar de lo que estás haciendo hoy en tiempo real, usamos el presente continuo: "I am working on...".', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000002'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 6, 'Exercise', '', '30000000-0000-0000-0000-000000000003'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000001', 7, 'Summary', '¡Excelente! Ahora tienes la base para participar en cualquier reunión diaria de equipo con seguridad.', NULL);


  -- =========================================================
  -- LECCIÓN 2 (A1 - Common Mistakes): False Friends: Actually vs Currently
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000002', 'False Friends: Actually vs Currently', 'Aprende a evitar uno de los errores más comunes de hispanohablantes al confundir ''actualmente'' con ''actually''.', 'A1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000002', v_cat_mistakes)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000004', 'MultipleChoice', '¿Qué significa realmente la palabra ''Actually'' en inglés?', 'En realidad / De hecho')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000004', 'En realidad / De hecho', true, 1),
    ('40000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000004', 'Actualmente / En el presente', false, 2),
    ('40000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000004', 'Actuando en teatro', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000005', 'FillInTheBlank', 'I am ___ living in Santiago. (Actualmente)', 'currently')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000002';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 1, 'Heading', 'Cuidado con los Falsos Amigos (False Friends)', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 2, 'Paragraph', '"Actually" NO significa "actualmente". Significa "en realidad" o "de hecho". Para decir "actualmente", usamos "Currently" o "Nowadays".', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000004'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 4, 'Paragraph', 'Ejemplo: "Actually, I disagree" (En realidad, no estoy de acuerdo). Mientras que: "I am currently unemployed" (Actualmente estoy desempleado).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000005'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000002', 6, 'Summary', '¡Genial! Has desbloqueado uno de los trucos más importantes para sonar profesional en inglés.', NULL);


  -- =========================================================
  -- LECCIÓN 3 (A1 - Travel & Daily Life): Ordering at a Coffee Shop
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000003', 'Ordering at a Coffee Shop', 'Aprende a pedir café, comida y pagar en un restaurante o cafetería de forma educada y natural.', 'A1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000003', v_cat_travel)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000006', 'Translation', 'Traduce: ''¿Puedo pedir un café con leche, por favor?''', 'Can I get a latte, please?')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000007', 'MultipleChoice', 'El mesero te pregunta: ''For here or to go?''. ¿Qué significa?', '¿Para servir aquí o para llevar?')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000007', '¿Para servir aquí o para llevar?', true, 1),
    ('40000000-0000-0000-0000-000000000008', '30000000-0000-0000-0000-000000000007', '¿Para ahora o para más tarde?', false, 2),
    ('40000000-0000-0000-0000-000000000009', '30000000-0000-0000-0000-000000000007', '¿Con azúcar o sin azúcar?', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000008', 'FillInTheBlank', 'Could we have the ___, please? (La cuenta)', 'check')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000003';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 1, 'Heading', 'Cómo ordenar en una cafetería', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 2, 'Paragraph', 'La forma más común y educada de pedir algo en inglés es usar "Can I get..." o "Could I have..." en lugar de "I want...".', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000006'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 4, 'Paragraph', 'Cuando estés listo para pagar, puedes pedir la cuenta diciendo "Can we have the check, please?" (en EE.UU.) o "the bill" (en Reino Unido).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000007'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 6, 'Exercise', '', '30000000-0000-0000-0000-000000000008'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000003', 7, 'Summary', '¡Lección completada! Ahora puedes entrar a Starbucks o cualquier café del mundo y pedir tu orden sin dudar.', NULL);


  -- =========================================================
  -- LECCIÓN 4 (A2 - Grammar): Past Simple vs Past Continuous
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000004', 'Past Simple vs Past Continuous', 'Aprende a narrar historias y acciones interrumpidas combinando el pasado simple y el continuo.', 'A2', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000004', v_cat_grammar)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000009', 'FillInTheBlank', 'I was coding when the power ___ out. (went/go/goes)', 'went')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000010', 'MultipleChoice', 'Elige la oración correcta que describe una acción que estaba en progreso:', 'She was reading a book when I called her.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000010', '30000000-0000-0000-0000-000000000010', 'She was reading a book when I called her.', true, 1),
    ('40000000-0000-0000-0000-000000000011', '30000000-0000-0000-0000-000000000010', 'She read a book while I was call her.', false, 2),
    ('40000000-0000-0000-0000-000000000012', '30000000-0000-0000-0000-000000000010', 'She is reading when I calling her.', false, 3)
  ON CONFLICT (id) DO NOTHING;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000004';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 1, 'Heading', 'Acciones Interrumpidas en el Pasado', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 2, 'Paragraph', 'Usamos Past Continuous (was/were + verbo-ing) para la acción que estaba en progreso, y Past Simple para la acción puntual que la interrumpió.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000009'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 4, 'Paragraph', 'Conectores clave: usamos "when" antes de la acción corta en pasado simple, y "while" antes de la acción larga continua.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000010'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000004', 6, 'Summary', '¡Excelente progreso! Ahora puedes relatar anécdotas del pasado con mucha mayor precisión.', NULL);


  -- =========================================================
  -- LECCIÓN 5 (A2 - Common Mistakes): Prepositions of Time: In, On, At
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000005', 'Prepositions of Time: In, On, At', 'Domina la pirámide de las preposiciones temporales y deja de dudar en cada frase.', 'A2', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000005', v_cat_mistakes), ('20000000-0000-0000-0000-000000000005', v_cat_grammar)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000011', 'FillInTheBlank', 'Our meeting is ___ Monday morning.', 'on')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000012', 'MultipleChoice', '¿Cuál es la preposición correcta para una hora específica: ''The train arrives ___ 7:30 PM''?', 'at')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000013', '30000000-0000-0000-0000-000000000012', 'at', true, 1),
    ('40000000-0000-0000-0000-000000000014', '30000000-0000-0000-0000-000000000012', 'in', false, 2),
    ('40000000-0000-0000-0000-000000000015', '30000000-0000-0000-0000-000000000012', 'on', false, 3)
  ON CONFLICT (id) DO NOTHING;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000005';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 1, 'Heading', 'La Regla de la Pirámide: IN, ON, AT', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 2, 'Paragraph', '• IN: Períodos largos (meses, años, siglos, estaciones) -> in 2026, in December.
• ON: Días específicos y fechas -> on Monday, on July 4th.
• AT: Horas y momentos exactos -> at 5:00 PM, at midnight.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000011'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 4, 'Paragraph', 'Recuerda: para las partes del día decimos "in the morning", "in the afternoon", pero ¡ojo!: decimos "at night".', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000012'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000005', 6, 'Summary', '¡Fantástico! Ya dominas una de las áreas que más confunden a los hispanohablantes.', NULL);


  -- =========================================================
  -- LECCIÓN 6 (A2 - Work & Tech): Writing Professional Emails
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000006', 'Writing Professional Emails', 'Escribe correos corporativos impecables con saludos, solicitudes amables y cierres formales.', 'A2', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000006', v_cat_work)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000013', 'MultipleChoice', '¿Cuál es la frase más profesional para abrir un correo corporativo?', 'I hope this email finds you well.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000016', '30000000-0000-0000-0000-000000000013', 'I hope this email finds you well.', true, 1),
    ('40000000-0000-0000-0000-000000000017', '30000000-0000-0000-0000-000000000013', 'Hey what is up?', false, 2),
    ('40000000-0000-0000-0000-000000000018', '30000000-0000-0000-0000-000000000013', 'I write you because I need help.', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000014', 'Translation', 'Traduce el cierre: ''Saludos cordiales,''', 'Best regards,')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000006';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 1, 'Heading', 'Estructura de un Correo Profesional', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 2, 'Paragraph', 'Un correo profesional en inglés sigue 4 partes: Saludo formal ("Dear Alex" / "Hi team"), apertura cortés, cuerpo del mensaje claro y cierre profesional.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000013'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 4, 'Paragraph', 'Para despedirte de manera elegante, "Best regards," o "Kind regards," son las opciones estándar más recomendadas.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000014'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000006', 6, 'Summary', '¡Excelente! Ahora puedes redactar mensajes que proyecten confianza y profesionalismo internacional.', NULL);


  -- =========================================================
  -- LECCIÓN 7 (B1 - Phrasal Verbs): 5 Essential Phrasal Verbs for Daily Life
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000007', '5 Essential Phrasal Verbs for Daily Life', 'Domina los verbos compuestos más usados por hablantes nativos: figure out, catch up, run out of y más.', 'B1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000007', v_cat_phrasal)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000015', 'MultipleChoice', '¿Qué significa el phrasal verb ''Figure out''?', 'Descubrir / Resolver / Entender cómo solucionar algo')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000019', '30000000-0000-0000-0000-000000000015', 'Descubrir / Resolver / Entender cómo solucionar algo', true, 1),
    ('40000000-0000-0000-0000-000000000020', '30000000-0000-0000-0000-000000000015', 'Dibujar una figura geométrica', false, 2),
    ('40000000-0000-0000-0000-000000000021', '30000000-0000-0000-0000-000000000015', 'Salirse de una reunión', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000016', 'FillInTheBlank', 'We ___ out of coffee! We need to buy more.', 'ran')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000017', 'Translation', 'Traduce: ''Espero con entusiasmo verte pronto.'' (Usa ''look forward to'')', 'I look forward to seeing you soon.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000007';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 1, 'Heading', 'El Poder de los Phrasal Verbs', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 2, 'Paragraph', 'Los nativos rara vez dicen "I will solve this problem", suelen decir "I will figure this out". Los phrasal verbs transforman tu inglés de básico a fluido.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000015'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 4, 'Paragraph', '"Run out of" significa que se te agotó algo (dinero, tiempo, batería o café).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000016'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 6, 'Exercise', '', '30000000-0000-0000-0000-000000000017'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000007', 7, 'Summary', '¡Impresionante! Usar estos verbos compuestos hará que tus conversaciones suenen 100% auténticas.', NULL);


  -- =========================================================
  -- LECCIÓN 8 (B1 - Grammar): First & Second Conditionals
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000008', 'First & Second Conditionals', 'Aprende a hablar de consecuencias reales futuras vs situaciones hipotéticas e imaginarias.', 'B1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000008', v_cat_grammar)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000018', 'FillInTheBlank', 'If I study hard, I ___ pass the certification exam.', 'will')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000019', 'MultipleChoice', 'Completa la hipótesis (2do condicional): ''If I had more free time, I ___ travel the world.''', 'would')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000022', '30000000-0000-0000-0000-000000000019', 'would', true, 1),
    ('40000000-0000-0000-0000-000000000023', '30000000-0000-0000-0000-000000000019', 'will', false, 2),
    ('40000000-0000-0000-0000-000000000024', '30000000-0000-0000-0000-000000000019', 'can', false, 3)
  ON CONFLICT (id) DO NOTHING;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000008';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 1, 'Heading', 'Planes Reales vs Sueños Hipotéticos', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 2, 'Paragraph', '• 1st Conditional: Situación real y probable en el futuro -> If + Presente Simple, Will + Verbo.
• 2nd Conditional: Situación hipotética o irreal en el presente -> If + Pasado Simple, Would + Verbo.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000018'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 4, 'Paragraph', 'Dato clave: Para el 2do condicional con el verbo "to be", es estándar usar "were" para todas las personas: "If I were you..." (Si yo fuera tú...).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000019'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000008', 6, 'Summary', '¡Excelente! Ahora tienes el poder de plantear escenarios, hipótesis y soluciones de negocio en inglés.', NULL);


  -- =========================================================
  -- LECCIÓN 9 (B1 - Work & Tech): Ace Your Job Interview in English
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000009', 'Ace Your Job Interview in English', 'Estructura tus respuestas profesionales usando el método STAR y destaca tus fortalezas con convicción.', 'B1', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000009', v_cat_work)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000020', 'MultipleChoice', 'El reclutador dice: ''Tell me about yourself''. ¿Cuál es el mejor enfoque?', 'Un resumen conciso de tu experiencia relevante, logros y por qué te interesa el puesto.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000025', '30000000-0000-0000-0000-000000000020', 'Un resumen conciso de tu experiencia relevante, logros y por qué te interesa el puesto.', true, 1),
    ('40000000-0000-0000-0000-000000000026', '30000000-0000-0000-0000-000000000020', 'Contar toda tu vida desde la infancia con tus pasatiempos favoritos.', false, 2),
    ('40000000-0000-0000-0000-000000000027', '30000000-0000-0000-0000-000000000020', 'Decir que no tienes nada que agregar y esperar preguntas específicas.', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000021', 'FillInTheBlank', 'My greatest strength is my ability to ___ complex problems under pressure.', 'solve')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000022', 'Translation', 'Traduce: ''Tengo 5 años de experiencia liderando proyectos.''', 'I have 5 years of experience leading projects.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000009';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 1, 'Heading', 'Preparando tu Entrevista en Inglés', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 2, 'Paragraph', 'El método STAR (Situation, Task, Action, Result) te permite estructurar respuestas convincentes sobre tus logros pasados.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000020'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 4, 'Paragraph', 'Al hablar de fortalezas, acompaña cada adjetivo con un ejemplo verificable de impacto en tu equipo.', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000021'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 6, 'Exercise', '', '30000000-0000-0000-0000-000000000022'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000009', 7, 'Summary', '¡Fantástico trabajo! Estás un paso más cerca de conseguir esa oferta laboral internacional.', NULL);


  -- =========================================================
  -- LECCIÓN 10 (B2 - Common Mistakes): Make vs Do: Master the Difference
  -- =========================================================
  INSERT INTO lessons (id, title, description, reference_level, status, created_at)
  VALUES ('20000000-0000-0000-0000-000000000010', 'Make vs Do: Master the Difference', 'Aprende las colocaciones exactas de Make vs Do para hablar como un nativo sin dudar.', 'B2', 'Published', NOW())
  ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title, description = EXCLUDED.description, reference_level = EXCLUDED.reference_level;

  INSERT INTO lesson_categories (lesson_id, category_id)
  VALUES ('20000000-0000-0000-0000-000000000010', v_cat_mistakes), ('20000000-0000-0000-0000-000000000010', v_cat_grammar)
  ON CONFLICT DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000023', 'FillInTheBlank', 'It is completely normal to ___ mistakes when learning a new language.', 'make')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000024', 'MultipleChoice', '¿Cuál combinación utiliza correctamente el verbo ''DO''?', 'Do business / Do research / Do someone a favor')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  INSERT INTO exercise_options (id, exercise_id, text, is_correct, display_order)
  VALUES 
    ('40000000-0000-0000-0000-000000000028', '30000000-0000-0000-0000-000000000024', 'Do business / Do research / Do someone a favor', true, 1),
    ('40000000-0000-0000-0000-000000000029', '30000000-0000-0000-0000-000000000024', 'Do a decision / Do a cake / Do an appointment', false, 2),
    ('40000000-0000-0000-0000-000000000030', '30000000-0000-0000-0000-000000000024', 'Do a phone call / Do progress / Do a promise', false, 3)
  ON CONFLICT (id) DO NOTHING;

  INSERT INTO exercises (id, type, statement, correct_answer)
  VALUES ('30000000-0000-0000-0000-000000000025', 'Translation', 'Traduce: ''Necesitamos tomar una decisión hoy.''', 'We need to make a decision today.')
  ON CONFLICT (id) DO UPDATE SET statement = EXCLUDED.statement, correct_answer = EXCLUDED.correct_answer;

  DELETE FROM lesson_blocks WHERE lesson_id = '20000000-0000-0000-0000-000000000010';
  INSERT INTO lesson_blocks (id, lesson_id, sequence, type, configuration, referenced_exercise_id)
  VALUES 
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 1, 'Heading', 'Regla de Oro: MAKE vs DO', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 2, 'Paragraph', '• MAKE: Crear, producir, construir algo tangible o abstracto (make coffee, make a decision, make money, make friends, make a mistake).
• DO: Tareas, actividades generales, deberes o trabajo (do homework, do business, do your best, do exercise).', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 3, 'Exercise', '', '30000000-0000-0000-0000-000000000023'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 4, 'Paragraph', 'Consejo Pro: En español decimos "hacer una llamada" y "hacer una decisión", pero en inglés es siempre "make a call" y "make a decision".', NULL),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 5, 'Exercise', '', '30000000-0000-0000-0000-000000000024'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 6, 'Exercise', '', '30000000-0000-0000-0000-000000000025'),
    (gen_random_uuid(), '20000000-0000-0000-0000-000000000010', 7, 'Summary', '¡Felicitaciones! Has completado una lección de nivel avanzado y perfeccionado tu precisión léxica.', NULL);

END $$;

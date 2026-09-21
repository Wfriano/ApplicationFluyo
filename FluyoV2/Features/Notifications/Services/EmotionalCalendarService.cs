namespace FluyoV2.Features.Notifications.Services;

public record EmotionalCalendarMessage(int Day, string Title, string Message, string Category);

public class EmotionalCalendarService
{
    private static readonly IReadOnlyList<EmotionalCalendarMessage> Messages = new List<EmotionalCalendarMessage>
    {
        new(1, "Calendario emocional - Día 1", "🌱 Antes de pensar en lo que falta... Mira todo lo que ya has recorrido.", "gratitud"),
        new(2, "Calendario emocional - Día 2", "🌅 Hoy estrenas 24 horas. Haz que al menos una decisión valga la pena.", "inicio"),
        new(3, "Calendario emocional - Día 3", "💰 La tranquilidad empieza cuando sabes dónde estás.", "claridad"),
        new(4, "Calendario emocional - Día 4", "💙 No todo necesita resolverse hoy. La calma también hace avanzar.", "calma"),
        new(5, "Calendario emocional - Día 5", "🎯 Antes de decidir... Pregúntate cómo quieres sentirte después.", "decisiones"),
        new(6, "Calendario emocional - Día 6", "🧠 A veces no necesitas una respuesta. Solo una nueva forma de mirar las cosas.", "perspectiva"),
        new(7, "Calendario emocional - Día 7", "🌱 Hoy también hay algo por agradecer. No dejes que pase desapercibido.", "gratitud"),
        new(8, "Calendario emocional - Día 8", "💰 El dinero es una herramienta. No una medida de tu valor.", "dinero"),
        new(9, "Calendario emocional - Día 9", "🌅 No cargues con ayer. Hoy merece una oportunidad nueva.", "inicio"),
        new(10, "Calendario emocional - Día 10", "💙 La tranquilidad no aparece por casualidad. Se construye un momento a la vez.", "calma"),
        new(11, "Calendario emocional - Día 11", "🎯 Cada decisión cambia algo. Elige la que te acerque a la vida que quieres.", "decisiones"),
        new(12, "Calendario emocional - Día 12", "🧠 No todo está en tus manos. Y eso también puede darte tranquilidad.", "perspectiva"),
        new(13, "Calendario emocional - Día 13", "💰 La claridad también es una forma de riqueza.", "dinero"),
        new(14, "Calendario emocional - Día 14", "🌱 Lo que hoy es normal para ti, alguna vez fue un sueño.", "gratitud"),
        new(15, "Calendario emocional - Día 15", "🌅 No hace falta tenerlo todo claro. Hace falta dar el primer paso.", "inicio"),
        new(16, "Calendario emocional - Día 16", "💰 Cada peso tiene un destino. Elegirlo también es cuidarte.", "dinero"),
        new(17, "Calendario emocional - Día 17", "💙 Respira un momento. Las mejores decisiones rara vez nacen del afán.", "calma"),
        new(18, "Calendario emocional - Día 18", "🎯 No todo merece un \"sí\". También se construye cuando eliges decir \"no\".", "decisiones"),
        new(19, "Calendario emocional - Día 19", "🧠 Lo que hoy parece un problema... Mañana puede ser una lección.", "perspectiva"),
        new(20, "Calendario emocional - Día 20", "🌱 No olvides agradecerte. Has superado días que parecían imposibles.", "gratitud"),
        new(21, "Calendario emocional - Día 21", "💰 Ahorrar no es dejar de vivir. Es darle oportunidades a tu yo de mañana.", "dinero"),
        new(22, "Calendario emocional - Día 22", "🌅 El día todavía no ha decidido cómo terminará. Tú sí puedes decidir cómo empezarlo.", "inicio"),
        new(23, "Calendario emocional - Día 23", "💙 Hoy no tienes que hacerlo perfecto. Solo hacerlo con calma.", "calma"),
        new(24, "Calendario emocional - Día 24", "🎯 No puedes elegir todo. Pero siempre puedes elegir lo importante.", "decisiones"),
        new(25, "Calendario emocional - Día 25", "🧠 Cambiar de perspectiva también es avanzar.", "perspectiva"),
        new(26, "Calendario emocional - Día 26", "💰 No se trata de gastar menos. Se trata de elegir mejor.", "dinero"),
        new(27, "Calendario emocional - Día 27", "🌱 Cada pequeño paso merece ser reconocido. Así también se construye el progreso.", "gratitud"),
        new(28, "Calendario emocional - Día 28", "🌅 La prisa quiere salir corriendo. La calma suele encontrar mejores caminos.", "inicio"),
        new(29, "Calendario emocional - Día 29", "💰 La paz de saber cuánto tienes vale más que adivinar.", "dinero"),
        new(30, "Calendario emocional - Día 30", "💙 No cargues con todo. Hoy también puedes soltar un poco.", "calma"),
        new(31, "Calendario emocional - Día 31", "🎯 Cada elección habla de tus prioridades. ¿Hoy cuáles quieres cuidar?", "decisiones"),
        new(32, "Calendario emocional - Día 32", "🧠 Hay días para resolver. Y hay días para comprender.", "perspectiva"),
        new(33, "Calendario emocional - Día 33", "🌱 Crecer no siempre significa llegar más lejos. A veces significa vivir con más calma.", "gratitud"),
        new(34, "Calendario emocional - Día 34", "💰 No necesitas más dinero para empezar. Necesitas más claridad.", "dinero"),
        new(35, "Calendario emocional - Día 35", "🌅 Cada mañana trae algo que ayer no tenía. La posibilidad de hacerlo diferente.", "inicio"),
        new(36, "Calendario emocional - Día 36", "💙 No confundas velocidad con progreso. Cada paso también cuenta.", "calma"),
        new(37, "Calendario emocional - Día 37", "🎯 Las decisiones difíciles pesan menos cuando sabes por qué las tomas.", "decisiones"),
        new(38, "Calendario emocional - Día 38", "🧠 La claridad no siempre cambia la realidad. Pero sí cambia la forma de vivirla.", "perspectiva"),
        new(39, "Calendario emocional - Día 39", "💰 El dinero no compra tranquilidad. Las buenas decisiones sí.", "dinero"),
        new(40, "Calendario emocional - Día 40", "🌱 Antes de empezar este día, agradece una cosa que ya tienes. 💙", "gratitud"),
        new(41, "Calendario emocional - Día 41", "🌅 Empieza por lo importante. Lo urgente siempre encontrará la forma de aparecer.", "inicio"),
        new(42, "Calendario emocional - Día 42", "💰 Comprar es fácil. Elegir bien requiere calma.", "dinero"),
        new(43, "Calendario emocional - Día 43", "💙 Cuando todo parece urgente... Recuerda qué es realmente importante.", "calma"),
        new(44, "Calendario emocional - Día 44", "🎯 El futuro no llega de repente. Empieza con lo que eliges hoy.", "decisiones"),
        new(45, "Calendario emocional - Día 45", "🧠 No todo lo que pesa merece seguir cargándose.", "perspectiva"),
        new(46, "Calendario emocional - Día 46", "💰 Cada decisión financiera también construye tu futuro.", "dinero"),
        new(47, "Calendario emocional - Día 47", "🌱 A veces el mayor avance es darte cuenta de cuánto has crecido.", "gratitud"),
        new(48, "Calendario emocional - Día 48", "🌅 Los buenos días no aparecen por suerte. Se construyen con pequeñas decisiones.", "inicio"),
        new(49, "Calendario emocional - Día 49", "💙 La paz empieza cuando dejas de pelear con cada minuto.", "calma"),
        new(50, "Calendario emocional - Día 50", "💰 El dinero va y viene. Los buenos hábitos permanecen.", "dinero"),
        new(51, "Calendario emocional - Día 51", "🎯 La prisa quiere respuestas. La calma suele encontrar mejores decisiones.", "decisiones"),
        new(52, "Calendario emocional - Día 52", "🧠 Una nueva mirada puede cambiar un mismo día.", "perspectiva"),
        new(53, "Calendario emocional - Día 53", "🌱 Antes de pensar en lo que falta... Mira todo lo que ya has recorrido. (Se repite intencionalmente para reforzar el hábito de gratitud).", "gratitud"),
        new(54, "Calendario emocional - Día 54", "💰 No todo lo que cuesta dinero tiene valor.", "dinero"),
        new(55, "Calendario emocional - Día 55", "💙 No todo necesita resolverse hoy. La calma también hace avanzar. (Repetición intencional de uno de los mensajes más fuertes).", "calma"),
        new(56, "Calendario emocional - Día 56", "🎯 Antes de decidir... Pregúntate cómo quieres sentirte después. (Repetición intencional).", "decisiones"),
        new(57, "Calendario emocional - Día 57", "🧠 A veces no necesitas una respuesta. Solo una nueva forma de mirar las cosas. (Repetición intencional).", "perspectiva"),
        new(58, "Calendario emocional - Día 58", "💰 Las mejores compras también saben esperar.", "dinero"),
        new(59, "Calendario emocional - Día 59", "💰 Cada peso que cuidas también está cuidando tu futuro.", "dinero"),
        new(60, "Calendario emocional - Día 60", "💰 Tu dinero debería darte claridad. Nunca miedo.", "dinero")
    };

    public EmotionalCalendarMessage GetMessageForDate(DateOnly date)
    {
        var index = (date.DayNumber - 1) % Messages.Count;
        return Messages[index];
    }
}

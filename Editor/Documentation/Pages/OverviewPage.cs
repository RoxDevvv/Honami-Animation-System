using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Documentation.Pages
{
    public sealed class OverviewPage : IHonamiDocumentationPage
    {
        public string Title => HonamiDocLocalization.Get("Welcome to Honami", "Ласкаво просимо до Honami");
        public string Category => HonamiDocLocalization.Get("01. Welcome & Fundamentals", "01. Вступ та Основи");
        public string SearchKeywords => "welcome introduction overview artist animator why honami вітання вступ огляд чому";
        public int Order => 10;
        public int EstimatedReadTime => 2;

        public void BuildContent(VisualElement root)
        {
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Welcome to the Honami Documentation. This is not just a technical manual — it's a comprehensive course on game animation logic. Whether you're here to master the Honami system or to learn the core principles of digital movement, you've come to the right place.",
                "Вітаємо у документації Honami. Це не просто технічний посібник — це повноцінний курс із логіки ігрової анімації. Незалежно від того, чи прагнете ви досконало опанувати систему Honami, чи хочете зрозуміти глибинні принципи цифрового руху, ви опинилися саме там, де потрібно."
            ));

            HonamiDocumentationBuilder.AddSeparator(root);

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("More Than Just a State Machine", "Більше, ніж просто кінцевий автомат"), HonamiEditorIcons.TimelineWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Honami is built on the philosophy that animation in games is a dynamic, reactive simulation. We created Honami to give you precise control over that simulation with tools designed for professional production. Here is what you will learn in this documentation:",
                "Honami побудовано на філософії, що анімація в іграх — це динамічна, реактивна симуляція. Ми створили Honami, щоб надати вам точний контроль над цією симуляцією за допомогою інструментів, призначених для професійної розробки. Ось що ви дізнаєтесь із цієї документації:"
            ));

            HonamiDocumentationBuilder.AddFeatureGrid(root,
                (HonamiDocLocalization.Get("Philosophy", "Філософія руху"), HonamiDocLocalization.Get("Why game animation is a complex reactive simulation.", "Чому ігрова анімація — це складна реактивна симуляція та як нею керувати."), HonamiEditorIcons.TimelineWhite),
                (HonamiDocLocalization.Get("Rigging", "Рігінг (Rig)"), HonamiDocLocalization.Get("Built-in Foot IK, LookAt and procedural adjustments.", "Вбудовані системи Foot IK, LookAt та процедурної фізики."), HonamiEditorIcons.BlendTreeWhite),
                (HonamiDocLocalization.Get("Comparisons", "Порівняння підходів"), HonamiDocLocalization.Get("Honami vs Animator vs Tweens vs Timeline.", "Чим Honami кращий за Animator, твіни та Timeline."), HonamiEditorIcons.Controller),
                (HonamiDocLocalization.Get("Blending", "Мистецтво змішування"), HonamiDocLocalization.Get("Layer math, additive blending, and curves.", "Математика шарів, адитивне змішування та тонка робота з кривими."), HonamiEditorIcons.BlendTreeWhite),
                (HonamiDocLocalization.Get("Avatars", "Аватари"), HonamiDocLocalization.Get("Dynamic runtime skeleton and mask swapping.", "Динамічна заміна скелетів та масок під час виконання (runtime)."), HonamiEditorIcons.Controller),
                (HonamiDocLocalization.Get("Inheritance", "Спадкування"), HonamiDocLocalization.Get("Child layers and Override Controllers for reusable animation logic.", "Дочірні шари та Override Controllers для повторного використання анімаційної логіки."), HonamiEditorIcons.Controller),
                (HonamiDocLocalization.Get("Extending", "Розширення"), HonamiDocLocalization.Get("Build custom Nodes and logic.", "Створення власних вузлів та кастомної логіки для ваших потреб."), HonamiEditorIcons.GraphWhite),
                (HonamiDocLocalization.Get("Glossary", "Словник"), HonamiDocLocalization.Get("Definitions for states, transitions, and graphs.", "Точні визначення для станів, переходів, нормалізованого часу тощо."), HonamiEditorIcons.Controller),
                (HonamiDocLocalization.Get("Fundamentals", "Основи"), HonamiDocLocalization.Get("Curves, interpolation, and skeletal theory.", "Криві, інтерполяція, кватерніони та теорія скелетної анімації."), HonamiEditorIcons.GraphWhite)
            );

            HonamiDocumentationBuilder.AddSeparator(root);

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Why is Honami So Powerful?", "Чому Honami такий потужний?"), HonamiEditorIcons.GraphWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Honami solves the biggest problems of modern game animation: state explosion, spaghetti graphs, and lack of visual feedback. It is designed by animators, for animators, but engineered to be perfectly performant for developers.",
                "Honami вирішує найболючіші проблеми сучасної ігрової анімації: вибух кількості станів (state explosion), заплутані графи (spaghetti-код) та відсутність візуального зворотного зв'язку. Він розроблений аніматорами для аніматорів, але при цьому інженерно оптимізований для розробників."
            ));

            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Sub-Nodes Architecture: Keep your graph clean by encapsulating secondary logic (like sounds, effects, or IK) directly into states without cluttering the main logic flow.", "Архітектура підвузлів (Sub-Nodes): Тримайте свій граф ідеально чистим, інкапсулюючи другорядну логіку (звуки, ефекти або IK) безпосередньо в стани, не засмічуючи основний потік прийняття рішень."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Zero-Allocation Runtime: Honami uses modern C# features to ensure that animation evaluation produces zero garbage, keeping your framerate perfectly smooth.", "Відсутність алокацій пам'яті (Zero-Allocation): Honami використовує сучасні можливості C#, щоб гарантувати відсутність збирання сміття (Garbage Collection) під час обчислення анімацій, зберігаючи ваш FPS ідеально стабільним."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Live Visual Debugging: See exactly which node is active, its exact weight, and the values of all parameters directly in the editor in real-time.", "Живе візуальне налагодження: Бачте в реальному часі, який саме вузол є активним, його точну вагу впливу (weight) та значення всіх параметрів прямо в редакторі."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Reusable Graphs: Layer inheritance and Override Controllers let you build animation families without duplicating entire graphs.", "Повторне використання графів: Спадкування шарів та Override Controllers дозволяють будувати сімейства анімацій без дублювання цілих графів."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Linked Brain System: An orchestrator that lets you command multiple characters simultaneously with precise timing and sync logic.", "Система Linked Brain: Потужний оркестратор, що дозволяє вам керувати кількома персонажами одночасно з ідеальною синхронізацією таймінгів та логіки."));

            HonamiDocumentationBuilder.AddSeparator(root);

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Quick Actions", "Швидкі дії"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddActionGroup(root,
                (HonamiDocLocalization.Get("Create New Controller", "Створити новий контролер"), HonamiEditorIcons.TimelineWhite, () => HonamiDocumentationBuilder.CreateController()),
                (HonamiDocLocalization.Get("Open Animator", "Відкрити аніматор"), HonamiEditorIcons.GraphWhite, () => HonamiGraphWindow.OpenWindow())
            );
        }
    }
}

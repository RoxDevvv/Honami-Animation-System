using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Documentation.Pages
{
    public sealed class AnimationComparisonPage : IHonamiDocumentationPage
    {
        public string Title => HonamiDocLocalization.Get("Comparison of Approaches", "Порівняння підходів");
        public string Category => HonamiDocLocalization.Get("01. Welcome & Fundamentals", "01. Вступ та Основи");
        public string SearchKeywords => "comparison mecanum tweening timeline advantages animator replacement migration порівняння переваги заміна міграція";
        public int Order => 30;
        public int EstimatedReadTime => 6;

        public void BuildContent(VisualElement root)
        {
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "In modern Unity development, there are several ways to move objects and skeletons. Understanding when to use Honami versus other systems is key to a clean architecture.",
                "У сучасній розробці на Unity існує кілька способів переміщення об'єктів та скелетів. Розуміння того, коли використовувати Honami, а коли інші системи, є ключем до чистої архітектури."
            ));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Honami vs. Unity Animator (Mecanim)", "Honami проти Unity Animator (Mecanim)"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddTable(root,
                (HonamiDocLocalization.Get("Feature", "Функція"), 150),
                ("Unity Animator", 0),
                ("Honami", 0),
                ("Logic Style", "AnimatorController state machine", HonamiDocLocalization.Get("Modular runtime graph with custom nodes and sub-nodes", "Модульний runtime-граф із кастомними вузлами та підвузлами")),
                ("Runtime Engine", "Unity AnimatorController evaluation", HonamiDocLocalization.Get("Honami evaluation stack driving PlayableGraph directly", "Власний стек обчислення Honami, який напряму керує PlayableGraph")),
                ("Layer Reuse", "Manual duplication or nested state machines", HonamiDocLocalization.Get("Layer inheritance with virtual inherited states and per-state overrides", "Спадкування шарів із віртуальними успадкованими станами та точковими override")),
                ("Controller Reuse", "AnimatorOverrideController mostly swaps clips", HonamiDocLocalization.Get("Override Controller can inherit layers, parameters and states, then replace nodes or add local layers", "Override Controller успадковує шари, параметри й стани, а потім замінює вузли або додає локальні шари")),
                ("Debugging", "Limited runtime visibility", HonamiDocLocalization.Get("Live node highlighting, state progress, layer weights and transition values", "Живе підсвічування вузлів, прогрес станів, ваги шарів і значення переходів")),
                ("Extensibility", "Difficult to extend without editor tooling", HonamiDocLocalization.Get("Create custom node types and state sub-nodes in C#", "Створення кастомних типів вузлів і state sub-nodes у C#")),
                ("Version Control", "Animator assets often produce noisy diffs", HonamiDocLocalization.Get("Scriptable assets with smaller, clearer changes", "Scriptable assets із меншими та зрозумілішими змінами"))
            );

            HonamiDocumentationBuilder.AddCallout(root,
                HonamiDocLocalization.Get(
                    "Honami still requires a Unity Animator component as the low-level bridge for skeletal output, culling and Root Motion. The Animator Controller slot should stay empty because Honami replaces the AnimatorController logic.",
                    "Honami все ще потребує компонент Unity Animator як низькорівневий міст для скелетного виводу, кулінгу та Root Motion. Слот Animator Controller має лишатися порожнім, бо Honami замінює логіку AnimatorController."
                ),
                HonamiDocumentationBuilder.CalloutType.Warning);

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("The Non-Humanoid Problem", "Проблема non-humanoid"), HonamiEditorIcons.BlendTreeWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "The main reason to use Honami is not only speed or nicer graphs. It is freedom from Unity's Humanoid-first assumptions. If a project has many enemies with strange silhouettes, Unity's Avatar setup, retargeting and Avatar Masks quickly become production friction.",
                "Головна причина використовувати Honami — не лише швидкість або красивіші графи. Це свобода від Unity Humanoid-first assumptions. Якщо в проєкті багато ворогів із дивними силуетами, Unity Avatar setup, retargeting та Avatar Masks швидко стають виробничим тертям."
            ));

            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Quadrupeds should not pretend to be bipeds just to get layered animation.", "Quadrupeds не мають прикидатися bipeds лише заради layered animation."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Tentacles, armor shells, weapon rigs and split bodies need bone-path masks, not humanoid body sections.", "Tentacles, armor shells, weapon rigs та split bodies потребують bone-path masks, а не humanoid body sections."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Honami blends arbitrary bone hierarchies through its own Avatar and mask atlas.", "Honami змішує довільні bone hierarchies через власний Avatar та mask atlas."));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Migration Mindset", "Мислення при міграції"), HonamiEditorIcons.GraphWhite);
            HonamiDocumentationBuilder.AddTable(root,
                (HonamiDocLocalization.Get("Animator habit", "Звичка Animator"), 190),
                (HonamiDocLocalization.Get("Honami approach", "Підхід Honami"), 0),
                ("", 0),
                ("Any State spam", HonamiDocLocalization.Get("Use Any State/Repeater only for global interrupts; keep ordinary flow local to states.", "Використовуйте Any State/Repeater лише для глобальних переривань; звичайний flow тримайте локальним для станів."), ""),
                ("Many duplicate layers", HonamiDocLocalization.Get("Use layer inheritance and override only the states that differ.", "Використовуйте спадкування шарів і перевизначайте лише стани, які відрізняються."), ""),
                ("Animation Events everywhere", HonamiDocLocalization.Get("Use Honami events or Sub-Nodes so gameplay effects live with the active state.", "Використовуйте Honami events або Sub-Nodes, щоб gameplay-ефекти жили поруч з активним станом."), ""),
                ("Clip-only override controllers", HonamiDocLocalization.Get("Use Honami Override Controllers when a variant needs different nodes, layers or parameters.", "Використовуйте Honami Override Controllers, коли варіанту потрібні інші вузли, шари або параметри."), ""),
                ("Blend tree as a black box", HonamiDocLocalization.Get("Use explicit parameters, live debug values and custom nodes for readable behavior.", "Використовуйте явні параметри, live-debug значення та кастомні вузли для читабельної поведінки."), "")
            );

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Code Comparison", "Порівняння в коді"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddCodeBlock(root,
@"// Unity Animator style
animator.SetFloat(""Speed"", speed);
animator.SetTrigger(""Reload"");
animator.CrossFade(""Reload"", 0.12f, 1);

// Honami style
honami.SetFloat(""Speed"", speed);
honami.PlayState(""Reload"", transitionDuration: 0.12f, layer: 1, forceRestart: true);");

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Honami vs. Tweening (DOTween/LeanTween)", "Honami проти Tween-систем"), HonamiEditorIcons.TimelineWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Tweens are excellent for UI and simple linear movements. However, they lack the skeletal blending, layer masking, and state logic required for complex character behavior. Honami provides the same 'code-first' control as tweens but for full skeletal animation.",
                "Твіни чудові для UI та простих лінійних переміщень. Проте їм бракує змішування скелета, маскування шарів та логіки станів, необхідних для складної поведінки персонажа. Honami надає такий самий контроль через код, як і твіни, але для повноцінної скелетної анімації."
            ));

            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Use tweens for UI panels, floating pickups and simple prop motion.", "Використовуйте твіни для UI-панелей, floating pickups і простого руху props."));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get("Use Honami when the pose must blend with locomotion, masks, transitions, Root Motion or state tags.", "Використовуйте Honami, коли поза має змішуватися з locomotion, масками, переходами, Root Motion або state tags."));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("When to use Timeline?", "Коли використовувати Timeline?"), HonamiEditorIcons.TimelineWhite);
            HonamiDocumentationBuilder.AddCallout(root,
                HonamiDocLocalization.Get(
                    "Use Timeline for non-interactive cutscenes where timing is fixed. Use Honami for gameplay where animations must react instantly to player input and environmental changes.",
                    "Використовуйте Timeline для неінтерактивних кат-сцен із фіксованим таймінгом. Використовуйте Honami для геймплею, де анімації мають миттєво реагувати на введення гравця та зміни в оточенні."
                ),
                HonamiDocumentationBuilder.CalloutType.Tip);
        }
    }
}

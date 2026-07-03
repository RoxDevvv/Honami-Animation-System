using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Documentation.Pages
{
    public sealed class BlendingAndLayersPage : IHonamiDocumentationPage
    {
        public string Title => HonamiDocLocalization.Get("Blending & Layers", "Змішування та шари");
        public string Category => HonamiDocLocalization.Get("04. Core Systems", "04. Основні системи");
        public string SearchKeywords => "blending layers additive override masking weight inheritance child layer parent layer змішування шари маски спадкування";
        public int Order => 420;
        public int EstimatedReadTime => 7;

        public void BuildContent(VisualElement root)
        {
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Layers allow you to run multiple animation states simultaneously. This is essential for complex characters that need to perform actions while moving.",
                "Шари дозволяють запускати кілька станів анімації одночасно. Це необхідно для складних персонажів, яким потрібно виконувати дії під час руху."
            ));

            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Think of a layer as an independent animation graph with its own default state, transitions, weight, mask and optional mirroring. The final pose is produced by blending layer outputs from bottom to top.",
                "Думайте про шар як про незалежний анімаційний граф із власним дефолтним станом, переходами, вагою, маскою та опціональним віддзеркаленням. Фінальна поза створюється змішуванням виходів шарів знизу вгору."
            ));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Layer Types", "Типи шарів"), HonamiEditorIcons.BlendTreeWhite);
            HonamiDocumentationBuilder.AddFeatureGrid(root,
                (HonamiDocLocalization.Get("Override", "Перекриття (Override)"), HonamiDocLocalization.Get("Completely replaces the animations of the layers below it based on weight and mask.", "Повністю замінює анімації нижніх шарів залежно від ваги та маски."), HonamiEditorIcons.Controller),
                (HonamiDocLocalization.Get("Additive", "Адитивний (Additive)"), HonamiDocLocalization.Get("Adds its motion on top of the existing pose. Ideal for breathing or recoil.", "Додає свій рух поверх існуючої пози. Ідеально для дихання або віддачі."), HonamiEditorIcons.TimelineWhite)
            );

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Common Layer Setups", "Типові схеми шарів"), HonamiEditorIcons.GraphWhite);
            HonamiDocumentationBuilder.AddTable(root,
                (HonamiDocLocalization.Get("Layer", "Шар"), 160),
                (HonamiDocLocalization.Get("Mask", "Маска"), 140),
                (HonamiDocLocalization.Get("Purpose", "Призначення"), 0),
                ("Base Locomotion", "Full Body", HonamiDocLocalization.Get("Idle, walk, run, jump and fall. Usually weight 1.0 and never disabled.", "Idle, ходьба, біг, стрибок та падіння. Зазвичай вага 1.0 і шар ніколи не вимикається.")),
                ("Upper Body Action", "Spine + Arms", HonamiDocLocalization.Get("Reload, shoot, throw, interact while legs keep following locomotion.", "Перезарядка, стрільба, кидок або взаємодія, поки ноги продовжують рухатися з locomotion.")),
                ("Additive Detail", "Selected Bones", HonamiDocLocalization.Get("Breathing, camera sway, recoil, small procedural corrections.", "Дихання, похитування камери, віддача, дрібні процедурні корекції.")),
                ("Damage Reaction", "Hit Region", HonamiDocLocalization.Get("Short one-shot reactions on the affected body region.", "Короткі one-shot реакції лише на ураженій частині тіла."))
            );

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Masking", "Маскування"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Avatar Masks define which bones a layer should affect. For example, an 'Upper Body' mask allows a player to reload or wave while the 'Base' layer handles running.",
                "Маски аватара визначають, на які кістки впливає шар. Наприклад, маска «Upper Body» дозволяє гравцеві перезаряджатися або махати рукою, поки базовий шар обробляє біг."
            ));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Layer Inheritance", "Спадкування шарів"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Layer inheritance lets a child layer reuse the states and transitions of an earlier parent layer. Honami creates virtual inherited states for the child layer, so the graph stays compact while the runtime still has a complete layer-specific state set.",
                "Спадкування шарів дозволяє дочірньому шару повторно використовувати стани та переходи попереднього батьківського шару. Honami створює віртуальні успадковані стани для дочірнього шару, тому граф залишається компактним, а рантайм усе одно має повний набір станів для конкретного шару."
            ));

            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get(
                "Create it from a layer's options menu with Create Child Layer.",
                "Створюється з меню шару через Create Child Layer."
            ));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get(
                "The child layer inherits the parent's states, default state and transitions until you create an override.",
                "Дочірній шар успадковує стани, дефолтний стан і переходи батьківського шару, доки ви не створите override."
            ));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get(
                "Inherited states are read-only in the graph. Use Create Override when this layer needs a different node, sub-nodes, events, tags or transitions.",
                "Успадковані стани в графі read-only. Використовуйте Create Override, коли цьому шару потрібен інший вузол, підвузли, події, теги або переходи."
            ));
            HonamiDocumentationBuilder.AddBulletPoint(root, HonamiDocLocalization.Get(
                "Only earlier layers can be parents. This prevents circular dependencies and keeps evaluation order deterministic.",
                "Батьківськими можуть бути лише попередні шари. Це запобігає циклічним залежностям і зберігає детермінований порядок обчислення."
            ));

            HonamiDocumentationBuilder.AddCallout(root,
                HonamiDocLocalization.Get(
                    "Example: build Base Locomotion once, then create a child layer named Injured Locomotion. Override only Walk and Run with limping clips. Idle, Jump, Fall and their transitions remain inherited.",
                    "Приклад: створіть Base Locomotion один раз, потім зробіть дочірній шар Injured Locomotion. Перевизначте лише Walk і Run кліпами кульгавості. Idle, Jump, Fall та їхні переходи залишаться успадкованими."
                ),
                HonamiDocumentationBuilder.CalloutType.Tip);

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Runtime Control", "Керування в рантаймі"), HonamiEditorIcons.GraphWhite);
            HonamiDocumentationBuilder.AddCodeBlock(root,
@"// Fade an upper-body action layer in and out.
honami.SetLayerWeight(1, isAiming ? 1f : 0f);

// Trigger a state on a specific layer.
honami.PlayState(""Reload"", transitionDuration: 0.12f, layer: 1, forceRestart: true);

// Mirror the whole avatar smoothly when gameplay asks for it.
honami.SetGlobalMirrorSpeed(8f);
honami.SetGlobalMirror(shouldMirror);");

            HonamiDocumentationBuilder.AddTip(root, HonamiDocLocalization.Get(
                "Always use the lowest weight possible for additive layers to avoid over-stretching the skeleton.",
                "Завжди використовуйте мінімально необхідну вагу для адитивних шарів, щоб уникнути надмірного розтягування скелета."
            ));
        }
    }
}

using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Documentation.Pages
{
    public sealed class EventSystemPage : IHonamiDocumentationPage
    {
        public string Title => HonamiDocLocalization.Get("Event System", "Система подій");
        public string Category => HonamiDocLocalization.Get("04. Core Systems", "04. Основні системи");
        public string SearchKeywords => "events markers animation events callbacks sounds effects local global receiver unityevent події маркери колбеки";
        public int Order => 440;
        public int EstimatedReadTime => 7;

        public void BuildContent(VisualElement root)
        {
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "The Honami Event System triggers logic precisely at specific moments of an animation. It has two halves: event markers authored on states, and C# events raised by the animator itself when states start and end.",
                "Система подій Honami запускає логіку точно у визначені моменти анімації. Вона складається з двох частин: маркери подій, розставлені на станах, і C#-події, які сам аніматор викликає на початку та в кінці станів."
            ));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Event Markers", "Маркери подій"), HonamiEditorIcons.TimelineWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Markers are placed on the state's timeline. When the playhead passes a marker, its event fires. Each marker is either Local or Global.",
                "Маркери розміщуються на таймлайні стану. Коли головка відтворення проходить маркер, спрацьовує його подія. Кожен маркер є або Local, або Global."
            ));
            HonamiDocumentationBuilder.AddTable(root,
                (HonamiDocLocalization.Get("Type", "Тип"), 100),
                (HonamiDocLocalization.Get("How it fires", "Як спрацьовує"), 0),
                ("", 0),
                ("Local", HonamiDocLocalization.Get("Calls TriggerEvent(eventName) on the HonamiLocalEventReceiver component next to the animator. The receiver maps names to UnityEvents you wire up in the inspector — no code required.", "Викликає TriggerEvent(eventName) на компоненті HonamiLocalEventReceiver поруч з аніматором. Ресівер зіставляє назви з UnityEvent, які ви налаштовуєте в інспекторі — код не потрібен."), ""),
                ("Global", HonamiDocLocalization.Get("Finds HonamiGlobalEvent components on the animator's GameObject whose eventId matches and calls their ExecuteEvent(). Subclass HonamiGlobalEvent to write your own reusable event behaviours.", "Знаходить на GameObject аніматора компоненти HonamiGlobalEvent з відповідним eventId і викликає їхній ExecuteEvent(). Наслідуйте HonamiGlobalEvent, щоб писати власні перевикористовувані події."), "")
            );

            HonamiDocumentationBuilder.AddCallout(root, HonamiDocLocalization.Get(
                "Both the HonamiLocalEventReceiver and HonamiGlobalEvent components are discovered on the same GameObject as the HonamiAnimator during initialization. Add them next to the animator, not on children.",
                "І HonamiLocalEventReceiver, і компоненти HonamiGlobalEvent шукаються на тому самому GameObject, що й HonamiAnimator, під час ініціалізації. Додавайте їх поруч з аніматором, а не на дочірні об'єкти."
            ), HonamiDocumentationBuilder.CalloutType.Info);

            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Marker guarantees: on looping states markers re-fire every loop (if several loops pass in one tick, at most 3 are replayed); on reversed states markers fire when the playhead moves back past them; paused states and paused layers do not fire events; TryAutoSkipState with cancelEvents: true suppresses markers the skipped state has not fired yet.",
                "Гарантії маркерів: на loop-станах маркери спрацьовують кожен цикл (якщо за один тік минуло кілька циклів — програється щонайбільше 3); на реверсних станах маркери спрацьовують, коли головка проходить їх у зворотному напрямку; стани та шари на паузі подій не генерують; TryAutoSkipState із cancelEvents: true скасовує маркери, які пропущений стан ще не встиг викликати."
            ));

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("Built-in global events", "Вбудовані глобальні події"), HonamiEditorIcons.Controller);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "HonamiRigWeightEvent is a ready-made HonamiGlobalEvent that sets the weight of a HonamiRig when executed — handy for enabling IK exactly on the frame a hand grabs a ladder.",
                "HonamiRigWeightEvent — готова HonamiGlobalEvent, яка при виконанні встановлює вагу HonamiRig — зручно, щоб увімкнути IK точно на кадрі, коли рука хапає драбину."
            ));
            HonamiDocumentationBuilder.AddCodeBlock(root,
@"using HonamiAnimationSystem.Runtime.Events;
using UnityEngine;

public sealed class FootstepEvent : HonamiGlobalEvent
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;

    public override void ExecuteEvent()
    {
        if (clips.Length > 0)
            source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
// Set eventId = ""Footstep"" in the inspector and reference it
// from a Global marker on any state.");

            HonamiDocumentationBuilder.AddHeader(root, HonamiDocLocalization.Get("C# state events", "C#-події станів"), HonamiEditorIcons.GraphWhite);
            HonamiDocumentationBuilder.AddParagraph(root, HonamiDocLocalization.Get(
                "Independently of markers, HonamiAnimator raises three C# events: OnStateEntered(string), OnStateFinished(string) for non-loop states that reached their end, and OnStateExited(HonamiStateExitInfo) with the exact exit reason. See the Scripting API page for the full HonamiStateExitInfo reference.",
                "Незалежно від маркерів, HonamiAnimator викликає три C#-події: OnStateEntered(string), OnStateFinished(string) для non-loop станів, що дійшли до кінця, та OnStateExited(HonamiStateExitInfo) з точною причиною виходу. Повний довідник HonamiStateExitInfo — на сторінці Scripting API."
            ));
            HonamiDocumentationBuilder.AddCodeBlock(root,
@"private void OnEnable()
{
    honami.OnStateEntered += HandleEntered;
    honami.OnStateFinished += HandleFinished;
}

private void OnDisable()
{
    honami.OnStateEntered -= HandleEntered;
    honami.OnStateFinished -= HandleFinished;
}

private void HandleEntered(string state)
{
    if (state == ""Reload"") ammoUI.ShowReloadSpinner();
}

private void HandleFinished(string state)
{
    if (state == ""Reload"") weapon.FillMagazine();
}");

            HonamiDocumentationBuilder.AddTip(root, HonamiDocLocalization.Get(
                "Use markers for content-driven moments (footsteps, VFX, damage windows) and C# events for structural logic (state machines, combos, UI). Markers live with the animation data; C# events live with your gameplay code.",
                "Використовуйте маркери для контентних моментів (кроки, VFX, вікна урону), а C#-події — для структурної логіки (стейт-машини, комбо, UI). Маркери живуть разом з анімаційними даними; C#-події — разом з ігровим кодом."
            ));
        }
    }
}

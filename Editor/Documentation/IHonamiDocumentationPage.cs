using UnityEngine.UIElements;

namespace HonamiAnimationSystem.Editor.Documentation
{
    public interface IHonamiDocumentationPage
    {
        string Title { get; }
        string Category { get; }
        string SearchKeywords { get; }
        int Order { get; }
        int EstimatedReadTime { get; }
        void BuildContent(VisualElement root);
    }
}

namespace TinyFactory.Interaction
{
    public interface ISelectable
    {
        string DisplayName { get; }
        string StatusText { get; }

        void Select(StationSelectionController selectionController);
        void Deselect(StationSelectionController selectionController);
    }
}

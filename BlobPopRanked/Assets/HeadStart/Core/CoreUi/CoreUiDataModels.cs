namespace Assets.HeadStart.CoreUi
{
    public enum UiDependency
    {
        ScreenPoints
    }

    public interface IUiDependency
    {
        void Register(CoreUiObservedValue obj);
    }

    public class CoreUiObservedValue
    {
        public CoreUiObservedValue() { }
    }
}

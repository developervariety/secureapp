namespace SecureApp.Utilities.Model.Interface
{
    public interface IPlugin
    {
        string Version { get; }
        string Author { get; }
        string Description { get; }
        string Name { get; }

        void Init();
        void Execute(params object[] args);
    }
}
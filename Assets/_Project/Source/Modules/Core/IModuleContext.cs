using Zenject;

namespace Core
{
    /// <summary>Module context: access to other modules and dependency resolution.</summary>
    public interface IModuleContext
    {
        IModule GetModule(string moduleName);
        T GetModuleFacade<T>() where T : class;
        DiContainer Container { get; }
    }
}

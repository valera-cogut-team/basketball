using System;
using System.Collections.Generic;
using Zenject;

namespace Core
{
    /// <summary>Default implementation of <see cref="IModuleContext"/>.</summary>
    public class ModuleContext : IModuleContext
    {
        private readonly ModuleManager _moduleManager;
        private readonly DiContainer _container;
        private readonly Dictionary<Type, object> _facadeCache = new Dictionary<Type, object>();

        public DiContainer Container => _container;

        public ModuleContext(ModuleManager moduleManager, DiContainer container)
        {
            _moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IModule GetModule(string moduleName) => _moduleManager.GetModule(moduleName);

        public T GetModuleFacade<T>() where T : class
        {
            var type = typeof(T);
            if (_facadeCache.TryGetValue(type, out var cached))
                return cached as T;
            if (_container.HasBinding<T>())
            {
                var facade = _container.Resolve<T>();
                _facadeCache[type] = facade;
                return facade;
            }
            return null;
        }
    }
}

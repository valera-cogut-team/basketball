using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zenject;

namespace Core
{
    /// <summary>Module lifecycle manager. Pure C# (no Unity types).</summary>
    public class ModuleManager
    {
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();
        private readonly Dictionary<string, IModule> _enabledModules = new Dictionary<string, IModule>();
        private readonly DiContainer _container;
        private ModuleContext _context;

        public IModuleContext Context => _context;
        public DiContainer Container => _container;

        public ModuleManager(DiContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _context = new ModuleContext(this, container);
        }

        public void RegisterModule(IModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (_modules.ContainsKey(module.Name)) throw new InvalidOperationException($"Module '{module.Name}' is already registered");
            _modules[module.Name] = module;
        }

        public void InitializeAll()
        {
            var sorted = SortByDependencies(_modules.Values);
            foreach (var m in sorted)
            {
                try { m.Initialize(_context); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Basketball.ModuleManager: Failed to initialize '{m.Name}': {ex.Message}");
                    throw;
                }
            }
        }

        public void EnableModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var m)) { Debug.WriteLine($"Basketball.ModuleManager: Module '{moduleName}' not found"); return; }
            if (m.IsEnabled) return;
            if (!AreDependenciesEnabled(m)) { Debug.WriteLine($"Basketball.ModuleManager: Cannot enable '{moduleName}': dependencies not met"); return; }
            m.Enable();
            _enabledModules[moduleName] = m;
        }

        public void DisableModule(string moduleName)
        {
            if (!_enabledModules.TryGetValue(moduleName, out var m)) return;
            if (HasDependents(moduleName)) { Debug.WriteLine($"Basketball.ModuleManager: Cannot disable '{moduleName}': dependents exist"); return; }
            m.Disable();
            _enabledModules.Remove(moduleName);
        }

        public IModule GetModule(string moduleName)
        {
            _modules.TryGetValue(moduleName, out var m);
            return m;
        }

        public T GetModuleFacade<T>() where T : class => _context.GetModuleFacade<T>();

        public void ShutdownAll()
        {
            var sorted = SortByDependencies(_modules.Values);
            for (var i = sorted.Count - 1; i >= 0; i--)
            {
                try { sorted[i].Shutdown(); }
                catch (Exception ex) { Debug.WriteLine($"Basketball.ModuleManager: Shutdown '{sorted[i].Name}': {ex.Message}"); }
            }
            _enabledModules.Clear();
            _modules.Clear();
        }

        private bool AreDependenciesEnabled(IModule m)
        {
            foreach (var d in m.Dependencies)
                if (!_enabledModules.ContainsKey(d)) return false;
            return true;
        }

        private bool HasDependents(string name)
        {
            foreach (var m in _enabledModules.Values)
                if (Array.IndexOf(m.Dependencies, name) >= 0) return true;
            return false;
        }

        private List<IModule> SortByDependencies(IEnumerable<IModule> modules)
        {
            var sorted = new List<IModule>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();
            var map = new Dictionary<string, IModule>();
            foreach (var m in modules)
                if (m != null && !map.ContainsKey(m.Name)) map[m.Name] = m;
            foreach (var m in modules)
                Visit(m, map, sorted, visited, visiting);
            return sorted;
        }

        private void Visit(IModule m, Dictionary<string, IModule> all, List<IModule> sorted, HashSet<string> visited, HashSet<string> visiting)
        {
            if (visited.Contains(m.Name)) return;
            if (visiting.Contains(m.Name)) throw new InvalidOperationException($"Circular dependency: {m.Name}");
            visiting.Add(m.Name);
            foreach (var d in m.Dependencies)
                if (all.TryGetValue(d, out var dep)) Visit(dep, all, sorted, visited, visiting);
            visiting.Remove(m.Name);
            visited.Add(m.Name);
            sorted.Add(m);
        }
    }
}

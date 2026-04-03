using UnityEngine;
using Core;
using LifeCycle.Application;
using LifeCycle.Facade;
using LifeCycle.Infrastructure;
using Logger.Facade;
using Zenject;

namespace LifeCycle
{
    public class LifeCycleModule : IModule
    {
        public string Name => "LifeCycle";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private ILifeCycleService _service;
        private ILifeCycleFacade _facade;
        private LifeCycleMonoBehaviour _mono;

        public void Initialize(IModuleContext context)
        {
            _context = context;
            var logger = context.GetModuleFacade<ILoggerFacade>();
            _service = new LifeCycleService(logger);
            context.Container.Bind<ILifeCycleService>().FromInstance(_service).AsSingle();
            _facade = new LifeCycleFacade(_service);
            context.Container.Bind<ILifeCycleFacade>().FromInstance(_facade).AsSingle();
            var go = new GameObject("LifeCycleMonoBehaviour");
            _mono = go.AddComponent<LifeCycleMonoBehaviour>();
            _mono.Initialize(_facade);
            Object.DontDestroyOnLoad(go);
        }

        public void Enable() { if (IsEnabled) return; IsEnabled = true; _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("LifeCycleModule enabled"); }
        public void Disable() { if (!IsEnabled) return; IsEnabled = false; _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("LifeCycleModule disabled"); }

        public void Shutdown()
        {
            Disable();
            _service?.Clear();
            if (_mono != null) { Object.Destroy(_mono.gameObject); _mono = null; }
            _service = null;
            _facade = null;
        }
    }
}

using System;
using AppFlow.Application;
using AppFlow.Facade;
using Core;
using Logger.Facade;
using SplashScreen.Facade;
using GameScreen.Facade;
using StateMachine.Facade;
using Zenject;

namespace AppFlow
{
    public sealed class AppFlowModule : IModule
    {
        public string Name => "AppFlow";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "StateMachine", "ScreenRouter", "SplashScreen", "GameScreen" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IAppFlowService _service;
        private IAppFlowFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var logger = context.GetModuleFacade<ILoggerFacade>();
            var sm = context.GetModuleFacade<IStateMachineFacade>();
            var splash = context.GetModuleFacade<ISplashScreenFacade>();
            var game = context.GetModuleFacade<IGameScreenFacade>();

            _service = new AppFlowService(logger, sm, splash, game);
            _facade = new AppFlowFacade(_service);

            context.Container.Bind<IAppFlowService>().FromInstance(_service).AsSingle();
            context.Container.Bind<IAppFlowFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("AppFlowModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("AppFlowModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            if (_service is IDisposable d)
                d.Dispose();

            _service = null;
            _facade = null;
        }
    }
}

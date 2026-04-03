using Audio.Application;
using Audio.Facade;
using Core;
using Logger.Facade;

namespace Audio
{
    public sealed class AudioModule : IModule
    {
        public string Name => "Audio";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IAudioService _service;
        private IAudioFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;
            var logger = context.GetModuleFacade<ILoggerFacade>();
            _service = new AudioService(logger);
            context.Container.Bind<IAudioService>().FromInstance(_service).AsSingle();
            _facade = new AudioFacade(_service);
            context.Container.Bind<IAudioFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            IsEnabled = true;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("AudioModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;
            _service?.StopMusic();
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("AudioModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service?.Shutdown();
            _service = null;
            _facade = null;
        }
    }
}

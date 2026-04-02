using Core;
using Logger.Facade;
using UIRoot.Facade;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Zenject;

namespace UIRoot
{
    /// <summary>
    /// UIRoot module - creates and provides stable roots for screens/popups.
    /// </summary>
    public sealed class UIRootModule : IModule
    {
        public string Name => "UIRoot";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IUIRootFacade _facade;
        private GameObject _rootsGo;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            _rootsGo = new GameObject("UIRoot");
            Object.DontDestroyOnLoad(_rootsGo);

            // Root canvas: stable single point for UI scaling + raycasting.
            // We intentionally keep screens and popups under this root to enable:
            // - predictable canvas setup (AAA standard)
            // - controlled canvas splitting (rebuild isolation)
            // - fewer accidental per-screen root canvases in Addressables prefabs
            var canvasGo = new GameObject("CanvasRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(_rootsGo.transform, worldPositionStays: false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Screens container (no extra Canvas here by default).
            // If we later need rebuild isolation inside a screen, we do it via nested canvases locally.
            var screens = new GameObject("ScreensRoot", typeof(RectTransform));
            screens.transform.SetParent(canvasGo.transform, worldPositionStays: false);

            // Popups get their own canvas to isolate rebuild spikes from screens and to guarantee overlay ordering.
            var popupsCanvasGo = new GameObject("PopupsCanvas", typeof(RectTransform), typeof(Canvas));
            popupsCanvasGo.transform.SetParent(canvasGo.transform, worldPositionStays: false);
            var popupsCanvas = popupsCanvasGo.GetComponent<Canvas>();
            popupsCanvas.overrideSorting = true;
            popupsCanvas.sortingOrder = 1000;

            var popups = new GameObject("PopupsRoot", typeof(RectTransform));
            popups.transform.SetParent(popupsCanvasGo.transform, worldPositionStays: false);

            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventGo.transform.SetParent(_rootsGo.transform, worldPositionStays: false);
            var eventSystem = eventGo.GetComponent<EventSystem>();
            eventSystem.sendNavigationEvents = false;

            _facade = new UIRootFacade(canvasGo.transform, screens.transform, popups.transform);

            context.Container.Bind<IUIRootFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IUIRootState>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            IsEnabled = true;

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("UIRootModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("UIRootModule disabled");
        }

        public void Shutdown()
        {
            Disable();

            if (_rootsGo != null)
            {
                Object.Destroy(_rootsGo);
                _rootsGo = null;
            }

            _facade = null;
        }
    }
}



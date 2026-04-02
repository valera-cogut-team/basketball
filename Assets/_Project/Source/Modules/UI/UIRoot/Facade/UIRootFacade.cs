using UnityEngine;

namespace UIRoot.Facade
{
    public sealed class UIRootFacade : IUIRootFacade
    {
        public Transform CanvasRoot { get; }
        public Transform ScreensRoot { get; }
        public Transform PopupsRoot { get; }

        public UIRootFacade(Transform canvasRoot, Transform screensRoot, Transform popupsRoot)
        {
            CanvasRoot = canvasRoot;
            ScreensRoot = screensRoot;
            PopupsRoot = popupsRoot;
        }
    }
}



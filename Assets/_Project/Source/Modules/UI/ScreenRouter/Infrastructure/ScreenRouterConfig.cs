using UnityEngine;

namespace ScreenRouter.Infrastructure
{
    /// <summary>
    /// Optional runtime configuration for <see cref="ScreenRouterService"/>.
    /// Unity-runtime configuration by design (references UnityEngine types).
    /// </summary>
    public sealed class ScreenRouterConfig
    {
        /// <summary>
        /// Root name for created router GameObject (DontDestroyOnLoad).
        /// </summary>
        public string RootName { get; set; } = "ScreenRouterRoot";

        /// <summary>
        /// Parent transform for instantiated screens (if null, router uses <see cref="UIRoot.Facade.IUIRootFacade.ScreensRoot"/>).
        /// </summary>
        public Transform ScreensParent { get; set; }

        /// <summary>
        /// If true, previous screen GameObjects are kept inactive in stack (enables fast back).
        /// If false, previous screens are destroyed on navigation.
        /// </summary>
        public bool KeepPreviousScreensInMemory { get; set; } = true;
    }
}


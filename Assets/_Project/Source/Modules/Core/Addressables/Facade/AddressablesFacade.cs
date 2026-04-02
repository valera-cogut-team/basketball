using System;
using Cysharp.Threading.Tasks;
using Addressables.Infrastructure;
using Core;
using Logger.Facade;
using UnityEngine;

namespace Addressables.Facade
{
    public sealed class AddressablesFacade : IAddressablesFacade
    {
        private readonly IAddressablesService _service;
        private readonly IModuleContext _context;

        public AddressablesFacade(IAddressablesService service, IModuleContext context)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _context = context;
        }

        public UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
            => _service.LoadAssetAsync<T>(address);

        public UniTask<GameObject> LoadPrefabAsync(string address)
            => _service.LoadAssetAsync<GameObject>(address);

        public UniTask ReleaseAssetAsync(string address)
            => _service.ReleaseAssetAsync(address);

        public bool IsAssetLoaded(string address)
            => _service.IsAssetLoaded(address);

        internal void LogInfo(string message) => _context?.GetModuleFacade<ILoggerFacade>()?.LogInfo(message);
    }
}


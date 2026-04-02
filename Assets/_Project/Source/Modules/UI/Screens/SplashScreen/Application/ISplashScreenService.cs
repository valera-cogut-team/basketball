using System.Threading;
using Cysharp.Threading.Tasks;

namespace SplashScreen.Application
{
    public interface ISplashScreenService
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}

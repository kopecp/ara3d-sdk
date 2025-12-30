using Ara3D.Logging;

namespace Ara3D.Studio.API;

public interface IHostApplication
{
    ILogger Logger { get; }
    void Invalidate(object obj);
    void RefreshUI(object obj);
}

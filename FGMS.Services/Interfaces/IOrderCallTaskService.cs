namespace FGMS.Services.Interfaces
{
    public interface IOrderCallTaskService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}

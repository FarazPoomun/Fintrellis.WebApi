namespace Fintrellis.Services.Interfaces
{
    public interface IRetryHandler
    {
        Task ExecuteWithRetryAsync(Func<Task> action);
    }
}

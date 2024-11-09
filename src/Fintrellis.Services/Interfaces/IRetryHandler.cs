namespace Fintrellis.Services.Interfaces
{

    public interface IRetryHandler
    {
        /// <summary>
        /// Executes an async function with retry policies
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task ExecuteWithRetryAsync(Func<Task> action);
    }
}

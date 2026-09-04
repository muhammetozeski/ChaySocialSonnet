namespace ChaySocialSonnet.MainProject.UI.Architecture
{
    /// <summary>
    /// Reusable busy/error state for an async UI action (e.g. a button click that awaits a network
    /// call). Call <see cref="RunAsync"/> from the event handler; it sets <see cref="IsBusy"/>, clears
    /// <see cref="ErrorMessage"/>, runs the work, catches any exception into <see cref="ErrorMessage"/>
    /// instead of letting it propagate unhandled out of a Blazor event handler, and always clears
    /// <see cref="IsBusy"/> afterwards — so a failed network call can never leave a button stuck
    /// disabled with no explanation.
    /// </summary>
    /// <param name="onStateChanged">Invoked after every state transition (typically the component's <c>StateHasChanged</c>) so the UI re-renders.</param>
    public sealed class AsyncOperationState(Action onStateChanged)
    {
        public bool IsBusy { get; private set; }
        public string? ErrorMessage { get; private set; }

        public async Task RunAsync(Func<Task> work)
        {
            IsBusy = true;
            ErrorMessage = null;
            onStateChanged();

            try
            {
                await work();
            }
            catch (Exception exception)
            {
                Log(exception, LogLevel.Warning);
                ErrorMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
                onStateChanged();
            }
        }
    }
}

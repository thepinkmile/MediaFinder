namespace MediaFinder.Helpers;

public abstract class ReactiveBackgroundWorkerContextBase
{
    protected ReactiveBackgroundWorkerContextBase(object progressToken)
    {
        ProgressToken = progressToken;
    }

    public object ProgressToken { get; }
}

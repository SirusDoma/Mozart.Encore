namespace Encore.Events;

public interface IEventPublisher<in T>
    where T : class
{
    void Monitor(T target);
}

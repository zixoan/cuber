using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Provider;

public interface ITargetProvider
{
    int Count { get; }
    IList<Target> Targets { get; }

    Target this[int index] { get; }

    void Add(Target target);
    void Remove(int index);
    Target? Aggregate(Func<Target, Target, Target> accumulator);
}
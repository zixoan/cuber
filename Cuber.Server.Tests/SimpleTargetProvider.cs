using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Tests;

public class SimpleTargetProvider : ITargetProvider
{
    private readonly IList<Target> targets;

    public Target this[int index] => this.targets[index];

    public int Count => this.targets.Count;

    public IList<Target> Targets => this.targets;

    public SimpleTargetProvider(IList<Target> targets)
        => this.targets = targets;

    public void Add(Target target)
        => this.targets.Add(target);

    public Target Aggregate(Func<Target, Target, Target> accumulator)
        => this.targets.Aggregate(accumulator);

    public void Remove(int index)
        => this.targets.RemoveAt(index);
}
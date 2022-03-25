using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Provider;

public class ThreadSafeTargetProvider : ITargetProvider
{
    private readonly IList<Target> targets;
    private readonly ReaderWriterLockSlim readWriteLock;

    public ThreadSafeTargetProvider(IEnumerable<Target> targets)
    {
        this.targets = new List<Target>(targets);
        this.readWriteLock = new ReaderWriterLockSlim();
    }

    public int Count
    {
        get
        {
            this.readWriteLock.EnterReadLock();

            try
            {
                return this.targets.Count;
            }
            finally
            {
                this.readWriteLock.ExitReadLock();
            }
        }
    }

    public IList<Target> Targets
    {
        get
        {
            this.readWriteLock.EnterReadLock();

            try
            {
                return new List<Target>(this.targets);
            }
            finally
            {
                this.readWriteLock.ExitReadLock();
            }
        }
    }

    public Target this[int index]
    {
        get
        {
            this.readWriteLock.EnterReadLock();

            try
            {
                return this.targets[index];
            }
            finally
            {
                this.readWriteLock.ExitReadLock();
            }
        }
    }

    public void Add(Target target)
    {
        this.readWriteLock.EnterWriteLock();

        try
        {
            this.targets.Add(target);
        }
        finally
        {
            this.readWriteLock.ExitWriteLock();
        }
    }

    public void Remove(int index)
    {
        this.readWriteLock.EnterWriteLock();

        try
        {
            this.targets.RemoveAt(index);
        }
        finally
        {
            this.readWriteLock.ExitWriteLock();
        }
    }

    public Target? Aggregate(Func<Target, Target, Target> accumulator)
    {
        this.readWriteLock.EnterReadLock();

        try
        {
            return this.targets.Count == 0 
                ? null
                : this.targets.Aggregate(accumulator);
        }
        finally
        {
            this.readWriteLock.ExitReadLock();
        }
    }
}
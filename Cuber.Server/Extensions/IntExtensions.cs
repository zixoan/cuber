using System.Runtime.CompilerServices;

namespace Zixoan.Cuber.Server.Extensions;

public static class IntExtensions
{
    /// <summary>
    /// Returns the mathematical floor modulus, where the result has the same sign as the divisor <paramref name="m"/>.
    /// </summary>
    /// <param name="this">The current value as dividend.</param>
    /// <param name="m">The divisor.</param>
    /// <returns>The floor modulus of the given dividend and divisor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorMod(this int @this, int m)
        => @this - (int)Math.Floor((double)@this / m) * m;
}
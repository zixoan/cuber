using Xunit;

using Zixoan.Cuber.Server.Extensions;

namespace Zixoan.Cuber.Server.Tests
{
    public class IntExtensionsTest
    {
        [Theory]
        [InlineData(9, 3, 0)]
        [InlineData(-10, 3, 2)]
        [InlineData(-3, 2, 1)]
        [InlineData(8, 0, 8)]
        [InlineData(202, 3, 1)]
        [InlineData(-23, 5, 2)]
        public void FloorMod(int value, int m, int result)
            => Assert.Equal(result, value.FloorMod(m));
    }
}

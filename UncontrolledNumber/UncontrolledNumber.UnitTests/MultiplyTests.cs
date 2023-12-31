using System.Numerics;
using static UncontrolledNumberUnitTests.RNG;

namespace UncontrolledNumberUnitTests
{
    public class MultiplyTests
    {
        [Fact]
        public void Multiply1()
        {
            // Google 98252.231273 * 121287 = 11916718374.4
            Assert.Equal("11916718374.4",
                (new UncontrolledNumber(98252.231273) * new UncontrolledNumber(121287)).ToString()
                .Substring(0, 13)
                );
        }

        [Fact]
        public void Multiply2()
        {
            // Google 123456789012345678.232 * 987654321098765432.123
            // = 121932631137021794566832799764434994.646536
            Assert.Equal("121932631137021794566832799764434994.646536",
                (new UncontrolledNumber(123456789012345678, 232, 0, false) 
                * new UncontrolledNumber(987654321098765432, 123, 0, false)).ToString()
                .Substring(0, "121932631137021794566832799764434994.646536".Length)
                );
        }
    }
}
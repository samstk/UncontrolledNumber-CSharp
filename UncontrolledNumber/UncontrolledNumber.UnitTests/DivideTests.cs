using System.Numerics;
using static UncontrolledNumberUnitTests.RNG;

namespace UncontrolledNumberUnitTests
{
    public class DivideTests
    {
        [Fact]
        public void Divide1()
        {
            // Google 982.231273 / 128715425.2323289 = 0.00000763103
            Assert.Equal("0.00000763103",
                (new UncontrolledNumber(982.231273) / new UncontrolledNumber(128715425.2323289)).ToString()
                .Substring(0, 13)
                );
        }

        [Fact]
        public void Divide2()
        {
            // Google 982.98252.231273 / 12128715425.2323289 = 0.0.00000810079
            Assert.Equal("0.00000810079",
                (new UncontrolledNumber(98252.231273) / new UncontrolledNumber(12128715425.2323289)).ToString()
                .Substring(0, 13)
                );
        }
    }
}
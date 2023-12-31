using System.Numerics;
using static UncontrolledNumberUnitTests.RNG;

namespace UncontrolledNumberUnitTests
{
    public class SubtractionTests
    {
        [Fact]
        public void SubtractWholeNumbers()
        {
            for (int i = 0; i < 100; i++)
            {
                UncontrolledNumber number1 = RG.Next(-1000000, 1000000);
                UncontrolledNumber number2 = RG.Next(-1000000, 1000000);
                Assert.Equal((int)(number1 - number2).Integer, (int)number1.Integer - (int)number2.Integer);
            }
        }
    }
}
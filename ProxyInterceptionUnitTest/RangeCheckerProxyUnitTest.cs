/**************************************************************************************************
 * Filename    = RangeCheckerProxyUnitTest.cs
 *
 * Author      = Ramaswamy Krishnan-Chittur
 *
 * Product     = AspectOrientedProgramming
 * 
 * Project     = ProxyInterceptionUnitTest
 *
 * Description = Unit tests for the range-checking interception library.
 *************************************************************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using ProxyInterception;
using System;

namespace ProxyInterceptionUnitTest
{
    /// <summary>
    /// Interface for the Math functions. Our proxy will be a reference of this interface.
    /// </summary>
    interface IMath
    {
        public double GetSectorArea([Range<double>(true, Lower = 0.0)] double radius,
                                    [Range<double>(true, Lower = 0.0, Upper = 6.28)] double angle);

        [method: Range<int>(true, Lower = 0, Upper = 1)]
        public int GetMax();
    }

    /// <summary>
    /// Test class derived that implements that Math functions.
    /// </summary>
    class Math : IMath
    {
        public double GetSectorArea(double radius, double angle)
        {
            // Note: Apply range check on the parameters.

            return (3.14 * radius * radius * angle);
        }

        public int GetMax()
        {
            // Note: Apply range check on the return value.

            // Incorrect implementation, just to test out of range exception being generated by interception.
            return -1;
        }
    }

    /// <summary>
    /// Unit test class for proxy interception.
    /// </summary>
    [TestClass]
    public class RangeCheckerProxyUnitTest
    {
        /// <summary>
        /// Tests interception on the parameter arguments.
        /// </summary>
        [TestMethod]
        public void TestRangeCheckOnParameters()
        {
            IMath math = RangeCheckerProxy<IMath>.Decorate(new Math());

            double result = math.GetSectorArea(1, 1);
            Assert.AreEqual(result, 3.14);

            result = math.GetSectorArea(0, 0);
            Assert.AreEqual(result, 0);

            try
            {
                result = math.GetSectorArea(0, -0.5);
                Assert.Fail("Math.GetSectorArea(0, -0.5) should throw an argument out of range exception.");
            }
            catch (Exception exception)
            {
                Logger.LogMessage(exception.Message);
                Assert.AreEqual(exception.GetType(), typeof(ArgumentOutOfRangeException));
            }
        }

        /// <summary>
        /// Tests interception on the return value.
        /// </summary>
        [TestMethod]
        public void TestRangeCheckOnReturnValue()
        {
            try
            {
                IMath math = RangeCheckerProxy<IMath>.Decorate(new Math());
                _ = math.GetMax();
                Assert.Fail("Math.GetProbability() should throw an argument out of range exception.");
            }
            catch (Exception exception)
            {
                Logger.LogMessage(exception.Message);
                Assert.AreEqual(exception.GetType(), typeof(ArgumentOutOfRangeException));
            }
        }
    }
}
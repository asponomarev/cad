using DSS.Wrappers;
using DSS;
using Moq.AutoMock;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Calculations;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace Tests
{
    public class AlgorithmsTests
    {
        [Test]
        public void TestBinarySearch()
        {
            List<ParameterValue> parameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new DoubleParameterValue("Parameter2", 1),
                new DoubleParameterValue("Parameter3", 5.5),

            };

            BinarySearch binarySearch = new BinarySearch("Parameter1", "Characteristic1", 3, 10);
            binarySearch.FirstValue = 1;
            // binarySearch.LastValue = 10;

            for (int i = 0; i < 3; ++i)
            {
                List<ParameterValue> calculationParameters = binarySearch.MakeCalculationParameters(parameters, "Parameter1", i);
            }

            double throughputCharacteristicValue;
            AlgorithmStatus StatusCode;
            // Найденная throughput "хорошая", поменялась первая граница
            throughputCharacteristicValue = 5.5;
            StatusCode = binarySearch.MoveBorder(throughputCharacteristicValue, 2);
            Assert.That(binarySearch.FirstValue, Is.EqualTo(5.5));

            // Найденная throughput "плохая", поменялась последняя граница
            throughputCharacteristicValue = 3;
            StatusCode = binarySearch.MoveBorder(throughputCharacteristicValue, 2);
            Assert.That(binarySearch.LastValue, Is.EqualTo(5.5));



            /*    List<ParameterValue> expectedParameters = new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 1),
                    new DoubleParameterValue("Parameter2", 3),
                    new DoubleParameterValue("Parameter3", 5.5),

                };
                Assert.IsTrue(expectedParameters.SequenceEquals(calculationParameters));*/
        }
    }


}

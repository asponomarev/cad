using DSS;
using Moq.AutoMock;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace Tests
{
    public class NearestNeighborsFinderTests
    {
        [Test]
        public void FoundBiggerAndSmallerNeighbors()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8Parameter8")
            };
            var expectedParameters = new List<List<ParameterValue>>()
            {
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 3),
                    new IntParameterValue("Parameter2", 3),
                    new IntParameterValue("Parameter3", 3),
                    new DoubleParameterValue("Parameter4", 3.3),
                    new DoubleParameterValue("Parameter5", 3.3),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 1),
                    new IntParameterValue("Parameter2", 1),
                    new IntParameterValue("Parameter3", 1),
                    new DoubleParameterValue("Parameter4", 1.1),
                    new DoubleParameterValue("Parameter5", 1.1),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
            };
            var autoMocker = new AutoMocker();
            var nearestNeighborsFinder = autoMocker.CreateInstance<NearestNeighborsFinder>();
            var resultParameters = nearestNeighborsFinder.Find(ClassCreator.GetListsParameters(), knownParameters, 2);
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(expectedParameters.Count, resultParameters.Count);
            CompareParametersLists(expectedParameters, resultParameters);
        }

        [Test]
        public void NeighborsFoundIgnoringBoolAndStringFields()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", true),
                new StringParameterValue("Parameter8", "ParameterTest")
            };
            var expectedParameters = new List<List<ParameterValue>>()
            {
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 3),
                    new IntParameterValue("Parameter2", 3),
                    new IntParameterValue("Parameter3", 3),
                    new DoubleParameterValue("Parameter4", 3.3),
                    new DoubleParameterValue("Parameter5", 3.3),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 1),
                    new IntParameterValue("Parameter2", 1),
                    new IntParameterValue("Parameter3", 1),
                    new DoubleParameterValue("Parameter4", 1.1),
                    new DoubleParameterValue("Parameter5", 1.1),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
            };
            var autoMocker = new AutoMocker();
            var nearestNeighborsFinder = autoMocker.CreateInstance<NearestNeighborsFinder>();
            var resultParameters = nearestNeighborsFinder.Find(ClassCreator.GetListsParameters(), knownParameters, 2);
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(expectedParameters.Count, resultParameters.Count);
            CompareParametersLists(expectedParameters, resultParameters);
        }

        private void CompareParametersLists(List<List<ParameterValue>> firstList, List<List<ParameterValue>> secondList)
        {
            for (var i = 0; i < firstList.Count; i++)
            {
                Assert.IsTrue(ResultsComparator.IsEquals(firstList[i], secondList[i]));
            }
        }
    }
}

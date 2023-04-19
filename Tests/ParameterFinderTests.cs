using DSS;
using DSS.Wrappers;
using Moq;
using Moq.AutoMock;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Infos;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace Tests
{
    public class ParameterFinderTests
    {
        [Test]
        public void FindingMatchingParametersSuccess()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 4.4),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var expectedParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new IntParameterValue("Parameter2", 2),
                new IntParameterValue("Parameter3", 3),
                new DoubleParameterValue("Parameter4", 4.4),
                new DoubleParameterValue("Parameter5", 5.5),
                new BoolParameterValue("Parameter6", true),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(ClassCreator.GetListsParameters()));
            var parameterFinder = autoMocker.CreateInstance<ParameterFinder>();
            var resultParameters = parameterFinder.GetMatching(knownParameters, "Model1").Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(expectedParameters.Count, resultParameters.Count);
            Assert.IsTrue(ResultsComparator.IsEquals(expectedParameters, new List<ParameterValue>(resultParameters)));
        }

        [Test]
        public void NoMatchingParametersFoundForModel()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 4.4),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(new List<List<ParameterValue>>()));
            var parameterFinder = autoMocker.CreateInstance<ParameterFinder>();
            var resultParameters = parameterFinder.GetMatching(knownParameters, "Model4").Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(0, resultParameters.Count);
        }

        [Test]
        public void NoParametersMatchingSpecifiedOnesWereFound()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 100),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 4.4),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(ClassCreator.GetListsParameters()));
            var parameterFinder = autoMocker.CreateInstance<ParameterFinder>();
            var resultParameters = parameterFinder.GetMatching(knownParameters, "Model4").Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(0, resultParameters.Count);
        }

        [Test]
        public void FindingNearestParametersSuccess()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8Parameter8")
            };
            var expectedParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new IntParameterValue("Parameter3", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new DoubleParameterValue("Parameter5", 2.2),
                new BoolParameterValue("Parameter6", true),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(ClassCreator.GetListsParameters()));
            var parameterFinder = new ParameterFinder(autoMocker.GetMock<IServerWrapper>().Object, new NearestNeighborsFinder());
            var resultParameters = parameterFinder.GetNearest(knownParameters, "Model1", 1).Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(expectedParameters.Count, resultParameters.Count);
            Assert.IsTrue(ResultsComparator.IsEquals(expectedParameters, new List<ParameterValue>(resultParameters)));
        }

        [Test]
        public void NoNearestParametersFoundForModel()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(new List<List<ParameterValue>>()));
            autoMocker.GetMock<INearestNeighborsFinder>().CallBase = true;
            var parameterFinder = new ParameterFinder(autoMocker.GetMock<IServerWrapper>().Object, new NearestNeighborsFinder());
            var resultParameters = parameterFinder.GetNearest(knownParameters, "ModelTest", 1).Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(0, resultParameters.Count);
        }

        [Test]
        public void NoNearestParametersFoundDueToBoolParameter()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", true),
                new StringParameterValue("Parameter8", "Parameter8Parameter8")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(ClassCreator.GetListsParameters()));
            autoMocker.GetMock<INearestNeighborsFinder>().CallBase = true;
            var parameterFinder = new ParameterFinder(autoMocker.GetMock<IServerWrapper>().Object, new NearestNeighborsFinder());
            var resultParameters = parameterFinder.GetNearest(knownParameters, "Model1", 1).Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(0, resultParameters.Count);
        }

        [Test]
        public void NoNearestParametersFoundDueToStringParameter()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 2),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 2.2),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "ParameterTest")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetModelParametersValues(It.IsAny<string>())).Returns(() => Task.FromResult(ClassCreator.GetListsParameters()));
            autoMocker.GetMock<INearestNeighborsFinder>().CallBase = true;
            var parameterFinder = new ParameterFinder(autoMocker.GetMock<IServerWrapper>().Object, new NearestNeighborsFinder());
            var resultParameters = parameterFinder.GetNearest(knownParameters, "Model1", 1).Result;
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(0, resultParameters.Count);
        }

        [Test]
        public void GetDefaultParameters()
        {
            var knownParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new IntParameterValue("Parameter2", 2),
                new DoubleParameterValue("Parameter4", 4.4),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var paremeterInfos = new List<ParameterInfo>()
            {
                new IntParameterInfo("Parameter1", "Parameter1", "", 1, 0, 0),
                new IntParameterInfo("Parameter2", "Parameter2", "", 2, 0, 0),
                new IntParameterInfo("Parameter3", "Parameter3", "", 3, 0, 0),
                new DoubleParameterInfo("Parameter4", "Parameter4", "", 4.4, 0, 0),
                new DoubleParameterInfo("Parameter5" ,"Parameter5", "", 5.5, 0, 0),
                new BoolParameterInfo("Parameter6", "Parameter6", "", true),
                new BoolParameterInfo("Parameter7", "Parameter7", "", false),
                new StringParameterInfo("Parameter8", "Parameter8", "", "Parameter8", new List<string>())
            };
            var expectedParameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new IntParameterValue("Parameter2", 2),
                new IntParameterValue("Parameter3", 3),
                new DoubleParameterValue("Parameter4", 4.4),
                new DoubleParameterValue("Parameter5", 5.5),
                new BoolParameterValue("Parameter6", true),
                new BoolParameterValue("Parameter7", false),
                new StringParameterValue("Parameter8", "Parameter8")
            };
            var autoMocker = new AutoMocker();
            var parameterFinder = autoMocker.CreateInstance<ParameterFinder>();
            var resultParameters = parameterFinder.GetDefault(knownParameters, paremeterInfos);
            Assert.IsNotNull(resultParameters);
            Assert.AreEqual(expectedParameters.Count, resultParameters.Count);
            Assert.IsTrue(ResultsComparator.IsEquals(expectedParameters, new List<ParameterValue>(resultParameters)));
        }
    }
}
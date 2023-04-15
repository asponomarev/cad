using DSS.Wrappers;
using DSS;
using Moq.AutoMock;
using UhlnocsServer.Models.Properties.Characteristics;

namespace Tests
{
    public class ModelSelectorTests
    {
        [Test]
        public void SuitableModelsWasFound()
        {
            var characteristics = new List<CharacteristicWithModel>()
            {
                new CharacteristicWithModel("Id1", "Model1"),
                new CharacteristicWithModel("Id2", null),
                new CharacteristicWithModel("Id3", "")
            };
            var expectedCharacteristics = new List<CharacteristicWithModel>()
            {
                new CharacteristicWithModel("Id1", "Model1"),
                new CharacteristicWithModel("Id2", "Model2"),
                new CharacteristicWithModel("Id3", "Model3")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetCharacteristicsWithModels()).Returns(() => ClassCreator.GetCharacteristics());
            var modelSelector = autoMocker.CreateInstance<ModelSelector>();
            var resultCharacteristics = modelSelector.GetSuitableModels(characteristics);
            Assert.IsNotNull(resultCharacteristics);
            Assert.AreEqual(expectedCharacteristics.Count, resultCharacteristics.Count);
            Assert.IsTrue(ResultsComparator.IsEquals(expectedCharacteristics, new List<CharacteristicWithModel>(resultCharacteristics)));
        }

        [Test]
        public void SuitableModelsWasNotFound()
        {
            var characteristics = new List<CharacteristicWithModel>()
            {
                new CharacteristicWithModel("IdNot1", "Model1"),
                new CharacteristicWithModel("IdNot2", null),
                new CharacteristicWithModel("IdNot3", "")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetCharacteristicsWithModels()).Returns(() => ClassCreator.GetCharacteristics());
            var modelSelector = autoMocker.CreateInstance<ModelSelector>();
            var resultCharacteristics = modelSelector.GetSuitableModels(characteristics);
            Assert.IsNotNull(resultCharacteristics);
            Assert.AreEqual(0, resultCharacteristics.Count);
        }

        [Test]
        public void DuplicateSuitableModelsWasFound()
        {
            var characteristics = new List<CharacteristicWithModel>()
            {
                new CharacteristicWithModel("Id1", "Model1"),
                new CharacteristicWithModel("Id2", null),
                new CharacteristicWithModel("Id5", "")
            };
            var expectedCharacteristics = new List<CharacteristicWithModel>()
            {
                new CharacteristicWithModel("Id1", "Model1"),
                new CharacteristicWithModel("Id2", "Model2"),
                new CharacteristicWithModel("Id5", "Model1")
            };
            var autoMocker = new AutoMocker();
            autoMocker.GetMock<IServerWrapper>().Setup(l => l.GetCharacteristicsWithModels()).Returns(() => ClassCreator.GetCharacteristics());
            var modelSelector = autoMocker.CreateInstance<ModelSelector>();
            var resultCharacteristics = modelSelector.GetSuitableModels(characteristics);
            Assert.IsNotNull(resultCharacteristics);
            Assert.AreEqual(expectedCharacteristics.Count, resultCharacteristics.Count);
            Assert.IsTrue(ResultsComparator.IsEquals(expectedCharacteristics, new List<CharacteristicWithModel>(resultCharacteristics)));
        }
    }
}

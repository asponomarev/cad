using DSS.Wrappers;
using UhlnocsServer.Models.Properties.Characteristics;

namespace DSS
{
    public class ModelSelector : IModelSelector
    {
        private readonly IServerWrapper _serverWrapper;

        public ModelSelector(IServerWrapper serverWrapper)
        {
            _serverWrapper = serverWrapper;
        }

        public IList<CharacteristicWithModel> GetSuitableModels(IList<CharacteristicWithModel> characteristics)
        {
            var suitableModelsId = new List<CharacteristicWithModel>();
            var characteristicsWithModels = _serverWrapper.GetCharacteristicsWithModels();
            foreach (var characteristic in characteristics)
            {
                if (characteristicsWithModels.ContainsKey(characteristic.Id))
                {
                    if (string.IsNullOrEmpty(characteristic.Model)
                        || string.IsNullOrWhiteSpace(characteristic.Model))
                    {
                        var possibleModelsIds = characteristicsWithModels[characteristic.Id].Models;
                        if (possibleModelsIds.Count > 0)
                        {
                            //TODO: use magic number witch describes model efficiency
                            var bestModelId = possibleModelsIds.First();
                            characteristic.Model = bestModelId;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    suitableModelsId.Add(characteristic);
                }
            }
            return suitableModelsId;
        }
    }
}

using UhlnocsServer.Models.Properties.Characteristics;

namespace DSS
{
    public interface IModelSelector
    {
        public IList<CharacteristicWithModel> GetSuitableModels(IList<CharacteristicWithModel> characteristics);
    }
}

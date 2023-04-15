using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace DSS
{
    public class NearestNeighborsFinder : INearestNeighborsFinder
    {
        public List<List<ParameterValue>> Find(List<List<ParameterValue>> parametersList, List<ParameterValue> knownParameters, int k)
        {
            {
                var distances = new List<double>();
                for (int i = 0; i < parametersList.Count; i++)
                {
                    double distance = 0;
                    foreach (var knownParameter in knownParameters)
                    {
                        if (knownParameter is IntParameterValue knownIntParameterValue && parametersList[i].FirstOrDefault(p => string.Equals(p.Id, knownParameter.Id)) is IntParameterValue intParameterValue)
                        {
                            // Считаем расстояние между элементами списков.
                            double diff = Convert.ToDouble(intParameterValue.Value) - Convert.ToDouble(knownIntParameterValue.Value);
                            distance += diff * diff;
                        }
                        else if (knownParameter is DoubleParameterValue knownDoubleParameterValue && parametersList[i].FirstOrDefault(p => string.Equals(p.Id, knownParameter.Id)) is DoubleParameterValue doubleParameterValue)
                        {
                            // Считаем расстояние между элементами списков.
                            double diff = doubleParameterValue.Value - knownDoubleParameterValue.Value;
                            distance += diff * diff;
                        }
                    }
                    distances.Add(distance);
                }

                // Сортируем списки по возрастанию расстояния.
                var sortedLists = parametersList
                    .Select((list, index) => new { List = list, Index = index })
                    .OrderBy(pair => distances[pair.Index])
                    .Select(pair => pair.List)
                    .ToList();

                // Выбираем первые k списков, которые будут являться ближайшими соседями.
                return sortedLists.Take(k).ToList();
            }
        }
    }
}

using UhlnocsServer.Optimizations;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Parameters.Infos;

namespace Tests
{
    public class AlgorithmsTests
    {
        [Test]
        public void TestConstantStep()
        {
            List<ParameterValue> parameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new DoubleParameterValue("Parameter2", 5.5),
                new StringParameterValue("Parameter3", "value1"),
                new BoolParameterValue("Parameter4", true)

            };
            int iterations = 5;
            double step = 2;
            ConstantStep constantStep = new ConstantStep("Parameter1", step, iterations);
            StringParameterInfo parameterInfo = new StringParameterInfo("Parameter3","","","", new List<string>(){"First","Second","Third"});
            ParameterValue variableParameter = ParameterValue.GetFromListById(parameters, "Parameter1");

            List<ParameterValue> calculationParameters = new();
            ParameterValue calculatedParameter;
            for (int i = 0; i < constantStep.Iterations; ++i)
            {
                if (i == 0)
                {
                    calculationParameters = constantStep.MakeCalculationParameters(parameters, "Parameter3", i, PropertyValueType.String, variableParameter, parameterInfo);
                    calculatedParameter = ParameterValue.GetFromListById(calculationParameters, "Parameter3");
                    string stringValue = ((StringParameterValue)calculatedParameter).Value;
                    Assert.That(stringValue, Is.EqualTo("First")); // проверка, что рассчитанный строковый параметр соответствует ожиданию

                    calculationParameters = constantStep.MakeCalculationParameters(parameters, "Parameter4", i, PropertyValueType.Bool, variableParameter, parameterInfo);
                    calculatedParameter = ParameterValue.GetFromListById(calculationParameters, "Parameter4");
                    bool boolValue = ((BoolParameterValue)calculatedParameter).Value;
                    Assert.That(boolValue, Is.EqualTo(true)); // проверка, что рассчитанный логический параметр соответствует ожиданию
                }
                calculationParameters = constantStep.MakeCalculationParameters(parameters, "Parameter1", i, PropertyValueType.Int, variableParameter, parameterInfo);

            }
            calculatedParameter = ParameterValue.GetFromListById(calculationParameters, "Parameter1");
            int intValue = ((IntParameterValue)calculatedParameter).Value;
            Assert.That(intValue, Is.EqualTo(9)); // проверка, что рассчитанный целочисленный параметр соответствует ожиданию


        }

        [Test]
        public void TestSmartConstantStep()
        {
            List<ParameterValue> parameters = new List<ParameterValue>() { new IntParameterValue("Parameter1", 1), };
            string variableParameterId = "Parameter1";
            double step = 1;
            int maxIterations = 1;
            SmartConstantStep smartConstantStep = new SmartConstantStep(variableParameterId, "Characteristic1", step, maxIterations);
            int iteration = 0;
            AlgorithmStatus Status = AlgorithmStatus.Calculating;
            do
            {
                _ = smartConstantStep.MakeCalculationParameters(parameters, variableParameterId, iteration);
                double throughputCharacteristicValue = 1;
                Status = smartConstantStep.CheckPointIsGood(throughputCharacteristicValue);
                ++iteration;
                if (iteration == smartConstantStep.MaxIterations)
                {
                    Status = AlgorithmStatus.ReachedMaxIteration;
                }
            }
            while (Status == AlgorithmStatus.Calculating);
            Assert.That(Status, Is.EqualTo(AlgorithmStatus.ReachedMaxIteration)); // проверка, что статус алгоритма поменялся
            Assert.That(iteration, Is.EqualTo(1)); // проверка, что цикл закончился на 1 итерации, так как maxIterations было задано 1
        }

        [Test]
        public void TestBinarySearch()
        {
            List<ParameterValue> parameters = new List<ParameterValue>() { new DoubleParameterValue("Parameter1", 1), };
            string variableParameterId = "Parameter1";
            int iterations = 3;
            double maxRate = 10;
            BinarySearch binarySearch = new BinarySearch(variableParameterId, "Characteristic1", iterations, maxRate);
            binarySearch.FirstValue = 1;
            double throughputCharacteristicValue;
            AlgorithmStatus Status;

            for (int i = 0; i < 3; ++i)
            {
                List<ParameterValue> calculationParameters = binarySearch.MakeCalculationParameters(parameters, variableParameterId, i);
                if (i == 0)
                {
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    double CalculatedValue = ((DoubleParameterValue)variableParameter).Value;
                    Assert.That(CalculatedValue, Is.EqualTo(1)); // проверка, что на 0 итерации возвращается левая граница
                    throughputCharacteristicValue = 0;
                    Status = binarySearch.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(Status, Is.EqualTo(AlgorithmStatus.FirstPointIsBad)); // проверка, что находится "первая точка плохая"
                }
                else if (i == 1)
                {
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    double CalculatedValue = ((DoubleParameterValue)variableParameter).Value;
                    Assert.That(CalculatedValue, Is.EqualTo(10)); // проверка, что на 1 итерации возвращается правая граница
                    throughputCharacteristicValue = 10;
                    Status = binarySearch.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(Status, Is.EqualTo(AlgorithmStatus.LastPointIsGood)); // проверка, что находится "последняя точка хорошая"
                }
                else
                {
                    throughputCharacteristicValue = 5.5;
                    _ = binarySearch.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(binarySearch.FirstValue, Is.EqualTo(5.5)); // проверка, что найденная throughput "хорошая", поменялась левая граница

                    throughputCharacteristicValue = 3;
                    _ = binarySearch.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(binarySearch.LastValue, Is.EqualTo(5.5)); // проверка, что найденная throughput "плохая", поменялась правая граница

                }
            }
        }

        [Test]
        public void TestSmartBinarySearch()
        {
            List<ParameterValue> parameters = new List<ParameterValue>() { new DoubleParameterValue("Parameter1", 1), };
            string variableParameterId = "Parameter1";
            double maxRate = 50;
            SmartBinarySearch smartBinarySearch = new SmartBinarySearch(variableParameterId, "Characteristic1", maxRate);
            smartBinarySearch.FirstValue = 1;
            double throughputCharacteristicValue;
            AlgorithmStatus Status = AlgorithmStatus.Calculating;
            int iteration = 0;
            do
            {
                List<ParameterValue> calculationParameters = smartBinarySearch.MakeCalculationParameters(parameters, variableParameterId, iteration);

                if (iteration == 2)
                {
                    Assert.That(smartBinarySearch.FirstChangedBorder, Is.EqualTo(null)); // проверка, что первые две итерации не повлияли на поля класса
                    throughputCharacteristicValue = 0;
                    Status = smartBinarySearch.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.Multiple(() =>
                    {
                        Assert.That(smartBinarySearch.FirstChangedBorder, Is.EqualTo("Right")); // проверка, что первой поменялась правая граница
                        Assert.That(smartBinarySearch.BothBordersChanged, Is.EqualTo(false)); // проверка, что еще не были изменены обе границы
                    });
                }

                if (iteration == 3)
                {
                    throughputCharacteristicValue = 13;
                    Status = smartBinarySearch.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartBinarySearch.BothBordersChanged, Is.EqualTo(true)); // проверка, что были изменены обе границы
                }

                if (iteration == 4)
                {
                    throughputCharacteristicValue = 0;
                    Status = smartBinarySearch.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartBinarySearch.ReachedSaturationPoint, Is.EqualTo(true)); // проверка, что точка насыщения достигнута
                }

                if (iteration == 5)
                {
                    throughputCharacteristicValue = 16;
                    Status = smartBinarySearch.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(Status, Is.EqualTo(AlgorithmStatus.FoundSaturationPoint)); // проверка, что статус алгоритма поменялся
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    double CalculatedValue = ((DoubleParameterValue)variableParameter).Value;
                    Assert.That(CalculatedValue, Is.EqualTo(16.3125d)); // проверка, что найдена искомая точка насыщения
                }

                ++iteration;
            }
            while (Status == AlgorithmStatus.Calculating && iteration < smartBinarySearch.MaxIterations);
            Assert.That(iteration, Is.EqualTo(6)); // проверка, что понадобилось 6 итераций
        }

        [Test]
        public void TestGoldenSection()
        {
            List<ParameterValue> parameters = new List<ParameterValue>() { new DoubleParameterValue("Parameter1", 1), };
            string variableParameterId = "Parameter1";
            int iterations = 5;
            double maxRate = 50;
            GoldenSection goldenSection = new GoldenSection(variableParameterId, "Characteristic1", iterations, maxRate);
            goldenSection.FirstValue = 1;
            double throughputCharacteristicValue;
            for (int i = 0; i < goldenSection.Iterations; ++i)
            {
                List<ParameterValue> calculationParameters = goldenSection.MakeCalculationParameters(parameters, variableParameterId, i);
                if (i == 2)
                {
                    throughputCharacteristicValue = 0;
                    Assert.That(Math.Round(goldenSection.X1), Is.EqualTo(20));// проверка, что точка X1 поменяла значение по формуле Фибоначчи
                    _ = goldenSection.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(goldenSection.NextPoint, Is.EqualTo("X1")); // проверка, что если точка "плохая", то снова будем искать X1
                }
                if (i == 3)
                {
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    throughputCharacteristicValue = ((DoubleParameterValue)variableParameter).Value;
                    Assert.That(Math.Round(goldenSection.LastValue), Is.EqualTo(20)); // проверка, что правая граница поменялась на значение точки X1
                    _ = goldenSection.MoveBorder(throughputCharacteristicValue, i);
                    Assert.That(goldenSection.NextPoint, Is.EqualTo("X2")); // проверка, что точка "хорошая" и далее будем менять X2
                }
                if (i == 4)
                {
                    Assert.That(Math.Round(goldenSection.X2), Is.EqualTo(13));// проверка, что точка X2 поменяла значение по формуле Фибоначчи
                }         
            }
        }

        [Test]
        public void TestSmartGoldenSection()
        {
            List<ParameterValue> parameters = new List<ParameterValue>() { new DoubleParameterValue("Parameter1", 1), };
            string variableParameterId = "Parameter1";
            double maxRate = 50;
            SmartGoldenSection smartGoldenSection = new SmartGoldenSection(variableParameterId, "Characteristic1", maxRate);
            smartGoldenSection.FirstValue = 1;
            double throughputCharacteristicValue;
            AlgorithmStatus Status = AlgorithmStatus.Calculating;
            int iteration = 0;
            do
            {
                List<ParameterValue> calculationParameters = smartGoldenSection.MakeCalculationParameters(parameters, variableParameterId, iteration);
                if (iteration == 2)
                {
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    throughputCharacteristicValue = ((DoubleParameterValue)variableParameter).Value;
                    Status = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartGoldenSection.LastFoundPoint, Is.EqualTo("X1")); // проверка, что была найдена точка X1
                }
                if (iteration == 3)
                {
                    throughputCharacteristicValue = 0;
                    Status = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartGoldenSection.LastFoundPoint, Is.EqualTo("X2"));// проверка, что была найдена точка X2
                    Assert.That(smartGoldenSection.InMiddleSegment, Is.EqualTo(true)); // проверка, что обе границы поменялись на X1 и X2
                }
                if (iteration == 4)
                {
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    throughputCharacteristicValue = ((DoubleParameterValue)variableParameter).Value;
                    Status = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartGoldenSection.NextPoint, Is.EqualTo("X2")); // проверка, что далее будем находить точку X2
                }
                if (iteration == 5)
                {
                    throughputCharacteristicValue = 0;
                    Status = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(smartGoldenSection.ReachedSaturationPoint, Is.EqualTo(true)); // проверка, что точка насыщения достигнута
                }
                if (iteration == 6)
                {
                    throughputCharacteristicValue = 0;
                    Status = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    Assert.That(Status, Is.EqualTo(AlgorithmStatus.FoundSaturationPoint)); // проверка, что статус алгоритма поменялся
                    ParameterValue variableParameter = ParameterValue.GetFromListById(calculationParameters, variableParameterId);
                    double CalculatedValue = ((DoubleParameterValue)variableParameter).Value;
                    Assert.That(Math.Round(CalculatedValue), Is.EqualTo(26)); // проверка, что найдена искомая точка насыщения
                }
                ++iteration;
            }
            while (Status == AlgorithmStatus.Calculating && iteration < smartGoldenSection.MaxIterations);
            Assert.That(iteration, Is.EqualTo(7)); // проверка, что понадобилось 7 итераций
        }
    }
}

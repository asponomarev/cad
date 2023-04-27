﻿using UhlnocsServer.Optimizations;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Parameters.Infos;
using UhlnocsServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UhlnocsServer.Calculations;
using UhlnocsServer.Models.Properties.Characteristics;
using Grpc.Core;
using Moq.AutoMock;
using Moq;
using DSS.Wrappers;
using DSS;
using ModelConfiguration = UhlnocsServer.Models.ModelConfiguration;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            string variableParameterId = "Parameter1";
            ConstantStep constantStep = new ConstantStep(variableParameterId, step, iterations);
            PropertyValueType valueType = PropertyValueType.Int;
            ParameterValue variableParameter = ParameterValue.GetFromListById(parameters, variableParameterId);
            List<ParameterValue> calculationParameters = new();
            for (int i = 0; i < constantStep.Iterations; ++i)
            {
                calculationParameters = constantStep.MakeCalculationParameters(parameters, variableParameterId, i, valueType, variableParameter);
            }
            int calculatedParameter = (calculationParameters[0] as IntParameterValue).Value;
            Assert.That(calculatedParameter, Is.EqualTo(9)); // проверка, что рассчитанный параметр соответствует ожиданию
        }

        [Test]
        public void TestSmartConstantStep()
        {
            List<ParameterValue> parameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new DoubleParameterValue("Parameter2", 1),
                new DoubleParameterValue("Parameter3", 5.5),
            };
            string variableParameterId = "Parameter1";

            double step = 1;
            int maxIterations = 1;
            SmartConstantStep smartConstantStep = new SmartConstantStep(variableParameterId, "Characteristic1", step, maxIterations);
            int iteration = 0;
            bool PointsStillGood;
            do
            {
                List<ParameterValue> calculationParameters = smartConstantStep.MakeCalculationParameters(parameters, variableParameterId, iteration);
                double throughputCharacteristicValue = 1;
                PointsStillGood = smartConstantStep.CheckPointIsGood(throughputCharacteristicValue);
                ++iteration;
            }
            while (iteration < smartConstantStep.MaxIterations && PointsStillGood);
            Assert.That(PointsStillGood, Is.EqualTo(true)); // проверка, что точки все еще "хорошие"
            Assert.That(iteration, Is.EqualTo(1)); // проверка, что цикл закончился на 1 итерации, так как maxIterations было задано 1
        }

        [Test]
        public void TestBinarySearch()
        {
            List<ParameterValue> parameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new DoubleParameterValue("Parameter2", 1),
                new DoubleParameterValue("Parameter3", 5.5),
            };
            string variableParameterId = "Parameter1";

            BinarySearch binarySearch = new BinarySearch(variableParameterId, "Characteristic1", 3, 10);
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
            List<ParameterValue> parameters = new List<ParameterValue>()
            {
                new IntParameterValue("Parameter1", 1),
                new DoubleParameterValue("Parameter2", 1),
                new DoubleParameterValue("Parameter3", 5.5),
            };
            string variableParameterId = "Parameter1";

            SmartBinarySearch smartBinarySearch = new SmartBinarySearch(variableParameterId, "Characteristic1", 10, 50);
            smartBinarySearch.FirstValue = 1;
            double throughputCharacteristicValue;
            AlgorithmStatus Status = AlgorithmStatus.Calculating;
            int iteration = 0;
            do
            {
                List<ParameterValue> calculationParameters = smartBinarySearch.MakeCalculationParameters(parameters, variableParameterId, iteration);

                if (iteration == 2)
                {
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
            Assert.That(iteration,Is.EqualTo(6)); // проверка, что понадобилось 6 итераций
        }      
    }
}

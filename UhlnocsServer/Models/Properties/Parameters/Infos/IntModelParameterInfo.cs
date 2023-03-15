﻿namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class IntModelParameterInfo : ModelParameterInfo
    {
        public int DefaultValue { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public IntModelParameterInfo(string id, string name, string description, int defaultValue,
                                     int minValue, int maxValue) : base(id, name, ModelPropertyValueType.Int, description)
        {
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}

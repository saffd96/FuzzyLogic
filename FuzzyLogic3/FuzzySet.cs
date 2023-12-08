using System;
using UnityEngine;

namespace FuzzyLogic3
{
    public class FuzzySet
    {
        private float _membershipValue;

        public delegate float MembershipFunction(float inputValue);

        private readonly MembershipFunction _membershipFunction;
        private readonly float _rangeMin;
        private readonly float _rangeMax;

        public float RangeMin => _rangeMin;
        public float RangeMax => _rangeMax;
        public float OutputValue { get; set; }


        public FuzzySet(MembershipFunction function, float rangeMin, float rangeMax)
        {
            _membershipFunction = function;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
        }

        public static MembershipFunction CreateGaussianFunction(float a, float fixedStandardDeviation = 1)
        {
            return x => Mathf.Exp(-Mathf.Pow((x - a) / fixedStandardDeviation, 2) / 2);
        }

        public static MembershipFunction CreateSigmoidFunction(float a, float slope = 1f)
        {
            return x => 1 / (1 + Mathf.Exp(-slope * (x - a)));
        }

        public static MembershipFunction CreateTriangularFunction(float a, float b, float c, float plateauWidth = 0.1f)
        {
            float plateauStart = Mathf.Max(b - plateauWidth / 2, a);
            float plateauEnd = Mathf.Min(b + plateauWidth / 2, c);

            return x =>
            {
                if (x <= a || x >= c)
                    return 0;
                if (x >= plateauStart && x <= plateauEnd)
                    return 1;
                if (x < plateauStart)
                    return (x - a) / (plateauStart - a);
                return (c - x) / (c - plateauEnd);
            };
        }

        public static MembershipFunction CreateTrapezoidalFunction(float a, float b, float c, float d,
            float plateauWidth = 0.1f)
        {
            return x =>
            {
                if (x <= a || x >= d)
                    return 0;

                if (x >= b && x <= c)
                    return 1;

                if (x > a && x < b)
                    return (x - a) / (b - a);

                if (x > c && x < d)
                    return (d - x) / (d - c);

                throw new Exception("Incorrect TrapezoidalFunction");
            };
        }

        public float GetMembershipValue(float inputValue)
        {
            _membershipValue = _membershipFunction(inputValue);
            return _membershipValue;
        }

        public void UpdateMembershipValue(float inputValue)
        {
            _membershipValue = _membershipFunction(inputValue);
        }
    }
}
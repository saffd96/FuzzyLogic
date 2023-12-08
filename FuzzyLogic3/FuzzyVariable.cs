using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic3
{
    public class FuzzyVariable
    {
        private readonly Dictionary<string, FuzzySet> _fuzzySets = new Dictionary<string, FuzzySet>();

        private float _inputValue;
        private float _outputValue;

        public void AddFuzzySet(string name, FuzzySet set)
        {
            _fuzzySets[name] = set;
        }

        public void SetInputValue(float value)
        {
            _inputValue = value;
            UpdateFuzzySetMemberships();
        }

        public float GetMembershipValue(string name)
        {
            if (_fuzzySets.ContainsKey(name))
            {
                return _fuzzySets[name].GetMembershipValue(_inputValue);
            }

            throw new KeyNotFoundException($"Fuzzy set '{name}' not found.");
        }

        public void SetOutputValue(string name, float value)
        {
            if (_fuzzySets.ContainsKey(name))
            {
                _fuzzySets[name].OutputValue = value;
                return;
            }

            throw new KeyNotFoundException($"Fuzzy set '{name}' not found.");
        }

        public float Defuzzify(int numSamples = 100)
        {
            float weightedSum = 0;
            float sumOfMemberships = 0;

            foreach (FuzzySet set in _fuzzySets.Values)
            {
                for (int i = 0; i <= numSamples; i++)
                {
                    float x = set.RangeMin + i * (set.RangeMax - set.RangeMin) / numSamples;
                    float membership = set.GetMembershipValue(x) * set.OutputValue;
                    weightedSum += x * membership;
                    sumOfMemberships += membership;
                }
            }

            if (Mathf.Abs(sumOfMemberships) > 1e-6)
            {
                _outputValue = weightedSum / sumOfMemberships;
            }
            return _outputValue;
        }

        public IEnumerable<string> FuzzySetNames
        {
            get { return _fuzzySets.Keys; }
        }

        private void UpdateFuzzySetMemberships()
        {
            foreach (FuzzySet set in _fuzzySets.Values)
            {
                set.UpdateMembershipValue(_inputValue);
            }
        }
    }
}
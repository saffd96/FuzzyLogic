using System;
using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic3
{
    public enum FuzzyOperator
    {
        And,
        Or,
        Not
    }

    public class FuzzyRule
    {
        private readonly string[] _inputVariableNames;
        private readonly string[] _inputSetNames;
        private readonly FuzzyOperator[] _operators;
        private readonly string _outputVariableName;
        private readonly string _outputSetName;

        public FuzzyRule(string[] inputVariableNames, string[] inputSetNames, FuzzyOperator[] operators, string outputVariableName, string outputSetName)
        {
            _inputVariableNames = inputVariableNames;
            _inputSetNames = inputSetNames;
            _operators = operators ?? Array.Empty<FuzzyOperator>();
            _outputVariableName = outputVariableName;
            _outputSetName = outputSetName;
        }

        public void Evaluate(Dictionary<string, FuzzyVariable> inputVariables, Dictionary<string, FuzzyVariable> outputVariables)
        {
            float result = EvaluateConditions(inputVariables);
            outputVariables[_outputVariableName].SetOutputValue(_outputSetName, result);
        }

        private float EvaluateConditions(Dictionary<string, FuzzyVariable> inputVariables)
        {
            List<float> memberships = new List<float>();

            for (int i = 0; i < _inputVariableNames.Length; i++)
            {
                float membership = inputVariables[_inputVariableNames[i]].GetMembershipValue(_inputSetNames[i]);
                memberships.Add(membership);
            }

            float result = memberships[0];

            for (int i = 1; i < memberships.Count; i++)
            {
                switch (_operators[i - 1])
                {
                    case FuzzyOperator.And:
                        result = Mathf.Min(result, memberships[i]);
                        break;
                    case FuzzyOperator.Or:
                        result = Mathf.Max(result, memberships[i]);
                        break;
                    case FuzzyOperator.Not:
                        result = 1 - result;
                        break;
                }
            }

            return result;
        }
    }
}
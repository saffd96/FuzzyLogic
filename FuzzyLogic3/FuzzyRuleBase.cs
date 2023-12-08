using System.Collections.Generic;

namespace FuzzyLogic3
{
    public class FuzzyRuleBase
    {
        private readonly List<FuzzyRule>             _rules = new List<FuzzyRule>();

        public void AddRule(FuzzyRule rule)
        {
            _rules.Add(rule);
        }

        public void Evaluate(Dictionary<string, FuzzyVariable> inputVariables, Dictionary<string, FuzzyVariable> outputVariables)
        {
            // Reset output values
            foreach (FuzzyVariable outputVariable in outputVariables.Values)
            {
                foreach (string setName in outputVariable.FuzzySetNames)
                {
                    outputVariable.SetOutputValue(setName, 0);
                }
            }

            // Evaluate rules
            foreach (FuzzyRule rule in _rules)
            {
                rule.Evaluate(inputVariables, outputVariables);
            }
        }
    }
}
using System.Collections.Generic;

namespace FuzzyLogic3
{
    public class FuzzyLogicController
    {
        private readonly FuzzyRuleBase _ruleBase = new FuzzyRuleBase();
        private readonly Dictionary<string, FuzzyVariable> _inputVariables = new Dictionary<string, FuzzyVariable>();
        private readonly Dictionary<string, FuzzyVariable> _outputVariables = new Dictionary<string, FuzzyVariable>();

        public void AddInputVariable(string name, FuzzyVariable variable)
        {
            _inputVariables[name] = variable;
        }

        public void AddOutputVariable(string name, FuzzyVariable variable)
        {
            _outputVariables[name] = variable;
        }

        public void AddRule(FuzzyRule rule)
        {
            _ruleBase.AddRule(rule);
        }

        public void Evaluate()
        {
            _ruleBase.Evaluate(_inputVariables, _outputVariables);
        }

        public float GetOutputValue(string name)
        {
            if (_outputVariables.ContainsKey(name))
            {
                return _outputVariables[name].Defuzzify();
            }

            throw new KeyNotFoundException($"Output variable '{name}' not found.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;
using System.Text.RegularExpressions;

namespace QueryExtract
{
    class IfParameter
    {
        private HashSet<string> _allIfConditions = new HashSet<string>();
        private Stack<string> _currentIfConditions = new Stack<string>();
        private HashSet<string> _validIfConditions = new HashSet<string>();

        private void RegistIfParameter(string condition, fmQueryExtract form)
        {
            _allIfConditions.Add(condition);
            _currentIfConditions.Push(condition);
        }

        private void RemoveIfParameter()
        {
            _currentIfConditions.Pop();
        }


        public string IfProcess(string inputAfterComment, fmQueryExtract form)
        {
            List<IfConditionAndVbLine> x = new List<IfConditionAndVbLine>();
            foreach (var line in inputAfterComment.Split(Environment.NewLine.ToArray()))
            {
                if (line != "")
                {
                    Regex ifRegex = new System.Text.RegularExpressions.Regex("^If");
                    Regex endIfRegex = new System.Text.RegularExpressions.Regex("^END If");

                    if (ifRegex.IsMatch(line))
                    {
                        var ifStatementParser
                            = from _a in Parse.String("If").Token()
                              from condition in Parse.LetterOrDigit.AtLeastOnce().Token()
                              from _b in Parse.String("Then").Token()
                              select new string(condition.ToArray());

                        var ifParameter = ifStatementParser.Parse(line);
                        RegistIfParameter(ifParameter, form);
                    }
                    else if (endIfRegex.IsMatch(line))
                    {
                        RemoveIfParameter();
                    }
                    else
                    {
                        x.Add(new IfConditionAndVbLine(new List<string>(_currentIfConditions.ToArray()), line));
                    }
                }
            }

            form.RegistIfParameter(_allIfConditions);

            var retval = "";

            foreach (var ifAndVbline in x)
            {
                var isAdd = true;
                foreach (var ifCondition in ifAndVbline.IfConditions)
                {
                    if (!_validIfConditions.Contains(ifCondition))
                    {
                        isAdd = false;
                    }
                }
                if (isAdd)
                {
                    retval += ifAndVbline.QueryString + Environment.NewLine;
                }
            }

            return retval;
        }

        public void AddIfParameters(string ifCodition, string trueOrFalse)
        {
            if (trueOrFalse == "true")
            {
                _validIfConditions.Add(ifCodition);
            }
        }

        public void ClearValidIfConditions()
        {
            _validIfConditions.Clear();
        }

        public class IfConditionAndVbLine
        {
            List<string> _ifConditions = new List<string>();
            string _vbLine = "";

            public IfConditionAndVbLine(List<string> ifConditions, string vbLine)
            {
                _ifConditions = ifConditions;
                _vbLine = vbLine;
            }

            public List<string> IfConditions
            {
                get { return this._ifConditions; }
            }

            public string QueryString
            {
                get { return this._vbLine; }
            }
        }


    }
}

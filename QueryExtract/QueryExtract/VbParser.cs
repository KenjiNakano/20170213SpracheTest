using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;
using System.Text.RegularExpressions;

namespace QueryExtract
{
    class VbParser
    {
        public static Parser<string> _strParser
            = from str in Parse.LetterOrDigit.Or(Parse.Chars(new char[] { '(', ')', '*', '.', '_', '=', '>', '<', '\'', '@', '\'', '-' })).AtLeastOnce().Token()
              select new string(str.ToArray());

        public static Parser<string> _dimStringBuilderParser
            = from dim in Parse.String("Dim").Text().Token()
              from v in _strParser
              from rest in Parse.String("As New StringBuilder").Text().Token()
              select v;

        private IfParameter _ifParameter = new IfParameter(); 

        private QueryStringVariables _queryStringVarialbes = new QueryStringVariables();
        private Dictionary<string, string> _sqlParams = new Dictionary<string, string>();

        public string ExecParse(string input, fmQueryExtract form)
        {
            ClearQueryStringVarialbes();
            var inputAfterTrim = GetVbLines(input); //空白の処理
            var inputAfterComment = CommentProcess(inputAfterTrim); //コメントの除去
            var vbLinesAfterIf = IfProcess(inputAfterComment, form);
            var vbLinesAfterParams = ParamsProcess(vbLinesAfterIf, form);
            var vbLinesAfterAppendLine = AppendLineProcess(vbLinesAfterParams);
            var vbLinesAfterReplaceParams = ReplaceParamsProcess(vbLinesAfterAppendLine);

            var vbLinesFinal = vbLinesAfterReplaceParams;
            return vbLinesFinal;
        }

        public void ClearQueryStringVarialbes()
        {
            _queryStringVarialbes = new QueryStringVariables();
        }

        public static string GetVbLines(string input)
        {
            //行前後の空白を削除
            var inputAfterTrim = "";
            foreach (var line in input.Split(Environment.NewLine.ToArray()))
            {
                if (line != "")
                {
                    inputAfterTrim += line.Trim() + Environment.NewLine;
                }
            }
            return inputAfterTrim;
        }

        public static string CommentProcess(string inputAfterTrim)
        {
            var inputAfterCommentProcess = "";
            foreach (var line in inputAfterTrim.Split(Environment.NewLine.ToArray()))
            {
                if (line != "")
                {
                    var l = "";
                    var inLineComment = false;
                    var inDoubleQuote = false;
                    foreach (var c in line)
                    {
                        if (c == '"')
                        {
                            inDoubleQuote = !inDoubleQuote;
                        }
                        if (!inDoubleQuote && c == '\'')
                        {
                            inLineComment = true;
                        }

                        if (!inLineComment)
                        {
                            l += c;
                        }
                    }
                    inputAfterCommentProcess += l + Environment.NewLine;
                }
            }

            return inputAfterCommentProcess;
        }

        public string IfProcess(string inputAfterComment, fmQueryExtract form)
        {
            return _ifParameter.IfProcess(inputAfterComment, form);
        }

        public void AddIfParameters(string ifCodition, string trueOrFalse)
        {
            _ifParameter.AddIfParameters(ifCodition, trueOrFalse);
        }

        public void ClearValidIfConditions()
        {
            _ifParameter.ClearValidIfConditions();
        }

        public string AppendLineProcess(string inputAfterComment)
        {
            foreach (var line in inputAfterComment.Split(Environment.NewLine.ToArray()))
            {
                if (line != "")
                {
                    Regex dimRegex = new System.Text.RegularExpressions.Regex("^Dim");
                    Regex stringBuilderRegex = new System.Text.RegularExpressions.Regex("StringBuilder$");
                    Regex sqlParameterRegex = new System.Text.RegularExpressions.Regex("SqlParameter$");
                    Regex apppendLineRegex = new System.Text.RegularExpressions.Regex("^(.+)AppendLine");

                    if (dimRegex.IsMatch(line) && stringBuilderRegex.IsMatch(line))
                    {
                        var result = _dimStringBuilderParser.Parse(line);
                        _queryStringVarialbes.Add(result);
                    }
                    else if (apppendLineRegex.IsMatch(line))
                    {
                        var p = CreateAppendLineParser();
                        var appLine = p.Parse(line);
                        _queryStringVarialbes.Add(appLine);
                    }
                }
            }
            return _queryStringVarialbes.ReturnQueryString();
        }

        public Parser<QueryVariableAndString> CreateAppendLineParser()
        {
            Parser<QueryVariableAndString> appendLineParser
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(\"").Text()
                      from queryString2 in _strParser.Many()
                      from _b in Parse.String("\")").Text().Token()
                      select new QueryVariableAndString(queryVar, string.Join(" ", queryString2.ToArray()));

            Parser<QueryVariableAndString> appendLineParser2
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(").Text()
                      from queryString in CreateFormatFunctionParser()
                      from _b in Parse.String(")").Text().Token()
                      select new QueryVariableAndString(queryVar, queryString);

            return appendLineParser.Or(appendLineParser2);
        }


        public string MyFormat(IEnumerable<string> first, string replaced, IEnumerable<string> rest, IEnumerable<char> arg)
        {
            List<string> result = new List<string>();
            result.AddRange(first);
            result.Add(new string(arg.ToArray()));
            result.AddRange(rest);

            return string.Join("", result);
        }

        public Parser<string> CreateFormatFunctionParser()
        {
            //String.Format("                (SWH.KjWarehouseFlag = '{0}')", FLAG_ON)
            var formatFunctionParser
                = from _a in Parse.String("String.Format(").Text().Token()
                  from _b in Parse.Char('"').Token()
                  from first in _strParser.AtLeastOnce()
                  from replaced in Parse.String("{0}").Text()
                  from rest in _strParser.AtLeastOnce()
                  from _d in Parse.Char('"').Token()
                  from _e in Parse.Char(',').Token()
                  from arg in Parse.CharExcept(')').AtLeastOnce().Token()
                  from _f in Parse.Char(')').Token()
                  select MyFormat(first, replaced, rest, arg);

            return formatFunctionParser;
        }

        public void AddSqlParams(string sqlParam, string value)
        {
            if (value != null)
            {
                _sqlParams.Add(sqlParam, value);
            }
        }

        public void ClearSqlParams()
        {
            _sqlParams.Clear();
        }

        public string ParamsProcess(string input, fmQueryExtract form)
        {
            //Dim pramItemID As New SqlParameter("@ItemID", SqlDbType.VarChar, 30)
            Parser<string> dimSqlParameterParser
                = from dim in Parse.String("Dim").Text().Token()
                  from _a in Parse.LetterOrDigit.Many().Token()
                  from _b in Parse.String("As New SqlParameter(\"").Text().Token()
                  from at in Parse.Char('@')
                  from param in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                  from _c in Parse.Char('"')
                  from _d in Parse.AnyChar.Many()
                  select at + param;

            foreach (var line in input.Split(Environment.NewLine.ToArray()))
            {
                if (line != "")
                {
                    Regex dimRegex = new System.Text.RegularExpressions.Regex("^Dim");
                    Regex sqlParameterRegex = new System.Text.RegularExpressions.Regex("SqlParameter");

                    if (dimRegex.IsMatch(line) && sqlParameterRegex.IsMatch(line))
                    {
                        var result = dimSqlParameterParser.Parse(line);
                        form.RegistSqlParam(result);
                    }
                }
            }

            return input;
        }

        public string ReplaceParamsProcess(string input)
        {
            var retval = input;
            foreach (var sqlParam in _sqlParams)
            {
                retval = retval.Replace(sqlParam.Key, sqlParam.Value);
            }

            return retval;
        }

        class QueryStringVariables
        {
            private Dictionary<string, List<string>> _queryStringVarialbes = new Dictionary<string, List<string>>();

            public void Add(string varName)
            {
                if (!_queryStringVarialbes.ContainsKey(varName))
                {
                    _queryStringVarialbes.Add(varName, new List<string>());
                }
            }

            public void Add(QueryVariableAndString appline)
            {
                List<string> lst;
                _queryStringVarialbes.TryGetValue(appline.QueryVar, out lst);

                lst.Add(appline.QueryString);
            }

            public string ReturnQueryString()
            {
                string retVal = "";
                foreach (var val in _queryStringVarialbes.Values) //StringBuilderの変数ごとのループ
                {
                    foreach (var v in val)
                    {
                        retVal += v + Environment.NewLine;
                    }
                }
                return retVal;
            }
        }

        public class QueryVariableAndString
        {
            string _queryVar = "";
            string _queryString = "";

            public QueryVariableAndString(string queryVar, string queryString)
            {
                _queryVar = queryVar;
                _queryString = queryString;
            }

            public string QueryVar
            {
                get { return this._queryVar; }
            }

            public string QueryString
            {
                get { return this._queryString; }
            }
        }



    }
}

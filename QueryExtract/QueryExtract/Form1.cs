using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sprache;
using Irony;
using System.Text.RegularExpressions;

namespace QueryExtract
{
    public partial class fmQueryExtract : Form
    {
        public fmQueryExtract()
        {
            InitializeComponent();
        }

        private void tbInput_TextChanged(object sender, EventArgs e)
        {
            tbOutput.Text = ExtractQuery(tbInput.Text);
        }

        private void tbOutput_TextChanged(object sender, EventArgs e)
        {

        }

        VbParser _vbParser = new VbParser();

        private string ExtractQuery(string input)
        {
            _vbParser.ClearQueryStringVarialbes();
            var inputAfterTrim = VbParser.GetVbLines(input); //空白の処理
            var inputAfterComment = VbParser.CommentProcess(inputAfterTrim); //コメントの除去
            var vbLinesAfterIf = _vbParser.IfProcess(inputAfterComment, this);
            var vbLinesAfterParams = _vbParser.ParamsProcess(vbLinesAfterIf, this);
            var vbLinesAfterAppendLine = _vbParser.AppendLineProcess(vbLinesAfterParams);
            var vbLinesAfterReplaceParams = _vbParser.ReplaceParamsProcess(vbLinesAfterAppendLine);

            var vbLinesFinal = vbLinesAfterReplaceParams;
            return vbLinesFinal;
        }


        public void RegistIfParameter(HashSet<String> lst)
        {
            List<string> ifParametersAlreadyRegistered = new List<string>();
            foreach (DataGridViewRow r in this.dvIfCondition.Rows)
            {
                ifParametersAlreadyRegistered.Add((string)r.Cells[0].Value);
            }

            foreach (var l in lst)
            {
                if (!ifParametersAlreadyRegistered.Contains(l))
                {
                    this.dvIfCondition.Rows.Add(l);
                }
            }
        }

        public void RegistSqlParam(string sqlParam)
        {
            List<string> sqlParametersAlreadyRegistered = new List<string>();
            foreach (DataGridViewRow r in this.dvSqlParams.Rows)
            {
                sqlParametersAlreadyRegistered.Add((string)r.Cells[0].Value);
            }

            
            if (!sqlParametersAlreadyRegistered.Contains(sqlParam))
            {
                this.dvSqlParams.Rows.Add(sqlParam);
            }
        }

        private void dvIfParameters_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                _vbParser.ClearValidIfConditions();
                foreach (DataGridViewRow r in dvIfCondition.Rows)
                {
                    _vbParser.AddIfParameters((string)r.Cells[0].Value, (string)r.Cells[1].Value);
                }
                tbOutput.Text = ExtractQuery(tbInput.Text);
            }
        }

        private void dvSqlParams_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                _vbParser.ClearSqlParams();
                foreach (DataGridViewRow r in dvSqlParams.Rows)
                {
                    _vbParser.AddSqlParams((string)r.Cells[0].Value, (string)r.Cells[1].Value);
                }
                tbOutput.Text = ExtractQuery(tbInput.Text);
            }

        }
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

        public void Add(QueryStringAndVariable appline)
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

    public class QueryStringAndVariable
    {
        string _queryVar = "";
        string _queryString = "";

        public QueryStringAndVariable(string queryVar, string queryString)
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


    class VbParser
    {
        public static Parser<string> _strParser
            = from str in Parse.LetterOrDigit.Or(Parse.Chars(new char[] { '(', ')', '*', '.', '_', '=', '>', '<', '\'', '@', '\'' })).AtLeastOnce().Token()
              select new string(str.ToArray());

        public static Parser<string> _dimStringBuilderParser
            = from dim in Parse.String("Dim").Text().Token()
              from v in _strParser
              from rest in Parse.String("As New StringBuilder").Text().Token()
              select v;


        private QueryStringVariables _queryStringVarialbes = new QueryStringVariables();
        private HashSet<string> _allIfConditions = new HashSet<string>();
        private Stack<string> _currentIfConditions = new Stack<string>();
        private HashSet<string> _validIfConditions = new HashSet<string>();
        private Dictionary<string, string> _sqlParams = new Dictionary<string, string>();

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
            //行前後の空白を削除
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

        public Parser<QueryStringAndVariable> CreateAppendLineParser()
        {
            Parser<QueryStringAndVariable> appendLineParser
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(\"").Text()
                      from queryString2 in _strParser.Many()
                      from _b in Parse.String("\")").Text().Token()
                      select new QueryStringAndVariable(queryVar, string.Join(" ", queryString2.ToArray()));

            Parser<QueryStringAndVariable> appendLineParser2
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(").Text()
                      from queryString in CreateFormatFunctionParser()
                      from _b in Parse.String(")").Text().Token()
                      select new QueryStringAndVariable(queryVar, queryString);

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

        public void AddIfParameters(string ifCodition, string trueOrFalse)
        {
            if (trueOrFalse == "true")
            {
                _validIfConditions.Add(ifCodition);
            }
        }

        public void AddSqlParams(string sqlParam, string value)
        {
            if (value != null)
            {
                _sqlParams.Add(sqlParam, value);
            }
        }

        public void ClearValidIfConditions()
        {
            _validIfConditions.Clear();
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
                  from x in Parse.LetterOrDigit.Many().Token()
                  from rest in Parse.String("As New SqlParameter(\"").Text().Token()
                  from at in Parse.Char('@')
                  from param in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                  from rest2 in Parse.Char('"')
                  from rest3 in Parse.AnyChar.Many()
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

    }
}
    /*
    Dim query As New StringBuilder
    query.AppendLine("SELECT")
    query.AppendLine("  CASE WHEN EXISTS (")
    query.AppendLine("    SEELCT *")
    query.AppendLine("    FROM dbo.PUR_PurchaseOrderHdrTable AS PURH")
    query.AppendLine("    INNER JOIN dbo.SYS_WarehouseTable AS SWH")
    query.AppendLine("      ON PURH.WarehouseID = SWH.WarehouseID")
    query.AppendLine("    INNER JOIN dbo.PUR_PurchaseOrderBdyTable AS PURB")
    query.AppendLine("      ON PURH.PurchaseOrderNo = PURB.PurchaseOrderNo")
    query.AppendLine("    WHERE")
    query.AppendLine(String.Format("                (SWH.KjWarehouseFlag = '{0}')", FLAG_ON))
    query.AppendLine("    AND (PURB.ItemID = @ItemID)")
    query.AppendLine(String.Format("              AND (PURH.PurchaseOrderTypeID = {0})", DEF_PURCHASE_TYPE_STOCK_TRANSFER_REQUEST_ENTRY))

    If isNotCompletedOnly Then
        query.AppendLine(String.Format("                AND (PURH.PurchaseOrderStatusID < {0})", DEF_PURCHASE_STATES_APPRD))
    END If

    query.AppendLine("  )  ")
    query.AppendLine("  THEN 1")
    query.AppendLine("  ELSE 0")
    query.AppendLine("  END AS Result")

    Dim pramItemID As New SqlParameter("@ItemID", SqlDbType.VarChar, 30)
    pramItemID.Valule = strItemID

    Dim sqlCom As New SqlCommand
    sqlCom.CommandText = query.ToString()
    sqlCom.Parameters.Add(pramItemID)

    Return CINT(Me.ExecSQLScalar(sqlCom))

    */

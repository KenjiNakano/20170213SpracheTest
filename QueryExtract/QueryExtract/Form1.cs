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
            _vbParser.Clear();
            foreach (var line in VbParser.GetVbLines(input))
            {
                _vbParser.ParseVbLine(line, this);
            }
            return _vbParser.ReturnQueryString();
        }


        public void RegistIfParameter(HashSet<String> lst)
        {
            List<string> ifParametersAlreadyRegistered = new List<string>();
            foreach (DataGridViewRow r in this.dataGridView1.Rows)
            {
                ifParametersAlreadyRegistered.Add((string)r.Cells[0].Value);
            }

            foreach (var l in lst)
            {
                if (!ifParametersAlreadyRegistered.Contains(l))
                {
                    this.dataGridView1.Rows.Add(l);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                _vbParser.ClearValidIfConditions();
                foreach (DataGridViewRow r in dataGridView1.Rows)
                {
                    _vbParser.AddIfParameters((string)r.Cells[0].Value, (string)r.Cells[1].Value);
                }
                tbOutput.Text = ExtractQuery(tbInput.Text);
            }
        }
    }

    class QueryString
    {
        string _query = "";
        bool _inIfBlock = false;
        string _ifCondition = "";

        public QueryString(string query, bool inIfBlock, string ifCondition)
        {
            _query = query;
            _inIfBlock = inIfBlock;
            _ifCondition = ifCondition;
        }

        public string Query()
        {
            return _query;
        }

        public string IfCondition()
        {
            return _ifCondition;
        }

        public bool InIfBlock()
        {
            return _inIfBlock;
        }
    }

    class QueryStringVariables
    {
        private Dictionary<string, List<QueryString>> _queryStringVarialbes = new Dictionary<string, List<QueryString>>();

        public void Add(string varName)
        {
            if (!_queryStringVarialbes.ContainsKey(varName))
            {
                _queryStringVarialbes.Add(varName, new List<QueryString>());
            }
        }

        public void Add(QueryVarialbeAndString appline)
        {
            List<QueryString> lst;
            _queryStringVarialbes.TryGetValue(appline.QueryVar(), out lst);

            lst.Add(new QueryString(appline.QueryString(), false, ""));
        }

        public void Add(QueryVarialbeAndString appline, bool inIfBlock, string ifCondition)
        {
            List<QueryString> lst;
            _queryStringVarialbes.TryGetValue(appline.QueryVar(), out lst);

            lst.Add(new QueryString(appline.QueryString(), inIfBlock, ifCondition));
        }

        public string ReturnQueryString(HashSet<string> validIfConditions)
        {
            string retVal = "";
            bool startIfBlock = false;
            foreach (var val in _queryStringVarialbes.Values) //StringBuilderの変数ごとのループ
            {
                foreach (var v in val)
                {
                    if (v.InIfBlock())
                    {
                        if (startIfBlock == false)
                        {
                            retVal += "--" + v.IfCondition() + "\r\n--Start--\r\n";
                        }
                        startIfBlock = true;
                        if (validIfConditions.Contains(v.IfCondition()))
                        {
                            retVal += v.Query() + "\r\n";
                        }
                    } else
                    {
                        if (startIfBlock == true)
                        {
                            retVal += "--End--\r\n";
                        }
                        startIfBlock = false;
                        retVal += v.Query() + "\r\n";
                    }
                }
            }
            return retVal;
        }
    }

    public class QueryVarialbeAndString
    {
        string _queryVar = "";
        string _queryString = "";

        public QueryVarialbeAndString(string queryVar, string queryString)
        {
            _queryVar = queryVar;
            _queryString = queryString;
        }

        public string QueryVar()
        {
            return _queryVar;
        }

        public string QueryString()
        {
            return _queryString;
        }
    }

    class VbParser
    {
        public static Parser<string> _strParser
            = from str in Parse.LetterOrDigit.Or(Parse.Chars(new char[] { '(', ')', '*', '.', '_', '=', '>', '<', '\'', '@' })).AtLeastOnce().Token()
              select new string(str.ToArray());

        public static Parser<string> _dimParser
            = from dim in Parse.String("Dim").Text().Token()
              from v in _strParser
              from rest in Parse.String("As New StringBuilder").Text().Token()
              select v;


        private QueryStringVariables _queryStringVarialbes = new QueryStringVariables();
        private bool _isInIfBlock = false;
        private HashSet<string> _allIfConditions = new HashSet<string>();
        private HashSet<string> _validIfConditions = new HashSet<string>();

        public static List<string> GetVbLines(string input)
        {
            List<string> vbLines = new List<string>();
            foreach (var line in input.Split("\r\n".ToArray()))
            {
                if (line != "")
                {
                    vbLines.Add(line);
                }
            }
            return vbLines;
        }

        public void Clear()
        {
            _queryStringVarialbes = new QueryStringVariables();
        }

        public string ReturnQueryString()
        {
            return _queryStringVarialbes.ReturnQueryString(_validIfConditions);
        }

        private void RegistIfParameter(string condition, fmQueryExtract form)
        {
            _allIfConditions.Add(condition);
            form.RegistIfParameter(_allIfConditions);
        }

        public void ParseVbLine(string vbline, fmQueryExtract form)
        {
            if (vbline.Contains("If") && !vbline.Contains("END If"))
            {
                _isInIfBlock = true;
                var ifStatementParser
                    = from _a in Parse.String("If").Token()
                      from condition in Parse.LetterOrDigit.AtLeastOnce().Token()
                      from _b in Parse.String("Then").Token()
                      select new string(condition.ToArray());

                var ifParameter = ifStatementParser.Parse(vbline);
                RegistIfParameter(ifParameter, form);
            }

            if (vbline.Contains("END If"))
            {
                _isInIfBlock = false;
            }

            if (vbline.Contains("Dim"))
            {
                var result = _dimParser.Parse(vbline);
                _queryStringVarialbes.Add(result);
            } else if (vbline.Contains("AppendLine"))
            {
                var p = CreateAppendLineParser();
                var appLine = p.Parse(vbline);
                if (_isInIfBlock)
                {
                    _queryStringVarialbes.Add(appLine, _isInIfBlock, _allIfConditions.Last());
                } else
                {
                    _queryStringVarialbes.Add(appLine);
                }
            }
        }

        public Parser<QueryVarialbeAndString> CreateAppendLineParser()
        {
            Parser<QueryVarialbeAndString> appendLineParser
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(\"").Text()
                      from queryString2 in _strParser.Many()
                      from _b in Parse.String("\")").Text().Token()
                      select new QueryVarialbeAndString(queryVar, string.Join(" ", queryString2.ToArray()));

            Parser<QueryVarialbeAndString> appendLineParser2
                    = from queryVar in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                      from _a in Parse.String(".AppendLine(").Text()
                      from queryString in CreateFormatFunctionParser()
                      from _b in Parse.String(")").Text().Token()
                      select new QueryVarialbeAndString(queryVar, queryString);

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

        public void ClearValidIfConditions()
        {
            _validIfConditions.Clear();
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

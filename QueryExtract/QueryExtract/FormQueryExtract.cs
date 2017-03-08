using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace QueryExtract
{
    public partial class fmQueryExtract : Form
    {
        VbParser _vbParser = new VbParser();

        public fmQueryExtract()
        {
            InitializeComponent();
        }

        private string ExtractQuery(string input)
        {
            return _vbParser.ExecParse(input, this);
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

        private void tbInput_TextChanged(object sender, EventArgs e)
        {
            tbOutput.Text = ExtractQuery(tbInput.Text);
        }

        private void tbOutput_TextChanged(object sender, EventArgs e)
        {

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

        private void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                tbInput.SelectAll();
        }

        private void tbOutput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                tbOutput.SelectAll();
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

namespace QueryExtract
{
    partial class fmQueryExtract
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.tbInput = new System.Windows.Forms.TextBox();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.dvIfCondition = new System.Windows.Forms.DataGridView();
            this.IfCondition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TrueOrFalse = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dvSqlParams = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dvIfCondition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dvSqlParams)).BeginInit();
            this.SuspendLayout();
            // 
            // tbInput
            // 
            this.tbInput.Location = new System.Drawing.Point(12, 12);
            this.tbInput.Multiline = true;
            this.tbInput.Name = "tbInput";
            this.tbInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInput.Size = new System.Drawing.Size(418, 485);
            this.tbInput.TabIndex = 0;
            this.tbInput.TextChanged += new System.EventHandler(this.tbInput_TextChanged);
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(447, 12);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Size = new System.Drawing.Size(382, 485);
            this.tbOutput.TabIndex = 1;
            this.tbOutput.TextChanged += new System.EventHandler(this.tbOutput_TextChanged);
            // 
            // dvIfCondition
            // 
            this.dvIfCondition.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvIfCondition.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IfCondition,
            this.TrueOrFalse});
            this.dvIfCondition.Location = new System.Drawing.Point(835, 12);
            this.dvIfCondition.Name = "dvIfCondition";
            this.dvIfCondition.RowTemplate.Height = 21;
            this.dvIfCondition.Size = new System.Drawing.Size(253, 150);
            this.dvIfCondition.TabIndex = 2;
            this.dvIfCondition.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dvIfParameters_CellEndEdit);
            this.dvIfCondition.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dvIfParameters_CellEndEdit);
            // 
            // IfCondition
            // 
            this.IfCondition.HeaderText = "IfCondition";
            this.IfCondition.Name = "IfCondition";
            // 
            // TrueOrFalse
            // 
            this.TrueOrFalse.HeaderText = "TrueOrFalse";
            this.TrueOrFalse.Name = "TrueOrFalse";
            // 
            // dvSqlParams
            // 
            this.dvSqlParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvSqlParams.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.dvSqlParams.Location = new System.Drawing.Point(835, 168);
            this.dvSqlParams.Name = "dvSqlParams";
            this.dvSqlParams.RowTemplate.Height = 21;
            this.dvSqlParams.Size = new System.Drawing.Size(253, 150);
            this.dvSqlParams.TabIndex = 3;
            this.dvSqlParams.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dvSqlParams_CellEndEdit);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "SqlParameter";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Value";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // fmQueryExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 509);
            this.Controls.Add(this.dvSqlParams);
            this.Controls.Add(this.dvIfCondition);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.tbInput);
            this.Name = "fmQueryExtract";
            this.Text = "QueryExtract";
            ((System.ComponentModel.ISupportInitialize)(this.dvIfCondition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dvSqlParams)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.DataGridView dvIfCondition;
        private System.Windows.Forms.DataGridView dvSqlParams;
        private System.Windows.Forms.DataGridViewTextBoxColumn IfCondition;
        private System.Windows.Forms.DataGridViewTextBoxColumn TrueOrFalse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    }
}


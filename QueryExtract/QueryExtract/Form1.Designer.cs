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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.IfConditionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TrueOrFalse = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IfConditionName,
            this.TrueOrFalse});
            this.dataGridView1.Location = new System.Drawing.Point(835, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(253, 150);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // IfConditionName
            // 
            this.IfConditionName.HeaderText = "IfConditionNameColumn";
            this.IfConditionName.Name = "IfConditionName";
            // 
            // TrueOrFalse
            // 
            this.TrueOrFalse.HeaderText = "TrueOrFalse";
            this.TrueOrFalse.Name = "TrueOrFalse";
            // 
            // fmQueryExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 509);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.tbInput);
            this.Name = "fmQueryExtract";
            this.Text = "QueryExtract";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn IfConditionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TrueOrFalse;
    }
}


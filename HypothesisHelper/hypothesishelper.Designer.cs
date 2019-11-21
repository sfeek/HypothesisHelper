namespace HypothesisHelper
{
    partial class Hypothesishelper
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Hypothesishelper));
            this.gbData = new System.Windows.Forms.GroupBox();
            this.lblBData = new System.Windows.Forms.Label();
            this.lblAData = new System.Windows.Forms.Label();
            this.txtBData = new System.Windows.Forms.TextBox();
            this.txtAData = new System.Windows.Forms.TextBox();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.grpResults = new System.Windows.Forms.GroupBox();
            this.rtbResults = new System.Windows.Forms.RichTextBox();
            this.txtConfLevel = new System.Windows.Forms.TextBox();
            this.lblConfLevel = new System.Windows.Forms.Label();
            this.txtPredMean = new System.Windows.Forms.TextBox();
            this.lblPredMean = new System.Windows.Forms.Label();
            this.gbData.SuspendLayout();
            this.grpResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbData
            // 
            this.gbData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbData.Controls.Add(this.lblBData);
            this.gbData.Controls.Add(this.lblAData);
            this.gbData.Controls.Add(this.txtBData);
            this.gbData.Controls.Add(this.txtAData);
            this.gbData.Location = new System.Drawing.Point(12, 48);
            this.gbData.Name = "gbData";
            this.gbData.Size = new System.Drawing.Size(760, 176);
            this.gbData.TabIndex = 0;
            this.gbData.TabStop = false;
            this.gbData.Text = "Data";
            this.gbData.SizeChanged += new System.EventHandler(this.gbData_SizeChanged);
            // 
            // lblBData
            // 
            this.lblBData.AutoSize = true;
            this.lblBData.Location = new System.Drawing.Point(561, 16);
            this.lblBData.Name = "lblBData";
            this.lblBData.Size = new System.Drawing.Size(40, 13);
            this.lblBData.TabIndex = 3;
            this.lblBData.Text = "B Data";
            // 
            // lblAData
            // 
            this.lblAData.AutoSize = true;
            this.lblAData.Location = new System.Drawing.Point(163, 16);
            this.lblAData.Name = "lblAData";
            this.lblAData.Size = new System.Drawing.Size(40, 13);
            this.lblAData.TabIndex = 2;
            this.lblAData.Text = "A Data";
            // 
            // txtBData
            // 
            this.txtBData.Location = new System.Drawing.Point(379, 41);
            this.txtBData.Multiline = true;
            this.txtBData.Name = "txtBData";
            this.txtBData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBData.Size = new System.Drawing.Size(375, 129);
            this.txtBData.TabIndex = 1;
            // 
            // txtAData
            // 
            this.txtAData.Location = new System.Drawing.Point(6, 41);
            this.txtAData.Multiline = true;
            this.txtAData.Name = "txtAData";
            this.txtAData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAData.Size = new System.Drawing.Size(367, 129);
            this.txtAData.TabIndex = 0;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(12, 12);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(225, 30);
            this.btnCalculate.TabIndex = 2;
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(243, 12);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(150, 30);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear Data && Results";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // grpResults
            // 
            this.grpResults.Controls.Add(this.rtbResults);
            this.grpResults.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpResults.Location = new System.Drawing.Point(0, 230);
            this.grpResults.Name = "grpResults";
            this.grpResults.Padding = new System.Windows.Forms.Padding(10);
            this.grpResults.Size = new System.Drawing.Size(784, 331);
            this.grpResults.TabIndex = 3;
            this.grpResults.TabStop = false;
            this.grpResults.Text = "Results";
            // 
            // rtbResults
            // 
            this.rtbResults.BackColor = System.Drawing.SystemColors.WindowText;
            this.rtbResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbResults.Location = new System.Drawing.Point(10, 23);
            this.rtbResults.Name = "rtbResults";
            this.rtbResults.ReadOnly = true;
            this.rtbResults.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbResults.Size = new System.Drawing.Size(764, 298);
            this.rtbResults.TabIndex = 0;
            this.rtbResults.TabStop = false;
            this.rtbResults.Text = "";
            // 
            // txtConfLevel
            // 
            this.txtConfLevel.Location = new System.Drawing.Point(527, 18);
            this.txtConfLevel.Name = "txtConfLevel";
            this.txtConfLevel.Size = new System.Drawing.Size(51, 20);
            this.txtConfLevel.TabIndex = 4;
            this.txtConfLevel.TabStop = false;
            this.txtConfLevel.Text = "95";
            // 
            // lblConfLevel
            // 
            this.lblConfLevel.AutoSize = true;
            this.lblConfLevel.Location = new System.Drawing.Point(431, 21);
            this.lblConfLevel.Name = "lblConfLevel";
            this.lblConfLevel.Size = new System.Drawing.Size(90, 13);
            this.lblConfLevel.TabIndex = 5;
            this.lblConfLevel.Text = "Confidence Level";
            // 
            // txtPredMean
            // 
            this.txtPredMean.Location = new System.Drawing.Point(672, 18);
            this.txtPredMean.Name = "txtPredMean";
            this.txtPredMean.Size = new System.Drawing.Size(100, 20);
            this.txtPredMean.TabIndex = 6;
            this.txtPredMean.TabStop = false;
            this.txtPredMean.Text = "0.0";
            // 
            // lblPredMean
            // 
            this.lblPredMean.AutoSize = true;
            this.lblPredMean.Location = new System.Drawing.Point(584, 21);
            this.lblPredMean.Name = "lblPredMean";
            this.lblPredMean.Size = new System.Drawing.Size(82, 13);
            this.lblPredMean.TabIndex = 7;
            this.lblPredMean.Text = "Predicted Mean";
            // 
            // Hypothesishelper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.lblPredMean);
            this.Controls.Add(this.txtPredMean);
            this.Controls.Add(this.lblConfLevel);
            this.Controls.Add(this.txtConfLevel);
            this.Controls.Add(this.grpResults);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.gbData);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Hypothesishelper";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hypothesis Helper v2.31";
            this.gbData.ResumeLayout(false);
            this.gbData.PerformLayout();
            this.grpResults.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbData;
        private System.Windows.Forms.TextBox txtAData;
        private System.Windows.Forms.TextBox txtBData;
        private System.Windows.Forms.Label lblAData;
        private System.Windows.Forms.Label lblBData;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.GroupBox grpResults;
        private System.Windows.Forms.RichTextBox rtbResults;
        private System.Windows.Forms.TextBox txtConfLevel;
        private System.Windows.Forms.Label lblConfLevel;
        private System.Windows.Forms.TextBox txtPredMean;
        private System.Windows.Forms.Label lblPredMean;
    }
}


namespace HypothesisHelper
{
    partial class Datagraph
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.XYchart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.XYchart)).BeginInit();
            this.SuspendLayout();
            // 
            // XYchart
            // 
            this.XYchart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.XYchart.ChartAreas.Add(chartArea1);
            this.XYchart.Location = new System.Drawing.Point(12, 12);
            this.XYchart.Name = "XYchart";
            this.XYchart.Size = new System.Drawing.Size(460, 437);
            this.XYchart.TabIndex = 0;
            this.XYchart.TabStop = false;
            this.XYchart.Text = "X-Y Data Plot";
            // 
            // Datagraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.XYchart);
            this.Name = "Datagraph";
            this.Text = "Normalized A-B Scatter Plot";
            ((System.ComponentModel.ISupportInitialize)(this.XYchart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart XYchart;
    }
}
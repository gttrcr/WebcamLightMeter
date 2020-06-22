namespace WebcamLightMeter
{
    partial class CalibrationForm
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.buttonSubmit = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonComplete = new System.Windows.Forms.Button();
            this.dataGridViewMeasure = new System.Windows.Forms.DataGridView();
            this.RealLuxValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReferenceSnap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chartCalibration = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMeasure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCalibration)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chartCalibration);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 396;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.buttonSubmit);
            this.splitContainer2.Panel1.Controls.Add(this.textBox1);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.buttonComplete);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dataGridViewMeasure);
            this.splitContainer2.Size = new System.Drawing.Size(396, 450);
            this.splitContainer2.SplitterDistance = 86;
            this.splitContainer2.TabIndex = 0;
            // 
            // buttonSubmit
            // 
            this.buttonSubmit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonSubmit.Location = new System.Drawing.Point(0, 36);
            this.buttonSubmit.Name = "buttonSubmit";
            this.buttonSubmit.Size = new System.Drawing.Size(392, 23);
            this.buttonSubmit.TabIndex = 3;
            this.buttonSubmit.Text = "Submit measure";
            this.buttonSubmit.UseVisualStyleBackColor = true;
            this.buttonSubmit.Click += new System.EventHandler(this.ButtonSubmit_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(128, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(170, 22);
            this.textBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Real lux value (lx)";
            // 
            // buttonComplete
            // 
            this.buttonComplete.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonComplete.Location = new System.Drawing.Point(0, 59);
            this.buttonComplete.Name = "buttonComplete";
            this.buttonComplete.Size = new System.Drawing.Size(392, 23);
            this.buttonComplete.TabIndex = 1;
            this.buttonComplete.Text = "Complete calibration";
            this.buttonComplete.UseVisualStyleBackColor = true;
            this.buttonComplete.Click += new System.EventHandler(this.ButtonComplete_Click);
            // 
            // dataGridViewMeasure
            // 
            this.dataGridViewMeasure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMeasure.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RealLuxValue,
            this.ReferenceSnap});
            this.dataGridViewMeasure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMeasure.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewMeasure.Name = "dataGridViewMeasure";
            this.dataGridViewMeasure.RowHeadersWidth = 51;
            this.dataGridViewMeasure.RowTemplate.Height = 24;
            this.dataGridViewMeasure.Size = new System.Drawing.Size(392, 356);
            this.dataGridViewMeasure.TabIndex = 2;
            // 
            // RealLuxValue
            // 
            this.RealLuxValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RealLuxValue.HeaderText = "Real lux value (lx)";
            this.RealLuxValue.MinimumWidth = 6;
            this.RealLuxValue.Name = "RealLuxValue";
            this.RealLuxValue.ReadOnly = true;
            // 
            // ReferenceSnap
            // 
            this.ReferenceSnap.HeaderText = "Snap";
            this.ReferenceSnap.MinimumWidth = 6;
            this.ReferenceSnap.Name = "ReferenceSnap";
            this.ReferenceSnap.ReadOnly = true;
            this.ReferenceSnap.Width = 125;
            // 
            // chartCalibration
            // 
            chartArea1.Name = "ChartArea1";
            this.chartCalibration.ChartAreas.Add(chartArea1);
            this.chartCalibration.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.chartCalibration.Legends.Add(legend1);
            this.chartCalibration.Location = new System.Drawing.Point(0, 0);
            this.chartCalibration.Name = "chartCalibration";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "Submitted values";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Linear dependence";
            this.chartCalibration.Series.Add(series1);
            this.chartCalibration.Series.Add(series2);
            this.chartCalibration.Size = new System.Drawing.Size(396, 446);
            this.chartCalibration.TabIndex = 0;
            this.chartCalibration.Text = "chart1";
            // 
            // CalibrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "CalibrationForm";
            this.Text = "CalibrationForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMeasure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCalibration)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button buttonSubmit;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonComplete;
        private System.Windows.Forms.DataGridView dataGridViewMeasure;
        private System.Windows.Forms.DataGridViewTextBoxColumn RealLuxValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReferenceSnap;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCalibration;
    }
}
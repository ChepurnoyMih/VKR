namespace CooperativeGameSolver
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtNumPlayers = new System.Windows.Forms.TextBox();
            this.lblNumPlayers = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.panelCoalitions = new System.Windows.Forms.Panel();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtNumPlayers
            // 
            this.txtNumPlayers.Location = new System.Drawing.Point(160, 25);
            this.txtNumPlayers.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtNumPlayers.Name = "txtNumPlayers";
            this.txtNumPlayers.Size = new System.Drawing.Size(132, 22);
            this.txtNumPlayers.TabIndex = 1;
            this.txtNumPlayers.Text = "3";
            // 
            // lblNumPlayers
            // 
            this.lblNumPlayers.Location = new System.Drawing.Point(27, 25);
            this.lblNumPlayers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNumPlayers.Name = "lblNumPlayers";
            this.lblNumPlayers.Size = new System.Drawing.Size(120, 25);
            this.lblNumPlayers.TabIndex = 0;
            this.lblNumPlayers.Text = "Число игроков:";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(307, 25);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(160, 37);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Сгенерировать поля";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // panelCoalitions
            // 
            this.panelCoalitions.AutoScroll = true;
            this.panelCoalitions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCoalitions.Location = new System.Drawing.Point(160, 74);
            this.panelCoalitions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelCoalitions.Name = "panelCoalitions";
            this.panelCoalitions.Size = new System.Drawing.Size(399, 399);
            this.panelCoalitions.TabIndex = 3;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(159, 481);
            this.btnCalculate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(133, 37);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "Рассчитать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(578, 74);
            this.txtResult.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(399, 399);
            this.txtResult.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 567);
            this.Controls.Add(this.lblNumPlayers);
            this.Controls.Add(this.txtNumPlayers);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.panelCoalitions);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.txtResult);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Кооперативная игра";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtNumPlayers;
        private System.Windows.Forms.Label lblNumPlayers;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Panel panelCoalitions;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.TextBox txtResult;
    }
}
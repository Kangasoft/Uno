namespace ClassicUno.Forms
{
    partial class FrmStats
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
            this.lblPlayed = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTotalScore = new System.Windows.Forms.Label();
            this.lblWins = new System.Windows.Forms.Label();
            this.lblInfo5 = new System.Windows.Forms.Label();
            this.lblInfo6 = new System.Windows.Forms.Label();
            this.lblInfo7 = new System.Windows.Forms.Label();
            this.lblAvgTime = new System.Windows.Forms.Label();
            this.lblLongest = new System.Windows.Forms.Label();
            this.lblShortest = new System.Windows.Forms.Label();
            this.lblLosses = new System.Windows.Forms.Label();
            this.tblData = new System.Windows.Forms.TableLayoutPanel();
            this.lblInfo1 = new System.Windows.Forms.Label();
            this.lblInfo2 = new System.Windows.Forms.Label();
            this.lblInfo3 = new System.Windows.Forms.Label();
            this.lblInfo4 = new System.Windows.Forms.Label();
            this.pctWheel = new ClassicUno.Controls.PercentageWheel();
            this.btnReset = new System.Windows.Forms.Button();
            this.tblData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPlayed
            // 
            this.lblPlayed.AutoSize = true;
            this.lblPlayed.BackColor = System.Drawing.Color.Transparent;
            this.lblPlayed.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayed.Location = new System.Drawing.Point(179, 0);
            this.lblPlayed.Name = "lblPlayed";
            this.lblPlayed.Size = new System.Drawing.Size(48, 17);
            this.lblPlayed.TabIndex = 2;
            this.lblPlayed.Text = "100000";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(337, 227);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lblTotalScore
            // 
            this.lblTotalScore.AutoSize = true;
            this.lblTotalScore.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalScore.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalScore.Location = new System.Drawing.Point(179, 23);
            this.lblTotalScore.Name = "lblTotalScore";
            this.lblTotalScore.Size = new System.Drawing.Size(69, 17);
            this.lblTotalScore.TabIndex = 4;
            this.lblTotalScore.Text = "100000000";
            // 
            // lblWins
            // 
            this.lblWins.AutoSize = true;
            this.lblWins.BackColor = System.Drawing.Color.Transparent;
            this.lblWins.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWins.Location = new System.Drawing.Point(179, 46);
            this.lblWins.Name = "lblWins";
            this.lblWins.Size = new System.Drawing.Size(55, 17);
            this.lblWins.TabIndex = 5;
            this.lblWins.Text = "1000000";
            // 
            // lblInfo5
            // 
            this.lblInfo5.AutoSize = true;
            this.lblInfo5.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo5.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo5.Location = new System.Drawing.Point(3, 102);
            this.lblInfo5.Name = "lblInfo5";
            this.lblInfo5.Size = new System.Drawing.Size(100, 17);
            this.lblInfo5.TabIndex = 6;
            this.lblInfo5.Text = "Shortest game:";
            // 
            // lblInfo6
            // 
            this.lblInfo6.AutoSize = true;
            this.lblInfo6.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo6.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo6.Location = new System.Drawing.Point(3, 125);
            this.lblInfo6.Name = "lblInfo6";
            this.lblInfo6.Size = new System.Drawing.Size(97, 17);
            this.lblInfo6.TabIndex = 7;
            this.lblInfo6.Text = "Longest game:";
            // 
            // lblInfo7
            // 
            this.lblInfo7.AutoSize = true;
            this.lblInfo7.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo7.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo7.Location = new System.Drawing.Point(3, 148);
            this.lblInfo7.Name = "lblInfo7";
            this.lblInfo7.Size = new System.Drawing.Size(154, 17);
            this.lblInfo7.TabIndex = 8;
            this.lblInfo7.Text = "Average time per game:";
            // 
            // lblAvgTime
            // 
            this.lblAvgTime.AutoSize = true;
            this.lblAvgTime.BackColor = System.Drawing.Color.Transparent;
            this.lblAvgTime.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvgTime.Location = new System.Drawing.Point(179, 148);
            this.lblAvgTime.Name = "lblAvgTime";
            this.lblAvgTime.Size = new System.Drawing.Size(58, 17);
            this.lblAvgTime.TabIndex = 9;
            this.lblAvgTime.Text = "54m 54s";
            // 
            // lblLongest
            // 
            this.lblLongest.AutoSize = true;
            this.lblLongest.BackColor = System.Drawing.Color.Transparent;
            this.lblLongest.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLongest.Location = new System.Drawing.Point(179, 125);
            this.lblLongest.Name = "lblLongest";
            this.lblLongest.Size = new System.Drawing.Size(43, 17);
            this.lblLongest.TabIndex = 10;
            this.lblLongest.Text = "(data)";
            // 
            // lblShortest
            // 
            this.lblShortest.AutoSize = true;
            this.lblShortest.BackColor = System.Drawing.Color.Transparent;
            this.lblShortest.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShortest.Location = new System.Drawing.Point(179, 102);
            this.lblShortest.Name = "lblShortest";
            this.lblShortest.Size = new System.Drawing.Size(43, 17);
            this.lblShortest.TabIndex = 11;
            this.lblShortest.Text = "(data)";
            // 
            // lblLosses
            // 
            this.lblLosses.AutoSize = true;
            this.lblLosses.BackColor = System.Drawing.Color.Transparent;
            this.lblLosses.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLosses.Location = new System.Drawing.Point(179, 69);
            this.lblLosses.Name = "lblLosses";
            this.lblLosses.Size = new System.Drawing.Size(41, 17);
            this.lblLosses.TabIndex = 16;
            this.lblLosses.Text = "10000";
            // 
            // tblData
            // 
            this.tblData.BackColor = System.Drawing.Color.Transparent;
            this.tblData.ColumnCount = 2;
            this.tblData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.tblData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tblData.Controls.Add(this.lblInfo1, 0, 0);
            this.tblData.Controls.Add(this.lblLosses, 1, 3);
            this.tblData.Controls.Add(this.lblPlayed, 1, 0);
            this.tblData.Controls.Add(this.lblInfo2, 0, 1);
            this.tblData.Controls.Add(this.lblTotalScore, 1, 1);
            this.tblData.Controls.Add(this.lblInfo3, 0, 2);
            this.tblData.Controls.Add(this.lblWins, 1, 2);
            this.tblData.Controls.Add(this.lblInfo4, 0, 3);
            this.tblData.Controls.Add(this.lblInfo7, 0, 7);
            this.tblData.Controls.Add(this.lblAvgTime, 1, 7);
            this.tblData.Controls.Add(this.lblInfo6, 0, 6);
            this.tblData.Controls.Add(this.lblLongest, 1, 6);
            this.tblData.Controls.Add(this.lblInfo5, 0, 5);
            this.tblData.Controls.Add(this.lblShortest, 1, 5);
            this.tblData.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblData.Location = new System.Drawing.Point(157, 28);
            this.tblData.Name = "tblData";
            this.tblData.RowCount = 8;
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tblData.Size = new System.Drawing.Size(255, 174);
            this.tblData.TabIndex = 4;
            // 
            // lblInfo1
            // 
            this.lblInfo1.AutoSize = true;
            this.lblInfo1.Location = new System.Drawing.Point(3, 0);
            this.lblInfo1.Name = "lblInfo1";
            this.lblInfo1.Size = new System.Drawing.Size(51, 17);
            this.lblInfo1.TabIndex = 0;
            this.lblInfo1.Text = "Played:";
            // 
            // lblInfo2
            // 
            this.lblInfo2.AutoSize = true;
            this.lblInfo2.Location = new System.Drawing.Point(3, 23);
            this.lblInfo2.Name = "lblInfo2";
            this.lblInfo2.Size = new System.Drawing.Size(44, 17);
            this.lblInfo2.TabIndex = 3;
            this.lblInfo2.Text = "Score:";
            // 
            // lblInfo3
            // 
            this.lblInfo3.AutoSize = true;
            this.lblInfo3.Location = new System.Drawing.Point(3, 46);
            this.lblInfo3.Name = "lblInfo3";
            this.lblInfo3.Size = new System.Drawing.Size(41, 17);
            this.lblInfo3.TabIndex = 5;
            this.lblInfo3.Text = "Wins:";
            // 
            // lblInfo4
            // 
            this.lblInfo4.AutoSize = true;
            this.lblInfo4.Location = new System.Drawing.Point(3, 69);
            this.lblInfo4.Name = "lblInfo4";
            this.lblInfo4.Size = new System.Drawing.Size(50, 17);
            this.lblInfo4.TabIndex = 17;
            this.lblInfo4.Text = "Losses:";
            // 
            // pctWheel
            // 
            this.pctWheel.BackColor = System.Drawing.Color.Transparent;
            this.pctWheel.Location = new System.Drawing.Point(12, 12);
            this.pctWheel.MaximumSize = new System.Drawing.Size(122, 122);
            this.pctWheel.MinimumSize = new System.Drawing.Size(122, 122);
            this.pctWheel.Name = "pctWheel";
            this.pctWheel.ProgressArcColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(166)))), ((int)(((byte)(81)))));
            this.pctWheel.ProgressColor = System.Drawing.Color.Black;
            this.pctWheel.Size = new System.Drawing.Size(122, 122);
            this.pctWheel.TabIndex = 3;
            this.pctWheel.TabStop = false;
            this.pctWheel.Value = 0D;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(256, 227);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // FrmStats
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 262);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.tblData);
            this.Controls.Add(this.pctWheel);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmStats";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game Statistics";
            this.tblData.ResumeLayout(false);
            this.tblData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblPlayed;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblWins;
        private System.Windows.Forms.Label lblInfo5;
        private System.Windows.Forms.Label lblInfo6;
        private System.Windows.Forms.Label lblInfo7;
        private System.Windows.Forms.Label lblAvgTime;
        private System.Windows.Forms.Label lblLongest;
        private System.Windows.Forms.Label lblShortest;
        private System.Windows.Forms.Label lblTotalScore;
        private Controls.PercentageWheel pctWheel;
        private System.Windows.Forms.Label lblLosses;
        private System.Windows.Forms.TableLayoutPanel tblData;
        private System.Windows.Forms.Label lblInfo1;
        private System.Windows.Forms.Label lblInfo2;
        private System.Windows.Forms.Label lblInfo3;
        private System.Windows.Forms.Label lblInfo4;
        private System.Windows.Forms.Button btnReset;
    }
}
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    sealed partial class FrmGame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGame));
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuGame = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.slGame = new System.Windows.Forms.ToolStripStatusLabel();
            this.slDirection = new System.Windows.Forms.ToolStripStatusLabel();
            this.slColor = new System.Windows.Forms.ToolStripStatusLabel();
            this.slTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnDraw = new ClassicUno.Controls.CustomButton();
            this.btnPass = new ClassicUno.Controls.CustomButton();
            this.tblButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnYellow = new ClassicUno.Controls.CustomButton();
            this.btnRed = new ClassicUno.Controls.CustomButton();
            this.btnGreen = new ClassicUno.Controls.CustomButton();
            this.btnBlue = new ClassicUno.Controls.CustomButton();
            this.mnuMain.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.tblButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGame,
            this.mnuOptions,
            this.mnuHelp});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.mnuMain.Size = new System.Drawing.Size(914, 24);
            this.mnuMain.TabIndex = 0;
            this.mnuMain.Text = "menuStrip1";
            // 
            // mnuGame
            // 
            this.mnuGame.Name = "mnuGame";
            this.mnuGame.Size = new System.Drawing.Size(50, 20);
            this.mnuGame.Text = "Game";
            // 
            // mnuOptions
            // 
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(61, 20);
            this.mnuOptions.Text = "Options";
            // 
            // mnuHelp
            // 
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(44, 20);
            this.mnuHelp.Text = "Help";
            // 
            // statusBar
            // 
            this.statusBar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slGame,
            this.slDirection,
            this.slColor,
            this.slTime});
            this.statusBar.Location = new System.Drawing.Point(0, 599);
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusBar.Size = new System.Drawing.Size(914, 22);
            this.statusBar.TabIndex = 1;
            this.statusBar.Text = "statusStrip1";
            // 
            // slGame
            // 
            this.slGame.Name = "slGame";
            this.slGame.Size = new System.Drawing.Size(457, 17);
            this.slGame.Spring = true;
            this.slGame.Text = "Ready!";
            this.slGame.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slDirection
            // 
            this.slDirection.AutoSize = false;
            this.slDirection.Name = "slDirection";
            this.slDirection.Size = new System.Drawing.Size(200, 17);
            this.slDirection.Text = "Direction: Clockwise";
            this.slDirection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slColor
            // 
            this.slColor.AutoSize = false;
            this.slColor.Name = "slColor";
            this.slColor.Size = new System.Drawing.Size(120, 17);
            this.slColor.Text = "Color: None";
            this.slColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slTime
            // 
            this.slTime.AutoSize = false;
            this.slTime.Name = "slTime";
            this.slTime.Size = new System.Drawing.Size(120, 17);
            this.slTime.Text = "Time: 0s";
            this.slTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDraw
            // 
            this.btnDraw.BackColor = System.Drawing.Color.Transparent;
            this.btnDraw.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDraw.BackgroundImage")));
            this.btnDraw.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDraw.Enabled = false;
            this.btnDraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDraw.Location = new System.Drawing.Point(3, 3);
            this.btnDraw.Name = "btnDraw";
            this.btnDraw.Size = new System.Drawing.Size(64, 55);
            this.btnDraw.TabIndex = 2;
            this.btnDraw.Tag = "DRAW";
            this.btnDraw.UseVisualStyleBackColor = false;
            // 
            // btnPass
            // 
            this.btnPass.BackColor = System.Drawing.Color.Transparent;
            this.btnPass.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPass.BackgroundImage")));
            this.btnPass.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPass.Enabled = false;
            this.btnPass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPass.Location = new System.Drawing.Point(73, 3);
            this.btnPass.Name = "btnPass";
            this.btnPass.Size = new System.Drawing.Size(61, 55);
            this.btnPass.TabIndex = 3;
            this.btnPass.Tag = "PASS";
            this.btnPass.UseVisualStyleBackColor = false;
            // 
            // tblButtons
            // 
            this.tblButtons.BackColor = System.Drawing.Color.Transparent;
            this.tblButtons.ColumnCount = 7;
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tblButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tblButtons.Controls.Add(this.btnYellow, 6, 0);
            this.tblButtons.Controls.Add(this.btnRed, 3, 0);
            this.tblButtons.Controls.Add(this.btnGreen, 5, 0);
            this.tblButtons.Controls.Add(this.btnDraw, 0, 0);
            this.tblButtons.Controls.Add(this.btnBlue, 4, 0);
            this.tblButtons.Controls.Add(this.btnPass, 1, 0);
            this.tblButtons.Location = new System.Drawing.Point(247, 533);
            this.tblButtons.Name = "tblButtons";
            this.tblButtons.RowCount = 1;
            this.tblButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tblButtons.Size = new System.Drawing.Size(402, 63);
            this.tblButtons.TabIndex = 4;
            this.tblButtons.Visible = false;
            // 
            // btnYellow
            // 
            this.btnYellow.BackColor = System.Drawing.Color.Transparent;
            this.btnYellow.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnYellow.BackgroundImage")));
            this.btnYellow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnYellow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnYellow.Location = new System.Drawing.Point(343, 3);
            this.btnYellow.Name = "btnYellow";
            this.btnYellow.Size = new System.Drawing.Size(54, 55);
            this.btnYellow.TabIndex = 8;
            this.btnYellow.Tag = "YELLOW";
            this.btnYellow.UseVisualStyleBackColor = false;
            this.btnYellow.Visible = false;
            // 
            // btnRed
            // 
            this.btnRed.BackColor = System.Drawing.Color.Transparent;
            this.btnRed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRed.BackgroundImage")));
            this.btnRed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnRed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRed.Location = new System.Drawing.Point(163, 3);
            this.btnRed.Name = "btnRed";
            this.btnRed.Size = new System.Drawing.Size(54, 55);
            this.btnRed.TabIndex = 5;
            this.btnRed.Tag = "RED";
            this.btnRed.UseVisualStyleBackColor = false;
            this.btnRed.Visible = false;
            // 
            // btnGreen
            // 
            this.btnGreen.BackColor = System.Drawing.Color.Transparent;
            this.btnGreen.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnGreen.BackgroundImage")));
            this.btnGreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnGreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGreen.Location = new System.Drawing.Point(283, 3);
            this.btnGreen.Name = "btnGreen";
            this.btnGreen.Size = new System.Drawing.Size(54, 55);
            this.btnGreen.TabIndex = 7;
            this.btnGreen.Tag = "GREEN";
            this.btnGreen.UseVisualStyleBackColor = false;
            this.btnGreen.Visible = false;
            // 
            // btnBlue
            // 
            this.btnBlue.BackColor = System.Drawing.Color.Transparent;
            this.btnBlue.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBlue.BackgroundImage")));
            this.btnBlue.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnBlue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBlue.Location = new System.Drawing.Point(223, 3);
            this.btnBlue.Name = "btnBlue";
            this.btnBlue.Size = new System.Drawing.Size(54, 55);
            this.btnBlue.TabIndex = 6;
            this.btnBlue.Tag = "BLUE";
            this.btnBlue.UseVisualStyleBackColor = false;
            this.btnBlue.Visible = false;
            // 
            // FrmGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 621);
            this.Controls.Add(this.tblButtons);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.mnuMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.mnuMain;
            this.MinimumSize = new System.Drawing.Size(930, 660);
            this.Name = "FrmGame";
            this.Text = "Classic Uno 2026";
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.tblButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuGame;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private CustomButton btnDraw;
        private CustomButton btnPass;
        private System.Windows.Forms.TableLayoutPanel tblButtons;
        private CustomButton btnRed;
        private CustomButton btnYellow;
        private CustomButton btnGreen;
        private CustomButton btnBlue;
        private System.Windows.Forms.ToolStripStatusLabel slGame;
        private System.Windows.Forms.ToolStripStatusLabel slDirection;
        private System.Windows.Forms.ToolStripStatusLabel slTime;
        private System.Windows.Forms.ToolStripStatusLabel slColor;
    }
}


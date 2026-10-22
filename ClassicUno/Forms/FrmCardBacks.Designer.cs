namespace ClassicUno.Forms
{
    partial class FrmCardBacks
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCardBacks));
            this.btnClose = new System.Windows.Forms.Button();
            this.ctlCards = new ClassicUno.Classes.UI.Controls.CardStripControl();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(345, 225);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Ok";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // ctlCards
            // 
            this.ctlCards.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(80)))), ((int)(((byte)(50)))));
            this.ctlCards.CardSize = new System.Drawing.Size(72, 108);
            this.ctlCards.CardSpacing = 6;
            this.ctlCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.ctlCards.Images = ((System.Collections.Generic.List<System.Drawing.Image>)(resources.GetObject("ctlCards.Images")));
            this.ctlCards.Index = -1;
            this.ctlCards.Location = new System.Drawing.Point(0, 0);
            this.ctlCards.Name = "ctlCards";
            this.ctlCards.Size = new System.Drawing.Size(432, 220);
            this.ctlCards.TabIndex = 1;
            // 
            // FrmCardBacks
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 260);
            this.Controls.Add(this.ctlCards);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCardBacks";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select card back image";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private Classes.UI.Controls.CardStripControl ctlCards;
    }
}
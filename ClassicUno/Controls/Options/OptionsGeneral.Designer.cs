namespace ClassicUno.Controls.Options
{
    partial class OptionsGeneral
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbConfirm = new System.Windows.Forms.GroupBox();
            this.chkNew = new System.Windows.Forms.CheckBox();
            this.chkExit = new System.Windows.Forms.CheckBox();
            this.gbExit = new System.Windows.Forms.GroupBox();
            this.chkSave = new System.Windows.Forms.CheckBox();
            this.chkLoad = new System.Windows.Forms.CheckBox();
            this.gbConfirm.SuspendLayout();
            this.gbExit.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbConfirm
            // 
            this.gbConfirm.Controls.Add(this.chkNew);
            this.gbConfirm.Controls.Add(this.chkExit);
            this.gbConfirm.Location = new System.Drawing.Point(3, 3);
            this.gbConfirm.Name = "gbConfirm";
            this.gbConfirm.Size = new System.Drawing.Size(300, 74);
            this.gbConfirm.TabIndex = 0;
            this.gbConfirm.TabStop = false;
            this.gbConfirm.Text = "Confirm game resignation:";
            // 
            // chkNew
            // 
            this.chkNew.AutoSize = true;
            this.chkNew.Location = new System.Drawing.Point(10, 47);
            this.chkNew.Name = "chkNew";
            this.chkNew.Size = new System.Drawing.Size(170, 19);
            this.chkNew.TabIndex = 1;
            this.chkNew.Tag = "NEW";
            this.chkNew.Text = "Before starting a new game";
            this.chkNew.UseVisualStyleBackColor = true;
            // 
            // chkExit
            // 
            this.chkExit.AutoSize = true;
            this.chkExit.Location = new System.Drawing.Point(10, 22);
            this.chkExit.Name = "chkExit";
            this.chkExit.Size = new System.Drawing.Size(180, 19);
            this.chkExit.TabIndex = 0;
            this.chkExit.Tag = "EXIT";
            this.chkExit.Text = "When closing the application";
            this.chkExit.UseVisualStyleBackColor = true;
            // 
            // gbExit
            // 
            this.gbExit.Controls.Add(this.chkLoad);
            this.gbExit.Controls.Add(this.chkSave);
            this.gbExit.Location = new System.Drawing.Point(3, 83);
            this.gbExit.Name = "gbExit";
            this.gbExit.Size = new System.Drawing.Size(300, 74);
            this.gbExit.TabIndex = 1;
            this.gbExit.TabStop = false;
            this.gbExit.Text = "On exit/start:";
            // 
            // chkSave
            // 
            this.chkSave.AutoSize = true;
            this.chkSave.Location = new System.Drawing.Point(10, 22);
            this.chkSave.Name = "chkSave";
            this.chkSave.Size = new System.Drawing.Size(169, 19);
            this.chkSave.TabIndex = 2;
            this.chkSave.Tag = "SAVE";
            this.chkSave.Text = "Save current running game";
            this.chkSave.UseVisualStyleBackColor = true;
            // 
            // chkLoad
            // 
            this.chkLoad.AutoSize = true;
            this.chkLoad.Enabled = false;
            this.chkLoad.Location = new System.Drawing.Point(30, 47);
            this.chkLoad.Name = "chkLoad";
            this.chkLoad.Size = new System.Drawing.Size(124, 19);
            this.chkLoad.TabIndex = 3;
            this.chkLoad.Tag = "LOAD";
            this.chkLoad.Text = "Reload on start-up";
            this.chkLoad.UseVisualStyleBackColor = true;
            // 
            // OptionsGeneral
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.gbExit);
            this.Controls.Add(this.gbConfirm);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "OptionsGeneral";
            this.Size = new System.Drawing.Size(306, 292);
            this.gbConfirm.ResumeLayout(false);
            this.gbConfirm.PerformLayout();
            this.gbExit.ResumeLayout(false);
            this.gbExit.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbConfirm;
        private System.Windows.Forms.CheckBox chkNew;
        private System.Windows.Forms.CheckBox chkExit;
        private System.Windows.Forms.GroupBox gbExit;
        private System.Windows.Forms.CheckBox chkLoad;
        private System.Windows.Forms.CheckBox chkSave;
    }
}

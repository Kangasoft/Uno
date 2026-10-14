using ClassicUno.Controls;

namespace ClassicUno.Controls.Options
{
    partial class OptionsAudio
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
            this.gbMusic = new System.Windows.Forms.GroupBox();
            this.tbMusic = new ClassicUno.Controls.VuMeterTrackBar();
            this.lblMusicVol = new System.Windows.Forms.Label();
            this.chkMusic = new System.Windows.Forms.CheckBox();
            this.gbSfx = new System.Windows.Forms.GroupBox();
            this.tbSfx = new ClassicUno.Controls.VuMeterTrackBar();
            this.lblSfxVol = new System.Windows.Forms.Label();
            this.chkSfx = new System.Windows.Forms.CheckBox();
            this.gbVoice = new System.Windows.Forms.GroupBox();
            this.tbVoice = new ClassicUno.Controls.VuMeterTrackBar();
            this.lblVoiceVol = new System.Windows.Forms.Label();
            this.chkVoice = new System.Windows.Forms.CheckBox();
            this.gbMusic.SuspendLayout();
            this.gbSfx.SuspendLayout();
            this.gbVoice.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMusic
            // 
            this.gbMusic.BackColor = System.Drawing.Color.Transparent;
            this.gbMusic.Controls.Add(this.tbMusic);
            this.gbMusic.Controls.Add(this.lblMusicVol);
            this.gbMusic.Controls.Add(this.chkMusic);
            this.gbMusic.Location = new System.Drawing.Point(0, 195);
            this.gbMusic.Name = "gbMusic";
            this.gbMusic.Size = new System.Drawing.Size(303, 90);
            this.gbMusic.TabIndex = 7;
            this.gbMusic.TabStop = false;
            this.gbMusic.Text = "Music:";
            // 
            // tbMusic
            // 
            this.tbMusic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tbMusic.Location = new System.Drawing.Point(10, 62);
            this.tbMusic.Maximum = 100;
            this.tbMusic.Minimum = 0;
            this.tbMusic.Name = "tbMusic";
            this.tbMusic.Segments = 15;
            this.tbMusic.Size = new System.Drawing.Size(282, 16);
            this.tbMusic.TabIndex = 2;
            this.tbMusic.Tag = "TBMUSIC";
            this.tbMusic.Value = 50;
            // 
            // lblMusicVol
            // 
            this.lblMusicVol.AutoSize = true;
            this.lblMusicVol.Location = new System.Drawing.Point(7, 44);
            this.lblMusicVol.Name = "lblMusicVol";
            this.lblMusicVol.Size = new System.Drawing.Size(50, 15);
            this.lblMusicVol.TabIndex = 1;
            this.lblMusicVol.Text = "Volume:";
            // 
            // chkMusic
            // 
            this.chkMusic.AutoSize = true;
            this.chkMusic.Location = new System.Drawing.Point(10, 22);
            this.chkMusic.Name = "chkMusic";
            this.chkMusic.Size = new System.Drawing.Size(96, 19);
            this.chkMusic.TabIndex = 0;
            this.chkMusic.Tag = "MUSICCHK";
            this.chkMusic.Text = "Enable music";
            this.chkMusic.UseVisualStyleBackColor = true;
            // 
            // gbSfx
            // 
            this.gbSfx.BackColor = System.Drawing.Color.Transparent;
            this.gbSfx.Controls.Add(this.tbSfx);
            this.gbSfx.Controls.Add(this.lblSfxVol);
            this.gbSfx.Controls.Add(this.chkSfx);
            this.gbSfx.Location = new System.Drawing.Point(0, 99);
            this.gbSfx.Name = "gbSfx";
            this.gbSfx.Size = new System.Drawing.Size(303, 90);
            this.gbSfx.TabIndex = 6;
            this.gbSfx.TabStop = false;
            this.gbSfx.Text = "Voices:";
            // 
            // tbSfx
            // 
            this.tbSfx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tbSfx.Location = new System.Drawing.Point(10, 62);
            this.tbSfx.Maximum = 100;
            this.tbSfx.Minimum = 0;
            this.tbSfx.Name = "tbSfx";
            this.tbSfx.Segments = 15;
            this.tbSfx.Size = new System.Drawing.Size(282, 16);
            this.tbSfx.TabIndex = 2;
            this.tbSfx.Tag = "TBSFX";
            this.tbSfx.Value = 50;
            // 
            // lblSfxVol
            // 
            this.lblSfxVol.AutoSize = true;
            this.lblSfxVol.Location = new System.Drawing.Point(7, 44);
            this.lblSfxVol.Name = "lblSfxVol";
            this.lblSfxVol.Size = new System.Drawing.Size(50, 15);
            this.lblSfxVol.TabIndex = 1;
            this.lblSfxVol.Text = "Volume:";
            // 
            // chkSfx
            // 
            this.chkSfx.AutoSize = true;
            this.chkSfx.Location = new System.Drawing.Point(10, 22);
            this.chkSfx.Name = "chkSfx";
            this.chkSfx.Size = new System.Drawing.Size(161, 19);
            this.chkSfx.TabIndex = 0;
            this.chkSfx.Tag = "SFXCHK";
            this.chkSfx.Text = "Enable card sound effects";
            this.chkSfx.UseVisualStyleBackColor = true;
            // 
            // gbVoice
            // 
            this.gbVoice.BackColor = System.Drawing.Color.Transparent;
            this.gbVoice.Controls.Add(this.tbVoice);
            this.gbVoice.Controls.Add(this.lblVoiceVol);
            this.gbVoice.Controls.Add(this.chkVoice);
            this.gbVoice.Location = new System.Drawing.Point(3, 3);
            this.gbVoice.Name = "gbVoice";
            this.gbVoice.Size = new System.Drawing.Size(300, 90);
            this.gbVoice.TabIndex = 5;
            this.gbVoice.TabStop = false;
            this.gbVoice.Text = "Voices:";
            // 
            // tbVoice
            // 
            this.tbVoice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tbVoice.Location = new System.Drawing.Point(10, 62);
            this.tbVoice.Maximum = 100;
            this.tbVoice.Minimum = 0;
            this.tbVoice.Name = "tbVoice";
            this.tbVoice.Segments = 15;
            this.tbVoice.Size = new System.Drawing.Size(282, 16);
            this.tbVoice.TabIndex = 2;
            this.tbVoice.Tag = "TBVOICE";
            this.tbVoice.Value = 50;
            // 
            // lblVoiceVol
            // 
            this.lblVoiceVol.AutoSize = true;
            this.lblVoiceVol.Location = new System.Drawing.Point(7, 44);
            this.lblVoiceVol.Name = "lblVoiceVol";
            this.lblVoiceVol.Size = new System.Drawing.Size(50, 15);
            this.lblVoiceVol.TabIndex = 1;
            this.lblVoiceVol.Text = "Volume:";
            // 
            // chkVoice
            // 
            this.chkVoice.AutoSize = true;
            this.chkVoice.Location = new System.Drawing.Point(10, 22);
            this.chkVoice.Name = "chkVoice";
            this.chkVoice.Size = new System.Drawing.Size(97, 19);
            this.chkVoice.TabIndex = 0;
            this.chkVoice.Tag = "VOICECHK";
            this.chkVoice.Text = "Enable voices";
            this.chkVoice.UseVisualStyleBackColor = true;
            // 
            // OptionsAudio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.gbMusic);
            this.Controls.Add(this.gbSfx);
            this.Controls.Add(this.gbVoice);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "OptionsAudio";
            this.Size = new System.Drawing.Size(306, 292);
            this.gbMusic.ResumeLayout(false);
            this.gbMusic.PerformLayout();
            this.gbSfx.ResumeLayout(false);
            this.gbSfx.PerformLayout();
            this.gbVoice.ResumeLayout(false);
            this.gbVoice.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbMusic;
        private VuMeterTrackBar tbMusic;
        private System.Windows.Forms.Label lblMusicVol;
        private System.Windows.Forms.CheckBox chkMusic;
        private System.Windows.Forms.GroupBox gbSfx;
        private VuMeterTrackBar tbSfx;
        private System.Windows.Forms.Label lblSfxVol;
        private System.Windows.Forms.CheckBox chkSfx;
        private System.Windows.Forms.GroupBox gbVoice;
        private VuMeterTrackBar tbVoice;
        private System.Windows.Forms.Label lblVoiceVol;
        private System.Windows.Forms.CheckBox chkVoice;
    }
}

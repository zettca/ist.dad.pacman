namespace pacman {
    partial class FormPacman {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.labelScore = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tbChat = new System.Windows.Forms.TextBox();
            this.labelForScore = new System.Windows.Forms.Label();
            this.labelForPos = new System.Windows.Forms.Label();
            this.panelCanvas = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // labelScore
            // 
            this.labelScore.AutoSize = true;
            this.labelScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScore.Location = new System.Drawing.Point(366, 25);
            this.labelScore.Name = "labelScore";
            this.labelScore.Size = new System.Drawing.Size(60, 24);
            this.labelScore.TabIndex = 71;
            this.labelScore.Text = "Score";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(442, 23);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(90, 26);
            this.labelTitle.TabIndex = 72;
            this.labelTitle.Text = "Position";
            // 
            // tbMsg
            // 
            this.tbMsg.Enabled = false;
            this.tbMsg.Location = new System.Drawing.Point(366, 297);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(166, 20);
            this.tbMsg.TabIndex = 143;
            this.tbMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbMsg_KeyDown);
            // 
            // tbChat
            // 
            this.tbChat.Enabled = false;
            this.tbChat.Location = new System.Drawing.Point(366, 52);
            this.tbChat.Multiline = true;
            this.tbChat.Name = "tbChat";
            this.tbChat.Size = new System.Drawing.Size(166, 239);
            this.tbChat.TabIndex = 144;
            // 
            // labelForScore
            // 
            this.labelForScore.AutoSize = true;
            this.labelForScore.Location = new System.Drawing.Point(367, 12);
            this.labelForScore.Name = "labelForScore";
            this.labelForScore.Size = new System.Drawing.Size(35, 13);
            this.labelForScore.TabIndex = 145;
            this.labelForScore.Text = "Score";
            // 
            // labelForPos
            // 
            this.labelForPos.AutoSize = true;
            this.labelForPos.Location = new System.Drawing.Point(444, 12);
            this.labelForPos.Name = "labelForPos";
            this.labelForPos.Size = new System.Drawing.Size(44, 13);
            this.labelForPos.TabIndex = 146;
            this.labelForPos.Text = "Position";
            // 
            // panelCanvas
            // 
            this.panelCanvas.BackColor = System.Drawing.Color.DimGray;
            this.panelCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCanvas.Location = new System.Drawing.Point(12, 12);
            this.panelCanvas.Name = "panelCanvas";
            this.panelCanvas.Size = new System.Drawing.Size(348, 305);
            this.panelCanvas.TabIndex = 147;
            // 
            // FormPacman
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(543, 324);
            this.Controls.Add(this.panelCanvas);
            this.Controls.Add(this.labelForPos);
            this.Controls.Add(this.labelForScore);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelScore);
            this.Name = "FormPacman";
            this.Text = "DADman";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelScore;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TextBox tbChat;
        private System.Windows.Forms.Label labelForScore;
        private System.Windows.Forms.Label labelForPos;
        private System.Windows.Forms.Panel panelCanvas;
    }
}


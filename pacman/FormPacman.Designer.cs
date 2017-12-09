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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPacman));
            this.labelScore = new System.Windows.Forms.Label();
            this.labelState = new System.Windows.Forms.Label();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tbChat = new System.Windows.Forms.TextBox();
            this.labelForScore = new System.Windows.Forms.Label();
            this.labelForState = new System.Windows.Forms.Label();
            this.panelCanvas = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // labelScore
            // 
            this.labelScore.AutoSize = true;
            this.labelScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScore.Location = new System.Drawing.Point(366, 25);
            this.labelScore.Name = "labelScore";
            this.labelScore.Size = new System.Drawing.Size(51, 20);
            this.labelScore.TabIndex = 71;
            this.labelScore.Text = "Score";
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelState.Location = new System.Drawing.Point(422, 25);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(65, 20);
            this.labelState.TabIndex = 72;
            this.labelState.Text = "Position";
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
            // labelForState
            // 
            this.labelForState.AutoSize = true;
            this.labelForState.Location = new System.Drawing.Point(423, 12);
            this.labelForState.Name = "labelForState";
            this.labelForState.Size = new System.Drawing.Size(44, 13);
            this.labelForState.TabIndex = 146;
            this.labelForState.Text = "Position";
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
            this.Controls.Add(this.labelForState);
            this.Controls.Add(this.labelForScore);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.labelState);
            this.Controls.Add(this.labelScore);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormPacman";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "DADman";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelScore;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TextBox tbChat;
        private System.Windows.Forms.Label labelForScore;
        private System.Windows.Forms.Label labelForState;
        private System.Windows.Forms.Panel panelCanvas;
    }
}


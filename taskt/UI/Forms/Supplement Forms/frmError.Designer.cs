namespace taskt.UI.Forms.Supplement_Forms
{
    partial class frmError
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmError));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.uiBtnContinue = new taskt.UI.CustomControls.CustomUIControls.UIPictureButton();
            this.uiBtnEnd = new taskt.UI.CustomControls.CustomUIControls.UIPictureButton();
            this.uiBtnCopyError = new taskt.UI.CustomControls.CustomUIControls.UIPictureButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnContinue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnCopyError)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblErrorMessage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(580, 169);
            this.panel1.TabIndex = 0;
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI Light", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblErrorMessage.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblErrorMessage.Location = new System.Drawing.Point(0, 0);
            this.lblErrorMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(580, 169);
            this.lblErrorMessage.TabIndex = 18;
            this.lblErrorMessage.Text = "lblErrorMessage";
            // 
            // uiBtnContinue
            // 
            this.uiBtnContinue.BackColor = System.Drawing.Color.Transparent;
            this.uiBtnContinue.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiBtnContinue.DisplayText = "Continue";
            this.uiBtnContinue.DisplayTextBrush = System.Drawing.Color.White;
            this.uiBtnContinue.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.uiBtnContinue.Image = ((System.Drawing.Image)(resources.GetObject("uiBtnContinue.Image")));
            this.uiBtnContinue.IsMouseOver = false;
            this.uiBtnContinue.Location = new System.Drawing.Point(419, 177);
            this.uiBtnContinue.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.uiBtnContinue.Name = "uiBtnContinue";
            this.uiBtnContinue.Size = new System.Drawing.Size(71, 60);
            this.uiBtnContinue.TabIndex = 23;
            this.uiBtnContinue.TabStop = false;
            this.uiBtnContinue.Text = "Continue";
            this.uiBtnContinue.Click += new System.EventHandler(this.uiBtnContinue_Click);
            // 
            // uiBtnEnd
            // 
            this.uiBtnEnd.BackColor = System.Drawing.Color.Transparent;
            this.uiBtnEnd.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiBtnEnd.DisplayText = "End";
            this.uiBtnEnd.DisplayTextBrush = System.Drawing.Color.White;
            this.uiBtnEnd.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.uiBtnEnd.Image = ((System.Drawing.Image)(resources.GetObject("uiBtnEnd.Image")));
            this.uiBtnEnd.IsMouseOver = false;
            this.uiBtnEnd.Location = new System.Drawing.Point(497, 178);
            this.uiBtnEnd.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.uiBtnEnd.Name = "uiBtnEnd";
            this.uiBtnEnd.Size = new System.Drawing.Size(71, 60);
            this.uiBtnEnd.TabIndex = 24;
            this.uiBtnEnd.TabStop = false;
            this.uiBtnEnd.Text = "End";
            this.uiBtnEnd.Click += new System.EventHandler(this.uiBtnEnd_Click);
            // 
            // uiBtnCopyError
            // 
            this.uiBtnCopyError.BackColor = System.Drawing.Color.Transparent;
            this.uiBtnCopyError.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiBtnCopyError.DisplayText = "Copy Error";
            this.uiBtnCopyError.DisplayTextBrush = System.Drawing.Color.White;
            this.uiBtnCopyError.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.uiBtnCopyError.Image = global::taskt.Properties.Resources.copy;
            this.uiBtnCopyError.IsMouseOver = false;
            this.uiBtnCopyError.Location = new System.Drawing.Point(14, 177);
            this.uiBtnCopyError.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.uiBtnCopyError.Name = "uiBtnCopyError";
            this.uiBtnCopyError.Size = new System.Drawing.Size(86, 60);
            this.uiBtnCopyError.TabIndex = 25;
            this.uiBtnCopyError.TabStop = false;
            this.uiBtnCopyError.Text = "Copy Error";
            this.uiBtnCopyError.Click += new System.EventHandler(this.uiBtnCopyError_Click);
            // 
            // frmError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 247);
            this.Controls.Add(this.uiBtnCopyError);
            this.Controls.Add(this.uiBtnContinue);
            this.Controls.Add(this.uiBtnEnd);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error";
            this.TopMost = true;
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnContinue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiBtnCopyError)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private CustomControls.CustomUIControls.UIPictureButton uiBtnContinue;
        private CustomControls.CustomUIControls.UIPictureButton uiBtnEnd;
        private System.Windows.Forms.Label lblErrorMessage;
        private CustomControls.CustomUIControls.UIPictureButton uiBtnCopyError;
    }
}
namespace com.esp.fakefelica
{
    partial class CardForm
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
            this.btEmulate = new System.Windows.Forms.Button();
            this.tbIdm = new System.Windows.Forms.TextBox();
            this.tbUri = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btEmulate
            // 
            this.btEmulate.Location = new System.Drawing.Point(349, 12);
            this.btEmulate.Name = "btEmulate";
            this.btEmulate.Size = new System.Drawing.Size(75, 65);
            this.btEmulate.TabIndex = 0;
            this.btEmulate.Text = "EMULATE";
            this.btEmulate.UseVisualStyleBackColor = true;
            this.btEmulate.Click += new System.EventHandler(this.btEmulate_Click);
            // 
            // tbIdm
            // 
            this.tbIdm.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tbIdm.Location = new System.Drawing.Point(73, 16);
            this.tbIdm.Name = "tbIdm";
            this.tbIdm.Size = new System.Drawing.Size(236, 23);
            this.tbIdm.TabIndex = 1;
            this.tbIdm.Text = "11 16 03 00 E8 0D D0 0B ";
            // 
            // tbUri
            // 
            this.tbUri.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tbUri.Location = new System.Drawing.Point(73, 45);
            this.tbUri.Name = "tbUri";
            this.tbUri.Size = new System.Drawing.Size(236, 23);
            this.tbUri.TabIndex = 1;
            this.tbUri.Text = "http://www.google.com/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "URL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(12, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "iDm";
            // 
            // CardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 92);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbUri);
            this.Controls.Add(this.tbIdm);
            this.Controls.Add(this.btEmulate);
            this.Name = "CardForm";
            this.Text = "CardForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CardBankForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btEmulate;
        private System.Windows.Forms.TextBox tbIdm;
        private System.Windows.Forms.TextBox tbUri;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
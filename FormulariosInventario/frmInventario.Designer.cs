namespace TRAMADE
{
    partial class frmInventario
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // frmInventario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(240)))), ((int)(((byte)(235)))));
            this.ClientSize = new System.Drawing.Size(963, 798);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmInventario";
            this.Text = "frmInventario";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmInventario_FormClosed);
            this.Load += new System.EventHandler(this.frmInventario_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
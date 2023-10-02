
namespace ChannelAccess_WinFormsApp
{
  partial class LinksToPvChannels_UserControl
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
      this.m_button = new System.Windows.Forms.Button();
      this.m_textBox = new System.Windows.Forms.TextBox();
      this.m_pvInfoLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // m_button
      // 
      this.m_button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_button.Location = new System.Drawing.Point(12, 48);
      this.m_button.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.m_button.Name = "m_button";
      this.m_button.Size = new System.Drawing.Size(666, 29);
      this.m_button.TabIndex = 0;
      this.m_button.Text = "Click to increment (via Channel)";
      this.m_button.UseVisualStyleBackColor = true;
      this.m_button.Click += new System.EventHandler(this.m_button_Click);
      // 
      // m_textBox
      // 
      this.m_textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_textBox.Location = new System.Drawing.Point(12, 85);
      this.m_textBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.m_textBox.Name = "m_textBox";
      this.m_textBox.ReadOnly = true;
      this.m_textBox.Size = new System.Drawing.Size(665, 26);
      this.m_textBox.TabIndex = 1;
      // 
      // m_pvInfoLabel
      // 
      this.m_pvInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_pvInfoLabel.Location = new System.Drawing.Point(12, 15);
      this.m_pvInfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.m_pvInfoLabel.Name = "m_pvInfoLabel";
      this.m_pvInfoLabel.Size = new System.Drawing.Size(666, 29);
      this.m_pvInfoLabel.TabIndex = 2;
      this.m_pvInfoLabel.Text = "PV info";
      this.m_pvInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // LinksToPvChannels_UserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.m_pvInfoLabel);
      this.Controls.Add(this.m_textBox);
      this.Controls.Add(this.m_button);
      this.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "LinksToPvChannels_UserControl";
      this.Size = new System.Drawing.Size(694, 137);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button m_button;
    private System.Windows.Forms.TextBox m_textBox;
    private System.Windows.Forms.Label m_pvInfoLabel;
  }
}

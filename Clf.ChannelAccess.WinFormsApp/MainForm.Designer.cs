
namespace ChannelAccess_WinFormsApp
{
  partial class MainForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_responseLinesListBox = new System.Windows.Forms.ListBox();
      this.m_commandInputTextBox = new System.Windows.Forms.TextBox();
      this.m_enterButton = new System.Windows.Forms.Button();
      this.m_clearButton = new System.Windows.Forms.Button();
      this.m_clearOnEnterCheckBox = new System.Windows.Forms.CheckBox();
      this.linksToPvChannels_UserControl1 = new ChannelAccess_WinFormsApp.LinksToPvChannels_UserControl();
      this.SuspendLayout();
      // 
      // m_responseLinesListBox
      // 
      this.m_responseLinesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_responseLinesListBox.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.m_responseLinesListBox.FormattingEnabled = true;
      this.m_responseLinesListBox.IntegralHeight = false;
      this.m_responseLinesListBox.ItemHeight = 19;
      this.m_responseLinesListBox.Location = new System.Drawing.Point(12, 76);
      this.m_responseLinesListBox.Name = "m_responseLinesListBox";
      this.m_responseLinesListBox.Size = new System.Drawing.Size(1404, 467);
      this.m_responseLinesListBox.TabIndex = 0;
      // 
      // m_commandInputTextBox
      // 
      this.m_commandInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_commandInputTextBox.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.m_commandInputTextBox.Location = new System.Drawing.Point(12, 12);
      this.m_commandInputTextBox.Name = "m_commandInputTextBox";
      this.m_commandInputTextBox.Size = new System.Drawing.Size(1404, 26);
      this.m_commandInputTextBox.TabIndex = 1;
      // 
      // m_enterButton
      // 
      this.m_enterButton.Location = new System.Drawing.Point(12, 44);
      this.m_enterButton.Name = "m_enterButton";
      this.m_enterButton.Size = new System.Drawing.Size(64, 26);
      this.m_enterButton.TabIndex = 2;
      this.m_enterButton.Text = "Enter";
      this.m_enterButton.UseVisualStyleBackColor = true;
      this.m_enterButton.Click += new System.EventHandler(this.m_enterButton_Click);
      // 
      // m_clearButton
      // 
      this.m_clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.m_clearButton.Location = new System.Drawing.Point(12, 560);
      this.m_clearButton.Name = "m_clearButton";
      this.m_clearButton.Size = new System.Drawing.Size(64, 26);
      this.m_clearButton.TabIndex = 2;
      this.m_clearButton.Text = "Clear";
      this.m_clearButton.UseVisualStyleBackColor = true;
      this.m_clearButton.Click += new System.EventHandler(this.m_clearButton_Click);
      // 
      // m_clearOnEnterCheckBox
      // 
      this.m_clearOnEnterCheckBox.AutoSize = true;
      this.m_clearOnEnterCheckBox.Location = new System.Drawing.Point(86, 49);
      this.m_clearOnEnterCheckBox.Name = "m_clearOnEnterCheckBox";
      this.m_clearOnEnterCheckBox.Size = new System.Drawing.Size(100, 19);
      this.m_clearOnEnterCheckBox.TabIndex = 3;
      this.m_clearOnEnterCheckBox.Text = "Clear on Enter";
      this.m_clearOnEnterCheckBox.UseVisualStyleBackColor = true;
      // 
      // linksToPvChannels_UserControl1
      // 
      this.linksToPvChannels_UserControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.linksToPvChannels_UserControl1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.linksToPvChannels_UserControl1.Location = new System.Drawing.Point(14, 604);
      this.linksToPvChannels_UserControl1.Margin = new System.Windows.Forms.Padding(4);
      this.linksToPvChannels_UserControl1.Name = "linksToPvChannels_UserControl1";
      this.linksToPvChannels_UserControl1.Size = new System.Drawing.Size(694, 137);
      this.linksToPvChannels_UserControl1.TabIndex = 6;
      // 
      // MainForm
      // 
      this.AcceptButton = this.m_enterButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1428, 756);
      this.Controls.Add(this.linksToPvChannels_UserControl1);
      this.Controls.Add(this.m_clearOnEnterCheckBox);
      this.Controls.Add(this.m_clearButton);
      this.Controls.Add(this.m_enterButton);
      this.Controls.Add(this.m_commandInputTextBox);
      this.Controls.Add(this.m_responseLinesListBox);
      this.Name = "MainForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.Text = "Channel Access demo (WinForms)";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox m_responseLinesListBox;
    private System.Windows.Forms.TextBox m_commandInputTextBox;
    private System.Windows.Forms.Button m_enterButton;
    private System.Windows.Forms.Button m_clearButton;
    private System.Windows.Forms.CheckBox m_clearOnEnterCheckBox;
    private LinksToPvChannels_UserControl m_linksToPvs_UserControl;
    private LinksToPvChannels_UserControl linksToPvChannels_UserControl1;
  }
}


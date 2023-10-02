using Clf.ChannelAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChannelAccess_WinFormsApp
{

  public partial class MainForm : Form
  {

    private readonly CommandLineInterpreter m_cli ;

    public MainForm ( )
    {
      InitializeComponent() ;
      m_cli = new(
        writeOutputLineAction : AddLine_ThreadSafe
      ) ;
    }

    //
    // Just in case the 'writeOutputLineAction' event comes in on a worker thread,
    // unexpectedly, let's test whether we need an Invoke - otherwise we'll crash.
    // If this is detected, we prefix the line with a '*'.
    //

    private void AddLine_ThreadSafe ( 
      string                                                             line,
      CommandLineInterpreter.TextCategory category_ignored 
    ) {
      if ( InvokeRequired )
      {
        m_responseLinesListBox.Invoke(
          AddLine_ThreadSafe,
          "* " + line,
          category_ignored
        ) ;
      }
      else
      {
        m_responseLinesListBox.Items.Add(line) ;
        m_responseLinesListBox.TopIndex = m_responseLinesListBox.Items.Count - 1 ;
      }
    }

    private async void m_enterButton_Click ( object sender, EventArgs e )
    {
      m_responseLinesListBox.Items.Add(
        CommandLineInterpreter.Prompt 
      + m_commandInputTextBox.Text
      ) ;
      await m_cli.HandleCommandLineCommand(
        m_commandInputTextBox.Text
      ) ;
      if ( m_clearOnEnterCheckBox.Checked )
      {
        m_commandInputTextBox.Text = "" ;
      }
    }

    private void m_clearButton_Click ( object sender, EventArgs e)
    {
      m_responseLinesListBox.Items.Clear() ;
    }

  }

}

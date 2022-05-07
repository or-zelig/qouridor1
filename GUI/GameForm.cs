using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class GameForm : Form
    {
        Board board;
        
        public GameForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.StartPosition=FormStartPosition.CenterScreen;
            this.Text = "Quoridor Game";
            board = new Board(9);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
           numOfBariers.Text = "Player1 -> " +(20 - board.Player1.BarierNum)/2;
           numOfBariers2.Text = "Player2 -> " +(20 - board.Player2.BarierNum)/2;
            if (board.CountClick == 1)
	        {
                buttonA.Enabled = false;
                buttonB.Enabled = false;
                buttonC.Enabled = false;
                buttonD.Enabled = false;
                buttonE.Enabled = false;
	        }
            else
	        {
                buttonA.Enabled = true;
                buttonB.Enabled = true;
                buttonC.Enabled = true;
                buttonD.Enabled = true;
                buttonE.Enabled = true;
	        }
           board.Paint(e.Graphics);
            
        }
        private void TurnOnButtons()
        {
                buttonA.Enabled = true;
                buttonB.Enabled = true;
                buttonC.Enabled = true;
                buttonD.Enabled = true;
                buttonE.Enabled = true;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int res;
            if ( (res=board.Click(e.Location)) !=0 )
            {
                pictureBox1.Invalidate();
                MessageBox.Show("End Game Won " + (res==1 ? "White" : "Red"), "end", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            board.CountClick = 0;
            board = new Board(9);
            TurnOnButtons();
            pictureBox1.Invalidate();
        }

        internal void buttonA_Click(object sender, EventArgs e)
        {
           board.Turn.LastClick = 'A';
           
        }

        internal void buttonB_Click(object sender, EventArgs e)
        {
           board.Turn.LastClick = 'B';
        }

        internal void buttonC_Click(object sender, EventArgs e)
        {
           board.Turn.LastClick = 'C';
        }

        internal void buttonD_Click(object sender, EventArgs e)
        {
           board.Turn.LastClick = 'D';
        }

        internal void buttonE_Click(object sender, EventArgs e)
        {
            if (board.CountClick == 1)
	        {
                string message = "You must put barier";  
                string title = "Alert";  
                MessageBox.Show(message, title);  
	        }
            else
           board.Turn.LastClick = 'E';
        }
    }
}

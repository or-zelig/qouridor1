using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{ 

    internal class Player 
    {
        #region ================== DATA MEMBERS ========
        private  int cellsize =44;
        private  int bariersize1 =40;
        private  int bariersize2 =2;
        private int  place;
        private int[] vitualplace = new int[81];
        private char lastclick;
        private int lastcell;
        private int bariernum = 0;
        private int[] barierlocation =  {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};
        private char[] barierimage = {' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' '};
        private Image image;
        private Image barier;
        private Image path;
        #endregion

        #region ================== ATTRIBUTES ========
        public int CellSize { get =>  cellsize; set => cellsize = value; }
        public int BarierSize1 { get =>  bariersize1; set => bariersize1 = value; }
        public int BarierSize2 { get =>  bariersize2; set => bariersize2 = value; }
        public int Place { get =>  place; set => place = value; }
        public int LastCells { get =>  lastcell; set => lastcell = value; }
        public char LastClick { get =>  lastclick; set => lastclick = value; }
        public int BarierNum { get =>  bariernum; set => bariernum = value; }
        public int[] BarierLocation { get =>  barierlocation; set => barierlocation = value; }
        public char[] BarierImage { get =>  barierimage; set => barierimage = value; }
        public int[] VitualPlace { get =>  vitualplace; set => vitualplace = value; }
        #endregion

        #region ================== CONSTRUCTOR ========
        public Player(bool color, int place)
        {
            LastClick = 'E'; 
            this.place = place;
            image = color ? Properties.Resources.White : Properties.Resources.Red;
            barier = color ? Properties.Resources.Barier : Properties.Resources.Barier2;
            path = Properties.Resources.Black;
        }
        #endregion

        #region ================== EVENTS ========
        internal void Paint(Graphics graphics, int Num)
        {
            int width = 0, lenght = 0, location = 0;
            graphics.DrawImage(image, place% Board.boardSize * CellSize, place/ Board.boardSize * CellSize, CellSize, CellSize); 
            /*for (int i = 0; i < 81; i++)
			{
                graphics.DrawImage(path, vitualplace[i]% 9 * CellSize, vitualplace[i]/ 9 * CellSize, CellSize, CellSize); 
			}*/
            for (int i = 0; i < 40; i++)
			{
                if (BarierImage[i] != ' ')
	            {
                    switch (BarierImage[i])
                    { 
                        case 'A':
                            lenght = BarierSize1;
                            width = BarierSize2;
                            location = BarierLocation[i];
                            break;
                        case 'B':
                            lenght = BarierSize2;
                            width = BarierSize1;
                            location = BarierLocation[i];
                            break;
                        case 'C':
                            lenght = BarierSize1;
                            width = BarierSize2;
                            location = BarierLocation[i] + Board.nextCol;
                            break;
                        case 'D':
                            lenght = BarierSize2;
                            width = BarierSize1;
                            location = BarierLocation[i] + Board.nextRow;
                            break;
                        default:
                            break;
                    }
                     graphics.DrawImage(barier,location% Num * CellSize, location/ Num * CellSize, width, lenght);
                    LastCells = location;
	            }
			}
        }
        #endregion

        #region ================== FUNCTIONS METHODS   =======
        /// update the new place of the player
        internal bool Move(int rowD, int colD, int Turn)
        {  
            place = (ushort)((rowD * Board.boardSize + colD));
            return Win(Turn);
        }
        /// update player data at the new barrier
        internal void PutBarier(int rowD, int colD)
        {
            if (BarierNum < 20)
            { 
                BarierLocation[BarierNum] = (rowD * Board.boardSize + colD);
                BarierImage[BarierNum] = LastClick;
                BarierNum++;
            }
            else
	        {
                    string message = "No more baraiers";  
                    string title = "Alert";  
                    MessageBox.Show(message, title);  
	        }
        }
        /// check if the player win after the movment
        private bool Win(int Turn)
        {
            int temp = Place;
            if (Turn == 1)
	        {
               if (temp/Board.boardSize == 8)
                    return true;
               return false;
	        }
            else
	        {
               if (temp/Board.boardSize == 0)
                    return true;
               return false;
	        }
        }
        /// check if the location is one of is neigbhers
        internal bool Empty(int location)
        {
            return (location) == place +Board.nextCol || (location) == place +Board.previousCol || (location) == place +Board.nextRow || (location) == place +Board.previousRow;
        }
        /// check if the other player doesnt in the location
        internal bool GetOtherLocation (int location)
        {
            if ((location) != place)
               return true;
            else
               return false;
        }
        #endregion
    

        
        


    }
}
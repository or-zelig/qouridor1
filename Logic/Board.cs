using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class Cell
    {
        public Cell [] neighbors ;
        public int row,col, dist = 0;
        public bool Left = false; 
        public bool Right = false; 
        public bool Up = false; 
        public bool Down = false; 
        public bool Player = false; 

        public Cell(int row, int col)
        {
            neighbors = new Cell[4]; // Left Right Up Down
            this.row = row;
            this.col = col;
        }

    }

    class Board
    {
        #region ================== DATA MEMBERS ========
        public const int boardSize = 9;
        public const int nextRow = 9;
        public const int previousRow = -9;
        public const int nextCol = 1;
        public const int previousCol = -1;
        public const int nextRowInMatrix = 1;
        public const int previousRowInMatrix = -1;
        public const int amountOfneighbers = 4;
        private static Player player1, player2;
        private static Player turn;
        private static int countclick = 0;
        private static Cell[,] boardstatues;
        private static bool humanplayer = false;
        #endregion
        
        #region ================== ATTRIBUTES ========
        public Player Player1 { get =>  player1; set => player1 = value; }
        public Player Player2 { get =>  player2; set => player2 = value; }
        public Player Turn  { get => turn  ; set => turn = value; }
        public int CountClick { get =>  countclick; set => countclick = value; }
        public Cell[,] BoardStatues { get => boardstatues; set => boardstatues = value; }
        #endregion
        
        #region ================== CONSTRUCTOR ========
        public Board(int n)
        {
            int Num = n;
            player1 = new Player(true, Num-5);
            player2 = new Player(false, Num*Num-5);
            turn = player1;
            BoardStatues = new Cell[Num,Num] ;
            for (int i = 0; i <  Num; i++)
			{
                for (int j = 0; j < Num; j++)
			         BoardStatues[i,j] = new Cell(i,j);
			}
            BoardStatues[0,4].Player = true;
            BoardStatues[8,4].Player = true;
            BuildGraph();
            
        }        
        #endregion

        #region ================== EVENTS ========
        /// Draws the players and there barriers on the board
        internal void Paint(Graphics graphics)
        {
            Player1.Paint(graphics,boardSize);
            Player2.Paint(graphics,boardSize);
         }
        /// The function receives the action selected by the human player
        /// if the action is Legal then the function change the place of the human player
        /// or put the barrier, else the human need to pick another action
        internal int Click(Point Getlocation)
        {
            int row = Getlocation.Y / Player1.CellSize;
            int col = Getlocation.X /Player1.CellSize;
            int location = row*boardSize +col;
            int player = 1;
            
           if (CountClick == 0 && Turn.LastClick == 'E' && ( Empty(location) || Empty_Speical(location)) && ThroughWalls(location))
           {
                humanplayer = true;
                BoardStatues[Turn.Place/boardSize,Turn.Place%boardSize].Player = false;
                if (Player1.Move(row, col,player))
                        return  1;
                BoardStatues[row,col].Player = true;
                
           }
           else
                humanplayer = false;
            if (turn.LastClick != 'E')
	        {
               if (Empty_barier(row,col,Player1) )
               { 
                    Locate_Barier(player1.LastClick,player1,row,col);

                    if (CountClick == 1)
                    { 
                    turn.LastClick = 'E';
                    CountClick = 0;
                    }
                    else
                        CountClick++;
               }
	        }
            if (Turn == Player1 && (countclick == 1 || turn.LastClick == 'E') && humanplayer)
            { 
              if (Player2Move())
                return 2;
            }
            return 0;
        }
        #endregion

        #region ================== FUNCTIONS METHODS   =======
        /// check if the place exist in the board
        private bool IsLegal(int row, int col)
        {
            return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
        }
        /// make connections between the cells to build the graph
        private void BuildGraph()
        {
            for (int i = 0; i <  boardSize; i++)
			{
                for (int j = 0; j <  boardSize; j++)
                { 
                    BoardStatues[i,j].neighbors[0] = IsLegal(i, j + previousCol)        ?  BoardStatues[i,j + previousCol] : null; // Left
                    BoardStatues[i,j].neighbors[1] = IsLegal(i, j + nextCol)            ?  BoardStatues[i, j + nextCol] : null;
                    BoardStatues[i,j].neighbors[2] = IsLegal(i + previousRowInMatrix,j) ?  BoardStatues[i + previousRowInMatrix,j] : null;
                    BoardStatues[i,j].neighbors[3] = IsLegal(i + nextRowInMatrix,j)     ?  BoardStatues[i + nextRowInMatrix,j] : null;
                }
            }
        }
        /// throw an error massage only for the human player
        /// and delete the first located barrier
        private void Throw_Message_delete_first_barrier(Player player)
        {
            string message = "Invalid Location";  
            string title = "Alert";
            if (player == player1 )
            { 

                MessageBox.Show(message, title);  
                if (CountClick == 1)
                { 
                    int row = player1.BarierLocation[player1.BarierNum -1] /boardSize;
                    int col = player1.BarierLocation[player1.BarierNum -1] %boardSize;
                    DeleteBarier(row,col,player1.LastClick,player1);
                    countclick = 0;
                    player1.LastClick = 'E';
                    humanplayer = false;
                }
            }

        }
        /// checks if there is no player in this location 
        private bool Empty(int location)
        {
            if (turn == player1)
            return Player1.Empty(location) && Player2.GetOtherLocation(location);
            else 
            return Player2.Empty(location) && Player1.GetOtherLocation(location);

        }
        /// check if the human player can jump two cells because 
        /// the Computerized player is next to him
        private bool Empty_Speical(int location)
        {

            int move = location;
            int play1 = Player1.Place;
            int play2 = Player2.Place;
            int row = play2/boardSize;
            int col = play2%boardSize;
            
            /// Check whether the player's jump is two cells from the current position (18 = two rows, 2 = two cols)
            if (play1 == play2 + previousRow && move == play1 + 18 && !BoardStatues[row,col].Down)
                return true;
            if (play1 == play2 + nextRow && move == play1 -18 && !BoardStatues[row,col].Up)
                return true;
            if (play1 == play2 + previousCol && move == play1 + 2 && !BoardStatues[row,col].Right)
                return true;
            if (play1 == play2 + nextCol && move == play1 -2 && !BoardStatues[row,col].Left)
                return true;
            
            return false;

        }
        /// check if the player movment go thruogh walls          
        private bool ThroughWalls(int location)
        { 
            int dif = location - player1.Place;
            int row = location/boardSize;
            int col = location%boardSize;
            bool result = true;
            
            switch (dif)
	        {
                case nextCol:
                    if (BoardStatues[row,col].Left)
                        result =  false;
                    break;
                case previousCol:
                    if (BoardStatues[row,col].Right)
                        result =  false;
                    break;
                case nextRow:
                    if (BoardStatues[row,col].Up)
                        result =  false;
                    break;
                case previousRow:
                    if (BoardStatues[row,col].Down)
                        result =  false;
                    break;
		        default:
                    break;
	        }
            
            return result;
            
        }
        /// check if its possible to put in this location barrier
        private bool Empty_barier(int row, int col, Player Turn)
        {
            bool result = true;
            int CurrentCell = row*boardSize + col;

            // In case that the barrier should be placed out of the edge return false
            if (row < 0 || row > 8 || col < 0 || col > 8)
                return false;
            
            switch (Turn.LastClick)
            { 
                  case 'A':
                    if (BoardStatues[row,col].Left)
                        result = false;

                    else if (CountClick != 1)
                    {   // if it is the first barrier and there is not place for the second one       
                        if      (row == 0 && BoardStatues[row+nextRowInMatrix,col].Left )
                                        result = false;
                        else if (row == 8 && BoardStatues[row+previousRowInMatrix,col].Left)
                                        result = false;
                        else if (row != 0 && row != 8 && BoardStatues[row+previousRowInMatrix,col].Left && BoardStatues[row+nextRowInMatrix,col].Left)
                                        result = false;
                    }
                    else
                    { 
                        
                        if      (row == 0 && Turn.LastCells != CurrentCell + nextRow)
                                    result = false;
                        else if (row == 8 && Turn.LastCells != CurrentCell + previousRow)
                                    result = false;
                        else
	                    {
                                if (Turn.LastCells != CurrentCell + nextRow && Turn.LastCells != CurrentCell + previousRow )
                                    result = false;
                                // check if the second barrier dont make a cross in a diffrent bariier
                                else if (BoardStatues[row,col+previousCol].Down && BoardStatues[row,col].Down && BoardStatues[row+nextRowInMatrix,col].Left)
                                    result = false;
                                else if (BoardStatues[row,col].Up && BoardStatues[row,col+previousCol].Up && BoardStatues[row+previousRowInMatrix,col].Left)
                                    result = false;

	                    } 
                    }
                        break;
                  case 'B':
                    if (BoardStatues[row,col].Up)
                        result = false;
                    else if (countclick != 1)
                    { 
                        if (col == 0 && BoardStatues[row,col+nextCol].Up)
                                        result = false;
                        else if (col == 8 && BoardStatues[row,col+previousCol].Up)
                                        result = false;
                        else if (col != 0 && col != 8 && BoardStatues[row,col+previousCol].Up && BoardStatues[row,col+nextCol].Up)
                                        result = false;
                    }
                    else
                    { 
                           if (col == 0 && Turn.LastCells != CurrentCell +nextCol)
                                    result = false;
                           else if (col == 8 && Turn.LastCells != CurrentCell +previousCol)
                                    result = false;
                           else
	                       {
                                if (Turn.LastCells != CurrentCell +nextCol && Turn.LastCells != CurrentCell +previousCol )
                                    result = false;
                                else if (BoardStatues[row+previousRowInMatrix,col].Right && BoardStatues[row,col].Right && BoardStatues[row,col+nextCol].Up)
                                    result = false;
                                else if (BoardStatues[row,col].Left && BoardStatues[row+previousRowInMatrix,col].Left && BoardStatues[row,col+previousCol].Up)
                                    result = false;
	                       }
                        
                    }
                        break;
                  case 'C':
                    if (BoardStatues[row,col].Right)
                        result = false;
                    else if (CountClick != 1)
                    { 
                        if (row == 0 && BoardStatues[row+nextRowInMatrix,col].Right)
                                        result = false;
                        else if (row == 8 && BoardStatues[row+previousRowInMatrix,col].Right)
                                        result = false;
                        else if (row != 0 && row != 8 && BoardStatues[row+previousRowInMatrix,col].Right && BoardStatues[row+nextRowInMatrix,col].Right)
                                        result = false;
                    }
                    else
                    { 
                           if (row == 0 && Turn.LastCells+previousCol != CurrentCell +nextRow)
                                    result = false;
                           else if (row == 8 && Turn.LastCells+previousCol != CurrentCell +previousRow)
                                    result = false;
                           else
	                       {
                                if (Turn.LastCells+previousCol != CurrentCell +nextRow && Turn.LastCells+previousCol != CurrentCell +previousRow )
                                    result = false;
                                else if (BoardStatues[row,col].Up && BoardStatues[row,col+nextCol].Up && BoardStatues[row+previousRowInMatrix,col].Right)
                                    result = false;
                                else if (BoardStatues[row,col].Down && BoardStatues[row,col+nextCol].Down && BoardStatues[row+nextRowInMatrix,col].Right)
                                    result = false;
	                       }
                        
                    } 
                        break;
                  case 'D':
                    if (BoardStatues[row,col].Down)
                                    result = false;
                    else if (countclick != 1)
                    { 
                        if (col == 0 && BoardStatues[row,col+nextCol].Down)
                                        result = false;
                        else if (col == 8 && BoardStatues[row,col+previousCol].Down)
                                        result = false;
                        else if (col != 0 && col != 8 && BoardStatues[row,col+previousCol].Down && BoardStatues[row,col+nextCol].Down)
                                        result = false;
                    }
                    else
                    { 
                           if (col == 0 && Turn.LastCells+previousRow != CurrentCell +nextCol)
                                    result = false;
                           else if (col == 8 && Turn.LastCells+previousRow != CurrentCell +previousCol)
                                    result = false;
                           else
	                       {
                                if (Turn.LastCells+previousRow != CurrentCell +nextCol && Turn.LastCells+previousRow != CurrentCell +previousCol )
                                    result = false;
                                else if (BoardStatues[row,col].Right && BoardStatues[row+nextRowInMatrix,col].Right && BoardStatues[row,col+nextCol].Down)
                                    result = false;
                                else if (BoardStatues[row,col].Left && BoardStatues[row+nextRowInMatrix,col].Left && BoardStatues[row,col+previousCol].Down)
                                    result = false;
                                
	                       }
                    }
                        break;
                  default:
                        break;
            }

            if (!result && Turn == player1)
                Throw_Message_delete_first_barrier(player1); 
	  
            return result;
        }
        /// put the barrier in the board if it isnt block any player
        private bool Locate_Barier(char Click,Player Turn,int row,int col)
        {
            bool result = true;
            int row_player1 = player1.Place/boardSize;
            int col_player1 = player1.Place%boardSize;
            int row_player2 = player2.Place/boardSize;
            int col_player2 = player2.Place%boardSize;
            Turn.LastClick = Click;
            switch (Click)
            { 
                        case 'A':
                            // upadte all the stuff that happend if this barrier will be locate
                            BoardStatues[row,col].Left = true;
                            BoardStatues[row,col+previousCol].Right = true;
                            BoardStatues[row,col].neighbors[0] = null;
                            BoardStatues[row,col+previousCol].neighbors[1] = null;
                            // check if the barrier cause block to one player
                            if (!BFS(BoardStatues[row_player1,col_player1],player1) || !BFS(BoardStatues[row_player2,col_player2],player2))
	                        {
                             //if its true, we bring back all the Connections that we cancel on the start
                             BoardStatues[row,col].Left = false;
                             BoardStatues[row,col+previousCol].Right = false;
                             BoardStatues[row,col].neighbors[0] = BoardStatues[row,col+previousCol];
                             BoardStatues[row,col+previousCol].neighbors[1] = BoardStatues[row,col];
                             Barrier_Cant_Be_Located(Turn);
                             result = false;
	                        }
                            else
                                Barrier_Can_Be_Located(Turn,row,col);
                            break;
                        case 'B':
                            BoardStatues[row,col].Up = true;
                            BoardStatues[row+previousRowInMatrix,col].Down = true;
                            BoardStatues[row,col].neighbors[2] = null;
                            BoardStatues[row+previousRowInMatrix,col].neighbors[3] = null;
                            if (!BFS(BoardStatues[row_player1,col_player1],player1) || !BFS(BoardStatues[row_player2,col_player2],player2))
	                        {
                             BoardStatues[row,col].Up = false;
                             BoardStatues[row+previousRowInMatrix,col].Down = false;
                             BoardStatues[row,col].neighbors[2] = BoardStatues[row+previousRowInMatrix,col];
                             BoardStatues[row+previousRowInMatrix,col].neighbors[3] = BoardStatues[row,col];
                             Barrier_Cant_Be_Located(Turn);
                             result = false;
	                        }
                            else
                                Barrier_Can_Be_Located(Turn,row,col);
                            break;
                        case 'C':
                            BoardStatues[row,col].Right = true;
                            BoardStatues[row,col+nextCol].Left = true;
                            BoardStatues[row,col].neighbors[1] = null;
                            BoardStatues[row,col+nextCol].neighbors[0] = null;
                            if (!BFS(BoardStatues[row_player1,col_player1],player1) || !BFS(BoardStatues[row_player2,col_player2],player2))
	                        {
                             BoardStatues[row,col].Right = false;
                             BoardStatues[row,col+nextCol].Left = false;
                             BoardStatues[row,col].neighbors[1] = BoardStatues[row,col+nextCol];
                             BoardStatues[row,col+nextCol].neighbors[0] = BoardStatues[row,col];
                             Barrier_Cant_Be_Located(Turn);
                             result = false;
	                        }
                            else
                                Barrier_Can_Be_Located(Turn,row,col);
                            break;
                        case 'D':
                            BoardStatues[row,col].Down = true;
                            BoardStatues[row+nextRowInMatrix,col].Up = true;
                            BoardStatues[row,col].neighbors[3] = null;
                            BoardStatues[row+nextRowInMatrix,col].neighbors[2] = null;
                            if (!BFS(BoardStatues[row_player1,col_player1],player1)|| !BFS(BoardStatues[row_player2,col_player2],player2))
	                        {
                             BoardStatues[row,col].Down = false;
                             BoardStatues[row+nextRowInMatrix,col].Up = false;
                             BoardStatues[row,col].neighbors[3] = BoardStatues[row+nextRowInMatrix,col];
                             BoardStatues[row+nextRowInMatrix,col].neighbors[2] = BoardStatues[row,col];
                             Barrier_Cant_Be_Located(Turn);
                             result = false;
	                        }
                            else
                                Barrier_Can_Be_Located(Turn,row,col);
                            break;
                        default:
                            result = false;
                            break;
            }
            return result;
        }
        /// delete located barrier if first barrier was located
        public void Barrier_Cant_Be_Located(Player Turn)
        {
            if (CountClick == 1 && Turn == player1)
            {
                Turn.BarierNum--;
                Turn.BarierLocation[Turn.BarierNum] = -1;
                Turn.BarierImage[Turn.BarierNum] = ' ';
            }
            Throw_Message_delete_first_barrier(Turn);
            CountClick = -1;
        }
        /// locate second barrier
        public void Barrier_Can_Be_Located(Player Turn,int row,int col)
        {
            if (countclick == 1)
                humanplayer = true;
            Turn.PutBarier(row, col);
        }
        /// chek if the player can get to the finish row
        public bool BFS(Cell start,Player player)
        {
            var visited = new HashSet<Cell>();

           // if (!graph.AdjacencyList.ContainsKey(start))
           //     return visited;
                
            Queue<Cell> queue = new Queue<Cell>();
            queue.Enqueue(start);

            while (queue.Count > 0) 
            {
                Cell vertex = queue.Dequeue();

                if (visited.Contains(vertex))
                    continue;

                visited.Add(vertex);
                
                if (player == player1 && vertex.row == 8)  
                    return true;
                if (player == player2 && vertex.row == 0)
                    return true;

                // foreach(Cell neighbor in vertex.neighbors)
                    for (int i = 0; i < amountOfneighbers; i++)
			        {
                      Cell neighbor = vertex.neighbors[i];
                       if (neighbor != null && !visited.Contains(neighbor))
                       { 
                            switch (i)
                            {
                                    case 0:
                                      if (!vertex.Left)
                                        queue.Enqueue(neighbor);
                                    break;
                                    case 1:
                                       if (!vertex.Right)
                                        queue.Enqueue(neighbor);
                                    break;
                                    case 2:
                                    if (!vertex.Up)
                                        queue.Enqueue(neighbor);
                                    break;
                                    case 3:
                                    if (!vertex.Down)
                                        queue.Enqueue(neighbor);
                                    break;
                            }
                        
                       }
                    }
            }

            return false;
        }
        /// put the distance from the start in every cell until we find the last cell and return it
        public Cell FindTheLastCell(Cell start,Player player)
        {
            var visited = new HashSet<Cell>();
            Cell nothing = null;

           // if (!graph.AdjacencyList.ContainsKey(start))
           //     return visited;
                
            Queue<Cell> queue = new Queue<Cell>();
            queue.Enqueue(start);

            while (queue.Count > 0) 
            {
                Cell vertex = queue.Dequeue();

                if (visited.Contains(vertex))
                    continue;

                visited.Add(vertex);
                
                if (player == player1 && vertex.row == 8)  
                    return vertex;
                if (player == player2 && vertex.row == 0)
                    return vertex;

                // foreach(Cell neighbor in vertex.neighbors)
                    for (int i = 0; i < amountOfneighbers; i++)
			        {
                      Cell neighbor = vertex.neighbors[i];
                       if (neighbor != null && !visited.Contains(neighbor))
                       { 
                            switch (i)
                            {
                                    case 0:
                                        if (!vertex.Left)
                                            neighbor.dist = vertex.dist + 1;
                                        queue.Enqueue(neighbor);
                                        break;
                                    case 1:
                                        if (!vertex.Right)
                                            neighbor.dist = vertex.dist + 1;
                                        queue.Enqueue(neighbor);
                                        break;
                                    case 2:
                                        if (!vertex.Up)
                                            neighbor.dist = vertex.dist + 1;
                                        queue.Enqueue(neighbor);
                                        break;
                                    case 3:
                                        if (!vertex.Down)
                                            neighbor.dist = vertex.dist + 1;
                                        queue.Enqueue(neighbor);
                                        break;
                            }
                        
                       }
                    }
            }

            return nothing;
        }
        /// finds the fastest way to win and return the number of steps
        public int Simulator(Cell start, Player turn)
        {
            Cell vertex = FindTheLastCell(start,turn);
            int maxdist = vertex.dist;
            MakeVirtualTrack(vertex,turn); 
            ResetBoard();   
            for (int i = 1; i < maxdist; i++)
			{
                int row = turn.VitualPlace[i]/boardSize;
                int col = turn.VitualPlace[i]%boardSize;
                if (BoardStatues[row,col].Player)
                {
                    maxdist--;
                    break;
                }
			}
            return maxdist;
        }
        /// delete the virtual barrier 
        public void DeleteBarier(int row,int col,char tav,Player player)
        {
            switch (tav)
                    { 
                        case 'A':
                            // bring back the connetions that been cancel
                            BoardStatues[row,col].Left = false;
                            BoardStatues[row,col+previousCol].Right = false;
                            BoardStatues[row,col].neighbors[0] = BoardStatues[row,col+previousCol];
                            BoardStatues[row,col+previousCol].neighbors[1] = BoardStatues[row,col];
                            player.BarierNum--;
                            player.BarierLocation[player.BarierNum] = -1;
                            player.BarierImage[player.BarierNum] = ' ';
                            break;
                        case 'B':
                            BoardStatues[row,col].Up = false;
                            BoardStatues[row+previousRowInMatrix,col].Down = false;
                            BoardStatues[row,col].neighbors[2] = BoardStatues[row+previousRowInMatrix,col];
                            BoardStatues[row+previousRowInMatrix,col].neighbors[3] = BoardStatues[row,col];
                            player.BarierNum--;
                            player.BarierLocation[player.BarierNum] = -1;
                            player.BarierImage[player.BarierNum] = ' ';
                            break;
                        case 'C':
                            BoardStatues[row,col].Right = false;
                            BoardStatues[row,col+nextCol].Left = false;
                            BoardStatues[row,col].neighbors[1] = BoardStatues[row,col+nextCol];
                            BoardStatues[row,col+nextCol].neighbors[0] = BoardStatues[row,col];
                            player.BarierNum--;
                            player.BarierLocation[player.BarierNum] = -1;
                            player.BarierImage[player.BarierNum] = ' ';
                            break;
                        case 'D':
                            BoardStatues[row,col].Down = false;
                            BoardStatues[row+nextRowInMatrix,col].Up = false;
                            BoardStatues[row,col].neighbors[3] = BoardStatues[row+nextRowInMatrix,col];
                            BoardStatues[row+nextRowInMatrix,col].neighbors[2] = BoardStatues[row,col];
                            player.BarierNum--;
                            player.BarierLocation[player.BarierNum] = -1;
                            player.BarierImage[player.BarierNum] = ' ';
                            break;
                        default:
                            break;
            }
        }
        /// check if there barrier which causes
        /// Computerized player be more close to win then human player
        public bool Player2Barier(int simulator1, int firstdiff)
        {
            int humanrow,humancol,secondrow,secondcol; 
            char tav = 'A';
            bool done = false,foundstep = false;   
            int maxdiff = firstdiff, diffbetweenplayers,bestrow1 = -2,bestcol1 = -2,bestrow2 = -2,bestcol2 = -2;
            char besttav = 'B';
                
            //while (moresteps)
	       // {

	
                for (int i = simulator1; i >= 1; i--)
			    {
                    // in each move we need to get the barrier direction
                    if (player1.VitualPlace[i] - player1.VitualPlace[i-1] == 1)
                     tav = 'A';
                    else if (player1.VitualPlace[i] - player1.VitualPlace[i-1] == -1)
                     tav = 'C';
                    else if (player1.VitualPlace[i] - player1.VitualPlace[i-1]== 9)
                     tav = 'B';
                    else if (player1.VitualPlace[i] - player1.VitualPlace[i-1] == -9)
                     tav = 'D';
                    player2.LastClick = tav;
                    humanrow = player1.VitualPlace[i]/boardSize;
                    humancol = player1.VitualPlace[i]%boardSize;
                    secondrow = humanrow;
                    secondcol = humancol;
                  
			        // if there is not a barrier in the location and its a ligel place, move on,
                    // else move to the next step ot the player
                    if (Empty_barier(humanrow,humancol,player2) && player2.BarierNum < 20)
                    { 
                      bool result = Locate_Barier(tav,player2,humanrow,humancol);
                      if ( result)
                      {
                        /// if the barrier dont block any player, try to put the next barrier
                        player2.LastCells = humanrow*boardSize +humancol;
                        countclick = 1;
                        
                        switch (tav)
                        {
                         case 'A': 
                            if (Empty_barier(humanrow+nextRowInMatrix,humancol,player2))
                            {
                                secondrow = humanrow+1; 
                                if (Locate_Barier(tav,player2,humanrow+nextRowInMatrix,humancol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                        
                            else if (Empty_barier(humanrow+previousRowInMatrix,humancol,player2))
                            {
                                secondrow = humanrow+previousRowInMatrix;
                                if (Locate_Barier(tav,player2,humanrow+previousRowInMatrix,humancol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else
                                DeleteBarier(humanrow,humancol,tav,player2);
                               
                            break;
                         case 'B': 
                            if (Empty_barier(humanrow,humancol+nextCol,player2))
                            {
                                secondcol = humancol+nextCol;
                                if (Locate_Barier(tav,player2,humanrow,humancol+nextCol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);       
                            }
                                                     
                            else if (Empty_barier(humanrow,humancol+previousCol,player2))
                            {
                                secondcol = humancol+previousCol;
                                if (Locate_Barier(tav,player2,humanrow,humancol+previousCol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else
                                DeleteBarier(humanrow,humancol,tav,player2);
                               
                            break;
                            case 'C':
                            if (Empty_barier(humanrow+nextRowInMatrix,humancol,player2))
                            {
                                secondrow=humanrow+nextRowInMatrix;
                                if (Locate_Barier(tav,player2,humanrow+nextRowInMatrix,humancol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }                            
                        
                            else if (Empty_barier(humanrow+previousRowInMatrix,humancol,player2))
                            {
                                secondrow = humanrow+previousRowInMatrix;
                                if (Locate_Barier(tav,player2,humanrow+previousRowInMatrix,humancol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else
                                DeleteBarier(humanrow,humancol,tav,player2);
                               
                            break;
                            case 'D':
                            if (Empty_barier(humanrow,humancol+nextCol,player2))
                            {
                                secondcol=humancol+nextCol;
                                if (Locate_Barier(tav,player2,humanrow,humancol+nextCol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }                            
                        
                            else if (Empty_barier(humanrow,humancol+previousCol,player2))
                            {
                                secondcol=humancol+previousCol;
                                if (Locate_Barier(tav,player2,humanrow,humancol+previousCol))
                                     done = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else
                                DeleteBarier(humanrow,humancol,tav,player2);   
                            break;
                            default:
                            break;
                        }
                         diffbetweenplayers = Simulator(BoardStatues[player1.Place/boardSize,player1.Place%boardSize], player1)
                        - Simulator(BoardStatues[player2.Place/boardSize,player2.Place%boardSize], player2);
                        /// if the barriers we put are make more diffrent from the max diffrent
                        /// save the barriers
                        if (diffbetweenplayers > maxdiff)
                        { 
                            bestrow1 = humanrow;
                            bestcol1 = humancol;
                            bestrow2 = secondrow;
                            bestcol2 = secondcol;
                            besttav = tav;
                            foundstep = true;
                        }
                        if (done)
                        {
                            DeleteBarier(humanrow,humancol,tav,player2);
                            DeleteBarier(secondrow,secondcol,tav,player2);
                            done = false;
                        }
                      }
                    }

                }
                /// if in the end of the for we found a barrier at all, locate them 
                if (foundstep)
                {
                    Locate_Barier(besttav,player2,bestrow1,bestcol1);
                    Locate_Barier(besttav,player2,bestrow2,bestcol2);
                    return true;
                }
                return false;

               /* else
                {
                    Simulator(BoardStatues[player2.Place/9,player2.Place%9], player2);
                    BoardStatues[player2.Place/9,player2.Place%9].Player = false;
                    int computerrow = player2.VitualPlace[1]/9;
                    int computercol = player2.VitualPlace[1]%9;
                    BoardStatues[computerrow ,computercol].Player = true;
                    bool status = player2.Move(computerrow,computercol, 2); 
                } */
          //  }
          //  return false;
        }
        /// AI
        public bool Player2Move()
        {
            int computerrow,computercol,diff,simulator1,simulator2,humanrow,humancol;
            char tav = 'D';
            bool makeAmove = false;
            simulator1 = Simulator(BoardStatues[player1.Place/boardSize,player1.Place%boardSize], player1) ;
            simulator2 = Simulator(BoardStatues[player2.Place/boardSize,player2.Place%boardSize], player2);
            diff = simulator1  - simulator2;
            /// if the human player is closer to win,
            /// we check if there a barrier that can short the diff between them
            if  (diff <= 0)
            {
               if (Player2Barier(simulator1,diff))
               {
                    countclick = 0;
                    makeAmove = true;
               }
                
            }
            /// if we dont found a barrier
            if (!makeAmove)
	        {
                humanrow = player1.Place/boardSize;
                humancol =player1.Place%boardSize;
                bool blocktrack = false;
                /// if there is more barriers to the human player,
                /// we try to block the fast track,
                /// else make a move in the shortest way of the Computerized player 
                if (Player1.BarierNum < 20)
                {
                    if (FastTrack(simulator1))
                    {
                        Locate_Barier(tav,player2,humanrow,humancol);
                        player2.LastCells = humanrow*boardSize +humancol + boardSize;
                        CountClick = 1;
                        if (BoardStatues[humanrow,humancol].Right && BoardStatues[humanrow,humancol].Left)
                        {
                            if (Empty_barier(humanrow,humancol+nextCol,player2))
                            {
                                
                                if (Locate_Barier(tav,player2,humanrow,humancol+nextCol))
                                     blocktrack = true;
                                else
                                     DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else if (Empty_barier(humanrow+previousRowInMatrix,humancol,player2))
                            {
                                    if (Locate_Barier(tav,player2,humanrow+previousRowInMatrix,humancol))
                                         blocktrack = true;
                                    else
                                         DeleteBarier(humanrow,humancol,tav,player2);
                            }
                            else
                                DeleteBarier(humanrow,humancol,tav,player2);
                        }
                        else if (BoardStatues[humanrow,humancol].Right && !BoardStatues[humanrow,humancol].Left)
                        {
                             if (Empty_barier(humanrow,humancol+previousCol,player2))
                             {
                                    Locate_Barier(tav,player2,humanrow,humancol+previousCol);
                                    blocktrack = true;
                             }
                             else
                                    DeleteBarier(humanrow,humancol,tav,player2);
                        }
                        else if (!BoardStatues[humanrow,humancol].Right && BoardStatues[humanrow,humancol].Left)
                        {
                             if (Empty_barier(humanrow,humancol+nextCol,player2))
                             {
                                    Locate_Barier(tav,player2,humanrow,humancol+nextCol);
                                    blocktrack = true;
                             }
                             else
                                    DeleteBarier(humanrow,humancol,tav,player2);
                        }
                    }
                } 
                /// we enter this condition if the human player doesnt have any mor barriers,
                /// or the human player dont have a fast track
                /// or we cant block the fast track
                if (!blocktrack)
                { 
                        Simulator(BoardStatues[player2.Place/boardSize,player2.Place%boardSize], player2);
                        countclick = 0;
                        computerrow = player2.VitualPlace[1]/boardSize;
                        computercol = player2.VitualPlace[1]%boardSize;
                        BoardStatues[player2.Place/boardSize,player2.Place%boardSize].Player = false;
                        if (BoardStatues[computerrow,computercol].Player)
                        {
                            computerrow = player2.VitualPlace[2]/boardSize;
                            computercol = player2.VitualPlace[2]%boardSize;
                        }
                        BoardStatues[computerrow ,computercol].Player = true;
                        return player2.Move(computerrow,computercol, 2); 
                }
            }
             
	        countclick = 0;
            player1.LastClick = 'E';
            return false;
               
                
        }
        /// return true if the human player have fast track
        public bool FastTrack(int tracksteps)
        {
            int i = 0;
            int minimuOfBariers = 4;
            
            if ((tracksteps == 8 - player1.Place / boardSize) && (player1.Place / boardSize != 0) )
            {
                while (i < minimuOfBariers)
                {
                    if (!CheckWidght(player1.VitualPlace[i]))
                        return false;
                    i++;
                }
                return true;
            }
            else
                return false;
        }
        /// return true if in this location there is barrier  
        public bool CheckWidght(int virtuallocation)
        {
            int row = virtuallocation/boardSize;
            int col = virtuallocation%boardSize;
            bool result = false;
            switch (col)
            {
                case 0:
                if (boardstatues[row,col+nextCol].Right || boardstatues[row,col].Right)
                    result = true;
                else
                    result = false;
                break;
                case 1:
                if (boardstatues[row,col].Right || (boardstatues[row,col].Right && boardstatues[row,col].Left))
                    result = true;
                else
                    result = false;
                break;
                case 7:
                if (boardstatues[row,col].Left || (boardstatues[row,col].Right && boardstatues[row,col].Left))
                    result = true;
                else
                    result = false;
                break;
                case 8:
                if (boardstatues[row,col+previousCol].Left || boardstatues[row,col].Left)
                    result = true;
                else
                    result = false;
                break;
                default:

                if (boardstatues[row,col].Right && boardstatues[row,col].Left)
                    result = true;
                else if (boardstatues[row,col].Right && boardstatues[row,col+previousCol].Left)
                    result = true;
                else if (boardstatues[row,col+nextCol].Right && boardstatues[row,col].Left)
                    result = true;
                else
                    result = false;
                break;
            }
            return result;
        }
        /// After we used the function FindTheLastCell, we need to reset all cell's dist to 0
        /// because we use FindTheLastCell many time 
        public void ResetBoard()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
			    {
                    BoardStatues[i, j].dist = 0;
                }
		    }
        }
        /// we go over the board and searching for the shortest track
        public void MakeVirtualTrack(Cell vertex,Player turn)
        {
            int dist = vertex.dist;
            int maxdist = vertex.dist;
            turn.VitualPlace[maxdist-1] = vertex.row*boardSize+vertex.col;
            while (dist >= 0)
            { 
                if (dist == 0)
                {
                    turn.VitualPlace[dist] = vertex.row*boardSize+vertex.col;
                    break;
                }

                    for (int i = 0; i < amountOfneighbers; i++)
			        {

                        if  (vertex.neighbors[i] != null && vertex.neighbors[i].dist == dist -1)
                        {
                            int neighbors_row = vertex.neighbors[i].row;
                            int neighbors_col = vertex.neighbors[i].col;
                            
                            turn.VitualPlace[dist] = vertex.row*boardSize+vertex.col;
                            vertex = vertex.neighbors[i];
                                
                            dist = vertex.dist;
                            break;
                        }
                    }
                        
            }
        }
        #endregion

        

        
    }
}


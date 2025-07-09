using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class GameState
    {
        public Player[,] GameGrid {  get; private set; } // private set; keep the property value safe, so only the class decides when it changes.
        public Player CurrentPlayer { get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        //event - When the event happens (is triggered / raised), it automatically calls all the subscriber methods, one by one.
        public event Action<int, int> MoveMade;
        public event Action<GameResult> GameEnded;
        public event Action GameRestarted;

        public GameState()
        {
            GameGrid =  new Player[3,3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
        }

        private bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r,c] == Player.None;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        private void SwitchPlayer()
        {
            if (CurrentPlayer == Player.X)
            {
                CurrentPlayer = Player.O;
            }
            else
            {
                CurrentPlayer = Player.X;
            }
        }

        //Return true if all of these squares belong to the given player; otherwise false.
        // An array of tuples, where each tuple holds two int values.
        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach ((int r, int c) in squares)
            {
                if (GameGrid[r,c] != player)
                {
                    return false;
                }
            }

            return true;
        }

        //out is a way to let a method give back more than one value.
        private bool DidMoveWin(int r, int c, out WinInfo wininfo)
        {
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] col = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] leftToRightDiag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] rightToLeftDiag = new[] { (2, 0), (1, 1), (0, 2) };

            if (AreSquaresMarked(row, CurrentPlayer))
            {
                wininfo = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }

            if (AreSquaresMarked(col, CurrentPlayer))
            {
                wininfo = new WinInfo { Type = WinType.Column, Number = c };
                return true;
            }

            if (AreSquaresMarked(leftToRightDiag, CurrentPlayer))
            {
                wininfo = new WinInfo { Type = WinType.LeftToRightDiagonal};
                return true;
            }

            if (AreSquaresMarked(rightToLeftDiag, CurrentPlayer))
            {
                wininfo = new WinInfo { Type = WinType.RighttoLeftDiagonal};
                return true;
            }

            wininfo = null;
            return false;
        }

        private bool DidMoveEndGame(int r, int c, out GameResult result)
        {
            if (DidMoveWin(r, c, out WinInfo wininfo))
            {
                result = new GameResult { Winner = CurrentPlayer, WinInfo = wininfo };
                return true;
            }

            if (IsGridFull())
            {
                result = new GameResult { Winner = Player.None };
                return true;
            }

            result = null;
            return false;
        }

        public void MakeMove(int r, int c)
        {
            if (!CanMakeMove(r, c))
            {
                return;
            }

            GameGrid[r, c] = CurrentPlayer;
            TurnsPassed++;

            if(DidMoveEndGame(r,c, out GameResult result))
            {
                GameOver = true;
                /*if (MoveMade != null)
                {
                    MoveMade(r, c);
                }*/
                MoveMade?.Invoke(r, c);
                GameEnded?.Invoke(result);
            }
            else
            {
                SwitchPlayer();
                MoveMade?.Invoke(r, c);
            }
        }

        public void Reset()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }
    }
}

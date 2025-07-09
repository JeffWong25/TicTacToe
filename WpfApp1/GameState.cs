using System;
using System.Collections.Generic;
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
    }
}

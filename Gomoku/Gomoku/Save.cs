﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    internal class Save
    {
        public int X {  get; set; }
        public int Y { get; set; }
        public STONE stone { get; set; }
        public int seq { get; set; }
        
        public Save(int x, int y, STONE s, int seq)
        {
            this.X = x;
            this.Y = y;
            this.stone = s;
            this.seq = seq;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SpaceInvaders.Classes
{
    internal class Bandit
    {
        public Bandit() 
        {
            Name= string.Empty;
            BanditImage = new Image();
        
        }
        internal bool IsAlive { get; set; }
        internal int ColumnIndex { get; set; }
        internal int RowIndex { get; set; }
        internal int CanvasId { get; set; }
        internal double CanvasLeftPos { get; set; }
        internal double CanvasTopPos { get; set; }
        internal string Name { get; set; }
        internal Image BanditImage { get; set; }
    }

    internal class LaserBeam
    {
        public LaserBeam()
        {
            Name = string.Empty;
            LaserImage = new Image();

        }
        internal bool IsAlive { get; set; }
        internal int CanvasId { get; set; }
        internal double CanvasLeftPos { get; set; }
        internal double CanvasBottomPos { get; set; }
        internal string Name { get; set; }
        internal Image LaserImage { get; set; }

    }
}


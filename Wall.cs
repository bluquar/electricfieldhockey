using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ElectricFieldHockey
{
    class Wall
    {
        private Point[] _points;

        public Point[] Points
        {
            get { return _points; }
            set { _points = value; }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ElectricFieldHockey
{
    class GameObject
    {
        private float _xPosition;
        private float _yPosition;

        private float _xPositionPrevious;
        private float _yPositionPrevious;

        private float _xVelocity;
        private float _yVelocity;

        private float _xAcceleration;
        private float _yAcceleration;

        private Bitmap _bitmap;

        public float XPosition
        {
            get { return _xPosition; }
            set { _xPosition = value; }
        }
        public float YPosition
        {
            get { return _yPosition; }
            set { _yPosition = value; }
        }
        public float XPositionPrevious
        {
            get { return _xPositionPrevious; }
            set { _xPositionPrevious = value; }
        }
        public float YPositionPrevious
        {
            get { return _yPositionPrevious; }
            set { _yPositionPrevious = value; }
        }
        public float XVelocity
        {
            get { return _xVelocity; }
            set { _xVelocity = value; }
        }
        public float YVelocity
        {
            get { return _yVelocity; }
            set { _yVelocity = value; }
        }

        public float XAcceleration
        {
            get { return _xAcceleration; }
            set { _xAcceleration = value; }
        }
        public float YAcceleration
        {
            get { return _yAcceleration; }
            set { _yAcceleration = value; }
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }


    }
}

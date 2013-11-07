using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ElectricFieldHockey
{
    public enum PuckType
    {
        Proton,
    }

    public class PuckDef
    {
        public PuckType _type;
        public float _charge;
        public int _bmpId;


        public PuckDef(PuckType type, float charge, int bmpId)
        {
            _type = type;
            _charge = charge;
            _bmpId = bmpId;
        }
    }

    class Puck : GameObject
    {
        public static PuckDef[] puckDefs = 
            {
                new PuckDef(PuckType.Proton, 1, 0),
            };


        private PuckType _type;
        private bool _positive;
        private float _charge;
        private int _bmpId;
        private List<Point> _trail;
        private bool _scored;

        public PuckType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public bool Positive
        {
            get { return _positive; }
            set { _positive = value; }
        }
        public float Charge
        {
            get { return _charge; }
            set { _charge = value; }
        }
        public int BmpID
        {
            get { return _bmpId; }
            set { _bmpId = value; }
        }
        public List<Point> Trail
        {
            get { return _trail; }
            set { _trail = value; }
        }
        public bool Scored
        {
            get { return _scored; }
            set { _scored = value; }
        }

        public Puck(PuckDef pd)
        {
            _type = pd._type;
            _charge = pd._charge;
            _positive = true;
            if (_charge < 0)
                _positive = false;
            _bmpId = pd._bmpId;
        }
        public static Puck GetPuck(PuckType type)
        {
            List<PuckDef> defs = new List<PuckDef>();
            foreach (PuckDef pd in puckDefs)
            {
                if (pd._type == type)
                {
                    defs.Add(pd);
                    break;
                }
            }

            Puck p = new Puck(defs[0]);

            p.XAcceleration = 0;
            p.XVelocity = 0;
            p.YAcceleration = 0;
            p.YVelocity = 0;
            

            return p;
        }
    }


}

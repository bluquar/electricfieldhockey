using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricFieldHockey
{
    public enum PointChargeType
    {
        Positive,
        Negative
    }

    public class PointChargeDef
    {
        public PointChargeType _type;
        public float _charge;
        public float _friction;
        public int _bmpId;

        public PointChargeDef(PointChargeType type, float charge, float friction, int bmpId)
        {
            _type = type;
            _charge = charge;
            _friction = friction;
            _bmpId = bmpId;
        }
    }

    class PointCharge : GameObject
    {
        public static PointChargeDef[] pointChargeDefs = 
            {
                new PointChargeDef(PointChargeType.Positive, 1, 100000, 0),
                new PointChargeDef(PointChargeType.Negative, -1, 100000, 1),
            };


        private PointChargeType _type;
        private float _charge;
        private int _bmpId;
        private float _initialX;
        private float _initialY;

        public PointChargeType Type
        {
            get { return _type; }
            set { _type = value; }
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
        public float InitialX
        {
            get { return _initialX; }
            set { _initialX = value; }
        }
        public float InitialY
        {
            get { return _initialY; }
            set { _initialY = value; }
        }

        public PointCharge(PointChargeDef pcd)
        {
            _type = pcd._type;
            _charge = pcd._charge;
            _bmpId = pcd._bmpId;
        }
        public static PointCharge GetPointCharge(PointChargeType type)
        {
            List<PointChargeDef> defs = new List<PointChargeDef>();
            foreach (PointChargeDef pd in pointChargeDefs)
            {
                if (pd._type == type)
                {
                    defs.Add(pd);
                    break;
                }
            }

            PointCharge p = new PointCharge(defs[0]);

            p.XAcceleration = 0;
            p.XVelocity = 0;
            p.YAcceleration = 0;
            p.YVelocity = 0;

            return p;
        }
    }


}


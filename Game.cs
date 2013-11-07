using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.IO;
using System.Xml.Serialization;

namespace ElectricFieldHockey
{
    public partial class Game : Form
    {
        #region member variables

        private Bitmap _canvasBmp;
        private Bitmap _cpBmp;

        private Point _canvasClickDown;
        private bool _canvasClickDownHandled = true;
        private Point _canvasClickUp;
        private bool _canvasClickUpHandled = true;
        private Point _canvasMouseMove;
        private bool _clicking = false;

        private Point _cpClickDown;
        private bool _cpClickDownHandled = true;
        private Point _cpClickUp;
        private bool _cpClickUpHandled = true;
        private Point _cpMouseMove;

        private bool _placing;
        private bool _cleartoPlace;
        private bool _dragging;
        private Point _dragAnchor;

        private int[] _highScores;

        private bool _finishing = false;
        private bool _anchored = false;

        private Bitmap _buttonGoNotHoveringBmp;
        private Bitmap _buttonGoBmp;
        private Bitmap _levelHoverAuraBmp;
        private Bitmap _levelSelectAuraBmp;

        private Bitmap[] _playAura;

        private bool _inMenu = true;
        private short _levelSelected = 1;
        private short _levelsAvailable = 8;

        private PointChargeType _chargeSelected = PointChargeType.Positive;

        private List<GameObject> _activeGameObjects = new List<GameObject>();

        private List<Puck> _activePucks = new List<Puck>();
        private List<PointCharge> _activePointCharges = new List<PointCharge>();

        private List<PointCharge> _pointChargesDragging = new List<PointCharge>();

        private List<Wall> _activeWalls = new List<Wall>();

        private List<Goal> _activeGoals = new List<Goal>();

        private const float _k = 60;

        private int _score = 0;
        private int _goalsNeeded;

        #endregion
        #region bitmap arrays
        private Bitmap[] _puckBitmaps = new Bitmap[Puck.puckDefs.Length];
        private Bitmap[] _pointChargeBitmaps = new Bitmap[PointCharge.pointChargeDefs.Length];
        #endregion //bitmap arrays
        #region Fonts
        private Font _menuTitleFont = new Font("Courier New", 40);
        private Font _tableDataFont = new Font("Arial", 12);
        private Font _cpFont1 = new Font("Courier New", 12);
        #endregion //Fonts
        #region Brushes
        private Brush _yellowBrush = new SolidBrush(Color.Yellow);
        private Brush _blueBrush = new SolidBrush(Color.Blue);
        private Brush _whiteBrush = new SolidBrush(Color.White);
        #endregion //brushes
        #region Pens
        private Pen _grayPen = new Pen(Color.LightGray);
        private Pen _blackPen = new Pen(Color.Black, 4);
        private Pen _redPen = new Pen(Color.Red, 2);
        private Pen _whitePen = new Pen(Color.White, 4);
        private Pen _greenPen = new Pen(Color.DarkGreen, 2);
        private Pen _yellowPen = new Pen(Color.Yellow, 2);
        private Pen _purplePen = new Pen(Color.Purple, 2);
        private Pen _darkGrayPen = new Pen(Color.DarkGray, 2);
        private Pen _whiterPen = new Pen(Color.White, 1);
        #endregion //pens
        private Point[] _playbutton = new Point[]{ new Point(850, 20), new Point(900, 40), new Point(850, 60), new Point(850, 18)};
        

        public Game()
        {
            InitializeComponent();
            InitializeCanvas();
            InitializeButtons();

            //this.KeyDown += new KeyEventHandler(GotKeyDown);
            //this.KeyUp += new KeyEventHandler(GotKeyUp);
            GameTick.Enabled = true;
            this.KeyDown += new KeyEventHandler(GotKeyDown);
            this.KeyUp += new KeyEventHandler(GotKeyUp);
        }
        private void InitializeCanvas()
        {
            //makes the canvas bitmap and assigns midx and midy
            _canvasBmp = new Bitmap(Canvas.Width, Canvas.Height);
            Canvas.Image = _canvasBmp;

            _cpBmp = new Bitmap(ControlPanel.Width, ControlPanel.Height);
            ControlPanel.Image = _cpBmp;
        }
        private void InitializeButtons()
        {
            _buttonGoNotHoveringBmp = new Bitmap(GetType(), "ButtonGo.bmp");
            _buttonGoBmp = new Bitmap(GetType(), "ButtonGoNotHovering.bmp");
            _levelHoverAuraBmp = new Bitmap(GetType(), "LevelHoverAura.bmp");
            _levelHoverAuraBmp.MakeTransparent(Color.Black);
            _levelSelectAuraBmp = new Bitmap(GetType(), "LevelSelectAura.bmp");
            _levelSelectAuraBmp.MakeTransparent(Color.Black);

            _playAura = new Bitmap[]{ new Bitmap(GetType(), "PlayAura1.bmp"),
                                      new Bitmap(GetType(), "PlayAura2.bmp"),
                                      new Bitmap(GetType(), "PlayAura3.bmp"),
                                      new Bitmap(GetType(), "PlayAura4.bmp"),
                                      new Bitmap(GetType(), "PlayAura5.bmp") };
        }
        private void InitializeBitmaps()
        {
            _puckBitmaps[0] = new Bitmap(GetType(), "ProtonPuck.bmp");
            foreach (Bitmap bmp in _puckBitmaps)
            {
                bmp.MakeTransparent(Color.White);
            }

            _pointChargeBitmaps[0] = new Bitmap(GetType(), "PositivePoint.bmp");
            _pointChargeBitmaps[1] = new Bitmap(GetType(), "NegativePoint.bmp");
            foreach (Bitmap bmp in _pointChargeBitmaps)
            {
                bmp.MakeTransparent(Color.Magenta);
            }
        }
        private void InitializeObjects()
        {
            switch (_levelSelected)
            {
                case 1:
                    CreateGoal(900, 380);
                    CreateWall(new Point[]{ new Point(550, 280), new Point(570, 280), new Point(570, 500), new Point(550, 500), new Point(550, 278)});
                    CreateWall(new Point[] { new Point(0, 0), new Point(0, Canvas.Height), new Point(Canvas.Width, Canvas.Height), new Point(Canvas.Width, 0), new Point(0, 0) });
                    CreatePuck(200, 380, PuckType.Proton);
                    break;
                case 2:
                    CreateGoal(900, 380);
                    CreatePuck(200, 380, PuckType.Proton);
                    CreateWall(new Point[] { new Point(0, 0), new Point(0, Canvas.Height), new Point(Canvas.Width, Canvas.Height), new Point(Canvas.Width, 0), new Point(0, 0) });
                    CreateWall(new Point[] { new Point(250, 0), new Point(250, 315), new Point(275, 315), new Point(275, 290), new Point(300, 290), new Point(300, 265), 
                                             new Point (325, 265), new Point (325, 240), new Point(350, 240), new Point(350, 0)});
                    CreateWall(new Point[] { new Point(250, Canvas.Height), new Point(250, 400), new Point(275, 400), new Point(275, 375), new Point(300, 375), new Point(300, 350),
                                             new Point(325, 350), new Point(325, 325), new Point(350, 325), new Point(350, Canvas.Height)});
                    break;
                case 3:
                    CreateGoal(900, 380);
                    CreatePuck(150, 30, PuckType.Proton);
                    CreateWall(new Point[] { new Point(0, 0), new Point(0, Canvas.Height), new Point(Canvas.Width, Canvas.Height), new Point(Canvas.Width, 0), new Point(0, 0) });
                    CreateWall(new Point[] { new Point(300, 0), new Point(300, 420), new Point(330, 420), new Point(330, 0), new Point(300, 0) });
                    CreateWall(new Point[] { new Point(550, Canvas.Height), new Point(550, Canvas.Height - 420), new Point(580, Canvas.Height - 420), new Point(580, Canvas.Height),
                                             new Point(550, Canvas.Height)});
                    break;
                case 4:
                    CreateGoal(900, Canvas.Height / 2);
                    CreatePuck(150, 150, PuckType.Proton);
                    CreatePuck(150, Canvas.Height - 150, PuckType.Proton);
                    CreateWall(new Point[] { new Point(435, 0), new Point(435, Canvas.Height / 2 - 60), new Point(450, Canvas.Height / 2 - 60), new Point(450, 0) });
                    CreateWall(new Point[] { new Point(435, Canvas.Height), new Point(435, Canvas.Height / 2 + 60), new Point(450, Canvas.Height / 2 + 60), new Point(450, Canvas.Height)});
                    break;
                case 5:
                    CreatePuck(Canvas.Width / 2, 280, PuckType.Proton);
                    CreatePointCharge(Canvas.Width / 2, Canvas.Height / 2, PointChargeType.Negative);
                    break;
                case 6:

                    break;
                case 7:
                    CreateWall(new Point[] { new Point(0, 0), new Point(0, Canvas.Height), new Point(Canvas.Width, Canvas.Height), new Point(Canvas.Width, 0), new Point(0, 0) });
                    break;
                case 8:

                    break;                    
                default:
                    break;
            }
            
        }

        private void NewGame()
        {
            InitializeBitmaps();
            InitializeObjects();
            _inMenu = false;
            _anchored = true;
            _placing = true;
            _score = 0;
            _goalsNeeded = _activePucks.Count;
        }
        private void GameTimer_Tick(object sender, EventArgs e) //every time the timer ticks this function is called
        {
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                if (_inMenu == false)
                    DoGameAI();
                else
                    DoMenuAI();
            }

            RenderCanvas();
        }
        private void DoMenuAI()
        {
            CheckForButtonPresses();
        }
        private void CheckForButtonPresses()
        {
            if (_canvasClickDownHandled == false && _canvasClickUpHandled == true)
            {
                _clicking = true;
                _canvasClickDownHandled = true;
            }
            if (_canvasClickUpHandled == false)
            {
                if (_canvasClickUp.X >= 410 && _canvasClickUp.X <= _buttonGoNotHoveringBmp.Width + 410 &&
                    _canvasClickUp.Y >= 600 && _canvasClickUp.Y <= _buttonGoNotHoveringBmp.Height + 600)
                {
                    NewGame();
                }

                if (_canvasClickUp.X >= 200 && _canvasClickUp.X <= 300 && _canvasClickUp.Y >= 130 && _canvasClickUp.Y <= 130 + (_levelsAvailable * 30))
                {
                    _levelSelected = (short)((_canvasClickUp.Y - 100) / 30);
                }
                _canvasClickUpHandled = true;
                _clicking = false;
            }

        }
        private void DoGameAI()
        {
            if (!_finishing)
            {
                ReleaseAnchor();
                if (!_anchored)
                {
                    DoPhysics();
                }
                CreateObjects();
                TestCollisions();
            }
        }

        private void DoPhysics()
        {
            ApplyForces();
            MoveObjects();
        }
        private void ReleaseAnchor()
        {
            if (_cpClickUp.X >= 850 && _cpClickUp.X <= 900 && _cpClickUp.Y >= 25 && _cpClickUp.Y <= 55 && _cpClickUpHandled == false)
            {
                if (_anchored)
                {
                    _anchored = false;
                    foreach (Puck puck in _activePucks)
                    {
                        puck.Trail.Clear();
                        puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                        puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));

                        _score = 0;

                        foreach (PointCharge pc in _activePointCharges)
                        {

                            _score -= 10;
                        }
                        _goalsNeeded = 0;
                        foreach (Puck p in _activePucks)
                        {
                            _goalsNeeded++;
                            p.Scored = false;
                        }
                    }
                }
                else
                {
                    float x = 0;
                    float y = 0;
                    foreach (Puck puck in _activePucks)
                    {
                        foreach (Point point in puck.Trail)
                        {
                            x = point.X;
                            y = point.Y;
                            break;
                        }
                        puck.XPosition = x;
                        puck.YPosition = y;
                        puck.XPositionPrevious = x;
                        puck.YPositionPrevious = y;
                        puck.XVelocity = 0;
                        puck.YVelocity = 0;
                    }

                    foreach (PointCharge pc in _activePointCharges)
                    {
                        pc.XPosition = pc.InitialX;
                        pc.YPosition = pc.InitialY;
                        pc.XVelocity = 0;
                        pc.YVelocity = 0;
                    }

                    _anchored = true;

                }
            }
            if (_cpClickUp.X > 2 && _cpClickUp.X < 60 && _cpClickUp.Y >= ControlPanel.Height - 20 && _cpClickUp.Y < ControlPanel.Height && _cpClickUpHandled == false)
            {
                GoBackToMenu();
            }
            _cpClickUpHandled = true;
        }
        private void TestCollisions()
        {
            foreach (GameObject puck in _activeGameObjects)
            {
                if (puck.XPositionPrevious != puck.XPosition || puck.YPosition != puck.YPositionPrevious)
                {
                    foreach (Wall wall in _activeWalls)
                    {
                        for (int i = 0; i < wall.Points.Length - 1; i++)
                        {
                            if (wall.Points[i].X != wall.Points[i + 1].X && wall.Points[i].Y != wall.Points[i + 1].Y)
                            {
                                float x = ((puck.YPositionPrevious - wall.Points[i].Y + (((puck.XPositionPrevious * puck.YPositionPrevious) - (puck.XPositionPrevious - puck.YPosition)) / (puck.XPosition - puck.XPositionPrevious)) + (((wall.Points[i].X * wall.Points[i].Y) - (wall.Points[i].X * wall.Points[i + 1].Y)) / (wall.Points[i + 1].X - wall.Points[i].X))) / (((wall.Points[i + 1].Y - wall.Points[i].Y) / (wall.Points[i + 1].X - wall.Points[i].X)) + ((puck.YPositionPrevious - puck.YPosition) / (puck.XPosition - puck.XPositionPrevious))));

                                float a = wall.Points[i].X;
                                float b = wall.Points[i].Y;
                                float c = wall.Points[i + 1].X;
                                float d = wall.Points[i + 1].Y;
                                float e = puck.XPositionPrevious;
                                float f = puck.YPositionPrevious;
                                float g = puck.XPosition;
                                float h = puck.YPosition;

                                float xx = ((((b * c) - (a * d)) / (c - a)) + (((e * h) - (f * g)) / (g - e))) / (((h - f) / (g - e)) + ((b - d) / (c - a)));

                                if (((xx >= puck.XPositionPrevious && xx <= puck.XPosition) || (xx <= puck.XPositionPrevious && xx >= puck.XPosition)) &&
                                    ((xx >= a && xx <= c) || (xx <= a && xx >= c)))
                                {
                                    float slopeofWall = (d - b) / (c - a);
                                    float slopeofPuck = (h - f) / (g - e);

                                    double anglePuck = Math.Atan(slopeofPuck);
                                    double angleWall = Math.Atan(slopeofWall);

                                    double angleOfIncidence = angleWall - anglePuck;

                                    if (slopeofPuck > slopeofWall && slopeofWall > 0)
                                    {
                                        angleOfIncidence = 90 - anglePuck - angleWall;
                                    }

                                    //if (slopeofPuck > 0 && slopeofWall < 0)
                                    //    angleOfIncidence *= -1;

                                    //need different angles for different positive/negative slopes and obsute/acute collisions

                                    float originalVelocity = (float)Math.Sqrt((puck.XVelocity * puck.XVelocity) + (puck.YVelocity * puck.YVelocity));

                                    float changeInXV = puck.XVelocity * (float)(2 * Math.Cos(angleOfIncidence));
                                    float changeInYV = puck.YVelocity * (float)(2 * Math.Sin(angleOfIncidence));

                                    puck.XVelocity -= changeInXV;
                                    puck.YVelocity -= changeInYV;


                                    float newVelocity = (float)Math.Sqrt((puck.XVelocity * puck.XVelocity) + (puck.YVelocity * puck.YVelocity));

                                    puck.XVelocity *= (originalVelocity / newVelocity);
                                    puck.YVelocity *= (originalVelocity / newVelocity);
                                }
                            }
                            if (wall.Points[i].Y == wall.Points[i + 1].Y)
                            {
                                float y = wall.Points[i].Y;

                                if (puck.YPositionPrevious > puck.YPosition)
                                    y += puck.Bitmap.Height / 2;
                                else
                                    y -= puck.Bitmap.Height / 2;

                                if ((puck.YPositionPrevious >= y && puck.YPosition <= y) || (puck.YPositionPrevious <= y && puck.YPosition >= y))
                                {
                                    if (puck.XPositionPrevious == puck.XPosition)
                                    {
                                        if ((wall.Points[i].X <= puck.XPosition && wall.Points[i + 1].X >= puck.XPosition) || (wall.Points[i].X >= puck.XPosition && wall.Points[i + 1].X <= puck.XPosition))
                                        {
                                            puck.YPosition = y - (0.6f) * (puck.YVelocity - (y - puck.YPositionPrevious));
                                            puck.YVelocity *= -0.94f;

                                            _score -= 5;
                                            //puck.XPosition += puck.XVelocity;

                                            //puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                                        }
                                    }
                                    else
                                    {
                                        float xx = puck.XPositionPrevious + ((puck.XPosition - puck.XPositionPrevious) * (wall.Points[i].Y - puck.YPositionPrevious)) / (puck.YPosition - puck.YPositionPrevious);
                                        if ((wall.Points[i].X <= xx && wall.Points[i + 1].X >= xx) || (wall.Points[i].X >= xx && wall.Points[i + 1].X <= xx))
                                        {
                                            puck.YPosition = y - (0.6f) * (puck.YVelocity - (y - puck.YPositionPrevious));
                                            puck.YVelocity *= -0.94f;

                                            _score -= 5;
                                            //puck.XPosition += puck.XVelocity;

                                            //puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                                        }
                                    }
                                }
                            }
                            if (wall.Points[i].X == wall.Points[i + 1].X)
                            {
                                float x = wall.Points[i].X;

                                if (puck.XPositionPrevious > puck.XPosition)
                                    x += puck.Bitmap.Width / 2;
                                else
                                    x -= puck.Bitmap.Width / 2;

                                if ((puck.XPositionPrevious >= x && puck.XPosition <= x) || (puck.XPositionPrevious <= x && puck.XPosition >= x))
                                {
                                    if (puck.YPositionPrevious == puck.YPosition)
                                    {
                                        if ((wall.Points[i].Y <= puck.YPosition && wall.Points[i + 1].Y >= puck.YPosition) || (wall.Points[i].Y >= puck.YPosition && wall.Points[i + 1].Y <= puck.YPosition))
                                        {
                                            puck.XPosition = x - (0.94f) * (puck.XVelocity - (x - puck.XPositionPrevious));
                                            puck.XVelocity *= -0.94f;

                                            _score -= 5;
                                            //puck.YPosition += puck.YVelocity;

                                            //puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                                        }
                                    }
                                    else
                                    {
                                        float yy = puck.YPositionPrevious + ((puck.YPosition - puck.YPositionPrevious) * (wall.Points[i].X - puck.XPositionPrevious)) / (puck.XPosition - puck.XPositionPrevious);
                                        if ((wall.Points[i].Y <= yy && wall.Points[i + 1].Y >= yy) || (wall.Points[i].Y >= yy && wall.Points[i + 1].Y <= yy))
                                        {
                                            puck.XPosition = x - (0.94f) * (puck.XVelocity - (x - puck.XPositionPrevious));
                                            puck.XVelocity *= -0.94f;

                                            _score -= 5;
                                            //puck.YPosition += puck.YVelocity;

                                            //puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
                foreach (Puck puck in _activePucks)
                {
                    foreach (Goal goal in _activeGoals)
                    {
                        if ((puck.XPosition >= goal.XPosition - goal.Bitmap.Width / 2 && puck.XPositionPrevious <= goal.XPosition - goal.Bitmap.Width / 2) ||
                            (puck.XPosition <= goal.XPosition - goal.Bitmap.Width / 2 && puck.XPositionPrevious >= goal.XPosition - goal.Bitmap.Width / 2))
                        {
                            float yy = puck.YPositionPrevious + ((puck.YPosition - puck.YPositionPrevious) * ((goal.XPosition - goal.Bitmap.Width / 2) - puck.XPositionPrevious)) / (puck.XPosition - puck.XPositionPrevious);
                            if (goal.YPosition - goal.Bitmap.Height / 2 <= yy && goal.YPosition + goal.Bitmap.Height / 2 >= yy)
                            {
                                _score += 100;

                                _goalsNeeded--;

                                puck.Scored = true;

                                if (_goalsNeeded == 0)
                                    FinishGame();
                            }
                        }
                    }
                }
                    /*foreach (PointCharge pc in _activePointCharges)
                    {
                        float distsq = ((puck.XPosition - pc.XPosition) * (puck.XPosition - pc.XPosition)) + ((puck.YPosition - pc.YPosition) * (puck.YPosition - pc.YPosition));
                        if (distsq <= ((puck.Bitmap.Height / 2) + (pc.Bitmap.Height / 2)) * ((puck.Bitmap.Height / 2) + (pc.Bitmap.Height / 2)))
                        {
                            float aoi = (float)Math.Atan((pc.YPosition - puck.YPosition) / (pc.XPosition / puck.XPosition));
                            puck.XVelocity -= (float)(2 * Math.Cos(aoi)) * puck.XVelocity;
                            puck.YVelocity -= (float)(2 * Math.Sin(aoi)) * puck.YVelocity;
                        }
                    }*/


        }
        private void FinishGame()
        {
            _finishing = true;
        }
        private void ApplyForces()
        {
            foreach (Puck puck in _activePucks)
            {
                puck.XAcceleration = 0;
                puck.YAcceleration = 0;

                float distance = 0;

                foreach (PointCharge charge in _activePointCharges)
                {
                    distance = (float)Math.Sqrt(((puck.XPosition - charge.XPosition) * (puck.XPosition - charge.XPosition)) + 
                                       ((puck.YPosition  - charge.YPosition) * (puck.YPosition - charge.YPosition)));
                    if (distance < 22)
                        distance = 22;
                    puck.XAcceleration += _k * (((puck.Charge * charge.Charge) / (distance * distance * distance)) * (puck.XPosition - charge.XPosition));
                    puck.YAcceleration += _k * (((puck.Charge * charge.Charge) / (distance * distance * distance)) * (puck.YPosition - charge.YPosition));


                }
                foreach (Puck other in _activePucks)
                {
                    if (other != puck)
                    {
                        distance = (float)Math.Sqrt(((puck.XPosition - other.XPosition) * (puck.XPosition - other.XPosition)) +
                                       ((puck.YPosition - other.YPosition) * (puck.YPosition - other.YPosition)));
                        if (distance < 18)
                            distance = 18;
                        puck.XAcceleration += _k * (((puck.Charge * other.Charge) / (distance * distance * distance)) * (puck.XPosition - other.XPosition));
                        puck.YAcceleration += _k * (((puck.Charge * other.Charge) / (distance * distance * distance)) * (puck.YPosition - other.YPosition));
                    }
                }
            }

            foreach (PointCharge charge in _activePointCharges)
            {
                charge.XAcceleration = 0;
                charge.YAcceleration = 0;
                foreach (PointCharge pch in _activePointCharges)
                {
                    if (charge != pch)
                    {
                        float distance = (float)(Math.Sqrt(((pch.XPosition - charge.XPosition) * (pch.XPosition - charge.XPosition)) +
                                            ((pch.YPosition - charge.YPosition) * (pch.YPosition - charge.YPosition))));
                        if (distance < 26)
                            distance = 26;
                        charge.XAcceleration += (_k * ((float)vScrollBar1.Value) / 50) * (((pch.Charge * charge.Charge) / (distance * distance * distance)) * (charge.XPosition - pch.XPosition));
                        charge.YAcceleration += (_k * ((float)vScrollBar1.Value) / 50) * (((pch.Charge * charge.Charge) / (distance * distance * distance)) * (charge.YPosition - pch.YPosition));
                    }
                }

                foreach (Puck pk in _activePucks)
                {
                    float distance = (float)(Math.Sqrt(((pk.XPosition - charge.XPosition) * (pk.XPosition - charge.XPosition)) +
                                            ((pk.YPosition - charge.YPosition) * (pk.YPosition - charge.YPosition))));
                    if (distance < 22)
                        distance = 22;
                    charge.XAcceleration += (_k * ((float)vScrollBar1.Value) / 50) * (((pk.Charge * charge.Charge) / (distance * distance * distance)) * (charge.XPosition - pk.XPosition));
                    charge.YAcceleration += (_k * ((float)vScrollBar1.Value) / 50) * (((pk.Charge * charge.Charge) / (distance * distance * distance)) * (charge.YPosition - pk.YPosition));
                }
            }
        }
        private void MoveObjects()
        {

            foreach (GameObject obj in _activeGameObjects)
            {
                obj.XVelocity += obj.XAcceleration;
                obj.YVelocity += obj.YAcceleration;

                obj.XPositionPrevious = obj.XPosition;
                obj.YPositionPrevious = obj.YPosition;

                bool move = true;

                if (obj is Puck)
                {
                    if (((Puck)obj).Scored == true)
                        move = false;
                }
                if (move)
                {
                    obj.XPosition += obj.XVelocity;
                    obj.YPosition += obj.YVelocity;
                }
            }

            foreach (PointCharge pc in _activePointCharges)
            {
                foreach (PointCharge other in _activePointCharges)
                {
                    if (pc != other)
                    {
                        float dist = (float)Math.Sqrt(((pc.XPosition - other.XPosition) * (pc.XPosition - other.XPosition)) + ((pc.YPosition - other.YPosition) * (pc.YPosition - other.YPosition)));
                        if (dist < 26 && dist != 0)
                        {
                            pc.XPosition = other.XPosition + 26 * ((pc.XPosition - other.XPosition) / dist);
                            pc.YPosition = other.YPosition + 26 * ((pc.YPosition - other.YPosition) / dist);
                        }
                    }
                }
            }

            foreach (Puck puck in _activePucks)
            {
                foreach (PointCharge pc in _activePointCharges)
                {
                    float dist = (float)Math.Sqrt(((pc.XPosition - puck.XPosition) * (pc.XPosition - puck.XPosition)) + ((pc.YPosition - puck.YPosition) * (pc.YPosition - puck.YPosition)));
                    if (dist < 22)
                    {
                        puck.XPosition = pc.XPosition + 22 * ((puck.XPosition - pc.XPosition) / dist);
                        puck.YPosition = pc.YPosition + 22 * ((puck.YPosition - pc.YPosition) / dist);
                    }
                }
                if ((puck.XPosition != puck.XPositionPrevious) || (puck.YPosition != puck.YPositionPrevious))
                {
                    puck.Trail.Add(new Point((int)puck.XPosition, (int)puck.YPosition));
                }

            }
            List<PointCharge> toDelete = new List<PointCharge>();
            foreach (PointCharge _pc in _activePointCharges)
            {
                if (_pc.XPosition < 0 || _pc.XPosition > Canvas.Width || _pc.YPosition < 0 || _pc.YPosition > Canvas.Height)
                {
                    toDelete.Add(_pc);
                }
            }
            foreach (PointCharge _pc in toDelete)
            {
                _activePointCharges.Remove(_pc);
                _activeGameObjects.Remove(_pc);
            }
            toDelete.Clear();
        }

        private void CreateObjects()
        {
            _cleartoPlace = true;

            int x = _canvasMouseMove.X;
            int y = _canvasMouseMove.Y;

            if (_dragging)
            {
                x -= _dragAnchor.X;
                y -= _dragAnchor.Y;
            }

            foreach (PointCharge pc in _activePointCharges)
            {


                if (676 >= ((pc.XPosition - x) * (pc.XPosition - x)) + ((pc.YPosition - y) * (pc.YPosition - y)))
                {
                    bool stillClear = false;
                    foreach (PointCharge point in _pointChargesDragging)
                    {
                        if (pc == point)
                        {
                            stillClear = true;
                        }
                    }
                    
                    if (169 >= ((pc.XPosition - x) * (pc.XPosition - x)) + ((pc.YPosition - y) * (pc.YPosition - y)))
                    {
                        if (_canvasClickDownHandled == false)
                        {
                            _dragging = true;
                            _placing = false;
                            _pointChargesDragging.Add(pc);
                            _canvasClickDownHandled = true;

                            _dragAnchor = new Point((int)(_canvasClickDown.X - pc.XPosition), (int)(_canvasClickDown.Y - pc.YPosition));
                        }
                    }

                    if (stillClear == false)
                    {
                        _cleartoPlace = false;
                        break;
                    }
                }
            }
            foreach (Puck puck in _activePucks)
            {
                if (486 >= ((puck.XPosition - x) * (puck.XPosition - x)) + ((puck.YPosition - y) * (puck.YPosition - y)))
                {
                    _cleartoPlace = false;
                    break;
                }
            }

            if (_cleartoPlace && _dragging)
            {
                foreach (PointCharge pc in _pointChargesDragging)
                {
                    pc.XPosition = _canvasMouseMove.X - _dragAnchor.X;
                    pc.YPosition = _canvasMouseMove.Y - _dragAnchor.Y;
                    pc.InitialX = pc.XPosition;
                    pc.InitialY = pc.YPosition; 
                }
            }

            if (_canvasClickUpHandled == false)
            {
                if (_placing && _cleartoPlace)
                {
                    CreatePointCharge(_canvasClickUp.X, _canvasClickUp.Y, _chargeSelected);
                    
                }
                else if (_dragging)
                {
                    _dragging = false;
                    _dragAnchor.X = 0;
                    _dragAnchor.Y = 0;
                    _pointChargesDragging.Clear();
                    _placing = true;
                }
                _canvasClickUpHandled = true;
            }
            _canvasClickDownHandled = true;
        }

        private void CreatePuck(float x, float y, PuckType type)
        {
            Puck p = Puck.GetPuck(type);
            p.XPosition = x;
            p.YPosition = y;

            p.XPositionPrevious = x;
            p.YPositionPrevious = y;

            if (_levelSelected == 5)
            {
                p.XVelocity = 10;
            }

            p.Trail = new List<Point>();
            p.Trail.Add(new Point((int)x, (int)y));
            p.Trail.Add(new Point((int)x, (int)y));

            p.Bitmap = _puckBitmaps[p.BmpID];

            _activeGameObjects.Add(p);
            _activePucks.Add(p);
        }
        private void CreatePointCharge(float x, float y, PointChargeType type)
        {
            PointCharge p = PointCharge.GetPointCharge(type);
            p.XPosition = x;
            p.YPosition = y;

            p.InitialX = x;
            p.InitialY = y;

            _score -= 10;

            p.Bitmap = _pointChargeBitmaps[p.BmpID];

            _activeGameObjects.Add(p);
            _activePointCharges.Add(p);
        }
        private void CreateGoal(float x, float y)
        {
            Goal g = new Goal();
            g.Bitmap = new Bitmap(GetType(), "Goal.bmp");
            g.XPosition = x;
            g.YPosition = y;
            _activeGoals.Add(g);
            _activeGameObjects.Add(g);
        }
        private void CreateWall(Point[] points)
        {
            Wall w = new Wall();
            w.Points = points;
            _activeWalls.Add(w);
        }
        private void GoBackToMenu()
        {
            _finishing = false;
            _inMenu = true;
            _activeGameObjects.Clear();
            _activeGoals.Clear();
            _activePointCharges.Clear();
            _activePucks.Clear();
            _activeWalls.Clear();
            _clicking = false;
            _canvasClickUp.X = 0;
            _canvasClickUp.Y = 0;
            _canvasClickDown.X = 0;
            _canvasClickDown.Y = 0;
            _canvasClickDownHandled = true;
            _canvasClickUpHandled = true;
            _cpClickUp.X = 0;
            _cpClickUp.Y = 0;
            _cpClickDown.X = 0;
            _cpClickDown.Y = 0;
            _cpClickDownHandled = true;
            _cpClickUpHandled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void RenderCanvas()
        {
            using (Graphics gr = Graphics.FromImage(Canvas.Image))
            {
                if (_inMenu == false)
                {
                    gr.Clear(Color.LightGray);

                    #region Crosshair
                    if (_placing || _dragging)
                    {
                        if (_canvasMouseMove.X > 5 && _canvasMouseMove.X < Canvas.Width - 5 && _canvasMouseMove.Y > 5 && _canvasMouseMove.Y < Canvas.Height - 5)
                        {
                            //gr.DrawLine(_greenPen, _canvasMouseMove.X, 0, _canvasMouseMove.X, Canvas.Height);
                            //gr.DrawLine(_greenPen, 0, _canvasMouseMove.Y, Canvas.Width, _canvasMouseMove.Y);

                            int x = _canvasMouseMove.X;
                            int y = _canvasMouseMove.Y;

                            if (_dragging)
                            {
                                x -= _dragAnchor.X;
                                y -= _dragAnchor.Y;
                            }

                            gr.DrawLine(_whiterPen, x - 13, 0, x - 13, Canvas.Height);
                            gr.DrawLine(_whiterPen, x + 13, 0, x + 13, Canvas.Height);

                            gr.DrawLine(_whiterPen, 0, y - 13, Canvas.Width, y - 13);
                            gr.DrawLine(_whiterPen, 0, y + 13, Canvas.Width, y + 13);

                            if (!_dragging)
                            {
                                if (!_cleartoPlace)
                                {
                                    gr.DrawEllipse(_darkGrayPen, _canvasMouseMove.X - 13, _canvasMouseMove.Y - 13, 26, 26);
                                }

                                else
                                {
                                    switch (_chargeSelected)
                                    {
                                        case PointChargeType.Negative:
                                            gr.DrawEllipse(_yellowPen, _canvasMouseMove.X - 13, _canvasMouseMove.Y - 13, 26, 26);
                                            break;
                                        case PointChargeType.Positive:
                                            gr.DrawEllipse(_purplePen, _canvasMouseMove.X - 13, _canvasMouseMove.Y - 13, 26, 26);
                                            break; ;
                                        default:
                                            break;
                                    }
                                }

                            }
                        }
                    }
                    #endregion //crosshair
                    #region trail
                    foreach (Puck puck in _activePucks)
                    {
                        Point[] trail = puck.Trail.ToArray();
                        gr.DrawLines(_redPen, trail);
                    }
                    #endregion //trail
                    #region walls
                    foreach (Wall wall in _activeWalls)
                    {
                        gr.DrawLines(_blackPen, wall.Points);
                    }
                    #endregion //walls
                    #region Game Objects
                    foreach (GameObject obj in _activeGameObjects)
                    {
                        gr.DrawImage(obj.Bitmap, obj.XPosition - obj.Bitmap.Width / 2, obj.YPosition - obj.Bitmap.Height / 2);
                    }
                    #endregion //gameObjects
                    #region Game Over Menu
                    if (_finishing)
                    {
                        if (_canvasMouseMove.X >= 456 && _canvasMouseMove.X <= 536 && _canvasMouseMove.Y >= 73 && _canvasMouseMove.Y <= 100)
                            gr.FillRectangle(_whiteBrush, 457, 73, 78, 25);

                        gr.DrawString("You Win!", _menuTitleFont, _yellowBrush, 360, 20);
                        gr.DrawRectangle(_blackPen, 456, 73, 80, 27);
                        gr.DrawString("Continue", _tableDataFont, _blueBrush, 458, 76);

                        #region endgame
                        if (_canvasClickUp.X >= 456 && _canvasClickUp.X <= 536 && _canvasClickUp.Y >= 73 && _canvasClickUp.Y <= 100)
                        {
                            GoBackToMenu();
                        }
                        #endregion
                    }
                    #endregion Game Over Menu
                }

                else
                #region MenuRendering
                {
                    gr.Clear(Color.Black);

                    gr.DrawString("Electric Field Hockey", _menuTitleFont, _yellowBrush, 120, 10);

                    if (_canvasMouseMove.X >= 410 && _canvasMouseMove.X <= 410 + _buttonGoNotHoveringBmp.Width &&
                        _canvasMouseMove.Y >= 600 && _canvasMouseMove.Y <= 600 + _buttonGoNotHoveringBmp.Height)
                        gr.DrawImage(_buttonGoNotHoveringBmp, 414, 592);
                    else
                        gr.DrawImage(_buttonGoBmp, 410, 600);

                    bool drawSelect = true;
                    if (_canvasMouseMove.X >= 200 && _canvasMouseMove.X <= 300 &&
                        _canvasMouseMove.Y >= 130 && _canvasMouseMove.Y <= (130 + (30 * _levelsAvailable)) &&
                        Math.Abs(_canvasMouseMove.Y - (100 + 30 * _levelSelected + _levelSelectAuraBmp.Height / 2)) > _levelHoverAuraBmp.Height - 10)
                    {
                        int y = _canvasMouseMove.Y;
                        if (y < 150)
                            y = 150;
                        if (y > 115 + (30 * _levelsAvailable))
                            y = 115 + (30 * _levelsAvailable);

                        if (_clicking == false)
                            gr.DrawImage(_levelHoverAuraBmp, 195, y - 20);
                        else
                        {
                            gr.DrawImage(_levelSelectAuraBmp, 200, y - 20);
                            drawSelect = false;
                        }
                    }
                    if (drawSelect)
                        gr.DrawImage(_levelSelectAuraBmp, 200, 100 + (30 * _levelSelected));

                    gr.DrawRectangle(_grayPen, 200, 130, 600, 400);

                    for (int i = 0; i < _levelsAvailable; i++)
                    {
                        gr.DrawString("Level " + (i + 1).ToString(), _tableDataFont, _yellowBrush, 220, 140 + (30 * i));
                    }


                }
                #endregion //menu drawing
            }
            Canvas.Invalidate();

            using (Graphics gr = Graphics.FromImage(ControlPanel.Image))
            {
                gr.Clear(Color.Black);

                if (!_inMenu)
                {
                    #region start and stop menu
                    int dist = ((865 - _cpMouseMove.X) * (865 - _cpMouseMove.X)) + ((40 - _cpMouseMove.Y) * (40 - _cpMouseMove.Y));

                    //gr.DrawString(dist.ToString(), _tableDataFont, _yellowBrush, 0, 0);

                    if (dist <= 200)
                        gr.DrawImage(_playAura[3], 830, 0);
                    else if (dist <= 400)
                        gr.DrawImage(_playAura[2], 830, 0);
                    else if (dist <= 700)
                        gr.DrawImage(_playAura[1], 830, 0);
                    else if (dist <= 1500)
                        gr.DrawImage(_playAura[0], 830, 0);

                    if (_anchored)
                        gr.DrawLines(_whitePen, _playbutton);
                    else
                        gr.DrawRectangle(_whitePen, 850, 20, 50, 40);

                    if (_cpMouseMove.X > 2 && _cpMouseMove.X <= 60 && _cpMouseMove.Y >= ControlPanel.Height - 20 && _cpMouseMove.Y <= ControlPanel.Height)
                    {
                        gr.DrawImage(_levelHoverAuraBmp, -20, ControlPanel.Height - 30);
                    }
                    #endregion //start/stop menu

                    gr.DrawString("MENU", _cpFont1, _whiteBrush, 3, ControlPanel.Height - 20);

                    gr.DrawString("Score: " + _score.ToString(), _cpFont1, _yellowBrush, 2, 2);
                }
            }

            ControlPanel.Invalidate();
        }

        private void GotKeyDown(object o, KeyEventArgs e)
        {
            e.Handled = ProcessKeyDown(e);
        }
        private void GotKeyUp(object o, KeyEventArgs e)
        {
            e.Handled = ProcessKeyUp(e);
        }

        public bool ProcessKeyDown(KeyEventArgs e)
        {
            bool handled = true;

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Right:
                    //_pyroXv += 3;
                    break;
                case System.Windows.Forms.Keys.Left:
                    //_pyroXv -= 3;
                    break;
                case System.Windows.Forms.Keys.Up:
                    if (_inMenu == true)
                    {
                        if (_levelSelected != 1)
                            _levelSelected--;
                    }
                    break;
                case System.Windows.Forms.Keys.Down:
                    if (_inMenu == true)
                    {
                        if (_levelSelected != _levelsAvailable)
                            _levelSelected++;
                    }
                    break;
                default:
                    handled = true;
                    break;
            }
            return handled;
        }
        public bool ProcessKeyUp(KeyEventArgs e)
        {
            bool handled = true;

            switch (e.KeyCode)
            {
                default:
                    handled = true;
                    break;
            }
            return handled;
        }

        void Canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    {
                        _canvasClickDown.X = e.X;
                        _canvasClickDown.Y = e.Y;
                        if (_canvasClickDown.X != 0 || _canvasClickDown.Y != 0)
                            _canvasClickDownHandled = false;
                        break;
                    }
                case System.Windows.Forms.MouseButtons.Right:
                    {
                        if (_chargeSelected == PointChargeType.Positive)
                            _chargeSelected = PointChargeType.Negative;
                        else
                            _chargeSelected = PointChargeType.Positive;
                        break;
                    }
                default:
                    break;
            }
            
        }
        void Canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _canvasClickUp.X = e.X;
                _canvasClickUp.Y = e.Y;
                if (_canvasClickUp.X != 0 || _canvasClickUp.Y != 0)
                    _canvasClickUpHandled = false;

            }
        }
        void Canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _canvasMouseMove.X = e.X;
            _canvasMouseMove.Y = e.Y;
        }

        void ControlPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _cpClickUp.X = e.X;
            _cpClickUp.Y = e.Y;
            if (e.X != 0 || e.Y != 0)
                _cpClickUpHandled = false;
        }
        void ControlPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _cpMouseMove.X = e.X;
            _cpMouseMove.Y = e.Y;
        }
        void ControlPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _cpClickDown.X = e.X;
            _cpClickDown.Y = e.Y;
            if (e.X != 0 || e.Y != 0)
                _cpClickDownHandled = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }


}

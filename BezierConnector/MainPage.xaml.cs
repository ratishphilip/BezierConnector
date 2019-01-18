using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BezierConnector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Vector2 _startPoint;
        private readonly Random _random;
        private readonly Color[] _colors;
        private int _colorIndex = -1;

        private Vector2 _endPoint;
        private bool _isDragging = false;
        private bool _drawSpline = false;
        private bool _showControlPoints = false;

        List<Tuple<Vector2, Vector2, Vector2, Vector2, Color>> _pointData = new List<Tuple<Vector2, Vector2, Vector2, Vector2, Color>>();
        private Vector2 _controlPoint1;
        private Vector2 _controlPoint2;
        private Color _splineColor;

        public MainPage()
        {
            InitializeComponent();

            _random = new Random();
            _colors = new Color[]
            {
                Colors.Crimson,
                Colors.BlueViolet,
                Colors.LightSeaGreen,
                Colors.DeepPink,
                Colors.DimGray,
                Colors.YellowGreen,
                Colors.Blue,
                Colors.DarkRed,
                Colors.DarkGreen
            };
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            foreach (var point in _pointData)
            {
                DrawSpline(sender, ds, point.Item1, point.Item2, point.Item3, point.Item4, point.Item5);
            }

            if (_drawSpline)
            {
                var controlDistance = Math.Abs(_startPoint.X - _endPoint.X) / 2f;
                _controlPoint1 = _startPoint + new Vector2(controlDistance, 0);
                _controlPoint2 = _endPoint - new Vector2(controlDistance, 0);
                DrawSpline(sender, ds, _startPoint, _controlPoint1, _controlPoint2, _endPoint, _splineColor);
            }
        }

        private void DrawSpline(CanvasControl sender, CanvasDrawingSession ds,
            Vector2 startPoint, Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint,
            Color color)
        {
            var strokeThickness = 2f;

            // Draw the spline
            using (var pathBuilder = new CanvasPathBuilder(sender))
            {
                pathBuilder.BeginFigure(startPoint);
                pathBuilder.AddCubicBezier(controlPoint1, controlPoint2, endPoint);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);

                var geometry = CanvasGeometry.CreatePath(pathBuilder);
                ds.DrawGeometry(geometry, Vector2.Zero, color, strokeThickness);
            }

            // Draw Control Points
            if (_showControlPoints)
            {
                var strokeStyle = new CanvasStrokeStyle() { DashStyle = CanvasDashStyle.Dot };
                ds.DrawLine(startPoint, controlPoint1, color, strokeThickness, strokeStyle);
                var rect1 = new Rect(controlPoint1.X - 3, controlPoint1.Y - 3, 6, 6);
                ds.FillRectangle(rect1, Colors.Beige);
                ds.DrawRectangle(rect1, color, strokeThickness);

                ds.DrawLine(endPoint, controlPoint2, color, strokeThickness, strokeStyle);
                var rect2 = new Rect(controlPoint2.X - 3, controlPoint2.Y - 3, 6, 6);
                ds.FillRectangle(rect2, Colors.Beige);
                ds.DrawRectangle(rect2, color, strokeThickness);
            }

            // Draw EndPoints
            ds.DrawCircle(startPoint, 5, color, strokeThickness);
            ds.FillCircle(startPoint, 5, Colors.Beige);
            ds.DrawCircle(endPoint, 5, color, strokeThickness);
            ds.FillCircle(endPoint, 5, Colors.Beige);

        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = e.NewSize.Width;
            var height = e.NewSize.Height;

            _startPoint = new Vector2((float)width / 2f, (float)height / 2f);
            DrawCanvas.Invalidate();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetCurrentPoint(DrawCanvas).Position.ToVector2();
            _endPoint = _startPoint;
            _colorIndex = (_colorIndex + 1) % _colors.Length;

            _splineColor = _colors[_colorIndex];
            DrawCanvas.Invalidate();
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            _drawSpline = true;
            _endPoint = e.GetCurrentPoint(DrawCanvas).Position.ToVector2();
            DrawCanvas.Invalidate();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            _drawSpline = false;
            _pointData.Add(
                new Tuple<Vector2, Vector2, Vector2, Vector2, Color>(
                    _startPoint, _controlPoint1, _controlPoint2, _endPoint, _splineColor));
            _startPoint = Vector2.Zero;
            _endPoint = Vector2.Zero;
            _controlPoint1 = Vector2.Zero;
            _controlPoint2 = Vector2.Zero;
        }

        private void OnShowControlPoints(object sender, RoutedEventArgs e)
        {
            _showControlPoints = true;
            DrawCanvas.Invalidate();
        }

        private void OnHideControlPoints(object sender, RoutedEventArgs e)
        {
            _showControlPoints = false;
            DrawCanvas.Invalidate();
        }

        private void OnClearSplines(object sender, RoutedEventArgs e)
        {
            _pointData.Clear();
            DrawCanvas.Invalidate();
        }
    }
}

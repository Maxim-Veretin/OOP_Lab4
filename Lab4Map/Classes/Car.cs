using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Lab3Map.Classes
{
    class Car : MapObject
    {
        private PointLatLng point;
        //private PointLatLng destinationPoint;
        private Route route;
        public List<Human> passengers = new List<Human>();
        GMapMarker markerCar;

        public event EventHandler CarArrived;

        public Car(string title, PointLatLng point) : base(title)
        {
            this.point = point;
        }

        public override double GetDistance(PointLatLng point2)
        {
            GeoCoordinate c1 = new GeoCoordinate(point.Lat, point.Lng);
            GeoCoordinate c2 = new GeoCoordinate(point2.Lat, point2.Lng);

            double distance = c1.GetDistanceTo(c2);

            return distance;
        }

        public override PointLatLng GetFocus()
        {
            return point;
        }

        public override GMapMarker GetMarker()
        {
            markerCar = new GMapMarker(point)
            {
                Shape = new Image
                {
                    Width = 30, // ширина маркера
                    Height = 30, // высота маркера
                    ToolTip = this.GetTitle(), // всплывающая подсказка
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/car_icon.png")) // картинка
                },
                Position = point
            };

            return markerCar;
        }

        public GMapMarker MoveTo(PointLatLng endPoint)
        {
            // провайдер навигации
            RoutingProvider routingProvider = GMapProviders.OpenStreetMap;
            // определение маршрута
            MapRoute route = routingProvider.GetRoute(point, endPoint, false, false, 15);
            // получение точек маршрута
            List<PointLatLng> routePoints = route.Points;

            this.route = new Route("go", routePoints);

            Thread newThread = new Thread(new ThreadStart(MoveByRoute));
            newThread.Start();

            return this.route.GetMarker();
        }

        private void MoveByRoute()
        {
            // последовательный перебор точек маршрута
            foreach (var point in route.GetPoints())
            {
                // делегат, возвращающий управление в главный поток
                Application.Current.Dispatcher.Invoke(delegate {
                    // изменение позиции маркера
                    markerCar.Position = point;
                    this.point = point;

                    if (passengers.Count() > 0)
                    {
                        passengers[0].SetPoint(point);
                        passengers[0].markerHuman.Position = point;
                    }
                });
                // задержка 500 мс
                Thread.Sleep(500);
            }
            // отправка события о прибытии после достижения последней точки маршрута
            CarArrived?.Invoke(this, null);
        }

        
    }
}
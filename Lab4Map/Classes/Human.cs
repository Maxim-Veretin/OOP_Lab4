using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Lab3Map.Classes
{
    class Human : MapObject
    {
        private PointLatLng point;
        private PointLatLng destinationPoint;
        public GMapMarker markerHuman;

        public event EventHandler PassengerSeated;

        public Human(string title, PointLatLng point) : base(title)
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

        public void SetPoint(PointLatLng newPoint)
        {
            this.point = newPoint;
        }
        
        public override GMapMarker GetMarker()
        {
            markerHuman = new GMapMarker(point)
            {
                Shape = new Image
                {
                    Width = 25, // ширина маркера
                    Height = 25, // высота маркера
                    ToolTip = this.GetTitle(), // всплывающая подсказка
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/man_icon.png")) // картинка
                },
                Position = point
            };

            return markerHuman;
        }

        public void SetDestination(PointLatLng point)
        {
            destinationPoint = point;
        }

        public PointLatLng GetDestination()
        {
            return destinationPoint;
        }

        /// <summary> Обработчик события прибытия машины </summary>
        public void CarArrived(object sender, EventArgs e)
        {
            PassengerSeated?.Invoke(this, null);

            MessageBox.Show("Car is arrived");

            (sender as Car).CarArrived -= CarArrived;
        }
    }
}
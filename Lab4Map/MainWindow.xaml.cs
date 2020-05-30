﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Device.Location;
using Lab3Map.Classes;

namespace Lab3Map
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            comboBox.SelectedIndex = 0;
        }

        List<MapObject> mapObjects = new List<MapObject>();
        List<PointLatLng> activePoints = new List<PointLatLng>();
        //PointLatLng locationLast = new PointLatLng();
        bool IsHumanSet = false;
        Human curHuman;
        Classes.Location curLocation;
        Car nearestCar;

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            // настройка доступа к данным
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // установка провайдера карт
            Map.MapProvider = OpenStreetMapProvider.Instance;

            // установка зума карты
            Map.MinZoom = 2;
            Map.MaxZoom = 17;
            Map.Zoom = 15;

            // установка фокуса карты
            Map.Position = new PointLatLng(55.012823, 82.950359);

            // настройка взаимодействия с картой
            Map.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            Map.CanDragMap = true;
            Map.DragButton = MouseButton.Left;
        }

        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointLatLng point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
            activePoints.Add(point);

            listResults.Items.Clear();

            if (buttonSearchMode.IsChecked == true && mapObjects.Count != 0)
            {
                mapObjects.Sort((obj1, obj2) => obj1.GetDistance(point).CompareTo(obj2.GetDistance(point)));

                foreach (MapObject obj in mapObjects)
                {
                    listResults.Items.Add(obj.GetTitle() + " " + obj.GetDistance(point));
                }
            }
        }

        private void butOKAdd_Click(object sender, RoutedEventArgs e)
        {
            if (buttonCreationMode.IsChecked == true && activePoints.Count != 0)
                CreateMapObject();
        }

        private void CreateMapObject()
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    Area area = new Area(textNameAdd.Text.ToString(), activePoints);
                    mapObjects.Add(area);
                    Map.Markers.Add(area.GetMarker());
                    activePoints.Clear();
                    break;

                case 1:
                    Car car = new Car(textNameAdd.Text.ToString(), activePoints.Last());
                    mapObjects.Add(car);
                    Map.Markers.Add(car.GetMarker());
                    activePoints.Clear();
                    break;

                case 2:
                    Classes.Route route = new Classes.Route(textNameAdd.Text.ToString(), activePoints);
                    mapObjects.Add(route);
                    Map.Markers.Add(route.GetMarker());
                    activePoints.Clear();
                    break;

                case 3:
                    Human human = new Human(textNameAdd.Text.ToString(), activePoints.Last());
                    mapObjects.Add(human);
                    Map.Markers.Add(human.GetMarker());
                    activePoints.Clear();
                    break;

                case 4:
                    Classes.Location location = new Classes.Location(textNameAdd.Text.ToString(), activePoints.Last());
                    mapObjects.Add(location);
                    Map.Markers.Add(location.GetMarker());
                    activePoints.Clear();
                    break;
            }
        }

        private void butReset_Click(object sender, RoutedEventArgs e) => activePoints.Clear();

        private void butOKSearch_Click(object sender, RoutedEventArgs e)
        {
            if (textNameSearch.ToString() == "..." || textNameSearch.ToString() == "")
                MessageBox.Show("Введите имя искомого объекта");
            else
            {
                foreach (MapObject obj in mapObjects)
                {
                    if (obj.GetTitle() == textNameSearch.ToString())
                        listResults.Items.Add(obj.GetTitle().ToString());
                }
            }
        }

        private void listResults_MouseDoubleClick(object sender, MouseButtonEventArgs e) => GetFocus(listResults.SelectedItem);

        private void GetFocus(object selectedItem)
        {
            foreach (MapObject obj in mapObjects)
            {
                try
                {
                    if (obj.GetTitle() == selectedItem.ToString())
                        Map.Position = obj.GetFocus();
                }
                catch(NullReferenceException)
                {

                }
            }
        }

        private void whereToBut_Click(object sender, RoutedEventArgs e)
        {
            //buttonCreationMode.IsChecked = true;
            //comboBox.SelectedIndex = 4;
            buttonSearchMode.IsChecked = true;

        }

        private void requestBut_Click(object sender, RoutedEventArgs e)
        {
            // строится маршрут между текущими человеком и ближайшей машиной
            mapObjects.Sort((obj1, obj2) => obj1.GetDistance(curHuman.GetFocus()).CompareTo(obj2.GetDistance(curHuman.GetFocus())));

            foreach (MapObject m in mapObjects)
            {
                if (m is Car)
                {
                    nearestCar = m as Car;
                    break;
                }
            }

            
            
            
                Map.Markers.Add(nearestCar.MoveTo(curHuman.GetFocus()));

            nearestCar.CarArrived += curHuman.CarArrived;
            curHuman.PassengerSeated += PassengerSeated;
        }

        private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PointLatLng point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
            mapObjects.Sort((obj1, obj2) => obj1.GetDistance(point).CompareTo(obj2.GetDistance(point)));

            if (buttonSearchMode.IsChecked == true && mapObjects.Count != 0)
            {
                if (IsHumanSet == false)
                {
                    foreach (MapObject h in mapObjects)
                    {
                        if (h is Human)
                        {
                            curHuman = h as Human;
                            IsHumanSet = true;
                            fromLb.Content += curHuman.GetTitle();
                            break;
                        }
                    }
                }
                else if (IsHumanSet == true)
                {
                    foreach (MapObject l in mapObjects)
                    {
                        if (l is Classes.Location)
                        {
                            curLocation = l as Classes.Location;
                            curHuman.SetDestination(curLocation.GetFocus());
                            toLb.Content += curLocation.GetTitle();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary> Обработчик события "пассажир в такси" </summary>
        public void PassengerSeated(object sender, EventArgs e)
        {
            MessageBox.Show("Passenger in the taxi");
            nearestCar.passengers.Add(sender as Human);
            Application.Current.Dispatcher.Invoke(delegate {
                Map.Markers.Add(nearestCar.MoveTo((sender as Human).GetDestination()));
            });

            
        }
    }
}
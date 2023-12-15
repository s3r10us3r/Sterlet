﻿using Chess.gui;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
        }

        private void PlayerVsMachine_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
            //MessageBox.Show("PLAYER VS MACHINE PLACEHOLDER");
        }

        private void TimePlusClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("+ clicked");
        }

        private void TimeMinusClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("- clicked");
        }

        private void DiffPlusClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("+ clicked");
        }

        private void DiffMinusClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("- clicked");
        }
    }

   
}

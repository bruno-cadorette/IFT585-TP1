﻿using IFT585_TP1.ViewModel;
using System;
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
using System.Windows.Shapes;

namespace IFT585_TP1
{
    /// <summary>
    /// Interaction logic for Receive.xaml
    /// </summary>
    public partial class Receive : Window
    {

        public Receive()
        {
            InitializeComponent();
            DataContext = new ReceiverViewModel();
        }
    }
}

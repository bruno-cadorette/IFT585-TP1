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
using System.Windows.Shapes;

namespace IFT585_TP1
{
    /// <summary>
    /// Interaction logic for Send.xaml
    /// </summary>
    public partial class Send : Window
    {
        private SendViewModel sendViewModel { get; set; }
        public Send()
        {
            InitializeComponent();
            sendViewModel = new SendViewModel();
            this.DataContext = sendViewModel;
        }
    }
}

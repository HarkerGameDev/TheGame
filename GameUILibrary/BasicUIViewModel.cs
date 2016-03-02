﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Input;
using EmptyKeys.UserInterface.Mvvm;
using System.Diagnostics;

namespace GameUILibrary
{
    /// <summary>
    /// Example of MVVM View Model
    /// </summary>
    public class BasicUIViewModel : ViewModelBase
    {
        private string buttonResult;
        private string textBoxText;
        private bool buttonEnabled;
        private float progressValue;
        private float sliderValue;
        private ObservableCollection<WindowViewModel> windows;
        private float numericTextBoxValue;
        private string password;
        private List<PointF> chartData;

        /// <summary>
        /// Gets or sets the button command.
        /// </summary>
        /// <value>
        /// The button command.
        /// </value>
        public ICommand ButtonCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the button result.
        /// </summary>
        /// <value>
        /// The button result.
        /// </value>
        public string ButtonResult
        {
            get { return buttonResult; }
            set { SetProperty(ref buttonResult, value); }
        }

        /// <summary>
        /// Gets or sets the text box text.
        /// </summary>
        /// <value>
        /// The text box text.
        /// </value>
        public string TextBoxText
        {
            get { return textBoxText; }
            set { SetProperty(ref textBoxText, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [button enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [button enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ButtonEnabled
        {
            get { return buttonEnabled; }
            set { SetProperty(ref buttonEnabled, value); }
        }

        /// <summary>
        /// Gets or sets the progress value.
        /// </summary>
        /// <value>
        /// The progress value.
        /// </value>
        public float ProgressValue
        {
            get { return progressValue; }
            set { SetProperty(ref progressValue, value); }
        }

        /// <summary>
        /// Gets or sets the slider value.
        /// </summary>
        /// <value>
        /// The slider value.
        /// </value>
        public float SliderValue
        {
            get { return sliderValue; }
            set { SetProperty(ref sliderValue, value); }
        }

        /// <summary>
        /// Gets or sets the windows.
        /// </summary>
        /// <value>
        /// The windows.
        /// </value>
        public ObservableCollection<WindowViewModel> Windows
        {
            get { return windows; }
            set { SetProperty(ref windows, value); }
        }

        /// <summary>
        /// Gets or sets the numeric text box value.
        /// </summary>
        /// <value>
        /// The numeric text box value.
        /// </value>
        public float NumericTextBoxValue
        {
            get { return numericTextBoxValue; }
            set { SetProperty(ref numericTextBoxValue, value); }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        /// <summary>
        /// Gets or sets the chart data.
        /// </summary>
        /// <value>
        /// The chart data.
        /// </value>
        public List<PointF> ChartData
        {
            get { return chartData; }
            set { SetProperty(ref chartData, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicUIViewModel"/> class.
        /// </summary>
        public BasicUIViewModel()
        {
            ButtonCommand = new RelayCommand(new Action<object>(OnButtonClick));

            Windows = new ObservableCollection<WindowViewModel>();
            //Windows.Add(new CustomWindow());

            ChartData = new List<PointF>();
            for (int i = 0; i < 10; i++)
            {
                ChartData.Add(new PointF(i, i * 40));
            }
        }

        private void OnButtonClick(object obj)
        {
            if (obj != null)
            {
                ButtonResult = obj.ToString();
                ProgressValue += 0.5f;
                ButtonEnabled = true;
                NumericTextBoxValue = 100;
                Password = string.Empty;
                Debug.WriteLine("Clicked button " + ButtonResult);
            }
        }

        //public void Update(double elapsedTime)
        //{
        //    if (Tetris != null)
        //    {
        //        Tetris.Update(elapsedTime);
        //    }
        //}
    }
}
using System;
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
        private string controlsText;
        private string playerText;
        private bool buttonEnabled;
        private int maxPlayers;
        private int levelValue;
        private int playerValue;
        private float numericTextBoxValue;
        private string password;

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
        /// Gets or sets the text for player controls.
        /// </summary>
        /// <value>
        /// The player controls.
        /// </value>
        public string ControlsText
        {
            get { return controlsText; }
            set { SetProperty(ref controlsText, value); }
        }

        /// <summary>
        /// Gets or sets the maximum number of players for the Setup screen
        /// </summary>
        /// <value>
        /// Maximum number of players
        /// </value>
        public int MaxPlayers
        {
            get { return maxPlayers; }
            set { SetProperty(ref maxPlayers, value); }
        }

        /// <summary>
        /// Gets or sets the menu text to display when player is picking a character.
        /// </summary>
        /// <value>
        /// The text to display.
        /// </value>
        public string PlayerText
        {
            get { return playerText; }
            set { SetProperty(ref playerText, value); }
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
        /// Gets or sets the slider value for level.
        /// </summary>
        /// <value>
        /// The progress value.
        /// </value>
        public int LevelValue
        {
            get { return levelValue; }
            set { SetProperty(ref levelValue, value); }
        }

        /// <summary>
        /// Gets or sets the slider value for number of players.
        /// </summary>
        /// <value>
        /// The slider value.
        /// </value>
        public int PlayerValue
        {
            get { return playerValue; }
            set { SetProperty(ref playerValue, value); }
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
        /// Initializes a new instance of the <see cref="BasicUIViewModel"/> class.
        /// </summary>
        public BasicUIViewModel()
        {
            ButtonCommand = new RelayCommand(new Action<object>(OnButtonClick));
            levelValue = 1;
        }

        private void OnButtonClick(object obj)
        {
            if (obj != null)
            {
                ButtonResult = obj.ToString();
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

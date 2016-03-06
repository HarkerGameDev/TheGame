// -----------------------------------------------------------
//  
//  This file was generated, please do not modify.
//  
// -----------------------------------------------------------
namespace EmptyKeys.UserInterface.Generated {
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.ObjectModel;
    using EmptyKeys.UserInterface;
    using EmptyKeys.UserInterface.Charts;
    using EmptyKeys.UserInterface.Data;
    using EmptyKeys.UserInterface.Controls;
    using EmptyKeys.UserInterface.Controls.Primitives;
    using EmptyKeys.UserInterface.Input;
    using EmptyKeys.UserInterface.Interactions.Core;
    using EmptyKeys.UserInterface.Interactivity;
    using EmptyKeys.UserInterface.Media;
    using EmptyKeys.UserInterface.Media.Animation;
    using EmptyKeys.UserInterface.Media.Imaging;
    using EmptyKeys.UserInterface.Shapes;
    using EmptyKeys.UserInterface.Renderers;
    using EmptyKeys.UserInterface.Themes;
    
    
    [GeneratedCodeAttribute("Empty Keys UI Generator", "1.13.0.0")]
    public partial class MainMenu : UIRoot {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private Button Start;
        
        private Button e_2;
        
        private Button e_3;
        
        public MainMenu() : 
                base() {
            this.Initialize();
        }
        
        public MainMenu(int width, int height) : 
                base(width, height) {
            this.Initialize();
        }
        
        private void Initialize() {
            Style style = RootStyle.CreateRootStyle();
            style.TargetType = this.GetType();
            this.Style = style;
            this.InitializeComponent();
        }
        
        private void InitializeComponent() {
            // e_0 element
            this.e_0 = new Grid();
            this.Content = this.e_0;
            this.e_0.Name = "e_0";
            this.e_0.Height = 640F;
            this.e_0.Width = 800F;
            this.e_0.HorizontalAlignment = HorizontalAlignment.Center;
            RowDefinition row_e_0_0 = new RowDefinition();
            row_e_0_0.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_0);
            RowDefinition row_e_0_1 = new RowDefinition();
            row_e_0_1.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_1);
            RowDefinition row_e_0_2 = new RowDefinition();
            row_e_0_2.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_2);
            RowDefinition row_e_0_3 = new RowDefinition();
            row_e_0_3.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_3);
            RowDefinition row_e_0_4 = new RowDefinition();
            row_e_0_4.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_4);
            RowDefinition row_e_0_5 = new RowDefinition();
            row_e_0_5.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_5);
            RowDefinition row_e_0_6 = new RowDefinition();
            row_e_0_6.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_6);
            RowDefinition row_e_0_7 = new RowDefinition();
            row_e_0_7.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_7);
            // e_1 element
            this.e_1 = new TextBlock();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            this.e_1.Width = float.NaN;
            this.e_1.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_1.VerticalAlignment = VerticalAlignment.Center;
            this.e_1.Text = "Game";
            this.e_1.TextAlignment = TextAlignment.Center;
            this.e_1.FontSize = 40F;
            this.e_1.FontStyle = FontStyle.Bold;
            // Start element
            this.Start = new Button();
            this.e_0.Children.Add(this.Start);
            this.Start.Name = "Start";
            this.Start.Height = 80F;
            this.Start.Width = 165F;
            this.Start.TabIndex = 1;
            this.Start.FontSize = 20F;
            this.Start.FontStyle = FontStyle.Bold;
            this.Start.Content = "Start";
            this.Start.CommandParameter = "Start";
            Grid.SetRow(this.Start, 2);
            Binding binding_Start_Command = new Binding("ButtonCommand");
            this.Start.SetBinding(Button.CommandProperty, binding_Start_Command);
            // e_2 element
            this.e_2 = new Button();
            this.e_0.Children.Add(this.e_2);
            this.e_2.Name = "e_2";
            this.e_2.Height = 80F;
            this.e_2.Width = 165F;
            this.e_2.TabIndex = 2;
            this.e_2.FontSize = 20F;
            this.e_2.FontStyle = FontStyle.Bold;
            this.e_2.Content = "Options";
            this.e_2.CommandParameter = "Options";
            Grid.SetRow(this.e_2, 4);
            Binding binding_e_2_Command = new Binding("ButtonCommand");
            this.e_2.SetBinding(Button.CommandProperty, binding_e_2_Command);
            // e_3 element
            this.e_3 = new Button();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.Height = 80F;
            this.e_3.Width = 165F;
            this.e_3.TabIndex = 3;
            this.e_3.FontSize = 20F;
            this.e_3.FontStyle = FontStyle.Bold;
            this.e_3.Content = "Quit";
            this.e_3.CommandParameter = "Exit";
            Grid.SetRow(this.e_3, 6);
            Binding binding_e_3_Command = new Binding("ButtonCommand");
            this.e_3.SetBinding(Button.CommandProperty, binding_e_3_Command);
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
        }
    }
}

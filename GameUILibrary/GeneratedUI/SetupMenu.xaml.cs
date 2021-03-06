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
    
    
    [GeneratedCodeAttribute("Empty Keys UI Generator", "2.0.0.0")]
    public partial class SetupMenu : UIRoot {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private Button e_2;
        
        private StackPanel e_3;
        
        private DockPanel e_4;
        
        private TextBlock e_5;
        
        private TextBlock e_6;
        
        private Slider e_7;
        
        private DockPanel e_8;
        
        private TextBlock e_9;
        
        private TextBlock e_10;
        
        private Slider e_11;
        
        private TextBlock e_12;
        
        private StackPanel e_13;
        
        private Button e_14;
        
        private Button e_15;
        
        private Button e_16;
        
        private Button e_17;
        
        public SetupMenu() : 
                base() {
            this.Initialize();
        }
        
        public SetupMenu(int width, int height) : 
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
            row_e_0_0.Height = new GridLength(1F, GridUnitType.Auto);
            this.e_0.RowDefinitions.Add(row_e_0_0);
            RowDefinition row_e_0_1 = new RowDefinition();
            row_e_0_1.Height = new GridLength(1F, GridUnitType.Star);
            this.e_0.RowDefinitions.Add(row_e_0_1);
            // e_1 element
            this.e_1 = new TextBlock();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            this.e_1.Width = float.NaN;
            this.e_1.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_1.VerticalAlignment = VerticalAlignment.Center;
            this.e_1.Text = "Setup Game";
            this.e_1.TextAlignment = TextAlignment.Center;
            this.e_1.FontSize = 40F;
            this.e_1.FontStyle = FontStyle.Bold;
            // e_2 element
            this.e_2 = new Button();
            this.e_0.Children.Add(this.e_2);
            this.e_2.Name = "e_2";
            this.e_2.Height = 80F;
            this.e_2.Width = 165F;
            this.e_2.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_2.VerticalAlignment = VerticalAlignment.Center;
            this.e_2.TabIndex = 0;
            this.e_2.FontSize = 20F;
            this.e_2.FontStyle = FontStyle.Bold;
            this.e_2.Content = "Back";
            this.e_2.CommandParameter = "MainMenu";
            Binding binding_e_2_Command = new Binding("ButtonCommand");
            this.e_2.SetBinding(Button.CommandProperty, binding_e_2_Command);
            // e_3 element
            this.e_3 = new StackPanel();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.Margin = new Thickness(30F, 30F, 30F, 30F);
            Grid.SetRow(this.e_3, 1);
            // e_4 element
            this.e_4 = new DockPanel();
            this.e_3.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.Margin = new Thickness(20F, 20F, 20F, 20F);
            // e_5 element
            this.e_5 = new TextBlock();
            this.e_4.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Text = "Players";
            this.e_5.Padding = new Thickness(10F, 10F, 10F, 10F);
            this.e_5.TextAlignment = TextAlignment.Center;
            this.e_5.FontSize = 28F;
            this.e_5.FontStyle = FontStyle.Bold;
            DockPanel.SetDock(this.e_5, Dock.Top);
            // e_6 element
            this.e_6 = new TextBlock();
            this.e_4.Children.Add(this.e_6);
            this.e_6.Name = "e_6";
            this.e_6.Width = 40F;
            this.e_6.TextAlignment = TextAlignment.Right;
            this.e_6.FontSize = 20F;
            DockPanel.SetDock(this.e_6, Dock.Right);
            Binding binding_e_6_Text = new Binding("PlayerValue");
            this.e_6.SetBinding(TextBlock.TextProperty, binding_e_6_Text);
            // e_7 element
            this.e_7 = new Slider();
            this.e_4.Children.Add(this.e_7);
            this.e_7.Name = "e_7";
            this.e_7.Minimum = 1F;
            Binding binding_e_7_Maximum = new Binding("MaxPlayers");
            this.e_7.SetBinding(Slider.MaximumProperty, binding_e_7_Maximum);
            Binding binding_e_7_Value = new Binding("PlayerValue");
            this.e_7.SetBinding(Slider.ValueProperty, binding_e_7_Value);
            // e_8 element
            this.e_8 = new DockPanel();
            this.e_3.Children.Add(this.e_8);
            this.e_8.Name = "e_8";
            this.e_8.Margin = new Thickness(20F, 20F, 20F, 20F);
            // e_9 element
            this.e_9 = new TextBlock();
            this.e_8.Children.Add(this.e_9);
            this.e_9.Name = "e_9";
            this.e_9.Text = "Level";
            this.e_9.Padding = new Thickness(10F, 10F, 10F, 10F);
            this.e_9.TextAlignment = TextAlignment.Center;
            this.e_9.FontSize = 28F;
            this.e_9.FontStyle = FontStyle.Bold;
            DockPanel.SetDock(this.e_9, Dock.Top);
            // e_10 element
            this.e_10 = new TextBlock();
            this.e_8.Children.Add(this.e_10);
            this.e_10.Name = "e_10";
            this.e_10.Width = 40F;
            this.e_10.TextAlignment = TextAlignment.Right;
            this.e_10.FontSize = 20F;
            DockPanel.SetDock(this.e_10, Dock.Right);
            Binding binding_e_10_Text = new Binding("LevelValue");
            this.e_10.SetBinding(TextBlock.TextProperty, binding_e_10_Text);
            // e_11 element
            this.e_11 = new Slider();
            this.e_8.Children.Add(this.e_11);
            this.e_11.Name = "e_11";
            this.e_11.Minimum = 0F;
            this.e_11.Maximum = 1F;
            Binding binding_e_11_Value = new Binding("LevelValue");
            this.e_11.SetBinding(Slider.ValueProperty, binding_e_11_Value);
            // e_12 element
            this.e_12 = new TextBlock();
            this.e_3.Children.Add(this.e_12);
            this.e_12.Name = "e_12";
            this.e_12.Text = "Camera Type (choose one to start game)";
            this.e_12.Padding = new Thickness(10F, 10F, 10F, 10F);
            this.e_12.TextAlignment = TextAlignment.Center;
            this.e_12.FontSize = 28F;
            this.e_12.FontStyle = FontStyle.Bold;
            // e_13 element
            this.e_13 = new StackPanel();
            this.e_3.Children.Add(this.e_13);
            this.e_13.Name = "e_13";
            this.e_13.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_13.Orientation = Orientation.Horizontal;
            // e_14 element
            this.e_14 = new Button();
            this.e_13.Children.Add(this.e_14);
            this.e_14.Name = "e_14";
            this.e_14.Height = 80F;
            this.e_14.Width = 165F;
            this.e_14.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_14.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_14.VerticalAlignment = VerticalAlignment.Center;
            this.e_14.TabIndex = 3;
            this.e_14.FontSize = 20F;
            this.e_14.FontStyle = FontStyle.Bold;
            this.e_14.Content = "Average";
            this.e_14.CommandParameter = "Average";
            Binding binding_e_14_Command = new Binding("ButtonCommand");
            this.e_14.SetBinding(Button.CommandProperty, binding_e_14_Command);
            // e_15 element
            this.e_15 = new Button();
            this.e_13.Children.Add(this.e_15);
            this.e_15.Name = "e_15";
            this.e_15.Height = 80F;
            this.e_15.Width = 165F;
            this.e_15.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_15.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_15.VerticalAlignment = VerticalAlignment.Center;
            this.e_15.TabIndex = 4;
            this.e_15.FontSize = 20F;
            this.e_15.FontStyle = FontStyle.Bold;
            this.e_15.Content = "Player";
            this.e_15.CommandParameter = "Player";
            Binding binding_e_15_Command = new Binding("ButtonCommand");
            this.e_15.SetBinding(Button.CommandProperty, binding_e_15_Command);
            // e_16 element
            this.e_16 = new Button();
            this.e_13.Children.Add(this.e_16);
            this.e_16.Name = "e_16";
            this.e_16.Height = 80F;
            this.e_16.Width = 165F;
            this.e_16.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_16.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_16.VerticalAlignment = VerticalAlignment.Center;
            this.e_16.TabIndex = 5;
            this.e_16.FontSize = 20F;
            this.e_16.FontStyle = FontStyle.Bold;
            this.e_16.Content = "Path";
            this.e_16.CommandParameter = "Path";
            Binding binding_e_16_Command = new Binding("ButtonCommand");
            this.e_16.SetBinding(Button.CommandProperty, binding_e_16_Command);
            // e_17 element
            this.e_17 = new Button();
            this.e_3.Children.Add(this.e_17);
            this.e_17.Name = "e_17";
            this.e_17.Height = 80F;
            this.e_17.Width = 165F;
            this.e_17.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_17.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_17.VerticalAlignment = VerticalAlignment.Center;
            this.e_17.TabIndex = 6;
            this.e_17.FontSize = 20F;
            this.e_17.FontStyle = FontStyle.Bold;
            this.e_17.Content = "Done";
            this.e_17.CommandParameter = "Character";
            Binding binding_e_17_Visibility = new Binding("ButtonEnabled");
            this.e_17.SetBinding(Button.VisibilityProperty, binding_e_17_Visibility);
            Binding binding_e_17_Command = new Binding("ButtonCommand");
            this.e_17.SetBinding(Button.CommandProperty, binding_e_17_Command);
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
            FontManager.Instance.AddFont("Segoe UI", 28F, FontStyle.Bold, "Segoe_UI_21_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Regular, "Segoe_UI_15_Regular");
        }
    }
}

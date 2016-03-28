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
    public partial class SetupMenu : UIRoot {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private Button e_2;
        
        private Button e_3;
        
        private StackPanel e_4;
        
        private DockPanel e_5;
        
        private TextBlock e_6;
        
        private TextBlock e_7;
        
        private Slider e_8;
        
        private DockPanel e_9;
        
        private TextBlock e_10;
        
        private TextBlock e_11;
        
        private Slider e_12;
        
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
            this.e_3 = new Button();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.Height = 80F;
            this.e_3.Width = 165F;
            this.e_3.Focusable = false;
            this.e_3.HorizontalAlignment = HorizontalAlignment.Right;
            this.e_3.VerticalAlignment = VerticalAlignment.Center;
            this.e_3.FontSize = 20F;
            this.e_3.FontStyle = FontStyle.Bold;
            this.e_3.Content = "Done";
            this.e_3.CommandParameter = "Character";
            Binding binding_e_3_Command = new Binding("ButtonCommand");
            this.e_3.SetBinding(Button.CommandProperty, binding_e_3_Command);
            // e_4 element
            this.e_4 = new StackPanel();
            this.e_0.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.Margin = new Thickness(30F, 30F, 30F, 30F);
            Grid.SetRow(this.e_4, 1);
            // e_5 element
            this.e_5 = new DockPanel();
            this.e_4.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Margin = new Thickness(20F, 20F, 20F, 20F);
            // e_6 element
            this.e_6 = new TextBlock();
            this.e_5.Children.Add(this.e_6);
            this.e_6.Name = "e_6";
            this.e_6.Text = "Players";
            this.e_6.Padding = new Thickness(10F, 10F, 10F, 10F);
            this.e_6.TextAlignment = TextAlignment.Center;
            this.e_6.FontSize = 28F;
            this.e_6.FontStyle = FontStyle.Bold;
            DockPanel.SetDock(this.e_6, Dock.Top);
            // e_7 element
            this.e_7 = new TextBlock();
            this.e_5.Children.Add(this.e_7);
            this.e_7.Name = "e_7";
            this.e_7.Width = 40F;
            this.e_7.TextAlignment = TextAlignment.Right;
            this.e_7.FontSize = 20F;
            DockPanel.SetDock(this.e_7, Dock.Right);
            Binding binding_e_7_Text = new Binding("PlayerValue");
            this.e_7.SetBinding(TextBlock.TextProperty, binding_e_7_Text);
            // e_8 element
            this.e_8 = new Slider();
            this.e_5.Children.Add(this.e_8);
            this.e_8.Name = "e_8";
            this.e_8.Minimum = 1F;
            Binding binding_e_8_Maximum = new Binding("MaxPlayers");
            this.e_8.SetBinding(Slider.MaximumProperty, binding_e_8_Maximum);
            Binding binding_e_8_Value = new Binding("PlayerValue");
            this.e_8.SetBinding(Slider.ValueProperty, binding_e_8_Value);
            // e_9 element
            this.e_9 = new DockPanel();
            this.e_4.Children.Add(this.e_9);
            this.e_9.Name = "e_9";
            this.e_9.Margin = new Thickness(20F, 20F, 20F, 20F);
            // e_10 element
            this.e_10 = new TextBlock();
            this.e_9.Children.Add(this.e_10);
            this.e_10.Name = "e_10";
            this.e_10.Text = "Level";
            this.e_10.Padding = new Thickness(10F, 10F, 10F, 10F);
            this.e_10.TextAlignment = TextAlignment.Center;
            this.e_10.FontSize = 28F;
            this.e_10.FontStyle = FontStyle.Bold;
            DockPanel.SetDock(this.e_10, Dock.Top);
            // e_11 element
            this.e_11 = new TextBlock();
            this.e_9.Children.Add(this.e_11);
            this.e_11.Name = "e_11";
            this.e_11.Width = 40F;
            this.e_11.TextAlignment = TextAlignment.Right;
            this.e_11.FontSize = 20F;
            DockPanel.SetDock(this.e_11, Dock.Right);
            Binding binding_e_11_Text = new Binding("LevelValue");
            this.e_11.SetBinding(TextBlock.TextProperty, binding_e_11_Text);
            // e_12 element
            this.e_12 = new Slider();
            this.e_9.Children.Add(this.e_12);
            this.e_12.Name = "e_12";
            this.e_12.Minimum = 0F;
            this.e_12.Maximum = 1F;
            Binding binding_e_12_Value = new Binding("LevelValue");
            this.e_12.SetBinding(Slider.ValueProperty, binding_e_12_Value);
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
            FontManager.Instance.AddFont("Segoe UI", 28F, FontStyle.Bold, "Segoe_UI_21_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Regular, "Segoe_UI_15_Regular");
        }
    }
}

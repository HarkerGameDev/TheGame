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
    public partial class OptionsMenu : UIRoot {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private Button e_2;
        
        private StackPanel e_3;
        
        private Button e_4;
        
        private Button e_5;
        
        private Button e_6;
        
        private Button e_7;
        
        public OptionsMenu() : 
                base() {
            this.Initialize();
        }
        
        public OptionsMenu(int width, int height) : 
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
            this.e_1.Text = "Options";
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
            this.e_2.TabIndex = 3;
            this.e_2.FontSize = 20F;
            this.e_2.FontStyle = FontStyle.Bold;
            this.e_2.Content = "Back";
            this.e_2.CommandParameter = "ExitOptions";
            Binding binding_e_2_Command = new Binding("ButtonCommand");
            this.e_2.SetBinding(Button.CommandProperty, binding_e_2_Command);
            // e_3 element
            this.e_3 = new StackPanel();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            Grid.SetRow(this.e_3, 1);
            // e_4 element
            this.e_4 = new Button();
            this.e_3.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.Height = 80F;
            this.e_4.Width = 165F;
            this.e_4.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_4.TabIndex = 2;
            this.e_4.FontSize = 20F;
            this.e_4.FontStyle = FontStyle.Bold;
            this.e_4.Content = "Controls";
            this.e_4.CommandParameter = "Controls";
            Binding binding_e_4_Command = new Binding("ButtonCommand");
            this.e_4.SetBinding(Button.CommandProperty, binding_e_4_Command);
            // e_5 element
            this.e_5 = new Button();
            this.e_3.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Height = 80F;
            this.e_5.Width = 165F;
            this.e_5.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_5.TabIndex = 1;
            this.e_5.FontSize = 20F;
            this.e_5.FontStyle = FontStyle.Bold;
            this.e_5.Content = "Toggle Fullscreen";
            this.e_5.CommandParameter = "Fullscreen";
            Binding binding_e_5_Command = new Binding("ButtonCommand");
            this.e_5.SetBinding(Button.CommandProperty, binding_e_5_Command);
            // e_6 element
            this.e_6 = new Button();
            this.e_3.Children.Add(this.e_6);
            this.e_6.Name = "e_6";
            this.e_6.Height = 80F;
            this.e_6.Width = 165F;
            this.e_6.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_6.TabIndex = 1;
            this.e_6.FontSize = 20F;
            this.e_6.FontStyle = FontStyle.Bold;
            this.e_6.Content = "Toggle VSync";
            this.e_6.CommandParameter = "VSync";
            Binding binding_e_6_Command = new Binding("ButtonCommand");
            this.e_6.SetBinding(Button.CommandProperty, binding_e_6_Command);
            // e_7 element
            this.e_7 = new Button();
            this.e_3.Children.Add(this.e_7);
            this.e_7.Name = "e_7";
            this.e_7.Height = 80F;
            this.e_7.Width = 165F;
            this.e_7.Margin = new Thickness(20F, 20F, 20F, 20F);
            this.e_7.TabIndex = 1;
            this.e_7.FontSize = 20F;
            this.e_7.FontStyle = FontStyle.Bold;
            this.e_7.Content = "Toggle Music";
            this.e_7.CommandParameter = "Music";
            Binding binding_e_7_Command = new Binding("ButtonCommand");
            this.e_7.SetBinding(Button.CommandProperty, binding_e_7_Command);
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
        }
    }
}

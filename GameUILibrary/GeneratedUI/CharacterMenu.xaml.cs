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
    public partial class CharacterMenu : UIRoot {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private Button e_2;
        
        private Button e_3;
        
        private Button e_4;
        
        private Button e_5;
        
        public CharacterMenu() : 
                base() {
            this.Initialize();
        }
        
        public CharacterMenu(int width, int height) : 
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
            this.Background = new SolidColorBrush(new ColorW(255, 255, 255, 0));
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
            this.e_1.TextAlignment = TextAlignment.Center;
            this.e_1.FontSize = 40F;
            this.e_1.FontStyle = FontStyle.Bold;
            Binding binding_e_1_Text = new Binding("PlayerText");
            this.e_1.SetBinding(TextBlock.TextProperty, binding_e_1_Text);
            // e_2 element
            this.e_2 = new Button();
            this.e_0.Children.Add(this.e_2);
            this.e_2.Name = "e_2";
            this.e_2.Height = 80F;
            this.e_2.Width = 165F;
            this.e_2.Focusable = false;
            this.e_2.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_2.VerticalAlignment = VerticalAlignment.Center;
            this.e_2.FontSize = 20F;
            this.e_2.FontStyle = FontStyle.Bold;
            this.e_2.Content = "Back";
            this.e_2.CommandParameter = "Setup";
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
            this.e_3.Content = "Pick Character";
            this.e_3.CommandParameter = "NextPlayer";
            Binding binding_e_3_Command = new Binding("ButtonCommand");
            this.e_3.SetBinding(Button.CommandProperty, binding_e_3_Command);
            // e_4 element
            this.e_4 = new Button();
            this.e_0.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.Height = 80F;
            this.e_4.Width = 165F;
            this.e_4.Focusable = false;
            this.e_4.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_4.VerticalAlignment = VerticalAlignment.Center;
            this.e_4.Content = "Previous";
            this.e_4.CommandParameter = "CharacterLeft";
            Grid.SetRow(this.e_4, 1);
            Binding binding_e_4_Command = new Binding("ButtonCommand");
            this.e_4.SetBinding(Button.CommandProperty, binding_e_4_Command);
            // e_5 element
            this.e_5 = new Button();
            this.e_0.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Height = 80F;
            this.e_5.Width = 165F;
            this.e_5.Focusable = false;
            this.e_5.HorizontalAlignment = HorizontalAlignment.Right;
            this.e_5.VerticalAlignment = VerticalAlignment.Center;
            this.e_5.TabIndex = 2;
            this.e_5.Content = "Next";
            this.e_5.CommandParameter = "CharacterRight";
            Grid.SetRow(this.e_5, 1);
            Binding binding_e_5_Command = new Binding("ButtonCommand");
            this.e_5.SetBinding(Button.CommandProperty, binding_e_5_Command);
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
        }
    }
}

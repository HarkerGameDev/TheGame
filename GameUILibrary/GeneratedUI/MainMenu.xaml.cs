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
    using EmptyKeys.UserInterface.Media;
    using EmptyKeys.UserInterface.Media.Animation;
    using EmptyKeys.UserInterface.Media.Imaging;
    using EmptyKeys.UserInterface.Shapes;
    using EmptyKeys.UserInterface.Renderers;
    using EmptyKeys.UserInterface.Themes;
    
    
    [GeneratedCodeAttribute("Empty Keys UI Generator", "1.11.0.0")]
    public partial class MainMenu : UIRoot {
        
        private Grid e_0;
        
        private Button e_1;
        
        private TextBlock e_2;
        
        private Button e_3;
        
        private TextBlock e_4;
        
        private Button e_5;
        
        private TextBlock e_6;
        
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
            this.e_1 = new Button();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            this.e_1.Height = 80F;
            this.e_1.Width = 160F;
            Grid.SetRow(this.e_1, 2);
            // e_2 element
            this.e_2 = new TextBlock();
            this.e_1.Content = this.e_2;
            this.e_2.Name = "e_2";
            this.e_2.Text = "Start(Press Enter)";
            this.e_2.FontSize = 20F;
            this.e_2.FontStyle = FontStyle.Bold;
            // e_3 element
            this.e_3 = new Button();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.Height = 80F;
            this.e_3.Width = 160F;
            Grid.SetRow(this.e_3, 4);
            // e_4 element
            this.e_4 = new TextBlock();
            this.e_3.Content = this.e_4;
            this.e_4.Name = "e_4";
            this.e_4.Text = "Options";
            this.e_4.FontSize = 20F;
            this.e_4.FontStyle = FontStyle.Bold;
            // e_5 element
            this.e_5 = new Button();
            this.e_0.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Height = 80F;
            this.e_5.Width = 160F;
            this.e_5.FontStyle = FontStyle.Bold;
            Grid.SetRow(this.e_5, 6);
            // e_6 element
            this.e_6 = new TextBlock();
            this.e_5.Content = this.e_6;
            this.e_6.Name = "e_6";
            this.e_6.Text = "Quit";
            this.e_6.FontSize = 20F;
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
            FontManager.Instance.AddFont("Segoe UI", 12F, FontStyle.Bold, "Segoe_UI_9_Bold");
        }
    }
}

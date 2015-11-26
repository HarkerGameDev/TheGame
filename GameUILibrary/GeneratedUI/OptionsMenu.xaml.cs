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
    public partial class OptionsMenu : UIRoot {
        
        private TextBlock e_0;
        
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
            this.e_0 = new TextBlock();
            this.Content = this.e_0;
            this.e_0.Name = "e_0";
            this.e_0.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_0.VerticalAlignment = VerticalAlignment.Top;
            this.e_0.Foreground = new SolidColorBrush(new ColorW(255, 0, 0, 255));
            this.e_0.Text = "Paused\r\nOptions\r\n";
            this.e_0.TextAlignment = TextAlignment.Center;
            this.e_0.FontSize = 40F;
            this.e_0.FontStyle = FontStyle.Bold;
            FontManager.Instance.AddFont("Segoe UI", 40F, FontStyle.Bold, "Segoe_UI_30_Bold");
            FontManager.Instance.AddFont("Segoe UI", 20F, FontStyle.Bold, "Segoe_UI_15_Bold");
            FontManager.Instance.AddFont("Segoe UI", 12F, FontStyle.Bold, "Segoe_UI_9_Bold");
        }
    }
}

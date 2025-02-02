    public class CustomTextBox : TextBox 
    {
        public bool IsEditMode
        {
            get { return (bool)GetValue(IsEditModeProperty); }
            set { SetValue(IsEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(CustomTextBox), new PropertyMetadata(false));

        public bool IsNumeric
        {
            get { return (bool)GetValue(IsNumericProperty); }
            set { SetValue(IsNumericProperty, value); }
        }
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.Register("IsNumeric", typeof(bool), typeof(CustomTextBox), new PropertyMetadata(false));

        private string _orgText = string.Empty; 
        public CustomTextBox()
        {
            this.KeyDown += (s, e) => 
            {
                if (e.Key == System.Windows.Input.Key.Enter && this.IsEditMode)
                {
                    _orgText = this.Text;
                    this.IsEditMode = false;
                    var be = this.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    this.Text = _orgText;
                    this.IsEditMode = false;
                    this.CaretIndex=this.Text.Length;
                }
                else
                {
                    if (this.IsEditMode == false) 
                    {
                        this.IsEditMode = true;
                    }
                    this.IsNumeric = double.TryParse(this.Text.Replace(',', '.'), CultureInfo.InvariantCulture, out double d);
                }
            };
        }
    }

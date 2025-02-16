using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UserControlLibrary
{
    public enum MouseWheelDeltaDirectionTypes
    { 
        UpIncrease, UpDecrease
    }
    
    public class NumericTextBox : TextBox
    {
        private bool suppressUpdate = false;
        #region Properties
        public double DeltaMinor { get; set; } = 0.1;
        public double DeltaMajor { get; set; } = 1.0;
        public double Minimum { get; set; } = double.MinValue;
        public double Maximum { get; set; } = double.MaxValue;
        public int Decimals { get; set; } = 0;
        public string DecimalSeparator { get; set; } = ".";
        public bool EnableUpdateValueOutOfRange { get; set; } = true;
        public bool EnableScrollWheelChange { get; set; } = true;
        public MouseWheelDeltaDirectionTypes MouseWheelDeltaDirection { get; set; } = MouseWheelDeltaDirectionTypes.UpIncrease;
        public string LatestEvent
        {
            get { return (string)GetValue(LatestEventProperty); }
            set { SetValue(LatestEventProperty, value); }
        }
        public static readonly DependencyProperty LatestEventProperty =
            DependencyProperty.Register("LatestEvent", typeof(string), typeof(NumericTextBox), new PropertyMetadata(""));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumericTextBox), new PropertyMetadata(0.0, OnValueUpdate));

        private static void OnValueUpdate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as NumericTextBox;
            if (tb == null) 
                return;
            if (tb.suppressUpdate == true)
            {
                tb.suppressUpdate = false;
                return;
            }

            var val = (double)tb.Value;
            val = Math.Round(val, tb.Decimals);
            tb.Text = string.Format(tb.nfi, "{0:f}", val);
            tb.CaretIndex = tb.Text.Length;
        }

        public bool InRange
        {
            get { return (bool)GetValue(InRangeProperty); }
            set { SetValue(InRangeProperty, value); }
        }
        public static readonly DependencyProperty InRangeProperty =
            DependencyProperty.Register("InRange", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));
        #endregion

        NumberFormatInfo nfi = new NumberFormatInfo();
        public NumericTextBox()
        {
            this.Initialized += (s, e) => 
            {
                nfi = new NumberFormatInfo
                {
                    NumberDecimalDigits = Decimals,
                    NumberDecimalSeparator = DecimalSeparator
                };
                var val = Math.Round(Value, Decimals);
                Text = string.Format(nfi, "{0:f}", val);
                CaretIndex = Text.Length;
            };
            this.PreviewKeyDown += NumericTextBox_PreviewKeyDown;
            this.MouseWheel += NumericTextBox_MouseWheel;
            this.TextChanged += NumericTextBox_TextChanged;
        }

        private void NumericTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Value == double.NaN) 
                return;
            if (EnableScrollWheelChange == false) 
                return;   

            if (e.Delta > 0)  // scroll up
            {
                if (MouseWheelDeltaDirection == MouseWheelDeltaDirectionTypes.UpIncrease)
                {
                    // increase
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        SetValue(Value, DeltaMajor);
                    else
                        SetValue(Value, DeltaMinor);
                }
                else
                {
                    // decrease
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        SetValue(Value, -DeltaMajor);
                    else
                        SetValue(Value, -DeltaMinor);
                }
            }
            else // scroll down
            {
                if (MouseWheelDeltaDirection == MouseWheelDeltaDirectionTypes.UpDecrease)
                {
                    // increase
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        SetValue(Value, DeltaMajor);
                    else
                        SetValue(Value, DeltaMinor);
                }
                else
                {
                    // decrease
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        SetValue(Value, -DeltaMajor);
                    else
                        SetValue(Value, -DeltaMinor);
                }
            }
        }

        private void NumericTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null)
                return;

            switch (e.Key)
            {
                case Key.Up: 
                    {
                        if (Decimals == 0 & DeltaMinor < 1) // no update 
                        {
                            e.Handled = true;
                            return;
                        }

                        SetValue(Value, DeltaMinor);
                        e.Handled = true;
                    } break;
                case Key.Down: 
                    {
                        if (Decimals == 0 & DeltaMinor < 1) // no update 
                        {
                            e.Handled = true;
                            return;
                        }

                        SetValue(Value, -DeltaMinor);
                        e.Handled = true;
                    } break;
                case Key.PageUp:
                    {
                        if (Decimals == 0 & DeltaMajor < 1) // no update 
                        {
                            e.Handled = true;
                            return;
                        }

                        SetValue(Value, DeltaMajor);
                    } break;
                case Key.PageDown: 
                    {
                        if (Decimals == 0 & DeltaMajor < 1) // no update 
                        {
                            e.Handled = true;
                            return;
                        }
                        
                        SetValue(Value, -DeltaMajor);
                    } break;
                case Key.Delete: { } break;
                case Key.Back: { } break;
                case Key.OemMinus:
                case Key.Subtract:
                    {
                        if (tb.CaretIndex != 0) // disable minus character if index is not the first
                        {
                            e.Handled = true;
                            return;
                        }
                        if (Minimum >= 0) // disable minus character if non negative values are allowed
                        {
                            e.Handled = true;
                            return;
                        }
                    } break;
                case Key.Left:
                    {
                        if (this.CaretIndex > 0)
                            this.CaretIndex -= 1;
                    }break;
                case Key.Right:
                    { 
                        if (this.CaretIndex < this.Text.Length)
                            this.CaretIndex += 1;
                    }
                    break;
                case Key.Home:
                case Key.End:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.OemComma:
                case Key.Decimal:
                case Key.OemPeriod: 
                    {
                        if (Decimals <= 0) // disable period character if non float values are accepted
                        {
                            e.Handled = true;
                            return;
                        }
                        else if (Text.Contains(",") || Text.Contains(".")) // disable additional comma separators
                        {
                            e.Handled = true;
                            return;
                        }
                    } break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    {
                        // Limit amout of decimals 
                        if (Decimals > 0)
                        {
                            var split = this.Text.Replace(",", ".").Split(".");
                            if (split.Length > 1) // decimal part exist
                            {
                                var indexOfPeriod = Text.Replace(",", ".").IndexOf(".");
                                var caretIsBehindPeriod = CaretIndex > indexOfPeriod;
                                if (split[1].Length >= Decimals &&
                                    caretIsBehindPeriod == true) // disable writing of more decimal characters
                                {
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }
                    }
                    break;
                default:
                    {
                        // ignore other keys!
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            suppressUpdate = true;

            if (double.TryParse(Text.Replace(",", "."), CultureInfo.InvariantCulture, out var d))
            {
                SetValue(d, 0);
            }
        }
        private void SetValue(double newValue)
        {
            void Set(double localValue)
            {
                Value = Math.Round(localValue, Decimals);
                InRange = Value <= Maximum && Value >= Minimum;
            }
            if (EnableUpdateValueOutOfRange == false) // cap values
            {
                if (newValue > Maximum) // set Value to maximun if over limit
                {
                    suppressUpdate = false;
                    Set(Maximum);
                }
                else if (newValue < Minimum) // set Value to minimum if over limit
                {
                    suppressUpdate = false;
                    Set(Minimum);
                }
                else
                {
                    Set(newValue);
                }
            }
            else
            {
                Set(newValue);
            }
        }
        private void SetValue(double newValue, double delta)
        {
            SetValue(newValue + delta);
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DateTimePicker
{
    /// <summary>
    /// Interaction logic for DateTimePickerControl.xaml
    /// </summary>
    public partial class DateTimePickerControl : UserControl
    {
        private const int FormatLengthOfLast = 2;
        private enum Direction : int
        {
            Previous = -1,
            Next = 1
        }

        public DateTimePickerControl()
        {
            InitializeComponent();
            DateDisplay.Text = DateTime.Now.ToString(DateFormat);
        }

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register("ButtonStyle", 
                typeof(Style), 
                typeof(DateTimePickerControl), 
                new PropertyMetadata(null));

        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        public DateTime? SelectedDate
        {
            get
            {
                return (DateTime)GetValue(SelectedDateProperty);
            }

            set
            {
                SetValue(SelectedDateProperty, value);
            }
        }

        public string? DateFormat
        {
            get
            {
                return System.Convert.ToString(GetValue(DateFormatProperty));
            }

            set
            {
                SetValue(DateFormatProperty, value);
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return (DateTime)GetValue(MinimumDateProperty);
            }
            set
            {
                SetValue(MinimumDateProperty, value);
            }
        }

        public DateTime MaximumDate
        {
            get
            {
                return (DateTime)GetValue(MaximumDateProperty);
            }
            set
            {
                SetValue(MaximumDateProperty, value);
            }
        }

        public event RoutedEventHandler DateChanged
        {
            add
            {
                AddHandler(DateChangedEvent, value);
            }
            remove
            {
                RemoveHandler(DateChangedEvent, value);
            }
        }

        private void OnDateChanged(System.Object sender, System.EventArgs e)
        {
        }

        public static readonly RoutedEvent DateChangedEvent =
            EventManager.RegisterRoutedEvent("DateChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(DateTimePickerControl));


        public event RoutedEventHandler DateFormatChanged
        {
            add
            {
                this.AddHandler(DateFormatChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(DateFormatChangedEvent, value);
            }
        }

        private void OnDateFormatChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        public static readonly RoutedEvent DateFormatChangedEvent =
            EventManager.RegisterRoutedEvent("DateFormatChanged",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                typeof(DateTimePickerControl));

        public static readonly DependencyProperty DateFormatProperty =
            DependencyProperty.Register("DateFormat", typeof(string),
                typeof(DateTimePickerControl),
                new FrameworkPropertyMetadata("yyyy-MM-dd HH:mm", OnDateFormatChanged),
                new ValidateValueCallback(ValidateDateFormat));

        public static DependencyProperty MaximumDateProperty =
            DependencyProperty.Register("MaximumDate",
                typeof(DateTime),
                typeof(DateTimePickerControl),
                new FrameworkPropertyMetadata(DateTime.MaxValue, null, new CoerceValueCallback(CoerceMaxDate)));

        public static DependencyProperty MinimumDateProperty =
            DependencyProperty.Register("MinimumDate",
                typeof(DateTime),
                typeof(DateTimePickerControl),
                new FrameworkPropertyMetadata(DateTime.MinValue, null, new CoerceValueCallback(CoerceMinDate)));

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate",
                typeof(Nullable<DateTime>),
                typeof(DateTimePickerControl),
                new FrameworkPropertyMetadata(
                    DateTime.Now,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnSelectedDateChanged),
                    new CoerceValueCallback(CoerceDate)));

        //public static readonly DependencyProperty TextProperty = 
        //    DependencyProperty.Register("Text",
        //    typeof(string),
        //    typeof(DateTimeTextBoxUserControl),
        //    new FrameworkPropertyMetadata(
        //        default(string),
        //        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //        new PropertyChangedCallback(OnTextChangedCallBack)
        //    )
        //);

        private static DateTime GetDateOrDefault(Object o, DateTime defaultValue)
        {
            if (o is DateTime dt)
            {
                return dt;
            }
            else if (DateTime.TryParse(o?.ToString(), out dt))
            {
                return dt;
            }
            else
            {
                return defaultValue;
            }
        }

        private void CalendarDisplay_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PopUpCalendarButton.IsChecked = false;
            SelectedDate = CalendarDisplay.SelectedDate!.Value;
        }

        private void DateDisplay_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectionStart = DateDisplay.SelectionStart;
            FocusOnDatePart(selectionStart);
        }

        private void DateDisplay_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime dt = GetDateOrDefault(DateDisplay.Text, DateTime.MinValue);

            while ((dt == DateTime.MinValue || (dt < MinimumDate) || (dt > MaximumDate)) && DateDisplay.CanUndo)
                DateDisplay.Undo();

            if (dt == DateTime.MinValue && SelectedDate != dt)
                SelectedDate = dt;
        }

        private void DateTimePicker_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DateTime dt = GetDateOrDefault(DateDisplay.Text, DateTime.MinValue);
            var selectionStart = DateDisplay.SelectionStart;
            while (dt == DateTime.MinValue)
                DateDisplay.Undo();

            e.Handled = true;
            switch (e.Key)
            {
                case Key.Up:
                {
                    SelectedDate = Increase(selectionStart, 1);
                    FocusOnDatePart(selectionStart);
                    break;
                }

                case Key.Down:
                {
                    SelectedDate = Increase(selectionStart, -1);
                    FocusOnDatePart(selectionStart);
                    break;
                }

                case Key.Left:
                {
                    selectionStart = SelectPreviousPosition(selectionStart);
                    if (selectionStart > -1)
                        FocusOnDatePart(selectionStart);
                    else
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    break;
                }

                case Key.Right:
                case Key.Tab:
                {
                    selectionStart = SelectNextPosition(selectionStart);
                    if (selectionStart > -1)
                        FocusOnDatePart(selectionStart);
                    else
                        PopUpCalendarButton.Focus();
                    break;
                }

                default:
                {
                    if (!char.IsDigit(System.Convert.ToChar(e.KeyboardDevice.ToString)))
                    {
                        if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9)
                            e.Handled = false;
                    }

                    break;
                }
            }
        }

        private static object CoerceDate(DependencyObject d, object value)
        {
            DateTimePickerControl dtp = (DateTimePickerControl)d;
            DateTime current = value is DateTime ? (DateTime)value : DateTime.Now;
            if (current < dtp.MinimumDate)
                current = dtp.MinimumDate;
            if (current > dtp.MaximumDate)
                current = dtp.MaximumDate;

            return current;
        }

        private static object CoerceMinDate(DependencyObject d, object value)
        {
            DateTimePickerControl dtp = (DateTimePickerControl)d;
            DateTime current = (DateTime)value;
            if (current >= dtp.MaximumDate)
                throw new ArgumentException("MinimumDate can not be equal to, or more than maximum date");
            if (current > dtp.SelectedDate)
                dtp.SelectedDate = current;

            return current;
        }

        private static object CoerceMaxDate(DependencyObject d, object value)
        {
            DateTimePickerControl dtp = (DateTimePickerControl)d;
            DateTime current = (DateTime)value;
            if (current <= dtp.MinimumDate)
                throw new ArgumentException("MaximumDate can not be equal to, or less than MinimumDate");
            if (current < dtp.SelectedDate)
                dtp.SelectedDate = current;

            return current;
        }

        public static void OnDateFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dtp = (DateTimePickerControl)obj;
            dtp.DateDisplay.Text = string.Format(dtp.DateFormat, dtp.SelectedDate);
        }

        public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dtp = (DateTimePickerControl)obj;
            if (args.NewValue == null)
            {
                dtp.SelectedDate = (DateTime?)args.NewValue;
                dtp.DateDisplay.Text = "Date not set";
            }
            else
            {
                DateTime dt = (DateTime)args.NewValue;
                dtp.DateDisplay.Text = dt.ToString(dtp.DateFormat);
                dtp.CalendarDisplay.SelectedDate = dt;
                dtp.CalendarDisplay.DisplayDate = dt;
            }
        }

        public static bool ValidateDateFormat(object par)
        {
            var dateFormat = par as string;
            if(dateFormat == null)
                return false;

            if (string.IsNullOrWhiteSpace(dateFormat))
            {
                return false;
            }

            try
            {
                DateTime.Now.ToString(dateFormat);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        private int SelectNextPosition(int selectionStart)
        {
            return SelectPosition(selectionStart, Direction.Next);
        }

        private int SelectPreviousPosition(int selectionStart)
        {
            return SelectPosition(selectionStart, Direction.Previous);
        }

        // Selects next or previous date value, depending on the incrementor value  
        // Alternatively moves focus to previous control or the calendar button
        private int SelectPosition(int selectionStart, Direction direction)
        {
            int returnValue = 0;

            if ((selectionStart > 0 || direction == DateTimePickerControl.Direction.Next) && (selectionStart < DateFormat.Length - FormatLengthOfLast || direction == DateTimePickerControl.Direction.Previous))
            {
                char firstCharacter = System.Convert.ToChar(DateFormat.Substring(selectionStart, 1));
                char nextCharacter = System.Convert.ToChar(DateFormat.Substring(selectionStart + (int)direction, 1));
                bool found = false;

                while (((nextCharacter == firstCharacter || !char.IsLetter(nextCharacter)) && (selectionStart > 1 || direction > 0) && (selectionStart < DateFormat.Length - 2 || direction == DateTimePickerControl.Direction.Previous)))
                {
                    selectionStart += (int)direction;
                    nextCharacter = System.Convert.ToChar(DateFormat.Substring(selectionStart + (int)direction, 1));
                }

                if (selectionStart > 1)
                    found = true;
                selectionStart = DateFormat.IndexOf(nextCharacter);

                if (found)
                    returnValue = selectionStart;
            }
            else
                returnValue = -1;

            return returnValue;
        }

        private void FocusOnDatePart(int selectionStart)
        {
            if (selectionStart > DateFormat.Length - 1)
                selectionStart = DateFormat.Length - 1;
            char firstCharacter = System.Convert.ToChar(DateFormat.Substring(selectionStart, 1));

            selectionStart = DateFormat.IndexOf(firstCharacter);
            int selectionLength = Math.Abs((selectionStart - (DateFormat.LastIndexOf(firstCharacter) + 1)));
            DateDisplay.Focus();
            DateDisplay.Select(selectionStart, selectionLength);
        }

        private DateTime Increase(int selectionStart, int value)
        {
            DateTime returnValue = DateTime.MinValue;
            DateTime.TryParse(DateDisplay.Text, out returnValue);

            try
            {
                switch (DateFormat.Substring(selectionStart, 1))
                {
                    case "h":
                    case "H":
                        {
                            returnValue = returnValue.AddHours(value);
                            break;
                        }

                    case "y":
                        {
                            returnValue = returnValue.AddYears(value);
                            break;
                        }

                    case "M":
                        {
                            returnValue = returnValue.AddMonths(value);
                            break;
                        }

                    case "m":
                        {
                            returnValue = returnValue.AddMinutes(value);
                            break;
                        }

                    case "d":
                        {
                            returnValue = returnValue.AddDays(value);
                            break;
                        }

                    case "s":
                        {
                            returnValue = returnValue.AddSeconds(value);
                            break;
                        }
                }
            }
            catch (ArgumentException)
            {
                throw;
            }

            return returnValue;
        }
    }
}
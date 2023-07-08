using Microsoft.VisualBasic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DateTimePicker
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
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

            // This call is required by the designer.
            InitializeComponent();
        }

        public Nullable<DateTime> SelectedDate
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

        public string DateFormat
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
        void OnDateChanged(System.Object sender, System.EventArgs e)
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

        void OnDateFormatChanged(object sender, System.Windows.RoutedEventArgs e)
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
                new FrameworkPropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnSelectedDateChanged),
                    new CoerceValueCallback(CoerceDate)));

        static DateTime GetDateOrDefault(Object o, DateTime defaultValue)
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

        private void CalDisplay_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PopUpCalendarButton.IsChecked = false;
            SelectedDate = CalDisplay.SelectedDate!.Value;
        }

        private void DateDisplay_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selstart = DateDisplay.SelectionStart;
            FocusOnDatePart(selstart);
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

            var selstart = DateDisplay.SelectionStart;
            while (dt == DateTime.MinValue)
                DateDisplay.Undo();

            e.Handled = true;
            switch (e.Key)
            {
                case Key.Up:
                    {
                        SelectedDate = Increase(selstart, 1);
                        FocusOnDatePart(selstart);
                        break;
                    }

                case Key.Down:
                    {
                        SelectedDate = Increase(selstart, -1);
                        FocusOnDatePart(selstart);
                        break;
                    }

                case Key.Left:
                    {
                        selstart = SelectPreviousPosition(selstart);
                        if (selstart > -1)
                            FocusOnDatePart(selstart);
                        else
                            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        break;
                    }

                case Key.Right:
                case Key.Tab:
                    {
                        selstart = SelectNextPosition(selstart);
                        if (selstart > -1)
                            FocusOnDatePart(selstart);
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
            DateTimePickerControl dtpicker = (DateTimePickerControl)d;
            DateTime current = (DateTime)value;
            if (current < dtpicker.MinimumDate)
                current = dtpicker.MinimumDate;
            if (current > dtpicker.MaximumDate)
                current = dtpicker.MaximumDate;
            return current;
        }

        private static object CoerceMinDate(DependencyObject d, object value)
        {
            DateTimePickerControl dtpicker = (DateTimePickerControl)d;
            DateTime current = (DateTime)value;
            if (current >= dtpicker.MaximumDate)
                throw new ArgumentException("MinimumDate can not be equal to, or more than maximum date");
            if (current > dtpicker.SelectedDate)
                dtpicker.SelectedDate = current;
            return current;
        }

        private static object CoerceMaxDate(DependencyObject d, object value)
        {
            DateTimePickerControl dtpicker = (DateTimePickerControl)d;
            DateTime current = (DateTime)value;
            if (current <= dtpicker.MinimumDate)
                throw new ArgumentException("MaximimumDate can not be equal to, or less than MinimumDate");
            if (current < dtpicker.SelectedDate)
                dtpicker.SelectedDate = current;
            return current;
        }

        public static void OnDateFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dtp = (DateTimePickerControl)obj;

            //TODO: VB
            dtp.DateDisplay.Text = Strings.Format(dtp.SelectedDate, dtp.DateFormat);
        }

        public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dtp = (DateTimePickerControl)obj;

            if (args.NewValue == null)
            {
                dtp.SelectedDate = (DateTime?)args.NewValue;

                dtp.DateDisplay.Text = "datum ej satt...";
            }
            else
            {
                DateTime dt = (DateTime)args.NewValue;
                dtp.DateDisplay.Text = dt.ToString(dtp.DateFormat);
                dtp.CalDisplay.SelectedDate = dt;
                dtp.CalDisplay.DisplayDate = dt;
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

        private int SelectNextPosition(int selstart)
        {
            return SelectPosition(selstart, Direction.Next);
        }

        private int SelectPreviousPosition(int selstart)
        {
            return SelectPosition(selstart, Direction.Previous);
        }

        // Selects next or previous date value, depending on the incrementor value  
        // Alternatively moves focus to previous control or the calender button
        private int SelectPosition(int selstart, Direction direction)
        {
            int retval = 0;

            if ((selstart > 0 || direction == DateTimePickerControl.Direction.Next) && (selstart < DateFormat.Length - FormatLengthOfLast || direction == DateTimePickerControl.Direction.Previous))
            {
                char firstchar = System.Convert.ToChar(DateFormat.Substring(selstart, 1));
                char nextchar = System.Convert.ToChar(DateFormat.Substring(selstart + (int)direction, 1));
                bool found = false;

                while (((nextchar == firstchar || !char.IsLetter(nextchar)) && (selstart > 1 || direction > 0) && (selstart < DateFormat.Length - 2 || direction == DateTimePickerControl.Direction.Previous)))
                {
                    selstart += (int)direction;
                    nextchar = System.Convert.ToChar(DateFormat.Substring(selstart + (int)direction, 1));
                }

                if (selstart > 1)
                    found = true;
                selstart = DateFormat.IndexOf(nextchar);

                if (found)
                    retval = selstart;
            }
            else
                retval = -1;

            return retval;
        }

        private void FocusOnDatePart(int selstart)
        {
            if (selstart > DateFormat.Length - 1)
                selstart = DateFormat.Length - 1;
            char firstchar = System.Convert.ToChar(DateFormat.Substring(selstart, 1));

            selstart = DateFormat.IndexOf(firstchar);
            int sellength = Math.Abs((selstart - (DateFormat.LastIndexOf(firstchar) + 1)));
            DateDisplay.Focus();
            DateDisplay.Select(selstart, sellength);
        }

        private DateTime Increase(int selstart, int value)
        {
            DateTime retval = DateTime.MinValue;
            DateTime.TryParse(DateDisplay.Text, out retval);

            try
            {
                switch (DateFormat.Substring(selstart, 1))
                {
                    case "h":
                    case "H":
                        {
                            retval = retval.AddHours(value);
                            break;
                        }

                    case "y":
                        {
                            retval = retval.AddYears(value);
                            break;
                        }

                    case "M":
                        {
                            retval = retval.AddMonths(value);
                            break;
                        }

                    case "m":
                        {
                            retval = retval.AddMinutes(value);
                            break;
                        }

                    case "d":
                        {
                            retval = retval.AddDays(value);
                            break;
                        }

                    case "s":
                        {
                            retval = retval.AddSeconds(value);
                            break;
                        }
                }
            }
            catch (ArgumentException ex)
            {
            }

            return retval;
        }
    }
}
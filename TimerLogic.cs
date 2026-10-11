using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace DOTA2RunesTimer
{
    public partial class TimerLogic : ObservableObject
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        
        // Содержимое кнопки старта или паузы
        private string _startOrPause = "START";
        public string StartOrPause
        {
            get { return _startOrPause; }
            set => SetProperty(ref _startOrPause, value);
        }

        // Название текущего события
        private string _currentEvent = "";
        public string CurrentEvent
        {
            get { return _currentEvent; }
            set => SetProperty(ref _currentEvent, value);
        }

        // Название следующего события
        private string _nextEvent = "";
        public string NextEvent
        {
            get { return _nextEvent; }
            set => SetProperty(ref _nextEvent, value);
        }

        // Игровое время
        private TimeSpan _inGameTime = TimeSpan.FromSeconds(-30);
        public TimeSpan InGameTime
        {
            get { return _inGameTime; }
            set
            {
                if (SetProperty(ref _inGameTime, value))
                {
                    UpdateTimeString();
                    UpdateTimings();
                }
            }
        }

        // Игровое время (строка)
        private string _inGameTimeString = "00:00";
        public string InGameTimeString
        {
            get { return _inGameTimeString; }
            set => SetProperty(ref _inGameTimeString, value);
        }

        // Обратный отсчет
        private TimeSpan _сountdown = TimeSpan.Zero;
        public TimeSpan Countdown
        {
            get { return _сountdown; }
            set => SetProperty(ref _сountdown, value);
        }

        // Обратный отсчет (строка)
        private string _сountdownString = "00:00";
        public string CountdownString
        {
            get { return _сountdownString; }
            set => SetProperty(ref _сountdownString, value);
        }

        // Флаг таймера и проверка доступности кнопок + -
        private bool _timerIsRunning;
        public bool TimerIsRunning
        {
            get => _timerIsRunning;
            set
            {
                if (SetProperty(ref _timerIsRunning, value))
                {
                    AddSecondCommand.NotifyCanExecuteChanged();
                    RemoveSecondCommand.NotifyCanExecuteChanged();
                }
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Режим игры
        private static bool _isAllPick;

        // Размер кэша (2 - максимальная длительность игры в часах)
        private const int CacheSize = 2 * 60 * 60 + 1;

        // Кэш событий
        private static readonly Dictionary<int, string> _eventCache = new();
        private static readonly List<int> _eventKeys = new();

        // Список событий
        private static string? Rules(int currentSeconds, bool IsAllPick)
        {
            if (currentSeconds == 0) return "BOUNTY";
            if (currentSeconds == 120) return "WATER";
            if (currentSeconds == 240) return "BOUNTY WATER";

            int interval = IsAllPick ? 60 : 30;
            if (currentSeconds % interval != 0) return null;

            var returnedString = new List<string>();

            if (currentSeconds % 240 == 0) returnedString.Add("BOUNTY");
            if (currentSeconds % 420 == 0) returnedString.Add("WISDOM");
            if (currentSeconds % (IsAllPick ? 180 : 90) == 0) returnedString.Add("LOTUS");
            if (currentSeconds % 120 == 0 && currentSeconds >= 360) returnedString.Add("POWER");
            
            if (IsAllPick && currentSeconds == 1200) returnedString.Add("TORMENTOR");
            if (!IsAllPick && currentSeconds == 600) returnedString.Add("TORMENTOR");

            if (returnedString.Count == 0) return null;

            return string.Join(" ", returnedString);
        }
        private static void CreateCache()
        {
            for (int i = 0; i < CacheSize; i += 30)
            {
                string? result = Rules(i, _isAllPick);

                if (!string.IsNullOrEmpty(result))
                {
                    _eventCache[i] = result;
                    _eventKeys.Add(i);
                }
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Таймер
        private System.Timers.Timer _timer;

        // Ссылка на элементы главной страницы
        private readonly MainWindow _mainWindow;

        public TimerLogic(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            // Первоначальная настройка таймера
            _timer = new System.Timers.Timer
            {
                Interval = 1000
            };
            _timer.Elapsed += (sender, e) =>
            {
                InGameTime += TimeSpan.FromSeconds(1);
            };

            UpdateTimings(); // Первоначальный расчет
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        private void UpdateTimings()
        {
            int currentSecond = (int)InGameTime.TotalSeconds;

            int currentEventKey = -1;
            int nextEventKey = -1;

            foreach (int key in _eventKeys)
            {
                if (key <= currentSecond)
                    currentEventKey = key;
                else
                {
                    nextEventKey = key;
                    break;
                }
            }

            CurrentEvent = currentEventKey >= 0 ? _eventCache[currentEventKey] : string.Empty;
            NextEvent = nextEventKey >= 0 ? _eventCache[nextEventKey] : string.Empty;
            Countdown = nextEventKey >= 0
                ? TimeSpan.FromSeconds(nextEventKey - currentSecond)
                : TimeSpan.Zero;

            UpdateCountdownString();
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Режим Turbo
        [RelayCommand]
        public void TurboMode()
        {
            _isAllPick = false;

            CreateCache();

            _mainWindow.BannerGameMode.Visibility = Visibility.Collapsed;
            _mainWindow.TBGamemode.Visibility = Visibility.Collapsed;
            _mainWindow.BUTurbo.Visibility = Visibility.Collapsed;
            _mainWindow.BUAllPick.Visibility = Visibility.Collapsed;
        }

        // Режим AllPick
        [RelayCommand]
        public void AllPickMode()
        {
            _isAllPick = true;

            CreateCache();

            _mainWindow.BannerGameMode.Visibility = Visibility.Collapsed;
            _mainWindow.TBGamemode.Visibility = Visibility.Collapsed;
            _mainWindow.BUTurbo.Visibility = Visibility.Collapsed;
            _mainWindow.BUAllPick.Visibility = Visibility.Collapsed;
        }

        // Старт или пауза
        [RelayCommand]
        public void StartPause()
        {
            if (!_timerIsRunning)
            {
                _timer.Start();
                TimerIsRunning = true;
                StartOrPause = "PAUSE";

                _mainWindow.TextPause.Visibility = Visibility.Collapsed;
                _mainWindow.BannerPause.Visibility = Visibility.Collapsed;
            }
            else
            {
                _timer.Stop();
                TimerIsRunning = false;
                StartOrPause = "START";

                _mainWindow.TextPause.Visibility = Visibility.Visible;
                _mainWindow.BannerPause.Visibility = Visibility.Visible;
            }
        }

        // Сброс
        [RelayCommand]
        public void Reset()
        {
            _timer.Stop();
            TimerIsRunning = false;

            InGameTime = TimeSpan.FromSeconds(-30);
            InGameTimeString = "00:00";

            Countdown = TimeSpan.Zero;
            CountdownString = "00:00";

            StartOrPause = "START";

            _mainWindow.TextPause.Visibility = Visibility.Collapsed;
            _mainWindow.BannerPause.Visibility = Visibility.Collapsed;
        }

        // Проверка на дооступность кнопок (если TimerIsRunning false кнопки недоступны)
        private bool CanModify() => TimerIsRunning;

        // Кнопка добавления секунды
        [RelayCommand(CanExecute = nameof(CanModify))]
        public void AddSecond()
        {
            InGameTime += TimeSpan.FromSeconds(1);

            UpdateTimeString();
            UpdateCountdownString();
        }

        // Кнопка вычитания секунды
        [RelayCommand(CanExecute = nameof(CanModify))]
        public void RemoveSecond()
        {
            InGameTime -= TimeSpan.FromSeconds(1);
        }

        private void UpdateTimeString()
        {
            int totalMinutes = (int)Math.Floor(Math.Abs(InGameTime.TotalMinutes));
            int seconds = Math.Abs(InGameTime.Seconds);

            string formatted = $"{totalMinutes:D2}:{seconds:D2}";

            if (InGameTime < TimeSpan.Zero)
            {
                InGameTimeString = $"-{formatted}";
            }
            else
            {
                InGameTimeString = formatted;
            }
        }

        private void UpdateCountdownString()
        {
            int totalSeconds = (int)Math.Abs(Countdown.TotalSeconds);

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            CountdownString = Countdown < TimeSpan.Zero
                ? $"-{minutes:D2}:{seconds:D2}"
                : $"{minutes:D2}:{seconds:D2}";
        }
    }
}

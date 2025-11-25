using coach_search.DB;
using coach_search.Models;
using coach_search.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace coach_search.ViewModels
{
    public class ScheduleSlot : BaseViewModel
    {
        public int DayOfWeek { get; set; }
        public string Time { get; set; }
        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set { _isAvailable = value; if (value) color = "White"; OnPropertyChanged(nameof(IsAvailable)); }
        }

        private bool _isPending;
        public bool IsPending
        {
            get => _isPending;
            set { _isPending = value; if (value) color = "Gray"; OnPropertyChanged(nameof(IsPending)); }
        }

        private bool _isBooked;
        public bool IsBooked
        {
            get => _isBooked;
            set { _isBooked = value; if(value) color = "Orange"; OnPropertyChanged(nameof(IsBooked)); }
        }
        private string color = "White";
        public string Color
        {
            get => color;
            set { color = value; OnPropertyChanged(nameof(Color)); }
        }
        public ICommand BookCommand { get; set; }
    }

    public class TutorScheduleViewModel : BaseViewModel
    {
        private ObservableCollection<ScheduleSlot> _scheduleSlots;
        private bool _isLoading = false;
        private bool _isInitializing = false;
        private int? _currentTutorId = null;
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        
        public ObservableCollection<ScheduleSlot> ScheduleSlots
        {
            get => _scheduleSlots;
            set
            {
                _scheduleSlots = value;
                OnPropertyChanged(nameof(ScheduleSlots));
            }
        }

        public ObservableCollection<string> DaysOfWeek { get; } = new ObservableCollection<string>
        {
            "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"
        };

        public ObservableCollection<string> TimeSlots { get; } = new ObservableCollection<string>
        {
            "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00"
        };

        public TutorScheduleViewModel()
        {
            ScheduleSlots = new ObservableCollection<ScheduleSlot>(Enumerable.Range(0, 70).Select(_ => new ScheduleSlot()));
        }

        public async Task LoadScheduleAsync(int? tutorId)
        {
            // Предотвращаем повторную инициализацию, если уже идет загрузка
            if (_isInitializing)
            {
                return;
            }

            _isInitializing = true;
            IsLoading = true;
            _currentTutorId = tutorId;

            try
            {
                if (!tutorId.HasValue)
                {
                    // Если нет ID, просто показываем пустое расписание
                    IsLoading = false;
                    _isInitializing = false;
                    return;
                }

                // Используем отдельный контекст для избежания конфликтов
                using var context = new DB.Context();
                using var unitOfWork = new UnitOfWork(context);
                var existingAppointments = await unitOfWork.Appointments.GetTutorAppointmentsAsync(tutorId.Value);

                // Обновляем слоты
                int index = 0;
                foreach (var time in TimeSlots)
                {
                    for (int day = 0; day <= 6; day++)
                    {
                        var appointment = existingAppointments.FirstOrDefault(a => a.DayOfWeek == day && a.Time == time);

                        bool isAvailable = appointment == null || appointment.Status == 2;
                        bool isPending = appointment?.Status == 0;
                        bool isBooked = appointment?.Status == 1;

                        var slot = ScheduleSlots[index];
                        slot.DayOfWeek = day;
                        slot.Time = time;
                        slot.IsAvailable = isAvailable;
                        slot.IsPending = isPending;
                        slot.IsBooked = isBooked;
                        slot.BookCommand = new AsyncRelayCommand(async param => await BookSlot(slot));

                        index++;
                    }
                }

                OnPropertyChanged(nameof(ScheduleSlots));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки расписания: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                _isInitializing = false;
            }
        }

        private async Task BookSlot(ScheduleSlot slot)
        {
            if (!slot.IsAvailable || ApplicationContext.CurrentUser == null || ApplicationContext.CurrentTutorId == null)
                return;

            if (ApplicationContext.CurrentUser.Role != 0)
            {
                return;
            }

            // Показать окно для комментария
            var commentWindow = new Views.BookingCommentWindow();
            bool? dialogResult = commentWindow.ShowDialog();
            
            // Если нажали OK (DialogResult == true), создаем запись
            if (dialogResult == true)
            {
                var appointment = new Appointment
                {
                    TutorId = ApplicationContext.CurrentTutorId.Value,
                    StudentId = ApplicationContext.CurrentUser.Id,
                    DayOfWeek = slot.DayOfWeek,
                    Time = slot.Time,
                    Comment = commentWindow.CommentText ?? string.Empty,
                    Status = 0 // pending
                };

                using var context = new DB.Context();
                using var unitOfWork = new UnitOfWork(context);

                appointment.Status = 0;
                await unitOfWork.Appointments.AddAsync(appointment);

                // Обновляем статус слота
                slot.IsAvailable = false;
                slot.IsPending = true;
                slot.IsBooked = false;
                OnPropertyChanged(nameof(ScheduleSlots));
            }
        }
    }
}

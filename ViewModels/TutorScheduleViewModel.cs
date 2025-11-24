using coach_search.DB;
using coach_search.Models;
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

        public TutorScheduleViewModel(int? id = null)
        {
            ScheduleSlots = new ObservableCollection<ScheduleSlot>(Enumerable.Range(0, 70).Select(_ => new ScheduleSlot()));
            LoadSchedule(id);
        }

        public async Task LoadSchedule(int? id = null)
        {
            List<Appointment> existingAppointments;
            AppointmentRepository appointmentRepository = new(new DB.Context());
            if (id is not null)
                existingAppointments = await appointmentRepository.GetTutorAppointmentsAsync(id ?? 0); 
            else
                existingAppointments = await appointmentRepository.GetTutorAppointmentsAsync((int)ApplicationContext.CurrentTutorId);//.Value

            int index = 0;
            foreach (var time in TimeSlots)
            {
                for (int day = 0; day <= 6; day++) // 0=Monday to 6=Sunday
                {
                    var appointment = existingAppointments.FirstOrDefault(a => a.DayOfWeek == day && a.Time == time);
                    //bool isAvailable = appointment == null || appointment.Status == 2; // No appointment or rejected
                    //bool isPending = appointment != null && appointment.Status == 0;
                    //bool isBooked = appointment != null && appointment.Status == 1;

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

        private async Task BookSlot(ScheduleSlot slot)
        {
            if (!slot.IsAvailable || ApplicationContext.CurrentUser == null || ApplicationContext.CurrentTutorId == null)
                return;

            if (ApplicationContext.CurrentUser.Role != 0)
            {
                return;
                //показать страницу юзера
            }

            // Показать MessageBox для комментария
            string comment = Microsoft.VisualBasic.Interaction.InputBox("Введите комментарий к бронированию (необязательно):", "Комментарий", "");

            var appointment = new Appointment
            {
                TutorId = ApplicationContext.CurrentTutorId.Value,
                StudentId = ApplicationContext.CurrentUser.Id,
                DayOfWeek = slot.DayOfWeek,
                Time = slot.Time,
                Comment = comment,
                Status = 0 // pending
            };

            await ApplicationContext.unitofwork.Appointments.AddAsync(appointment);

            // Обновляем статус
            slot.IsAvailable = false;
            slot.IsPending = true;
            slot.IsBooked = false;

            OnPropertyChanged(nameof(ScheduleSlots));
            // OnPropertyChanged не нужен, так как свойства INotifyPropertyChanged
        }
    }
}

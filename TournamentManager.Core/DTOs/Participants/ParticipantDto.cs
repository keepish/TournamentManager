using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TournamentManager.Core.DTOs.Participants
{
    public class ParticipantDto : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _surname = string.Empty;
        private string? _patronymic;
        private string? _phone;
        private ulong _gender;
        private DateTime _birthday;
        private decimal _weight;


        public int Id { get; set; }
        public string Name 
        { 
            get => _name;
            set
            { 
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            } 
        }
        public string Surname 
        {
            get => _surname;
            set
            {
                _surname = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Surname));
            }
        }
        public string? Patronymic 
        {
            get => _patronymic; 
            set
            {
                _patronymic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Patronymic));
            }
        }
        public string? Phone 
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }
        public ulong Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Gender));
            }
        }
        public DateTime Birthday 
        {
            get => _birthday;
            set
            {
                _birthday = value;
                OnPropertyChanged();
            } 
        }
        public decimal Weight 
        { 
            get => _weight; 
            set
            {
                _weight = value;
                OnPropertyChanged();
            }
        }

        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
        public string GenderDisplay => Gender == 1 ? "Мужской" : "Женский";
        public int Age => DateTime.Now.Year - Birthday.Year;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

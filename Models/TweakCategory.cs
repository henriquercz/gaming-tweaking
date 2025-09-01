using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GamingTweaksManager.Models
{
    /// <summary>
    /// Representa uma categoria de tweaks com seus itens associados
    /// </summary>
    public class TweakCategory : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private int _appliedCount;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public ObservableCollection<TweakItem> Tweaks { get; set; } = new ObservableCollection<TweakItem>();
        public int TotalCount { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public int AppliedCount
        {
            get => _appliedCount;
            set
            {
                _appliedCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText => $"{AppliedCount}/{Tweaks?.Count ?? 0} aplicados";

        public TweakCategory()
        {
            Tweaks = new ObservableCollection<TweakItem>();
            IsExpanded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

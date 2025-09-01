using System;
using System.ComponentModel;

namespace GamingTweaksManager.Models
{
    /// <summary>
    /// Representa um item de tweak individual com informações detalhadas
    /// </summary>
    public class TweakItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isApplied;
        private bool _isEnabled;

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DetailedDescription { get; set; }
        public string BatchContent { get; set; }
        public string Category { get; set; }
        public string[] CompatibleHardware { get; set; }
        public string Impact { get; set; } // Low, Medium, High
        public bool RequiresRestart { get; set; }
        public string[] Benefits { get; set; }
        public string[] Risks { get; set; }
        public string TechnicalDetails { get; set; }
        public string PerformanceGain { get; set; }
        public string RecommendedFor { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsApplied
        {
            get => _isApplied;
            set
            {
                _isApplied = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Estado do toggle (ON/OFF) do tweak
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Cor do impacto baseada no nível
        /// </summary>
        public string ImpactColor
        {
            get
            {
                return Impact?.ToLower() switch
                {
                    "low" => "#4CAF50",      // Verde
                    "medium" => "#FF9800",   // Laranja  
                    "high" => "#F44336",     // Vermelho
                    _ => "#9E9E9E"           // Cinza
                };
            }
        }

        /// <summary>
        /// Ícone do impacto baseado no nível
        /// </summary>
        public string ImpactIcon => "AlertCircle";

        /// <summary>
        /// Texto do impacto traduzido
        /// </summary>
        public string ImpactText
        {
            get
            {
                return Impact?.ToLower() switch
                {
                    "low" => "Baixo",
                    "medium" => "Médio",
                    "high" => "Alto",
                    _ => "Desconhecido"
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

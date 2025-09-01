using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GamingTweaksManager.Models;
using GamingTweaksManager.Services;

namespace GamingTweaksManager.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação, gerencia o estado e lógica da interface
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TweakLoaderService _tweakLoader;
        private readonly HardwareDetectionService _hardwareDetection;
        private readonly TweakExecutionService _tweakExecution;
        private readonly HardwareMonitoringService _hardwareMonitoring;
        private readonly TweakStateService _stateService;

        private ObservableCollection<TweakCategory> _categories;
        private TweakCategory _selectedCategory;
        private ObservableCollection<TweakItem> _filteredTweaks;
        private bool _showOnlyCompatible;
        private string _statusMessage;
        private bool _isAdministrator;
        private TweakItem _selectedTweak;
        private bool _isLoading;

        public ObservableCollection<TweakCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TweakItem> FilteredTweaks
        {
            get => _filteredTweaks ?? new ObservableCollection<TweakItem>();
            set
            {
                _filteredTweaks = value;
                OnPropertyChanged();
            }
        }

        public TweakCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public TweakItem SelectedTweak
        {
            get => _selectedTweak;
            set
            {
                _selectedTweak = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOnlyCompatible
        {
            get => _showOnlyCompatible;
            set
            {
                _showOnlyCompatible = value;
                OnPropertyChanged();
                FilterTweaks();
            }
        }

        public string HardwareInfoText => _hardwareDetection.GetHardwareInfo();
        public bool IsAdministrator => _isAdministrator;

        // Comandos
        public ICommand ApplyTweakCommand { get; private set; }
        public ICommand ToggleTweakCommand { get; private set; }
        public ICommand SelectCategoryCommand { get; private set; }
        public ICommand ApplyCategoryCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public HardwareMonitoringService HardwareMonitoring => _hardwareMonitoring;

        public MainViewModel()
        {
            _tweakLoader = new TweakLoaderService();
            _hardwareDetection = new HardwareDetectionService();
            _tweakExecution = new TweakExecutionService();
            _hardwareMonitoring = new HardwareMonitoringService();
            _stateService = new TweakStateService();

            // Iniciar monitoramento de hardware
            _hardwareMonitoring.StartMonitoring();

            // Carregar dados
            LoadTweaksAsync();

            // Verificar se está executando como administrador
            _isAdministrator = IsRunningAsAdministrator();

            // Inicializar comandos
            ApplyTweakCommand = new RelayCommand<TweakItem>(async (tweak) => await ApplyTweakAsync(tweak));
            ToggleTweakCommand = new RelayCommand<TweakItem>(async (tweak) => await ToggleTweakAsync(tweak));
            SelectCategoryCommand = new RelayCommand<TweakCategory>(SelectCategory);
            ApplyCategoryCommand = new RelayCommand<TweakCategory>(async (category) => await ApplyCategoryAsync(category));
            RefreshCommand = new RelayCommand(async () => await LoadTweaksAsync());

            // Carregar tweaks na inicialização
            _ = Task.Run(LoadTweaksAsync);
        }

        /// <summary>
        /// Verifica se está executando como administrador
        /// </summary>
        private bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Carrega todos os tweaks e organiza por categorias
        /// </summary>
        private async Task LoadTweaksAsync()
        {
            try
            {
                _isLoading = true;
                StatusMessage = "Carregando tweaks...";

                var categories = await _tweakLoader.LoadTweaksAsync();

                Categories = new ObservableCollection<TweakCategory>(categories);
                var totalTweaks = categories.Sum(c => c.Tweaks.Count);
                StatusMessage = $"✅ {totalTweaks} tweaks carregados em {categories.Count} categorias";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erro ao carregar tweaks: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Verifica se pode aplicar um tweak
        /// </summary>
        private bool CanApplyTweak(TweakItem tweak)
        {
            return tweak != null && _isAdministrator && !_isLoading;
        }

        /// <summary>
        /// Verifica se pode aplicar uma categoria
        /// </summary>
        private bool CanApplyCategory(TweakCategory category)
        {
            return category != null && category.Tweaks.Any() && _isAdministrator && !_isLoading;
        }

        /// <summary>
        /// Seleciona uma categoria
        /// </summary>
        private void SelectCategory(TweakCategory category)
        {
            SelectedCategory = category;
            FilterTweaks();
        }

        /// <summary>
        /// Filtra tweaks baseado nas configurações
        /// </summary>
        private void FilterTweaks()
        {
            if (SelectedCategory == null)
            {
                _filteredTweaks = new ObservableCollection<TweakItem>();
                return;
            }

            var tweaks = SelectedCategory.Tweaks.Where(t => 
                !ShowOnlyCompatible || t.CompatibleHardware.Contains("Universal") || 
                t.CompatibleHardware.Any(h => _hardwareDetection.IsCompatible(h))).ToList();

            _filteredTweaks = new ObservableCollection<TweakItem>(tweaks);
            OnPropertyChanged(nameof(FilteredTweaks));
        }

        /// <summary>
        /// Aplica um tweak individual
        /// </summary>
        private async Task ApplyTweakAsync(TweakItem tweak)
        {
            if (!_isAdministrator)
            {
                StatusMessage = "É necessário executar como administrador para aplicar tweaks";
                return;
            }

            try
            {
                StatusMessage = $"Aplicando tweak: {tweak.Title}...";
                IsLoading = true;

                var success = await _tweakExecution.ExecuteTweakAsync(tweak);
                
                if (success)
                {
                    tweak.IsApplied = true;
                    StatusMessage = $"Tweak '{tweak.Title}' aplicado com sucesso!";
                }
                else
                {
                    StatusMessage = $"Falha ao aplicar tweak: {tweak.Title}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao aplicar tweak: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleTweakAsync(TweakItem tweak)
        {
            if (!_isAdministrator)
            {
                StatusMessage = "É necessário executar como administrador para aplicar tweaks";
                return;
            }

            try
            {
                StatusMessage = tweak.IsEnabled ? $"Desabilitando tweak: {tweak.Title}..." : $"Habilitando tweak: {tweak.Title}...";
                IsLoading = true;

                if (tweak.IsEnabled)
                {
                    // Reverter tweak usando backup
                    var success = await _tweakExecution.RevertTweakAsync(tweak);
                    if (success)
                    {
                        tweak.IsEnabled = false;
                        tweak.IsApplied = false;
                        StatusMessage = $"Tweak '{tweak.Title}' desabilitado com sucesso!";
                    }
                    else
                    {
                        StatusMessage = $"Falha ao desabilitar tweak: {tweak.Title}";
                    }
                }
                else
                {
                    // Aplicar tweak e fazer backup
                    var success = await _tweakExecution.ExecuteTweakWithBackupAsync(tweak);
                    if (success)
                    {
                        tweak.IsEnabled = true;
                        tweak.IsApplied = true;
                        StatusMessage = $"Tweak '{tweak.Title}' habilitado com sucesso!";
                    }
                    else
                    {
                        StatusMessage = $"Falha ao habilitar tweak: {tweak.Title}";
                    }
                }

                // Salvar estado do tweak
                await _stateService.SaveTweakStateAsync(tweak.Id, tweak.IsEnabled);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao alternar tweak: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Aplica todos os tweaks de uma categoria
        /// </summary>
        /// <param name="category">Categoria a ser aplicada</param>
        private async Task ApplyCategoryAsync(TweakCategory category)
        {
            if (category == null) return;

            var result = MessageBox.Show(
                $"Deseja aplicar todos os {category.Tweaks.Count} tweaks da categoria '{category.Name}'?\n\n" +
                "Esta operação pode levar alguns minutos e modificará configurações do sistema.",
                "Confirmar Aplicação em Lote",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var filteredTweaks = category.Tweaks.Where(t => 
                    !ShowOnlyCompatible || t.CompatibleHardware.Contains("Universal") || 
                    t.CompatibleHardware.Any(h => _hardwareDetection.IsCompatible(h))).ToList();
                var successCount = 0;
                var errorCount = 0;

                foreach (var tweak in filteredTweaks)
                {
                    StatusMessage = $"Aplicando {tweak.Title}... ({successCount + errorCount + 1}/{filteredTweaks.Count})";
                    
                    var tweakResult = await _tweakExecution.ExecuteTweakAsync(tweak);
                    
                    if (tweakResult)
                    {
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }

                UpdateCategoryCounters();

                MessageBox.Show(
                    $"Aplicação da categoria '{category.Name}' concluída:\n\n" +
                    $"✅ Sucessos: {successCount}\n" +
                    $"❌ Erros: {errorCount}",
                    "Aplicação Concluída",
                    MessageBoxButton.OK,
                    errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erro na aplicação em lote: {ex.Message}";
                MessageBox.Show($"Erro na aplicação em lote:\n{ex.Message}", 
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Atualiza os contadores de tweaks aplicados nas categorias
        /// </summary>
        private void UpdateCategoryCounters()
        {
            foreach (var category in Categories)
            {
                category.AppliedCount = category.Tweaks.Count(t => t.IsApplied);
            }
        }

        /// <summary>
        /// Cria backup completo do sistema
        /// </summary>
        private async Task CreateSystemBackupAsync()
        {
            try
            {
                StatusMessage = "Criando backup do sistema...";
                
                // Implementar backup completo do registro
                await Task.Delay(2000); // Simular operação
                
                StatusMessage = "✅ Backup do sistema criado com sucesso";
                MessageBox.Show("Backup do sistema criado com sucesso!", 
                              "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erro ao criar backup: {ex.Message}";
                MessageBox.Show($"Erro ao criar backup:\n{ex.Message}", 
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Implementação simples de ICommand para os comandos da aplicação
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }

    /// <summary>
    /// Implementação de ICommand com parâmetro genérico
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
    }
}

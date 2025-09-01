using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GamingTweaksManager.Models;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço para gerenciar estado dos tweaks (ON/OFF) com persistência
    /// </summary>
    public class TweakStateService
    {
        private readonly string _stateFilePath;
        private Dictionary<string, TweakState> _tweakStates;

        public TweakStateService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GamingTweaksManager");
            Directory.CreateDirectory(appDataPath);
            _stateFilePath = Path.Combine(appDataPath, "tweak_states.json");
            _tweakStates = new Dictionary<string, TweakState>();
            LoadStates();
        }

        /// <summary>
        /// Obtém o estado atual de um tweak
        /// </summary>
        public bool GetTweakState(string tweakId)
        {
            return _tweakStates.ContainsKey(tweakId) && _tweakStates[tweakId].IsApplied;
        }

        /// <summary>
        /// Define o estado de um tweak
        /// </summary>
        public async Task SetTweakStateAsync(string tweakId, bool isApplied, string originalValue = null)
        {
            if (!_tweakStates.ContainsKey(tweakId))
            {
                _tweakStates[tweakId] = new TweakState
                {
                    TweakId = tweakId,
                    IsApplied = false,
                    OriginalValue = originalValue,
                    LastModified = DateTime.Now
                };
            }

            _tweakStates[tweakId].IsApplied = isApplied;
            _tweakStates[tweakId].LastModified = DateTime.Now;
            
            if (!string.IsNullOrEmpty(originalValue))
            {
                _tweakStates[tweakId].OriginalValue = originalValue;
            }

            await SaveStatesAsync();
        }

        /// <summary>
        /// Obtém o valor original de um tweak
        /// </summary>
        public string GetOriginalValue(string tweakId)
        {
            return _tweakStates.ContainsKey(tweakId) ? _tweakStates[tweakId].OriginalValue : null;
        }

        /// <summary>
        /// Salva o estado de um tweak
        /// </summary>
        public async Task SaveTweakStateAsync(string tweakId, bool isEnabled)
        {
            try
            {
                var state = await LoadTweakStatesAsync();
                state.States[tweakId] = new TweakState
                {
                    IsEnabled = isEnabled,
                    LastModified = DateTime.Now
                };

                await SaveTweakStatesAsync(state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar estado do tweak: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém o estado de um tweak
        /// </summary>
        public async Task<bool> GetTweakStateAsync(string tweakId)
        {
            try
            {
                var state = await LoadTweakStatesAsync();
                return state.States.ContainsKey(tweakId) ? state.States[tweakId].IsEnabled : false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter estado do tweak: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Salva dados de backup para um tweak
        /// </summary>
        public async Task SaveBackupDataAsync(string tweakId, Dictionary<string, object> backupData)
        {
            try
            {
                var state = await LoadTweakStatesAsync();
                if (!state.States.ContainsKey(tweakId))
                {
                    state.States[tweakId] = new TweakState();
                }
                
                state.States[tweakId].BackupData = backupData;
                state.States[tweakId].LastModified = DateTime.Now;

                await SaveTweakStatesAsync(state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar backup do tweak: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém dados de backup para um tweak
        /// </summary>
        public async Task<Dictionary<string, object>> GetBackupDataAsync(string tweakId)
        {
            try
            {
                var state = await LoadTweakStatesAsync();
                return state.States.ContainsKey(tweakId) ? state.States[tweakId].BackupData : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter backup do tweak: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Carrega estados do arquivo
        /// </summary>
        private void LoadStates()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    var json = File.ReadAllText(_stateFilePath);
                    var states = JsonSerializer.Deserialize<Dictionary<string, TweakState>>(json);
                    _tweakStates = states ?? new Dictionary<string, TweakState>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estados: {ex.Message}");
                _tweakStates = new Dictionary<string, TweakState>();
            }
        }

        /// <summary>
        /// Salva estados no arquivo
        /// </summary>
        private async Task SaveStatesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_tweakStates, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar estados: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém todos os estados
        /// </summary>
        public Dictionary<string, TweakState> GetAllStates()
        {
            return new Dictionary<string, TweakState>(_tweakStates);
        }

        private async Task<TweakStates> LoadTweakStatesAsync()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    var json = await File.ReadAllTextAsync(_stateFilePath);
                    return JsonSerializer.Deserialize<TweakStates>(json) ?? new TweakStates();
                }
                return new TweakStates();
            }
            catch
            {
                return new TweakStates();
            }
        }

        private async Task SaveTweakStatesAsync(TweakStates state)
        {
            try
            {
                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar estados: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Representa o estado de um tweak
    /// </summary>
    public class TweakState
    {
        public string TweakId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsApplied { get; set; }
        public string OriginalValue { get; set; }
        public Dictionary<string, object> BackupData { get; set; }
        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// Container para todos os estados dos tweaks
    /// </summary>
    public class TweakStates
    {
        public Dictionary<string, TweakState> States { get; set; } = new Dictionary<string, TweakState>();
    }
}

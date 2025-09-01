using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using GamingTweaksManager.Models;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço responsável por executar tweaks com backup e reversão automática
    /// </summary>
    public class TweakExecutionService
    {
        private readonly TweakStateService _stateService;

        public TweakExecutionService()
        {
            _stateService = new TweakStateService();
        }

        /// <summary>
        /// Executa um tweak específico
        /// </summary>
        /// <param name="tweak">Item de tweak a ser executado</param>
        /// <returns>True se executado com sucesso</returns>
        public async Task<bool> ExecuteTweakAsync(TweakItem tweak)
        {
            try
            {
                if (string.IsNullOrEmpty(tweak.BatchContent))
                    return false;

                var cleanedContent = CleanBatchContent(tweak.BatchContent);
                var success = await ExecuteBatchContentAsync(cleanedContent);
                
                if (success)
                {
                    tweak.IsApplied = true;
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao executar tweak: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executa um tweak com backup automático dos valores originais
        /// </summary>
        /// <param name="tweak">Tweak a ser executado</param>
        /// <returns>True se executado com sucesso</returns>
        public async Task<bool> ExecuteTweakWithBackupAsync(TweakItem tweak)
        {
            try
            {
                // Fazer backup dos valores originais antes de aplicar
                var backupData = await CreateBackupAsync(tweak);
                if (backupData != null)
                {
                    await _stateService.SaveBackupDataAsync(tweak.Id, backupData);
                }

                // Executar o tweak
                var success = await ExecuteTweakAsync(tweak);
                
                if (success)
                {
                    await _stateService.SaveTweakStateAsync(tweak.Id, true);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao executar tweak com backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reverte um tweak usando os dados de backup
        /// </summary>
        /// <param name="tweak">Tweak a ser revertido</param>
        /// <returns>True se revertido com sucesso</returns>
        public async Task<bool> RevertTweakAsync(TweakItem tweak)
        {
            try
            {
                var backupData = await _stateService.GetBackupDataAsync(tweak.Id);
                if (backupData == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Nenhum backup encontrado para o tweak: {tweak.Id}");
                    return false;
                }

                // Restaurar valores do backup
                var success = await RestoreFromBackupAsync(backupData);
                
                if (success)
                {
                    tweak.IsApplied = false;
                    await _stateService.SaveTweakStateAsync(tweak.Id, false);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao reverter tweak: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executa o conteúdo batch
        /// </summary>
        /// <param name="batchContent">Conteúdo do script</param>
        /// <returns>True se executado com sucesso</returns>
        private async Task<bool> ExecuteBatchContentAsync(string batchContent)
        {
            try
            {
                // Criar arquivo temporário para o script
                var tempFile = Path.Combine(Path.GetTempPath(), $"tweak_{Guid.NewGuid()}.bat");
                await File.WriteAllTextAsync(tempFile, batchContent);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{tempFile}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        
                        // Limpar arquivo temporário
                        try { File.Delete(tempFile); } catch { }
                        
                        return process.ExitCode == 0;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao executar batch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cria backup dos valores que serão modificados pelo tweak
        /// </summary>
        private async Task<Dictionary<string, object>> CreateBackupAsync(TweakItem tweak)
        {
            var backup = new Dictionary<string, object>();

            try
            {
                var registryKeys = ExtractRegistryKeysFromBatch(tweak.BatchContent);
                
                foreach (var keyInfo in registryKeys)
                {
                    try
                    {
                        var value = GetRegistryValue(keyInfo.Key, keyInfo.ValueName);
                        if (value != null)
                        {
                            backup[$"{keyInfo.Key}\\{keyInfo.ValueName}"] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao fazer backup da chave {keyInfo.Key}: {ex.Message}");
                    }
                }

                return backup.Count > 0 ? backup : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar backup: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Restaura valores do backup
        /// </summary>
        private async Task<bool> RestoreFromBackupAsync(Dictionary<string, object> backupData)
        {
            try
            {
                foreach (var item in backupData)
                {
                    var parts = item.Key.Split('\\');
                    if (parts.Length >= 2)
                    {
                        var keyPath = string.Join("\\", parts.Take(parts.Length - 1));
                        var valueName = parts.Last();
                        
                        SetRegistryValue(keyPath, valueName, item.Value);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao restaurar backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Extrai informações de chaves de registro do conteúdo batch
        /// </summary>
        private List<(string Key, string ValueName)> ExtractRegistryKeysFromBatch(string batchContent)
        {
            var keys = new List<(string, string)>();
            var lines = batchContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("REG ADD", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    string keyPath = null;
                    string valueName = null;

                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        if (parts[i].Equals("/v", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
                        {
                            valueName = parts[i + 1].Trim('"');
                        }
                        else if (i == 2)
                        {
                            keyPath = parts[i].Trim('"');
                        }
                    }

                    if (!string.IsNullOrEmpty(keyPath) && !string.IsNullOrEmpty(valueName))
                    {
                        keys.Add((keyPath, valueName));
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// Obtém valor do registro
        /// </summary>
        private object GetRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (var key = GetRegistryKey(keyPath, false))
                {
                    return key?.GetValue(valueName);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Define valor no registro
        /// </summary>
        private void SetRegistryValue(string keyPath, string valueName, object value)
        {
            try
            {
                using (var key = GetRegistryKey(keyPath, true))
                {
                    if (key != null && value != null)
                    {
                        key.SetValue(valueName, value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao definir valor do registro: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém chave do registro
        /// </summary>
        private RegistryKey GetRegistryKey(string keyPath, bool writable)
        {
            try
            {
                if (keyPath.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase))
                {
                    return Registry.LocalMachine.OpenSubKey(keyPath.Substring(5), writable);
                }
                else if (keyPath.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase))
                {
                    return Registry.CurrentUser.OpenSubKey(keyPath.Substring(5), writable);
                }
                else if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
                {
                    return Registry.LocalMachine.OpenSubKey(keyPath.Substring(19), writable);
                }
                else if (keyPath.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
                {
                    return Registry.CurrentUser.OpenSubKey(keyPath.Substring(18), writable);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Limpa e corrige o conteúdo do batch
        /// </summary>
        private string CleanBatchContent(string batchContent)
        {
            if (string.IsNullOrEmpty(batchContent))
                return batchContent;

            var lines = batchContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("::") || trimmedLine.StartsWith("REM"))
                    continue;

                if (trimmedLine.StartsWith("REG ADD", StringComparison.OrdinalIgnoreCase))
                {
                    trimmedLine = FixRegAddCommand(trimmedLine);
                }

                cleanedLines.Add(trimmedLine);
            }

            return string.Join("\r\n", cleanedLines);
        }

        /// <summary>
        /// Corrige comandos REG ADD com erros comuns
        /// </summary>
        private string FixRegAddCommand(string command)
        {
            // Corrigir caminhos de chaves comuns
            if (!command.Contains("\"HKLM\\") && !command.Contains("\"HKCU\\"))
            {
                command = command.Replace("HKLM\\", "\"HKLM\\");
                command = command.Replace("HKCU\\", "\"HKCU\\");
                
                // Fechar aspas após o caminho da chave
                var parts = command.Split(' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("\"HKLM\\") || parts[i].StartsWith("\"HKCU\\"))
                    {
                        if (!parts[i].EndsWith("\""))
                        {
                            parts[i] += "\"";
                        }
                        break;
                    }
                }
                command = string.Join(" ", parts);
            }

            if (!command.Contains("/f"))
            {
                command += " /f";
            }

            return command;
        }
    }
}
                    string keyPath = null;
                    string valueName = null;

                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        if (parts[i].Equals("/v", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
                        {
                            valueName = parts[i + 1].Trim('"');
                        }
                        else if (i == 2) // Terceiro elemento é geralmente o caminho da chave
                        {
                            keyPath = parts[i].Trim('"');
                        }
                    }

                    if (!string.IsNullOrEmpty(keyPath) && !string.IsNullOrEmpty(valueName))
                    {
                        keys.Add((keyPath, valueName));
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// Obtém valor do registro
        /// </summary>
        private object GetRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (var key = GetRegistryKey(keyPath, false))
                {
                    return key?.GetValue(valueName);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Define valor no registro
        /// </summary>
        private void SetRegistryValue(string keyPath, string valueName, object value)
        public static bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
    }

    /// <summary>
    /// Resultado da execução de um tweak
    /// </summary>
    public class ExecutionResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Output { get; set; }
    }
}

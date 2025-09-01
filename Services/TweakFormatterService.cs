using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço para formatação e limpeza de nomes de tweaks
    /// </summary>
    public static class TweakFormatterService
    {
        /// <summary>
        /// Formata o nome do tweak removendo extensões e melhorando capitalização
        /// </summary>
        public static string FormatTweakName(string originalName)
        {
            if (string.IsNullOrEmpty(originalName))
                return "Tweak Desconhecido";

            // Remove extensões comuns
            string name = originalName;
            string[] extensions = { ".bat", ".cmd", ".ps1", ".reg" };
            foreach (var ext in extensions)
            {
                if (name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - ext.Length);
                    break;
                }
            }

            // Remove caracteres especiais e números no início
            name = Regex.Replace(name, @"^[0-9\-_\s]+", "");
            
            // Substitui caracteres especiais por espaços
            name = Regex.Replace(name, @"[_\-\.]", " ");
            
            // Remove espaços múltiplos
            name = Regex.Replace(name, @"\s+", " ");
            
            // Capitaliza palavras importantes
            name = CapitalizeWords(name);
            
            // Aplica formatações específicas conhecidas
            name = ApplySpecificFormatting(name);
            
            return name.Trim();
        }

        /// <summary>
        /// Capitaliza palavras importantes mantendo acrônimos
        /// </summary>
        private static string CapitalizeWords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].ToLower();
                
                // Acrônimos conhecidos
                if (IsKnownAcronym(word))
                {
                    words[i] = word.ToUpper();
                }
                // Palavras pequenas que não devem ser capitalizadas (exceto primeira)
                else if (i > 0 && IsSmallWord(word))
                {
                    words[i] = word;
                }
                // Capitalização normal
                else
                {
                    words[i] = char.ToUpper(word[0]) + word.Substring(1);
                }
            }
            
            return string.Join(" ", words);
        }

        /// <summary>
        /// Verifica se é um acrônimo conhecido
        /// </summary>
        private static bool IsKnownAcronym(string word)
        {
            string[] acronyms = { 
                "cpu", "gpu", "ram", "ssd", "hdd", "usb", "tcp", "ip", "dns", "dhcp",
                "nvidia", "amd", "intel", "directx", "opengl", "vulkan", "api",
                "ppm", "tcc", "msi", "fps", "vsync", "gsync", "freesync",
                "windows", "xbox", "steam", "epic", "origin", "uplay"
            };
            
            return Array.Exists(acronyms, a => a.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Verifica se é uma palavra pequena que não deve ser capitalizada
        /// </summary>
        private static bool IsSmallWord(string word)
        {
            string[] smallWords = { "de", "da", "do", "das", "dos", "e", "ou", "para", "com", "sem", "por" };
            return Array.Exists(smallWords, w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica formatações específicas para nomes conhecidos
        /// </summary>
        private static string ApplySpecificFormatting(string name)
        {
            var replacements = new Dictionary<string, string>
            {
                { "adware cleaner", "Limpador de Adware" },
                { "disable nvidia telm", "Desabilitar Telemetria NVIDIA" },
                { "nv tweaks", "Otimizações NVIDIA" },
                { "disable amd logging", "Desabilitar Logs AMD" },
                { "clean directx cache", "Limpar Cache DirectX" },
                { "fix stock cpu speed", "Corrigir Velocidade do CPU" },
                { "idle power", "Gerenciamento de Energia Idle" },
                { "io priority", "Prioridade de E/S" },
                { "additional power", "Configurações Avançadas de Energia" },
                { "latency tolerance", "Tolerância de Latência" },
                { "disable tcpip priority", "Desabilitar Prioridade TCP/IP" },
                { "disable autologgers", "Desabilitar Logs Automáticos" },
                { "advanced services revert", "Reverter Serviços Avançados" },
                { "clean temp", "Limpeza de Arquivos Temporários" },
                { "controller tweaks", "Otimizações de Controle" },
                { "uninstall edge", "Remover Microsoft Edge" }
            };

            string lowerName = name.ToLower();
            foreach (var replacement in replacements)
            {
                if (lowerName.Contains(replacement.Key))
                {
                    return replacement.Value;
                }
            }

            return name;
        }
    }
}

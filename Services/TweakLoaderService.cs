using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GamingTweaksManager.Models;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço responsável por carregar e categorizar os tweaks dos arquivos JSON
    /// </summary>
    public class TweakLoaderService
    {
        private readonly string _tweaksDirectory;

        public TweakLoaderService(string tweaksDirectory = null)
        {
            _tweaksDirectory = tweaksDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tweaks");
            
            // Se o diretório não existir, tentar o diretório pai (para desenvolvimento)
            if (!Directory.Exists(_tweaksDirectory))
            {
                var parentDir = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "", "Tweaks");
                if (Directory.Exists(parentDir))
                    _tweaksDirectory = parentDir;
            }
        }

        /// <summary>
        /// Carrega todos os tweaks dos arquivos JSON e organiza por categorias
        /// </summary>
        /// <returns>Lista de categorias com tweaks</returns>
        public async Task<List<TweakCategory>> LoadTweaksAsync()
        {
            var categories = InitializeCategories();
            
            // Verificar se o diretório existe
            if (!Directory.Exists(_tweaksDirectory))
            {
                System.Diagnostics.Debug.WriteLine($"Diretório de tweaks não encontrado: {_tweaksDirectory}");
                return categories;
            }
            
            var tweakFiles = Directory.GetFiles(_tweaksDirectory, "*", SearchOption.AllDirectories)
                                    .Where(f => !Path.GetFileName(f).EndsWith(".config") && !Path.GetFileName(f).Contains("."))
                                    .ToList();
                                    
            System.Diagnostics.Debug.WriteLine($"Encontrados {tweakFiles.Count} arquivos de tweaks em: {_tweaksDirectory}");

            foreach (var file in tweakFiles)
            {
                try
                {
                    var tweak = await LoadTweakFromFileAsync(file);
                    if (tweak != null)
                    {
                        // Aplicar formatação ao título
                        tweak.Title = TweakFormatterService.FormatTweakName(tweak.Title);
                    
                        // Carregar estado salvo do tweak
                        var stateService = new TweakStateService();
                        tweak.IsEnabled = await stateService.GetTweakStateAsync(tweak.Id);
                    
                        // Aplicar descrição detalhada
                        TweakDescriptionService.ApplyDetailedDescription(tweak);

                        // Determinar categoria baseada no título
                        var categoryName = DetermineCategory(tweak.Title, tweak.BatchContent);
                        var category = categories.FirstOrDefault(c => c.Name == categoryName);
                        if (category != null)
                        {
                            category.Tweaks.Add(tweak);
                        }
                        else
                        {
                            category = new TweakCategory { Name = categoryName, Description = $"Tweaks de {categoryName}" };
                            categories.Add(category);
                            category.Tweaks.Add(tweak);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao carregar tweak do arquivo {file}: {ex.Message}");
                }
            }

            // Atualizar contadores de cada categoria
            foreach (var category in categories)
            {
                category.TotalCount = category.Tweaks.Count;
                category.AppliedCount = category.Tweaks.Count(t => t.IsApplied);
            }

            return categories;
        }

        /// <summary>
        /// Carrega um tweak individual de um arquivo
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>Item de tweak carregado</returns>
        private async Task<TweakItem> LoadTweakFromFileAsync(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var tweakData = JsonConvert.DeserializeObject<TweakFileData>(content);

                if (tweakData?.Success == true && !string.IsNullOrEmpty(tweakData.BatchContent))
                {
                    var description = TweakDescriptionService.GetTweakDescription(tweakData.BatchTitle);
                        
                    var tweak = new TweakItem
                    {
                        Id = Path.GetFileNameWithoutExtension(filePath),
                        Title = tweakData.BatchTitle ?? "Tweak sem título",
                        Description = description.DetailedDescription,
                        DetailedDescription = description.DetailedDescription,
                        BatchContent = tweakData.BatchContent ?? "",
                        Category = DetermineCategory(tweakData.BatchTitle, tweakData.BatchContent),
                        CompatibleHardware = DetermineCompatibleHardware(tweakData.BatchTitle, tweakData.BatchContent),
                        Impact = description.Impact,
                        RequiresRestart = DetermineRequiresRestart(tweakData.BatchContent),
                        Benefits = description.Benefits,
                        Risks = description.Risks,
                        TechnicalDetails = description.TechnicalDetails,
                        PerformanceGain = description.PerformanceGain,
                        RecommendedFor = description.RecommendedFor
                    };

                    return tweak;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar arquivo {filePath}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Determina a categoria do tweak baseado no título e conteúdo
        /// </summary>
        private string DetermineCategory(string title, string content)
        {
            var titleLower = title?.ToLowerInvariant() ?? "";
            var contentLower = content?.ToLowerInvariant() ?? "";

            if (titleLower.Contains("nvidia") || contentLower.Contains("nvidia"))
                return "NVIDIA";
            else if (titleLower.Contains("amd") || contentLower.Contains("amd"))
                return "AMD";
            else if (titleLower.Contains("intel") || contentLower.Contains("intel"))
                return "Intel";
            else if (titleLower.Contains("cpu") || titleLower.Contains("processor") || contentLower.Contains("cpu"))
                return "CPU";
            else if (titleLower.Contains("power") || titleLower.Contains("energia") || contentLower.Contains("power"))
                return "Energia";
            else if (titleLower.Contains("network") || titleLower.Contains("tcp") || contentLower.Contains("network"))
                return "Rede";
            else if (titleLower.Contains("clean") || titleLower.Contains("temp") || contentLower.Contains("clean"))
                return "Limpeza";
            else
                return "Sistema";
        }

        /// <summary>
        /// Determina hardware compatível baseado no título e conteúdo
        /// </summary>
        private string[] DetermineCompatibleHardware(string title, string content)
        {
            var titleLower = title?.ToLowerInvariant() ?? "";
            var contentLower = content?.ToLowerInvariant() ?? "";

            if (titleLower.Contains("nvidia") || contentLower.Contains("nvidia"))
                return new[] { "NVIDIA" };
            else if (titleLower.Contains("amd") || contentLower.Contains("amd"))
                return new[] { "AMD" };
            else if (titleLower.Contains("intel") || contentLower.Contains("intel"))
                return new[] { "Intel" };
            else
                return new[] { "Universal" };
        }

        /// <summary>
        /// Determina se o tweak requer reinicialização
        /// </summary>
        private bool DetermineRequiresRestart(string content)
        {
            var contentLower = content?.ToLowerInvariant() ?? "";
            return contentLower.Contains("restart") || contentLower.Contains("reboot") || 
                   contentLower.Contains("reiniciar") || contentLower.Contains("shutdown");
        }

        /// <summary>
        /// Inicializa as categorias padrão do aplicativo
        /// </summary>
        /// <returns>Lista de categorias inicializadas</returns>
        private List<TweakCategory> InitializeCategories()
        {
            return new List<TweakCategory>
            {
                new TweakCategory
                {
                    Name = "NVIDIA",
                    Description = "Otimizações específicas para placas de vídeo NVIDIA",
                    Icon = "🎮"
                },
                new TweakCategory
                {
                    Name = "AMD", 
                    Description = "Otimizações para hardware AMD (GPU e CPU)",
                    Icon = "🔴"
                },
                new TweakCategory
                {
                    Name = "Intel",
                    Description = "Otimizações para processadores Intel",
                    Icon = "🔵"
                },
                new TweakCategory
                {
                    Name = "CPU",
                    Description = "Tweaks de processador e performance geral",
                    Icon = "⚡"
                },
                new TweakCategory
                {
                    Name = "Energia",
                    Description = "Configurações de energia e latência",
                    Icon = "🔋"
                },
                new TweakCategory
                {
                    Name = "Rede",
                    Description = "Otimizações de rede e internet",
                    Icon = "🌐"
                },
                new TweakCategory
                {
                    Name = "Sistema",
                    Description = "Tweaks gerais do sistema Windows",
                    Icon = "🖥️"
                },
                new TweakCategory
                {
                    Name = "Limpeza",
                    Description = "Ferramentas de limpeza e manutenção",
                    Icon = "🧹"
                },
                new TweakCategory
                {
                    Name = "GPU Tweaks",
                    Description = "Otimizações específicas para placas de vídeo NVIDIA, AMD e DirectX",
                    Icon = "🎮"
                },
                new TweakCategory
                {
                    Name = "CPU & Performance",
                    Description = "Tweaks de processador, prioridades e desempenho geral",
                    Icon = "⚡"
                },
                new TweakCategory
                {
                    Name = "Power Management",
                    Description = "Configurações de energia e latência do sistema",
                    Icon = "🔋"
                },
                new TweakCategory
                {
                    Name = "Network & Internet",
                    Description = "Otimizações de rede, TCP/IP e latência de internet",
                    Icon = "🌐"
                },
                new TweakCategory
                {
                    Name = "System Optimization",
                    Description = "Tweaks gerais do sistema, serviços e configurações",
                    Icon = "🛠️"
                },
                new TweakCategory
                {
                    Name = "Cleanup Tools",
                    Description = "Ferramentas de limpeza e remoção de software desnecessário",
                    Icon = "🧹"
                }
            };
        }

        /// <summary>
        /// Classe para deserializar os dados dos arquivos JSON
        /// </summary>
        private class TweakFileData
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("batchTitle")]
            public string BatchTitle { get; set; }

            [JsonProperty("batchContent")]
            public string BatchContent { get; set; }
        }
    }
}

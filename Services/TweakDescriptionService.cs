using System;
using System.Collections.Generic;
using GamingTweaksManager.Models;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço responsável por fornecer descrições detalhadas e informações técnicas dos tweaks
    /// </summary>
    public static class TweakDescriptionService
    {
        /// <summary>
        /// Dicionário com descrições detalhadas dos tweaks baseado no título/conteúdo
        /// </summary>
        private static readonly Dictionary<string, TweakDescription> TweakDescriptions = new()
        {
            // NVIDIA Tweaks
            ["disable nvidia telm"] = new TweakDescription
            {
                DetailedDescription = "Desabilita completamente a telemetria da NVIDIA, incluindo coleta de dados de uso e estatísticas de performance.",
                Benefits = new[] { "Reduz uso de CPU/RAM", "Melhora privacidade", "Elimina processos em background", "Reduz latência do sistema" },
                Risks = new[] { "GeForce Experience pode não funcionar corretamente", "Atualizações automáticas podem falhar" },
                TechnicalDetails = "Remove serviços: NvTelemetryContainer, NvDisplay.Container.exe, modifica registro HKLM\\SOFTWARE\\NVIDIA Corporation",
                PerformanceGain = "1-3% redução no uso de CPU, 50-100MB menos RAM",
                RecommendedFor = "Usuários que não usam GeForce Experience e priorizam performance",
                Impact = "Low"
            },

            ["nvtweaks"] = new TweakDescription
            {
                DetailedDescription = "Aplica otimizações avançadas do driver NVIDIA para reduzir latência e melhorar performance em jogos.",
                Benefits = new[] { "Reduz input lag significativamente", "Melhora frametime consistency", "Otimiza pipeline de renderização", "Reduz stuttering" },
                Risks = new[] { "Pode causar instabilidade em alguns jogos", "Possível aumento de temperatura da GPU" },
                TechnicalDetails = "Modifica PowerMizerEnable, PerfLevelSrc, PowerMizerLevel no registro da NVIDIA",
                PerformanceGain = "5-15ms redução de input lag, 2-8% aumento de FPS",
                RecommendedFor = "Gamers competitivos, especialmente FPS e MOBA",
                Impact = "Medium"
            },

            // AMD Tweaks
            ["disableamdlogging"] = new TweakDescription
            {
                DetailedDescription = "Desabilita logs e telemetria da AMD para reduzir overhead do sistema e melhorar privacidade.",
                Benefits = new[] { "Reduz escrita no disco", "Melhora performance do driver", "Elimina processos de logging", "Aumenta privacidade" },
                Risks = new[] { "Dificuldade para diagnosticar problemas do driver", "AMD Software pode apresentar avisos" },
                TechnicalDetails = "Desabilita AMD External Events Utility, modifica configurações de logging no registro",
                PerformanceGain = "1-2% redução no uso de CPU, menos I/O de disco",
                RecommendedFor = "Usuários AMD que não precisam de logs detalhados",
                Impact = "Low"
            },

            // CPU Tweaks
            ["fix stock cpu speed"] = new TweakDescription
            {
                DetailedDescription = "Corrige problemas de throttling do CPU e otimiza configurações de power management para performance máxima.",
                Benefits = new[] { "Elimina throttling desnecessário", "Melhora boost clocks", "Reduz latência do CPU", "Performance mais consistente" },
                Risks = new[] { "Aumento no consumo de energia", "Possível aumento de temperatura", "Redução da vida útil da bateria (laptops)" },
                TechnicalDetails = "Modifica Power Policy Manager (PPM), desabilita CPU parking, ajusta C-States",
                PerformanceGain = "3-10% aumento de performance, redução de 2-5ms na latência",
                RecommendedFor = "Desktops com cooling adequado, workstations",
                Impact = "Medium"
            },

            ["idlepower"] = new TweakDescription
            {
                DetailedDescription = "Otimiza configurações de idle do processador para reduzir latência ao sair do estado de baixo consumo.",
                Benefits = new[] { "Resposta mais rápida do sistema", "Reduz micro-stutters", "Melhora responsividade", "Otimiza wake-up time" },
                Risks = new[] { "Aumento no consumo em idle", "Maior aquecimento em repouso" },
                TechnicalDetails = "Ajusta Processor Idle States, modifica C1E, C3, C6 states no registro",
                PerformanceGain = "1-3ms melhoria na resposta do sistema",
                RecommendedFor = "Sistemas de gaming e workstations",
                Impact = "Low"
            },

            ["iopriority"] = new TweakDescription
            {
                DetailedDescription = "Otimiza prioridades de I/O para jogos e aplicações críticas, melhorando responsividade do sistema.",
                Benefits = new[] { "Reduz stuttering causado por I/O", "Melhora loading times", "Prioriza processos de jogos", "Reduz latência de disco" },
                Risks = new[] { "Outros processos podem ficar mais lentos", "Possível impacto em tarefas de background" },
                TechnicalDetails = "Modifica I/O Priority Classes, ajusta Disk Scheduler, otimiza Queue Depth",
                PerformanceGain = "10-30% melhoria em loading times, redução de stuttering",
                RecommendedFor = "Sistemas com HDDs ou SSDs SATA",
                Impact = "Medium"
            },

            // Power Tweaks
            ["additional power"] = new TweakDescription
            {
                DetailedDescription = "Aplica configurações avançadas de energia para maximizar performance, desabilitando recursos de economia.",
                Benefits = new[] { "Performance máxima constante", "Elimina throttling por energia", "Reduz latência do sistema", "Melhora responsividade" },
                Risks = new[] { "Aumento significativo no consumo", "Maior aquecimento", "Redução drástica da bateria (laptops)" },
                TechnicalDetails = "Modifica Power Schemes, desabilita USB Selective Suspend, PCI Express Link State",
                PerformanceGain = "2-5% aumento geral de performance",
                RecommendedFor = "Desktops com fonte adequada, sistemas sempre plugados",
                Impact = "High"
            },

            ["latencytolerance"] = new TweakDescription
            {
                DetailedDescription = "Reduz tolerâncias de latência do sistema para melhorar responsividade e reduzir input lag.",
                Benefits = new[] { "Redução significativa de input lag", "Melhora responsividade geral", "Otimiza interrupt handling", "Reduz jitter" },
                Risks = new[] { "Possível instabilidade em sistemas antigos", "Aumento no uso de CPU" },
                TechnicalDetails = "Ajusta Interrupt Moderation, Timer Resolution, DPC Latency settings",
                PerformanceGain = "3-8ms redução de latência, melhoria na responsividade",
                RecommendedFor = "Gaming competitivo, aplicações em tempo real",
                Impact = "Medium"
            },

            // Network Tweaks
            ["disable tcpip priority"] = new TweakDescription
            {
                DetailedDescription = "Otimiza configurações de rede TCP/IP para reduzir latência e melhorar throughput em jogos online.",
                Benefits = new[] { "Reduz ping em jogos", "Melhora estabilidade da conexão", "Otimiza packet processing", "Reduz network jitter" },
                Risks = new[] { "Possível incompatibilidade com alguns roteadores", "Pode afetar outras aplicações de rede" },
                TechnicalDetails = "Modifica TCP Window Scaling, Nagle Algorithm, TCP Chimney Offload",
                PerformanceGain = "5-20ms redução de ping, melhoria na estabilidade",
                RecommendedFor = "Gamers online, streaming, aplicações de rede críticas",
                Impact = "Medium"
            },

            // System Tweaks
            ["disableautologgers"] = new TweakDescription
            {
                DetailedDescription = "Desabilita loggers automáticos do Windows que consomem recursos desnecessariamente.",
                Benefits = new[] { "Reduz uso de CPU", "Menos escrita no disco", "Melhora performance geral", "Reduz overhead do sistema" },
                Risks = new[] { "Dificuldade para diagnosticar problemas", "Alguns recursos de diagnóstico podem parar de funcionar" },
                TechnicalDetails = "Desabilita ETW (Event Tracing for Windows) providers, WER (Windows Error Reporting)",
                PerformanceGain = "1-3% redução no uso de CPU, menos I/O de disco",
                RecommendedFor = "Sistemas estáveis que não precisam de logging extensivo",
                Impact = "Low"
            },

            ["advancedservicesrevert"] = new TweakDescription
            {
                DetailedDescription = "Otimiza serviços do Windows desabilitando aqueles desnecessários para gaming e performance.",
                Benefits = new[] { "Reduz uso de RAM", "Melhora boot time", "Elimina processos em background", "Reduz overhead do sistema" },
                Risks = new[] { "Alguns recursos do Windows podem parar de funcionar", "Possível impacto em funcionalidades específicas" },
                TechnicalDetails = "Modifica startup type de serviços: Fax, Windows Search, Superfetch, etc.",
                PerformanceGain = "100-500MB menos uso de RAM, boot 10-30% mais rápido",
                RecommendedFor = "Sistemas dedicados a gaming",
                Impact = "High"
            },

            ["cleantemp"] = new TweakDescription
            {
                DetailedDescription = "Remove arquivos temporários e cache desnecessários para liberar espaço e melhorar performance.",
                Benefits = new[] { "Libera espaço em disco", "Melhora performance do sistema", "Reduz fragmentação", "Limpa cache corrompido" },
                Risks = new[] { "Possível perda de dados temporários úteis", "Alguns programas podem precisar recriar cache" },
                TechnicalDetails = "Limpa %TEMP%, Prefetch, DNS Cache, Thumbnail Cache, Browser Cache",
                PerformanceGain = "Varia conforme quantidade de arquivos, melhoria geral do sistema",
                RecommendedFor = "Manutenção regular, sistemas com pouco espaço",
                Impact = "Low"
            },

            ["controllertweaks"] = new TweakDescription
            {
                DetailedDescription = "Otimiza configurações USB e de controles para reduzir input lag e melhorar responsividade.",
                Benefits = new[] { "Reduz input lag de controles", "Melhora polling rate", "Otimiza USB performance", "Reduz jitter de input" },
                Risks = new[] { "Possível incompatibilidade com alguns dispositivos USB", "Aumento no uso de CPU" },
                TechnicalDetails = "Ajusta USB polling rate, desabilita USB power saving, otimiza HID settings",
                PerformanceGain = "1-4ms redução de input lag, melhoria na precisão",
                RecommendedFor = "Gaming com controles, dispositivos de input de alta precisão",
                Impact = "Medium"
            },

            // DirectX Tweaks
            ["cleandirectxcache"] = new TweakDescription
            {
                DetailedDescription = "Limpa cache do DirectX e otimiza configurações para melhor performance gráfica.",
                Benefits = new[] { "Resolve problemas gráficos", "Melhora loading de shaders", "Elimina cache corrompido", "Otimiza renderização" },
                Risks = new[] { "Jogos podem precisar recompilar shaders", "Primeiro load pode ser mais lento" },
                TechnicalDetails = "Limpa DirectX Shader Cache, redefine configurações DirectDraw/Direct3D",
                PerformanceGain = "Resolve stuttering, melhoria na consistência de FPS",
                RecommendedFor = "Problemas gráficos, stuttering em jogos",
                Impact = "Low"
            },

            // Cleanup Tweaks
            ["adwarecleaner"] = new TweakDescription
            {
                DetailedDescription = "Remove adware, toolbars e software indesejado que pode impactar performance do sistema.",
                Benefits = new[] { "Remove software malicioso", "Melhora performance", "Reduz uso de recursos", "Aumenta segurança" },
                Risks = new[] { "Possível remoção de software legítimo", "Alguns programas podem parar de funcionar" },
                TechnicalDetails = "Remove entradas de registro, arquivos e processos relacionados a adware",
                PerformanceGain = "Varia conforme infecção, melhoria geral significativa",
                RecommendedFor = "Sistemas infectados, limpeza preventiva",
                Impact = "Medium"
            },

            ["uninstalledge"] = new TweakDescription
            {
                DetailedDescription = "Remove Microsoft Edge e componentes relacionados para liberar recursos do sistema.",
                Benefits = new[] { "Libera espaço em disco", "Reduz processos em background", "Elimina telemetria do Edge", "Melhora privacidade" },
                Risks = new[] { "Alguns recursos do Windows podem parar de funcionar", "Aplicações que dependem do Edge podem falhar" },
                TechnicalDetails = "Remove Edge via PowerShell, limpa entradas de registro e arquivos relacionados",
                PerformanceGain = "100-200MB menos uso de RAM, redução de processos",
                RecommendedFor = "Usuários que não usam Edge e priorizam performance",
                Impact = "Medium"
            }
        };

        /// <summary>
        /// Obtém descrição detalhada de um tweak baseado no título
        /// </summary>
        public static TweakDescription GetTweakDescription(string title)
        {
            if (string.IsNullOrEmpty(title))
                return GetDefaultDescription();

            var key = title.ToLowerInvariant()
                .Replace(".bat", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "");

            foreach (var kvp in TweakDescriptions)
            {
                if (key.Contains(kvp.Key.Replace(" ", "")))
                {
                    return kvp.Value;
                }
            }

            return GetDefaultDescription();
        }

        /// <summary>
        /// Retorna descrição padrão para tweaks não catalogados
        /// </summary>
        private static TweakDescription GetDefaultDescription()
        {
            return new TweakDescription
            {
                DetailedDescription = "Tweak de otimização do sistema. Consulte o conteúdo do script para detalhes específicos.",
                Benefits = new[] { "Otimização do sistema", "Possível melhoria de performance" },
                Risks = new[] { "Efeitos podem variar entre sistemas", "Recomenda-se backup antes da aplicação" },
                TechnicalDetails = "Detalhes técnicos não disponíveis para este tweak específico.",
                PerformanceGain = "Ganho de performance varia conforme o sistema",
                RecommendedFor = "Usuários experientes",
                Impact = "Medium"
            };
        }
    }

    /// <summary>
    /// Classe que representa a descrição detalhada de um tweak
    /// </summary>
    public class TweakDescription
    {
        public string DetailedDescription { get; set; }
        public string[] Benefits { get; set; }
        public string[] Risks { get; set; }
        public string TechnicalDetails { get; set; }
        public string PerformanceGain { get; set; }
        public string RecommendedFor { get; set; }
        public string Impact { get; set; }
    }
}

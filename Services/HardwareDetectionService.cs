using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço responsável por detectar hardware do sistema para filtrar tweaks compatíveis
    /// </summary>
    public class HardwareDetectionService
    {
        private HardwareInfo _hardwareInfo;

        public string GpuVendor => _hardwareInfo?.GpuVendor ?? "Unknown";
        public string CpuVendor => _hardwareInfo?.CpuVendor ?? "Unknown";
        public string CpuInfo => _hardwareInfo?.CpuName ?? "Unknown";
        public string GpuInfo => string.Join(", ", _hardwareInfo?.GpuNames ?? new List<string>());

        public HardwareDetectionService()
        {
            _hardwareInfo = DetectHardware();
        }

        /// <summary>
        /// Verifica se o hardware é compatível com o tweak
        /// </summary>
        public bool IsCompatible(string hardwareType)
        {
            return hardwareType switch
            {
                "NVIDIA" => GpuVendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase),
                "AMD" => GpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase),
                "Intel" => CpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase) || 
                          GpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase),
                "Universal" => true,
                _ => true
            };
        }

        public string GetHardwareInfo()
        {
            return $"CPU: {CpuInfo} | GPU: {GpuInfo}";
        }
        public class HardwareInfo
        {
            public string GpuVendor { get; set; } = "Unknown";
            public string CpuVendor { get; set; } = "Unknown";
            public List<string> GpuNames { get; set; } = new List<string>();
            public string CpuName { get; set; } = "Unknown";
            public bool HasNvidia => GpuVendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);
            public bool HasAmd => GpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase) || 
                                  GpuVendor.Contains("ATI", StringComparison.OrdinalIgnoreCase);
            public bool HasIntel => GpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase);
            public bool HasIntelCpu => CpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase);
            public bool HasAmdCpu => CpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Detecta informações de hardware do sistema
        /// </summary>
        /// <returns>Informações de hardware detectadas</returns>
        public static HardwareInfo DetectHardware()
        {
            var hardwareInfo = new HardwareInfo();

            try
            {
                // Detectar GPU
                DetectGpu(hardwareInfo);
                
                // Detectar CPU
                DetectCpu(hardwareInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar hardware: {ex.Message}");
            }

            return hardwareInfo;
        }

        private static void DetectGpu(HardwareInfo hardwareInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    var gpuVendors = new List<string>();
                    
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            hardwareInfo.GpuNames.Add(name);
                            
                            if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("NVIDIA");
                            else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || 
                                     name.Contains("ATI", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("AMD");
                            else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("Intel");
                        }
                    }

                    if (gpuVendors.Any())
                        hardwareInfo.GpuVendor = string.Join(", ", gpuVendors.Distinct());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar GPU: {ex.Message}");
            }
        }

        private static void DetectCpu(HardwareInfo hardwareInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            hardwareInfo.CpuName = name;
                            
                            if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                                hardwareInfo.CpuVendor = "Intel";
                            else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase))
                                hardwareInfo.CpuVendor = "AMD";
                            
                            break; // Pegar apenas o primeiro processador
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar CPU: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se um tweak é compatível com o hardware atual
        /// </summary>
        /// <param name="tweakCompatibility">String de compatibilidade do tweak</param>
        /// <param name="hardwareInfo">Informações de hardware</param>
        /// <returns>True se compatível</returns>
        public static bool IsCompatible(string tweakCompatibility, HardwareInfo hardwareInfo)
        {
            if (string.IsNullOrEmpty(tweakCompatibility) || tweakCompatibility.Equals("Universal", StringComparison.OrdinalIgnoreCase))
                return true;

            var compatibility = tweakCompatibility.ToLowerInvariant();

            if (compatibility.Contains("nvidia") && !hardwareInfo.HasNvidia)
                return false;
            
            if (compatibility.Contains("amd") && !hardwareInfo.HasAmd && !hardwareInfo.HasAmdCpu)
                return false;
            
            if (compatibility.Contains("intel") && !hardwareInfo.HasIntel && !hardwareInfo.HasIntelCpu)
                return false;

            return true;
        }
    }
}

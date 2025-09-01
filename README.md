# 🎮 Gaming Tweaks Manager

**Aplicativo Profissional de Otimização do Windows para Jogos Competitivos**

Desenvolvido por **Capitão Henrique Gaming Solutions**

## 📋 Descrição

O Gaming Tweaks Manager é uma solução completa e profissional para otimizar o Windows especificamente para jogos competitivos. O aplicativo organiza e aplica automaticamente mais de 280 tweaks categorizados, com detecção inteligente de hardware e sistema de backup integrado.

## ✨ Características Principais

### 🔧 **Categorias de Otimização**
- **🎮 GPU Tweaks**: Otimizações específicas para NVIDIA, AMD e DirectX
- **⚡ CPU & Performance**: Tweaks de processador, prioridades e desempenho
- **🔋 Power Management**: Configurações de energia e latência do sistema
- **🌐 Network & Internet**: Otimizações de rede, TCP/IP e latência
- **🛠️ System Optimization**: Tweaks gerais do sistema e serviços
- **🧹 Cleanup Tools**: Ferramentas de limpeza e remoção

### 🎯 **Recursos Avançados**
- **Detecção Automática de Hardware**: Identifica NVIDIA/AMD/Intel automaticamente
- **Filtros de Compatibilidade**: Mostra apenas tweaks compatíveis com seu hardware
- **Sistema de Backup**: Backup automático do registro antes de aplicar tweaks
- **Interface Moderna**: Design Material Design intuitivo e profissional
- **Aplicação em Lote**: Aplique todos os tweaks de uma categoria de uma vez
- **Feedback em Tempo Real**: Status detalhado de cada operação

### 🔒 **Segurança e Confiabilidade**
- **Backup Automático**: Cada tweak cria backup das chaves de registro modificadas
- **Validação de Permissões**: Verifica se está executando como Administrador
- **Tratamento de Erros**: Sistema robusto de tratamento de erros e rollback
- **Logs Detalhados**: Registro completo de todas as operações

## 🚀 Instalação e Uso

### **Pré-requisitos**
- Windows 10/11
- .NET 8.0 Runtime
- Permissões de Administrador

### **Instalação**
1. Baixe o executável do Gaming Tweaks Manager
2. **IMPORTANTE**: Clique com o botão direito e selecione "Executar como administrador"
3. O aplicativo detectará automaticamente seu hardware

### **Como Usar**
1. **Selecione uma Categoria**: Clique em uma das 6 categorias no painel esquerdo
2. **Revise os Tweaks**: Veja os tweaks disponíveis para sua categoria
3. **Aplique Individualmente**: Clique em "Aplicar" em tweaks específicos
4. **Ou Aplique em Lote**: Use "Aplicar Todos" para aplicar toda a categoria
5. **Monitore o Status**: Acompanhe o progresso na barra de status

### **Filtros e Compatibilidade**
- Marque "Mostrar apenas compatíveis" para ver apenas tweaks para seu hardware
- Tweaks incompatíveis ficam desabilitados automaticamente
- Informações de hardware são mostradas no cabeçalho

## 📁 Estrutura do Projeto

```
GamingTweaksManager/
├── Models/                 # Modelos de dados
│   ├── TweakItem.cs       # Modelo de tweak individual
│   └── TweakCategory.cs   # Modelo de categoria
├── Services/              # Serviços de negócio
│   ├── HardwareDetectionService.cs    # Detecção de hardware
│   ├── TweakExecutionService.cs       # Execução de tweaks
│   └── TweakLoaderService.cs          # Carregamento de tweaks
├── ViewModels/            # ViewModels MVVM
│   └── MainViewModel.cs   # ViewModel principal
├── Tweaks/                # Arquivos JSON com tweaks
│   └── [280+ arquivos]    # Tweaks organizados
├── MainWindow.xaml        # Interface principal
├── App.xaml              # Configuração da aplicação
└── README.md             # Este arquivo
```

## 🔧 Tecnologias Utilizadas

- **Framework**: .NET 8.0 WPF
- **UI**: Material Design In XAML Toolkit
- **Arquitetura**: MVVM (Model-View-ViewModel)
- **Serialização**: Newtonsoft.Json
- **Hardware Detection**: System.Management (WMI)
- **Registry**: Microsoft.Win32.Registry

## ⚠️ Avisos Importantes

### **Segurança**
- **SEMPRE execute como Administrador** para aplicar tweaks
- **Crie backups** antes de aplicar tweaks em lote
- **Teste individualmente** tweaks críticos antes da aplicação em massa

### **Compatibilidade**
- Tweaks são categorizados por compatibilidade (NVIDIA/AMD/Intel/Universal)
- Alguns tweaks podem não ser compatíveis com versões específicas do Windows
- **Recomendado**: Teste em ambiente controlado antes de usar em sistema principal

### **Responsabilidade**
- Use por sua própria conta e risco
- Sempre mantenha backups do sistema
- O desenvolvedor não se responsabiliza por danos ao sistema

## 🎯 Categorias Detalhadas

### **🎮 GPU Tweaks**
- Desabilitar telemetria NVIDIA/AMD
- Otimizações DirectX e Direct3D
- Configurações de latência de GPU
- Tweaks específicos por fabricante

### **⚡ CPU & Performance**
- Prioridades de processos do sistema
- Configurações de power management do CPU
- Otimizações de scheduler
- Tweaks Intel/AMD específicos

### **🔋 Power Management**
- Configurações de energia para máximo desempenho
- Redução de latência do sistema
- Desabilitar recursos de economia de energia
- Otimizações de C-States

### **🌐 Network & Internet**
- Otimizações TCP/IP
- Redução de latência de rede
- Configurações de buffer de rede
- Tweaks de DNS e conectividade

### **🛠️ System Optimization**
- Desabilitar serviços desnecessários
- Otimizações de I/O
- Configurações de sistema para gaming
- Tweaks de responsividade

### **🧹 Cleanup Tools**
- Remoção de software desnecessário
- Limpeza de arquivos temporários
- Desinstalação de bloatware
- Ferramentas de manutenção

## 📞 Suporte

Para suporte técnico ou dúvidas:
- **Desenvolvedor**: Capitão Henrique Gaming Solutions
- **Versão**: 1.0.0
- **Data**: 2024

## 📄 Licença

© 2024 Capitão Henrique Gaming Solutions. Todos os direitos reservados.

---

**🎮 Maximize seu desempenho nos jogos competitivos com o Gaming Tweaks Manager!**

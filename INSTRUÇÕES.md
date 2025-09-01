# 🎮 Gaming Tweaks Manager - Instruções de Instalação e Uso

## 📋 Pré-requisitos

### **1. .NET 8.0 Runtime**
O aplicativo requer o .NET 8.0 Desktop Runtime para funcionar.

**Opção A - Instalação Automática:**
- Execute o arquivo `install-dotnet.bat` como Administrador
- O script baixará e instalará automaticamente o .NET 8.0

**Opção B - Instalação Manual:**
- Acesse: https://dotnet.microsoft.com/download/dotnet/8.0
- Baixe o "Desktop Runtime" para Windows x64
- Execute o instalador

### **2. Permissões de Administrador**
⚠️ **CRÍTICO**: O aplicativo DEVE ser executado como Administrador para aplicar os tweaks.

## 🚀 Como Compilar (Para Desenvolvedores)

### **Método 1 - Script Automático:**
1. Execute `build.bat` como Administrador
2. O script verificará dependências e compilará automaticamente

### **Método 2 - Manual:**
```bash
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build --configuration Release

# Publicar
dotnet publish --configuration Release --output "./bin/Release/Publish"
```

## 🎯 Como Usar o Aplicativo

### **1. Executar o Aplicativo**
- **SEMPRE** clique com botão direito no executável
- Selecione "Executar como administrador"
- Confirme no UAC (Controle de Conta de Usuário)

### **2. Interface Principal**

#### **Painel Esquerdo - Categorias:**
- **🎮 GPU Tweaks**: Otimizações NVIDIA/AMD/DirectX
- **⚡ CPU & Performance**: Tweaks de processador e desempenho
- **🔋 Power Management**: Configurações de energia e latência
- **🌐 Network & Internet**: Otimizações de rede
- **🛠️ System Optimization**: Tweaks gerais do sistema
- **🧹 Cleanup Tools**: Ferramentas de limpeza

#### **Painel Direito - Tweaks:**
- Lista todos os tweaks da categoria selecionada
- Mostra compatibilidade (NVIDIA/AMD/Intel/Universal)
- Indica nível de risco e impacto

### **3. Aplicar Tweaks**

#### **Aplicação Individual:**
1. Selecione uma categoria no painel esquerdo
2. Clique em "Aplicar" no tweak desejado
3. Aguarde a confirmação de sucesso

#### **Aplicação em Lote:**
1. Selecione uma categoria
2. Clique em "Aplicar Todos" na categoria
3. Confirme a operação
4. Aguarde a conclusão de todos os tweaks

### **4. Filtros e Compatibilidade**
- Marque "Mostrar apenas compatíveis" para ver apenas tweaks para seu hardware
- O aplicativo detecta automaticamente NVIDIA/AMD/Intel
- Tweaks incompatíveis ficam desabilitados

## 🔒 Segurança e Backup

### **Sistema de Backup Automático**
- Cada tweak cria backup automático das chaves de registro
- Backups são salvos em: `%AppData%\GamingTweaksManager\Backups\`
- Use "Criar Backup" para backup completo do sistema

### **Níveis de Risco**
- **🟢 Baixo**: Tweaks seguros, modificações menores
- **🟡 Médio**: Tweaks que desabilitam serviços
- **🔴 Alto**: Tweaks que removem/deletam componentes

## ⚠️ Avisos Importantes

### **Antes de Usar:**
1. **Crie um ponto de restauração do Windows**
2. **Faça backup completo do registro**
3. **Teste em ambiente controlado primeiro**

### **Durante o Uso:**
- Aplique tweaks gradualmente
- Reinicie o sistema após aplicar tweaks críticos
- Monitore o desempenho após cada aplicação

### **Em Caso de Problemas:**
1. Use os backups automáticos criados pelo aplicativo
2. Restaure ponto de restauração do Windows
3. Execute `sfc /scannow` no Prompt de Comando (Admin)

## 🎯 Tweaks Recomendados por Categoria

### **Para Jogos Competitivos (FPS/MOBA):**
1. **GPU Tweaks**: Desabilitar telemetria, otimizar DirectX
2. **Power Management**: Configurar para máximo desempenho
3. **Network**: Reduzir latência TCP/IP
4. **System**: Desabilitar serviços desnecessários

### **Para Streaming:**
1. **CPU & Performance**: Otimizar prioridades de processo
2. **System**: Configurar scheduler para multitarefa
3. **Cleanup**: Remover bloatware para liberar recursos

### **Para Máximo FPS:**
1. **GPU Tweaks**: Todos os tweaks de GPU
2. **Power Management**: Desabilitar economia de energia
3. **System**: Otimizar I/O e responsividade

## 📞 Suporte e Solução de Problemas

### **Problemas Comuns:**

**"Aplicativo não inicia"**
- Instale o .NET 8.0 Desktop Runtime
- Execute como Administrador

**"Tweaks não aplicam"**
- Verifique se está executando como Administrador
- Desabilite temporariamente o antivírus
- Verifique se o UAC está habilitado

**"Sistema instável após tweaks"**
- Restaure backup automático
- Use ponto de restauração do Windows
- Aplique tweaks gradualmente

### **Logs e Diagnóstico:**
- Logs são salvos em: `%AppData%\GamingTweaksManager\Logs\`
- Use o Visualizador de Eventos do Windows
- Monitore a barra de status do aplicativo

## 🏆 Dicas de Otimização

### **Ordem Recomendada de Aplicação:**
1. **Cleanup Tools** (limpar sistema primeiro)
2. **System Optimization** (configurações base)
3. **Power Management** (energia e latência)
4. **GPU/CPU Tweaks** (otimizações específicas)
5. **Network** (por último, para não afetar downloads)

### **Após Aplicar Tweaks:**
- Reinicie o sistema
- Teste jogos favoritos
- Monitore temperaturas
- Verifique estabilidade por 24h

---

**🎮 Desenvolvido por Capitão Henrique Gaming Solutions**
**📧 Para suporte: Entre em contato através dos canais oficiais**

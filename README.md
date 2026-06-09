# 🍰 Red Velvet — Rede Social em Grafos

Aplicação desktop em **C# WinForms (.NET 8)** que simula uma rede social usando estruturas de grafos.  
Trabalho da disciplina **Algoritmos e Estruturas de Dados III — UNISAPIENS**.

---

## ✨ Funcionalidades

| Aba | O que faz |
|-----|-----------|
| **↔ Amizades** | Adiciona/remove amizades com nível de proximidade (1–10). Grafo **não-direcionado**. |
| **⟿ BFS** | Busca em Largura — encontra o **caminho mais curto** entre dois usuários. `O(V+E)` |
| **↗ Seguidores** | Seguir/deixar de seguir. Grafo **direcionado**. |
| **✦ Recomendações** | Sugere amigos com base em **amigos em comum**. |
| **▦ Stats** | Vértices, arestas, grau médio, hubs, conectividade via **DFS**. |

**Sidebar esquerda** — lista de usuários com avatar colorido, grau e contagem de seguidores.  
**Sidebar direita** — detalhes do usuário selecionado, lista de adjacência e grau dos vértices.

---

## 🚀 Como rodar

### Pré-requisito
- [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado
- Windows (WinForms é exclusivo para Windows)

### Passos

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/red-velvet.git
cd red-velvet

# Rode o projeto
dotnet run
```

Ou abra a pasta no **Visual Studio / VS Code** e pressione `F5`.

---

## 🗂 Estrutura dos arquivos

```
RedVelvetApp/
├── RedVelvetApp.csproj   # Configuração do projeto .NET 8 WinForms
├── Program.cs            # Entry point
├── RedeSocial.cs         # Back-end: grafo, BFS, DFS, seguidores, recomendações
└── MainForm.cs           # Front-end: interface WinForms completa
```

---

## 🧠 Algoritmos implementados

- **BFS (Busca em Largura)** — caminho mais curto entre dois nós — `O(V + E)`
- **DFS (Busca em Profundidade)** — verificação de conectividade do grafo — `O(V + E)`
- **Grafo não-direcionado** — amizades bidirecionais com peso (nível)
- **Grafo direcionado** — relação de seguimento (seguidor → seguido)
- **Recomendação** — amigos de amigos com contagem de conexões em comum

---

## 👩‍💻 Tecnologias

- C# 12 / .NET 8
- Windows Forms (WinForms)
- Sem dependências externas (NuGet)

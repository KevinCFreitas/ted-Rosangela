

# Red Velvet — Rede Social com Grafos

> Aplicação desktop em **C# WinForms (.NET 8)** que simula uma rede social utilizando estruturas de grafos não-direcionados e direcionados, com implementação dos algoritmos BFS e DFS.
> Trabalho prático da disciplina **Algoritmos e Estruturas de Dados III — UNISAPIENS**

---

## Funcionalidades

| Aba | Descrição | Tipo de Grafo |
|-----|-----------|---------------|
|  Grafo | Visualização interativa com nós arrastáveis e arestas com peso | Não-direcionado |
|  BFS | Caminho mais curto entre dois usuários destacado em tempo real | Não-direcionado |
|  Seguidores | Seguir / deixar de seguir outros usuários | Direcionado |
|  Recomendações | Sugestão de amigos com base em amigos em comum | Não-direcionado |

**Sidebar esquerda** — lista de usuários com avatar colorido, grau e contagem de seguidores
**Sidebar direita** — detalhes do usuário selecionado: amigos, seguidores, lista de adjacência e grau dos vértices

---

## Algoritmos

### BFS — Busca em Largura 
Utilizado para encontrar o **caminho mais curto** entre dois usuários no grafo de amizades. Explora os vértices por níveis usando uma fila (`Queue`), garantindo que o primeiro caminho encontrado seja o menor. O caminho é reconstruído via dicionário de predecessores e destacado visualmente no grafo.

### DFS — Busca em Profundidade
Utilizado para **verificar a conectividade** do grafo — se todos os usuários estão acessíveis a partir de um nó inicial. Percorre recursivamente todos os vizinhos não visitados a partir do primeiro vértice.

### Recomendação de Amigos
Sugere usuários com **amigos em comum** com o alvo mas que ainda não são amigos diretos. Os candidatos são ordenados pelo número de conexões em comum.

---

## Modelagem

### Grafo Não-Direcionado (Amizades)
Cada aresta possui peso de **1 a 10** representando o nível de proximidade.
```
Dictionary<string, Pessoa>
  └── Pessoa.Amigos: Dictionary<string, int>  // amigo → nível
```

### Grafo Direcionado (Seguidores)
A aresta A → B significa que A segue B, sem reciprocidade obrigatória.
```
Dictionary<string, HashSet<string>>  // seguidor → conjunto de seguidos
```

---

## Estrutura do Projeto

```
RedVelvetApp/
├── RedVelvetApp.csproj   → Configuração do projeto .NET 8 WinForms
├── Program.cs            → Entry point
├── RedeSocial.cs         → Back-end: grafo, BFS, DFS, seguidores, recomendações
└── MainForm.cs           → Front-end: interface WinForms com grafo desenhado via Graphics
```

---

## Como Executar

**Pré-requisito:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado (Windows)

```bash
# Clone o repositório
git clone https://github.com/KevinCFreitas/ted-Rosangela.git
cd ted-Rosangela

# Execute
dotnet run
```

Ou abra no **Visual Studio / VS Code** e pressione `F5`.

---

## 🔢 Dados de Exemplo

Ao iniciar, a aplicação carrega automaticamente:

**Usuários:** Ana, Bruno, Carlos, Diana, Eduardo, Fernanda

**Amizades:**
```
Ana ↔ Bruno    (N10)      Bruno ↔ Diana    (N8)
Ana ↔ Carlos   (N5)       Carlos ↔ Diana   (N6)
Diana ↔ Eduardo (N9)      Eduardo ↔ Fernanda (N7)
```

**Seguidores:**
```
Ana → Bruno      Bruno → Diana
Carlos → Ana     Carlos → Fernanda
```

---

## Tecnologias

| Tecnologia | Detalhe |
|------------|---------|
| C# 12 | Linguagem principal |
| .NET 8 | Framework |
| WinForms | Interface gráfica desktop |
| System.Drawing | Desenho do grafo (Graphics, PictureBox) |
| Git / GitHub | Controle de versão |


*Red Velvet — Algoritmos e Estruturas de Dados III · UNISAPIENS*
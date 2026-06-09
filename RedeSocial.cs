using System;
using System.Collections.Generic;
using System.Linq;

namespace RedVelvetApp
{
    public class Pessoa
    {
        public string Nome { get; set; }
        public Dictionary<string, int> Amigos { get; set; } = new();
        public int CorIndex { get; set; }
        public Pessoa(string nome, int corIndex = 0) { Nome = nome; CorIndex = corIndex; }
    }

    public class RedeSocial
    {
        private readonly Dictionary<string, Pessoa> pessoas = new();
        private readonly Dictionary<string, HashSet<string>> seguidores = new();
        private int _cor = 0;

        public event Action? GrafoAlterado;
        public IReadOnlyDictionary<string, Pessoa> Pessoas => pessoas;
        public IReadOnlyDictionary<string, HashSet<string>> Seguidores => seguidores;

        // ── USUÁRIOS ─────────────────────────────────────────────────

        public (bool ok, string msg) AdicionarPessoa(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return (false, "Nome não pode ser vazio.");
            if (pessoas.ContainsKey(nome))
                return (false, $"'{nome}' já existe na rede.");
            pessoas[nome]    = new Pessoa(nome, _cor++ % 8);
            seguidores[nome] = new HashSet<string>();
            GrafoAlterado?.Invoke();
            return (true, $"'{nome}' adicionado(a) à rede.");
        }

        public (bool ok, string msg) RemoverPessoa(string nome)
        {
            if (!Existe(nome, out string err)) return (false, err);
            pessoas.Remove(nome);
            seguidores.Remove(nome);
            foreach (var p in pessoas.Values)    p.Amigos.Remove(nome);
            foreach (var s in seguidores.Values) s.Remove(nome);
            GrafoAlterado?.Invoke();
            return (true, $"'{nome}' removido(a).");
        }

        // ── AMIZADES (grafo não-direcionado) ─────────────────────────

        public (bool ok, string msg) AdicionarAmizade(string p1, string p2, int nivel)
        {
            if (!Existe(p1, out string e1)) return (false, e1);
            if (!Existe(p2, out string e2)) return (false, e2);
            if (p1 == p2) return (false, "Uma pessoa não pode ser amiga de si mesma.");
            pessoas[p1].Amigos[p2] = nivel;
            pessoas[p2].Amigos[p1] = nivel;
            GrafoAlterado?.Invoke();
            return (true, $"Amizade '{p1}' ↔ '{p2}' (nível {nivel}) criada.");
        }

        public (bool ok, string msg) RemoverAmizade(string p1, string p2)
        {
            if (!Existe(p1, out string e1)) return (false, e1);
            if (!Existe(p2, out string e2)) return (false, e2);
            pessoas[p1].Amigos.Remove(p2);
            pessoas[p2].Amigos.Remove(p1);
            GrafoAlterado?.Invoke();
            return (true, $"Amizade '{p1}' ↔ '{p2}' removida.");
        }

        // ── SEGUIDORES (grafo direcionado) ────────────────────────────

        public (bool ok, string msg) Seguir(string quem, string alvo)
        {
            if (!Existe(quem, out string e1)) return (false, e1);
            if (!Existe(alvo, out string e2)) return (false, e2);
            if (quem == alvo) return (false, "Uma pessoa não pode se seguir.");
            bool novo = seguidores[quem].Add(alvo);
            GrafoAlterado?.Invoke();
            return novo
                ? (true,  $"'{quem}' agora segue '{alvo}'.")
                : (false, $"'{quem}' já segue '{alvo}'.");
        }

        public (bool ok, string msg) Desseguir(string quem, string alvo)
        {
            if (!Existe(quem, out string e1)) return (false, e1);
            if (!Existe(alvo, out string e2)) return (false, e2);
            bool rem = seguidores[quem].Remove(alvo);
            GrafoAlterado?.Invoke();
            return rem
                ? (true,  $"'{quem}' deixou de seguir '{alvo}'.")
                : (false, $"'{quem}' não seguia '{alvo}'.");
        }

        public List<string> ObterSeguindo(string nome)
            => Existe(nome, out _) ? seguidores[nome].ToList() : new();

        public List<string> ObterSeguidores(string nome)
            => seguidores.Where(kv => kv.Value.Contains(nome)).Select(kv => kv.Key).ToList();

        // ── AMIGOS EM COMUM ───────────────────────────────────────────

        public List<string> AmigosEmComum(string p1, string p2)
        {
            if (!Existe(p1, out _) || !Existe(p2, out _)) return new();
            return pessoas[p1].Amigos.Keys.Intersect(pessoas[p2].Amigos.Keys).ToList();
        }

        // ── BFS — CAMINHO MAIS CURTO — O(V+E) ────────────────────────

        public List<string> CaminhoMaisCurto(string origem, string destino)
        {
            if (!Existe(origem, out _) || !Existe(destino, out _)) return new();
            if (origem == destino) return new() { origem };

            var visitado    = new HashSet<string> { origem };
            var fila        = new Queue<string>();
            var predecessor = new Dictionary<string, string?>();

            fila.Enqueue(origem);
            predecessor[origem] = null;

            while (fila.Count > 0)
            {
                string atual = fila.Dequeue();
                foreach (var vizinho in pessoas[atual].Amigos.Keys)
                {
                    if (visitado.Contains(vizinho)) continue;
                    visitado.Add(vizinho);
                    predecessor[vizinho] = atual;
                    if (vizinho == destino)
                    {
                        var caminho = new List<string>();
                        string? no = destino;
                        while (no != null) { caminho.Add(no); predecessor.TryGetValue(no, out no); }
                        caminho.Reverse();
                        return caminho;
                    }
                    fila.Enqueue(vizinho);
                }
            }
            return new();
        }

        // ── DFS — VERIFICAR CONECTIVIDADE — O(V+E) ───────────────────

        public (bool Conectado, List<string> Visitados, List<string> Isolados) VerificarConectividade()
        {
            if (pessoas.Count == 0) return (true, new(), new());
            var visitado = new HashSet<string>();
            DFS(pessoas.Keys.First(), visitado);
            bool ok = visitado.Count == pessoas.Count;
            return (ok, visitado.ToList(), pessoas.Keys.Except(visitado).ToList());
        }

        private void DFS(string atual, HashSet<string> visitado)
        {
            visitado.Add(atual);
            foreach (var viz in pessoas[atual].Amigos.Keys)
                if (!visitado.Contains(viz)) DFS(viz, visitado);
        }

        // ── RECOMENDAÇÕES ─────────────────────────────────────────────

        public List<(string Candidato, int EmComum)> RecomendarAmigos(string nome)
        {
            if (!Existe(nome, out _)) return new();
            var diretos = new HashSet<string>(pessoas[nome].Amigos.Keys) { nome };
            return pessoas[nome].Amigos.Keys
                .SelectMany(am => pessoas[am].Amigos.Keys)
                .Where(c => !diretos.Contains(c))
                .GroupBy(c => c)
                .Select(g => (g.Key, g.Count()))
                .OrderByDescending(t => t.Item2)
                .ToList();
        }

        // ── ESTATÍSTICAS ──────────────────────────────────────────────

        public (int V, int E, double GrauMedio, int GrauMax, List<string> Hubs, int ADir) Estatisticas()
        {
            int v  = pessoas.Count;
            int e  = pessoas.Values.Sum(p => p.Amigos.Count) / 2;
            var gs = pessoas.Values.Select(p => p.Amigos.Count).ToList();
            double gm  = gs.Count > 0 ? gs.Average() : 0;
            int gmax   = gs.Count > 0 ? gs.Max() : 0;
            var hubs   = pessoas.Where(kv => kv.Value.Amigos.Count == gmax).Select(kv => kv.Key).ToList();
            int aDir   = seguidores.Values.Sum(s => s.Count);
            return (v, e, gm, gmax, hubs, aDir);
        }

        // ── HELPER ────────────────────────────────────────────────────

        private bool Existe(string nome, out string erro)
        {
            if (pessoas.ContainsKey(nome)) { erro = ""; return true; }
            erro = $"Usuário '{nome}' não encontrado.";
            return false;
        }

        // ── DADOS DE EXEMPLO ──────────────────────────────────────────

        public void CarregarExemplo()
        {
            foreach (var n in new[]{"Ana","Bruno","Carlos","Diana","Eduardo","Fernanda"})
                AdicionarPessoa(n);

            AdicionarAmizade("Ana",     "Bruno",    10);
            AdicionarAmizade("Ana",     "Carlos",   5);
            AdicionarAmizade("Bruno",   "Diana",    8);
            AdicionarAmizade("Carlos",  "Diana",    6);
            AdicionarAmizade("Diana",   "Eduardo",  9);
            AdicionarAmizade("Eduardo", "Fernanda", 7);

            Seguir("Ana",    "Bruno");
            Seguir("Bruno",  "Diana");
            Seguir("Carlos", "Ana");
            Seguir("Carlos", "Fernanda");
        }
    }
}

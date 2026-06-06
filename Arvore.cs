namespace JogoAdivinhacao;

public class No
{
    public string Conteudo   { get; set; }
    public bool   EhResposta { get; set; }
    public No?    FilhoSim   { get; set; }
    public No?    FilhoNao   { get; set; }

    public No(string conteudo, bool ehResposta = false)
    {
        Conteudo   = conteudo;
        EhResposta = ehResposta;
    }
}

public class ArvoreBinariaDecisao
{
    public No Raiz { get; private set; }

    public ArvoreBinariaDecisao()
    {
        var aguia    = new No("Águia",    true);
        var morcego  = new No("Morcego",  true);
        var urso     = new No("Urso",     true);
        var peixe    = new No("Peixe",    true);
        var golfinho = new No("Golfinho", true);

        var temPenas = new No("Tem penas?");
        temPenas.FilhoSim = aguia;
        temPenas.FilhoNao = morcego;

        var temAsas = new No("Tem asas?");
        temAsas.FilhoSim = temPenas;
        temAsas.FilhoNao = urso;

        var temEscamas = new No("Tem escamas?");
        temEscamas.FilhoSim = peixe;
        temEscamas.FilhoNao = golfinho;

        Raiz = new No("Vive na água?");
        Raiz.FilhoSim = temEscamas;
        Raiz.FilhoNao = temAsas;
    }

    public void Aprender(No folha, string novoAnimal, string novaPergunta, bool respostaParaNovoAnimal)
    {
        var antiga = new No(folha.Conteudo, true);
        var novoNo = new No(novoAnimal, true);

        folha.Conteudo   = novaPergunta;
        folha.EhResposta = false;

        if (respostaParaNovoAnimal)
        { folha.FilhoSim = novoNo; folha.FilhoNao = antiga; }
        else
        { folha.FilhoSim = antiga; folha.FilhoNao = novoNo; }
    }

    public int ContarAnimais(No? no = null)
    {
        no ??= Raiz;
        if (no.EhResposta) return 1;
        return ContarAnimais(no.FilhoSim) + ContarAnimais(no.FilhoNao);
    }

    public string GerarTextoArvore()
    {
        var sb = new System.Text.StringBuilder();
        PreOrdem(Raiz, "", true, sb, true);
        return sb.ToString();
    }

    private void PreOrdem(No? no, string prefixo, bool ehRaiz, System.Text.StringBuilder sb, bool ehSim)
    {
        if (no == null) return;
        string tipo = no.EhResposta ? "🐾 " : "❓ ";
        if (ehRaiz)
            sb.AppendLine($"RAIZ: {tipo}{no.Conteudo}");
        else
            sb.AppendLine($"{prefixo}{(ehSim ? "├─ SIM→ " : "└─ NÃO→ ")}{tipo}{no.Conteudo}");

        string novoPfx = prefixo + (ehSim ? "│        " : "         ");
        PreOrdem(no.FilhoSim, novoPfx, false, sb, true);
        PreOrdem(no.FilhoNao, novoPfx, false, sb, false);
    }
}
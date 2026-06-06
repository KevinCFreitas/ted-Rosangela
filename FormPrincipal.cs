namespace JogoAdivinhacao;

public class FormPrincipal : Form
{
    static readonly Color CCorFundo      = Color.FromArgb(245, 245, 242);
    static readonly Color CCorCard       = Color.White;
    static readonly Color CBordaCard     = Color.FromArgb(220, 218, 210);
    static readonly Color CVerdeClaro    = Color.FromArgb(225, 245, 238);
    static readonly Color CVerdeTxt      = Color.FromArgb(15, 110, 86);
    static readonly Color CVermelhoClaro = Color.FromArgb(250, 236, 231);
    static readonly Color CVermTxt       = Color.FromArgb(153, 60, 29);
    static readonly Color CAmarelClaro   = Color.FromArgb(250, 238, 218);
    static readonly Color CAmarelTxt     = Color.FromArgb(99, 56, 6);
    static readonly Color CTexto         = Color.FromArgb(30, 30, 28);
    static readonly Color CTextoSecund   = Color.FromArgb(100, 100, 96);
    static readonly Color CAzulClaro     = Color.FromArgb(230, 241, 251);
    static readonly Color CAzulTxt       = Color.FromArgb(12, 68, 124);

    Label lblAcertos = null!, lblErros = null!, lblAnimais = null!;
    int acertos = 0, erros = 0;

    ArvoreBinariaDecisao arvore = null!;
    No atual = null!;

    Panel pnlGame  = null!, pnlLearn = null!, pnlResult = null!;
    Label lblBadgeGame = null!, lblPergunta = null!, lblResultado = null!;
    Button btnSim = null!, btnNao = null!;
    TextBox txtAnimal = null!, txtPerguntaLearn = null!, txtArvore = null!;
    Button btnLearnSim = null!, btnLearnNao = null!, btnConfirmar = null!;
    bool? learnAnswer = null;
    bool treeVisible = false;

    public FormPrincipal()
    {
        InitUI();
        NovaPartida();
    }

    void InitUI()
    {
        Text            = "Jogo da Adivinhação — Árvore Binária";
        ClientSize      = new Size(500, 700);
        MinimumSize     = new Size(500, 600);
        BackColor       = CCorFundo;
        Font            = new Font("Segoe UI", 10f);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;

        var main = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoScroll    = true,
            Padding       = new Padding(20, 16, 20, 16),
            BackColor     = CCorFundo,
        };
        Controls.Add(main);

        int W = 460;

        // Cabeçalho
        main.Controls.Add(new Label
        {
            Text = "Jogo da Adivinhação",
            Font = new Font("Segoe UI", 17f, FontStyle.Bold),
            ForeColor = CTexto, BackColor = Color.Transparent,
            AutoSize = false, Width = W, Height = 34,
        });
        main.Controls.Add(new Label
        {
            Text = "Pense em um animal. Vou tentar adivinhar!",
            Font = new Font("Segoe UI", 10f),
            ForeColor = CTextoSecund, BackColor = Color.Transparent,
            AutoSize = false, Width = W, Height = 22,
        });
        main.Controls.Add(Gap(W, 12));

        // Placar
        var placar = new TableLayoutPanel
        {
            ColumnCount = 3, RowCount = 1,
            Width = W, Height = 68, BackColor = CCorFundo,
        };
        placar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        placar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        placar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        placar.Controls.Add(MakeScoreCard("0", "Acertos", ref lblAcertos), 0, 0);
        placar.Controls.Add(MakeScoreCard("0", "Erros",   ref lblErros),   1, 0);
        placar.Controls.Add(MakeScoreCard("5", "Animais", ref lblAnimais), 2, 0);
        main.Controls.Add(placar);
        main.Controls.Add(Gap(W, 10));

        // Painel Jogo
        pnlGame      = new Panel { Width = W, BackColor = CCorCard, Padding = new Padding(14) };
        lblBadgeGame = MakeBadge("  ❓  Pergunta", CAzulClaro, CAzulTxt, W - 28);
        lblPergunta  = new Label
        {
            Text = "", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = CTexto, BackColor = CCorCard,
            AutoSize = false, Width = W - 28, Height = 50,
        };
        btnSim = MakeBtn("✔  Sim", CVerdeClaro, CVerdeTxt, (W - 28 - 10) / 2);
        btnNao = MakeBtn("✖  Não", CVermelhoClaro, CVermTxt, (W - 28 - 10) / 2);
        btnSim.Click += (_, _) => Responder(true);
        btnNao.Click += (_, _) => Responder(false);
        var rowGame = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Width = W - 28, Height = 44, BackColor = CCorCard, WrapContents = false,
        };
        rowGame.Controls.Add(btnSim);
        rowGame.Controls.Add(Gap(10, 44, CCorCard));
        rowGame.Controls.Add(btnNao);
        BuildCard(pnlGame, W, lblBadgeGame, lblPergunta, rowGame);
        main.Controls.Add(pnlGame);
        main.Controls.Add(Gap(W, 8));

        // Painel Aprender
        pnlLearn    = new Panel { Width = W, BackColor = CCorCard, Padding = new Padding(14), Visible = false };
        txtAnimal        = MakeTextBox(W - 28);
        txtPerguntaLearn = MakeTextBox(W - 28);
        btnLearnSim = MakeBtn("✔  Sim", CVerdeClaro, CVerdeTxt, (W - 28 - 10) / 2);
        btnLearnNao = MakeBtn("✖  Não", CVermelhoClaro, CVermTxt, (W - 28 - 10) / 2);
        btnLearnSim.Click += (_, _) => { learnAnswer = true;  AtualizarBotoesLearn(); };
        btnLearnNao.Click += (_, _) => { learnAnswer = false; AtualizarBotoesLearn(); };
        var rowLearn = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Width = W - 28, Height = 44, BackColor = CCorCard, WrapContents = false,
        };
        rowLearn.Controls.Add(btnLearnSim);
        rowLearn.Controls.Add(Gap(10, 44, CCorCard));
        rowLearn.Controls.Add(btnLearnNao);
        btnConfirmar = MakeBtn("Confirmar e salvar ✓", CVerdeClaro, CVerdeTxt, W - 28);
        btnConfirmar.Enabled   = false;
        btnConfirmar.ForeColor = Color.FromArgb(160, 200, 180);
        btnConfirmar.Click    += (_, _) => ConfirmarAprendizado();
        txtAnimal.TextChanged        += (_, _) => AtualizarBotoesLearn();
        txtPerguntaLearn.TextChanged += (_, _) => AtualizarBotoesLearn();
        BuildCard(pnlLearn, W,
            MakeBadge("  💡  Aprendendo", CAmarelClaro, CAmarelTxt, W - 28),
            new Label { Text = "Não sei! Me ensine. 🤔", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = CTexto, BackColor = CCorCard, AutoSize = false, Width = W - 28, Height = 28 },
            MakeLbl("Qual era o animal?", W - 28), txtAnimal,
            MakeLbl("Uma pergunta que diferencia seu animal do que eu errei:", W - 28), txtPerguntaLearn,
            MakeLbl("Para o seu animal, a resposta a essa pergunta seria:", W - 28),
            rowLearn, btnConfirmar);
        main.Controls.Add(pnlLearn);
        main.Controls.Add(Gap(W, 8));

        // Painel Resultado
        pnlResult    = new Panel { Width = W, BackColor = CCorCard, Padding = new Padding(14), Visible = false };
        lblResultado = new Label { Text = "", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = CTexto, BackColor = CCorCard, AutoSize = false, Width = W - 28, Height = 50 };
        var btnDeNovo = MakeBtn("↺  Jogar de novo", Color.FromArgb(241, 239, 232), CTextoSecund, W - 28);
        btnDeNovo.Click += (_, _) => NovaRodada();
        BuildCard(pnlResult, W,
            MakeBadge("  🏆  Resultado", CVerdeClaro, CVerdeTxt, W - 28),
            lblResultado, btnDeNovo);
        main.Controls.Add(pnlResult);
        main.Controls.Add(Gap(W, 8));

        // Seção Árvore
        var rowTree = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Width = W, Height = 36, BackColor = CCorFundo, WrapContents = false,
        };
        rowTree.Controls.Add(new Label
        {
            Text = "Estrutura da árvore", Font = new Font("Segoe UI", 9f),
            ForeColor = CTextoSecund, BackColor = CCorFundo,
            AutoSize = false, Width = W - 160, Height = 36,
            TextAlign = ContentAlignment.MiddleLeft,
        });
        var btnToggle = MakeBtn("Ver / Esconder", Color.FromArgb(241, 239, 232), CTextoSecund, 150);
        btnToggle.Height = 32;
        btnToggle.Click += (_, _) => ToggleTree();
        rowTree.Controls.Add(btnToggle);
        main.Controls.Add(rowTree);

        txtArvore = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Width = W, Height = 170,
            BackColor = Color.FromArgb(241, 239, 232), ForeColor = CTextoSecund,
            Font = new Font("Consolas", 9f), BorderStyle = BorderStyle.FixedSingle,
            Visible = false,
        };
        main.Controls.Add(txtArvore);
    }

    // Helpers de layout
    Panel Gap(int w, int h, Color? cor = null) =>
        new Panel { Width = w, Height = h, BackColor = cor ?? CCorFundo };

    Label MakeLbl(string text, int width) =>
        new Label { Text = text, Font = new Font("Segoe UI", 9f), ForeColor = CTextoSecund, BackColor = CCorCard, AutoSize = false, Width = width, Height = 20 };

    void BuildCard(Panel card, int W, params Control[] controls)
    {
        int y = 14;
        foreach (var c in controls)
        {
            c.Left = 14; c.Top = y; c.Width = W - 28;
            card.Controls.Add(c);
            y += c.Height + 8;
        }
        card.Height = y + 6;
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(CBordaCard, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
    }

    Label MakeBadge(string text, Color back, Color fore, int width) =>
        new Label { Text = text, AutoSize = false, Width = width, Height = 26, BackColor = back, ForeColor = fore, Font = new Font("Segoe UI", 9f), TextAlign = ContentAlignment.MiddleLeft };

    Button MakeBtn(string text, Color back, Color fore, int width)
    {
        var b = new Button
        {
            Text = text, Width = width, Height = 40, BackColor = back, ForeColor = fore,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f),
            Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter,
            UseVisualStyleBackColor = false,
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(200, 198, 190);
        b.FlatAppearance.BorderSize  = 1;
        return b;
    }

    TextBox MakeTextBox(int width) =>
        new TextBox { Width = width, Height = 28, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 247, 244), ForeColor = CTexto };

    Panel MakeScoreCard(string num, string label, ref Label numLabel)
    {
        var p = new Panel { BackColor = Color.FromArgb(241, 239, 232), Dock = DockStyle.Fill, Margin = new Padding(3) };
        p.Paint += (s, e) => { using var pen = new Pen(CBordaCard); e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
        var lNum = new Label { Text = num, Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = CTexto, TextAlign = ContentAlignment.BottomCenter, Dock = DockStyle.Top, Height = 38 };
        var lLbl = new Label { Text = label, Font = new Font("Segoe UI", 8.5f), ForeColor = CTextoSecund, TextAlign = ContentAlignment.TopCenter, Dock = DockStyle.Fill };
        numLabel = lNum;
        p.Controls.Add(lLbl);
        p.Controls.Add(lNum);
        return p;
    }

    // Lógica do jogo
    void NovaPartida()
    {
        arvore = new ArvoreBinariaDecisao();
        acertos = 0; erros = 0;
        AtualizarPlacar();
        NovaRodada();
    }

    void NovaRodada()
    {
        atual = arvore.Raiz;
        learnAnswer = null;
        if (txtAnimal        != null) txtAnimal.Text        = "";
        if (txtPerguntaLearn != null) txtPerguntaLearn.Text = "";
        MostrarTela(pnlGame);
        AtualizarNoPergunta();
    }

    void AtualizarNoPergunta()
    {
        if (atual.EhResposta)
        {
            lblBadgeGame.Text      = "  🔍  Meu palpite";
            lblBadgeGame.BackColor = CVerdeClaro;
            lblBadgeGame.ForeColor = CVerdeTxt;
            lblPergunta.Text       = "Seria... " + atual.Conteudo + "?";
        }
        else
        {
            lblBadgeGame.Text      = "  ❓  Pergunta";
            lblBadgeGame.BackColor = CAzulClaro;
            lblBadgeGame.ForeColor = CAzulTxt;
            lblPergunta.Text       = atual.Conteudo;
        }
    }

    void Responder(bool sim)
    {
        if (atual.EhResposta)
        {
            if (sim)
            {
                acertos++;
                AtualizarPlacar();
                MostrarResultado("✔  Acertei! Sabia que era " + atual.Conteudo + ".", true);
            }
            else
            {
                erros++;
                AtualizarPlacar();
                learnAnswer = null;
                if (txtAnimal        != null) txtAnimal.Text        = "";
                if (txtPerguntaLearn != null) txtPerguntaLearn.Text = "";
                AtualizarBotoesLearn();
                MostrarTela(pnlLearn);
            }
            return;
        }
        atual = sim ? atual.FilhoSim! : atual.FilhoNao!;
        AtualizarNoPergunta();
    }

    void ConfirmarAprendizado()
    {
        string novoAnimal   = txtAnimal.Text.Trim();
        string novaPergunta = txtPerguntaLearn.Text.Trim();
        if (string.IsNullOrEmpty(novoAnimal) || string.IsNullOrEmpty(novaPergunta) || learnAnswer == null) return;
        arvore.Aprender(atual, novoAnimal, novaPergunta, learnAnswer.Value);
        AtualizarPlacar();
        MostrarResultado("Aprendi! Agora sei o que é " + novoAnimal + ". 🎉", false);
        if (treeVisible) AtualizarArvoreTexto();
    }

    void MostrarResultado(string msg, bool acerto)
    {
        lblResultado.Text = msg;
        MostrarTela(pnlResult);
    }

    void AtualizarBotoesLearn()
    {
        bool ok = !string.IsNullOrWhiteSpace(txtAnimal?.Text)
               && !string.IsNullOrWhiteSpace(txtPerguntaLearn?.Text)
               && learnAnswer != null;
        if (btnConfirmar == null) return;
        btnConfirmar.Enabled   = ok;
        btnConfirmar.ForeColor = ok ? CVerdeTxt : Color.FromArgb(160, 200, 180);
        if (learnAnswer == true)
        { btnLearnSim.BackColor = CVerdeClaro;    btnLearnNao.BackColor = Color.FromArgb(241, 239, 232); }
        else if (learnAnswer == false)
        { btnLearnNao.BackColor = CVermelhoClaro; btnLearnSim.BackColor = Color.FromArgb(241, 239, 232); }
    }

    void AtualizarPlacar()
    {
        lblAcertos.Text = acertos.ToString();
        lblErros.Text   = erros.ToString();
        lblAnimais.Text = arvore.ContarAnimais().ToString();
    }

    void MostrarTela(Panel alvo)
    {
        pnlGame.Visible   = alvo == pnlGame;
        pnlLearn.Visible  = alvo == pnlLearn;
        pnlResult.Visible = alvo == pnlResult;
    }

    void ToggleTree()
    {
        treeVisible = !treeVisible;
        txtArvore.Visible = treeVisible;
        if (treeVisible) AtualizarArvoreTexto();
    }

    void AtualizarArvoreTexto() => txtArvore.Text = arvore.GerarTextoArvore();
}
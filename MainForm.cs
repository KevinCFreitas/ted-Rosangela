using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RedVelvetApp
{
    public class MainForm : Form
    {
        // ── Paleta ────────────────────────────────────────
        static readonly Color C_BG     = Color.FromArgb(245, 240, 232);
        static readonly Color C_CREAM  = Color.FromArgb(237, 231, 214);
        static readonly Color C_VELVET = Color.FromArgb(139, 26,  46);
        static readonly Color C_MIST   = Color.FromArgb(138, 127, 114);
        static readonly Color C_BORDER = Color.FromArgb(206, 197, 178);
        static readonly Color C_INK    = Color.FromArgb(26,  20,  16);
        static readonly Color C_WHITE  = Color.White;

        static readonly Color[] AVG_BG = {
            Color.FromArgb(250,234,238), Color.FromArgb(232,243,236),
            Color.FromArgb(253,245,224), Color.FromArgb(232,234,249),
            Color.FromArgb(252,232,225), Color.FromArgb(225,240,245),
            Color.FromArgb(240,232,245), Color.FromArgb(245,240,232),
        };
        static readonly Color[] AVG_FG = {
            Color.FromArgb(139,26,46),  Color.FromArgb(61,107,82),
            Color.FromArgb(200,146,42), Color.FromArgb(60,58,142),
            Color.FromArgb(139,58,26),  Color.FromArgb(26,90,139),
            Color.FromArgb(107,58,139), Color.FromArgb(107,90,58),
        };

        // ── Backend ───────────────────────────────────────
        readonly RedeSocial _rede = new();

        // ── Posições dos nós no grafo ─────────────────────
        readonly Dictionary<string, PointF> _pos = new();
        string? _dragging = null;
        PointF  _dragOffset;
        List<string> _hlPath = new();

        // ── Controles ─────────────────────────────────────
        ListBox  lstUsers   = null!;
        TextBox  txtNew     = null!;
        Label    lblSelName = null!;
        Label    lblSelGrau = null!;
        ListBox  lstRAmigos = null!;
        ListBox  lstRSegue  = null!;
        ListBox  lstRSegBy  = null!;
        ListBox  lstAdj     = null!;
        ListBox  lstGraus   = null!;

        Panel      pnlGraph   = null!;   // canvas do grafo
        PictureBox pbGraph    = null!;

        // abas
        Button[] _tabBtns  = null!;
        Panel[]  _tabPanels = null!;

        // painel gerenciar amizade (dentro do grafo)
        ComboBox cmbA1 = null!, cmbA2 = null!;
        TrackBar trkN  = null!;
        Label    lblNV = null!;
        Label    lblAM = null!;

        // painel BFS
        ComboBox     cmbBO = null!, cmbBD = null!;
        Label        lblBM = null!;
        FlowLayoutPanel flBFS = null!;

        // painel seguidores
        ComboBox cmbSQ = null!, cmbSA = null!;
        Label    lblSM = null!;
        ListBox  lstST = null!;

        // painel recomendações
        ComboBox cmbRU = null!;
        ListBox  lstRR = null!;

        // painel stats
        ListView lvStats = null!;

        // ══════════════════════════════════════════════════
        public MainForm()
        {
            SuspendLayout();
            Build();
            ResumeLayout(false);
            PerformLayout();
            _rede.GrafoAlterado += () => { if (InvokeRequired) Invoke(RefreshAll); else RefreshAll(); };
            _rede.CarregarExemplo();
        }

        // ══════════════════════════════════════════════════
        //  BUILD
        // ══════════════════════════════════════════════════
        void Build()
        {
            Text          = "Red Velvet — Rede Social";
            Size          = new Size(1280, 800);
            MinimumSize   = new Size(1060, 680);
            BackColor     = C_BG;
            Font          = new Font("Segoe UI", 9f);
            StartPosition = FormStartPosition.CenterScreen;

            // HEADER
            var hdr = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_CREAM };
            hdr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(C_BORDER), 0, 57, hdr.Width, 57);
            hdr.Controls.Add(new Label { Text="Red Velvet", Left=20, Top=8, AutoSize=true, Font=new Font("Georgia",22f,FontStyle.Bold), ForeColor=C_VELVET });
            hdr.Controls.Add(new Label { Text="GRAFO SOCIAL", Left=178, Top=16, AutoSize=true, Font=new Font("Courier New",9f), ForeColor=C_MIST });
            hdr.Controls.Add(new Label { Text="Algoritmos e Estruturas de Dados III  ·  UNISAPIENS", Left=740, Top=12, AutoSize=true, Font=new Font("Courier New",8f), ForeColor=C_MIST });
            Controls.Add(hdr);

            // BODY
            var body = new Panel { Dock=DockStyle.Fill, BackColor=C_BG };
            Controls.Add(body);

            // sidebar esquerda
            var left = BuildLeft();
            left.Dock = DockStyle.Left; left.Width = 260;
            body.Controls.Add(left);

            // sidebar direita
            var right = BuildRight();
            right.Dock = DockStyle.Right; right.Width = 260;
            body.Controls.Add(right);

            // centro
            var center = BuildCenter();
            center.Dock = DockStyle.Fill;
            body.Controls.Add(center);

            body.Controls.SetChildIndex(left,   0);
            body.Controls.SetChildIndex(right,  1);
            body.Controls.SetChildIndex(center, 2);
        }

        // ══════════════════════════════════════════════════
        //  SIDEBAR ESQUERDA
        // ══════════════════════════════════════════════════
        Panel BuildLeft()
        {
            var p = new Panel { BackColor=C_CREAM, Padding=new Padding(12,14,12,12) };
            p.Paint += (s,e) => e.Graphics.DrawLine(new Pen(C_BORDER), p.Width-1, 0, p.Width-1, p.Height);

            p.Controls.Add(Pos(SLbl("USUÁRIOS DA REDE"), 0, 2));

            lstUsers = new ListBox {
                Top=22, Left=0, Width=236, Height=340,
                BorderStyle=BorderStyle.None, BackColor=C_CREAM,
                DrawMode=DrawMode.OwnerDrawFixed, ItemHeight=44,
                Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom
            };
            lstUsers.DrawItem += DrawUser;
            lstUsers.SelectedIndexChanged += (s,e) => UpdateRight();
            p.Controls.Add(lstUsers);

            p.Controls.Add(Pos(SLbl("ADICIONAR USUÁRIO"), 0, 374));

            txtNew = new TextBox {
                Top=392, Left=0, Width=196, BorderStyle=BorderStyle.FixedSingle,
                BackColor=C_WHITE, Font=new Font("Segoe UI",9.5f), PlaceholderText="Nome do usuário..."
            };
            txtNew.KeyDown += (s,e) => { if (e.KeyCode==Keys.Enter) DoAdd(); };
            p.Controls.Add(txtNew);

            var bp = FBtn("+", 200, 390, 38, 28, C_VELVET, C_WHITE);
            bp.Font = new Font("Segoe UI",12f,FontStyle.Bold);
            bp.Click += (s,e) => DoAdd();
            p.Controls.Add(bp);

            var bd = FBtn("Remover selecionado", 0, 428, 238, 28, C_CREAM, C_INK, true);
            bd.Click += (s,e) => DoRem();
            p.Controls.Add(bd);

            p.Resize += (s,e) => {
                lstUsers.Width = p.Width-24;
                txtNew.Width   = p.Width-60;
                bp.Left        = p.Width-56;
                bd.Width       = p.Width-24;
            };
            return p;
        }

        void DrawUser(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstUsers.Items.Count) return;
            string nome = lstUsers.Items[e.Index]?.ToString() ?? "";
            bool sel = (e.State & DrawItemState.Selected) != 0;
            e.Graphics.FillRectangle(new SolidBrush(sel ? Color.FromArgb(250,234,238) : C_CREAM), e.Bounds);
            if (sel) e.Graphics.DrawRectangle(new Pen(Color.FromArgb(210,150,165),1.5f), e.Bounds.X, e.Bounds.Y, e.Bounds.Width-1, e.Bounds.Height-1);

            int ci = _rede.Pessoas.TryGetValue(nome, out var px) ? px.CorIndex%8 : 0;
            var ar = new Rectangle(e.Bounds.X+8, e.Bounds.Y+9, 28, 28);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new SolidBrush(AVG_BG[ci]), ar);
            e.Graphics.DrawEllipse(new Pen(AVG_FG[ci],1.5f), ar);
            var fmt = new StringFormat { Alignment=StringAlignment.Center, LineAlignment=StringAlignment.Center };
            string ini = nome.Length>=2 ? nome[..2].ToUpper() : nome.ToUpper();
            e.Graphics.DrawString(ini, new Font("Georgia",7.5f,FontStyle.Bold), new SolidBrush(AVG_FG[ci]), ar, fmt);
            e.Graphics.SmoothingMode = SmoothingMode.Default;

            int grau  = px?.Amigos.Count ?? 0;
            int segC  = _rede.Seguidores.TryGetValue(nome, out var sg) ? sg.Count : 0;
            int segBy = _rede.Seguidores.Values.Count(s => s.Contains(nome));
            e.Graphics.DrawString(nome, new Font("Segoe UI",9.5f), new SolidBrush(C_INK), e.Bounds.X+46, e.Bounds.Y+8);
            e.Graphics.DrawString($"grau {grau}  · ↑{segC} ↓{segBy}", new Font("Courier New",7.5f), new SolidBrush(C_MIST), e.Bounds.X+46, e.Bounds.Y+26);
        }

        // ══════════════════════════════════════════════════
        //  SIDEBAR DIREITA
        // ══════════════════════════════════════════════════
        Panel BuildRight()
        {
            var p = new Panel { BackColor=C_WHITE, Padding=new Padding(12,14,12,12) };
            p.Paint += (s,e) => e.Graphics.DrawLine(new Pen(C_BORDER), 0, 0, 0, p.Height);

            int y = 2;
            p.Controls.Add(Pos(SLbl("DETALHES"), 0, y)); y+=20;
            lblSelName = new Label { Text="—", Left=0, Top=y, Width=236, Height=28, Font=new Font("Georgia",15f,FontStyle.Bold), ForeColor=C_VELVET }; p.Controls.Add(lblSelName); y+=30;
            lblSelGrau = new Label { Text="", Left=0, Top=y, Width=236, Height=16, Font=new Font("Courier New",8f), ForeColor=C_MIST }; p.Controls.Add(lblSelGrau); y+=22;

            p.Controls.Add(Pos(SLbl("AMIGOS"),     0, y)); y+=18; lstRAmigos = SList(p, ref y, 52);
            p.Controls.Add(Pos(SLbl("SEGUE"),      0, y)); y+=18; lstRSegue  = SList(p, ref y, 38);
            p.Controls.Add(Pos(SLbl("SEGUIDORES"), 0, y)); y+=18; lstRSegBy  = SList(p, ref y, 38);
            p.Controls.Add(Pos(SLbl("LISTA DE ADJACÊNCIA"), 0, y)); y+=18; lstAdj   = SList(p, ref y, 110);
            p.Controls.Add(Pos(SLbl("GRAU DOS VÉRTICES"),   0, y)); y+=18; lstGraus = SList(p, ref y, 110);

            p.Resize += (s,e) => { foreach (Control c in p.Controls) c.Width = p.Width-24; };
            return p;
        }

        // ══════════════════════════════════════════════════
        //  CENTRO
        // ══════════════════════════════════════════════════
        Panel BuildCenter()
        {
            var c = new Panel { BackColor=C_BG };

            // Barra de abas
            var tabBar = new Panel { Dock=DockStyle.Top, Height=44, BackColor=C_CREAM };
            tabBar.Paint += (s,e) => e.Graphics.DrawLine(new Pen(C_BORDER), 0, 43, tabBar.Width, 43);

            string[] names = { "◉  Grafo", "⟿  BFS — Caminho", "↗  Seguidores", "✦  Recomendações" };
            int[] widths   = { 120, 160, 140, 160 };
            _tabBtns  = new Button[names.Length];
            _tabPanels = new Panel[names.Length];
            int tx = 0;
            for (int i = 0; i < names.Length; i++) {
                int idx = i;
                var btn = new Button {
                    Text=names[i], Left=tx, Top=0, Width=widths[i], Height=44,
                    FlatStyle=FlatStyle.Flat, BackColor=C_CREAM, ForeColor=C_MIST,
                    Font=new Font("Segoe UI",9f), Cursor=Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s,e) => SelectTab(idx);
                _tabBtns[i] = btn;
                tabBar.Controls.Add(btn);
                tx += widths[i];
            }
            c.Controls.Add(tabBar);

            _tabPanels[0] = BuildPanelGrafo();
            _tabPanels[1] = BuildPanelBFS();
            _tabPanels[2] = BuildPanelSeg();
            _tabPanels[3] = BuildPanelRec();

            foreach (var pnl in _tabPanels) {
                pnl.Dock = DockStyle.Fill;
                pnl.Visible = false;
                c.Controls.Add(pnl);
            }

            SelectTab(0);
            return c;
        }

        void SelectTab(int idx)
        {
            for (int i = 0; i < _tabBtns.Length; i++) {
                bool a = i == idx;
                _tabBtns[i].BackColor = a ? C_WHITE : C_CREAM;
                _tabBtns[i].ForeColor = a ? C_VELVET : C_MIST;
                _tabBtns[i].Font      = new Font("Segoe UI", 9f, a ? FontStyle.Bold : FontStyle.Regular);
                _tabPanels[i].Visible = a;
                if (a) _tabPanels[i].BringToFront();
            }
        }

        // ══════════════════════════════════════════════════
        //  PAINEL GRAFO
        // ══════════════════════════════════════════════════
        Panel BuildPanelGrafo()
        {
            var p = new Panel { BackColor=C_BG };

            // PictureBox = canvas do grafo
            pbGraph = new PictureBox {
                Dock=DockStyle.Fill, BackColor=C_BG,
                Cursor=Cursors.Hand
            };
            pbGraph.Paint       += DrawGraph;
            pbGraph.MouseDown   += GraphMouseDown;
            pbGraph.MouseMove   += GraphMouseMove;
            pbGraph.MouseUp     += (s,e) => { _dragging = null; };
            p.Controls.Add(pbGraph);

            // Painel inferior: gerenciar amizade
            var bot = new Panel {
                Dock=DockStyle.Bottom, Height=80, BackColor=C_WHITE,
                Padding=new Padding(16,10,16,10)
            };
            bot.Paint += (s,e) => e.Graphics.DrawLine(new Pen(C_BORDER), 0, 0, bot.Width, 0);

            bot.Controls.Add(Pos(SLbl("GERENCIAR AMIZADE"), 0, 4));

            bot.Controls.Add(Pos(SLbl("DE"),   4,  26));
            cmbA1 = Cmb(30,  22, 130); bot.Controls.Add(cmbA1);
            bot.Controls.Add(Pos(SLbl("PARA"), 170, 26));
            cmbA2 = Cmb(196, 22, 130); bot.Controls.Add(cmbA2);
            bot.Controls.Add(Pos(SLbl("NÍVEL"), 336, 26));

            trkN = new TrackBar { Left=376, Top=16, Width=100, Height=36, Minimum=1, Maximum=10, Value=7, TickFrequency=1 };
            bot.Controls.Add(trkN);
            lblNV = new Label { Left=480, Top=24, Width=24, Height=20, Text="7", Font=new Font("Georgia",12f,FontStyle.Bold), ForeColor=C_VELVET };
            trkN.ValueChanged += (s,e) => lblNV.Text = trkN.Value.ToString();
            bot.Controls.Add(lblNV);

            var bA = FBtn("Adicionar", 514, 22, 100, 30, C_VELVET, C_WHITE);
            bA.Click += (s,e) => {
                var r = _rede.AdicionarAmizade(cmbA1.Text, cmbA2.Text, trkN.Value);
                lblAM.ForeColor = r.ok ? C_VELVET : Color.Crimson;
                lblAM.Text = r.msg;
            };
            bot.Controls.Add(bA);

            var bR = FBtn("Remover", 622, 22, 90, 30, C_CREAM, C_INK, true);
            bR.Click += (s,e) => {
                var r = _rede.RemoverAmizade(cmbA1.Text, cmbA2.Text);
                lblAM.ForeColor = r.ok ? C_VELVET : Color.Crimson;
                lblAM.Text = r.msg;
            };
            bot.Controls.Add(bR);

            lblAM = new Label { Left=720, Top=26, AutoSize=true, Font=new Font("Courier New",8f), ForeColor=C_MIST };
            bot.Controls.Add(lblAM);

            p.Controls.Add(bot);
            return p;
        }

        // ── DESENHO DO GRAFO ──────────────────────────────
        void DrawGraph(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode    = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var names  = _rede.Pessoas.Keys.ToList();
            if (names.Count == 0) return;

            InitPos(pbGraph.Width, pbGraph.Height);

            // Arestas
            var drawn = new HashSet<string>();
            foreach (var (p, pp) in _rede.Pessoas) {
                foreach (var (a, lvl) in pp.Amigos) {
                    string key = string.Join("|", new[]{p,a}.OrderBy(x=>x));
                    if (!drawn.Add(key)) continue;
                    if (!_pos.ContainsKey(p) || !_pos.ContainsKey(a)) continue;

                    var p1 = _pos[p]; var p2 = _pos[a];
                    bool inPath = _hlPath.Count > 1 &&
                        _hlPath.Contains(p) && _hlPath.Contains(a) &&
                        Math.Abs(_hlPath.IndexOf(p) - _hlPath.IndexOf(a)) == 1;

                    using var pen = new Pen(inPath ? C_VELVET : C_BORDER, inPath ? 2.5f : 1.2f + lvl*0.12f);
                    g.DrawLine(pen, p1, p2);

                    // nível
                    if (!inPath) {
                        var mx = (p1.X+p2.X)/2; var my = (p1.Y+p2.Y)/2;
                        g.DrawString($"N{lvl}", new Font("Courier New",7.5f), new SolidBrush(C_MIST), mx-8, my-14);
                    }
                }
            }

            // Nós
            float R = 30;
            foreach (var nome in names) {
                if (!_pos.ContainsKey(nome)) continue;
                var cp = _pos[nome];
                int ci = _rede.Pessoas[nome].CorIndex % 8;
                bool inPath = _hlPath.Contains(nome);
                bool isSel  = lstUsers.SelectedItem?.ToString() == nome;

                var rect = new RectangleF(cp.X-R, cp.Y-R, R*2, R*2);

                // sombra
                var shadow = new RectangleF(cp.X-R+2, cp.Y-R+3, R*2, R*2);
                using (var sb = new SolidBrush(Color.FromArgb(18, 0, 0, 0))) g.FillEllipse(sb, shadow);

                // círculo
                using (var fb = new SolidBrush(inPath ? Color.FromArgb(250,234,238) : AVG_BG[ci]))
                    g.FillEllipse(fb, rect);

                using (var bp = new Pen(inPath ? C_VELVET : (isSel ? C_VELVET : C_BORDER), inPath||isSel ? 2.5f : 1.2f))
                    g.DrawEllipse(bp, rect);

                // nome
                var fmt = new StringFormat { Alignment=StringAlignment.Center, LineAlignment=StringAlignment.Center };
                using (var tb = new SolidBrush(inPath ? C_VELVET : AVG_FG[ci]))
                    g.DrawString(nome, new Font("Segoe UI", 9f, FontStyle.Bold), tb, rect, fmt);
            }

            // Legenda
            var lg = new Rectangle(pbGraph.Width-170, pbGraph.Height-70, 160, 56);
            g.FillRectangle(new SolidBrush(Color.FromArgb(220, 245, 240, 232)), lg);
            g.DrawRectangle(new Pen(C_BORDER), lg);
            g.DrawLine(new Pen(C_BORDER, 1.5f), lg.X+10, lg.Y+20, lg.X+40, lg.Y+20);
            g.DrawString("Amizade + Nível", new Font("Courier New",7.5f), new SolidBrush(C_MIST), lg.X+46, lg.Y+12);
            g.DrawLine(new Pen(C_VELVET, 2.5f), lg.X+10, lg.Y+40, lg.X+40, lg.Y+40);
            g.DrawString("Caminho BFS",     new Font("Courier New",7.5f), new SolidBrush(C_MIST), lg.X+46, lg.Y+32);
        }

        void InitPos(int w, int h)
        {
            var names = _rede.Pessoas.Keys.ToList();
            float cx = w/2f, cy = h/2f;
            float r  = Math.Min(cx, cy) * 0.72f;
            foreach (var (nome, i) in names.Select((n,i)=>(n,i))) {
                if (!_pos.ContainsKey(nome)) {
                    double a = 2*Math.PI*i/names.Count - Math.PI/2;
                    _pos[nome] = new PointF(cx + r*(float)Math.Cos(a), cy + r*(float)Math.Sin(a));
                }
            }
            foreach (var k in _pos.Keys.ToList())
                if (!_rede.Pessoas.ContainsKey(k)) _pos.Remove(k);
        }

        void GraphMouseDown(object? sender, MouseEventArgs e)
        {
            foreach (var (nome, pt) in _pos) {
                if (Math.Sqrt(Math.Pow(e.X-pt.X,2)+Math.Pow(e.Y-pt.Y,2)) < 32) {
                    _dragging  = nome;
                    _dragOffset = new PointF(pt.X-e.X, pt.Y-e.Y);
                    lstUsers.SelectedItem = nome;
                    break;
                }
            }
        }

        void GraphMouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragging == null) return;
            _pos[_dragging] = new PointF(e.X+_dragOffset.X, e.Y+_dragOffset.Y);
            pbGraph.Invalidate();
        }

        // ══════════════════════════════════════════════════
        //  PAINEL BFS
        // ══════════════════════════════════════════════════
        Panel BuildPanelBFS()
        {
            var p = new Panel { BackColor=C_WHITE, Padding=new Padding(22,18,22,18) };
            int y = 18;
            p.Controls.Add(Pos(SLbl("BUSCA EM LARGURA — CAMINHO MAIS CURTO   ·   O(V + E)"), 0, y)); y+=22;

            p.Controls.Add(Pos(SLbl("ORIGEM"),  0, y+3));
            cmbBO = Cmb(58, y, 150); p.Controls.Add(cmbBO);
            p.Controls.Add(Pos(SLbl("DESTINO"), 218, y+3));
            cmbBD = Cmb(274, y, 150); p.Controls.Add(cmbBD);

            var bb = FBtn("Encontrar caminho", 436, y+2, 160, 30, C_VELVET, C_WHITE);
            bb.Click += (s,e) => DoBFS();
            p.Controls.Add(bb); y+=46;

            lblBM = new Label { Left=0, Top=y, AutoSize=true, Font=new Font("Courier New",8.5f), ForeColor=C_MIST };
            p.Controls.Add(lblBM); y+=24;

            p.Controls.Add(Pos(SLbl("CAMINHO ENCONTRADO"), 0, y)); y+=22;

            flBFS = new FlowLayoutPanel {
                Top=y, Left=0, Width=800, Height=60,
                BackColor=Color.Transparent, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false, Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right
            };
            p.Controls.Add(flBFS);
            return p;
        }

        // ══════════════════════════════════════════════════
        //  PAINEL SEGUIDORES
        // ══════════════════════════════════════════════════
        Panel BuildPanelSeg()
        {
            var p = new Panel { BackColor=C_WHITE, Padding=new Padding(22,18,22,18) };
            int y = 18;
            p.Controls.Add(Pos(SLbl("GRAFO DIRECIONADO — SEGUIR / DEIXAR DE SEGUIR"), 0, y)); y+=22;

            p.Controls.Add(Pos(SLbl("QUEM"),  0,   y+3));
            cmbSQ = Cmb(44,  y, 140); p.Controls.Add(cmbSQ);
            p.Controls.Add(Pos(SLbl("SEGUE"), 194, y+3));
            cmbSA = Cmb(234, y, 140); p.Controls.Add(cmbSA);

            var bS = FBtn("Seguir", 386, y+2, 90, 30, C_VELVET, C_WHITE);
            bS.Click += (s,e) => {
                var r = _rede.Seguir(cmbSQ.Text, cmbSA.Text);
                lblSM.ForeColor = r.ok ? C_VELVET : Color.Crimson;
                lblSM.Text = r.msg; RefreshSeg();
            };
            p.Controls.Add(bS);

            var bD = FBtn("Deixar de seguir", 484, y+2, 140, 30, C_CREAM, C_INK, true);
            bD.Click += (s,e) => {
                var r = _rede.Desseguir(cmbSQ.Text, cmbSA.Text);
                lblSM.ForeColor = r.ok ? C_VELVET : Color.Crimson;
                lblSM.Text = r.msg; RefreshSeg();
            };
            p.Controls.Add(bD); y+=46;

            lblSM = new Label { Left=0, Top=y, AutoSize=true, Font=new Font("Courier New",8.5f), ForeColor=C_MIST };
            p.Controls.Add(lblSM); y+=24;

            p.Controls.Add(Pos(SLbl("RELAÇÕES DE SEGUIMENTO"), 0, y)); y+=20;
            lstST = new ListBox {
                Top=y, Left=0, Width=700, Height=320,
                Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom,
                BorderStyle=BorderStyle.FixedSingle, BackColor=C_WHITE, Font=new Font("Courier New",9f)
            };
            p.Controls.Add(lstST);
            return p;
        }

        // ══════════════════════════════════════════════════
        //  PAINEL RECOMENDAÇÕES
        // ══════════════════════════════════════════════════
        Panel BuildPanelRec()
        {
            var p = new Panel { BackColor=C_WHITE, Padding=new Padding(22,18,22,18) };
            int y = 18;
            p.Controls.Add(Pos(SLbl("PESSOAS QUE VOCÊ PODE CONHECER"), 0, y)); y+=22;

            p.Controls.Add(Pos(SLbl("USUÁRIO"), 0, y+3));
            cmbRU = Cmb(66, y, 180); p.Controls.Add(cmbRU);
            var bR = FBtn("Recomendar", 256, y+2, 120, 30, C_VELVET, C_WHITE);
            bR.Click += (s,e) => DoRec();
            p.Controls.Add(bR); y+=46;

            p.Controls.Add(Pos(SLbl("SUGESTÕES"), 0, y)); y+=20;
            lstRR = new ListBox {
                Top=y, Left=0, Width=600, Height=340,
                Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom,
                BorderStyle=BorderStyle.FixedSingle, BackColor=C_WHITE,
                Font=new Font("Segoe UI",10f), DrawMode=DrawMode.OwnerDrawFixed, ItemHeight=32
            };
            lstRR.DrawItem += (s,e) => {
                if (e.Index < 0) return;
                string txt = lstRR.Items[e.Index]?.ToString() ?? "";
                e.Graphics.FillRectangle(new SolidBrush(e.Index%2==0 ? C_WHITE : C_BG), e.Bounds);
                e.Graphics.DrawString(txt, new Font("Segoe UI",9.5f), new SolidBrush(C_INK), e.Bounds.X+14, e.Bounds.Y+8);
            };
            p.Controls.Add(lstRR);
            return p;
        }

        // ══════════════════════════════════════════════════
        //  LÓGICA
        // ══════════════════════════════════════════════════
        void DoAdd()
        {
            string n = txtNew.Text.Trim();
            var (ok, msg) = _rede.AdicionarPessoa(n);
            if (ok) txtNew.Clear();
            else MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        void DoRem()
        {
            if (lstUsers.SelectedItem is not string n) return;
            if (MessageBox.Show($"Remover '{n}'?","Confirmar",MessageBoxButtons.YesNo)==DialogResult.Yes)
                _rede.RemoverPessoa(n);
        }

        void DoBFS()
        {
            flBFS.Controls.Clear();
            _hlPath.Clear();
            var path = _rede.CaminhoMaisCurto(cmbBO.Text, cmbBD.Text);
            if (path.Count == 0) {
                lblBM.ForeColor = Color.Crimson;
                lblBM.Text = $"Nenhum caminho entre '{cmbBO.Text}' e '{cmbBD.Text}'.";
                pbGraph.Invalidate();
                return;
            }
            _hlPath = path;
            lblBM.ForeColor = C_VELVET;
            lblBM.Text = $"Caminho encontrado: {path.Count-1} salto(s)   ·   O(V + E)";

            for (int i = 0; i < path.Count; i++) {
                int ci = _rede.Pessoas.TryGetValue(path[i], out var px) ? px.CorIndex%8 : 0;
                flBFS.Controls.Add(new Label {
                    Text=path[i], AutoSize=true,
                    Font=new Font("Georgia",11f,FontStyle.Bold),
                    ForeColor=AVG_FG[ci], BackColor=AVG_BG[ci],
                    Padding=new Padding(10,5,10,5), Margin=new Padding(2,4,2,4),
                    BorderStyle=BorderStyle.FixedSingle
                });
                if (i < path.Count-1)
                    flBFS.Controls.Add(new Label {
                        Text="→", AutoSize=true, Margin=new Padding(2,12,2,4),
                        Font=new Font("Segoe UI",11f), ForeColor=C_MIST
                    });
            }
            pbGraph.Invalidate();
        }

        void DoRec()
        {
            var recs = _rede.RecomendarAmigos(cmbRU.Text);
            lstRR.Items.Clear();
            if (recs.Count == 0) { lstRR.Items.Add("Sem sugestões."); return; }
            foreach (var (c, n) in recs)
                lstRR.Items.Add($"  {c}   —   {n} amigo(s) em comum");
        }

        void RefreshSeg()
        {
            lstST.Items.Clear();
            foreach (var (p, set) in _rede.Seguidores) {
                var by = _rede.ObterSeguidores(p);
                string s1 = set.Count>0 ? string.Join(", ", set) : "—";
                string s2 = by.Count>0  ? string.Join(", ", by)  : "—";
                lstST.Items.Add($"{p,-12}  segue: {s1,-22}  seguidores: {s2}");
            }
        }

        // ══════════════════════════════════════════════════
        //  REFRESH GLOBAL
        // ══════════════════════════════════════════════════
        void RefreshAll()
        {
            string? sel = lstUsers.SelectedItem?.ToString();
            lstUsers.Items.Clear();
            foreach (var n in _rede.Pessoas.Keys) lstUsers.Items.Add(n);
            if (sel != null && lstUsers.Items.Contains(sel)) lstUsers.SelectedItem = sel;
            lstUsers.Invalidate();

            foreach (var cmb in new[]{cmbA1,cmbA2,cmbBO,cmbBD,cmbSQ,cmbSA,cmbRU}) {
                if (cmb==null) continue;
                string v = cmb.Text;
                cmb.Items.Clear();
                foreach (var n in _rede.Pessoas.Keys) cmb.Items.Add(n);
                if (cmb.Items.Contains(v)) cmb.Text = v;
                else if (cmb.Items.Count>0) cmb.SelectedIndex = 0;
            }

            RefreshSeg();
            UpdateRight();
            pbGraph?.Invalidate();
        }

        void UpdateRight()
        {
            string? nome = lstUsers.SelectedItem?.ToString();
            if (nome==null || !_rede.Pessoas.TryGetValue(nome, out var px)) {
                lblSelName.Text="—"; lblSelGrau.Text="";
                lstRAmigos.Items.Clear(); lstRSegue.Items.Clear(); lstRSegBy.Items.Clear();
            } else {
                lblSelName.Text = nome;
                lblSelGrau.Text = $"grau {px.Amigos.Count}";
                lstRAmigos.Items.Clear();
                foreach (var (a,l) in px.Amigos) lstRAmigos.Items.Add($"{a}  N{l}");
                lstRSegue.Items.Clear();
                foreach (var s in _rede.ObterSeguindo(nome)) lstRSegue.Items.Add(s);
                lstRSegBy.Items.Clear();
                foreach (var s in _rede.ObterSeguidores(nome)) lstRSegBy.Items.Add(s);
            }

            lstAdj.Items.Clear();
            foreach (var (n,pp) in _rede.Pessoas) {
                string viz = pp.Amigos.Count>0 ? string.Join(", ", pp.Amigos.Select(kv=>$"{kv.Key}(N{kv.Value})")) : "∅";
                lstAdj.Items.Add($"{n} → [{viz}]");
            }

            lstGraus.Items.Clear();
            foreach (var (n,pp) in _rede.Pessoas.OrderByDescending(kv=>kv.Value.Amigos.Count))
                lstGraus.Items.Add($"{n}  —  {pp.Amigos.Count}");
        }

        // ══════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════
        Label SLbl(string t) => new Label { Text=t, AutoSize=false, Height=16, Width=700, Font=new Font("Courier New",7.5f), ForeColor=C_MIST };
        Control Pos(Control c, int x, int y) { c.Left=x; c.Top=y; return c; }
        ListBox SList(Panel p, ref int y, int h) {
            var lb = new ListBox { Top=y, Left=0, Width=236, Height=h, BorderStyle=BorderStyle.None, BackColor=C_WHITE, Font=new Font("Segoe UI",8.5f) };
            p.Controls.Add(lb); y+=h+8; return lb;
        }
        ComboBox Cmb(int x, int y, int w) => new ComboBox { Left=x, Top=y, Width=w, DropDownStyle=ComboBoxStyle.DropDownList, Font=new Font("Segoe UI",9.5f), BackColor=C_WHITE, FlatStyle=FlatStyle.Flat };
        Button FBtn(string t, int x, int y, int w, int h, Color bg, Color fg, bool brd=false) {
            var b = new Button { Text=t, Left=x, Top=y, Width=w, Height=h, FlatStyle=FlatStyle.Flat, BackColor=bg, ForeColor=fg, Font=new Font("Segoe UI",9f), Cursor=Cursors.Hand };
            b.FlatAppearance.BorderSize=brd?1:0; b.FlatAppearance.BorderColor=brd?C_BORDER:bg; return b;
        }
    }
}

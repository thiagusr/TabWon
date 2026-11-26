using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("TabWon")]
[assembly: AssemblyDescription("TabWon - Tabulador Livre de dados do SUS")]
[assembly: AssemblyCompany("Thiago Rodrigues")]
[assembly: AssemblyProduct("TabWon")]
[assembly: AssemblyCopyright("Copyright (C) 2026 Thiago Rodrigues - GNU GPL v3")]
[assembly: AssemblyTrademark("TabWon")]
[assembly: AssemblyVersion("1.0.3.0")]
[assembly: AssemblyFileVersion("1.0.3.0")]
[assembly: AssemblyInformationalVersion("1.00.3")]
[assembly: ComVisible(false)]

namespace TabWonLoader
{
    static class Programa
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TelaCarregamento(args));
        }
    }

    sealed class TelaCarregamento : Form
    {
        const string Tabulador = "TabWon_tabulador.exe";
        const string VariavelEvento = "TABWON_LOADER_EVENTO";
        const string Subtitulo = "Tabulador Código Aberto para dados SUS";
        const string Aviso = "A primeira abertura pode levar cerca de um minuto.";
        const string Iniciando = "Iniciando o TabWon...";
        const string Carregando = "Carregando as bibliotecas...";
        const string Demorando = "Ainda carregando: pela rede ou na primeira abertura demora mais.";
        const string Preparando = "Preparando a janela...";
        const string SemPermissao = "Seu usuário não tem permissão de escrita, contate o suporte técnico.";
        const int LimiteSegundos = 300;
        const float MolX = 245f / 580f, MolY = 1f / 211f, MolL = 248f / 580f, MolA = 206f / 211f;
        static readonly int[] CorteLetras = { 0, 69, 167, 300, 483, 580 };
        const int PosicaoMolecula = 3;
        const double CicloOndaMs = 2600, AtrasoLetraMs = 110, PuloMs = 420;
        const int FolgaJanelaSegundos = 20;
        static readonly Color Azul = Color.FromArgb(31, 95, 168);
        static readonly Color AzulClaro = Color.FromArgb(220, 231, 241);

        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int processId);

        readonly string[] argumentos;
        readonly Stopwatch relogio = Stopwatch.StartNew();
        readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        readonly float esc = 1f;
        Process proc;
        EventWaitHandle pronto;
        Image logo, molecula;
        Font fonteSub, fonteAviso, fonteStatus;
        long janelaVistaEm = -1;
        long ultimaConsulta = -1000;
        int fase;
        bool iniciado;

        public TelaCarregamento(string[] args)
        {
            argumentos = args;
            using (Graphics g = CreateGraphics()) esc = Math.Max(1f, g.DpiX / 96f);
            Text = "TabWon";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = true;
            TopMost = true;
            BackColor = Color.White;
            DoubleBuffered = true;
            ClientSize = new Size(S(520), S(260));
            Centralizar();
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            try
            {
                Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream("TabWon_loader.png");
                if (st != null) logo = Image.FromStream(st);
                Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("TabWon_loader_molecula.png");
                if (sm != null) molecula = Image.FromStream(sm);
            }
            catch { logo = null; molecula = null; }
            fonteSub = new Font("Segoe UI", 11f);
            fonteAviso = new Font("Segoe UI", 8.5f);
            fonteStatus = new Font("Segoe UI", 9f);
            timer.Interval = 40;
            timer.Tick += Atualizar;
        }

        int S(float v) { return (int)Math.Round(v * esc); }

        void Centralizar()
        {
            Rectangle area;
            try { area = Screen.FromPoint(Cursor.Position).WorkingArea; }
            catch { area = Screen.PrimaryScreen.WorkingArea; }
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
        }

        protected override void OnLoad(EventArgs e)
        {
            Centralizar();
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Activate();
            Update();
            BeginInvoke(new MethodInvoker(IniciarTabulador));
        }

        void IniciarTabulador()
        {
            if (iniciado) return;
            iniciado = true;
            string pasta = AppDomain.CurrentDomain.BaseDirectory;
            string alvo = Path.Combine(pasta, Tabulador);
            if (!File.Exists(alvo))
            {
                Hide();
                MessageBox.Show("Não encontrei o " + Tabulador + " nesta pasta:\n" + pasta + "\n\nInstale o TabWon novamente.",
                                "TabWon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            if (!PodeGravar(pasta) || (Directory.Exists(Path.Combine(pasta, "TabWon_configs"))
                                       && !PodeGravar(Path.Combine(pasta, "TabWon_configs"))))
            {
                Hide();
                MessageBox.Show(SemPermissao, "TabWon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            string nomeEvento = "Local\\TabWon_pronto_" + Guid.NewGuid().ToString("N");
            pronto = new EventWaitHandle(false, EventResetMode.ManualReset, nomeEvento);
            ProcessStartInfo psi = new ProcessStartInfo(alvo, JuntarArgumentos(argumentos));
            psi.UseShellExecute = false;
            psi.WorkingDirectory = pasta;
            psi.EnvironmentVariables[VariavelEvento] = nomeEvento;
            try
            {
                proc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                Hide();
                MessageBox.Show("Não foi possível iniciar o " + Tabulador + ":\n" + ex.Message, "TabWon",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            try { AllowSetForegroundWindow(proc.Id); } catch { }
            timer.Start();
        }

        static bool PodeGravar(string pasta)
        {
            string teste = Path.Combine(pasta, ".tabwon_permissao_" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (FileStream fs = new FileStream(teste, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                                                      16, FileOptions.DeleteOnClose))
                {
                    fs.WriteByte(1);
                    fs.Flush();
                }
                return true;
            }
            catch
            {
                try { if (File.Exists(teste)) File.Delete(teste); } catch { }
                return false;
            }
        }

        static string JuntarArgumentos(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string a in args)
            {
                if (sb.Length > 0) sb.Append(' ');
                if (a.Length > 0 && a.IndexOfAny(new char[] { ' ', '\t', '"' }) < 0) { sb.Append(a); continue; }
                sb.Append('"');
                int barras = 0;
                foreach (char c in a)
                {
                    if (c == '\\') { barras++; continue; }
                    if (c == '"') sb.Append('\\', barras * 2 + 1);
                    else sb.Append('\\', barras);
                    barras = 0;
                    sb.Append(c);
                }
                sb.Append('\\', barras * 2);
                sb.Append('"');
            }
            return sb.ToString();
        }

        void Atualizar(object sender, EventArgs e)
        {
            fase = (fase + S(6)) % Math.Max(1, ClientSize.Width + ClientSize.Width / 3);
            long ms = relogio.ElapsedMilliseconds;
            if (pronto != null && pronto.WaitOne(0)) { Fechar(); return; }
            if (ms - ultimaConsulta >= 500)
            {
                ultimaConsulta = ms;
                try
                {
                    if (proc.HasExited) { Fechar(); return; }
                    proc.Refresh();
                    if (janelaVistaEm < 0 && proc.MainWindowHandle != IntPtr.Zero) janelaVistaEm = ms;
                }
                catch { Fechar(); return; }
                if (janelaVistaEm >= 0 && ms - janelaVistaEm > FolgaJanelaSegundos * 1000L) { Fechar(); return; }
                if (ms > LimiteSegundos * 1000L) { Fechar(); return; }
            }
            Invalidate();
        }

        void Fechar()
        {
            timer.Stop();
            Close();
        }

        string Status()
        {
            long s = relogio.ElapsedMilliseconds / 1000;
            if (janelaVistaEm >= 0) return Preparando;
            if (s < 2) return Iniciando;
            if (s < 20) return Carregando;
            return Demorando;
        }

        float Pulo(int ordem, int alturaLogo)
        {
            double p = (relogio.ElapsedMilliseconds - ordem * AtrasoLetraMs) % CicloOndaMs;
            if (p < 0 || p > PuloMs) return 0f;
            return (float)(Math.Sin(p / PuloMs * Math.PI) * alturaLogo * 0.12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int w = ClientSize.Width, h = ClientSize.Height;
            int y = S(30);
            if (logo != null)
            {
                int lw = S(290);
                int lh = (int)Math.Round(lw * (double)logo.Height / logo.Width);
                int lx = (w - lw) / 2;
                float fx = (float)lw / logo.Width, fy = (float)lh / logo.Height;
                for (int i = 0; i + 1 < CorteLetras.Length; i++)
                {
                    int ordem = i < PosicaoMolecula ? i : i + 1;
                    int sx = CorteLetras[i], sl = CorteLetras[i + 1] - sx;
                    RectangleF destino = new RectangleF(lx + sx * fx, y - Pulo(ordem, lh), sl * fx, lh);
                    g.DrawImage(logo, destino, new RectangleF(sx, 0, sl, logo.Height), GraphicsUnit.Pixel);
                }
                if (molecula != null)
                {
                    float mx = lx + lw * MolX, my = y + lh * MolY - Pulo(PosicaoMolecula, lh), mw = lw * MolL, mh = lh * MolA;
                    double t = relogio.ElapsedMilliseconds / 1400.0 * 2 * Math.PI;
                    float escala = 1f + 0.05f * (float)(0.5 - 0.5 * Math.Cos(t));
                    float angulo = 4f * (float)Math.Sin(t / 2);
                    GraphicsState estado = g.Save();
                    g.TranslateTransform(mx + mw / 2, my + mh / 2);
                    g.RotateTransform(angulo);
                    g.ScaleTransform(escala, escala);
                    g.DrawImage(molecula, new RectangleF(-mw / 2, -mh / 2, mw, mh));
                    g.Restore(estado);
                }
                y += lh + S(12);
            }
            else
            {
                using (Font f = new Font("Segoe UI", 28f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Azul))
                {
                    SizeF sz = g.MeasureString("TabWon", f);
                    g.DrawString("TabWon", f, b, (w - sz.Width) / 2, y);
                    y += (int)sz.Height + S(8);
                }
            }
            using (SolidBrush cinza = new SolidBrush(Color.FromArgb(90, 90, 90)))
            using (SolidBrush cinzaClaro = new SolidBrush(Color.FromArgb(130, 130, 130)))
            using (SolidBrush azul = new SolidBrush(Azul))
            using (Pen linha = new Pen(Color.FromArgb(225, 225, 225)))
            using (Pen borda = new Pen(Color.FromArgb(200, 200, 200)))
            {
                SizeF s1 = g.MeasureString(Subtitulo, fonteSub);
                g.DrawString(Subtitulo, fonteSub, cinza, (w - s1.Width) / 2, y);
                y += (int)s1.Height + S(6);
                SizeF s2 = g.MeasureString(Aviso, fonteAviso);
                g.DrawString(Aviso, fonteAviso, cinzaClaro, (w - s2.Width) / 2, y);
                int yLinha = h - S(46);
                g.DrawLine(linha, S(20), yLinha, w - S(20), yLinha);
                string tempo = (relogio.ElapsedMilliseconds / 1000) + " s";
                SizeF st = g.MeasureString(tempo, fonteStatus);
                g.DrawString(tempo, fonteStatus, cinzaClaro, w - S(20) - st.Width, yLinha + S(9));
                RectangleF area = new RectangleF(S(20), yLinha + S(9), w - S(40) - st.Width - S(8), st.Height + S(2));
                using (StringFormat sf = new StringFormat(StringFormatFlags.NoWrap))
                {
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    g.DrawString(Status(), fonteStatus, azul, area, sf);
                }
                int alt = S(5);
                using (SolidBrush trilho = new SolidBrush(AzulClaro)) g.FillRectangle(trilho, 0, h - alt, w, alt);
                int seg = w / 3;
                g.FillRectangle(azul, fase - seg, h - alt, seg, alt);
                g.SmoothingMode = SmoothingMode.None;
                g.DrawRectangle(borda, 0, 0, w - 1, h - 1);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Dispose();
            if (pronto != null) pronto.Dispose();
            if (logo != null) logo.Dispose();
            if (molecula != null) molecula.Dispose();
            fonteSub.Dispose(); fonteAviso.Dispose(); fonteStatus.Dispose();
            base.OnFormClosed(e);
        }
    }
}

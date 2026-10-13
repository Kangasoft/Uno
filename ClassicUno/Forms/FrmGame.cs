/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Helpers.UI;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed partial class FrmGame : FeltForm, IGame
    {
        #region Fields
        private Game _currentGame;
        private readonly Timer _tmrNew;

        private readonly Timer _time;
        private int _currentTime;

        private readonly UiSynchronize _sync;
        #endregion

        #region Constructor
        public FrmGame()
        {
            InitializeComponent();

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            Text = $@"Classic UNO! 2026 - v{version.Major}.{version.Minor}";
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);

            foreach (var btn in tblButtons.Controls.Cast<Button>().Where(btn => btn != null))
            {
                btn.Click += OnButtonClick;
            }

            _tmrNew = new Timer {Interval = 10, Enabled = true};
            _tmrNew.Tick += ShowNewGameDialog;

            _time = new Timer {Interval = 1000, Enabled = false};
            _time.Tick += TimerCount;

            _sync = new UiSynchronize(this);

            BuildMenuGame(mnuGame);
            BuildMenuOptions(mnuOptions);

            mnuOptions.DropDownOpening += OnMenuOptionsOpening;

            /* Help menu */
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[]
            {
                MenuHelper.AddMenuItem("About", "ABOUT", Keys.None, true, OnMenuClick)
            });
        }
        #endregion

        #region Form overrides
        protected override void OnLoad(EventArgs e)
        {
            /* Set window position and size */
            var loc = SettingsManager.Settings.Location;
            if (loc == Point.Empty)
            {
                /* Scale form to less than the screen width/height */
                var screen = Utils.GetCurrentMonitor(this);
                var x = screen.Bounds.Width - 100;
                var y = screen.Bounds.Height - 100;
                Size = new Size(x, y);
                /* Set location to center screen */
                Location = new Point((screen.Bounds.Width / 2) - (Size.Width / 2), (screen.Bounds.Height / 2) - (Size.Height / 2));
            }
            else
            {
                Location = loc;
                Size = SettingsManager.Settings.Size;
                if (SettingsManager.Settings.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }
            OnResize(e);
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GraphicsManager.Rescale(ClientSize);
            /* Move button table layout */
            var x = ClientRectangle.Width / 2 - (tblButtons.Width - (!btnRed.Visible ? 260 : 0)) / 2;

            tblButtons.Bounds = new Rectangle(x, ClientRectangle.Height - statusBar.Height - tblButtons.Height,
                tblButtons.Width, tblButtons.Height);

            base.OnResize(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_currentGame?.GameRunning == true)
            {
                if (MessageBox.Show(this, @"Are you sure you want to resign this current game?", @"Classic UNO! 2026",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (WindowState == FormWindowState.Normal)
            {
                SettingsManager.Settings.Location = Location;
                SettingsManager.Settings.Size = Size;
            }
            SettingsManager.Settings.Maximized = WindowState == FormWindowState.Maximized;
            SettingsManager.Save();
            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsManager.DrawGame(_currentGame, e.Graphics, ClientRectangle);
            base.OnPaint(e);
        }
        #endregion

        #region Menus
        private void BuildMenuGame(ToolStripDropDownItem m)
        {
            m.DropDownItems.Clear();

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    MenuHelper.AddMenuItem("New game", "NEW", Keys.Control | Keys.N, true,  OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Statistics...", "STATS", Keys.Alt | Keys.S, true, OnMenuClick),
                    new ToolStripSeparator(), 
                    MenuHelper.AddMenuItem("Exit", "EXIT", Keys.Alt | Keys.F4, true,  OnMenuClick)
                });
        }

        private void OnMenuOptionsOpening(object sender, EventArgs e)
        {
            BuildMenuOptions(mnuOptions);
        }

        private void BuildMenuOptions(ToolStripDropDownItem m)
        {
            m.DropDownItems.Clear();

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    MenuHelper.AddMenuItem("Enable voice", "VOICE", Keys.Alt | Keys.V, true,
                        SettingsManager.Settings.Options.Sounds.EnableVoice, null, OnMenuClick),
                    MenuHelper.AddMenuItem("Enable card sound effects", "SFX", Keys.Alt | Keys.E, true,
                        SettingsManager.Settings.Options.Sounds.EnableEffects, null, OnMenuClick),
                    MenuHelper.AddMenuItem("Enable music", "MUSIC", Keys.Alt | Keys.M, true,
                        SettingsManager.Settings.Options.Sounds.EnableMusic, null, OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Options", "OPTIONS", Keys.Alt | Keys.O, true, OnMenuClick),
                });
        }

        private void OnMenuClick(object sender, EventArgs e)
        {
            var o = (ToolStripItem)sender;
            switch (o.Tag.ToString())
            {
                case "NEW":
                    NewGame();
                    break;

                case "STATS":
                    using (var stats = new FrmStats())
                    {
                        stats.ShowDialog(this);
                    }
                    break;

                case "EXIT":
                    Close();
                    break;

                case "VOICE":
                    SettingsManager.Settings.Options.Sounds.EnableVoice =
                        !SettingsManager.Settings.Options.Sounds.EnableVoice;
                    break;

                case "SFX":
                    SettingsManager.Settings.Options.Sounds.EnableEffects =
                        !SettingsManager.Settings.Options.Sounds.EnableEffects;
                    break;

                case "MUSIC":
                    var music = !SettingsManager.Settings.Options.Sounds.EnableMusic;
                    SettingsManager.Settings.Options.Sounds.EnableMusic = music;
                    switch (music)
                    {
                        case true:
                            AudioManager.PlayMusic();
                            break;

                        default:
                            AudioManager.StopMusic();
                            break;
                    }

                    break;

                case "OPTIONS":
                    using (var options = new FrmOptions())
                    {
                        options.ShowDialog(this);
                    }
                    break;

                case "ABOUT":
                    using (var about = new FrmAbout())
                    {
                        about.ShowDialog(this);
                    }
                    break;
            }
        }
        #endregion

        #region Control buttons
        private void OnButtonClick(object sender, EventArgs e)
        {
            if (_currentGame == null)
            {
                return;
            }

            var btn = (Button)sender;
            if (btn == null)
            {
                return;
            }

            switch (btn.Tag)
            {
                case "DRAW":
                    _currentGame.HumanDrawCard();
                    break;

                case "PASS":
                    _currentGame.HumanPass();
                    break;

                case "PLAY":
                    _currentGame.HumanPlaysCard();
                    break;

                case "RED":
                    _currentGame.PlayerChoosesColor(_currentGame.CurrentPlayer, CardColor.Red);
                    break;

                case "BLUE":
                    _currentGame.PlayerChoosesColor(_currentGame.CurrentPlayer, CardColor.Blue);
                    break;

                case "GREEN":
                    _currentGame.PlayerChoosesColor(_currentGame.CurrentPlayer, CardColor.Green);
                    break;

                case "YELLOW":
                    _currentGame.PlayerChoosesColor(_currentGame.CurrentPlayer, CardColor.Yellow);
                    break;
            }
        }
        #endregion

        #region Mouse

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_currentGame == null)
            {
                base.OnMouseMove(e);
                return;
            }

            if (_currentGame.CurrentPlayer is HumanPlayer)
            {
                /* Your turn */
                var c = HitTest(_currentGame.Players[0].Cards, e.Location);
                _currentGame.HighlightCard = c;
                Invalidate();
            }
            btnPlay.Enabled = _currentGame.HighlightCard != null;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //if (_currentGame == null)
            //{
            //    base.OnMouseMove(e);
            //    return;
            //}

            //if (_currentGame.CurrentPlayer is HumanPlayer)
            //{
            //    /* Your turn */
            //    var c = HitTest(_currentGame.Players[0].Cards, e.Location);
            //    _currentGame.HighlightCard = c;
            //    Invalidate();
            //}

            base.OnMouseMove(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (_currentGame.CurrentPlayer == null) 
            {
                /* Do nothing */
                return;
            }

            if (!(_currentGame.CurrentPlayer is HumanPlayer))
            {
                /* Do nothing as it's not your turn */
                return;
            }

            if (_currentGame.HighlightCard != null && e.Button == MouseButtons.Left)
            {
                _currentGame.HumanPlaysCard();
            }

            base.OnMouseDoubleClick(e);
        }
        #endregion

        #region Timer callbacks
        private void ShowNewGameDialog(object sender, EventArgs e)
        {
            /* This allows the form to initialize and load properly before displaying the dialog */
            _tmrNew.Enabled = false;
            _tmrNew.Tick -= ShowNewGameDialog;
            _tmrNew.Dispose();

            NewGame();
        }

        private void TimerCount(object sender, EventArgs e)
        {
            if (_currentGame == null)
            {
                return;
            }
            _currentTime++;
            slTime.Text = $@"Time: {Utils.FormatTime(_currentTime)}";
        }
        #endregion

        #region Private methods
        private void NewGame()
        {
            if (_currentGame?.GameRunning == true)
            {
                if (MessageBox.Show(this, @"Are you sure you want to resign this current game?", @"Classic UNO! 2026",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    return;
                }
            }

            if (_currentGame != null)
            {
                _currentGame.Dispose();
                _currentGame = null;
                Invalidate();
            }

            /* Reset default button states */
            btnDraw.Enabled = false;
            btnPass.Enabled = false;
            ShowButtons(false);
            ShowColorButtons(false);

            slTime.Text = @"Time: 0s";

            var data = SettingsManager.Settings.Options;
            var form = new FrmNew
            {
                NameData = data.NameData,
                NumberOfPlayers = data.NumberOfPlayers
            };
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                slGame.Text = @"No game running. Select the Game menu and New Game.";
                slTime.Visible = false;
                return;
            }
            data.NameData = form.NameData;
            data.NumberOfPlayers = form.NumberOfPlayers;

            _currentGame = new Game(this, data);
            AudioManager.PlayEffect(SoundType.Shuffle);

            _currentTime = 0;
            _time.Enabled = true;
            slTime.Visible = true;

            Invalidate();
        }

        private static Card HitTest(IEnumerable<Card> cards, Point location)
        {
            return cards.FirstOrDefault(c =>
                location.X >= c.Region.X && location.X <= c.Region.X + c.Region.Width && location.Y >= c.Region.Y &&
                location.Y <= c.Region.Y + c.Region.Height);
        }

        public void UpdateStatus(string text)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => UpdateStatus(text));
                return;
            }
            slGame.Text = text;
        }

        public void GameOver(IPlayer player, int score)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => GameOver(player, score));
                return;
            }

            _time.Enabled = false;
            /* Record statistics */
            SettingsManager.Settings.Statistics.GamesPlayed++;
            if (player is HumanPlayer)
            {
                SettingsManager.Settings.Statistics.GamesWon++;
                SettingsManager.Settings.Statistics.HighScore += score;
            }
            else
            {
                SettingsManager.Settings.Statistics.GamesLost++;
            }

            SettingsManager.Settings.Statistics.ShortestGame =
                SettingsManager.Settings.Statistics.ShortestGame == 0
                    ? _currentTime
                    : Math.Min(SettingsManager.Settings.Statistics.ShortestGame, _currentTime);

            SettingsManager.Settings.Statistics.LongestGame =
                Math.Max(SettingsManager.Settings.Statistics.LongestGame, _currentTime);

            SettingsManager.Settings.Statistics.TotalGameTime += _currentTime;

            var msg = player is HumanPlayer
                ? $"You have won and earned {score} points!"
                : $"{player} has won!";

            if (MessageBox.Show(this, $@"{msg} Play again?", @"Classic UNO! 2026", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                NewGame();
                return;
            }

            _currentGame.Dispose();
            _currentGame = null;

            slGame.Text = @"No game running. Select the Game menu and New Game.";
            slTime.Visible = false;

            Invalidate();
        }
        #endregion

        #region IGame
        public void SoundEffectRequest(SoundType type)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => SoundEffectRequest(type));
                return;
            }
            AudioManager.PlayEffect(type);
        }

        public void VoiceRequest(VoiceType type, IPlayer player)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => VoiceRequest(type, player));
                return;
            }

            AudioManager.PlayVoice(type, player);
        }

        public void ShowButtons(bool visible)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => ShowButtons(visible));
                return;
            }

            tblButtons.Visible = visible;
        }

        public void EnableDraw(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnableDraw(enabled));
                return;
            }

            btnDraw.Enabled = enabled;
        }

        public void EnablePass(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePass(enabled));
                return;
            }

            btnPass.Enabled = enabled;
        }

        public void EnablePlay(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePlay(enabled));
                return;
            }

            btnPlay.Enabled = enabled;
        }

        public void ShowColorButtons(bool visible)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => ShowColorButtons(visible));
                return;
            }

            btnRed.Visible = visible;
            btnBlue.Visible = visible;
            btnGreen.Visible = visible;
            btnYellow.Visible = visible;

            OnResize(new EventArgs());
        }
        #endregion
    }
}

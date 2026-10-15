/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Helpers.UI;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Serialization;
using ClassicUno.Controls;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    public sealed partial class FrmGame : FeltForm, IGame
    {
        #region Fields
        private Game _currentGame;
        private readonly Timer _tmrNew;

        private readonly Timer _time;

        private readonly UiSynchronize _sync;

        private readonly string _recoveryFile = Utils.MainDir(string.Format(Resources.userPath, "recovery.dat"), true);
        private string _saveFile = Utils.MainDir(string.Format(Resources.userPath, "saved.dat"), true); /* TODO */
        #endregion

        #region Constructor
        public FrmGame()
        {
            InitializeComponent();

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            Text = $@"{Resources.appTitle} - v{version.Major}.{version.Minor}";
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);

            foreach (var btn in tblButtons.Controls.Cast<Button>().Where(btn => btn != null))
            {
                btn.Click += OnButtonClick;
            }

            var renderer = new UnoToolStripRenderer();

            mnuMain.RenderMode = ToolStripRenderMode.Professional;
            mnuMain.Renderer = renderer;

            statusBar.RenderMode = ToolStripRenderMode.Professional;
            statusBar.Renderer = renderer;
            statusBar.Visible = SettingsManager.Settings.General.ShowStatus;

            _tmrNew = new Timer {Interval = 10, Enabled = true};
            _tmrNew.Tick += ShowNewGameDialog;

            _time = new Timer {Interval = 1000, Enabled = false};
            _time.Tick += TimerCount;

            _sync = new UiSynchronize(this);

            BuildMenuGame(mnuGame);
            BuildMenuOptions(mnuOptions);

            mnuGame.DropDownOpening += OnMenuGameOpening;
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
            var loc = SettingsManager.Settings.General.Window.Location;
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
                Size = SettingsManager.Settings.General.Window.Size;
                if (SettingsManager.Settings.General.Window.Maximized)
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
            if (_currentGame != null)
            {
                if (_currentGame?.GameState == GameState.Running || _currentGame?.GameState == GameState.Paused)
                {
                    if (SettingsManager.Settings.General.Recovery.Save)
                    {
                        /* Save the current game state to the recovery file */
                        var state = GameStateManager.CreateGameState(_currentGame.GameData, _currentGame.CurrentTime);
                        if (state != null)
                        {
                            BinarySerialize<GameStateData>.Save(_recoveryFile, state);
                        }
                    }

                    if (SettingsManager.Settings.General.Confirm.OnExit)
                    {
                        if (MessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                else
                {
                    /* If we're dealing a round, cancel close request */
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                /* We need to delete the recovery file */
                if (File.Exists(_recoveryFile))
                {
                    File.Delete(_recoveryFile);
                }
            }

            if (WindowState == FormWindowState.Normal)
            {
                SettingsManager.Settings.General.Window.Location = Location;
                SettingsManager.Settings.General.Window.Size = Size;
            }
            SettingsManager.Settings.General.Window.Maximized = WindowState == FormWindowState.Maximized;
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

            var restart = false;
            if (_currentGame != null)
            {
                /* Is the current game able to be restarted? */
                restart = _currentGame.Restartable && _currentGame.GameState == GameState.Running ||
                          _currentGame.GameState == GameState.Paused;
            }

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    MenuHelper.AddMenuItem("New game", "NEW", Keys.Control | Keys.N, true, OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Load game", "LOAD", Keys.Control | Keys.O, true, OnMenuClick),
                    MenuHelper.AddMenuItem("Save game", "SAVE", Keys.Control | Keys.S,
                        _currentGame?.GameState == GameState.Running || _currentGame?.GameState == GameState.Paused,
                        OnMenuClick),
                    new ToolStripSeparator(), 
                    MenuHelper.AddMenuItem("Restart game", "RESTART", Keys.Control | Keys.R, restart, OnMenuClick),
                });

            var pause = MenuHelper.AddMenuItem(_currentGame?.GameState == GameState.Paused ? "Resume game" : "Pause game",
                "PAUSE", Keys.None,
                _currentGame != null && _currentGame?.GameState != GameState.Dealing,
                OnMenuClick);
            pause.ShortcutKeyDisplayString = @"Pause";

            m.DropDownItems.AddRange(new ToolStripItem[]
            {
                pause,
                new ToolStripSeparator(),
                MenuHelper.AddMenuItem("Statistics...", "STATS", Keys.Alt | Keys.S, true, OnMenuClick),
                new ToolStripSeparator(),
                MenuHelper.AddMenuItem("Exit", "EXIT", Keys.Alt | Keys.F4, true, OnMenuClick)
            });
        }

        private void OnMenuGameOpening(object sender, EventArgs e)
        {
            BuildMenuGame(mnuGame);
        }
        
        private void BuildMenuOptions(ToolStripDropDownItem m)
        {
            m.DropDownItems.Clear();

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    MenuHelper.AddMenuItem("Enable voice", "VOICE", Keys.Alt | Keys.V, true,
                        SettingsManager.Settings.Sound.EnableVoice, null, OnMenuClick),
                    MenuHelper.AddMenuItem("Enable card sound effects", "SFX", Keys.Alt | Keys.E, true,
                        SettingsManager.Settings.Sound.EnableEffects, null, OnMenuClick),
                    MenuHelper.AddMenuItem("Enable music", "MUSIC", Keys.Alt | Keys.M, true,
                        SettingsManager.Settings.Sound.EnableMusic, null, OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Show status bar", "STATUS", Keys.None, true, SettingsManager.Settings.General.ShowStatus, null, OnMenuClick),
                    new ToolStripSeparator(), 
                    MenuHelper.AddMenuItem("Options", "OPTIONS", Keys.Alt | Keys.O, true, OnMenuClick),
                });
        }

        private void OnMenuOptionsOpening(object sender, EventArgs e)
        {
            BuildMenuOptions(mnuOptions);
        }

        private void OnMenuClick(object sender, EventArgs e)
        {
            var o = (ToolStripItem)sender;
            switch (o.Tag.ToString())
            {
                case "NEW":
                    NewGame();
                    break;

                case "RESTART":
                    if (_currentGame != null)
                    {
                        if (_currentGame.GameState == GameState.Running || _currentGame.GameState == GameState.Paused)
                        {
                            if (MessageBox.Show(this,
                                @"Are you sure you want to restart this game? All progress will be lost.",
                                Resources.appTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                if (_currentGame.GameState == GameState.Paused)
                                {
                                    AudioManager.ResumeMusic();
                                }
                                _currentGame.Restart();
                                Invalidate();
                            }
                        }
                    }
                    break;

                case "STATS":
                    using (var stats = new FrmStats())
                    {
                        stats.Statistics = SettingsManager.Settings.Statistics;
                        stats.ShowDialog(this);

                        /* Update settings if it was cleared in the dialog */
                        SettingsManager.Settings.Statistics = stats.Statistics;
                    }
                    break;

                case "EXIT":
                    Close();
                    break;

                case "VOICE":
                    SettingsManager.Settings.Sound.EnableVoice = !SettingsManager.Settings.Sound.EnableVoice;
                    break;

                case "SFX":
                    SettingsManager.Settings.Sound.EnableEffects = !SettingsManager.Settings.Sound.EnableEffects;
                    break;

                case "MUSIC":
                    var music = !SettingsManager.Settings.Sound.EnableMusic;
                    SettingsManager.Settings.Sound.EnableMusic = music;
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

                case "STATUS":
                    var status = !SettingsManager.Settings.General.ShowStatus;
                    statusBar.Visible = status;
                    SettingsManager.Settings.General.ShowStatus = status;
                    break;

                case "OPTIONS":
                    using (var options = new FrmOptions())
                    {
                        options.ShowDialog(this);
                        statusBar.Visible = SettingsManager.Settings.General.ShowStatus;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            /* Handle the "Pause/Break" key */
            if (keyData == Keys.Pause)
            {
                if (_currentGame != null)
                {
                    switch (_currentGame.GameState)
                    {
                        case GameState.Running:
                            _currentGame.GameState = GameState.Paused;
                            AudioManager.PauseMusic();
                            break;

                        case GameState.Paused:
                            _currentGame.GameState = GameState.Running;
                            AudioManager.ResumeMusic();
                            break;
                    }
                    BuildMenuGame(mnuGame);
                    Invalidate();
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
                    _currentGame.PlayerChoosesColor(_currentGame.GameData.CurrentPlayer, CardColor.Red);
                    break;

                case "BLUE":
                    _currentGame.PlayerChoosesColor(_currentGame.GameData.CurrentPlayer, CardColor.Blue);
                    break;

                case "GREEN":
                    _currentGame.PlayerChoosesColor(_currentGame.GameData.CurrentPlayer, CardColor.Green);
                    break;

                case "YELLOW":
                    _currentGame.PlayerChoosesColor(_currentGame.GameData.CurrentPlayer, CardColor.Yellow);
                    break;
            }
        }
        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_currentGame?.GameState == GameState.Paused)
            {
                base.OnMouseMove(e);
                return;
            }

            if (!(_currentGame?.GameData.CurrentPlayer is HumanPlayer))
            {
                base.OnMouseMove(e);
                return;
            }

            if (_currentGame != null)
            {
                var c = HitTest(_currentGame.GameData.Players[0].Cards, e.Location);
                _currentGame.HighlightedCard = c;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_currentGame?.GameState == GameState.Paused)
            {
                base.OnMouseDown(e);
                return;
            }

            if (_currentGame?.GameData.CurrentPlayer is HumanPlayer && e.Button == MouseButtons.Left)
            {
                /* Your turn */
                var c = HitTest(_currentGame.GameData.Players[0].Cards, e.Location);
                _currentGame.SelectedCard = c;
                Invalidate();
            }
            btnPlay.Enabled = _currentGame?.SelectedCard != null;

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (_currentGame?.GameState == GameState.Paused)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            if (!(_currentGame?.GameData.CurrentPlayer is HumanPlayer))
            {
                /* Do nothing as it's not your turn */
                base.OnMouseDoubleClick(e);
                return;
            }

            if (_currentGame?.SelectedCard != null && e.Button == MouseButtons.Left)
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

            if (File.Exists(_recoveryFile) && SettingsManager.Settings.General.Recovery.Load)
            {
                /* Recover game state that was saved from on-exit. */
                var state = new GameStateData();
                if (BinarySerialize<GameStateData>.Load(_recoveryFile, ref state))
                {
                    if (state != null)
                    {
                        _currentGame = new Game(this, state);
                        if (_currentGame != null)
                        {
                            _time.Enabled = true;
                            slTime.Visible = true;
                            _currentGame.CurrentTime = state.CurrentTime;
                            _currentGame.Start();
                            UpdateStatus(string.Format(Resources.gameLoad, "Game reloaded from recovery file"));
                            return;
                        }
                    }
                }
            }

            /* Else, we start a new game. */
            NewGame();
        }

        private void TimerCount(object sender, EventArgs e)
        {
            if (_currentGame?.GameState != GameState.Running)
            {
                return;
            }

            /* Update the game time */
            _currentGame.CurrentTime++;
            slTime.Text = $@"Time: {Utils.FormatTime(_currentGame.CurrentTime)}";
        }
        #endregion

        #region Private methods
        private void NewGame()
        {
            if (SettingsManager.Settings.General.Confirm.OnNew)
            {
                if (_currentGame?.GameState == GameState.Running || _currentGame?.GameState == GameState.Paused)
                {
                    if (MessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        return;
                    }
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

            var data = SettingsManager.Settings.Game;
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

        public void GameStateChanged()
        {
            if (InvokeRequired)
            {
                _sync.Execute(GameStateChanged);
                return;
            }

            switch (_currentGame?.GameState)
            {
                case GameState.Running:
                    _time.Enabled = true;
                    break;

                case GameState.Stopped:
                case GameState.Paused:
                    _time.Enabled = false;
                    break;
            }
            BuildMenuGame(mnuGame);
        }

        public void GameOver(IPlayer player, int score)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => GameOver(player, score));
                return;
            }

            /* Record statistics */
            SettingsManager.Settings.Statistics.GamesPlayed++;
            if (player is HumanPlayer)
            {
                SettingsManager.Settings.Statistics.GamesWon++;
                SettingsManager.Settings.Statistics.HighScore += score;

                /* Streaks */
                SettingsManager.Settings.Statistics.CurrentStreak++;
                var streak = SettingsManager.Settings.Statistics.CurrentStreak;
                if (streak > SettingsManager.Settings.Statistics.LongestStreak)
                {
                    SettingsManager.Settings.Statistics.LongestStreak = streak;
                }
            }
            else
            {
                SettingsManager.Settings.Statistics.GamesLost++;
                SettingsManager.Settings.Statistics.CurrentStreak = 0;
            }

            var time = _currentGame.CurrentTime;

            SettingsManager.Settings.Statistics.ShortestGame =
                SettingsManager.Settings.Statistics.ShortestGame == 0
                    ? time
                    : Math.Min(SettingsManager.Settings.Statistics.ShortestGame, time);

            SettingsManager.Settings.Statistics.LongestGame =
                Math.Max(SettingsManager.Settings.Statistics.LongestGame, time);

            SettingsManager.Settings.Statistics.TotalGameTime += time;

            var msg = player is HumanPlayer
                ? $"You have won and earned {score} points!"
                : $"{player} has won!";

            if (MessageBox.Show(this, $@"{msg} Play again?", Resources.appTitle, MessageBoxButtons.YesNo,
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

/* Classic Uno 2026
 * Version: 1.0.1
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
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Serialization;
using ClassicUno.Classes.UI;
using ClassicUno.Classes.UI.Controls;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    public sealed class FrmGame : GraphicsRenderer, IGame
    {
        #region Fields
        private readonly MenuStrip _mnuMain;
        private readonly ToolStripMenuItem _mnuGame;
        private readonly StatusStrip _statusBar;

        private readonly ToolStripStatusLabel _slGame;
        private readonly ToolStripStatusLabel _slTime;

        private readonly ToolStripMenuItem _mnuOptions;

        private readonly TableLayoutPanel _tblButtons;

        private readonly CustomButton _btnDraw;
        private readonly CustomButton _btnPass;
        private readonly CustomButton _btnPlay;

        private readonly CustomButton _btnRed;
        private readonly CustomButton _btnBlue;
        private readonly CustomButton _btnGreen;
        private readonly CustomButton _btnYellow;

        private readonly Timer _tmrNew;

        private readonly Timer _time;

        private readonly UiSynchronize _sync;

        private readonly string _recoveryFile = Utils.MainDir(string.Format(Resources.userPath, "recovery.dat"), true);
        private readonly string _saveFile = Utils.MainDir(string.Format(Resources.userPath, "saved.dat"), true);

        private readonly List<string> _logText = new List<string>();
        #endregion

        #region Constructor
        public FrmGame()
        {
            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            Text = $@"{Resources.appTitle} - v{version.Major}.{version.Minor}.{version.MinorRevision}";
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);
            ClientSize = new Size(914, 621);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = _mnuMain;
            MinimumSize = new Size(930, 660);

            /* Menubar */
            _mnuMain = new MenuStrip
            {
                Location = new Point(0, 0),
                Padding = new Padding(7, 2, 0, 2),
                Size = new Size(914, 24),
                TabIndex = 0
            };

            _mnuGame = new ToolStripMenuItem {Size = new Size(50, 20), Text = @"Game"};

            _mnuOptions = new ToolStripMenuItem {Size = new Size(61, 20), Text = @"Options"};

            var mnuHelp = new ToolStripMenuItem {Size = new Size(44, 20), Text = @"Help"};

            _mnuMain.Items.AddRange(new ToolStripItem[] {_mnuGame, _mnuOptions, mnuHelp});

            /* Statusbar */
            _statusBar = new StatusStrip
            {
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(0, 599),
                Padding = new Padding(1, 0, 16, 0),
                Size = new Size(914, 22),
                TabIndex = 1
            };

            _slGame = new ToolStripStatusLabel
            {
                Size = new Size(897, 17), Spring = true, Text = @"Ready!", TextAlign = ContentAlignment.MiddleLeft
            };

            _slTime = new ToolStripStatusLabel
            {
                AutoSize = false,
                Size = new Size(120, 17),
                Text = @"Time: 0s",
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            _statusBar.Items.AddRange(new ToolStripItem[] {_slGame, _slTime});
            
            /* Button table layout */
            _tblButtons = new TableLayoutPanel
            {
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 8,
                Location = new Point(247, 533),
                Size = new Size(472, 63),
                TabIndex = 4,
                Visible = false
            };

            _tblButtons.RowStyles.Add(new RowStyle(SizeType.Absolute, 63F));

            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            _tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));

            /* User buttons */
            _btnPlay = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnPlay,
                BackgroundImageLayout = ImageLayout.Center,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(143, 3),
                Size = new Size(61, 55),
                TabIndex = 5,
                Tag = "PLAY",
                UseVisualStyleBackColor = false
            };

            _btnDraw = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnDraw,
                BackgroundImageLayout = ImageLayout.Center,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(3, 3),
                Size = new Size(64, 55),
                TabIndex = 2,
                Tag = "DRAW",
                UseVisualStyleBackColor = false
            };

            _btnPass = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnPass,
                BackgroundImageLayout = ImageLayout.Center,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(73, 3),
                Size = new Size(61, 55),
                TabIndex = 3,
                Tag = "PASS",
                UseVisualStyleBackColor = false
            };

            _btnYellow = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnYellow,
                BackgroundImageLayout = ImageLayout.Center,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(413, 3),
                Size = new Size(54, 55),
                TabIndex = 8,
                Tag = "YELLOW",
                UseVisualStyleBackColor = false,
                Visible = false
            };

            _btnGreen = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnGreen,
                BackgroundImageLayout = ImageLayout.Center,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(353, 3),
                Size = new Size(54, 55),
                TabIndex = 7,
                Tag = "GREEN",
                UseVisualStyleBackColor = false,
                Visible = false
            };

            _btnBlue = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnBlue,
                BackgroundImageLayout = ImageLayout.Center,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(293, 3),
                Size = new Size(54, 55),
                TabIndex = 6,
                Tag = "BLUE",
                UseVisualStyleBackColor = false,
                Visible = false
            };

            _btnRed = new CustomButton
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.btnRed,
                BackgroundImageLayout = ImageLayout.Center,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(233, 3),
                Size = new Size(54, 55),
                TabIndex = 5,
                Tag = "RED",
                UseVisualStyleBackColor = false,
                Visible = false
            };

           
            _tblButtons.Controls.Add(_btnDraw, 0, 0);
            _tblButtons.Controls.Add(_btnPass, 1, 0);
            _tblButtons.Controls.Add(_btnPlay, 2, 0);
            _tblButtons.Controls.Add(_btnRed, 4, 0);
            _tblButtons.Controls.Add(_btnBlue, 5, 0);
            _tblButtons.Controls.Add(_btnGreen, 6, 0);
            _tblButtons.Controls.Add(_btnYellow, 7, 0);
            
            foreach (var btn in _tblButtons.Controls.Cast<Button>().Where(btn => btn != null))
            {
                btn.Click += OnButtonClick;
            }

            var renderer = new UnoToolStripRenderer();

            _mnuMain.RenderMode = ToolStripRenderMode.Professional;
            _mnuMain.Renderer = renderer;

            _statusBar.RenderMode = ToolStripRenderMode.Professional;
            _statusBar.Renderer = renderer;

            var status = SettingsManager.Settings.General.ShowStatus;
            _statusBar.Visible = status;

            _tmrNew = new Timer {Interval = 10, Enabled = true};
            _tmrNew.Tick += ShowNewGameDialog;

            _time = new Timer {Interval = 1000, Enabled = false};
            _time.Tick += TimerCount;

            _sync = new UiSynchronize(this);

            BuildMenuGame(_mnuGame);
            BuildMenuOptions(_mnuOptions);

            LogRenderer.Visible = SettingsManager.Settings.General.ShowLogText;
            LogRenderer.BottomOffset = status ? 20 : 0;
            LogRenderer.UpdateBounds(ClientRectangle);
            LogRenderer.ShowDropShadow = SettingsManager.Settings.General.ShowDropShadow;

            CardBackIndex = SettingsManager.Settings.General.CardBack;

            _mnuGame.DropDownOpening += OnMenuGameOpening;
            _mnuOptions.DropDownOpening += OnMenuOptionsOpening;

            /* Help menu */
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[]
            {
                MenuHelper.AddMenuItem("About", "ABOUT", Keys.None, true, OnMenuClick)
            });

            /* Add controls */
            Controls.AddRange(new Control[] {_mnuMain, _statusBar, _tblButtons});
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
            if (!Visible)
            {
                return;
            }
            /* Move button table layout */
            var x = ClientRectangle.Width / 2 - (_tblButtons.Width - (!_btnRed.Visible ? 260 : 0)) / 2;

            _tblButtons.Bounds = new Rectangle(x, ClientRectangle.Height - _statusBar.Height - _tblButtons.Height,
                _tblButtons.Width, _tblButtons.Height);

            base.OnResize(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (CurrentGame != null)
            {
                switch (CurrentGame?.GameState)
                {
                    case GameState.Running:
                    case GameState.Paused:
                        if (SettingsManager.Settings.General.Recovery.Save)
                        {
                            /* Save the current game state to the recovery file */
                            SaveGame(_recoveryFile);
                        }

                        if (SettingsManager.Settings.General.Confirm.OnExit)
                        {
                            if (MessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        break;

                    case GameState.Dealing:
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
        #endregion

        #region Menus
        private void BuildMenuGame(ToolStripDropDownItem m)
        {
            m.DropDownItems.Clear();

            var restart = false;
            if (CurrentGame != null)
            {
                /* Is the current game able to be restarted? */
                restart = CurrentGame.Restartable && CurrentGame.GameState == GameState.Running ||
                          CurrentGame.GameState == GameState.Paused;
            }

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    MenuHelper.AddMenuItem("New game", "NEW", Keys.Control | Keys.N, true, OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Load game", "LOAD", Keys.Control | Keys.O, true, OnMenuClick),
                    MenuHelper.AddMenuItem("Save game", "SAVE", Keys.Control | Keys.S,
                        CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused,
                        OnMenuClick),
                    new ToolStripSeparator(), 
                    MenuHelper.AddMenuItem("Restart game", "RESTART", Keys.Control | Keys.R, restart, OnMenuClick),
                });

            var pause = MenuHelper.AddMenuItem(CurrentGame?.GameState == GameState.Paused ? "Resume game" : "Pause game",
                "PAUSE", Keys.None,
                CurrentGame != null && CurrentGame?.GameState != GameState.Dealing,
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
            BuildMenuGame(_mnuGame);
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
                    MenuHelper.AddMenuItem("Show game log text", "LOG", Keys.None, true, SettingsManager.Settings.General.ShowLogText, null, OnMenuClick),
                    new ToolStripSeparator(),
                    MenuHelper.AddMenuItem("Card back...", "BACKS", Keys.None, true, OnMenuClick),
                    MenuHelper.AddMenuItem("Options...", "OPTIONS", Keys.Alt | Keys.O, true, OnMenuClick)
                });
        }

        private void OnMenuOptionsOpening(object sender, EventArgs e)
        {
            BuildMenuOptions(_mnuOptions);
        }

        private void OnMenuClick(object sender, EventArgs e)
        {
            var o = (ToolStripItem) sender;
            switch (o.Tag.ToString())
            {
                case "NEW":
                    NewGame();
                    break;

                case "LOAD":
                    if ((CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused))
                    {
                        if (MessageBox.Show(this, Resources.resignGame, Resources.appTitle, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    /* Restore saved game */
                    UpdateStatus(LoadGame(_saveFile)
                        ? string.Format(Resources.gameLoad, "Game loaded successfully")
                        : "No games loaded.");

                    break;

                case "SAVE":
                    if ((CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused) &&
                        SaveGame(_saveFile))
                    {
                        UpdateStatus("Game successfully saved.");
                    }
                    else
                    {
                        UpdateStatus("Nothing saved.");
                    }
                    break;

                case "RESTART":
                    if (CurrentGame != null)
                    {
                        if (CurrentGame.GameState == GameState.Running || CurrentGame.GameState == GameState.Paused)
                        {
                            if (MessageBox.Show(this,
                                @"Are you sure you want to restart this game? All progress will be lost.",
                                Resources.appTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                if (CurrentGame.GameState == GameState.Paused)
                                {
                                    AudioManager.ResumeMusic();
                                }

                                CurrentGame.Restart();
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
                    _statusBar.Visible = status;
                    SettingsManager.Settings.General.ShowStatus = status;
                    LogRenderer.BottomOffset = status ? 20 : 0;
                    LogRenderer.UpdateBounds(ClientRectangle);
                    Invalidate(LogRenderer.Bounds);
                    break;

                case "LOG":
                    var log = !SettingsManager.Settings.General.ShowLogText;
                    SettingsManager.Settings.General.ShowLogText = log;
                    LogRenderer.Visible = log;
                    LogRenderer.Update(_logText);
                    LogRenderer.UpdateBounds(ClientRectangle);
                    Invalidate(LogRenderer.Bounds);
                    break;

                case "OPTIONS":
                    using (var options = new FrmOptions())
                    {
                        options.ShowDialog(this);
                        _statusBar.Visible = SettingsManager.Settings.General.ShowStatus;

                        LogRenderer.ShowDropShadow = SettingsManager.Settings.General.ShowDropShadow;
                        LogRenderer.UpdateBounds(ClientRectangle);
                        LogRenderer.Update(SettingsManager.Settings.General.ShowLogText ? _logText : null);
                    }
                    break;

                case "BACKS":
                    using (var backs = new FrmCardBacks())
                    {
                        backs.CardImages = CardBackImages;
                        backs.SelectedIndex = SettingsManager.Settings.General.CardBack;

                        if (backs.ShowDialog(this) == DialogResult.OK)
                        {
                            if (backs.SelectedIndex > -1)
                            {
                                CardBackIndex = backs.SelectedIndex;
                                SettingsManager.Settings.General.CardBack = CardBackIndex;
                                Invalidate();
                            }
                        }
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
                if (CurrentGame != null)
                {
                    switch (CurrentGame.GameState)
                    {
                        case GameState.Running:
                            CurrentGame.GameState = GameState.Paused;
                            AudioManager.PauseMusic();
                            break;

                        case GameState.Paused:
                            CurrentGame.GameState = GameState.Running;
                            AudioManager.ResumeMusic();
                            break;
                    }
                    BuildMenuGame(_mnuGame);
                    Invalidate();
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Control buttons
        private void OnButtonClick(object sender, EventArgs e)
        {
            if (CurrentGame == null)
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
                    CurrentGame.HumanDrawCard();
                    break;

                case "PASS":
                    CurrentGame.HumanPass();
                    break;

                case "PLAY":
                    CurrentGame.HumanPlaysCard();
                    break;

                case "RED":
                    CurrentGame.PlayerChoosesColor(CurrentGame.GameData.CurrentPlayer, CardColor.Red);
                    break;

                case "BLUE":
                    CurrentGame.PlayerChoosesColor(CurrentGame.GameData.CurrentPlayer, CardColor.Blue);
                    break;

                case "GREEN":
                    CurrentGame.PlayerChoosesColor(CurrentGame.GameData.CurrentPlayer, CardColor.Green);
                    break;

                case "YELLOW":
                    CurrentGame.PlayerChoosesColor(CurrentGame.GameData.CurrentPlayer, CardColor.Yellow);
                    break;
            }
        }
        #endregion

        #region Mouse
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (CurrentGame?.GameState == GameState.Paused || CurrentGame?.GameState == GameState.Dealing || !(CurrentGame?.GameData.CurrentPlayer is HumanPlayer))
            {
                base.OnMouseMove(e);
                return;
            }

            if (CurrentGame != null)
            {
                var c = HitTest(CurrentGame.GameData.Players[0].Cards, e.Location);
                CurrentGame.HighlightedCard = c;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (CurrentGame?.GameState == GameState.Paused)
            {
                base.OnMouseDown(e);
                return;
            }

            if (CurrentGame?.GameData.CurrentPlayer is HumanPlayer && e.Button == MouseButtons.Left)
            {
                /* Your turn */
                var c = HitTest(CurrentGame.GameData.Players[0].Cards, e.Location);
                CurrentGame.SelectedCard = c;
                Invalidate();
            }
            _btnPlay.Enabled = CurrentGame?.SelectedCard != null;

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (CurrentGame?.GameState == GameState.Paused)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            if (!(CurrentGame?.GameData.CurrentPlayer is HumanPlayer))
            {
                /* Do nothing as it's not your turn */
                base.OnMouseDoubleClick(e);
                return;
            }

            if (CurrentGame?.SelectedCard != null && e.Button == MouseButtons.Left)
            {
                CurrentGame.HumanPlaysCard();
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
                if (LoadGame(_recoveryFile))
                {
                    UpdateStatus(string.Format(Resources.gameLoad, "Game reloaded from recovery file"));
                    return;
                }
            }

            /* Else, we start a new game. */
            NewGame();
        }

        private void TimerCount(object sender, EventArgs e)
        {
            if (CurrentGame?.GameState != GameState.Running)
            {
                return;
            }

            /* Update the game time */
            CurrentGame.CurrentTime++;
            _slTime.Text = $@"Time: {Utils.FormatTime(CurrentGame.CurrentTime)}";
        }
        #endregion

        #region Private methods
        private void NewGame()
        {
            if (SettingsManager.Settings.General.Confirm.OnNew)
            {
                if (CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused)
                {
                    if (MessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                }
            }

            if (CurrentGame != null)
            {
                CurrentGame.Dispose();
                CurrentGame = null;
                _logText.Clear();
                LogRenderer.Update(_logText);
                Invalidate();
            }

            /* Reset default button states */
            _btnDraw.Enabled = false;
            _btnPass.Enabled = false;
            ShowButtons(false);
            ShowColorButtons(false);

            _slTime.Text = @"Time: 0s";

            var data = SettingsManager.Settings.Game;
            var form = new FrmNew
            {
                NameData = data.NameData,
                NumberOfPlayers = data.NumberOfPlayers
            };
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                _slGame.Text = @"No game running. Select the Game menu and New Game.";
                _slTime.Visible = false;
                return;
            }
            data.NameData = form.NameData;
            data.NumberOfPlayers = form.NumberOfPlayers;

            ResetAnimation();

            CurrentGame = new Game(this, data);
            AudioManager.PlayEffect(SoundType.Shuffle);

            _time.Enabled = true;
            _slTime.Visible = true;

            _logText.Clear();
            LogRenderer.Update(null);

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
            _slGame.Text = text;
        }

        public void UpdateLogText(string text)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => UpdateLogText(text));
                return;
            }

            _logText.Add(text);
            if (LogRenderer?.Visible != true)
            {
                return;
            }

            LogRenderer.UpdateBounds(ClientRectangle);
            LogRenderer.Update(_logText);
            Invalidate(LogRenderer.Bounds);
        }

        public void GameStateChanged()
        {
            if (InvokeRequired)
            {
                _sync.Execute(GameStateChanged);
                return;
            }

            switch (CurrentGame?.GameState)
            {
                case GameState.Running:
                    _time.Enabled = true;
                    break;

                case GameState.Stopped:
                case GameState.Paused:
                    _time.Enabled = false;
                    break;
            }
            BuildMenuGame(_mnuGame);
            Invalidate();
        }

        public void GameOver(IPlayer player, int score)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => GameOver(player, score));
                return;
            }

            UpdateStatus($"Game over! { player} won!");
            
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

            var time = CurrentGame.CurrentTime;

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

            CurrentGame.Dispose();
            CurrentGame = null;

            _slGame.Text = @"No game running. Select the Game menu and New Game.";
            _slTime.Visible = false;

            ResetAnimation();

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

            _tblButtons.Visible = visible;
        }

        public void EnableDraw(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnableDraw(enabled));
                return;
            }
            _btnDraw.Enabled = enabled;
        }

        public void EnablePass(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePass(enabled));
                return;
            }

            _btnPass.Enabled = enabled;
        }

        public void EnablePlay(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePlay(enabled));
                return;
            }

            _btnPlay.Enabled = enabled;
        }

        public void ShowColorButtons(bool visible)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => ShowColorButtons(visible));
                return;
            }

            _btnRed.Visible = visible;
            _btnBlue.Visible = visible;
            _btnGreen.Visible = visible;
            _btnYellow.Visible = visible;

            OnResize(new EventArgs());
        }
        #endregion

        #region Load/save

        private bool LoadGame(string path)
        {
            var state = new GameStateData();

            if (BinarySerialize<GameStateData>.Load(path, ref state))
            {
                if (state != null)
                {
                    CurrentGame = new Game(this, state);
                    if (CurrentGame != null)
                    {
                        _time.Enabled = true;
                        _slTime.Visible = true;

                        _logText.Clear();
                        LogRenderer.Update(null);

                        CurrentGame.CurrentTime = state.CurrentTime;
                        CurrentGame.Start();
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SaveGame(string path)
        {
            if (CurrentGame == null)
            {
                return false;
            }
            var state = GameStateManager.CreateGameState(CurrentGame.GameData, CurrentGame.CurrentTime);
            if (state != null)
            {
                BinarySerialize<GameStateData>.Save(path, state);
                return true;
            }

            return false;
        }
        #endregion
    }
}

/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Serialization;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Classes.Shared;
using ClassicUno.Classes.Shared.Management;
using ClassicUno.Classes.UI;
using ClassicUno.Classes.UI.Rendering;
using ClassicUno.Controls;
using ClassicUno.Controls.UserButtons;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    public sealed class FrmGame : GraphicsRenderer, IGame
    {
        /*
         * Q: Why did the computer get cold?
         * A: It left all its Windows open.
         */
        #region Fields
        private readonly MenuStrip _mnuMain;
        private readonly ToolStripMenuItem _mnuGame;
        private readonly StatusStrip _statusBar;

        private readonly ToolStripStatusLabel _slGame;
        private readonly ToolStripStatusLabel _slTime;

        private readonly ToolStripMenuItem _mnuOptions;

        private readonly ButtonStrip _controlButtons;

        private readonly Timer _tmrNew;

        private readonly Timer _time;

        private readonly UiSynchronize _sync;

        private readonly string _recoveryFile = Misc.MainDir(string.Format(Resources.userPath, "recovery.dat"), true);

        private readonly List<string> _logText = new List<string>();
        #endregion

        #region Constructor
        public FrmGame()
        {
            /* Initalize settings */
            SettingsManager.Load();

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
            
            /* User control buttons */
            _controlButtons = new ButtonStrip
            {
                Visible = false
            };

            _controlButtons.ButtonClick += OnControlButtonClick;

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
                Menus.AddMenuItem("UNO on Github", "GITHUB", Keys.F2, true, false, Resources.world, OnMenuClick),
                new ToolStripSeparator(),
                Menus.AddMenuItem("Contents", "CONTENTS", Keys.F1, true, false, Resources.help, OnMenuClick),
                Menus.AddMenuItem("About...", "ABOUT", Keys.Shift | Keys.F1, true, false, Resources.about, OnMenuClick)
            });

            /* Add controls */
            Controls.AddRange(new Control[] { _mnuMain, _statusBar, _controlButtons });
        }
        #endregion

        #region Form overrides
        protected override void OnLoad(EventArgs e)
        {
            /* Initialize Audio manager */
            AudioManager.Initialize();

            /* Set window position and size */
            var loc = SettingsManager.Settings.General.Window.Location;
            if (loc == Point.Empty)
            {
                /* Scale form to less than the screen width/height */
                var screen = Misc.GetCurrentMonitor(this);
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
            var x = ClientRectangle.Width / 2 - _controlButtons.Width / 2;

            _controlButtons.Bounds = new Rectangle(x,
                ClientRectangle.Height - _statusBar.Height - _controlButtons.Height - 12,
                _controlButtons.Width, _controlButtons.Height);

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
                            if (CustomMessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                                CustomMessageBoxButtons.YesNo, CustomMessageBoxIcon.Question) == DialogResult.No)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        break;

                    case GameState.Dealing:
                        /* If we're dealing a round, cancel close request */
                        SystemSounds.Beep.Play();
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

            var saved = SettingsManager.Settings.Saved.Games.Count;
            var restart = false;
            var load = saved > 0;
            var undo = false;
            if (CurrentGame != null)
            {
                /* Is the current game able to be restarted? */
                restart = CurrentGame.Restartable && CurrentGame.GameState == GameState.Running ||
                          CurrentGame.GameState == GameState.Paused;

                load = CurrentGame.GameState != GameState.Dealing && saved > 0;

                undo = CurrentGame.Undo.CanUndo && CurrentGame.GameState == GameState.Running &&
                       CurrentGame.GameData.CurrentPlayer is HumanPlayer;
            }

            m.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    Menus.AddMenuItem("New game", "NEW", Keys.Control | Keys.N, true, false, Resources.newGame,
                        OnMenuClick),
                    new ToolStripSeparator(),
                    Menus.AddMenuItem("Load game", "LOAD", Keys.Control | Keys.O, load, false, Resources.load,
                        OnMenuClick),
                    Menus.AddMenuItem("Save game", "SAVE", Keys.Control | Keys.S,
                        CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused,
                        false, Resources.save,
                        OnMenuClick),
                    new ToolStripSeparator(),
                    Menus.AddMenuItem("Undo last move", "UNDO", Keys.Control | Keys.Z, undo, false, Resources.undo,
                        OnMenuClick),
                    Menus.AddMenuItem("Restart game", "RESTART", Keys.Control | Keys.R, restart, false,
                        Resources.restart, OnMenuClick),
                });

            var pause = Menus.AddMenuItem(CurrentGame?.GameState == GameState.Paused ? "Resume game" : "Pause game",
                "PAUSE", Keys.None,
                CurrentGame != null && CurrentGame?.GameState != GameState.Dealing, false, Resources.pause,
                OnMenuClick);
            pause.ShortcutKeyDisplayString = @"Pause";

            m.DropDownItems.AddRange(new ToolStripItem[]
            {
                pause,
                new ToolStripSeparator(),
                Menus.AddMenuItem("Statistics...", "STATS", Keys.Alt | Keys.S, true, false, Resources.stats,
                    OnMenuClick),
                new ToolStripSeparator(),
                Menus.AddMenuItem("Exit", "EXIT", Keys.Alt | Keys.F4, true, false, Resources.close, OnMenuClick)
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
                    Menus.AddMenuItem("Enable voice", "VOICE", Keys.Alt | Keys.V, true,
                        SettingsManager.Settings.Sound.EnableVoice, Resources.sound, OnMenuClick),
                    Menus.AddMenuItem("Enable card sound effects", "SFX", Keys.Alt | Keys.E, true,
                        SettingsManager.Settings.Sound.EnableEffects, Resources.sound, OnMenuClick),
                    Menus.AddMenuItem("Enable music", "MUSIC", Keys.Alt | Keys.M, true,
                        SettingsManager.Settings.Sound.EnableMusic, Resources.music, OnMenuClick),
                    new ToolStripSeparator(),
                    Menus.AddMenuItem("Show status bar", "STATUS", Keys.None, true,
                        SettingsManager.Settings.General.ShowStatus, Resources.status, OnMenuClick),
                    Menus.AddMenuItem("Show game log text", "LOG", Keys.None, true,
                        SettingsManager.Settings.General.ShowLogText, Resources.log, OnMenuClick),
                    new ToolStripSeparator(),
                    Menus.AddMenuItem("Card back...", "BACKS", Keys.None, true, false, Resources.cardBack, OnMenuClick),
                    Menus.AddMenuItem("Options...", "OPTIONS", Keys.Alt | Keys.O, true, false, Resources.options,
                        OnMenuClick)
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
                        if (CustomMessageBox.Show(this, Resources.resignGame, Resources.appTitle, CustomMessageBoxButtons.YesNo,
                            CustomMessageBoxIcon.Question) == DialogResult.No)
                        {
                            return;
                        }
                    }

                    using (var d = new FrmSaveLoadGame(false))
                    {
                        d.ShowDialog(this);
                        if (d.DialogResult == DialogResult.OK)
                        {
                            /* Restore saved game */
                            UpdateStatus(LoadGame(d.SelectedSave.FileName)
                                ? string.Format(Resources.gameLoad, "Game loaded successfully")
                                : "No games loaded.");
                        }
                    }

                    break;

                case "SAVE":
                    if ((CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused))
                    {
                        using (var d = new FrmSaveLoadGame(true) {SaveName = Tournament.IsActive ? "My tournament" : "My UNO game"})
                        {
                            d.ShowDialog(this);
                            if (d.DialogResult == DialogResult.OK)
                            {
                                /* Save current game */
                                d.SelectedSave.Tournament = Tournament.IsActive;
                                if (SaveGame(d.SelectedSave.FileName))
                                {
                                    UpdateStatus("Game successfully saved.");
                                    CustomMessageBox.Show(this, "Current game successfully saved.", Resources.appTitle,
                                        CustomMessageBoxButtons.Ok,
                                        CustomMessageBoxIcon.Information);
                                }
                                SettingsManager.Save();
                                _mnuGame.DropDownItems[2].Enabled = SettingsManager.Settings.Saved.Games.Count > 0;
                            }
                            else
                            {
                                UpdateStatus("Nothing saved.");
                            }
                        }
                    }
                    else
                    {
                        UpdateStatus("Nothing saved.");
                    }

                    break;

                case "UNDO":
                    CurrentGame?.UndoLastMove();
                    break;

                case "RESTART":
                    if (CurrentGame != null)
                    {
                        if (CurrentGame.GameState == GameState.Running || CurrentGame.GameState == GameState.Paused)
                        {
                            if (CustomMessageBox.Show(this,
                                "Are you sure you want to restart this game?\r\n\r\nAll progress will be lost.",
                                Resources.appTitle, CustomMessageBoxButtons.YesNo, CustomMessageBoxIcon.Question) == DialogResult.Yes)
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
                        backs.CardImages = Cards.CardBackImages;
                        backs.SelectedIndex = SettingsManager.Settings.General.CardBack;

                        if (backs.ShowDialog(this) == DialogResult.OK)
                        {
                            if (backs.SelectedIndex > -1)
                            {
                                CardBackIndex = backs.SelectedIndex;
                                SettingsManager.Settings.General.CardBack = CardBackIndex;
                                InvalidateBackground();
                            }
                        }
                    }
                    break;

                case "GITHUB":
                    Misc.ShellExecute(Resources.gitHub);
                    break;

                case "CONTENTS":
                    Misc.ShellExecute(Misc.MainDir(@"\ClassicUno.chm", false));
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
                            CurrentGame.PauseGame();
                            _controlButtons.Enabled = false;
                            AudioManager.PauseMusic();
                            break;

                        case GameState.Paused:
                            CurrentGame.ResumeGame();
                            _controlButtons.Enabled = true;
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
        private void OnControlButtonClick(CustomButtonType type, CustomButtonIdentifier identifier)
        {
            if (CurrentGame == null)
            {
                return;
            }

            switch (identifier)
            {
                case CustomButtonIdentifier.Draw:
                    CurrentGame.SaveUndoState();
                    CurrentGame.GameData.CurrentPlayer.DrawCard();
                    break;

                case CustomButtonIdentifier.Pass:
                    CurrentGame.GameData.CurrentPlayer.Pass();
                    break;

                case CustomButtonIdentifier.Play:
                    PlayCard();
                    break;

                case CustomButtonIdentifier.Red:
                    CurrentGame.GameData.CurrentPlayer.ChooseColor(CardColor.Red);
                    break;

                case CustomButtonIdentifier.Blue:
                    CurrentGame.GameData.CurrentPlayer.ChooseColor(CardColor.Blue);
                    break;

                case CustomButtonIdentifier.Green:
                    CurrentGame.GameData.CurrentPlayer.ChooseColor(CardColor.Green);
                    break;

                case CustomButtonIdentifier.Yellow:
                    CurrentGame.GameData.CurrentPlayer.ChooseColor(CardColor.Yellow);
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
                var previousCard = CurrentGame.HighlightedCard;
                var currentCard = HitTest(CurrentGame.GameData.Players[0].Cards, e.Location);

                if (previousCard != currentCard)
                {
                    CurrentGame.HighlightedCard = currentCard;

                    /* Redraw only the affected card areas. */
                    InvalidateHoverCard(previousCard);
                    InvalidateHoverCard(currentCard);
                }
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
                if (CurrentGame.GameData.CurrentColor == CardColor.None)
                {
                    /* Don't select a card at all */
                    return;
                }
                var c = HitTest(CurrentGame.GameData.Players[0].Cards, e.Location);
                CurrentGame.SelectedCard = c;
                InvalidateBackground();
            }

            _controlButtons.EnableButton(CustomButtonIdentifier.Play,
                CurrentGame?.SelectedCard != null && CurrentGame?.GameData.CurrentColor != CardColor.None);
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

            if (e.Button == MouseButtons.Left)
            {
                if (CurrentGame?.SelectedCard != null)
                {
                    PlayCard();
                }
                else if (CurrentGame?.GameData.CurrentColor == CardColor.None)
                {
                    SoundEffectRequest(SoundType.Error);
                    UpdateStatus("You must select a card color to continue.");
                }
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
            _slTime.Text = $@"Time: {Misc.FormatTime(CurrentGame.CurrentTime)}";
        }
        #endregion

        #region Private methods
        private void NewGame()
        {
            if (SettingsManager.Settings.General.Confirm.OnNew)
            {
                if (CurrentGame?.GameState == GameState.Running || CurrentGame?.GameState == GameState.Paused)
                {
                    if (CustomMessageBox.Show(this, Resources.resignGame, Resources.appTitle,
                        CustomMessageBoxButtons.YesNo, CustomMessageBoxIcon.Question) == DialogResult.No)
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

            /* Reset tournament */
            if (Tournament.IsActive)
            {
                Tournament.End();
            }

            /* Reset default button states */
            _controlButtons.EnableButton(CustomButtonIdentifier.Draw, false);
            _controlButtons.EnableButton(CustomButtonIdentifier.Pass, false);

            ShowButtons(false);
            ShowColorButtons(false);

            var data = SettingsManager.Settings.Game;
            var form = new FrmNew
            {
                NameData = data.NameData,
                NumberOfPlayers = data.NumberOfPlayers,
                Difficulty = data.Difficulty,
                TournamentMode = data.TournamentMode,
                RandomPlayerStarts = data.RandomPlayerStarts
            };
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                _slGame.Text = @"No game running. Select the Game menu and New Game.";
                _slTime.Visible = false;
                return;
            }
            data.NameData = form.NameData;
            data.NumberOfPlayers = form.NumberOfPlayers;
            data.Difficulty = form.Difficulty;
            data.TournamentMode = form.TournamentMode;
            data.RandomPlayerStarts = form.RandomPlayerStarts;

            if (form.TournamentMode)
            {
                /* Create a tournament penis game ignoring number of players. The structure of tournaments is 3
                 * successive games (which the human player has to win each of) starting with 2 players round 1,
                 * 3 in round 2 and finally 4 in round 3. */
                Tournament.Start();
                StartTournamentRound();
                return;
            }

            /* Start normal game based on data.NumberOfPlayers penis. */
            StartGame(data);
        }

        private void StartTournamentRound()
        {
            var settings = SettingsManager.Settings.Game;

            var data = new GameOptionData
            {
                NameData = settings.NameData,
                NumberOfPlayers = Tournament.GetPlayerCount(),
                TournamentMode = true
            };

            Tournament.BeginNextRound();
            
            StartGame(data);
        }

        private void StartGame(GameOptionData data)
        {
            ResetAnimation();

            /* Make sure log text area is blank */
            _logText.Clear();
            LogRenderer.Update(null);

            CurrentGame = new Game(this, data);

            _time.Enabled = true;
            _slTime.Text = @"Time: 0s";
            _slTime.Visible = true;

            Invalidate();
        }

        private static Card HitTest(IEnumerable<Card> cards, Point location)
        {
            return cards.FirstOrDefault(c =>
                location.X >= c.Region.X && location.X <= c.Region.X + c.Region.Width && location.Y >= c.Region.Y &&
                location.Y <= c.Region.Y + c.Region.Height);
        }

        private void InvalidateHoverCard(Card card)
        {
            if (card == null || card == CurrentGame?.SelectedCard)
            {
                return;
            }

            var bounds = Rectangle.Ceiling(new RectangleF(
                card.Region.X,
                card.Region.Y,
                CardWidth,
                CardHeight));

            Invalidate(bounds);
        }

        public void UpdateStatus(string text)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => UpdateStatus(text));
                return;
            }

            if (string.IsNullOrEmpty(text) && Tournament.IsActive)
            {
                /* Normally this text field is blank during a normal UNO game. However, if a tournament
                 * is running we can use the "blank spots" to display the tournament round information. */
                text = $"Tournament: Round {Tournament.CurrentRound} of {Tournament.TotalRounds}";
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

        public async void GameOver(IPlayer player, int score)
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

                /* Play winning sound effect */
                if (SettingsManager.Settings.Sound.EnableEffects)
                {
                    AudioManager.PlayEffect(SoundType.Win);
                    await Task.Delay(3000);
                }
            }
            else
            {
                SettingsManager.Settings.Statistics.GamesLost++;
                SettingsManager.Settings.Statistics.CurrentStreak = 0;

                /* Play loss sound effect */
                if (SettingsManager.Settings.Sound.EnableEffects)
                {
                    AudioManager.PlayEffect(SoundType.Loss);
                    await Task.Delay(2000);
                }
            }

            var time = CurrentGame.CurrentTime;

            SettingsManager.Settings.Statistics.ShortestGame =
                SettingsManager.Settings.Statistics.ShortestGame == 0
                    ? time
                    : Math.Min(SettingsManager.Settings.Statistics.ShortestGame, time);

            SettingsManager.Settings.Statistics.LongestGame =
                Math.Max(SettingsManager.Settings.Statistics.LongestGame, time);

            SettingsManager.Settings.Statistics.TotalGameTime += time;

            /* Handle tournament games separately */
            if (Tournament.IsActive)
            {
                HandleTournamentGameOver(player, score);
                return;
            }

            var msg = player is HumanPlayer
                ? $"You have won and earned {score} points!\r\n\r\n"
                : $"{player} has won!\r\n\r\n";

            if (CustomMessageBox.Show(this, $@"{msg}Play again?", Resources.appTitle, CustomMessageBoxButtons.YesNo,
                CustomMessageBoxIcon.Question) == DialogResult.Yes)
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

        public void UpdateUndo()
        {
            if (InvokeRequired)
            {
                _sync.Execute(UpdateUndo);
                return;
            }

            var undo = false;
            if (CurrentGame != null)
            {
                undo = CurrentGame.GameData.CurrentPlayer is HumanPlayer && CurrentGame.Undo.CanUndo;
            }
            _mnuGame.DropDownItems[5].Enabled = undo;
        }

        private void HandleTournamentGameOver(IPlayer player, int score)
        {
            var humanWon = player is HumanPlayer;
            var completedRound = Tournament.CurrentRound;
            var tournamentWon = Tournament.RecordResult(humanWon, score);

            if (tournamentWon)
            {
                CustomMessageBox.Show(this,
                    $"Congratulations! You have won all {completedRound} rounds and completed the tournament!\r\n\r\nYou have won a total of {Tournament.TotalScore} points, including a {Tournament.Bonus} point bonus!",
                    Resources.appTitle,
                    CustomMessageBoxButtons.Ok,
                    CustomMessageBoxIcon.Information);

                /* Record the bonus points */
                SettingsManager.Settings.Statistics.HighScore += Tournament.Bonus;

                Tournament.End();
                EndCurrentGame();
                return;
            }

            if (!humanWon)
            {
                CustomMessageBox.Show(this,
                    $"Tournament over! {player} won round {completedRound}.\r\n\r\n" +
                    "You must win every round to complete the tournament.",
                    Resources.appTitle,
                    CustomMessageBoxButtons.Ok,
                    CustomMessageBoxIcon.Exclamation);

                Tournament.End();
                EndCurrentGame();
                return;
            }

            /* The human won this round, and another round remains. */
            CustomMessageBox.Show(this,
                $"You won round {completedRound} of {Tournament.TotalRounds} gaining {score} points!\r\n\r\n" +
                "Prepare for the next round.",
                Resources.appTitle,
                CustomMessageBoxButtons.Ok,
                CustomMessageBoxIcon.Information);

            EndCurrentGame();
            StartTournamentRound();
        }

        private void EndCurrentGame()
        {
            if (CurrentGame != null)
            {
                CurrentGame.Dispose();
                CurrentGame = null;
            }

            _time.Enabled = false;

            _slGame.Text = @"No game running. Select the Game menu and New Game.";
            _slTime.Visible = false;

            _controlButtons.EnableButton(CustomButtonIdentifier.Draw, false);
            _controlButtons.EnableButton(CustomButtonIdentifier.Pass, false);

            ShowButtons(false);
            ShowColorButtons(false);

            ResetAnimation();

            _logText.Clear();
            LogRenderer.Update(_logText);

            UpdateUndo();

            Invalidate();
        }

        private void PlayCard()
        {
            var game = CurrentGame.GameData;
            var discardCard = game.DiscardPile[game.DiscardPile.Count - 1];
            var player = game.CurrentPlayer;

            if (player is HumanPlayer human && human.HasDrawn && !ReferenceEquals(CurrentGame.SelectedCard, human.DrawnCard))
            {
                /* This is an offical UNO rule - you can only play the card you drew (if playable). You cannot draw a card
                 * and play another one in your hand that is playable. */
                SoundEffectRequest(SoundType.Error);
                UpdateStatus("You can only play the card you just drew.");
                return;
            }

            if (!Validation.IsCardPlayable(discardCard, CurrentGame.SelectedCard, game.CurrentColor, player.Cards))
            {
                SoundEffectRequest(SoundType.Error);
                UpdateStatus("Selected card cannot be played. Choose another, or draw.");
                return;
            }

            CurrentGame.SaveUndoState();
            player.PlayCard(CurrentGame.SelectedCard);
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

            _controlButtons.Visible = visible;
        }

        public void EnableDraw(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnableDraw(enabled));
                return;
            }

            _controlButtons.EnableButton(CustomButtonIdentifier.Draw, enabled);
        }

        public void EnablePass(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePass(enabled));
                return;
            }

            _controlButtons.EnableButton(CustomButtonIdentifier.Pass, enabled);
        }

        public void EnablePlay(bool enabled)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => EnablePlay(enabled));
                return;
            }

            _controlButtons.EnableButton(CustomButtonIdentifier.Play, enabled);
        }

        public void ShowColorButtons(bool visible)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => ShowColorButtons(visible));
                return;
            }

            _controlButtons.ShowColorButtons(visible);

            OnResize(new EventArgs());
        }
        #endregion

        #region Load/save
        private bool LoadGame(string path)
        {
            var state = new GameStateData();

            if (!BinarySerialize<GameStateData>.Load(path, ref state))
            {
                return false;
            }

            if (state == null)
            {
                return false;
            }

            CurrentGame = new Game(this, state);
            if (CurrentGame == null)
            {
                return false;
            }
            Tournament.RestoreState(state.TournamentActive, state.TournamentRound, state.TournamentScore);

            _time.Enabled = true;
            _slTime.Visible = true;

            _logText.Clear();
            LogRenderer.Update(null);

            CurrentGame.CurrentTime = state.CurrentTime;
            CurrentGame.Start();
            return true;

        }

        private bool SaveGame(string path)
        {
            if (CurrentGame == null)
            {
                return false;
            }
            var state = GameStateManager.CreateGameState(CurrentGame.GameData, CurrentGame.CurrentTime, CurrentGame.FirstMove);
            if (state == null)
            {
                return false;
            }

            state.TournamentActive = Tournament.IsActive;
            state.TournamentRound = Tournament.CurrentRound;
            state.TournamentScore = Tournament.TotalScore;

            BinarySerialize<GameStateData>.Save(path, state);
            return true;
        }
        #endregion
    }
}

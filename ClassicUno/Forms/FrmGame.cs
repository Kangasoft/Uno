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

            _sync = new UiSynchronize(this);

            SettingsManager.Load();
            GraphicsManager.Initialize();
            AudioManager.Initialize();

            BuildMenuGame(mnuGame);

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
            /* Add a check if game is running */
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
                    MenuHelper.AddMenuItem("Exit", "EXIT", Keys.Alt | Keys.F4, true,  OnMenuClick)
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

                case "EXIT":
                    Close();
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
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_currentGame == null)
            {
                base.OnMouseMove(e);
                return;
            }

            if (_currentGame.CurrentPlayer != null && _currentGame.CurrentPlayer is HumanPlayer)
            {
                /* Your turn */
                var c = HitTest(_currentGame.Players[0].Cards, e.Location);
                _currentGame.HighlightCard = c;
                Invalidate();
            }

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

        #region Timer callback
        private void ShowNewGameDialog(object sender, EventArgs e)
        {
            /* This allows the form to initialize and load properly before displaying the dialog */
            _tmrNew.Enabled = false;
            _tmrNew.Tick -= ShowNewGameDialog;
            _tmrNew.Dispose();

            NewGame();
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
                _currentGame.Dispose();
                _currentGame = null;
                Invalidate();
            }

            /* Reset default button states */
            btnDraw.Enabled = false;
            btnPass.Enabled = false;
            ShowButtons(false);
            ShowColorButtons(false);

            var data = SettingsManager.Settings.Options;
            var form = new FrmNew
            {
                NameData = data.NameData,
                NumberOfPlayers = data.NumberOfPlayers
            };
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            data.NameData = form.NameData;
            data.NumberOfPlayers = form.NumberOfPlayers;

            _currentGame = new Game(this, data);
            AudioManager.PlayEffect(SoundType.Shuffle);
            Invalidate();
        }

        private static Card HitTest(IEnumerable<Card> cards, Point location)
        {
            return cards.FirstOrDefault(c =>
                location.X >= c.Region.X && location.X <= c.Region.X + c.Region.Width && location.Y >= c.Region.Y &&
                location.Y <= c.Region.Y + c.Region.Height);
        }

        public void GameOver(IPlayer player, int score)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => GameOver(player, score));
                return;
            }

            var msg = player is HumanPlayer
                ? $"You have won and earned {score} points!"
                : $"{player.NameData.Name} has won!";

            if (MessageBox.Show(this, $@"{msg} Play again?", @"Classic UNO! 2026", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                NewGame();
                return;
            }

            _currentGame.Dispose();
            _currentGame = null;

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

/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Classes.Shared;
using ClassicUno.Classes.Shared.Management;
using ClassicUno.Controls;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    public sealed class FrmSaveLoadGame : FormEx
    {
        #region Fields
        private readonly bool _saveMode;

        private readonly BindingList<SavedGameData> _saves = SettingsManager.Settings.Saved.Games;

        private readonly BindingSource _source = new BindingSource();

        private readonly ImageList _saveImages = new ImageList
        {
            ImageSize = new Size(64, 64),
            ColorDepth = ColorDepth.Depth32Bit
        };

        private const int ButtonWidth = 75;
        private const int ButtonHeight = 23;

        private ListView _lstSaves;
        private TextBox _txtName;
        private Label _lblName;
        private Button _btnAction;
        private Button _btnClear;
        private Button _btnCancel;
        #endregion

        #region Properties
        public SavedGameData SelectedSave { get; private set; }

        public string SaveName
        {
            get => _txtName.Text.Trim() /* Trim whitespace around the "friendly" name */;
            set => _txtName.Text = value;
        } 
        #endregion

        #region Constructor
        public FrmSaveLoadGame(bool saveMode)
        {
            _saveMode = saveMode;

            BuildForm();
            LayoutBottomControls();
            RefreshList();
        }
        #endregion

        #region Form building
        private void BuildForm()
        {
            Text = _saveMode ? "Save Game" : "Load Game";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9F);

            _saveImages.Images.AddRange(new Image[]
            {
                Resources.saveIcon,
                Resources.tournamentIcon
            });

            _lstSaves = new ListView
            {
                Location = new Point(12, 12),
                Size = new Size(456, 285),
                View = View.LargeIcon,
                FullRowSelect = true,
                GridLines = false,
                HideSelection = false,
                MultiSelect = false,
                LargeImageList = _saveImages,
                ShowGroups = true,
                TabIndex = 1,
                TabStop = true,
                ShowItemToolTips = true
            };

            _lstSaves.Groups.AddRange(new[]
            {
                new ListViewGroup("Normal Games"), 
                new ListViewGroup("Tournaments") 
            });

            _lstSaves.SelectedIndexChanged += OnSelectedIndexChanged;
            _lstSaves.DoubleClick += OnDoubleClick;

            _lblName = new Label
            {
                Text = @"Name:",
                BackColor = Color.Transparent,
                Location = new Point(12, 307),
                Size = new Size(48, 23),
                Visible = _saveMode
            };

            _txtName = new TextBox
            {
                Location = new Point(60, 304),
                Size = new Size(408, 23),
                Visible = _saveMode,
                MaxLength = 80,
                TabIndex = 0,
                TabStop = true
            };

            _txtName.TextChanged += (s, e) => _btnAction.Enabled = !string.IsNullOrWhiteSpace(_txtName.Text);

            _btnAction = new Button
            {
                Text = _saveMode ? "Save" : "Load",
                Size = new Size(ButtonWidth, ButtonHeight),
                Enabled = false,
                TabIndex = 2,
                TabStop = true
            };
            _btnAction.Click += OnActionClick;

            _btnClear = new Button
            {
                Text = @"Clear",
                Size = new Size(ButtonWidth, ButtonHeight),
                Visible = !_saveMode,
                TabIndex = 3,
                TabStop = true
            };
            _btnClear.Click += OnClearClick;

            _btnCancel = new Button
            {
                Text = @"Cancel",
                Size = new Size(ButtonWidth, ButtonHeight),
                DialogResult = DialogResult.Cancel,
                TabIndex = 4,
                TabStop = true
            };

            Controls.AddRange(new Control[]{ _lstSaves, _lblName, _txtName, _btnAction, _btnClear, _btnCancel});

            AcceptButton = _btnAction;
            CancelButton = _btnCancel;

            _source.DataSource = _saves;
        }

        private void LayoutBottomControls()
        {
            const int marginRight = 12;
            const int marginBottom = 10;
            const int gap = 6;
            const int spacingBelowList = 14;

            /* Position the buttons below the ListView. */
            var y = _saveMode ? _txtName.Bottom + spacingBelowList : _lstSaves.Bottom + spacingBelowList;
            var right = _lstSaves.Right;

            _btnCancel.SetBounds(right - ButtonWidth, y, ButtonWidth, ButtonHeight);

            if (_saveMode)
            {
                _btnClear.Visible = false;

                _btnAction.SetBounds(_btnCancel.Left - gap - ButtonWidth, y, ButtonWidth, ButtonHeight);
            }
            else
            {
                _btnClear.Visible = true;

                _btnClear.SetBounds(_btnCancel.Left - gap - ButtonWidth, y, ButtonWidth, ButtonHeight);

                _btnAction.SetBounds(_btnClear.Left - gap - ButtonWidth, y, ButtonWidth, ButtonHeight);
            }

            /* Calculate the bottom edge of all visible controls. */
            var bottom = _saveMode ? _txtName.Bottom + ButtonHeight + spacingBelowList : _lstSaves.Bottom + ButtonHeight + spacingBelowList;

            /* Keep the form edges tight around the controls. */
            ClientSize = new Size(_lstSaves.Right + marginRight, bottom + marginBottom);
        }
        #endregion

        #region ListView callbacks
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = _lstSaves.SelectedItems.Count > 0;

            if (!_saveMode)
            {
                _btnAction.Enabled = selected;
            }
            else if (selected)
            {
                if (_lstSaves.SelectedItems[0].Tag is SavedGameData save)
                {
                    _txtName.Text = save.Name;
                }
            }
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (_lstSaves.SelectedItems.Count > 0)
            {
                OnActionClick(sender, e);
            }
        }
        #endregion

        #region Button callbacks
        private void OnActionClick(object sender, EventArgs e)
        {
            if (_saveMode)
            {
                SelectedSave = GetSelectedSave();

                /* Check for another save with the same friendly name. */
                var existing = _saves.FirstOrDefault(s =>
                    string.Equals(s.Name, SaveName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    if (CustomMessageBox.Show(
                        this, $"A game named \"{SaveName}\" already exists.\r\n\r\nDo you want to overwrite it?",
                        Resources.appTitle,
                        CustomMessageBoxButtons.YesNo,
                        CustomMessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    /* Reuse the existing entry and its filename. */
                    SelectedSave = existing;
                }

                if (SelectedSave != null)
                {
                    /* Overwrite the selected save's friendly name. */
                    SelectedSave.Name = SaveName;
                    SelectedSave.DateSaved = DateTime.Now;
                }
                else
                {
                    /* Create a new save entry. */
                    SelectedSave = new SavedGameData
                    {
                        Name = SaveName,
                        FileName = CreateFileName(SaveName),
                        DateSaved = DateTime.Now
                    };

                    _saves.Add(SelectedSave);
                }

                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            /* Loading mode */
            if (_lstSaves.SelectedItems.Count == 0)
            {
                return;
            }

            SelectedSave = _lstSaves.SelectedItems[0].Tag as SavedGameData;

            if (SelectedSave == null)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            if (CustomMessageBox.Show(
                    this,
                    "Are you sure you want to delete all saved games?\r\n\r\nThis operation cannot be undone.",
                    "Clear Saved Games",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Exclamation) != DialogResult.Yes)
            {
                return;
            }

            /* Iterate the saved files in the binding list */
            foreach (var f in _saves.Where(f => File.Exists(f.FileName)))
            {
                File.Delete(f.FileName);
            }

            /* Remove the entries from settings */
            _saves.Clear();
            _lstSaves.Items.Clear();
            _btnClear.Enabled = false;
        }
        #endregion

        #region Private helpers
        private void RefreshList()
        {
            _lstSaves.BeginUpdate();

            try
            {
                _lstSaves.Items.Clear();

                foreach (var item in from save in _saves let index = save.Tournament ? 1 : 0 select new ListViewItem(
                    $"{ShortenName(save.Name)}",
                    index)
                {
                    Tag = save,
                    Group = _lstSaves.Groups[index],
                    ToolTipText = $"{save.Name}\r\nSaved: {save.DateSaved:dd/MM/yyyy HH:mm}"
                })
                {
                    _lstSaves.Items.Add(item);
                }
            }
            finally
            {
                _lstSaves.EndUpdate();
            }

            if (!_saveMode)
            {
                _btnAction.Enabled = false;
            }
        }

        private static string ShortenName(string name, int maxLength = 32)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= maxLength)
            {
                return name;
            }

            return $"{name.Substring(0, maxLength - 3)}...";
        }

        private SavedGameData GetSelectedSave()
        {
            if (_lstSaves.SelectedItems.Count == 0)
            {
                return null;
            }

            return _lstSaves.SelectedItems[0].Tag as SavedGameData;
        }

        private static string CreateFileName(string friendlyName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            var fileName = new string(
                friendlyName
                    .Replace(' ', '_')
                    .Where(c => !invalidChars.Contains(c))
                    .ToArray());

            return Misc.MainDir(string.Format(Resources.userPath, $"saved\\{fileName}.dat"), true);
        }
        #endregion
    }
}
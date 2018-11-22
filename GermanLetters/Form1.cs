using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotkeys;
using SavedControls;
using System.Runtime.InteropServices;

namespace GermanLetters
{


    public partial class Form1 : Form
    {
        #region Setting Dropdown options
        Dictionary<int, string> dropDownModifers = new Dictionary<int, string>() {
            {Constants.NOMOD, "----" },
            {Constants.CTRL, "CTRL" },
            {Constants.ALT, "ALT" },
            {Constants.WIN, "WIN" },
            {Constants.SHIFT, "SHIFT" }
        };

        Dictionary<Keys, string> dropDownKeys = new Dictionary<Keys, string>() {
            {Keys.A, "A"},
            {Keys.B, "B"},
            {Keys.C, "C"},
            {Keys.D, "D"},
            {Keys.E, "E"},
            {Keys.F, "F"},
            {Keys.G, "G"},
            {Keys.H, "H"},
            {Keys.I, "I"},
            {Keys.J, "J"},
            {Keys.K, "K"},
            {Keys.L, "L"},
            {Keys.M, "M"},
            {Keys.N, "N"},
            {Keys.O, "O"},
            {Keys.P, "P"},
            {Keys.Q, "Q"},
            {Keys.R, "R"},
            {Keys.S, "S"},
            {Keys.T, "T"},
            {Keys.U, "U"},
            {Keys.V, "V"},
            {Keys.W, "W"},
            {Keys.X, "X"},
            {Keys.Y, "Y"},
            {Keys.Z, "Z"},
            {Keys.PageUp, "Page Up"},
            {Keys.PageDown, "Page Down"},
            {Keys.Home, "Home"},
            {Keys.End, "End"},
            {Keys.Delete, "Delete"},
            {Keys.Insert, "Insert"},
            {Keys.OemPeriod, "."},
            {Keys.Oemcomma, ","},
            {Keys.OemQuestion, "?"},
            {Keys.Oemtilde, "~"},
            {Keys.OemSemicolon, ";"},
            {Keys.OemQuotes, "\""},
            {Keys.OemBackslash, "|"},
            {Keys.OemOpenBrackets, "{"},
            {Keys.OemCloseBrackets, "}"},
            {Keys.OemMinus, "-"},
            {Keys.Oemplus, "+"},
            {Keys.NumLock, "Num Lock"},
            {Keys.NumPad0, "Num Pad 0"},
            {Keys.NumPad1, "Num Pad 1"},
            {Keys.NumPad2, "Num Pad 2"},
            {Keys.NumPad3, "Num Pad 3"},
            {Keys.NumPad4, "Num Pad 4"},
            {Keys.NumPad5, "Num Pad 5"},
            {Keys.NumPad6, "Num Pad 6"},
            {Keys.NumPad7, "Num Pad 7"},
            {Keys.NumPad8, "Num Pad 8"},
            {Keys.NumPad9, "Num Pad 9"},
            {Keys.D0, "Number 0"},
            {Keys.D1, "Number 1"},
            {Keys.D2, "Number 2"},
            {Keys.D3, "Number 3"},
            {Keys.D4, "Number 4"},
            {Keys.D5, "Number 5"},
            {Keys.D6, "Number 6"},
            {Keys.D7, "Number 7"},
            {Keys.D8, "Number 8"},
            {Keys.D9, "Number 9"},
            {Keys.Tab, "Tab"},
            {Keys.CapsLock, "Caps Lock"},
            {Keys.Escape, "ESC"},
            {Keys.F1, "F1"},
            {Keys.F2, "F2"},
            {Keys.F3, "F3"},
            {Keys.F4, "F4"},
            {Keys.F5, "F5"},
            {Keys.F6, "F6"},
            {Keys.F7, "F7"},
            {Keys.F8, "F8"},
            {Keys.F9, "F9"},
            {Keys.F10, "F10"},
            {Keys.F11, "F11"},
            {Keys.F12, "F12"},
            {Keys.Scroll, "Scroll Lock"}
        };
        #endregion

        List<keyBindStruct> keybinds = new List<keyBindStruct>();

        List<userInputControlStruct> comboboxes = new List<userInputControlStruct>();

        public Form1()
        {
            InitializeComponent();
            Resize += Form1_OnMin;

            comboboxes.Add(new userInputControlStruct(aUmlautMod1, aUmlautMod2, aUmlautMod3, aUmlautKey, textToSend1, 0));
            comboboxes.Add(new userInputControlStruct(eUmlautMod1, eUmlautMod2, eUmlautMod3, eUmlautKey, textToSend2, 1));
            comboboxes.Add(new userInputControlStruct(uUmlautMod1, uUmlautMod2, uUmlautMod3, uUmlautKey, textToSend3, 2));
            comboboxes.Add(new userInputControlStruct(esssetUmlautMod1, esssetUmlautMod2, esssetUmlautMod3, esssetUmlautKey, textToSend4, 3));

        }


        private void setDropdownOptions<T>(List<syncComboBox> boxes, Dictionary<T, string> itemsToAdd)
        {
            foreach (syncComboBox x in boxes)
            {
                x.DataSource = itemsToAdd.ToList();
                x.DisplayMember = "Value";
                x.ValueMember = "Key";
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            List<syncComboBox> modComboBoxes = new List<syncComboBox>();
            List<syncComboBox> keyComboBoxes = new List<syncComboBox>();
            foreach (userInputControlStruct x in comboboxes)
            {
                modComboBoxes.AddRange(x.modifiers);
                keyComboBoxes.Add(x.keycomboBox);
            }

            setDropdownOptions(modComboBoxes, dropDownModifers);
            setDropdownOptions(keyComboBoxes, dropDownKeys);

            if (!syncComboBox.loadFromFile())
            {
                setDefaultKeyBinds();
                syncComboBox.UpdateAllSavedIndexes();
            }

            if (!syncTextBox.load())
            {
                setDefaultToSendText();
            }

            syncComboBox.onAnyIndexChanged = ComboBoxIndexsChanged;

            notifyIcon1.Icon = Properties.Resources.dank_AuG_icon;
            notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Options", showForm), new MenuItem("Stop and exit", Exit) });
            notifyIcon1.Text = "German Letters";
            ShowInTaskbar = false;
            Hide();
            this.Visible = false;

            setKeybindsToSelected();
            createAllKeybinds();
            registerAllKeybinds();

        }

        private void ComboBoxIndexsChanged()
        {
            unregisterAllKeybinds();
            setKeybindsToSelected();
            createAllKeybinds();
            registerAllKeybinds();
        }

        private void setDefaultKeyBinds()
        {
            aUmlautMod1.SelectedValue = Constants.NOMOD;
            aUmlautMod2.SelectedValue = Constants.NOMOD;
            aUmlautMod3.SelectedValue = Constants.CTRL;
            eUmlautMod1.SelectedValue = Constants.NOMOD;
            eUmlautMod2.SelectedValue = Constants.NOMOD;
            eUmlautMod3.SelectedValue = Constants.CTRL;
            uUmlautMod1.SelectedValue = Constants.NOMOD;
            uUmlautMod2.SelectedValue = Constants.NOMOD;
            uUmlautMod3.SelectedValue = Constants.CTRL;
            esssetUmlautMod1.SelectedValue = Constants.NOMOD;
            esssetUmlautMod2.SelectedValue = Constants.NOMOD;
            esssetUmlautMod3.SelectedValue = Constants.CTRL;

            aUmlautKey.SelectedValue = Keys.D1;
            eUmlautKey.SelectedValue = Keys.D2;
            uUmlautKey.SelectedValue = Keys.D3;
            esssetUmlautKey.SelectedValue = Keys.D4;

            TextSendComboBox.SelectedIndex = 1;

        }

        private void setKeybindsToSelected()
        {
            if (keybinds.Count == 0)
            {
                foreach (userInputControlStruct x in comboboxes)
                {
                    keybinds.Add(new keyBindStruct(null, (int)x.modifiers[0].SelectedValue | (int)x.modifiers[1].SelectedValue | (int)x.modifiers[2].SelectedValue, (Keys)x.keycomboBox.SelectedValue, x.toSendText ,x.index ));
                }

            }
            else
            {
                foreach (userInputControlStruct x in comboboxes)
                {
                    keybinds[x.index].modifiers = (int)x.modifiers[0].SelectedValue | (int)x.modifiers[1].SelectedValue | (int)x.modifiers[2].SelectedValue;
                    keybinds[x.index].key = (Keys)x.keycomboBox.SelectedValue;
                }
            }

        }

        private void setDefaultToSendText()
        {
            textToSend1.Text = "ä";
            textToSend2.Text = "ë";
            textToSend3.Text = "ü";
            textToSend4.Text = "ß";

        }

        private void createAllKeybinds()
        {
            foreach(keyBindStruct x in keybinds)
            {
                x.winApiHotkey = new KKHotkeys(x.modifiers, x.key, this, HotkeyPressedCallback, x.index);
            }
        }

        private void registerAllKeybinds()
        {
            foreach (keyBindStruct x in keybinds)
            {
                if (x.winApiHotkey != null)
                    x.winApiHotkey.Register(this);
            }
        }

        private void unregisterAllKeybinds()
        {
            foreach (keyBindStruct x in keybinds)
            {
                if (x.winApiHotkey != null)
                    x.winApiHotkey.Unregiser();
            }
        }

        private void resetKeybinds()
        {
            unregisterAllKeybinds();
            registerAllKeybinds();
        }

        private void Form1_OnMin(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                hideForm();
            }
        }


        void showForm(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            ShowInTaskbar = true;
            Show();
            Activate();
            BringToFront();
            WindowState = FormWindowState.Normal;
            resetKeybinds();
        }

        void hideForm()
        {
            Hide();
            this.Visible = false;
            notifyIcon1.Visible = true;
            resetKeybinds();
        }

        void Exit(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;

            Application.Exit();
        }

        private void sendKeyMethod2(string toSend)
        {
            string[] unk = Clipboard.GetDataObject().GetFormats();

            object[] dank = new object[unk.Length];

            for (int i = 0; i < unk.Length; i++)
            {
                dank[i] = Clipboard.GetData(unk[i]);
            }

            Clipboard.SetText(toSend);

            SendKeys.Send("^V");

            for (int i = 0; i < unk.Length; i++)
            {
                Clipboard.SetData(unk[i], dank[i]);
            }
        }

        private void sendText(string txt)
        {
            if (TextSendComboBox.SelectedIndex == 0)
            {
                SendKeys.Send(txt);
            }
            else
            {
                sendKeyMethod2(txt);
            }
        }

        public void HotkeyPressedCallback(int modifiers, Keys k, int index)
        {
            sendText(keybinds[index].textToSend.Text);
        }

        protected override void WndProc(ref Message m)
        {
            KKHotkeys.callHandler(ref m);
            base.WndProc(ref m);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            hideForm();
        }

    }

    public class userInputControlStruct
    {
        public syncComboBox[] modifiers { get; set; }
        public syncComboBox keycomboBox { get; set; }
        public syncTextBox toSendText { get; set; }
        public int index { get; }

        public userInputControlStruct(int index1 = -1) { index = index1; }

        public userInputControlStruct(syncComboBox mod1, syncComboBox mod2, syncComboBox mod3, syncComboBox keybox, syncTextBox tosend, int index1 = -1)
        {
            modifiers = new syncComboBox[] { mod1, mod2, mod3 };
            keycomboBox = keybox;
            index = index1;
            toSendText = tosend;
        }

        public userInputControlStruct(syncComboBox[] mods, syncComboBox keybox, syncTextBox tosend, int index1 = -1)
        {
            modifiers = mods;
            keycomboBox = keybox;
            index = index1;
            toSendText = tosend;
        }
    }

    public class keyBindStruct
    {
        public int index { get; }
        public KKHotkeys winApiHotkey { get; set; }
        public int modifiers { get; set; }
        public Keys key { get; set; }
        public syncTextBox textToSend { get; set; }

        public keyBindStruct(int index1 = -1) { index = index1; }

        public keyBindStruct(KKHotkeys hotkey, int modifiers1, Keys key1, syncTextBox toSendTextBox,  int index1 = -1)
        {
            index = index1;
            winApiHotkey = hotkey;
            modifiers = modifiers1;
            key = key1;
            textToSend = toSendTextBox;
        }
    }
}

using NAudio.Midi;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LaunchToy.UserControls
{
    /// <summary>
    /// Interaction logic for PadButton.xaml
    /// </summary>
    public partial class PadButton : NovationButton
    {
        public override MidiCommandCode CommandCode => MidiCommandCode.NoteOn;
        public int ButtonIndex { get; set; }
        protected override Image CheckmarkImage => this.checkmarkImage;
        protected override MenuItem EditAssignmentMenu => this.editAssignmentMenu;
        protected override MenuItem RemoveAssignmentMenu => this.removeAssignmentMenu;
        protected override MenuItem CreateStopAssignmentMenu => this.createStopAssignmentMenu;
        protected override MenuItem? CreateQuantizeToggleAssignmentMenu { get; } = null;

        private Color[] RowColors = new Color[] { 
            Color.FromArgb(80, 255,0,255),
            Color.FromArgb(80, 0,0,255),
            Color.FromArgb(80, 255,64,0),
            Color.FromArgb(80, 255,255,0),
            Color.FromArgb(80, 255,128,128),
            Color.FromArgb(80, 0,255,255),
            Color.FromArgb(80, 0,255,0),
            Color.FromArgb(80, 255,64,255),
            Color.FromArgb(80, 255,0,64),
        };

        public override int GetMidiValue()
        {
            if (Env.ButtonLayout == ButtonLayout.Drum)
            {
                return ((this.ButtonIndex >> 4) + Env.LayoutOffset) * 4 + (this.ButtonIndex % 4) + ((this.ButtonIndex % 8) / 4) * 0x20;
            }
            else
            {
                // Not implemented
                return this.ButtonIndex;
            }
        }

        public PadButton()
        {
            InitializeComponent();

            Env.AssignmentsChanged += Env_AssignmentsChanged;
            Env.LayoutChanged += Env_AssignmentsChanged;
            Env.ProjectChanged += Env_ProjectChanged;
        }

        protected override void Env_AssignmentsChanged()
        {
            if (Env.ButtonLayout == ButtonLayout.Drum)
            {
                var row = (((this.ButtonIndex >> 4) + Env.LayoutOffset + 3) / 4) + ((this.ButtonIndex % 8) / 4 == 0 ? 0 : 2);
                this.overlayBrush.Color = row < this.RowColors.Length ? this.RowColors[row] : Colors.Transparent;
            }
            else
            {
                // Not implemented
            }

            base.Env_AssignmentsChanged();
        }

        private bool isRunning;

        public override void Flash()
        {
            var flashButton = FindResource("FlashButton") as Storyboard;
            flashButton?.Begin();
        }
    }
}

using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaunchToy.UserControls
{
    /// <summary>
    /// Interaction logic for CtrlButton.xaml
    /// </summary>
    public partial class CtrlButton : NovationButton
    {
        public override MidiCommandCode CommandCode => MidiCommandCode.ControlChange;
        public NovationControlChange NovationControlChange
        {
            get => this.novationControlChange;
            set
            {
                this.novationControlChange = value;
                UpdateText();
            }
        }
        public ControlButtonSpecialFunction SpecialFunction 
        {
            get => this.specialFunction;
            set
            {
                this.specialFunction = value;
                UpdateText();
            }
        }
        protected override Image CheckmarkImage => this.checkmarkImage;
        protected override MenuItem EditAssignmentMenu => this.editAssignmentMenu;
        protected override MenuItem RemoveAssignmentMenu => this.removeAssignmentMenu;
        protected override MenuItem CreateStopAssignmentMenu => this.createStopAssignmentMenu;
        protected override MenuItem? CreateQuantizeToggleAssignmentMenu => this.createQuantizeToggleAssignmentMenu;
        public override int GetMidiValue() => (int)this.NovationControlChange;
        private NovationControlChange novationControlChange;
        private ControlButtonSpecialFunction specialFunction;
        protected override bool CheckDropOK(DragEventArgs e)
        {
            return this.NovationControlChange == NovationControlChange.None ? false : base.CheckDropOK(e);
        }

        public CtrlButton()
        {
            InitializeComponent();

            Env.AssignmentsChanged += Env_AssignmentsChanged;
            Env.ProjectChanged += Env_ProjectChanged;
        }

        protected override void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Env.Project == null)
            {
                return;
            }

            switch (this.SpecialFunction)
            {
                case ControlButtonSpecialFunction.PageUp:
                    {
                        Env.LayoutOffset = Math.Min(Env.LayoutOffset + 4, Env.LayoutRowsCount - 8);
                        break;
                    }
                case ControlButtonSpecialFunction.PageDown:
                    {
                        Env.LayoutOffset = Math.Max(Env.LayoutOffset - 4, 0);
                        break;
                    }
                case ControlButtonSpecialFunction.RowUp:
                    {
                        Env.LayoutOffset = Math.Min(Env.LayoutOffset + 1, Env.LayoutRowsCount - 8);
                        break;
                    }
                case ControlButtonSpecialFunction.RowDown:
                    {
                        Env.LayoutOffset = Math.Max(Env.LayoutOffset - 1, 0);
                        break;
                    }
                default:
                    {
                        base.Image_MouseLeftButtonDown(sender, e);
                        return;
                    }
            }

            Env.OnLayoutChanged();
        }

        private void UpdateText()
        {
            var size = 12;
            this.labelTextBlock.Text = GetLabel(out size);
            this.labelTextBlock.FontSize = size;
        }

        private string GetLabel(out int size)
        {
            size = 18;
            switch (this.SpecialFunction)
            {
                case ControlButtonSpecialFunction.PageUp: return "▲";
                case ControlButtonSpecialFunction.PageDown: return "▼";
                case ControlButtonSpecialFunction.RowUp: return "◀";
                case ControlButtonSpecialFunction.RowDown: return "▶";
                default:
                    {
                        switch (this.NovationControlChange)
                        {
                            case NovationControlChange.Row0:
                            case NovationControlChange.Row1:
                            case NovationControlChange.Row2:
                            case NovationControlChange.Row3:
                            case NovationControlChange.Row4:
                            case NovationControlChange.Row5:
                            case NovationControlChange.Row6:
                            case NovationControlChange.Row7:
                                return "▷";
                            default:
                                size = 10;
                                return this.NovationControlChange.ToString("g").Replace("_", " ");
                        }                        
                    }
            }
        }

        public override void Flash()
        {

        }
    }
}

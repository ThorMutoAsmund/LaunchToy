using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy
{
    public enum QuantizeState
    {
        Off,
        Waiting,
        Active
    }

    public enum SpecialFunction
    {
        None,
        GroupStop,
        ArmToggle
    }
    public enum QuantizationMethod
    {
        None,
        Q1,
        Q1_2,
        Q1_4,
        Q1_8,
        Q1_16,
        Q1_32,
        Q1_64,
        Q1_3,
        Q1_6,
        Q1_5,
        Q2,
        Q4,
        Q3
    }

    public enum PlayMode
    {
        Poly,
        Mono,
        Gated,
        Looped
    }

    public enum ProjectChangedAction
    {
        Opened,
        Closed,
        Saved
    }

    public enum ButtonLayout
    {
        Drum,
        Note
    }

    public enum ControlButtonSpecialFunction
    {
        None,
        PageUp,
        PageDown,
        RowUp,
        RowDown
    }

    public enum NovationControlChange // B0
    {
        None = 0,
        Record_Arm = 0x01,
        Track_Select = 0x02,
        Mute = 0x03,
        Solo = 0x04,
        Volume = 0x05,
        Pan = 0x06,
        Sends = 0x07,
        Stop_Clip = 0x08,
        Record = 0x0A,
        Row7 = 0x13,
        Double = 0x14,
        Row6 = 0x1D,
        Duplicate = 0x1E,
        Row5 = 0x27,
        Quantise = 0x28,
        Row4 = 0x31,
        Delete = 0x32,
        Row3 = 0x3B,
        Undo = 0x3c,
        Row2 = 0x45,
        Click = 0x46,
        Row1 = 0x4F,
        Shift = 0x50,
        Row0 = 0x59,
        User = 0x62,
        Device = 0x61,
        Note = 0x60,
        Session = 0x5F,
    }
}

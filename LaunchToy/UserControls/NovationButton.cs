using LaunchToy.Dialogs;
using LaunchToy.Impl;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaunchToy.UserControls
{
    public abstract class NovationButton : UserControl
    {
        public abstract MidiCommandCode CommandCode { get; }
        public abstract int GetMidiValue();
        protected abstract Image CheckmarkImage { get; }
        protected abstract MenuItem EditAssignmentMenu { get; }
        protected abstract MenuItem RemoveAssignmentMenu { get; }
        protected abstract MenuItem CreateStopAssignmentMenu { get; }
        protected abstract MenuItem? CreateQuantizeToggleAssignmentMenu { get; }

        private bool isAssigned;
        protected virtual bool CheckDropOK(DragEventArgs e)
        {
            return !Env.IsRunning && e.Data.GetDataPresent(DragDropKey.Sample);
        }

        protected void partCanvas_DragEnter(object sender, DragEventArgs e)
        {
            // Highlight the button
            if (CheckDropOK(e))
            {
                this.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        protected void partCanvas_PreviewDragOver(object sender, DragEventArgs e)
        {
            // Check if dropping is ok
            if (!CheckDropOK(e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        protected void partCanvas_DragLeave(object sender, DragEventArgs e)
        {
            this.BorderBrush = null;
        }

        protected void partCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DragDropKey.Sample))
            {
                var sample = e.Data.GetData(DragDropKey.Sample) as Sample;

                if (sample != null)
                {
                    Project.AddAssignment(sample, this.CommandCode, GetMidiValue());
                }
            }

            partCanvas_DragLeave(sender, e);
        }

        protected virtual void Env_AssignmentsChanged()
        {
            if (Env.Project == null)
            {
                this.CheckmarkImage.Visibility = Visibility.Hidden;
                this.isAssigned = false;
                return;
            }

            this.isAssigned = Env.Project.Assignments.Any(a => a.CommandCode == this.CommandCode && a.MidiValue == GetMidiValue());
            this.CheckmarkImage.Visibility = this.isAssigned ? Visibility.Visible : Visibility.Hidden;
        }

        protected void Env_ProjectChanged(ProjectChangedAction projectChangedAction)
        {
            if (projectChangedAction == ProjectChangedAction.Closed || projectChangedAction == ProjectChangedAction.Opened)
            {
                Env_AssignmentsChanged();
            }
        }

        protected void partCanvas_EditAssignment(object sender, RoutedEventArgs e)
        {
            EditAssignment();
        }

        protected void partCanvas_RemoveAssignment(object sender, RoutedEventArgs e)
        {
            Project.RemoveAssigments(this.CommandCode, GetMidiValue());
        }

        protected void partCanvas_CreateStopAssignment(object sender, RoutedEventArgs e)
        {
            Project.AddSpecialFunction(this.CommandCode, GetMidiValue(), SpecialFunction.GroupStop);
        }

        protected void partCanvas_CreateQuantizeToggleAssigment(object sender, RoutedEventArgs e)
        {
            Project.AddSpecialFunction(this.CommandCode, GetMidiValue(), SpecialFunction.ArmToggle);
        }

        protected virtual void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Env.Project == null)
            {
                return;
            }

            if (Env.IsRunning)
            {
                var tick = Env.TickCount;

                if (Env.QuantizeState == QuantizeState.Waiting)
                {
                    AudioConfig.StartQuantizing(Env.Project.BPM, Env.Project.BeatsPerBar, tick);
                    Env.ChangePower(Env.IsRunning, QuantizeState.Active);
                }

                var assignment = Env.Project.Assignments.FirstOrDefault(a => a.CommandCode == this.CommandCode && a.MidiValue == GetMidiValue());
                if (assignment != null)
                {
                    AudioConfig.AddAssigment(assignment, tick);
                }
            }
        }

        protected void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Env.Project == null)
            {
                return;
            }

            if (Env.IsRunning)
            {
                var assignment = Env.Project.Assignments.FirstOrDefault(a => a.CommandCode == this.CommandCode && a.MidiValue == GetMidiValue());
                if (assignment != null)
                {
                    AudioConfig.RemoveAssignmentIfGated(assignment);
                }
            }
            else
            {
                EditAssignment();
            }
        }

        private void EditAssignment()
        {
            if (Env.Project == null)
            {
                return;
            }

            if (Env.Project.TryGetAssignment(this.CommandCode, GetMidiValue(), out var assignment) && assignment != null)
            {
                if (EditAssignmentDialog.OpenDialog(assignment))
                {
                    Env.OnAssignmentsChanged();
                    Env.OnDirtyChanged(true);
                }
            }
        }

        protected void contextMenu_Opened(object sender, RoutedEventArgs e)
        {
            this.EditAssignmentMenu.IsEnabled = Env.IsProjectLoaded && !Env.IsRunning && this.isAssigned;
            this.RemoveAssignmentMenu.IsEnabled = Env.IsProjectLoaded && !Env.IsRunning && this.isAssigned;
            this.CreateStopAssignmentMenu.IsEnabled = Env.IsProjectLoaded && !Env.IsRunning && !this.isAssigned;
            if (this.CreateQuantizeToggleAssignmentMenu != null)
            {
                this.CreateQuantizeToggleAssignmentMenu.IsEnabled = Env.IsProjectLoaded && !Env.IsRunning && !this.isAssigned;
            }
        }

        public abstract void Flash();
    }
}

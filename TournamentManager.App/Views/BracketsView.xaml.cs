using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TournamentManager.Client.ViewModels;
using System.Linq;

namespace TournamentManager.Client.Views
{
    public partial class BracketsView : UserControl
    {
        private Point _startPos;
        private bool _isDraggingParticipant;
        private DragInfo _pendingDragInfo;

        public BracketsView()
        {
            InitializeComponent();
        }

        private class DragInfo
        {
            public MatchItemViewModel SourceMatch { get; set; }
            public int SourceSlot { get; set; } // 1 or 2
        }

        private void Participant_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPos = e.GetPosition(this);
            _isDraggingParticipant = false;

            var tb = sender as TextBlock;
            if (tb == null) return;
            if (tb.DataContext is MatchItemViewModel m)
            {
                int slot = Grid.GetRow(tb) == 0 ? 1 : 2;
                _pendingDragInfo = new DragInfo { SourceMatch = m, SourceSlot = slot };
            }
        }

        private void Participant_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _pendingDragInfo = null;
            _isDraggingParticipant = false;
        }

        private void Participant_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_pendingDragInfo == null) return;

            var pos = e.GetPosition(this);
            if (!_isDraggingParticipant)
            {
                if (Math.Abs(pos.X - _startPos.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(pos.Y - _startPos.Y) < SystemParameters.MinimumVerticalDragDistance)
                    return;

                _isDraggingParticipant = true;
            }

            // ensure source slot not locked
            if ((_pendingDragInfo.SourceSlot == 1 && _pendingDragInfo.SourceMatch.FirstParticipantLocked) ||
                (_pendingDragInfo.SourceSlot == 2 && _pendingDragInfo.SourceMatch.SecondParticipantLocked))
            {
                return;
            }

            var data = new DataObject("MatchDrag", _pendingDragInfo);

            // start drag from TextBlock element
            var tb = sender as TextBlock;
            DragDrop.DoDragDrop(tb ?? (DependencyObject)this, data, DragDropEffects.Move);

            e.Handled = true;
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // allow dragging to be initiated also by moving over listbox (keeps compatibility)
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_pendingDragInfo == null) return;
            if (_isDraggingParticipant == false)
            {
                var pos = e.GetPosition(this);
                if (Math.Abs(pos.X - _startPos.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(pos.Y - _startPos.Y) < SystemParameters.MinimumVerticalDragDistance)
                    return;

                _isDraggingParticipant = true;
            }

            var listBox = sender as ListBox;
            if (listBox == null) return;

            if (_pendingDragInfo.SourceSlot == 1 && _pendingDragInfo.SourceMatch.FirstParticipantLocked) return;
            if (_pendingDragInfo.SourceSlot == 2 && _pendingDragInfo.SourceMatch.SecondParticipantLocked) return;

            var data = new DataObject("MatchDrag", _pendingDragInfo);
            DragDrop.DoDragDrop(listBox, data, DragDropEffects.Move);
        }

        private async void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("MatchDrag")) return;
            var info = e.Data.GetData("MatchDrag") as DragInfo;
            if (info == null) return;

            var listBox = sender as ListBox;
            if (listBox == null) return;

            // find target ListBoxItem under mouse
            Point pt = e.GetPosition(listBox);
            var element = listBox.InputHitTest(pt) as DependencyObject;
            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            MatchItemViewModel targetMatch = null;
            int targetSlot = 1;

            if (element != null)
            {
                var targetContainer = element as ListBoxItem;
                if (targetContainer != null)
                {
                    targetMatch = listBox.ItemContainerGenerator.ItemFromContainer(targetContainer) as MatchItemViewModel;
                    var rel = e.GetPosition(targetContainer);
                    targetSlot = rel.Y < (targetContainer.ActualHeight / 2) ? 1 : 2;
                }
            }

            // If dropped not on an item, find first suitable empty slot in the list
            if (targetMatch == null)
            {
                // decide slot by Y position relative to listbox: upper half -> slot 1, lower half -> slot 2
                targetSlot = (pt.Y < listBox.ActualHeight / 2) ? 1 : 2;

                targetMatch = listBox.Items
                    .OfType<MatchItemViewModel>()
                    .FirstOrDefault(m => (targetSlot == 1 ? m.FirstParticipantTournamentCategoryId == 0 : m.SecondParticipantTournamentCategoryId == null));

                if (targetMatch == null)
                {
                    // try opposite slot if none in chosen
                    int altSlot = targetSlot == 1 ? 2 : 1;
                    targetMatch = listBox.Items
                        .OfType<MatchItemViewModel>()
                        .FirstOrDefault(m => (altSlot == 1 ? m.FirstParticipantTournamentCategoryId == 0 : m.SecondParticipantTournamentCategoryId == null));
                    if (targetMatch != null) targetSlot = altSlot;
                }

                if (targetMatch == null)
                {
                    // nothing to accept drop into
                    MessageBox.Show("Нет доступного слота в этом раунде для размещения участника.", "Информация");
                    return;
                }
            }

            // don't allow dropping into locked slot
            if (targetSlot == 1 && targetMatch.FirstParticipantLocked)
            {
                MessageBox.Show("Эта позиция закреплена и недоступна для изменения.", "Внимание");
                return;
            }
            if (targetSlot == 2 && targetMatch.SecondParticipantLocked)
            {
                MessageBox.Show("Эта позиция закреплена и недоступна для изменения.", "Внимание");
                return;
            }

            // get source participant data
            int sourceId = 0;
            string sourceName = string.Empty;
            if (info.SourceSlot == 1)
            {
                sourceId = info.SourceMatch.FirstParticipantTournamentCategoryId;
                sourceName = info.SourceMatch.FirstParticipantName;
            }
            else
            {
                sourceId = info.SourceMatch.SecondParticipantTournamentCategoryId ?? 0;
                sourceName = info.SourceMatch.SecondParticipantName ?? string.Empty;
            }

            if (sourceId == 0) return; // nothing to move

            // avoid noop (same match and slot)
            if (targetMatch == info.SourceMatch && targetSlot == info.SourceSlot)
                return;

            // confirmation
            var confirm = MessageBox.Show(
                "Точно ли вы хотите переместить участника в следующий поединок? Эти действия больше невозможно будет отменить.",
                "Подтверждение перемещения",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            // call ViewModel to persist change
            if (this.DataContext is BracketsViewModel vm)
            {
                bool ok = await vm.MoveParticipantAsync(info.SourceMatch, info.SourceSlot, targetMatch, targetSlot);
                if (ok)
                {
                    // update UI target slot, do NOT clear source
                    if (targetSlot == 1)
                    {
                        targetMatch.FirstParticipantTournamentCategoryId = sourceId;
                        targetMatch.FirstParticipantName = sourceName;
                        targetMatch.FirstParticipantLocked = true;
                    }
                    else
                    {
                        targetMatch.SecondParticipantTournamentCategoryId = sourceId;
                        targetMatch.SecondParticipantName = sourceName;
                        targetMatch.SecondParticipantLocked = true;
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось зафиксировать изменение на сервере.", "Ошибка");
                }
            }

            _pendingDragInfo = null;
            _isDraggingParticipant = false;

            e.Handled = true;
        }

        private async void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            object tagObj = btn.Tag;
            int slot;
            if (tagObj is int ti) slot = ti;
            else if (tagObj is string ts && int.TryParse(ts, out var parsed)) slot = parsed;
            else return;

            var container = FindAncestor<ListBoxItem>(btn);
            if (container == null) return;
            var match = container.DataContext as MatchItemViewModel;
            if (match == null) return;

            // confirmation: irreversible action
            var confirm = MessageBox.Show(
                "Подтвердите перемещение участника вправо. Действие нельзя отменить.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            if (confirm != MessageBoxResult.Yes) return;

            if (this.DataContext is BracketsViewModel vm)
            {
                await vm.MoveParticipantByRounds(match, slot, +1);
            }
        }

        private T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject current = child;
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}

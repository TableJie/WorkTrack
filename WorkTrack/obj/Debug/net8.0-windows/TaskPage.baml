   M S B A M L   `   `   `  ����D  @WorkTrack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:WorkTrack	WorkTrack  5      9!clr-namespace:WorkTrack.ViewModelWorkTrack.ViewModel  5          WorkTrack.TaskPage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      )localclr-namespace:WorkTrack     5      7	viewmodel!clr-namespace:WorkTrack.ViewModel     5      _ [MaterialDesignThemes.Wpf, Version=4.5.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      :d2http://schemas.microsoft.com/expression/blend/2008  5        � DataContext  5      '   !WorkTrack.ViewModel.TaskViewModel  5   
   .��5   
   � $��Stretch=�6   .+�5   
   ��    	CardStyle#��[  6   ���#F3F3F3._�5      �  � RowDefinitions 5      �� 5      $��40 �6$   �� 5      $��* �6$   .+�5      �  � ColumnDefinitions 5      �� 5      $��* �6+   �� 5      $��* �6+   .+�5      �� $��TaskList��6    $��0��60   $
��Left=�5   !   $��Center=�5   !   $��18(�5   !   $
��Bold"�5    !   $��5,0,0,0q�5!   !   �� 5#       �� Orientation$ 
Horizontal=�6!   $��1��6:   $��Center=�6J   $��Right=�6e   .+�5$      (  "System.Windows.Controls.DatePicker   ( $MaterialDesignFloatingHintDatePicker#��[ 5%   %   $��90��5&   %   $��30��5'   %   $��	5,-10,5,0q�5(   %   $��Center=�5)   %   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ TaskDate��5*   %      FloatingOffset$ 0,-12&�5+   %      FirstDayOfWeek$ Monday=�5,   %      SelectedDateFormat$ Short=�5-   %      SelectedDate 5$   %   �� *SelectedDate+	 �� Mode$	 TwoWay=�5-   %   �� 5/      $��30��6D   $
��-5,0q�6O   $��0q�6^     MaterialDesignFlatButton#��[ 6i   ��6!   �� *AddTaskCommand+.��50      '  !MaterialDesignThemes.Wpf.PackIcon  
   Kind$
 ClipboardPlusOutline=�66   $��20��51   6   $��20��52   6   ���#008080$��Center=�54   6   $��Center=�55   6   56      57      58      &   System.Windows.Controls.DataGrid  5:      $��1��6F      AutoGenerateColumns$ False��5;         CanUserAddRows$ False��5<         CanUserDeleteRows$ False��5=         
IsReadOnly$
 True��5>         SelectionUnit$ FullRow=�5?      $��0q�5@      
�������$��5q�5B         ColumnHeaderHeight$ 40Q�5C         	RowHeight$ 35Q�5D        MaterialDesignDataGrid#��[ 5E      ��5:      �� *TaskBodyCollection+   Columns 5F      0  *System.Windows.Controls.DataGridTextColumn  5G      ,  &System.Windows.Controls.DataGridColumn   Header$ TaskName��6-      Width5  /System.Windows.Controls.DataGridLengthConverter$	 120 6\      SortMemberPath$ TaskName��6h   1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 6?   �� *
TaskName+6h     5H      $ UnitName��6-   $ * 6\   $ UnitName��6f    6?   �� *
UnitName+6f     5I      $ Duration��6-   $ * 6\   $ Duration��6f    6?   �� *
Duration+6f     5J      $ Description��6-   $ * 6b   $ Description��6l    6B   �� *Description+6l   4
  .System.Windows.Controls.DataGridTemplateColumn
  5K      $ Action��61   $ * 6A    
  CellTemplate 5L      �� 5M   "   .R�5N   &   �� $ 
Horizontal=�61   $
��Left=�6J   $��-5q�6e   .+�5O   *   ��  ��Name$ bt_TaskEdit��61   $��30��5P   N   $��30��6Y   $
��-5,0q�6e   $��0q�6t   $��Center=�6   $��Center=�6�   #��[ 6�   ��5O   F   �� *DataContext.EditTaskCommand+ �� RelativeSource ��  �� AncestorType Page��5P   1   �� .��5Q   .     $
 ClipboardEditOutline=�6F   $��20��5R   >   $��20��5S   >   
�������$��Center=�5U   >   $��Center=�5V   >    � ToolTip$
 Edit��5W   >   5X   +   �� 5Y   *   $ bt_TaskCopy��61   $��30��5Z   N   $��30��6Y   $
��-5,0q�6e   $��0q�6t   $��Center=�6   $��Center=�6�   #��[ 6�   ��5Y   F   �� *DataContext.CopyTaskCommand+ ��  Page��5Z   1   �� .��5[   .     $
 ClipboardMultipleOutline=�6F   $��20��5\   >   $��20��5]   >   
�������$��Center=�5_   >   $��Center=�5`   >   $
 Copy��5a   >   5b   +   d� 5c   *   $��5,0,0,0q�67   $��20��6H   $��20��6S   
������  $  MaterialDesignActionToggleButton#��[ 6x    d� 	IsChecked 5d   7   �� *
DeleteFlag+��5e   7   �� *%#DataContext.ToggleTaskDeleteCommand+ ��  Page��5f   7   �� 1  +MaterialDesignThemes.Wpf.ToggleButtonAssist   	OnContent 5g   .     5h   2   $
 ClipboardRemoveOutline=�6J   $��20��6h   $��20��6s     ComplementaryBrush#��[ 6   $ Delete��6�   ��5j   .     5k   2   $
 ClipboardPulseOutline=�6J   $��20��6g   $��20��6r     PrimaryBrush#��[ 6~   $ Alive��6�   5m   +   5n   '   5o   #   5q      5s      5t      5u      5v      5w      
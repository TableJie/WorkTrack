﻿#pragma checksum "..\..\..\InputTask.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "A92CC218CA7A8A1A84E3A85C7CD63CA02DCEA61C"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WorkTrack;


namespace WorkTrack {
    
    
    /// <summary>
    /// InputTask
    /// </summary>
    public partial class InputTask : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel MainForm;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker ip_TaskDate;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ip_TaskID;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ip_TaskName;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ip_Describe;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ip_DurationLevel;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ip_Duration;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ip_UnitName;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ip_ApplicationID;
        
        #line default
        #line hidden
        
        
        #line 137 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button bt_Refresh;
        
        #line default
        #line hidden
        
        
        #line 147 "..\..\..\InputTask.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button bt_Close;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.7.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WorkTrack;component/inputtask.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\InputTask.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.7.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MainForm = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 2:
            this.ip_TaskDate = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 3:
            this.ip_TaskID = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.ip_TaskName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.ip_Describe = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.ip_DurationLevel = ((System.Windows.Controls.ComboBox)(target));
            
            #line 69 "..\..\..\InputTask.xaml"
            this.ip_DurationLevel.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ip_DurationLevel_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.ip_Duration = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.ip_UnitName = ((System.Windows.Controls.ComboBox)(target));
            
            #line 109 "..\..\..\InputTask.xaml"
            this.ip_UnitName.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ip_DurationLevel_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.ip_ApplicationID = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 10:
            this.bt_Refresh = ((System.Windows.Controls.Button)(target));
            
            #line 145 "..\..\..\InputTask.xaml"
            this.bt_Refresh.Click += new System.Windows.RoutedEventHandler(this.RefreshButton_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.bt_Close = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


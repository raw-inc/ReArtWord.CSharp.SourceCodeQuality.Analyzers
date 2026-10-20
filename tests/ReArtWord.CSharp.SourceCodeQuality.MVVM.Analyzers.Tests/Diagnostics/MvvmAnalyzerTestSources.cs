namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// MVVM Analyzerテスト用ソースを組み立てます。
/// </summary>
internal static class MvvmAnalyzerTestSources
{
    /// <summary>
    /// MVVM Toolkit最小スタブ付きソースを作成します。
    /// </summary>
    /// <param name="targetSource">テスト対象ソースです。</param>
    /// <returns>スタブ付きソースです。</returns>
    public static string WithToolkitStubs(string targetSource)
    {
        return $$"""
            using System;
            using System.Collections.Generic;
            using System.Collections.ObjectModel;
            using System.ComponentModel;
            using System.Threading.Tasks;
            using System.Windows.Input;
            using CommunityToolkit.Mvvm.ComponentModel;
            using CommunityToolkit.Mvvm.Input;

            namespace CommunityToolkit.Mvvm.ComponentModel
            {
                [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
                public sealed class ObservablePropertyAttribute : Attribute
                {
                }

                [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
                public sealed class NotifyPropertyChangedForAttribute : Attribute
                {
                    public NotifyPropertyChangedForAttribute(string propertyName)
                    {
                    }

                    public NotifyPropertyChangedForAttribute(params string[] propertyNames)
                    {
                    }
                }

                public class ObservableObject
                {
                    public event PropertyChangedEventHandler? PropertyChanged;

                    public event PropertyChangingEventHandler? PropertyChanging;

                    protected bool SetProperty<T>(ref T field, T value)
                    {
                        field = value;
                        return true;
                    }

                    protected void OnPropertyChanged(string? propertyName = null)
                    {
                    }

                    protected void OnPropertyChanging(string? propertyName = null)
                    {
                    }
                }

                public class ObservableRecipient : ObservableObject
                {
                }

                public class ObservableValidator : ObservableObject
                {
                }
            }

            namespace CommunityToolkit.Mvvm.Input
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                public sealed class RelayCommandAttribute : Attribute
                {
                    public string? CanExecute { get; set; }

                    public bool AllowConcurrentExecutions { get; set; }
                }

                public interface IRelayCommand : ICommand
                {
                }

                public interface IAsyncRelayCommand : IRelayCommand
                {
                }

                public class RelayCommand : IRelayCommand
                {
                    public RelayCommand(Action execute)
                    {
                    }

                    public event EventHandler? CanExecuteChanged;

                    public bool CanExecute(object? parameter)
                    {
                        return true;
                    }

                    public void Execute(object? parameter)
                    {
                    }
                }

                public class AsyncRelayCommand : IAsyncRelayCommand
                {
                    public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute)
                    {
                    }

                    public event EventHandler? CanExecuteChanged;

                    public bool CanExecute(object? parameter)
                    {
                        return true;
                    }

                    public void Execute(object? parameter)
                    {
                    }
                }
            }

            namespace System.Windows
            {
                public class Application
                {
                    public static Application? Current { get; set; }
                }

                public class Window
                {
                }

                public static class MessageBox
                {
                    public static void Show(string message)
                    {
                    }
                }
            }

            namespace System.Windows.Controls
            {
                public class Page
                {
                }

                public class UserControl
                {
                }

                public class Control
                {
                }
            }

            namespace Microsoft.Maui.Controls
            {
                public class Application
                {
                    public static Application? Current { get; set; }
                }

                public class Page
                {
                }

                public class ContentPage : Page
                {
                }

                public class View
                {
                }
            }

            namespace Avalonia
            {
                public class Application
                {
                    public static Application? Current { get; set; }
                }
            }

            namespace Avalonia.Controls
            {
                public class Window
                {
                }

                public class UserControl
                {
                }

                public class Page
                {
                }
            }

            namespace ReArtWord.App
            {
                {{targetSource}}
            }
            """;
    }
}

using System;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusGear.ViewModels;

/// <summary>Base for screens that show a result message and report to the status bar.</summary>
public abstract partial class PageViewModel : ViewModelBase
{
    private readonly Action<string> _status;

    protected PageViewModel(Action<string> status)
    {
        _status = status;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    private string _message = "";

    [ObservableProperty]
    private bool _isError;

    public bool HasMessage => Message.Length > 0;

    protected void Report(OperationResult result)
    {
        Message = result.Message;
        IsError = !result.Success;
        _status(result.Message);
    }
}

﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.Messages;

namespace MediaFinder.Views.Status;

public partial class ProcessCompletedViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public ProcessCompletedViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [RelayCommand]
    public void OnNavigateBack()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Previous));
    }

    [RelayCommand]
    public void OnFinished()
    {
        _messenger.Send(FinishedMessage.Create());
    }
}
